using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;


namespace QuizBot
{
	class Config
	{
		public static void SendInline(User lol)
		{
			InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(new[] 
			{
				//Row 1
				new[] { new InlineKeyboardButton("Wait Time", "wait"), new InlineKeyboardButton("Rolelist", "rolelist")},
				//Row 2
				new[] { new InlineKeyboardButton("Max Users", "userCount"), new InlineKeyboardButton("this button does nothing")}
			});
			Program.Bot.SendTextMessageAsync(lol.Id, "Configuration Options", replyMarkup: keyboard);
		}
	}
}
