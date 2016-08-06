using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Console_API;

using NDesk.Options;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;

namespace QuizBot
{
	class Program
	{
		internal static readonly TelegramBotClient Bot = new TelegramBotClient("255326562:AAHF0Qq2zjjY9tfX2JnqnYSUeXPT6rej3Fk");

		static LogForm ConsoleForm = new LogForm();

		static void Main(string[] notused)
		{
			//Begin Setting the Bot Event Handlers
			Bot.OnMessage += OnMessage;
			Bot.OnMessageEdited += OnMessage;

			//var me = Bot.GetMeAsync().Result;
			Application.Run(ConsoleForm);
		}

		static async void OnMessage(object sender, MessageEventArgs messageEventArgs)
		{
			var message = messageEventArgs.Message;
			var msgtext = message.Text;

			if (message == null || message.Type != MessageType.TextMessage) return;
			
			//If it is a command
			if(msgtext.StartsWith("/")) 
			{
				//Send to the command processor
				Commands.Parse(msgtext);
			}
		}
	}
}
