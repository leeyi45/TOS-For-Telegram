using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuizBot
{
  partial class Startup : Form
  {
    private ProgressBar progressBar1;
    private Label InfoLabel;
    private Label ExtraInfo;
    private Button CancelButton;

    private void InitializeComponent()
    {
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.InfoLabel = new System.Windows.Forms.Label();
      this.CancelButton = new System.Windows.Forms.Button();
      this.ExtraInfo = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // progressBar1
      // 
      this.progressBar1.Location = new System.Drawing.Point(25, 64);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(410, 23);
      this.progressBar1.TabIndex = 0;
      // 
      // InfoLabel
      // 
      this.InfoLabel.AutoSize = true;
      this.InfoLabel.Location = new System.Drawing.Point(22, 32);
      this.InfoLabel.Name = "InfoLabel";
      this.InfoLabel.Size = new System.Drawing.Size(45, 13);
      this.InfoLabel.TabIndex = 1;
      this.InfoLabel.Text = "Loading";
      // 
      // CancelButton
      // 
      this.CancelButton.Location = new System.Drawing.Point(25, 114);
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
      this.ExtraInfo.Location = new System.Drawing.Point(22, 48);
      this.ExtraInfo.Name = "ExtraInfo";
      this.ExtraInfo.Size = new System.Drawing.Size(45, 13);
      this.ExtraInfo.TabIndex = 3;
      this.ExtraInfo.Text = "Loading";
      // 
      // Startup
      // 
      this.ClientSize = new System.Drawing.Size(465, 200);
      this.Controls.Add(this.ExtraInfo);
      this.Controls.Add(this.CancelButton);
      this.Controls.Add(this.InfoLabel);
      this.Controls.Add(this.progressBar1);
      this.Name = "Startup";
      this.Text = "Loading";
      this.Shown += new System.EventHandler(this.DoTheLoading);
      this.ResumeLayout(false);
      this.PerformLayout();

    }
  }
}
