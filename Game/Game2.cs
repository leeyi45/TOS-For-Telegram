using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Telegram.Bot.Types;

namespace QuizBot
{
  //This file contains all the details with regards to starting a game
  public partial class Game
  {
    public Game(string rolelist, Message msg)
    {
      Stopwatch = new System.Diagnostics.Stopwatch();
      Messages = new Dictionary<int, Tuple<int, int>>();
      Joined = new List<Player>();
      settings = new Settings(QuizBot.Settings.AllSettings);
      Parsers = new Dictionary<string, Action<Callback>>();

      Parsers.Add(Protocols["NightActions"], new Action<Callback>(ParseNightAction));
      Parsers.Add(Protocols["Vote"], new Action<Callback>(ParseVoteChoice));

      Rolelist = GameData.RoleLists[rolelist];
      Roles = GameData.Roles;
      CurrentGroup = msg.Chat.Id;
      GameMessages = GameData.Messages;
      Protocols = GameData.Protocols;

      Program.ConsoleLog("New game instance in group " + msg.Chat.Title);
      BotMessage("LobbyCreated", msg.From.Username);
    }

    public bool HasJoined(Player player)
    {
      return Joined.Contains(player);
    }

    #region Commands
    public void Join(Message msg)
    {
      var player = msg.From;
      if (HasJoined(player) && GamePhase == GamePhase.Joining)
      { //Joined already
        BotMessage("AlreadyJoin");
      }
      else if (GamePhase == GamePhase.Running || GamePhase == GamePhase.Assigning)
      { //Game is running
        BotMessage("GameRunningJoin");
      }
      else if (GamePhase == GamePhase.Joining)
      { //Join the player thanks
        if (PlayerCount == settings.MaxPlayers)
        {
          BotMessage("MaxPlayersReached");
          return;
        }
        BotMessage(msg.Chat.Id, "PlayerJoin", player.Username, PlayerCount,
          settings.MinPlayers, settings.MaxPlayers);
        Program.BotMessage(player.Id, "JoinGameSuccess", msg.Chat.Title);
        Joined.Add(player);

        if (PlayerCount >= settings.MinPlayers)
        {
          BotMessage("MinPlayersReached");
        }
      }
    }

    public void CloseLobby(Message msg)
    {
      if ((int)GameData.GamePhase > 1) BotMessage("GameBegun");
      else
      {
        GamePhase = GamePhase.Inactive;
        Joined.Clear();
        BotMessage("LobbyClosed", msg.From.Username);
      }
    }

    public void Leave(Message msg)
    {
      if (GamePhase == GamePhase.Joining ||
       GamePhase == GamePhase.Assigning)
      {
        if (GetPlayer(msg.From.Id) != null)
        {
          BotMessage(msg.Chat.Id, "LeftGame", msg.From.Username);
          Joined.Remove(Joined.Where(x => x == msg.From).ToArray()[0]);
        }
        else BotMessage(msg.Chat.Id, "NotInGame");
      }

      else if (GamePhase == GamePhase.Running)
      {
        if (GetPlayer(msg.From.Id) != null)
        {
          BotMessage(msg.Chat.Id, "LeftGame", msg.From.Username);
          Alive.Where(x => x == msg.From).ToArray()[0].Kill();
        }
        else BotMessage(msg.Chat.Id, "NotInGame");
      }
    }

    public void StartGame()
    {
      if (PlayerCount < settings.MinPlayers)
      {
        BotMessage("NotEnoughPlayers", PlayerCount, settings.MinPlayers);
        return;
      }
      if ((int)GamePhase > 1)
      {
        BotMessage("GameBegun");
        return;
      }
      GamePhase = GamePhase.Assigning;
      BotMessage("BeginGame");
      GameStart = new Thread(new ThreadStart(StartRolesAssign));

      if (settings.UseNicknames) ObtainNicknames();
      else GameStart.Start();
    }

    public void Say(Message msg, string[] args)
    {
      string[] otherargs = new string[args.Length - 1];
      Array.Copy(args, 1, otherargs, 0, args.Length - 1);
      BotNormalMessage(CurrentGroup, string.Join(" ", otherargs));
    }

    public void Players()
    {
      var output = new StringBuilder("*Players: *" + AliveCount + "/" + PlayerCount + "\n\n");
      foreach (var each in Joined)
      {
        output.Append(each.Username);
        if (!each.IsAlive) output.Append(": " + each.role.ToString());
        output.AppendLine();
      }
      BotNormalMessage(CurrentGroup, output.ToString());
    }
    #endregion

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
  }
}
