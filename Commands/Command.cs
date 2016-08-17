using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Args;
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

	public class Command : Attribute
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
		public static Dictionary<string, CommandDelegate> AdminCommands = new Dictionary<string, CommandDelegate>();

		public static Dictionary<string, CommandDelegate> NormalCommands = new Dictionary<string, CommandDelegate>();

		public delegate void CommandDelegate(Message msg, params object[] args);

		public static void InitializeCommands()
		{
			AdminCommands.Add("config", (x, y) => Config(x));
			NormalCommands.Add("join", (x, y) => Join(x));
			NormalCommands.Add("roles", (x, y) => Roles(x, (string[])y));
			NormalCommands.Add("start", (x, y) => Start(x));
			NormalCommands.Add("say", (x, y) => Say(x, (string)y[0]));
			NormalCommands.Add("players", (x, y) => Players(x));
			AdminCommands.Add("ping", (x, y) => Ping(x));
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
        args[0].Length - GameData.WeirdThing.Length);

			try
			{
        #region Admin Commands
        if (AdminCommands.ContainsKey(args[0]))
				{ //If admin command
					if (!admin) NotAdmin(msg);
					else
					{
						AdminCommands[args[0]](msg);
					}
				}
        #endregion

        #region Non admin commands
        else
        { //Non admin command
          switch (args[0])
          {
            case "roles":
              {
                NormalCommands[args[0]](msg, args);
                break;
              }
            case "say":
              {
                NormalCommands[args[0]](msg, cmd);
                break;
              }
            default:
              {
                NormalCommands[args[0]](msg);
                break;
              }
          }
				}
        #endregion
      }
			catch (KeyNotFoundException) { /*Ignore if the command is not recognised */ }
		
		}

		public static void NotAdmin(Message msg)
		{
			Program.BotMessage(msg.Chat.Id, "NotAdminError", msg.From.FirstName);
		}

		#region Join Game Logic
		public static void StartJoinGame(Message msg)
		{
			if (Settings.JoinTime > 60) Time.Tick += new EventHandler(TickHandler1);
			else Time.Tick += new EventHandler(TickHandler2);

			Time.Interval = 10000; //10 seconds by default
			GameData.CurrentGroup = msg.Chat.Id;
			Program.BotMessage("StartGame", msg.From.Username);
			GameData.GamePhase = GamePhase.Joining;
			GameData.Joined.Add(GameData.PlayerCount, msg.From);
			Time.Start();
		}

		public static int TimeLeft = Settings.JoinTime;

		static System.Windows.Forms.Timer Time = new System.Windows.Forms.Timer();

		public static void TickHandler1(object sender, EventArgs e) 
		{ //Less than 1 minute join time
			TimeLeft -= Time.Interval/1000;
			if (TimeLeft % 30 == 0 || TimeLeft == 10)
			{
				Program.BotMessage("JoinSeconds", Settings.JoinTime - TimeLeft);
			}
			else if (TimeLeft == 0)
			{
				Program.BotMessage("BeginGame");
				StartRolesAssign();
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
			foreach (var each in Settings.CurrentRoles)
			{
				totaltoassign += each.Value;
			}
			GameData.GamePhase = GamePhase.Assigning;
			int[] randoms = random.GenerateIntegers(totaltoassign, 0, noroles.Count, false);
		}
		#endregion
		
		public static void EndGame()
		{
			GameData.GamePhase = GamePhase.Inactive;
			GameData.Joined = new Dictionary<int, Player>();
		}

		#region Commands
		[Command(Trigger = "config", GroupAdminOnly = true)]
		private static void Config(Message msg)
		{
			//Send the config menu to the player
			Program.BotMessage(msg.Chat.Id, "SentConfig", msg.From.GetName());
			QuizBot.Config.SendInline(msg.From);
		}

		[Command(InGroupOnly = true, Trigger = "join")]
		private static void Join(Message msg)
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
				Program.BotMessage(msg.Chat.Id, "PlayerJoin", player.Username, GameData.Joined.Count, 5, Settings.MaxPlayers);
				GameData.Joined.Add(GameData.PlayerCount, player);
			}
		}

		[Command(Trigger = "players")]
		private static void Players(Message msg)
		{
			if (GameData.GamePhase == GamePhase.Inactive)
			{
				Program.BotMessage("NoGameJoin");
			}
			else
			{
				StringBuilder output = new StringBuilder("*Players*\n\n");
				foreach (var each in GameData.Joined)
				{
					output.AppendLine(each.Value.FirstName);
				}
				Program.Bot.SendTextMessageAsync(msg.Chat.Id, output.ToString(), parseMode: ParseMode.Markdown);
			}
		}

		[Command(InGroupOnly = true, Trigger = "ping")]
		private static void Ping(Message msg)
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
		private static void Say(Message msg, string cmd)
		{
			if (msg.Chat.Type == ChatType.Group) Program.BotMessage(msg.Chat.Id, "PrivateOnly");
			else
			{
				try
				{
					Program.Bot.SendTextMessageAsync(GameData.CurrentGroup,
						cmd.Substring(3 + GameData.WeirdThing.Length));
				}
				catch (ArgumentOutOfRangeException)
				{
					Program.Bot.SendTextMessageAsync(GameData.CurrentGroup,
						cmd.Substring(3));
				}
				//catch { Program.BotMessage(msg.Chat.Id, "NoGameJoin"); }
			}
		}

		[Command(InGroupOnly = true, Trigger = "start")]
		private static void Start(Message msg)
		{
			if (msg.Chat.Type == ChatType.Private)
			{
				Program.BotMessage(msg.Chat.Id, "PleaseStartBot", msg.Chat.FirstName);
				return;
			}
			if (GameData.GamePhase == GamePhase.Joining)
			{
				Program.BotMessage("RunningGameStart");
				return;
			}
			GameData.CurrentGroup = msg.Chat.Id;
			Program.ConsoleLog("Game started!");
			StartJoinGame(msg);
		}

		[Command(Trigger = "roles")]
		private static void Roles(Message msg, string[] args)
		{
			if (args.Length == 1)
			{
				StringBuilder output = new StringBuilder("*" + Settings.CurrentRoleList + "*" + "\n\n");
				foreach (var each in Settings.CurrentRoles)
				{
					output.Append(each.Key.Name + ", Count: " + each.Value.ToString() + "\n");
				}
				Program.Bot.SendTextMessageAsync(msg.Chat.Id, output.ToString(), parseMode: ParseMode.Markdown);
			}
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
			}
		}
		#endregion
	}
}