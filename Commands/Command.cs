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
        public bool GlobalAdminOnly { get; set; }

        /// <summary>
        /// Is this command limited to group admins only
        /// </summary>
        public bool GroupAdminOnly { get; set; }

        /// <summary>
        /// Developer only command
        /// </summary>
        public bool DevOnly { get; set; }

        /// <summary>
        /// Marks the command as something to block (for example, in support chat)
        /// </summary>
        public bool Blockable { get; set; }

        public bool InGroupOnly { get; set; }
    }

	//Okay this is my work
	public class Commands
	{
		//The default parser
		public static void Parse(Message msg)
		{
			string cmd = msg.Text;
			bool admin = UpdateHelper.IsGroupAdmin(msg.From.Id, msg.Chat.Id);
			//remove the slash as necessary
			cmd = cmd.ToLower().Substring(1, cmd.Length - 1);
			string[] args = cmd.Split(' ');
			//Remove the @quiztestbot
			args[0] = args[0].Substring(0, args[0].Length - GameData.WeirdThing.Length);
			Console.WriteLine("The arg is " + args[0]);
			switch (args[0])
			{
				case "start":
					{
						if (GameData.GamePhase == GamePhase.Joining)
						{
							Program.Bot.SendTextMessageAsync(msg.Chat.Id, "Game has already started. Use /join to join!");
							return;
						}
						GameData.CurrentGroup = msg.Chat.Id;
						Program.ConsoleLog("Game started!");
						StartJoinGame(msg);
						break;
					}
				case "join":
					{
						JoinPlayer(msg.From);
						break;
					}
				case "forcestart":
					{
						break;
					}
				case "config":
					{ //PM the config menu
						if (!admin) 
						{
							Program.Bot.SendTextMessageAsync(msg.Chat.Id, msg.From.FirstName + ", you are not an admin!");
						}
						else
						{
							//Send the config menu to the player
							Program.Bot.SendTextMessageAsync(msg.Chat.Id, msg.From.FirstName + ", I have sent you the config via PM");
							Config.SendInline(msg.From);
						}
						break;
					}
				case "leave":
					{
						
						break;
					}
				case "roles":
					{
						StringBuilder output = new StringBuilder( "*" + Settings.CurrentRoleList + "*" + "\n\n");
						foreach (var each in Settings.CurrentRoles)
						{
							output.Append(each.Key.Name + ", Count: " + each.Value.ToString() + "\n");
						}
						Program.Bot.SendTextMessageAsync(msg.Chat.Id, output.ToString(), parseMode: ParseMode.Markdown);
						break;
					}
			}
		}

		#region Join Game Logic
		public static void JoinPlayer(Player player)
		{
			if (UpdateHelper.HasJoined(player))
			{
				Program.BotMessage(player.Username + ", you have already joined the game!");
				return;
			}
			Program.BotMessage(player.Username + " has joined the game!");
			GameData.Joined.Add(GameData.PlayerCount, player);
		}

		public static void StartJoinGame(Message msg)
		{
			if (Settings.JoinTime > 60) Time.Tick += new EventHandler(TickHandler1);
			else Time.Tick += new EventHandler(TickHandler2);

			Time.Interval = 10000; //10 seconds by default
			GameData.CurrentGroup = msg.Chat.Id;
			GameData.GameStarted = true;
			Program.BotMessage(msg.From.Username + " has started a game! /join to join! \n" +
				Settings.JoinTime + " seconds left to /join!");
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
				Program.BotMessage(Settings.JoinTime + " seconds left to /join!");
			}
			else if (TimeLeft == 0)
			{
				Program.BotMessage("Game started. Please switch over to message the bot to begin");
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
	}
}