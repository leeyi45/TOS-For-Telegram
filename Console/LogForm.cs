﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;

using Telegram.Bot.Types.Enums;

namespace QuizBot
{
  public partial class LogForm : Form
  {
    public LogForm(Startup parent)
    {
      pastCommands = new Dictionary<int, string>();
      InitializeComponent();
      AddLabels();
      test = new System.Windows.Forms.Timer();
      test.Interval = 6000;
      test.Tick += new EventHandler(Tick);
      Parent = parent;
    }

    public new Startup Parent;

    private AllCommands CommandContainer;

    #region Textbox Management
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

    private void OnEnter(object sender, EventArgs e)
    {
      ActiveControl = null;
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
            if (!string.IsNullOrWhiteSpace(commandBox.Text))
            {
              var text = commandBox.Text.Trim();
              DebugMenu(text);
              pastCommands.Add(pastCommands.Count + 1, text);
              commandBox.Clear();
              selectedCommand = 0;
            }
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
              selectedCommand++;
              commandBox.Text = pastCommands[selectedCommand];
            }
            commandBox.SelectionStart = commandBox.Text.Length;
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
              selectedCommand--;
              commandBox.Text = pastCommands[selectedCommand];
            }
            commandBox.SelectionStart = commandBox.Text.Length;
            break;
          }
        #endregion
        #region A
        //For some reason control a stopped working so I needed to reimplement this
        case Keys.A:
          {
            if (e.Control) commandBox.SelectAll();
            break;
          }
        #endregion
      }
    }
    #endregion

    #region Button Events
    private void StartButton_Click(object sender, EventArgs e)
    {
      StartBot();
    }

    private void CloseButton_Click(object sender, EventArgs e)
    {
      try { Program.Bot.StopReceiving(); }
      catch { }
      Application.Exit();
    }

    private void StopButton_Click(object sender, EventArgs e)
    {
      StopBot();
    }

    private void ReloadBotButton_Click(object sender, EventArgs e)
    {
      Program.TryToBot(true);
    }
    #endregion

    public void Log(string text, bool timestamp = true)
    {
      if (timestamp) text = DateTime.Now.ToString("[HH:mm:ss] ") + text;
      if(InvokeRequired)
      {
        logBox.Invoke(new Action<string>(logBox.AppendText), text);
      }
      else
      {
        logBox.AppendText(text);
      }
    }

    public void LogLine(string text, bool timestamp = true)
    {
      Log(text + "\n", timestamp);
    }

    public void StartBot(bool yes = true)
    {
      if (Program.Bot.IsReceiving) return;
      
      if (!CommandVars.Connected)
      {
        Program.ConsoleLog("Bot is currently not connected! Use reload bot!");
        return;
      }
      if (yes) Log("Starting...");
      Thread.Sleep(200);
      Program.Bot.StartReceiving();
      if (yes) LogLine("Bot started receving messages", false);
      SwitchLabelState("running", true);
      GameData.StartTime = DateTime.Now.AddHours(-8);
    }

    public void StopBot(bool yes = true)
    {
      if (!Program.Bot.IsReceiving) return;
      if (yes) Log("Stopping... ");
      Thread.Sleep(200);
      Program.Bot.StopReceiving();
      if (yes) LogLine("Bot stopped receving messages", false);
      SwitchLabelState("running", false);
      return;
    }

    public void SwitchLabelState(string key, bool value)
    {
      Labels[key].SetLabelInfo(value);
    }

    public void UpdateLabels()
    { //Seems stupid, but it does the trick
      CommandVars.messagesLoaded = CommandVars.messagesLoaded;
      CommandVars.protocolsLoaded = CommandVars.protocolsLoaded;
      CommandVars.Connected = CommandVars.Connected;
      CommandVars.RolesLoaded = CommandVars.RolesLoaded;
    }

    public new void Show()
    {
      Parent.Hide();
      CommandContainer = new AllCommands(this);
      commandBox.AutoCompleteCustomSource = AddSuggestStrings();
      base.Show();
    }

    private void OnFormClose(object sender, FormClosingEventArgs e)
    {
      Application.Exit();
    }

    private void OnFormShow(object sender, EventArgs e)
    {
      UpdateLabels();
      //Automatically start the bot
      StartBot(false);
    }

    #region Stuff for the console
    private AutoCompleteStringCollection AddSuggestStrings()
    {
      var col = new AutoCompleteStringCollection();
      col.AddRange(CommandContainer.ConsoleCommands.Keys.ToArray());
      foreach(var each in CommandContainer.Metadata)
      {
        col.AddRange((from data in each.Value() select each.Key.ToLower() + " " + data).ToArray());
      }
      return col;
    }

    private Dictionary<string, StatusLabel> Labels;

    private delegate string CommandDelegate(string[] args);

    private delegate string[] CommandMetadata();

    private void AddLabels()
    {
      Labels = new Dictionary<string, StatusLabel>();
      Labels.Add("protocol", protocolStatus);
      Labels.Add("message", messageStatus);
      Labels.Add("connect", connectLabel);
      Labels.Add("role", roleStatus);
      Labels.Add("running", stateLabel);
    }

    private void Tick(object sender, EventArgs e)
    {
      Program.Bot.SendTextMessageAsync(Chats.chats["wfp"], "Timer has ticked!");
    }

    public void DebugMenu(string input)
    {
      if (string.IsNullOrWhiteSpace(input)) return;
      string[] args = input.Split(' ');
      try
      {
        if(CommandContainer.ConsoleCommands.Keys.Contains(args[0]))
        {
          LogLine(CommandContainer.ConsoleCommands[args[0]](args));
        }
        else LogLine("Unknown command: " + args[0]);
      }
      catch (InvalidCommandException e) { LogLine("Unknown argument: " + e); }
    }

    private class AllCommands
    {
      public AllCommands(LogForm parent)
      {
        this.parent = parent;
        ConsoleCommands = AddCommands();
        Metadata = AddMetadata();
        //List commands
        CreateListCommand("messages", GameData.Messages.Select(x => x.Key + ": " + x.Value));
        CreateListCommand("protocols", GameData.Protocols.Select(x => x.Key + ": " + x.Value));
        CreateListCommand("instances", Commands.GameInstances
                                       .Select(each => each.GroupName + ": " + each.CurrentGroup));
        //Initialize This Last
        CreateListCommand("commands", ConsoleCommands.Keys);
      }

      private LogForm parent;

      public Dictionary<string, CommandDelegate> ConsoleCommands { get; private set; }

      public Dictionary<string, CommandMetadata> Metadata { get; private set; }

      private Dictionary<string, CommandDelegate> AddCommands()
      {
        var output = new Dictionary<string, CommandDelegate>();
        foreach (var method in typeof(AllCommands).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
        {
          var attribute = method.GetCustomAttribute<ConsoleCommand>();
          if (attribute != null && !attribute.IsMetaData)
          {
            output.Add(attribute.Trigger, (CommandDelegate)Delegate.CreateDelegate(
              typeof(CommandDelegate), this, method));
          }
        }
        return output;
      }

      private Dictionary<string, CommandMetadata> AddMetadata()
      {
        var metadata = new Dictionary<string, CommandMetadata>();
        foreach (var each in typeof(AllCommands).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
        {
          var attri = each.GetCustomAttribute<ConsoleCommand>();
          if (attri != null && attri.IsMetaData)
          {
            metadata.Add(each.Name, (CommandMetadata)Delegate.CreateDelegate(typeof(CommandMetadata), this,
          each));
          }
        }
        return metadata;
      }

      private void CreateListCommand(string text, IEnumerable<string> thing)
      {
        ConsoleCommands.Add(text, new CommandDelegate((args) =>
        {
          var output = new StringBuilder(text.ToUpperFirst() + ":");
          output.AppendLine();
          foreach (var each in thing)
          {
            output.AppendLine(each);
          }
          output.AppendLine();
          return output.ToString();
        }));
      }

      #region Commands
      [ConsoleCommand("say")]
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

      [ConsoleCommand("parseSay")]
      private string parseSay(string[] args)
      {
        string[] otherargs;
        try
        {
          ParseMode mode;
          if (!Enum.TryParse(args[2], out mode))
          {
            mode = ParseMode.Html;
            otherargs = new string[args.Length - 2];
            Array.Copy(args, 2, otherargs, 0, args.Length - 2);
          }
          else
          {
            otherargs = new string[args.Length - 3];
            Array.Copy(args, 3, otherargs, 0, args.Length - 3);
          }

          Program.Bot.SendTextMessageAsync(Chats.chats[args[1].ToLower()],
             string.Join(" ", otherargs), parseMode: mode);
          return "Message sent";
        }
        catch (KeyNotFoundException) { return "Unknown chat: " + args[1]; }
      }

      [ConsoleCommand("start")]
      private string Start(string[] args) { parent.StartBot(false); return "Bot started receiving messages"; }

      [ConsoleCommand("stop")]
      private string Stop(string[] args) { parent.StopBot(false); return "Bot stopped receiving messages"; }

      [ConsoleCommand("reload")]
      private string Reload(string[] args)
      {
        if (args.Length == 1)
        {
          return "roles - To reload roles.xml\nmsgs - To reload Messages.xml\nprotocols - To reload Protocols.xml";
        }
        switch (args[1].ToLower())
        {
          case "roles": { GameData.InitializeRoles(true); break; }
          case "messages": { goto case "msgs"; }
          case "msgs": { GameData.InitializeMessages(true); break; }
          case "protocols": { GameData.InitializeProtocols(true); break; }
          case "bot": { Program.TryToBot(true); break; }
          default: { return "Unrecognised argument " + args[1]; }
        }
        return args[1] + " reloaded";
      }

      [ConsoleCommand("timestart")]
      private string TimeStart(string[] args)
      {
        parent.test.Start();
        return "Timer started";
      }

      [ConsoleCommand("timestop")]
      private string TimeStop(string[] args)
      {
        parent.test.Stop();
        return "Timer stopped";
      }

      [ConsoleCommand("json")]
      private string JSONTest(string[] args)
      {
        return "JSon test";
      }

      [ConsoleCommand("config")]
      private string Config(string[] args)
      {
        switch (args.Length)
        {
          case 1:
            {
              var output = new StringBuilder("Config Options: \n");
              foreach (var each in Settings.AllSettings)
              {
                output.AppendLine(each.Name);
              }
              return output.ToString();
            }
          case 2:
            {
              try { return "The value of " + args[1] + " is: " + Settings.GetPropertyValue(args[1]); }
              catch (KeyNotFoundException) { return "Unrecognised argument: " + args[1]; }
            }
            /*
          case 3:
            {
              try
              {
                Settings.SetPropertyValue[args[1]].SetValue(args[2]);
                return "Set value of " + args[1] + " to " + args[2];
              }
              catch (KeyNotFoundException) { return "Unrecognised argument: " + args[1]; }
              catch (ArgumentException) { return args[2] + " is an invalid value for " + args[1]; }
            }*/
          default:
            {
              return "Only 1 to 2 argument(s) are accepted";
            }
        }
      }

      [ConsoleCommand("roles", "[role name]")]
      private string Roles(string[] args)
      {
        StringBuilder output;
        switch (args.Length)
        {
          case 1:
            {
              output = new StringBuilder("Currently Registered Roles \n\n");
              foreach (var each in GameData.Roles)
              {
                output.AppendLine(each.Value.ToString());
              }
              break;
            }
          case 2:
            {
              try
              {
                var role = GameData.Roles[args[1].ToLower()];
                output = new StringBuilder("Role Data:\n\n");
                foreach (var field in typeof(Role).GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                  output.AppendLine(field.Name + ": " + field.GetValue(role).ToString());
                }
              }
              catch (KeyNotFoundException)
              {
                output = new StringBuilder("No such role \"" + args[1] + "\" found!");
              }
              break;
            }
          default:
            {
              output = new StringBuilder("Only one or two arguments are accepted");
              break;
            }
        }
        return output.ToString();
      }

      [ConsoleCommand("clear")]
      private string Clear(string[] args)
      {
        parent.logBox.Clear();
        return "Cleared";
      }

      [ConsoleCommand("rolelist", "[role list name]")]
      private string RoleLists(string[] args)
      {
        StringBuilder output;
        switch (args.Length)
        {
          case 1:
            {
              output = new StringBuilder("Rolelists:");
              foreach (var each in GameData.RoleLists)
              {
                output.AppendLine(each.Key);
              }
              break;
            }
          case 2:
            {
              try
              {
                output = new StringBuilder("Rolelist " + args[1]);
                foreach (var each in GameData.RoleLists[args[1]])
                {
                  output.AppendLine(each.Key.Name + ": " + each.Value);
                }
              }
              catch (KeyNotFoundException)
              {
                output = new StringBuilder("No such rolelist found!");
              }
              break;
            }
          default:
            {
              output = new StringBuilder("Only 1 to 2 arguments are expected!");
              break;
            }
        }
        return output.ToString();
      }

      [ConsoleCommand("test")]
      private string Test(string[] args)
      {
        GameData.ArrangeXML();
        return "";
      }
      #endregion

      #region Metadata
      [ConsoleCommand]
      private string[] Config()
      {
        return Settings.AllSettings.Select(each => each.Name).ToArray();
      }

      [ConsoleCommand]
      private string[] Reload()
      {
        return new[] { "roles", "msgs", "protocols", "bot" };
      }

      [ConsoleCommand]
      private string[] RoleLists()
      {
        return GameData.RoleLists.Keys.ToArray();
      }

      [ConsoleCommand]
      private string[] Say()
      {
        return Chats.chats.Keys.ToArray();
      }

      [ConsoleCommand]
      private string[] parseSay()
      {
        return Chats.chats.Keys.ToArray();
      }
      #endregion

    }
    #endregion

    public class StatusLabel : Label
    {
      public StatusLabel() : base() { }

      public void SetLabelInfo(bool state)
      {
        if (InvokeRequired)
        {
          Invoke(new Action<bool>(setlabelinfo), state);
        }
        else
        {
          setlabelinfo(state);
        }
      }

      private void setlabelinfo(bool state)
      {
        if (state)
        {
          Text = TrueStateText;
          ForeColor = Color.ForestGreen;
        }
        else
        {
          Text = FalseStateText;
          ForeColor = Color.Red;
        }
      }

      public string TrueStateText { get; set; } = "Loaded";

      public string FalseStateText { get; set; } = "Not Loaded";
    }
  }
}
