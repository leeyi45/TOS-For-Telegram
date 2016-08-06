using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

//This contains all the console window information

namespace QuizBot
{
	class LogForm : Form
	{
		public LogForm()
		{
			InitializeComponent();
		}

		private Dictionary<int, string> pastCommands = new Dictionary<int, string>();
		private int selectedCommand = 0;
		private TextBox logBox;
		private TextBox commandBox;
		private Label label2;
		private GroupBox statBox;
		private Button StartButton;
		private Button StopButton;
		private Button CloseButton;
		private Label label3;
		private Label StatusLabel;
		private Label label1;

		private void InitializeComponent()
		{
			this.logBox = new System.Windows.Forms.TextBox();
			this.commandBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.statBox = new System.Windows.Forms.GroupBox();
			this.StatusLabel = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.StartButton = new System.Windows.Forms.Button();
			this.StopButton = new System.Windows.Forms.Button();
			this.CloseButton = new System.Windows.Forms.Button();
			this.statBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// logBox
			// 
			this.logBox.Location = new System.Drawing.Point(181, 33);
			this.logBox.Multiline = true;
			this.logBox.Name = "logBox";
			this.logBox.Size = new System.Drawing.Size(340, 261);
			this.logBox.TabIndex = 1;
			this.logBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CancelKey);
			// 
			// commandBox
			// 
			this.commandBox.Location = new System.Drawing.Point(181, 329);
			this.commandBox.Name = "commandBox";
			this.commandBox.Size = new System.Drawing.Size(340, 20);
			this.commandBox.TabIndex = 0;
			this.commandBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxPress);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(178, 313);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(57, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Command:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(181, 13);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(25, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Log";
			// 
			// statBox
			// 
			this.statBox.Controls.Add(this.StatusLabel);
			this.statBox.Controls.Add(this.label3);
			this.statBox.Location = new System.Drawing.Point(12, 13);
			this.statBox.Name = "statBox";
			this.statBox.Size = new System.Drawing.Size(162, 249);
			this.statBox.TabIndex = 4;
			this.statBox.TabStop = false;
			this.statBox.Text = "Stats";
			// 
			// StatusLabel
			// 
			this.StatusLabel.AutoSize = true;
			this.StatusLabel.ForeColor = System.Drawing.Color.Red;
			this.StatusLabel.Location = new System.Drawing.Point(54, 20);
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = new System.Drawing.Size(47, 13);
			this.StatusLabel.TabIndex = 2;
			this.StatusLabel.Text = "Stopped";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(7, 20);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(40, 13);
			this.label3.TabIndex = 1;
			this.label3.Text = "Status:";
			// 
			// StartButton
			// 
			this.StartButton.Location = new System.Drawing.Point(13, 268);
			this.StartButton.Name = "StartButton";
			this.StartButton.Size = new System.Drawing.Size(75, 23);
			this.StartButton.TabIndex = 5;
			this.StartButton.Text = "Start";
			this.StartButton.UseVisualStyleBackColor = true;
			this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
			// 
			// StopButton
			// 
			this.StopButton.Location = new System.Drawing.Point(12, 297);
			this.StopButton.Name = "StopButton";
			this.StopButton.Size = new System.Drawing.Size(75, 23);
			this.StopButton.TabIndex = 6;
			this.StopButton.Text = "Stop";
			this.StopButton.UseVisualStyleBackColor = true;
			this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Location = new System.Drawing.Point(13, 326);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = new System.Drawing.Size(75, 23);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.Text = "Close";
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// LogForm
			// 
			this.ClientSize = new System.Drawing.Size(533, 361);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.StopButton);
			this.Controls.Add(this.StartButton);
			this.Controls.Add(this.statBox);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.commandBox);
			this.Controls.Add(this.logBox);
			this.Name = "LogForm";
			this.statBox.ResumeLayout(false);
			this.statBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

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
						Game.InitializeRoles();
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
