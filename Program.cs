using System;
using System.Linq;
using System.Windows.Forms;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
		  GameData.StartTime = DateTime.Now.AddHours(-8);
		  ConsoleForm = new LogForm();
      GameData.InitializeRoles();
		  GameData.InitializeMessages();
      Commands.InitializeCommands();
      Chats.getChats();
      GameData.BotUsername = Bot.GetMeAsync().Result.Username;
		  Application.Run(ConsoleForm);
		}

		static void OnMessage(object sender, MessageEventArgs messageEventArgs)
		{
			var message = messageEventArgs.Message;
			var msgtext = message.Text;

      //Spam filter
      //Messages received while not receiving are ignored
      if (message.Date.Ticks < GameData.StartTime.Ticks) return;

			if (message == null || message.Type != MessageType.TextMessage) return;

      if(message.ForwardFrom != null && Settings.GetUserId)
      {
        Bot.SendTextMessageAsync(message.From.Id, Commands.ProcessUserId(message));
      }

			ConsoleLog("Message \"" + msgtext + "\" received from " + message.From.FirstName + " " + message.From.LastName);

			//If it is a command
			if(msgtext.StartsWith("/")) 
			{
				//Send to the command processor
				Commands.Parse(message);
			}
		}

		static void OnCallbackQuery(object sender, CallbackQueryEventArgs e)
		{
      var From = e.CallbackQuery.From;
      var result = new Callback(e.CallbackQuery.Data);    
			switch (result.Protocol)
			{
				case "config":
					{
            Config.Parse(result);
						break;
					}
        case "nightAction":
          {
            Game.ParseNightAction(result);
            break;
          }
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

    public async static void BotMessage(string message)
    {
      try
      {
        await Bot.SendTextMessageAsync(GameData.CurrentGroup, message, parseMode: ParseMode.Markdown);
      }
      catch (Telegram.Bot.Exceptions.ApiRequestException) { }
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
