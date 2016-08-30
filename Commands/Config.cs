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
				new[] { new InlineKeyboardButton("Wait Time", new Callback(lol, "config", "wait")), new InlineKeyboardButton("Rolelist", new Callback(lol, "Config", "rolelist"))},
				//Row 2
				new[] { new InlineKeyboardButton("Max Users", new Callback(lol, "config", "userCount")), new InlineKeyboardButton("this button does nothing")}
			});
			Program.Bot.SendTextMessageAsync(lol.Id, "Configuration Options", replyMarkup: keyboard);
      Program.ConsoleLog("Sent config markup to " + lol.Username);
    }

    public async static void Parse(Callback data)
    {
      switch(data.Data)
      {
        case "userCount":
          {
            await Program.Bot.SendTextMessageAsync(data.From.Id, "The current maximum number of players is " + Settings.MaxPlayers + ".");
            break;
          }
        case "wait":
          {
            try { await Program.Bot.SendTextMessageAsync(data.From.Id, "The current wait time is " + Settings.JoinTime + " seconds."); }
            catch { }
            break;
          }
      }
    }
	}
}
