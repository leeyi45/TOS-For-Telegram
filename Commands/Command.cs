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
using static QuizBot.GameData;

namespace QuizBot
{
	public class Commands
	{
    #region Command Registration
    public static Dictionary<string, Command> AllCommands = new Dictionary<string, Command>();

		public delegate void CommandDelegate(Message msg, string[] args);

    private static bool TestMode = true;

		public static void InitializeCommands()
		{
      //Just means I don't have to add the functions myself cause I'm lazy af
      Log("Registering commands", false);
      foreach(var method in typeof(Commands).GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
      {
        var attri = method.GetCustomAttribute<Command>();
        if(attri != null)
        {
          attri.Info = (Game.CommandDelegate)Delegate.CreateDelegate(typeof(Game.CommandDelegate), method);
          AllCommands.Add(attri.Trigger, attri);
          Log("Command \"" + attri.Trigger + "\" registered", false);
        }
      }

      Log("Loading Blocked People", false);
      var doc = GDExtensions.SafeLoad("UserData.xml");
      BlockedPeople = doc.Root.Elements("Id").Select(x => x.Value).ToList();
    }

		//The default parser
		public static void Parse(Message msg)
		{
			//remove the slash as necessary
			string cmd = msg.Text.ToLower().Substring(1, msg.Text.Length - 1);
			if (string.IsNullOrWhiteSpace(cmd)) return;

			string[] args = cmd.Split(' ');
			//Remove the @quiztestbot
			if (args[0].Contains(BotUsername)) args[0] = args[0].Replace("@" + BotUsername, "");
      //Program.PrintWait("the arg is " + args[0]);

      //Check stuff
      Command attribute;

      if(!AllCommands.TryGetValue(args[0], out attribute))
      {
        //Check for game instance
        if(GameInstances.Keys.Contains(msg.Chat.Id))
        { //Game Instance exists
          if(!GameInstances[msg.Chat.Id].AllCommands.TryGetValue(args[0], out attribute))
          { //No such command in game instance
            throw new InvalidCommandException(args[0]);
          }
        }
        else
        { //Game Instance does not exist
          Program.BotMessage(msg.Chat.Id, "NoInstance");
          return;
        }
      }

      if (!CommandCheck(attribute, msg)) return;

      if (attribute.IsNotInstance) attribute.Info(msg, args);
      else GameInstances[msg.Chat.Id].AllCommands[args[0]].Info(msg, args);
		}
    #endregion

    #region Commands
    [Command(Trigger = "config", GroupAdminOnly = true)]
    private static void Config(Message msg, string[] args)
    {
      //Send the config menu to the player
      Program.BotMessage(msg.Chat.Id, "SentConfig", msg.From.GetName());
      QuizBot.Config.SendInline(msg.From, msg.Chat.Title);
    }

    [Command(Trigger = "createinstance", InGroupOnly = true)]
    private static void CreateInstance(Message msg, string[] args)
    {
      if(GameInstances.Keys.Contains(msg.Chat.Id))
      {
        Program.BotMessage(msg.Chat.Id, "HasInstance");
      }
      else
      {
        GameInstances.Add(msg.Chat.Id, new Game(msg));
      }
    }

    [Command(Trigger = "ping", InGroupOnly = true)]
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

    [Command(Trigger = "Refresh")]
    private static void Refresh(Message msg, string[] args)
    {
      GameInstances[msg.Chat.Id].Refresh(msg);
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
      Program.BotMessage(msg.Chat.Id, "GetChatID", msg.Chat.Id.ToString());
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
        string[] name = each.TryGetElementValue("WFPMembers.xml", "name").Split(' ');
        otherroles.Add(otherroles.Count, 
          new Player(int.Parse(each.TryGetElementValue("WFPMembers.xml", "Id")), 
          each.TryGetElementValue("WFPMembers.xml", "username"),
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
      if (!CommandVars.GetUserId)
      {
        CommandVars.GetUserId = true;
        Program.Bot.SendTextMessageAsync(msg.Chat.Id, "Forward the messages please");
      }
      else
      {
        CommandVars.GetUserId = false;
        Program.Bot.SendTextMessageAsync(msg.Chat.Id, "Done registering IDs");
      }
    }

    [Command(Trigger = "reload", GroupAdminOnly = true, InGroupOnly = true)]
    private static void Reload(Message msg, string[] args)
    {
      if(args.Length < 2)
      {
        Program.BotMessage(msg.Chat.Id, "NotEnoughArgs", "2", "reload");
      }
      else
      {
        var output = "";
        try
        {
          switch (args[1].ToLower())
          {
            case "roles": { InitializeRoles(true); break; }
            case "messages": { goto case "msgs"; }
            case "msgs": { InitializeMessages(true); break; }
            case "protocols": { InitializeProtocols(true); break; }
            default: { throw new ArgumentException(); }
          }
          output = args[1] + " reloaded";
        }
        catch(ArgumentException)
        {
          output = "Unrecognised argument " + args[1];
        }
        Program.BotNormalMessage(msg.Chat.Id, output);
      }
    }

    [Command(Trigger = "block", GroupAdminOnly = true)]
    private static void Block(Message msg, string[] args)
    {
      try { BlockedPeople.Add(args[1]); }
      catch(IndexOutOfRangeException) { Program.BotMessage(msg.Chat.Id, "BlockError");  }
    }

    [Command(Trigger = "unblock", GroupAdminOnly = true)]
    private static void Unblock(Message msg, string[] args)
    {
      try { BlockedPeople.Remove(args[1]); }
      catch (IndexOutOfRangeException) { Program.BotMessage(msg.Chat.Id, "BlockError"); }
    }
    #endregion

    /*
    #region Assign Roles
    private static void BeginGame()
    {
      if (Settings.UseNicknames) ObtainNicknames();
      else GameStart.Start();
    }
    private static void StartRolesAssign()
		{
			var noroles = Joined;
      var hasroles = new List<Player>();
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
      if(noroles.Count > 0)
      {
        foreach(var each in noroles)
        {
          each.role = GameData.Roles["villager"];
          hasroles.Add(each);
        }
      }

      Joined = hasroles;
      foreach(var each in hasroles)
      {
        each.OnAssignRole();
      }
      GameData.GamePhase = GamePhase.Running;
      Program.BotMessage("RolesAssigned");
      Game.RunGame();
    }

    private static void ObtainNicknames()
    {
      foreach(var each in Joined)
      {
        Program.BotMessage(each.Id, "GetNickname");
      }
      CommandVars.GettingNicknames = true;
    }
    #endregion*/

    private static void TestFunction()
    {
      Program.PrintWait("Waiting");
    }

    public static bool CommandCheck(Command attribute, Message msg)
    {
      if (attribute.InGroupOnly &&
  (msg.Chat.Type == ChatType.Private || msg.Chat.Type == ChatType.Channel))
      { //If the chat is not a group and the command is group only
        Program.BotMessage(msg.Chat.Id, "GroupOnly");
        return false;
      }

      if (attribute.InPrivateOnly && msg.Chat.Type != ChatType.Private)
      { //If the chat is not private and the command is private only
        Program.BotMessage(msg.Chat.Id, "PrivateOnly");
        return false;
      }

      if (attribute.GroupAdminOnly && !Player.IsGroupAdmin(msg.From.Id, msg.Chat.Id))
      { //If command is admin only and user is not admin
        Program.BotMessage(msg.Chat.Id, "NotAdminError", msg.From.Username);
        return false;
      }

      if (attribute.DevOnly && msg.From.Id != Chats.chats["dev"])
      { //User is not dev
        Program.BotMessage(msg.Chat.Id, "NotDevError", msg.From.Username);
        return false;
      }

      try
      {
        if (attribute.GameStartOnly && !GameInstances[msg.Chat.Id].GameStarted)
        {
          throw new KeyNotFoundException();
        }
      }
      catch(KeyNotFoundException)
      {
        Program.BotMessage(msg.Chat.Id, "NoGameJoin");
        return false;
      }
      return true;
    }

    public static void EndGame()
		{
			GameData.GamePhase = GamePhase.Inactive;
			Joined = new List<Player>();
		}

    private const string WerewolfFile = @"WFPMembers.xml";

    //Use this to add new users
    public static string ProcessUserId(Message msg)
    {
      XDocument doc = GDExtensions.SafeLoad(WerewolfFile);
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
      doc.Save(xmlLocation + WerewolfFile);
      return "User " + forward.Username + " has been registered!";
    }

    public static void ProcessNicknames(Message msg)
    {
      var player = Player.GetPlayer(msg.From.Id);
      if (player.Nickname != null)
      {
        Program.BotMessage(msg.From.Id, "ChangedNickname", msg.Text);
      }
      else
      {
        Program.BotMessage(msg.From.Id, "GotNickname", msg.Text);
      }
      player.Nickname = msg.Text;
      var count = Joined.Count(x => x.Nickname == null);
      if(count == 0)
      {
        CommandVars.GettingNicknames = false;
      }
      else Program.BotMessage("NicknamesLeft", count);
    }

    public static Dictionary<long, Game> GameInstances { get; set; }

    public static List<string> BlockedPeople { get; set; }
	}
}