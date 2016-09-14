using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.Xml.Linq;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace QuizBot
{
  static class Program
	{
		internal static readonly TelegramBotClient Bot = new TelegramBotClient(Chats.BotToken);

    //public static LogForm ConsoleForm;
    public static Startup startup { get; set; }

    [STAThread]
		static void Main(string[] notused)
    {
      startup = new Startup();
      Application.ApplicationExit += OnClosing;
      GameData.StartTime = DateTime.Now.AddHours(-8);
      Application.EnableVisualStyles();
      Application.Run(startup);

		}

    public static Dictionary<string, Action<Callback>> Parsers { get; set; }

		static void OnMessage(object sender, MessageEventArgs messageEventArgs)
		{
      try
      {
        var message = messageEventArgs.Message;
        var msgtext = message.Text;

        //Spam filter
        //Messages received while not receiving are ignored
        if (message.Date.Ticks < GameData.StartTime.Ticks) return;

        if (message == null || message.Type != MessageType.TextMessage) return;

        if (message.ForwardFrom != null && CommandVars.GetUserId)
        {
          Bot.SendTextMessageAsync(message.From.Id, Commands.ProcessUserId(message));
        }

        if (message.Chat.Type == ChatType.Private && CommandVars.GettingNicknames)
        {
          Commands.ProcessNicknames(message);
        }

        if (Commands.BlockedPeople.Contains(message.From.Id.ToString())) return;

        try
        {
          if (CommandVars.ReceivingVals[message.From.Id].Item1)
          {
            Config.ParseValue(message);
          }
        }
        catch (KeyNotFoundException) { }

        ConsoleLog("Message \"" + msgtext + "\" received from " + message.From.FirstName + " " + message.From.LastName);

        //If it is a command
        if (msgtext.StartsWith("/"))
        {
          //Send to the command processor
          Commands.Parse(message);
        }
      }
      finally
      {
        OnClosing(null, null);
      }
		}

		static void OnCallbackQuery(object sender, CallbackQueryEventArgs e)
		{
      var From = e.CallbackQuery.From;
      var result = JsonConvert.DeserializeObject<Callback>(e.CallbackQuery.Data);
      try
      {
        Commands.GameInstances[result.Group].Parsers[result.Protocol](result);
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

    public static void TryToBot(bool logtoconsole)
    {
      bool lol = true;
      while (lol)
      {
        try
        {
          GameData.Log("Connecting to bot", logtoconsole);
          //Test if the bot is working
          GameData.BotUsername = Bot.GetMeAsync().Result.Username;
          GameData.Log("Connected to bot", logtoconsole);
          CommandVars.Connected = true;
          break;
        }
        catch when (!logtoconsole)
        {
          switch (MessageBox.Show("Failed to connect to the telegram servers", "Connection Error",
            MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error, MessageBoxDefaultButton.Button3))
          {
            case DialogResult.Abort:
              {
                Application.Exit();
                break;
              }
            case DialogResult.Ignore:
              {
                lol = false;
                break;
              }
            case DialogResult.Retry:
              {
                break;
              }
          }
        }
        catch
        {
          ConsoleLog("Failed to connect to the telegram server");
          break;
        }
      }
      GameData.Log("Loading bot handlers", false);
      Bot.OnMessage += OnMessage;
      Bot.OnMessageEdited += OnMessage;
      Bot.OnCallbackQuery += OnCallbackQuery;
      GameData.Log("Loaded bot handlers", false);
    }

    #region Console logging
    public static void ConsoleLog(string text)
		{
      try { startup.ConsoleForm?.Invoke(new Action(() => { startup.ConsoleForm.LogLine(text); })); }
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

    public async static void BotNormalMessage(string message)
    {
      try
      {
        await Bot.SendTextMessageAsync(GameData.CurrentGroup, message, parseMode: ParseMode.Markdown);
      }
      catch (Telegram.Bot.Exceptions.ApiRequestException) { }
    }

    public async static void BotNormalMessage(long Id, string message)
    {
      try
      {
        await Bot.SendTextMessageAsync(Id, message, parseMode: ParseMode.Markdown);
      }
      catch (Telegram.Bot.Exceptions.ApiRequestException) { }
    }

    public async static void EditBotMessage(long id, int messageId, string key, params object[] args)
    {
      await Bot.EditMessageTextAsync(id, messageId, string.Format(GameData.Messages[key], args),
        parseMode: ParseMode.Markdown);
    }

    public async static void EditBotMessage(long id, int messageId, string key, 
      Telegram.Bot.Types.ReplyMarkups.IReplyMarkup markup, params object[] args)
    {
      await Bot.EditMessageTextAsync(id, messageId, string.Format(GameData.Messages[key], args),
        parseMode: ParseMode.Markdown, replyMarkup: markup);
    }
    #endregion

    public static void OnClosing(object sender, EventArgs e)
    { //Closing logic
      var dataFile = XDocument.Load(GameData.xmlLocation + @"UserData.xml");
      try
      {
        var element = dataFile.Element("BlockedUsers");
        foreach (var each in Commands.BlockedPeople)
        {
          element.Add(new XElement("Id", each));
        }
      }
      catch { }

      var instances = new XDocument(new XElement("Instances"));

      try
      {
        foreach(var each in Commands.GameInstances.Values)
        {
          var element = new XElement("Instance");
          element.Add(new XElement("CurrentGroup", each.CurrentGroup));
          element.Add(new XElement("Name", each.GroupName));
          element.Add(each.settings.ToXElement());
          instances.Root.Add(element);
        }
      }
      catch { }

      dataFile.Save(GameData.xmlLocation + @"UserData.xml");
      instances.Save(GameData.xmlLocation + "InstanceData.xml");
    }
  }
}