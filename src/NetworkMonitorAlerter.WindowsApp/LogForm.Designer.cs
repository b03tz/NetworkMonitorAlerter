using System.ComponentModel;

namespace NetworkMonitorAlerter.WindowsApp
{
    partial class LogForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogForm));
            this.listLogViewer = new System.Windows.Forms.ListView();
            this.buttonDaily = new System.Windows.Forms.Button();
            this.buttonWeekly = new System.Windows.Forms.Button();
            this.buttonMonthly = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listLogViewer
            // 
            this.listLogViewer.Location = new System.Drawing.Point(12, 43);
            this.listLogViewer.Name = "listLogViewer";
            this.listLogViewer.Size = new System.Drawing.Size(776, 395);
            this.listLogViewer.TabIndex = 0;
            this.listLogViewer.UseCompatibleStateImageBehavior = false;
            // 
            // buttonDaily
            // 
            this.buttonDaily.Location = new System.Drawing.Point(12, 12);
            this.buttonDaily.Name = "buttonDaily";
            this.buttonDaily.Size = new System.Drawing.Size(115, 23);
            this.buttonDaily.TabIndex = 1;
            this.buttonDaily.Text = "Daily";
            this.buttonDaily.UseVisualStyleBackColor = true;
            this.buttonDaily.Click += new System.EventHandler(this.buttonDaily_Click);
            // 
            // buttonWeekly
            // 
            this.buttonWeekly.Location = new System.Drawing.Point(133, 12);
            this.buttonWeekly.Name = "buttonWeekly";
            this.buttonWeekly.Size = new System.Drawing.Size(115, 23);
            this.buttonWeekly.TabIndex = 2;
            this.buttonWeekly.Text = "Weekly";
            this.buttonWeekly.UseVisualStyleBackColor = true;
            this.buttonWeekly.Click += new System.EventHandler(this.buttonWeekly_Click);
            // 
            // buttonMonthly
            // 
            this.buttonMonthly.Location = new System.Drawing.Point(254, 12);
            this.buttonMonthly.Name = "buttonMonthly";
            this.buttonMonthly.Size = new System.Drawing.Size(115, 23);
            this.buttonMonthly.TabIndex = 3;
            this.buttonMonthly.Text = "Monthly";
            this.buttonMonthly.UseVisualStyleBackColor = true;
            this.buttonMonthly.Click += new System.EventHandler(this.buttonMonthly_Click);
            // 
            // LogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.buttonMonthly);
            this.Controls.Add(this.buttonWeekly);
            this.Controls.Add(this.buttonDaily);
            this.Controls.Add(this.listLogViewer);
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(15, 15);
            this.Name = "LogForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LogForm_FormClosing);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button buttonDaily;
        private System.Windows.Forms.Button buttonWeekly;
        private System.Windows.Forms.Button buttonMonthly;

        private System.Windows.Forms.ListView listLogViewer;

        #endregion
    }
}