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

	public class Commands
	{
		//The default parser
		public static void Parse(string cmd)
		{
			//remove the slash as necessary
			cmd = cmd.Substring(1, cmd.Length - 1);
			string[] args = cmd.Split(' ');
		}
	}
}
