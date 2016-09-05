using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace QuizBot
{
	//Okay this is my work
	public class Commands
	{
    #region Command Registration
    public static Dictionary<string, Tuple<CommandDelegate, Command>> AllCommands = 
      new Dictionary<string, Tuple<CommandDelegate, Command>>();

		public delegate void CommandDelegate(Message msg, string[] args);

    private static bool TestMode = true;

		public static void InitializeCommands()
		{
      //Just means I don't have to add the functions myself cause I'm lazy af
      foreach(var method in typeof(Commands).GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
      {
        Command attribute = method.GetCustomAttribute(typeof(Command)) as Command;
        if (attribute == null) continue;
        AllCommands.Add(attribute.Trigger, new Tuple<CommandDelegate, Command>(
          (CommandDelegate)Delegate.CreateDelegate(typeof(CommandDelegate), method), attribute));
        GameData.Log("Command \"" + attribute.Trigger + "\" registered", false);
      }
      GameData.Log("", false);
    }

		//The default parser
		public static void Parse(Message msg)
		{
			//remove the slash as necessary
			string cmd = msg.Text.ToLower().Substring(1, msg.Text.Length - 1);
			if (string.IsNullOrWhiteSpace(cmd)) return;

			string[] args = cmd.Split(' ');
			//Remove the @quiztestbot
			if (args[0].Contains("@quiztestbot")) args[0] = args[0].Substring(0, 
        args[0].Length - GameData.BotUsername.Length - 1);
      try
      {
        //Check stuff
        Command attribute = AllCommands[args[0]].Item2;

        #region Checks
        if (attribute.InGroupOnly &&
          (msg.Chat.Type == ChatType.Private || msg.Chat.Type == ChatType.Channel))
        { //If the chat is not a group and the command is group only
          Program.BotMessage(msg.Chat.Id, "GroupOnly");
          return;
        }

        if (attribute.InPrivateOnly && msg.Chat.Type != ChatType.Private)
        { //If the chat is not private and the command is private only
          Program.BotMessage(msg.Chat.Id, "PrivateOnly");
          return;
        }

        if (attribute.GroupAdminOnly && !UpdateHelper.IsGroupAdmin(msg.From.Id, msg.Chat.Id))
        { //If command is admin only and user is not admin
          Program.BotMessage(msg.Chat.Id, "NotAdminError", msg.From.Username);
          return;
        }

        if(attribute.DevOnly && msg.From.Id != Chats.chats["dev"])
        { //User is not dev
          Program.BotMessage(msg.Chat.Id, "NotDevError", msg.From.Username);
          return;
        }

        if(attribute.GameStartOnly && !GameData.GameStarted)
        { //Game has not been started
          Program.BotMessage(msg.Chat.Id, "NoGameJoin");
          return;
        }
        #endregion

        AllCommands[args[0]].Item1(msg, args);
      }
      catch (KeyNotFoundException)
      { //Program.ConsoleLog(e.Message); }
      }
		}
    #endregion

    #region Commands
    [Command(Trigger = "config", GroupAdminOnly = true)]
    private static void Config(Message msg, string[] args)
    {
      //Send the config menu to the player
      Program.BotMessage(msg.Chat.Id, "SentConfig", msg.From.GetName());
      QuizBot.Config.SendInline(msg.From, msg.Chat);
    }

    [Command(InGroupOnly = true, Trigger = "join", GameStartOnly = true)]
    private static void Join(Message msg, string[] args)
    {
      var player = msg.From;
      if (UpdateHelper.HasJoined(player) && GameData.GamePhase == GamePhase.Joining)
      { //Joined already
        Program.BotMessage("AlreadyJoin");
      }
      else if (GameData.GamePhase == GamePhase.Running || GameData.GamePhase == GamePhase.Assigning)
      { //Game is running
        Program.BotMessage("GameRunningJoin");
      }
      else if (GameData.GamePhase == GamePhase.Joining)
      { //Join the player thanks
        if(GameData.PlayerCount == Settings.MaxPlayers)
        {
          Program.BotMessage(msg.Chat.Id, "MaxPlayersReached");
          return;
        }
        Program.BotMessage(msg.Chat.Id, "PlayerJoin", player.Username, GameData.PlayerCount,
          Settings.MinPlayers, Settings.MaxPlayers);
        Program.BotMessage(msg.From.Id, "JoinGameSuccess", msg.Chat.Title);
        GameData.Joined.Add(GameData.PlayerCount, player);
        if(GameData.PlayerCount >= Settings.MinPlayers)
        {
          Program.BotMessage(msg.Chat.Id, "MinPlayersReached");
        }
      }
    }

    [Command(Trigger = "begin", InGroupOnly = true, GameStartOnly = true)]
    private static void Begin(Message msg, string[] args)
    {
      if(GameData.PlayerCount < Settings.MinPlayers)
      {
        Program.BotMessage("NotEnoughPlayers", GameData.PlayerCount, Settings.MinPlayers);
        return;
      }
      GameData.GamePhase = GamePhase.Assigning;
      Program.BotMessage("BeginGame");
      //Run the game on another thread
      //Keep the main one open for commands
      GameStart = new Thread(new ThreadStart(StartRolesAssign));
      GameStart.Start();
    }

    [Command(Trigger = "players", GameStartOnly = true)]
    private static void Players(Message msg, string[] args)
    {
      var output = new StringBuilder("*Players: *" + GameData.AliveCount + "/" + GameData.PlayerCount + "\n\n");
      foreach (var each in GameData.Joined)
      {
        output.Append(each.Value.Username);
        if (!each.Value.IsAlive) output.Append(": " + each.Value.role.ToString());
        output.Append("\n");
      }
      Program.Bot.SendTextMessageAsync(msg.Chat.Id, output.ToString(), parseMode: ParseMode.Markdown);
    }

    [Command(Trigger = "say", InPrivateOnly = true, GameStartOnly = true)]
    private static void Say(Message msg, string[] args)
    {
      string[] otherargs = new string[args.Length - 1];
      Array.Copy(args, 1, otherargs, 0, args.Length - 1);
      Program.Bot.SendTextMessageAsync(GameData.CurrentGroup, string.Join(" ", otherargs));
      //catch { Program.BotMessage(msg.Chat.Id, "NoGameJoin"); }
    }

    [Command(Trigger = "leave", InGroupOnly = true, GameStartOnly = true)]
    private static void Leave(Message msg, string[] args)
    {
      if(GameData.Alive.Values.Contains(msg.From))
      {
        Program.BotMessage(msg.Chat.Id, "LeftGame", msg.From.Username);
        GameData.Alive.Values.Where(x => x == msg.From).ToArray()[0].Kill(null);
      }
      else Program.BotMessage(msg.Chat.Id, "NotInGame");
    }

    [Command(InGroupOnly = true, Trigger = "ping")]
    private static void Ping(Message msg, string[] args)
    {
      var ts = DateTime.UtcNow - msg.Date;
      var send = DateTime.UtcNow;
      var message = "*PingInfo*\n" + "Time to receive ping message: " + ts.ToString("fff'.'ff") + "ms";
      var result = Program.Bot.SendTextMessageAsync(msg.Chat.Id, message,
        parseMode: ParseMode.Markdown).Result;
      ts = DateTime.UtcNow - send;
      Program.Bot.EditMessageTextAsync(msg.Chat.Id, result.MessageId, message + "\nTime to send ping message: " +
        ts.ToString("fff'.'ff") + "ms", parseMode: ParseMode.Markdown);
    }

    [Command(Trigger = "startgame")]
    private static void StartGame(Message msg, string[] args)
    {
      if (GameData.GamePhase == GamePhase.Joining) Program.BotMessage("RunningGameStart");
      else
      {
        GameData.CurrentGroup = msg.Chat.Id;
        Program.ConsoleLog("Game started!");
        Program.BotMessage("GameStart", msg.From.Username);
        GameData.GamePhase = GamePhase.Joining;
        GameData.Joined.Add(GameData.PlayerCount, msg.From);
      }
    }

    [Command(Trigger = "roles")]
    private static void Roles(Message msg, string[] args)
    {
      StringBuilder output;
      switch(args.Length)
      {
        case 1:
          {
            output = new StringBuilder("*" + Settings.CurrentRoleList + "*" + "\n\n");
            foreach (var each in Settings.CurrentRoles)
            {
              output.AppendLine(each.Key.Name + ", Count: " + each.Value.ToString());
            }
            break;
          }
        case 2:
          {
            try
            {
              var role = GameData.Roles[args[1].ToLower()];
              output = new StringBuilder("*Role Data:*\n\n");
              foreach(var field in typeof(Role).GetProperties(BindingFlags.Instance | BindingFlags.Public))
              {
                output.AppendLine(field.Name + ": " + field.GetValue(role).ToString());
              }
            }
            catch(KeyNotFoundException)
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

      Program.Bot.SendTextMessageAsync(msg.Chat.Id, output.ToString(), parseMode: ParseMode.Markdown);
    }

    [Command(Trigger = "listroles")]
    private static void ListRoles(Message msg, string[] args)
    {
      StringBuilder output = new StringBuilder("<b>Currently Registered Roles:</b>\n\n");
      foreach (var each in GameData.Roles)
      {
        output.AppendLine(each.Value.Name + ": <i>" + each.Value.Description + "</i>");
      }
      Program.Bot.SendTextMessageAsync(msg.Chat.Id, output.ToString(), parseMode: ParseMode.Html);
    }

    [Command(Trigger = "version")]
    private static void Version(Message msg, string[] args)
    {
      Assembly main = Assembly.GetExecutingAssembly();
      var version = main.GetName().Version;
      StringBuilder output = new StringBuilder("*Version information*\n\n");
      output.AppendLine("Version: " + version.Major + "." + version.Minor);
      output.AppendLine("Build: " + version.Build);
      Program.Bot.SendTextMessageAsync(msg.Chat.Id, output.ToString(), parseMode: ParseMode.Markdown);
    }

    [Command(Trigger = "getchatid")]
    private static void GetChatId(Message msg, string[] args)
    {
      Program.Bot.SendTextMessageAsync(msg.Chat.Id, "The chat ID is: " + msg.Chat.Id.ToString());
    }
    
    [Command(Trigger = "getcommands")]
    private static void GetCommands(Message msg, string[] args)
    {
      var output = new StringBuilder("*Current Commands:*\n");
      try
      {
        foreach (var each in AllCommands)
        {
          output.AppendLine(each.Key);
        }
      }
      catch (NullReferenceException)
      {
        output = new StringBuilder("Commands have not been loaded!");
      }
      Program.Bot.SendTextMessageAsync(msg.Chat.Id, output.ToString(), parseMode: ParseMode.Markdown);
    }

    [Command(Trigger = "testassign", DevOnly = true)]
    private static void TestAssign(Message msg, string[] args)
    {
      var otherroles = new Dictionary<int, Player>();
      var doc = XDocument.Load(WerewolfFile);
      foreach(var each in doc.Root.Elements("member"))
      {
        string[] name = each.GetElementValue("name").Split(' ');
        otherroles.Add(otherroles.Count, 
          new Player(int.Parse(each.GetElementValue("Id")), each.GetElementValue("username"),
          name[0], name[name.Length-1]));
      }
      var noroles = otherroles;
      var hasroles = new Dictionary<int, Player>();
      var random = new Random();
      //new org.random.JSONRPC.RandomJSONRPC("bbcfa0f8-dbba-423a-8798-c8984c4fc5c5");
      int totaltoassign = 0;
      //GameData.GamePhase = GamePhase.Assigning;
      foreach (var each in Settings.CurrentRoles) { totaltoassign += each.Value; }

      int[] randoms = random.Next(totaltoassign, 0, noroles.Count);

      int i = 0;
      #region Assign the roles
      foreach (var role in Settings.CurrentRoles)
      {
        int count = i + role.Value;
        for (; i < count; i++)
        {
          Role assignThis = null;
          var player = otherroles[randoms[i]];
          try
          {
            if (role.Key is Role) assignThis = role.Key as Role;
            else
            {
              var assignThese = new Role[] { };
              if (role.Key is Alignment)
              {
                if ((role.Key as Alignment).Name == "Any") assignThese = GameData.Roles.Values.ToArray();
                else assignThese = GameData.Roles.Values.Where(x => x.Alignment == (role.Key as Alignment))
                    .ToArray();
              }
              else if (role.Key is TeamWrapper)
              {
                assignThese = GameData.Roles.Values.Where(x => x.team == (role.Key as TeamWrapper).team)
                  .ToArray();
              }
              assignThis = assignThese[random.Next(0, assignThese.Length)];
            }
          }
          catch(IndexOutOfRangeException)
          {
            throw new AssignException("There is no role defined for " + role.Key.Name);
          }
          player.role = assignThis;
          hasroles.Add(hasroles.Count, player);
          noroles.Remove(randoms[i]);
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
          each.Value.role = GameData.Roles["villager"];
          hasroles.Add(hasroles.Count, each.Value);
        }
      }

      otherroles = hasroles;
      StringBuilder output = new StringBuilder("*Role assignment results:*\n\n");
      foreach(var each in hasroles)
      {
        output.Append(each.Value.Name + ": " + each.Value.role.Name + ", Assigned as ");
        try { output.AppendLine(Settings.CurrentRoles.ToArray()[each.Key].Key.Name); }
        catch(IndexOutOfRangeException) { output.AppendLine("Villager"); }
      }
      Program.Bot.SendTextMessageAsync(msg.Chat.Id, output.ToString(), parseMode: ParseMode.Markdown);
    }

    [Command(Trigger = "testmode", DevOnly = true)]
    private static void EngageTestMode(Message msg, string[] args)
    { //Switch the game into a test mode
      TestMode = !TestMode;
    }

    [Command(Trigger = "getuserid", InPrivateOnly = true, DevOnly = true)]
    private static void GetUserId(Message msg, string[] args)
    {
      if (!Settings.GetUserId)
      {
        Settings.GetUserId = true;
        Program.Bot.SendTextMessageAsync(msg.Chat.Id, "Forward the messages please");
      }
      else
      {
        Settings.GetUserId = false;
        Program.Bot.SendTextMessageAsync(msg.Chat.Id, "Done registering IDs");
      }
    }
    #endregion

    #region Assign Roles
    public static void StartRolesAssign()
		{
			var noroles = GameData.Joined;
			var hasroles = new Dictionary<int, Player>();
      var random = new Random();
			int totaltoassign = 0;
      GameData.GamePhase = GamePhase.Assigning;
      foreach (var each in Settings.CurrentRoles) { totaltoassign += each.Value; }

      int[] randoms = random.Next(totaltoassign, 0, noroles.Count);

      int i = 0;
      #region Assign the roles
      foreach(var role in Settings.CurrentRoles)
      {
        Role assignThis = null;
        for (; i < role.Value; i++)
        {
          var player = GameData.Joined[randoms[i]];
          try
          {
            if (role.Key is Role) assignThis = role.Key as Role;
            else
            {
              var assignThese = new Role[] { };
              if (role.Key is Alignment)
              {
                if ((role.Key as Alignment).Name == "Any") assignThese = GameData.Roles.Values.ToArray();
                else assignThese = GameData.Roles.Values.Where(x => x.Alignment == (role.Key as Alignment))
                    .ToArray();
              }
              else if (role.Key is TeamWrapper)
              {
                assignThese = GameData.Roles.Values.Where(x => x.team == (role.Key as TeamWrapper).team)
                  .ToArray();
              }
              assignThis = assignThese[random.Next(0, assignThese.Length)];
            }
          }
          catch (IndexOutOfRangeException)
          {
            throw new AssignException("There is no role defined for " + role.Key.Name);
          }
          player.role = assignThis;
          hasroles.Add(hasroles.Count, player);
          noroles.Remove(randoms[i]);
        }

        //If there are no more players to assign
        if (noroles.Count == 0) break;
      }
      #endregion

      //Assign the rest of the people to be villagers
      if(noroles.Count > 0)
      {
        foreach(var each in noroles)
        {
          each.Value.role = GameData.Roles["villager"];
          hasroles.Add(hasroles.Count, each.Value);
        }
      }

      GameData.Joined = hasroles;
      foreach(var each in hasroles)
      {
        each.Value.OnAssignRole();
      }
      GameData.GamePhase = GamePhase.Running;
      Program.BotMessage("RolesAssigned");
      Game.RunGame();
    }
    #endregion

    private static void TestFunction()
    {
      Program.PrintWait("Waiting");
    }

    public static void EndGame()
		{
			GameData.GamePhase = GamePhase.Inactive;
			GameData.Joined = new Dictionary<int, Player>();
		}

    private const string WerewolfFile = @"C:\Users\Lee Yi\Desktop\Everything, for the moment\Coding\C# Bot\TOS-For-Telegram\WFPMembers.xml";

    //Use this to add new users
    public static string ProcessUserId(Message msg)
    {
      XDocument doc = XDocument.Load(WerewolfFile);
      User forward = msg.ForwardFrom;
      foreach(var element in doc.Root.Elements("member"))
      {
        if(long.Parse(element.Element("Id").Value) == msg.ForwardFrom.Id)
        {
          return "User " + forward.Username + " has already been registered!";
        }
      }

      string username = forward.Username;
      if (string.IsNullOrWhiteSpace(forward.Username)) username = string.Empty;

      doc.Root.Add(
        new XElement
        ("member",
        new XElement("username", username),
        new XElement("name", forward.FirstName + " " + forward.LastName),
        new XElement("Id", forward.Id)));
      doc.Save(WerewolfFile);
      return "User " + forward.Username + " has been registered!";
    }

    private static Thread GameStart;
	}
}