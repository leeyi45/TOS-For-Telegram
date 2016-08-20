using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace QuizBot
{
  /* The following classes are not my own work, but taken from the 
	 * werewolf for telegram bot
	 * https://github.com/parabola949/Werewolf/
	 */
  internal static class UpdateHelper
	{
	  internal static bool IsGroupAdmin(Update update)
	  {
		  return IsGroupAdmin(update.Message.From.Id, update.Message.Chat.Id);
	  }

	  internal static bool IsGroupAdmin(int user, long group)
	  {
		  //fire off admin request
		  try
		  {
			  var admin = Program.Bot.GetChatMemberAsync(group, user).Result;
			  return admin.Status == ChatMemberStatus.Administrator || admin.Status == ChatMemberStatus.Creator;
		  }
		  catch
		  {
			  return false;
		  }
	  }

	  public static bool HasJoined(Player x)
	  {
		  foreach (var each in GameData.Joined)
		  {
			  if (x == each.Value) return true;
		  }
		  return false;
	  }
	}

	public class Command : System.Attribute
  {
    /// <summary>
    /// The string to trigger the command
    /// </summary>
    public string Trigger { get; set; }

    /// <summary>
    /// Is this command limited to bot admins only
    /// </summary>
    public bool GlobalAdminOnly { get; set; } = false;

    /// <summary>
    /// Is this command limited to group admins only
    /// </summary>
    public bool GroupAdminOnly { get; set; } = false;

    /// <summary>
    /// Developer only command
    /// </summary>
    public bool DevOnly { get; set; } = false;

    /// <summary>
    /// Marks the command as something to block (for example, in support chat)
    /// </summary>
    public bool Blockable { get; set; } = false;

    /// <summary>
    /// Marks the command as to be used within a group only
    /// </summary>
    public bool InGroupOnly { get; set; } = false;

    /// <summary>
    /// Marks the command as to be used within a private chat only
    /// </summary>
    public bool InPrivateOnly { get; set; } = false;
  }

	//Okay this is my work
	public class Commands
	{
    #region Command Registration
    public static Dictionary<string, Tuple<CommandDelegate, Command>> AllCommands = 
      new Dictionary<string, Tuple<CommandDelegate, Command>>();

		public delegate void CommandDelegate(Message msg, string[] args);

		public static void InitializeCommands()
		{
      //Just means I don't have to add the functions myself cause I'm lazy af
      foreach(var method in typeof(Commands).GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
      {
        Command attribute = method.GetCustomAttribute(typeof(Command)) as Command;
        if (attribute == null) continue;
        AllCommands.Add(attribute.Trigger, new Tuple<CommandDelegate, Command>(
          (CommandDelegate)Delegate.CreateDelegate(typeof(CommandDelegate), method), attribute));
      }
		}

		//The default parser
		public static void Parse(Message msg)
		{
			string cmd = msg.Text;
			bool admin = UpdateHelper.IsGroupAdmin(msg.From.Id, msg.Chat.Id);

			//remove the slash as necessary
			cmd = cmd.ToLower().Substring(1, cmd.Length - 1);
			if (string.IsNullOrWhiteSpace(cmd)) return;

			string[] args = cmd.Split(' ');
			//Remove the @quiztestbot
			if (args[0].Contains("@quiztestbot")) args[0] = args[0].Substring(0, 
        args[0].Length - GameData.BotUsername.Length - 1);
      //Program.ConsoleLog("The arg is " + args[0]);
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

        if (attribute.GroupAdminOnly && !admin)
        { //If command is admin only and user is not admin
          NotAdmin(msg);
          return;
        }
        #endregion

        AllCommands[args[0]].Item1(msg, args);
      }
      catch (KeyNotFoundException)
      { //Program.ConsoleLog(e.Message); }
      }
		}

		public static void NotAdmin(Message msg)
		{
			Program.BotMessage(msg.Chat.Id, "NotAdminError", msg.From.FirstName);
		}

    #region Commands
    [Command(Trigger = "config", GroupAdminOnly = true)]
    private static void Config(Message msg, string[] args)
    {
      //Send the config menu to the player
      Program.BotMessage(msg.Chat.Id, "SentConfig", msg.From.GetName());
      QuizBot.Config.SendInline(msg.From);
    }

    [Command(InGroupOnly = true, Trigger = "join")]
    private static void Join(Message msg, string[] args)
    {
      var player = msg.From;
      if (UpdateHelper.HasJoined(player) && GameData.GamePhase == GamePhase.Joining)
      { //Joined already
        Program.BotMessage("AlreadyJoin");
      }
      else if (GameData.GamePhase == GamePhase.Inactive)
      { //Game is not running
        Program.BotMessage("NoGameJoin");
      }
      else if (GameData.GamePhase == GamePhase.Running || GameData.GamePhase == GamePhase.Assigning)
      { //Game is running
        Program.BotMessage("GameRunningJoin");
      }
      else if (GameData.GamePhase == GamePhase.Joining)
      { //Join the player thanks
        Program.BotMessage(msg.Chat.Id, "PlayerJoin", player.Username, GameData.Joined.Count,
          Settings.MinPlayers, Settings.MaxPlayers);
        Program.BotMessage(msg.From.Id, "JoinGameSuccess", msg.Chat.Title);
        GameData.Joined.Add(GameData.PlayerCount, player);
      }
    }

    [Command(Trigger = "players")]
    private static void Players(Message msg, string[] args)
    {
      if (GameData.GamePhase == GamePhase.Inactive) Program.BotMessage("NoGameJoin");
      else
      {
        StringBuilder output = new StringBuilder("*Players*\n\n");
        foreach (var each in GameData.Joined)
        {
          output.Append(each.Value.Username);
          if (!each.Value.IsAlive) output.Append(": " + each.Value.role.ToString());
          output.Append("\n");

        }
        Program.Bot.SendTextMessageAsync(msg.Chat.Id, output.ToString(), parseMode: ParseMode.Markdown);
      }
    }

    [Command(InGroupOnly = true, Trigger = "ping")]
    private static void Ping(Message msg, string[] args)
    {
      var ts = DateTime.UtcNow - msg.Date;
      var send = DateTime.UtcNow;
      /*
			var message = "PingInfo\n" + ts.ToString("mm\\:ss\\.ff") + "\n" +
					System.Diagnostics..AvgCpuTime.ToString("F0") +
					Program.MessageRxPerSecond.ToString("F0") + " MAX IN\n" +
					Program.MessageTxPerSecond.ToString("F0") + "MAX OUT";*/
      var message = "*PingInfo*\n" + "Time to receive ping message: " + ts.ToString("fff'.'ff") + "ms";
      var result = Program.Bot.SendTextMessageAsync(msg.Chat.Id, message,
        parseMode: ParseMode.Markdown).Result;
      ts = DateTime.UtcNow - send;
      Program.Bot.EditMessageTextAsync(msg.Chat.Id, result.MessageId, message + "\nTime to send ping message: " +
        ts.ToString("fff'.'ff") + "ms", parseMode: ParseMode.Markdown);
    }

    [Command(InPrivateOnly = true, Trigger = "say")]
    private static void Say(Message msg, string[] args)
    {
      //if (msg.Chat.Type == ChatType.Group) Program.BotMessage(msg.Chat.Id, "PrivateOnly");
      string[] otherargs = new string[args.Length - 1];
      Array.Copy(args, 1, otherargs, 0, args.Length - 1);
      Program.Bot.SendTextMessageAsync(GameData.CurrentGroup, string.Join(" ", otherargs));
      //catch { Program.BotMessage(msg.Chat.Id, "NoGameJoin"); }
    }

    [Command(Trigger = "start")]
    private static void Start(Message msg, string[] args)
    {
      if (msg.Chat.Type == ChatType.Private)
      {
        //Program.BotMessage(msg.Chat.Id, "PleaseStartBot", msg.Chat.FirstName);
        return;
      }
      if (GameData.GamePhase == GamePhase.Joining) Program.BotMessage("RunningGameStart");
      // else 
      else
      {
        GameData.CurrentGroup = msg.Chat.Id;
        Program.ConsoleLog("Game started!");
        StartJoinGame(msg);
      }
    }

    [Command(Trigger = "roles")]
    private static void Roles(Message msg, string[] args)
    {
      StringBuilder output = new StringBuilder("*" + Settings.CurrentRoleList + "*" + "\n\n");
      foreach (var each in Settings.CurrentRoles)
      {
        output.Append(each.Key.Name + ", Count: " + each.Value.ToString() + "\n");
      }
      Program.Bot.SendTextMessageAsync(msg.Chat.Id, output.ToString(), parseMode: ParseMode.Markdown);

      /*
			else if (args.Length == 2) // /role + rolename
			{
				string r = "";
				try
				{
					r = args[1][0].ToString().ToUpper() + args[1].Substring(1, args[1].Length);

					Program.Bot.SendTextMessageAsync(msg.Chat.Id, "*" + GameData.Roles[r].Name + "*\n" + GameData.Roles[r].description,
						parseMode: ParseMode.Markdown);
				}
				catch (KeyNotFoundException)
				{
					Program.BotMessage(msg.Chat.Id, "RoleNotFound", r);
				}
				catch { }
			}*/
    }

    [Command(Trigger = "listroles")]
    private static void ListRoles(Message msg, string[] args)
    {
      StringBuilder output = new StringBuilder("<b>Currently Registered Roles:</b>\n\n");
      foreach (var each in GameData.Roles)
      {
        output.AppendLine(each.Value.Name + ": <i>" + each.Value.description + "</i>");
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
    #endregion
    #endregion

    #region Join Game Logic
    public static void StartJoinGame(Message msg)
		{
			if (Settings.JoinTime < 60) Time.Tick += new EventHandler(TickHandler1);
			else Time.Tick += new EventHandler(TickHandler2);

			Time.Interval = 1000; //1 seconds by default
			GameData.CurrentGroup = msg.Chat.Id;
			Program.BotMessage("GameStart", msg.From.Username);
			GameData.GamePhase = GamePhase.Joining;
			GameData.Joined.Add(GameData.PlayerCount, msg.From);
			Time.Start();
		}

		public static int TimeLeft = Settings.JoinTime;

		static System.Windows.Forms.Timer Time = new System.Windows.Forms.Timer();

		public static void TickHandler1(object sender, EventArgs e) 
		{ //Less than 1 minute join time
      System.Windows.Forms.MessageBox.Show("I got here");
			TimeLeft -= Time.Interval/1000;
      Console.Write(TimeLeft.ToString());
			if (TimeLeft % 30 == 0 || TimeLeft == 10)
			{
				Program.BotMessage("JoinSeconds", Settings.JoinTime - TimeLeft);
			}
			else if (TimeLeft == 0)
			{
        if (GameData.PlayerCount < Settings.MinPlayers)
        {
          Program.BotMessage("NotEnoughPlayers");
        }
        else
        {
          Program.BotMessage("BeginGame");
          StartRolesAssign();
        }
			}
		}

		public static void TickHandler2(object sender, EventArgs e)
		{ //More than 1 minute join time

		}
		#endregion

		#region Assign Roles
		public static void StartRolesAssign()
		{
			var noroles = GameData.Joined;
			var hasroles = new Dictionary<int, Player>();
			var random = new org.random.JSONRPC.RandomJSONRPC("bbcfa0f8-dbba-423a-8798-c8984c4fc5c5");
			int totaltoassign = 0;
      GameData.GamePhase = GamePhase.Assigning;
      foreach (var each in Settings.CurrentRoles)
			{
        totaltoassign += each.Value;
			}

      //Only actual randoms are being used for players
      int[] randoms = random.GenerateIntegers(totaltoassign, 0, noroles.Count, false);

      for(int i = 0; i < Settings.CurrentRoles.Count; i++)
      { //based on priority assign
        var role = Settings.CurrentRoles.ToArray();
        Player player = new Player();
        for(; i < i + role[i].Value; i++)
        {
          player = noroles[randoms[i]];
          if(role[i].Key is Role) player.role = role[i].Key as Role;
          else if (role[i].Key is Alignment)
          {
            var attribute = role[i].Key as Alignment;
            var ranGen = new Random();
            if (attribute.Name == "Any")
            { //Assign any role to the player
              player.role = GameData.Roles.ToArray()[ranGen.Next(0, GameData.Roles.Count)].Value;
            }
            else
            {
              //Need to randomly select a role
              var iterator = from each in GameData.Roles
                             where each.Value.attribute == (attribute)
                             select each;
              var tempRoles = new Dictionary<int, Role>();
              foreach (var each in iterator)
              {
                tempRoles.Add(tempRoles.Count, each.Value);
              }
              player.role = tempRoles[ranGen.Next(0, tempRoles.Count)];
            }
          }

          hasroles.Add(hasroles.Count, player);
          noroles.Remove(i);
        }
        //Break if there are no more players or no more roles to assign
        if (noroles.Count == 0  || Settings.CurrentRoles.Count == i) break;
      }

      //Check if there are players that still have no roles
      if (noroles.Count > 0)
      {
        //If there are unassigned people
        foreach (var each in noroles)
        {
          each.Value.role = GameData.Roles["Villager"];
          hasroles.Add(hasroles.Count, each.Value);
          noroles.Remove(each.Key);
        }
      }
      GameData.Joined = hasroles; //Update the main dictionary
		}
		#endregion
		
		public static void EndGame()
		{
			GameData.GamePhase = GamePhase.Inactive;
			GameData.Joined = new Dictionary<int, Player>();
		}

	}
}