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
		internal static readonly TelegramBotClient Bot = new TelegramBotClient(Chats.BotToken);

		public static LogForm ConsoleForm;

		[STAThread]
		static void Main(string[] notused)
		{
		    //Begin Setting the Bot Event Handlers
		  Bot.OnMessage += OnMessage;
		  Bot.OnMessageEdited += OnMessage;
		  Bot.OnCallbackQuery += OnCallbackQuery;
		  GameData.StartTime = DateTime.Now;
		  ConsoleForm = new LogForm();
      GameData.InitializeRoles();
		  GameData.InitializeMessages();
      Commands.InitializeCommands();
      Chats.getChats();
      GameData.BotUsername = Bot.GetMeAsync().Result.Username;
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
      try { await Bot.SendTextMessageAsync(GameData.CurrentGroup, string.Format(GameData.Messages[key], args),
        parseMode: ParseMode.Markdown); }
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
      try { await Bot.SendTextMessageAsync(id, string.Format(GameData.Messages[key], args), parseMode: ParseMode.Markdown); }
      catch(Exception) { }
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

    #region Extension Methods
    public static string GetName(this User x)
		{
			return x.FirstName + " " + x.LastName;
		}

    public static string GetAttributeValue(this System.Xml.Linq.XElement x, System.Xml.Linq.XName name)
    {
      return x.Attribute(name).Value;
    }

    public static string GetElementValue(this System.Xml.Linq.XElement x, System.Xml.Linq.XName name)
    {
      return x.Element(name).Value;
    }

    public static bool TryGetAttribute(this System.Xml.Linq.XElement x, System.Xml.Linq.XName name, out string output)
    {
      try
      {
        output = x.Attribute(name).Value;
        return true;
      }
      catch(NullReferenceException)
      {
        output = null;
        return false;
      }
    }

    public static bool TryGetElement(this System.Xml.Linq.XElement x, System.Xml.Linq.XName name, out string output)
    {
      try
      {
        output = x.Element(name).Value;
        return true;
      }
      catch (NullReferenceException)
      {
        output = null;
        return false;
      }
    }

    /// <summary>
    /// Only checks the root element for containing elements
    /// </summary>
    /// <param name="doc">Document</param>
    /// <param name="name">Name of the element</param>
    /// <returns>Boolean value indicating if the element was found</returns>
    public static bool HasElement(this System.Xml.Linq.XDocument doc, System.Xml.Linq.XName name)
    {
      foreach(var each in doc.Root.Elements())
      {
        //Program.PrintWait(each.Name.ToString());
        if (each.Name == name) return true;
      }
      return false;
    }

    public static int[] Next(this Random random, int count, int min, int max, bool replace = false)
    {
      int[] output = new int[count];
      int x;
      for(int i = 0; i < count; i++)
      {
        x = random.Next(min, max);
        if (output.Contains(x) && !replace)
        {
          i--;
          continue;
        }
        else output[i] = x;
      }
      return output;
    }
    #endregion
  }
}
