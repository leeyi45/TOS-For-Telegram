using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

using Telegram.Bot.Types.Enums;

//Event Data for the Console
namespace QuizBot
{
  partial class LogForm : Form
  {
    public LogForm()
    {
      InitializeComponent();
      AddCommands();
      test = new System.Windows.Forms.Timer();
      test.Interval = 6000;
      test.Tick += new EventHandler(Tick);
    }

    private void CloseButton_Click(object sender, EventArgs e)
    {
      try { Program.Bot.StopReceiving(); }
      catch { }
      Application.Exit();
    }

    private void CancelKey2(object sender, KeyPressEventArgs e)
    {
      e.Handled = true;
    }

    private void CancelKey(object sender, KeyEventArgs e)
    {
      if (!e.Control) return;
      switch (e.KeyCode)
      {
        case Keys.A:
          {
            logBox.SelectAll();
            break;
          }
        case Keys.C:
          {
            try { Clipboard.SetText(logBox.SelectedText); }
            catch { }
            break;
          }
      }
    }

    private void TextBoxPress(object sender, KeyEventArgs e)
    {
      switch (e.KeyCode)
      {
        #region Escape
        case Keys.Escape:
          {
            commandBox.Clear();
            break;
          }
        #endregion
        #region Enter
        case Keys.Enter:
          {
            DebugMenu(commandBox.Text);
            if (!string.IsNullOrWhiteSpace(commandBox.Text))
            {
              pastCommands.Add(pastCommands.Count + 1, commandBox.Text);
            }
            commandBox.Clear();
            selectedCommand = 0;
            break;
          }
        #endregion
        #region Up
        case Keys.Up:
          {
            if (pastCommands.Count == 0) return;
            if (selectedCommand + 1 > pastCommands.Count)
            {
              commandBox.Clear();
              selectedCommand = 0;
            }
            else
            {
              selectedCommand += 1;
              commandBox.Text = pastCommands[selectedCommand];
            }
            break;
          }
        #endregion
        #region Down
        case Keys.Down:
          {
            if (pastCommands.Count == 0) return;
            if (selectedCommand == 1)
            {
              commandBox.Clear();
              selectedCommand = 0;
            }
            else if (selectedCommand == 0)
            {
              selectedCommand = pastCommands.Count;
              commandBox.Text = pastCommands[selectedCommand];
            }
            else
            {
              selectedCommand -= 1;
              commandBox.Text = pastCommands[selectedCommand];
            }
            break;
          }
        #endregion
        #region A
        //For some reason control a stopped working so I needed to reimplement this
        case Keys.A:
          {
            if (e.Control)
            {
              commandBox.SelectAll();
            }
            break;
          }
          #endregion
      }
    }

    public void Log(string text, bool timestamp = true)
    {
      if (timestamp) logBox.AppendText(DateTime.Now.ToString("[HH:mm:ss] ") + text);
      else logBox.AppendText(text);
    }

    public void LogLine(string text, bool timestamp = true)
    {
      Log(text + "\n", timestamp);
    }

    private void StartButton_Click(object sender, EventArgs e)
    {
      StartBot();
    }

    public void StartBot(bool yes = true)
    {
      if (Program.Bot.IsReceiving) return;
      if(yes) Log("Starting...");
      Thread.Sleep(200);
      Program.Bot.StartReceiving();
      if(yes) LogLine("Bot started receving messages", false);
      StatusLabel.Text = "Running";
      StatusLabel.ForeColor = Color.ForestGreen;
      GameData.StartTime = DateTime.Now.AddHours(-8);
    }

    public void StopBot(bool yes = true)
    {
      if (!Program.Bot.IsReceiving) return;
      if(yes) Log("Stopping... ");
      Thread.Sleep(200);
      Program.Bot.StopReceiving();
      if(yes) LogLine("Bot stopped receving messages", false);
      StatusLabel.Text = "Stopped";
      StatusLabel.ForeColor = Color.Red;
      return;
    }

    private void StopButton_Click(object sender, EventArgs e)
    {
      StopBot();
    }

    private AutoCompleteStringCollection AddSuggestStrings()
    {
      var col = new AutoCompleteStringCollection();
      col.AddRange(new string[] { "test" });
      return col;
    }

    private System.Windows.Forms.Timer test;

    #region Stuff for the console
    private Dictionary<string, ConsoleCommand> ConsoleCommands;

    private delegate string ConsoleCommand(string[] args);

    private void AddCommands()
    {
      ConsoleCommands = new Dictionary<string, ConsoleCommand>();
      foreach (var method in typeof(LogForm).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
      {
        Command attribute = method.GetCustomAttribute(typeof(Command)) as Command;
        if (attribute != null)
        {
          ConsoleCommands.Add(attribute.Trigger, (ConsoleCommand)Delegate.CreateDelegate(
            typeof(ConsoleCommand), this, method));
        }
      }
    }

    #region Console Commands
    [Command(Trigger = "say")]
    private string Say(string[] args)
    {
      string[] otherargs = new string[args.Length - 2];
      Array.Copy(args, 2, otherargs, 0, args.Length - 2);
      try
      {
        Program.Bot.SendTextMessageAsync(Chats.chats[args[1].ToLower()], string.Join(" ", otherargs));
        return "Message sent";
      }
      catch (KeyNotFoundException) { return "Unknown chat: " + args[1]; }
    }

    [Command(Trigger = "parseSay")]
    private string parseSay(string[] args)
    {
      string[] otherargs = new string[args.Length - 3];
      Array.Copy(args, 3, otherargs, 0, args.Length - 3);
      try
      {
        ParseMode mode;
        if (!Enum.TryParse(args[2], out mode)) mode = ParseMode.Html;

        Program.Bot.SendTextMessageAsync(Chats.chats[args[1].ToLower()],
           string.Join(" ", otherargs), parseMode: mode);
        return "Message sent";
      }
      catch (KeyNotFoundException) { return "Unknown chat: " + args[1]; }
    }

    [Command(Trigger = "start")]
    private string Start(string[] args) { StartBot(false); return "Bot started receiving messages"; }

    [Command(Trigger = "stop")]
    private string Stop(string[] args) { StopBot(false); return "Bot stopped receiving messages"; }

    [Command(Trigger = "reload")]
    private string Reload(string[] args)
    {
      if(args.Length == 1)
      {
        return "roles - To reload roles.xml\nmsgs - To reload Messages.xml";
      }
      switch (args[1])
      {
        case "roles": { GameData.InitializeRoles(); break; }
        case "messages": { goto case "msgs"; }
        case "msgs": { GameData.InitializeMessages(); break; }
        default: { throw new ArgumentException(args[1]); }
      }
      return args[1] + " reloaded";
    }

    [Command(Trigger = "timestart")]
    private string TimeStart(string[] args)
    {
      test.Start();
      return "Timer started";
    }

    [Command(Trigger = "timestop")]
    private string TimeStop(string[] args)
    {
      test.Stop();
      return "Timer stopped";
    }

    private void Tick(object sender, EventArgs e)
    {
      Program.Bot.SendTextMessageAsync(Chats.chats["wfp"], "Timer has ticked!");
    }
    #endregion

    public void DebugMenu(string input)
    {
      if (string.IsNullOrWhiteSpace(input)) return;
      input.ToLower();
      string[] args = input.Split(' ');
      try
      {
        LogLine(ConsoleCommands[args[0]](args));
      }
      catch (KeyNotFoundException) { LogLine("Unknown command: " + args[0]); }
      catch (ArgumentException e) { LogLine("Unknown argument: " + e); }
    }
    #endregion
  }
}
