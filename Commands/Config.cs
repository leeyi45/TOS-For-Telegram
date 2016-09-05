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
		public static void SendInline(User lol, Chat chat)
		{
			InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(new[] 
			{
				//Row 1
				new[] { new InlineKeyboardButton("Wait Time", new Callback(lol.Id, "config", "wait")), new InlineKeyboardButton("Rolelist", new Callback(lol.Id, "config", "rolelist"))},
				//Row 2
				new[] { new InlineKeyboardButton("Max Users", new Callback(lol.Id, "config", "userCount")), new InlineKeyboardButton("Nicknames", new Callback(lol.Id, "config", "nicknames"))}
			});
			Program.Bot.SendTextMessageAsync(lol.Id, "Configuration Options for group: " + chat.Title, replyMarkup: keyboard);
      Program.ConsoleLog("Sent config markup to " + lol.Username);
    }

    public async static void Parse(Callback data)
    {
      switch(data.Data)
      {
        case "userCount":
          {
            await Program.Bot.SendTextMessageAsync(data.From, "The current maximum number of players is " + Settings.MaxPlayers + ".");
            break;
          }
        case "wait":
          {
            try { await Program.Bot.SendTextMessageAsync(data.From, "The current wait time is " + Settings.JoinTime + " seconds."); }
            catch { }
            break;
          }
        case "nicknames":
          {
            Settings.UseNicknames = !Settings.UseNicknames;
            await Program.Bot.SendTextMessageAsync(data.From, "Allow Nicknames set to: " + Settings.UseNicknames);
            break;
          }
      }
    }
	}
}
