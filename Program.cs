using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Console_API;

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

		public static LogForm ConsoleForm = new LogForm();

		[STAThread]
		static void Main(string[] notused)
		{
			//Begin Setting the Bot Event Handlers
			Bot.OnMessage += OnMessage;
			Bot.OnMessageEdited += OnMessage;
			Bot.OnCallbackQuery += OnCallbackQuery;
			GameData.StartTime = DateTime.Now;

			//var me = Bot.GetMeAsync().Result;
			Application.Run(ConsoleForm);
		}

		static async void OnMessage(object sender, MessageEventArgs messageEventArgs)
		{
			var message = messageEventArgs.Message;
			var msgtext = message.Text;

			/*
			if (message.Date < GameData.StartTime)
			{
				return;
			}*/

			if (message == null || message.Type != MessageType.TextMessage) return;
			
			//If it is a command
			if(msgtext.StartsWith("/")) 
			{
				//Send to the command processor
				Commands.Parse(message);
			}
		}

		static async void OnCallbackQuery(object sender, CallbackQueryEventArgs e)
		{
			var result = e.CallbackQuery;
			switch (result.Data)
			{
				#region Configuration Options
				case "wait":
					{
						await Program.Bot.SendTextMessageAsync(result.From.Id, "The current wait time is " + Settings.JoinTime + " seconds.");
						break;
					}
				case "playerCount":
					{
						await Program.Bot.SendTextMessageAsync(result.From.Id, "The current maximum number of players is " + Settings.MaxPlayers + ".");
						break;
					}
				#endregion
			}
		}

		public async static void BotMessage(string text)
		{
			await Bot.SendTextMessageAsync(GameData.CurrentGroup, text);
		}

		public static void ConsoleLog(string text)
		{
			ConsoleForm.Invoke(new Action(() => { ConsoleForm.LogLine(text); }));
		}

		public static void PrintWait(string text)
		{
			Console.WriteLine(text);
			Console.ReadLine();
		}
	}
}
