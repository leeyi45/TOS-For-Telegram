using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using System.Linq;

namespace QuizBot
{
  public partial class Startup : Form
  {
    public Startup()
    {
      InitializeComponent();
      ConsoleForm = new LogForm(this);
      worker = new BackgroundWorker();
      worker.DoWork += new DoWorkEventHandler(Loading);
      worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnLoadFinish);
    }

    private void SetLabelText(string text, bool loading = true)
    {
      if (InfoLabel.InvokeRequired)
      {
        InfoLabel.Invoke(new Action(() =>
        {
          if (loading) InfoLabel.Text = "Loading " + text;
          else InfoLabel.Text = text;
        }));
      }
      else
      {
        if (loading) InfoLabel.Text = "Loading " + text;
        else InfoLabel.Text = text;
      }
    }

    public void SetExtraInfo(string text)
    {
      ExtraInfo.Invoke(new Action(() => { ExtraInfo.Text = text; }));
    }

    public void SetExtraInfo(string text, int amount)
    {
      ExtraInfo.Invoke(new Action(() => { ExtraInfo.Text = text; }));
      progressBar1.Invoke(new Action(() => 
      {
        progressBar1.Step = amount;
        progressBar1.PerformStep();
      }));
    }

    private BackgroundWorker worker;

    private void Loading(object sender, DoWorkEventArgs e)
    {
      var ToLoad = typeof(StartupLoaders).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).
  ToDictionary(x => x.Name, x => (Action)Delegate.CreateDelegate(typeof(Action), x));
      Invoke(new Action(() => progressBar1.Step = 100 / ToLoad.Count));

      foreach (var each in ToLoad)
      {
        SetLabelText(each.Key);
        retry:
        try { each.Value(); }
        catch (InitException ex)
        {
          switch (GameData.ErrorShow(ex.Message))
          {
            case DialogResult.Retry: { goto retry; }
            case DialogResult.Abort: { Application.Exit(); break; }
            case DialogResult.Ignore: { continue; }
          }
        }
        progressBar1.Invoke(new Action(progressBar1.PerformStep));
      }
    }

    private void OnLoadFinish(object sender, RunWorkerCompletedEventArgs e)
    {
      try
      {
        if (e.Error != null) throw e.Error;
        progressBar1.Value = 100;
        ConsoleForm.Show();
      }
      finally
      {
        Program.OnClosing(null, null);
      }
    }

    private void OnShow(object sender, EventArgs e)
    {
      Activate();
      worker.RunWorkerAsync();
    }

    public LogForm ConsoleForm { get; set; }

    private void CancelButton_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private void OnEnter(object sender, EventArgs e)
    {
      ActiveControl = null;
    }
  }

  static class StartupLoaders
  {
    private static void Bot() { Program.TryToBot(false); }

    private static void Files() { GDExtensions.LoadXmlFiles(); }

    //private static void Protocols() { GameData.InitializeProtocols(false); }

    private static void Others() { GameData.InitializeOthers(false); }

    private static void Parsers()
    {
      try
      {
        if (!CommandVars.protocolsLoaded) throw new KeyNotFoundException();

        Program.Parsers = new Dictionary<string, Action<Callback>>();
        Program.Parsers.Add(GameData.Protocols["ConfigOptions"], new Action<Callback>(QuizBot.Config.Parse));
        Program.Parsers.Add(GameData.Protocols["SelectedConfigOption"], new Action<Callback>(QuizBot.Config.ChangeParse));
        //Program.Parsers.Add(GameData.Protocols["NightActions"], new Action<Callback>(Game.ParseNightAction));
        //Program.Parsers.Add(GameData.Protocols["Vote"], new Action<Callback>(Game.ParseVoteChoice));
      }
      catch(KeyNotFoundException)
      {
        MessageBox.Show("Protocols.xml was not loaded, skipping the loading of parsers", "Warning",
  MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
    }

    private static void Roles() { GameData.InitializeRoles(false); }

   // private static void Messages() { GameData.InitializeMessages(false); }

    private static void Commands() { QuizBot.Commands.InitializeCommands(); }

    private static void Config() { QuizBot.Config.Load(); }

    private static void Chats() { QuizBot.Chats.getChats(); }

    private static void Settings() { QuizBot.Settings.LoadProperties(); }

    private static void Instances() { Game.LoadInstances(); }
  }
}
