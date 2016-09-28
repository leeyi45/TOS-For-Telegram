using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Telegram.Bot.Types;
using System.Reflection;
using System.ComponentModel;

namespace QuizBot
{
  //This file contains all the details with regards to starting a game and commands
  public partial class Game
  {
    public Game(Message msg)
    {
      CurrentGroup = msg.Chat.Id;
      GroupName = msg.Chat.Title;
      settings = new Settings(QuizBot.Settings.AllSettings, this);
      PrivateID = Commands.GameInstances.GenerateNewPrivateId();
      InitializeGame();
      SetDetails();

      Parsers.Add(Protocols["NightActions"], new Action<Callback>(ParseNightAction));
      Parsers.Add(Protocols["Vote"], new Action<Callback>(ParseVoteChoice));

      Program.ConsoleLog("New game instance in group " + msg.Chat.Title);
      BotMessage("InstanceCreated");
    }

    private Game(string groupName, int group, int privateId, System.Xml.Linq.XElement settings)
    {
      CurrentGroup = group;
      GroupName = groupName;
      InitializeGame();
      this.settings = new Settings(settings, this);
      SetDetails();
      PrivateID = privateId;

      Parsers.Add(Protocols["NightActions"], new Action<Callback>(ParseNightAction));
      Parsers.Add(Protocols["Vote"], new Action<Callback>(ParseVoteChoice));
    }

    private Game(Settings settings)
    {
      InitializeGame();
      this.settings = settings;
      SetDetails();

      Parsers.Add(Protocols["NightActions"], new Action<Callback>(ParseNightAction));
      Parsers.Add(Protocols["Vote"], new Action<Callback>(ParseVoteChoice));
    }

    private void InitializeGame()
    {
      Stopwatch = new System.Diagnostics.Stopwatch();
      Messages = new Dictionary<int, Tuple<long, int>>();
      Joined = new List<Player>();
      Parsers = new Dictionary<string, Action<Callback>>();
      CommandContainer = new GameCommands(this);
      GameStart = new GameThread(StartRolesAssign);
      GameStart.Finished += OnGameFinish;
    }

    private void SetDetails()
    {
      Rolelist = GameData.RoleLists[settings.CurrentRoleList];
      Roles = GameData.Roles;
      GameMessages = GameData.Messages;
      Protocols = GameData.Protocols;
      Alignments = GameData.Alignments;
    }

    public bool HasJoined(Player player)
    {
      try { return Joined.Where(x => x.Id == player.Id).ToArray().Length == 1; }
      catch(IndexOutOfRangeException) { return false; }
    }

    public bool HasJoined(int Id)
    {
      try { return Joined.Where(x => x.Id == Id).ToArray().Length == 1; }
      catch (IndexOutOfRangeException) { return false; }
    }

    private void StartRolesAssign()
    {
      var noroles = Joined;
      var hasroles = new List<Player>();
      var random = new Random();
      int totaltoassign = 0;
      GamePhase = GamePhase.Assigning;
      foreach (var each in Rolelist.Values) { totaltoassign += each; ; }

      int[] randoms = Program.GenerateInts(Math.Min(noroles.Count, totaltoassign), 0, noroles.Count);

      int i = 0;
      #region Assign the roles
      foreach (var role in Rolelist)
      {
        Role assignThis = null;
        for (; i < role.Value; i++)
        {
          var player = Joined[randoms[i]];
          try
          {
            if (role.Key is Role) assignThis = role.Key as Role;
            else
            {
              while (true)
              {
                var assignThese = new Role[] { };
                if (role.Key is Alignment)
                {
                  if ((role.Key as Alignment).Name == "Any") assignThese = Roles.Values.ToArray();
                  else assignThese = Roles.Values.Where(x => x.Alignment == (role.Key as Alignment))
                      .ToArray();
                }
                else if (role.Key is TeamWrapper)
                {
                  assignThese = Roles.Values.Where(x => x.team == (role.Key as TeamWrapper).team)
                    .ToArray();
                }
                assignThis = assignThese[random.Next(0, assignThese.Length)];
                if (!(assignThis.Unique && hasroles.Count(x => x.role == assignThis) >= 1)) break;
                //If the role is unique and there's already such an assignment try again
                //If not break
              }
            }
          }
          catch (IndexOutOfRangeException)
          {
            throw new AssignException("There is no role defined for " + role.Key.Name);
          }
          player.role = assignThis;
          hasroles.Add(player);
          noroles.Remove(player);
        }

        //If there are no more players to assign
        if (noroles.Count == 0) break;
      }
      #endregion

      //Assign the rest of the people to be villagers
      if (noroles.Count > 0)
      {
        foreach (var each in noroles)
        {
          each.role = Roles["villager"];
          hasroles.Add(each);
        }
      }

      Joined = hasroles;
      hasroles.ForEach(x => x.OnAssignRole());
      BotMessage("RolesAssigned");
      RunGame();
    }

    private void ObtainNicknames()
    {
      Joined.ForEach(x =>
      {
        BotMessage(x.Id, "GetNickname");
        x.GettingNickname = true;
      });
    }

    public void ProcessNicknames(Message msg)
    {
      var player = GetPlayer(msg.From.Id);
      if (player.Nickname != null)
      {
        BotMessage(msg.From.Id, "ChangedNickname", msg.Text);
      }
      else
      {
        BotMessage(msg.From.Id, "GotNickname", msg.Text);
      }
      player.Nickname = msg.Text;
      var count = Joined.Count(x => string.IsNullOrWhiteSpace(x.Nickname));
      if (count == 0)
      {
        GameStart.Start();
        Joined.ForEach(x => x.GettingNickname = false);
      }
      else BotMessage("NicknamesLeft", count);
    }

    private GameCommands CommandContainer;

    public delegate void CommandDelegate(Message msg, string[] args);

    public Dictionary<string, Command> AllCommands
    {
      get { return CommandContainer.AllCommands; }
    }

    private void OnGameFinish(object sender, GameThread.GameFinishEventArgs e)
    {
      if (e.Error != null)
      {
        //throw e.Error;
        Program.LogError(e.Error);
        BotMessage("Error");
      }

      if(RefreshQueued)
      {
        RefreshGame();
        RefreshQueued = false;
      }
    }

    private void RefreshGame()
    {
      SetDetails();
      GroupName = Program.Bot.GetChatAsync(CurrentGroup).Result.Title;
      BotMessage("Refreshed");
    }

    private class GameCommands
    {
      public GameCommands(Game parent)
      {
        AllCommands = new Dictionary<string, Command>();
        foreach (var each in typeof(GameCommands).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
        {
          var attri = each.GetCustomAttribute<Command>();
          if (attri != null)
          {
            attri.Info = (CommandDelegate)Delegate.CreateDelegate(typeof(CommandDelegate), this, each);
            attri.IsNotInstance = false;
            attri.InGroupOnly = true;
            AllCommands.Add(attri.Trigger, attri);
          }
        }
        this.parent = parent;
      }

      public readonly Dictionary<string, Command> AllCommands;

      private readonly Game parent;

      [Command(Trigger = "join", InGroupOnly = true, GameStartOnly = true)]
      private void Join(Message msg, string[] args)
      {
        var player = (Player)msg.From;
        if(CommandVars.PlayersInGame.Contains(player.Id))
        {
          if(CommandVars.PlayersInGame.Where(x => x == player).ToArray()[0].GroupCode != parent.CurrentGroup)
          {
            parent.BotMessage("AlreadyInGame");
            return;
          }
          else if (parent.GamePhase == GamePhase.Joining)
          {
            parent.BotMessage("AlreadyJoin");
          }
        }
        else if (parent.GameStarted) parent.BotMessage("GameRunningJoin");
        else if (parent.GamePhase == GamePhase.Joining)
        { //Join the player thanks

          //Check if the bot PM is enabled
          try { parent.BotMessage(player.Id, "JoinGameSuccess", msg.Chat.Title, false); }
          catch (Exception)
          {
            parent.BotMessage("PleaseStartBot", player.Username);
            return;
          }
          if (parent.PlayerCount == parent.settings.MaxPlayers)
          {
            parent.BotMessage("MaxPlayersReached");
            return;
          }

          player.GroupCode = parent.CurrentGroup;
          parent.BotMessage("PlayerJoin", player.Username, parent.PlayerCount + 1,
            parent.settings.MinPlayers, parent.settings.MaxPlayers);

          parent.Joined.Add(player);
          CommandVars.PlayersInGame.Add(player);

          if (parent.PlayerCount >= parent.settings.MinPlayers)
          {
            parent.BotMessage("MinPlayersReached", parent.Joined.Count);
          }
        }
      }

      [Command(Trigger = "closelobby", InGroupOnly = true, GameStartOnly = true)]
      private void CloseLobby(Message msg, string[] args)
      {
        if (parent.GameStarted) parent.BotMessage("GameBegun");
        else if (parent.GamePhase == GamePhase.Joining)
        {
          parent.GamePhase = GamePhase.Inactive;
          parent.Joined.Clear();
          parent.BotMessage("LobbyClosed", msg.From.Username);
          parent.LobbyCreated = false;
        }
      }

      [Command(Trigger = "createlobby", InGroupOnly = true)]
      private void CreateLobby(Message msg, string[] args)
      {
        if (parent.GamePhase == GamePhase.Inactive)
        {
          var player = (Player)msg.From;
          //Check if the bot PM is enabled
          try { parent.BotMessage(player.Id, "JoinGameSuccess", msg.Chat.Title, false); }
          catch (Exception)
          {
            parent.BotMessage("PleaseStartBot", player.Username);
            return;
          }
          player.GroupCode = parent.CurrentGroup;
          parent.Rolelist = GameData.RoleLists[parent.settings.CurrentRoleList];
          parent.Roles = GameData.Roles;
          parent.CurrentGroup = msg.Chat.Id;
          parent.GameMessages = GameData.Messages;
          parent.Protocols = GameData.Protocols;
          parent.Alignments = GameData.Alignments;
          parent.GamePhase = GamePhase.Joining;
          parent.LobbyCreated = true;
          parent.BotMessage("LobbyCreated", player.Username);
          CommandVars.PlayersInGame.Add(player);
          parent.Joined.Add(player);
        }
        else if (parent.LobbyCreated) parent.BotMessage("LobbyExists");
        else parent.BotMessage("GameBegun");
      }

      [Command(Trigger = "leave", InGroupOnly = true, GameStartOnly = true)]
      private void Leave(Message msg, string[] args)
      {
        if (parent.GamePhase == GamePhase.Joining ||
         parent.GamePhase == GamePhase.Assigning)
        {
          if (parent.HasJoined(msg.From.Id))
          {
            parent.BotMessage(msg.Chat.Id, "LeftGame", msg.From.Username);
            parent.Joined.Remove(parent.Joined.Where(x => x == msg.From).ToArray()[0]);
          }
          else parent.BotMessage(msg.Chat.Id, "NotInGame");
        }

        else if (parent.GamePhase == GamePhase.Running)
        {
          if (parent.GetPlayer(msg.From.Id) != null)
          {
            parent.BotMessage(msg.Chat.Id, "LeftGame", msg.From.Username);
            parent.Alive.Where(x => x == msg.From).ToArray()[0].Kill();
          }
          else parent.BotMessage(msg.Chat.Id, "NotInGame");
        }
      }

      [Command(Trigger = "startgame", InGroupOnly = true, GameStartOnly = true)]
      private void StartGame(Message msg, string[] args)
      {
        if (parent.PlayerCount < parent.settings.MinPlayers)
        {
          parent.BotMessage("NotEnoughPlayers", parent.settings.MinPlayers, parent.PlayerCount);
          return;
        }
        if ((int)parent.GamePhase > 1)
        {
          parent.BotMessage("GameBegun");
          return;
        }
        parent.GamePhase = GamePhase.Assigning;
        parent.BotMessage("BeginGame");

        if (parent.settings.UseNicknames) parent.ObtainNicknames();
        else parent.GameStart.Start();
      }

      [Command(Trigger = "stopgame", InGroupOnly = true, GameStartOnly = true)]
      private void StopGame(Message msg, string[] args)
      {
        parent.QuitGame = true;
      }

      [Command(Trigger = "say", InPrivateOnly = true, GameStartOnly = true)]
      private void Say(Message msg, string[] args)
      {
        string[] otherargs = new string[args.Length - 1];
        Array.Copy(args, 1, otherargs, 0, args.Length - 1);
        parent.BotNormalMessage(parent.CurrentGroup, string.Join(" ", otherargs));
      }

      [Command(Trigger = "players", GameStartOnly = true)]
      private void Players(Message msg, string[] args)
      {
        var output = new StringBuilder("<strong>Players:</strong> " + parent.AliveCount + "/" + parent.PlayerCount + "\n\n");
        foreach (var each in parent.Joined)
        {
          output.Append(each.Username + "\uD83D\uDE03");
          if (!each.IsAlive) output.Append(": " + each.role.ToString());
          output.AppendLine();
        }
        parent.BotNormalMessage(parent.CurrentGroup, output.ToString());
      }

      [Command(Trigger = "config", GroupAdminOnly = true)]
      private void Config(Message msg, string[] args)
      {
        //Temporary replacement for inline system
        StringBuilder output;
        switch(args.Length)
        {
          case 1:
            {
              output = new StringBuilder("<b>Config Options:</b>\n");
              foreach(var each in parent.settings.AllSettings)
              {
                output.AppendLine(each.DisplayName + ": " + each.GetValue(parent.settings));
              }
              break;
            }
          case 2:
            {
              SettingDetail prop;
              try
              {
                prop = parent.settings.SetPropertyValue[args[1]];
                if(!string.IsNullOrWhiteSpace(prop.ExtraMessage))
                {
                  parent.BotNormalMessage(prop.ExtraMessage);
                }
              }
              catch (KeyNotFoundException)
              {
                output = new StringBuilder("Unrecognized Config Option: " + args[1]);
                break;
              }
              output = new StringBuilder("Config Option: " + prop.GetValue(parent.settings).ToString());
              break;
            }
          case 3:
            {
              if((int)parent.GamePhase > 1)
              {
                parent.BotMessage("RunningGameConfig", args[1]);
                return;
              }
              SettingDetail prop;
              try
              {
                prop = parent.settings.SetPropertyValue[args[1]];
                if (!string.IsNullOrWhiteSpace(prop.ExtraMessage))
                {
                  parent.BotNormalMessage(prop.ExtraMessage);
                }
              }
              catch (KeyNotFoundException)
              {
                output = new StringBuilder("Unrecognized Config Option: " + args[1]);
                break;
              }
              try { prop.SetValue(parent.settings, Convert.ChangeType(args[2], prop.Info.PropertyType)); }
              catch (FormatException)
              {
                output = new StringBuilder(args[2] + " is an invalid value for " + prop.DisplayName);
                break;
              }
              catch (ConfigException e)
              {
                output = new StringBuilder(e.Message);
                break;
              }
              output = new StringBuilder("Set the value of " + prop.DisplayName + " to " + args[2]);
              break;
            }
          default:
            {
              output = new StringBuilder("Only 1-2 arguments are expected!");
              break;
            }
        }
        parent.BotNormalMessage(output.ToString());
      }

      [Command(Trigger = "roles")]
      private void Roles(Message msg, string[] args)
      {
        StringBuilder output;
        switch (args.Length)
        {
          case 1:
            {
              output = new StringBuilder("<b>Currently Registered Roles</b>\n\n");
              foreach (var each in parent.Roles)
              {
                output.AppendLine(each.Value.ToString());
              }
              break;
            }
          case 2:
            {
              try
              {
                var role = parent.Roles[args[1].ToLower()];
                output = new StringBuilder("<b>Role Data:</b>\n\n");
                foreach (var field in typeof(Role).GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                  output.AppendLine(field.Name + ": " + field.GetValue(role).ToString());
                }
              }
              catch (KeyNotFoundException)
              {
                output = new StringBuilder("No such role \"" + args[1] + "\" found!");
              }
              break;
            }
          default:
            {
              output = new StringBuilder("Only one or two arguments are accepted");
              break;
            }
        }

        parent.BotNormalMessage(output.ToString());
      }

      [Command(Trigger = "rolelist")]
      private void RoleList(Message msg, string[] args)
      {
        var output = new StringBuilder("<b>" + parent.settings.CurrentRoleList + "</b>\n");
        foreach(var each in parent.Rolelist)
        {
          output.AppendLine(each.Key.ToString() + ", Count: " + each.Value);
        }
        parent.BotNormalMessage(output.ToString());
      }

      [Command(Trigger = "refresh")]
      private void Refresh(Message msg, string[] args)
      {
        if (parent.GameStarted)
        {
          if (parent.RefreshQueued)
          {
            parent.BotMessage("RefreshAlreadyQueued");
          }
          else
          {
            parent.RefreshQueued = true;
            parent.BotMessage("RefreshQueued");
          }
        }
        else parent.RefreshGame();
      }
    }

    private class GameThread
    {
      public GameThread(ThreadStart work)
      {
        DoWork = work;
        thread = new Thread(Work);
      }

      private ThreadStart DoWork;

      private Thread thread;

      public void OnStart(object sender, EventArgs e)
      {
        Started?.Invoke(sender, e);
      }

      public void OnFinish(object sender, GameFinishEventArgs e)
      {
        Finished?.Invoke(sender, e);
      }

      private void Work()
      {
        GameFinishEventArgs args;
        OnStart(this, EventArgs.Empty);
        try
        {
          DoWork();
          args = new GameFinishEventArgs();
        }
        catch(Exception e) { args = new GameFinishEventArgs(e); }
        OnFinish(this, args);
      }

      public void Start() { thread.Start(); }

      public event EventHandler Started;

      public event GameFinishEventHandler Finished;

      public delegate void GameFinishEventHandler(object sender, GameFinishEventArgs e);

      public class GameFinishEventArgs : EventArgs
      {
        public GameFinishEventArgs(Exception e) { Error = e; }

        public GameFinishEventArgs() { }

        public Exception Error;
      }
    }
  }
}
