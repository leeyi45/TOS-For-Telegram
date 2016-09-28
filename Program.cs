using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Text;
using System.Diagnostics;
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

    public static bool RestartRequired = true;

    [STAThread]
		static void Main(string[] notused)
    {
      Application.ApplicationExit += OnClosing;
      Application.EnableVisualStyles();
      //Even if there's an error it should pick right back up and restart itself
      while (true)
      {
        try
        {
          if (RestartRequired)
          {
            MessageCount = new Dictionary<int, Telegram.Bot.Types.Message>();
            GameData.StartTime = DateTime.UtcNow;
            RestartRequired = false;
            startup = new Startup();
            Application.Run(startup);
          }
          if (!Application.MessageLoop) break;
        }
        catch (Exception e) { LogError(e); RestartRequired = true; }
        finally { OnClosing(null, null); }
      }
		}

    public static Dictionary<string, Action<Callback>> Parsers { get; set; }

    static Dictionary<int, Telegram.Bot.Types.Message> MessageCount;

    static void OnMessage(object sender, MessageEventArgs messageEventArgs)
		{
      try
      {
        var message = messageEventArgs.Message;
        var msgtext = message.Text;

        #region Spam Filters
        //Messages received while not receiving are ignored
        if (message.Date.Ticks < GameData.StartTime.Ticks) return;

        //Messages that aren't text are ignored
        if (message == null || message.Type != MessageType.TextMessage) return;

        //Messages that have nothing are ignored
        if (string.IsNullOrWhiteSpace(message.Text)) return;

        //Users that exceed the limit are ignored
        if (MessageCount.Keys.Contains(message.From.Id))
        {
          if ((message.Date - MessageCount[message.From.Id].Date).TotalMilliseconds <
            Settings.MaxMessage) return;
          else MessageCount[message.From.Id] = message;
        }
        else
        {
          MessageCount.Add(message.From.Id, message);
        }
        #endregion

        if (message.ForwardFrom != null && CommandVars.GetUserId)
        {
          Bot.SendTextMessageAsync(message.From.Id, Commands.ProcessUserId(message));
        }

        if (Commands.BlockedPeople?.Contains(message.From.Id.ToString()) ?? false) return;

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
        if (message.Entities.Count > 0)
        {
          if (message.Entities.First().Type == MessageEntityType.BotCommand)
          {
            //Send to the command processor
            Commands.Parse(message);
          }
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
        catch (Exception) when (!logtoconsole)
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
            case DialogResult.Retry: { break; }
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
      try
      {
        startup.ConsoleForm?.Invoke(new Action(() => startup.ConsoleForm.LogLine(text)));
        //startup.ConsoleForm.LogLine(text);
      }
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

    public static int[] GenerateInts(int count, int min, int max, bool replace = false)
    {
      var random = new Random();
      int[] output = new int[count];
      for(int i = 0; i < output.Length; i++) { output[i] = -1; }
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
    /// Send a message to the specified group using a message with specified key
    /// </summary>
    /// <param name="id">Group id</param>
    /// <param name="key">Message Key</param>
    /// <param name="args">Message Arguments</param>
    public static void BotMessage(long id, string key, params object[] args)
    {
      try { BotNormalMessage(id, string.Format(GameData.Messages[key], args)); }
      catch(KeyNotFoundException)
      {
        ConsoleLog("Unknown message key : " + key);
      }
      catch (Exception) { }
    }

    public async static void BotNormalMessage(long Id, string message, bool handle = true)
    {
      try
      {
        await Bot.SendTextMessageAsync(Id, message, parseMode: ParseMode.Html);
      }
      catch (Telegram.Bot.Exceptions.ApiRequestException) when (handle) { }
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
      try
      {
        var dataFile = GDExtensions.SafeLoad(Files.UserData);
        var element = dataFile.Element("BlockedUsers");
        foreach (var each in Commands.BlockedPeople)
        {
          element.Add(new XElement("Id", each));
        }
        dataFile.SafeSave(Files.UserData);
      }
      catch { }

      if (Commands.GameInstances.Count > 0)
      {
        try
        {
          var instances = new XDocument(new XElement("Instances"));
          foreach (var each in Commands.GameInstances)
          {
            var element = new XElement("Instance");
            element.Add(new XElement("CurrentGroup", each.CurrentGroup));
            element.Add(new XElement("Name", each.GroupName));
            element.Add(new XElement("PrivateID", each.PrivateID));
            element.Add(each.settings.ToXElement());
            instances.Root.Add(element);
          }
          instances.SafeSave(Files.InstanceData);
          //instances.Save(GameData.xmlLocation + "InstanceData.xml");
        }
        catch { }
      }
    }

    public static void LogError(Exception e)
    {
      var output = new StringBuilder("[" + DateTime.Now.ToString() + "]\n");
      var frames = new StackTrace(e, true).GetFrames().Reverse().ToArray();

      output.AppendLine(e.GetType().FullName + " caught inside " + 
        frames[0].GetMethod().Name + "() at Line " +
        frames[0].GetFileLineNumber());
      for(int i = 1; i < frames.Length; i++)
      {
        output.AppendLine("  through " + frames[i].GetMethod().Name + "() at Line " + 
          frames[i].GetFileLineNumber());
      }
      output.AppendLine();
      System.IO.File.AppendAllText("Debug.txt", output.ToString());
    }
  }
}