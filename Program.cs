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
	static class Program
	{
		internal static readonly TelegramBotClient Bot = new TelegramBotClient("255326562:AAHF0Qq2zjjY9tfX2JnqnYSUeXPT6rej3Fk");

		public static LogForm ConsoleForm;

		[STAThread]
		static void Main(string[] notused)
		{
			//Begin Setting the Bot Event Handlers
			Bot.OnMessage += OnMessage;
			Bot.OnMessageEdited += OnMessage;
			Bot.OnCallbackQuery += OnCallbackQuery;
			GameData.StartTime = DateTime.Now;
			GameData.GamePhase = GamePhase.Inactive;
			ConsoleForm = new LogForm();
			//var me = Bot.GetMeAsync().Result;
			GameData.InitializeRoles();
			GameData.InitializeMessages();
      Commands.InitializeCommands();
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

			ConsoleLog("Message \"" + msgtext + "\" received from " + message.From.FirstName + " " + message.From.LastName);

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

		/// <summary>
		/// Send a message using a message with the specified key
		/// </summary>
		/// <param name="key">Message key</param>
		/// <param name="args">Message arguments</param>
		public async static void BotMessage(string key, params object[] args)
		{
      try { await Bot.SendTextMessageAsync(GameData.CurrentGroup, string.Format(GameData.Messages[key], args)); }
			catch (Telegram.Bot.Exceptions.ApiRequestException) { }
		}

		/// <summary>
		/// Send a message to the specified group using a message with specified key
		/// </summary>
		/// <param name="id">Group id</param>
		/// <param name="key">Message Key</param>
		/// <param name="args">Message Arguments</param>
		public async static void BotMessage(long id, string key, params object[] args)
		{
			await Bot.SendTextMessageAsync(id, string.Format(GameData.Messages[key], args));
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

		public static string GetName(this User x)
		{
			return x.FirstName + " " + x.LastName;
		}
	}
}
