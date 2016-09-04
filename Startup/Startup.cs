using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;

namespace QuizBot
{
  partial class Startup
  {
    public Startup()
    {
      InitializeComponent();
      LoadLoaders();
      progressBar1.Step = 100 / ToLoad.Count;
      worker = new BackgroundWorker();
      worker.DoWork += new DoWorkEventHandler(Loading);
      worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnLoadFinish);
      //DoTheLoading();
    }

    private void SetLabelText(string text, bool loading = true)
    {
      InfoLabel.Invoke(new Action(() =>
      {
        if (loading) InfoLabel.Text = "Loading " + text;
        else InfoLabel.Text = text;
      }));
    }

    public void SetExtraInfo(string text)
    {
      ExtraInfo.Invoke(new Action(() => { ExtraInfo.Text = text; }));
    }

    private Dictionary<string, Action> ToLoad;

    private BackgroundWorker worker;

    private void LoadLoaders()
    {
      ToLoad = new Dictionary<string, Action>();
      ToLoad.Add("Bot", new Action(Program.LoadBot));
      ToLoad.Add("Roles", new Action(GameData.InitializeRoles));
      ToLoad.Add("Messages", new Action(GameData.InitializeMessages));
      ToLoad.Add("Commands", new Action(Commands.InitializeCommands));
      ToLoad.Add("Chats", new Action(Chats.getChats));
      ToLoad.Add("Parsers", new Action(Program.LoadParsers));
    }

    private void Loading(object sender, DoWorkEventArgs e)
    {
      foreach (var each in ToLoad)
      {
        Thread.Sleep(200);
        SetLabelText(each.Key);
        retry:
        try { each.Value(); }
        catch (InitException ex)
        {
          switch (MessageBox.Show(ex.Message, "Error",
            MessageBoxButtons.RetryCancel, MessageBoxIcon.Error))
          {
            case DialogResult.Retry:
              {
                goto retry;
              }
            case DialogResult.Cancel:
              {
                Application.Exit();
                break;
              }
          }
        }
        progressBar1.Invoke(new Action(progressBar1.PerformStep));
      }
    }

    private void OnLoadFinish(object sender, RunWorkerCompletedEventArgs e)
    {
      progressBar1.Value = 100;
      ConsoleForm = new LogForm(this);
      ConsoleForm.Show();
    }

    private void DoTheLoading(object sender, EventArgs e)
    {
      worker.RunWorkerAsync();
    }

    public LogForm ConsoleForm { get; set; }

    private void CancelButton_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }
  }
}
