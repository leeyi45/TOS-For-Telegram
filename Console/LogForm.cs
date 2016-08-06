using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


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
			Program.Bot.StopReceiving();
			Application.Exit();
		}

		public void CancelKey(object sender, KeyPressEventArgs e)
		{
			e.Handled = true;
		}

		public void TextBoxPress(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				#region Delete
				case Keys.Delete:
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
			}
		}

		public void Log(string text)
		{
			logBox.AppendText(text);
		}

		public void LogLine(string text)
		{
			logBox.AppendText(text + "\n");
		}

		public void StartBot()
		{
			if (Program.Bot.IsReceiving) return;
			Log("Starting...");
			Thread.Sleep(600);
			Program.Bot.StartReceiving();
			LogLine("Bot started receving messages");
			StatusLabel.Text = "Running";
			StatusLabel.ForeColor = Color.ForestGreen;
		}

		public void StopBot()
		{
			if (!Program.Bot.IsReceiving) return;
			Log("Stopping... ");
			Thread.Sleep(600);
			Program.Bot.StopReceiving();
			LogLine("Bot stopped receving messages");
			StatusLabel.Text = "Stopped";
			StatusLabel.ForeColor = Color.Red;
			return;
		}

		public async void DebugMenu(string input)
		{
			if (input == null) return;
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
						break;
					}
				case "xml":
					{
						GameData.InitializeRoles();
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
	}
}
