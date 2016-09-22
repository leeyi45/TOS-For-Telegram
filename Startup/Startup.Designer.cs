using System;
using System.Windows.Forms;

namespace QuizBot
{
  partial class Startup
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
    private ProgressBar progressBar1;
    private Label InfoLabel;
    private Label ExtraInfo;
    private RichTextBox richTextBox1;
    private new Button CancelButton;

    private void InitializeComponent()
    {
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.InfoLabel = new System.Windows.Forms.Label();
      this.CancelButton = new System.Windows.Forms.Button();
      this.ExtraInfo = new System.Windows.Forms.Label();
      this.richTextBox1 = new System.Windows.Forms.RichTextBox();
      this.SuspendLayout();
      // 
      // progressBar1
      // 
      this.progressBar1.Location = new System.Drawing.Point(25, 102);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(410, 23);
      this.progressBar1.TabIndex = 0;
      // 
      // InfoLabel
      // 
      this.InfoLabel.AutoSize = true;
      this.InfoLabel.Font = new System.Drawing.Font("Amatic", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.InfoLabel.Location = new System.Drawing.Point(22, 61);
      this.InfoLabel.Name = "InfoLabel";
      this.InfoLabel.Size = new System.Drawing.Size(40, 16);
      this.InfoLabel.TabIndex = 1;
      this.InfoLabel.Text = "Loading";
      // 
      // CancelButton
      // 
      this.CancelButton.Location = new System.Drawing.Point(25, 156);
      this.CancelButton.Name = "CancelButton";
      this.CancelButton.Size = new System.Drawing.Size(75, 23);
      this.CancelButton.TabIndex = 2;
      this.CancelButton.Text = "Cancel";
      this.CancelButton.UseVisualStyleBackColor = true;
      this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
      // 
      // ExtraInfo
      // 
      this.ExtraInfo.AutoSize = true;
      this.ExtraInfo.Font = new System.Drawing.Font("Amatic", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.ExtraInfo.Location = new System.Drawing.Point(22, 77);
      this.ExtraInfo.Name = "ExtraInfo";
      this.ExtraInfo.Size = new System.Drawing.Size(40, 16);
      this.ExtraInfo.TabIndex = 3;
      this.ExtraInfo.Text = "Loading";
      // 
      // richTextBox1
      // 
      this.richTextBox1.BackColor = System.Drawing.Color.White;
      this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.richTextBox1.Font = new System.Drawing.Font("Amatic", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.richTextBox1.Location = new System.Drawing.Point(99, 24);
      this.richTextBox1.Name = "richTextBox1";
      this.richTextBox1.ReadOnly = true;
      this.richTextBox1.Size = new System.Drawing.Size(273, 36);
      this.richTextBox1.TabIndex = 4;
      this.richTextBox1.Text = "Quizbot - Town of Salem for Telegram";
      this.richTextBox1.Enter += new EventHandler(OnEnter);
      // 
      // Startup
      // 
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(465, 200);
      this.Controls.Add(this.richTextBox1);
      this.Controls.Add(this.ExtraInfo);
      this.Controls.Add(this.CancelButton);
      this.Controls.Add(this.InfoLabel);
      this.Controls.Add(this.progressBar1);
      this.Name = "Startup";
      this.Text = "Loading";
      this.Shown += new System.EventHandler(this.OnShow);
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    #endregion
  }
}