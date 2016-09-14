﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Telegram.Bot.Types;
using System.Reflection;

namespace QuizBot
{
  //This file contains all the details with regards to starting a game
  public partial class Game
  {
    public Game(Message msg)
    {
      Rolelist = GameData.RoleLists[QuizBot.Settings.CurrentRoleList];
      Roles = GameData.Roles;
      CurrentGroup = msg.Chat.Id;
      GameMessages = GameData.Messages;
      Protocols = GameData.Protocols;
      Alignments = GameData.Alignments;

      Stopwatch = new System.Diagnostics.Stopwatch();
      Messages = new Dictionary<int, Tuple<int, int>>();
      Joined = new List<Player>();
      settings = new Settings(QuizBot.Settings.AllSettings);
      Parsers = new Dictionary<string, Action<Callback>>();
      CommandContainer = new GameCommands(this);

      Parsers.Add(Protocols["NightActions"], new Action<Callback>(ParseNightAction));
      Parsers.Add(Protocols["Vote"], new Action<Callback>(ParseVoteChoice));

      Program.ConsoleLog("New game instance in group " + msg.Chat.Title);
      BotMessage("InstanceCreated");
    }

    private Game(string groupName, int group, Settings settings)
    {
      Stopwatch = new System.Diagnostics.Stopwatch();
      Messages = new Dictionary<int, Tuple<int, int>>();
      Joined = new List<Player>();
      this.settings = settings;
      Parsers = new Dictionary<string, Action<Callback>>();
      CurrentGroup = group;
      GroupName = groupName;
      CommandContainer = new GameCommands(this);

      Parsers.Add(Protocols["NightActions"], new Action<Callback>(ParseNightAction));
      Parsers.Add(Protocols["Vote"], new Action<Callback>(ParseVoteChoice));
    }

    private Game(Settings settings)
    {
      Stopwatch = new System.Diagnostics.Stopwatch();
      Messages = new Dictionary<int, Tuple<int, int>>();
      Joined = new List<Player>();
      this.settings = settings;
      Parsers = new Dictionary<string, Action<Callback>>();
      CommandContainer = new GameCommands(this);

      Parsers.Add(Protocols["NightActions"], new Action<Callback>(ParseNightAction));
      Parsers.Add(Protocols["Vote"], new Action<Callback>(ParseVoteChoice));
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

    public void Refresh(Message msg)
    {
      if(GameStarted)
      {
        BotMessage("GameRunningRefresh");
        return;
      }
      Rolelist = GameData.RoleLists[QuizBot.Settings.CurrentRoleList];
      Roles = GameData.Roles;
      CurrentGroup = msg.Chat.Id;
      GameMessages = GameData.Messages;
      Protocols = GameData.Protocols;
      Alignments = GameData.Alignments;
      GamePhase = GamePhase.Joining;

      Stopwatch = new System.Diagnostics.Stopwatch();
      Messages = new Dictionary<int, Tuple<int, int>>();
      Joined = new List<Player>();
      settings = new Settings(QuizBot.Settings.AllSettings);
      Parsers = new Dictionary<string, Action<Callback>>();

      Parsers.Add(Protocols["NightActions"], new Action<Callback>(ParseNightAction));
      Parsers.Add(Protocols["Vote"], new Action<Callback>(ParseVoteChoice));
    }

    private void StartRolesAssign()
    {
      var noroles = Joined;
      var hasroles = new List<Player>();
      var random = new Random();
      int totaltoassign = 0;
      GameData.GamePhase = GamePhase.Assigning;
      foreach (var each in Rolelist) { totaltoassign += each.Value; }

      int[] randoms = random.Next(totaltoassign, 0, noroles.Count);

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
      foreach (var each in hasroles)
      {
        each.OnAssignRole();
      }
      GamePhase = GamePhase.Running;
      BotMessage("RolesAssigned");
      RunGame();
    }

    private void ObtainNicknames()
    {
      foreach (var each in Joined)
      {
        BotMessage(each.Id, "GetNickname");
      }
      CommandVars.GettingNicknames = true;
    }

    private GameCommands CommandContainer;

    public delegate void CommandDelegate(Message msg, string[] args);

    public Dictionary<string, Command> AllCommands
    {
      get { return CommandContainer.AllCommands; }
    }

    private class GameCommands
    {
      public GameCommands(Game parent)
      {
        AllCommands = new Dictionary<string, Command>();
        GetCommands();
        this.parent = parent;
      }

      public readonly Dictionary<string, Command> AllCommands;

      private readonly Game parent;

      private void GetCommands()
      {
        foreach(var each in typeof(GameCommands).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
        {
          var attri = each.GetCustomAttribute<Command>();
          if(attri != null)
          {
            attri.Info = (CommandDelegate)Delegate.CreateDelegate(typeof(CommandDelegate), this, each);
            attri.IsNotInstance = false;
            AllCommands.Add(attri.Trigger, attri);
          }
        }
      }

      [Command(Trigger = "join", InGroupOnly = true, GameStartOnly = true)]
      private void Join(Message msg, string[] args)
      {
        var player = msg.From;
        if (parent.HasJoined(player) && parent.GamePhase == GamePhase.Joining)
        { //Joined already
          parent.BotMessage("AlreadyJoin");
        }
        else if (parent.GamePhase == GamePhase.Running || parent.GamePhase == GamePhase.Assigning)
        { //Game is running
          parent.BotMessage("GameRunningJoin");
        }
        else if (parent.GamePhase == GamePhase.Joining)
        { //Join the player thanks
          if (parent.PlayerCount == parent.settings.MaxPlayers)
          {
            parent.BotMessage("MaxPlayersReached");
            return;
          }
          parent.BotMessage("PlayerJoin", player.Username, parent.PlayerCount + 1,
            parent.settings.MinPlayers, parent.settings.MaxPlayers);
          parent.BotMessage(player.Id, "JoinGameSuccess", msg.Chat.Title);
          parent.Joined.Add(player);

          if (parent.PlayerCount >= parent.settings.MinPlayers)
          {
            parent.BotMessage("MinPlayersReached");
          }
        }
      }

      [Command(Trigger = "closelobby", InGroupOnly = true, GameStartOnly = true)]
      private void CloseLobby(Message msg, string[] args)
      {
        if ((int)parent.GamePhase > 1) parent.BotMessage("GameBegun");
        else
        {
          parent.GamePhase = GamePhase.Inactive;
          parent.Joined.Clear();
          parent.BotMessage("LobbyClosed", msg.From.Username);
        }
      }

      [Command(Trigger = "createlobby", InGroupOnly = true)]
      private void CreateLobby(Message msg, string[] args)
      {
        if (parent.GameStarted) parent.BotMessage(msg.Chat.Id, "RunningGameStart");
        else
        {
          parent.Rolelist = GameData.RoleLists[parent.settings.CurrentRoleList];
          parent.Roles = GameData.Roles;
          parent.CurrentGroup = msg.Chat.Id;
          parent.GameMessages = GameData.Messages;
          parent.Protocols = GameData.Protocols;
          parent.Alignments = GameData.Alignments;
          parent.GamePhase = GamePhase.Joining;
          parent.BotMessage("LobbyCreated", msg.From.Username);
        }
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
          parent.BotMessage("NotEnoughPlayers", parent.PlayerCount, parent.settings.MinPlayers);
          return;
        }
        if ((int)parent.GamePhase > 1)
        {
          parent.BotMessage("GameBegun");
          return;
        }
        parent.GamePhase = GamePhase.Assigning;
        parent.BotMessage("BeginGame");
        parent.GameStart = new Thread(new ThreadStart(parent.StartRolesAssign));

        if (parent.settings.UseNicknames) parent.ObtainNicknames();
        else parent.GameStart.Start();
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
        var output = new StringBuilder("*Players: *" + parent.AliveCount + "/" + parent.PlayerCount + "\n\n");
        foreach (var each in parent.Joined)
        {
          output.Append(each.Username);
          if (!each.IsAlive) output.Append(": " + each.role.ToString());
          output.AppendLine();
        }
        parent.BotNormalMessage(parent.CurrentGroup, output.ToString());
      }
    }
  }
}
