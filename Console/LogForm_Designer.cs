using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace QuizBot
{
	//Contains all the design elements

	partial class LogForm
	{
		private void InitializeComponent()
		{
      this.logBox = new TextBox();
      this.commandBox = new TextBox();
      this.label1 = new Label();
      this.label2 = new Label();
      this.statBox = new GroupBox();
      this.StatusLabel = new Label();
      this.label3 = new Label();
      this.StartButton = new Button();
      this.StopButton = new Button();
      this.CloseButton = new Button();
      this.pastCommands = new Dictionary<int, string>();
      this.statBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // logBox
      // 
      this.logBox.Location = new System.Drawing.Point(181, 33);
      this.logBox.Multiline = true;
      this.logBox.Name = "logBox";
      this.logBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.logBox.Size = new System.Drawing.Size(340, 261);
      this.logBox.TabIndex = 1;
      this.logBox.KeyDown += new KeyEventHandler(this.CancelKey);
      this.logBox.KeyPress += new KeyPressEventHandler(this.CancelKey2);
      // 
      // commandBox
      // 
      this.commandBox.Location = new System.Drawing.Point(181, 329);
      this.commandBox.Name = "commandBox";
      this.commandBox.Size = new System.Drawing.Size(340, 20);
      this.commandBox.TabIndex = 0;
      this.commandBox.KeyDown += new KeyEventHandler(this.TextBoxPress);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(181, 307);
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
      this.CloseButton.Click += new EventHandler(this.CloseButton_Click);
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
      this.FormClosing += new FormClosingEventHandler(this.OnFormClose);
      this.statBox.ResumeLayout(false);
      this.statBox.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		private Dictionary<int, string> pastCommands;
		private int selectedCommand = 0;
		private TextBox logBox;
		private TextBox commandBox;
    private Timer test;
		private Label label2;
		private GroupBox statBox;
		private Button StartButton;
		private Button StopButton;
		private Button CloseButton;
		private Label label3;
		private Label StatusLabel;
		private Label label1;
	}
}
