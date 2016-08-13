using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

//Event Data for the Console
namespace QuizBot
{
	partial class LogForm : Form
	{
		public LogForm()
		{
			InitializeComponent();
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

		public void StartBot()
		{
			if (Program.Bot.IsReceiving) return;
			Log("Starting...");
			Thread.Sleep(600);
			Program.Bot.StartReceiving();
			LogLine("Bot started receving messages", false);
			StatusLabel.Text = "Running";
			StatusLabel.ForeColor = Color.ForestGreen;
		}

		public void StopBot()
		{
			if (!Program.Bot.IsReceiving) return;
			Log("Stopping... ");
			Thread.Sleep(600);
			Program.Bot.StopReceiving();
			LogLine("Bot stopped receving messages", false);
			StatusLabel.Text = "Stopped";
			StatusLabel.ForeColor = Color.Red;
			return;
		}

		public async void DebugMenu(string input)
		{
			if (string.IsNullOrWhiteSpace(input)) return;
			string[] args = input.Split(' ');
			switch (args[0])
			{
				#region StartStop
				case "stop":
					{
						StopBot();
						return;
					}
				case "start":
					{
						StartBot();
						return;
					}
				#endregion
				case "say":
					{
						await Program.Bot.SendTextMessageAsync(Chats.WFPChat, input.Substring(4, input.Length-4));
						LogLine("Message sent");
						break;
					}
				case "xml":
					{
						GameData.InitializeRoles();
						break;
					}
				case "roles":
					{
						foreach (var each in GameData.RoleLists["Default"])
						{
							Console.WriteLine(each.Key.Name);
						}
						Console.ReadLine();
						break;
					}
				case "parseSay":
					{
						await Program.Bot.SendTextMessageAsync(Chats.WFPChat, input.Substring(9, input.Length - 9), 
							parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
						LogLine("Message sent");
						break;
					}
				default:
					{
						LogLine("Unrecognised command: " + args[0]);
						break;
					}
			}
		}

		private void StartButton_Click(object sender, EventArgs e)
		{
			StartBot();
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
	}
}
