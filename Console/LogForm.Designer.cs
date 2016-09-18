using System.Windows.Forms;
using System;

namespace QuizBot
{
  partial class LogForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
		private void InitializeComponent()
    {
      this.logBox = new System.Windows.Forms.TextBox();
      this.commandBox = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.statBox = new System.Windows.Forms.GroupBox();
      this.protocolStatus = new QuizBot.LogForm.StatusLabel();
      this.label7 = new System.Windows.Forms.Label();
      this.messageStatus = new QuizBot.LogForm.StatusLabel();
      this.label6 = new System.Windows.Forms.Label();
      this.roleStatus = new QuizBot.LogForm.StatusLabel();
      this.label5 = new System.Windows.Forms.Label();
      this.connectLabel = new QuizBot.LogForm.StatusLabel();
      this.label4 = new System.Windows.Forms.Label();
      this.stateLabel = new QuizBot.LogForm.StatusLabel();
      this.label3 = new System.Windows.Forms.Label();
      this.StartButton = new System.Windows.Forms.Button();
      this.StopButton = new System.Windows.Forms.Button();
      this.CloseButton = new System.Windows.Forms.Button();
      this.richTextBox1 = new System.Windows.Forms.RichTextBox();
      this.ReloadBotButton = new System.Windows.Forms.Button();
      this.statBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // logBox
      // 
      this.logBox.BackColor = System.Drawing.Color.White;
      this.logBox.Location = new System.Drawing.Point(181, 76);
      this.logBox.Multiline = true;
      this.logBox.Name = "logBox";
      this.logBox.ReadOnly = true;
      this.logBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.logBox.Size = new System.Drawing.Size(340, 261);
      this.logBox.TabIndex = 1;
      this.logBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CancelKey);
      this.logBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CancelKey2);
      // 
      // commandBox
      // 
      this.commandBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
      this.commandBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
      this.commandBox.Location = new System.Drawing.Point(181, 375);
      this.commandBox.Name = "commandBox";
      this.commandBox.Size = new System.Drawing.Size(340, 20);
      this.commandBox.TabIndex = 0;
      this.commandBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxPress);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(181, 353);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(57, 13);
      this.label1.TabIndex = 2;
      this.label1.Text = "Command:";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(180, 58);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(25, 13);
      this.label2.TabIndex = 3;
      this.label2.Text = "Log";
      // 
      // statBox
      // 
      this.statBox.Controls.Add(this.protocolStatus);
      this.statBox.Controls.Add(this.label7);
      this.statBox.Controls.Add(this.messageStatus);
      this.statBox.Controls.Add(this.label6);
      this.statBox.Controls.Add(this.roleStatus);
      this.statBox.Controls.Add(this.label5);
      this.statBox.Controls.Add(this.connectLabel);
      this.statBox.Controls.Add(this.label4);
      this.statBox.Controls.Add(this.stateLabel);
      this.statBox.Controls.Add(this.label3);
      this.statBox.Location = new System.Drawing.Point(12, 13);
      this.statBox.Name = "statBox";
      this.statBox.Size = new System.Drawing.Size(162, 295);
      this.statBox.TabIndex = 4;
      this.statBox.TabStop = false;
      this.statBox.Text = "Stats";
      // 
      // protocolStatus
      // 
      this.protocolStatus.AutoSize = true;
      this.protocolStatus.FalseStateText = "Not Loaded";
      this.protocolStatus.ForeColor = System.Drawing.Color.Red;
      this.protocolStatus.Location = new System.Drawing.Point(77, 89);
      this.protocolStatus.Name = "protocolStatus";
      this.protocolStatus.Size = new System.Drawing.Size(63, 13);
      this.protocolStatus.TabIndex = 10;
      this.protocolStatus.Text = "Not Loaded";
      this.protocolStatus.TrueStateText = "Loaded";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(7, 89);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(54, 13);
      this.label7.TabIndex = 9;
      this.label7.Text = "Protocols:";
      // 
      // messageStatus
      // 
      this.messageStatus.AutoSize = true;
      this.messageStatus.FalseStateText = "Not Loaded";
      this.messageStatus.ForeColor = System.Drawing.Color.Red;
      this.messageStatus.Location = new System.Drawing.Point(77, 72);
      this.messageStatus.Name = "messageStatus";
      this.messageStatus.Size = new System.Drawing.Size(63, 13);
      this.messageStatus.TabIndex = 8;
      this.messageStatus.Text = "Not Loaded";
      this.messageStatus.TrueStateText = "Loaded";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(7, 72);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(58, 13);
      this.label6.TabIndex = 7;
      this.label6.Text = "Messages:";
      // 
      // roleStatus
      // 
      this.roleStatus.AutoSize = true;
      this.roleStatus.FalseStateText = "Not Loaded";
      this.roleStatus.ForeColor = System.Drawing.Color.Red;
      this.roleStatus.Location = new System.Drawing.Point(77, 55);
      this.roleStatus.Name = "roleStatus";
      this.roleStatus.Size = new System.Drawing.Size(63, 13);
      this.roleStatus.TabIndex = 6;
      this.roleStatus.Text = "Not Loaded";
      this.roleStatus.TrueStateText = "Loaded";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(7, 55);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(37, 13);
      this.label5.TabIndex = 5;
      this.label5.Text = "Roles:";
      // 
      // connectLabel
      // 
      this.connectLabel.AutoSize = true;
      this.connectLabel.FalseStateText = "Disconnected";
      this.connectLabel.ForeColor = System.Drawing.Color.Red;
      this.connectLabel.Location = new System.Drawing.Point(77, 38);
      this.connectLabel.Name = "connectLabel";
      this.connectLabel.Size = new System.Drawing.Size(73, 13);
      this.connectLabel.TabIndex = 4;
      this.connectLabel.Text = "Disconnected";
      this.connectLabel.TrueStateText = "Connected";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(7, 38);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(64, 13);
      this.label4.TabIndex = 3;
      this.label4.Text = "Connection:";
      // 
      // stateLabel
      // 
      this.stateLabel.AutoSize = true;
      this.stateLabel.FalseStateText = "Stopped";
      this.stateLabel.ForeColor = System.Drawing.Color.Red;
      this.stateLabel.Location = new System.Drawing.Point(77, 21);
      this.stateLabel.Name = "stateLabel";
      this.stateLabel.Size = new System.Drawing.Size(47, 13);
      this.stateLabel.TabIndex = 2;
      this.stateLabel.Text = "Stopped";
      this.stateLabel.TrueStateText = "Running";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(7, 21);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(40, 13);
      this.label3.TabIndex = 1;
      this.label3.Text = "Status:";
      // 
      // StartButton
      // 
      this.StartButton.Location = new System.Drawing.Point(12, 314);
      this.StartButton.Name = "StartButton";
      this.StartButton.Size = new System.Drawing.Size(75, 23);
      this.StartButton.TabIndex = 5;
      this.StartButton.Text = "Start";
      this.StartButton.UseVisualStyleBackColor = true;
      this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
      // 
      // StopButton
      // 
      this.StopButton.Location = new System.Drawing.Point(12, 343);
      this.StopButton.Name = "StopButton";
      this.StopButton.Size = new System.Drawing.Size(75, 23);
      this.StopButton.TabIndex = 6;
      this.StopButton.Text = "Stop";
      this.StopButton.UseVisualStyleBackColor = true;
      this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
      // 
      // CloseButton
      // 
      this.CloseButton.Location = new System.Drawing.Point(12, 372);
      this.CloseButton.Name = "CloseButton";
      this.CloseButton.Size = new System.Drawing.Size(75, 23);
      this.CloseButton.TabIndex = 7;
      this.CloseButton.Text = "Close";
      this.CloseButton.UseVisualStyleBackColor = true;
      this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
      // 
      // richTextBox1
      // 
      this.richTextBox1.BackColor = System.Drawing.Color.White;
      this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.richTextBox1.Font = new System.Drawing.Font("Amatic", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.richTextBox1.Location = new System.Drawing.Point(212, 15);
      this.richTextBox1.Name = "richTextBox1";
      this.richTextBox1.ReadOnly = true;
      this.richTextBox1.Size = new System.Drawing.Size(273, 36);
      this.richTextBox1.TabIndex = 8;
      this.richTextBox1.TabStop = false;
      this.richTextBox1.Text = "Quizbot - Town of Salem for Telegram";
      this.richTextBox1.Enter += new System.EventHandler(this.OnEnter);
      // 
      // ReloadBotButton
      // 
      this.ReloadBotButton.Location = new System.Drawing.Point(93, 314);
      this.ReloadBotButton.Name = "ReloadBotButton";
      this.ReloadBotButton.Size = new System.Drawing.Size(75, 23);
      this.ReloadBotButton.TabIndex = 9;
      this.ReloadBotButton.Text = "Reload Bot";
      this.ReloadBotButton.UseVisualStyleBackColor = true;
      this.ReloadBotButton.Click += new System.EventHandler(this.ReloadBotButton_Click);
      // 
      // LogForm
      // 
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(533, 407);
      this.Controls.Add(this.ReloadBotButton);
      this.Controls.Add(this.richTextBox1);
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

    private System.Collections.Generic.Dictionary<int, string> pastCommands;
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
    private StatusLabel stateLabel;
    private Label label1;
    private RichTextBox richTextBox1;
    private StatusLabel connectLabel;
    private Label label4;
    private StatusLabel roleStatus;
    private Label label5;
    private StatusLabel protocolStatus;
    private Label label7;
    private StatusLabel messageStatus;
    private Label label6;
    #endregion

    private Button ReloadBotButton;
  }
}