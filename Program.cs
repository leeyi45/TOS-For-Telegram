using System;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using Newtonsoft.Json;

namespace QuizBot
{
  static class Program
	{
		internal static readonly TelegramBotClient Bot = new TelegramBotClient(Chats.BotToken);

    //public static LogForm ConsoleForm;
    public static Startup startup;

		[STAThread]
		static void Main(string[] notused)
		{
      startup = new Startup();
      GameData.BotUsername = Bot.GetMeAsync().Result.Username;
      GameData.StartTime = DateTime.Now.AddHours(-8);
      Application.EnableVisualStyles();
      Application.Run(startup);
		}

    public static void LoadBot()
    {

      Bot.OnMessage += OnMessage;
      Bot.OnMessageEdited += OnMessage;
      Bot.OnCallbackQuery += OnCallbackQuery;

    }

    static Dictionary<string, Action<Callback>> Parsers;

    public static void LoadParsers()
    {
      Parsers = new Dictionary<string, Action<Callback>>();
      Parsers.Add("config", new Action<Callback>(Config.Parse));
      Parsers.Add("nightAction", new Action<Callback>(Game.ParseNightAction));
      Parsers.Add("voteAction", new Action<Callback>(Game.ParseVoteChoice));
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
      var result = JsonConvert.DeserializeObject<Callback>(e.CallbackQuery.Data);
      try
      {
        Parsers[result.Protocol](result);
      }
      catch(NullReferenceException)
      {
        ConsoleLog("Callback parsers have not been loaded!");
      }
      catch(KeyNotFoundException)
      {
        ConsoleLog("Callback received from " + From.Username + " with unknown protocol + \"" +
          result.Protocol + "\"");
      }
		}

    #region Console logging
    public static void ConsoleLog(string text)
		{
      try { startup.ConsoleForm.Invoke(new Action(() => { startup.ConsoleForm.LogLine(text); })); }
      catch(NullReferenceException) { }
      catch(InvalidOperationException) { }
		}

    public static void MessageLog(string text, string caption = "")
    {
      MessageBox.Show(text, caption);
      ConsoleLog(text);
    }

		public static void PrintWait(string text = "")
		{
			Console.WriteLine(text);
			Console.ReadLine();
		}
    #endregion

    #region Extension Methods
    public static string GetName(this User x)
		{
			return x.FirstName + " " + x.LastName;
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

    #region BotMessage
    /// <summary>
    /// Send a message using a message with the specified key
    /// </summary>
    /// <param name="key">Message key</param>
    /// <param name="args">Message arguments</param>
    public async static void BotMessage(string key, params object[] args)
    {
      try
      {
        await Bot.SendTextMessageAsync(GameData.CurrentGroup, string.Format(GameData.Messages[key], args),
    parseMode: ParseMode.Markdown);
      }
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
      catch (Exception) { }
    }

    public async static void BotMessage(string message)
    {
      try
      {
        await Bot.SendTextMessageAsync(GameData.CurrentGroup, message, parseMode: ParseMode.Markdown);
      }
      catch (Telegram.Bot.Exceptions.ApiRequestException) { }
    }
    #endregion
  }
}
