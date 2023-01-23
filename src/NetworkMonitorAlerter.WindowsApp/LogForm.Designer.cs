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
            this.tabLogView = new System.Windows.Forms.TabControl();
            this.tabDaily = new System.Windows.Forms.TabPage();
            this.tabWeekly = new System.Windows.Forms.TabPage();
            this.tabMonthly = new System.Windows.Forms.TabPage();
            this.tabLogView.SuspendLayout();
            this.tabDaily.SuspendLayout();
            this.SuspendLayout();
            // 
            // listLogViewer
            // 
            this.listLogViewer.Location = new System.Drawing.Point(0, 0);
            this.listLogViewer.Name = "listLogViewer";
            this.listLogViewer.Size = new System.Drawing.Size(769, 400);
            this.listLogViewer.TabIndex = 0;
            this.listLogViewer.UseCompatibleStateImageBehavior = false;
            // 
            // tabLogView
            // 
            this.tabLogView.Controls.Add(this.tabDaily);
            this.tabLogView.Controls.Add(this.tabWeekly);
            this.tabLogView.Controls.Add(this.tabMonthly);
            this.tabLogView.Location = new System.Drawing.Point(12, 12);
            this.tabLogView.Name = "tabLogView";
            this.tabLogView.SelectedIndex = 0;
            this.tabLogView.Size = new System.Drawing.Size(780, 426);
            this.tabLogView.TabIndex = 4;
            this.tabLogView.SelectedIndexChanged += new System.EventHandler(this.tabLogView_SelectedIndexChanged);
            // 
            // tabDaily
            // 
            this.tabDaily.Controls.Add(this.listLogViewer);
            this.tabDaily.Location = new System.Drawing.Point(4, 22);
            this.tabDaily.Name = "tabDaily";
            this.tabDaily.Padding = new System.Windows.Forms.Padding(3);
            this.tabDaily.Size = new System.Drawing.Size(772, 400);
            this.tabDaily.TabIndex = 0;
            this.tabDaily.Text = "Daily";
            this.tabDaily.UseVisualStyleBackColor = true;
            // 
            // tabWeekly
            // 
            this.tabWeekly.Location = new System.Drawing.Point(4, 22);
            this.tabWeekly.Name = "tabWeekly";
            this.tabWeekly.Padding = new System.Windows.Forms.Padding(3);
            this.tabWeekly.Size = new System.Drawing.Size(772, 400);
            this.tabWeekly.TabIndex = 1;
            this.tabWeekly.Text = "Weekly";
            this.tabWeekly.UseVisualStyleBackColor = true;
            // 
            // tabMonthly
            // 
            this.tabMonthly.Location = new System.Drawing.Point(4, 22);
            this.tabMonthly.Name = "tabMonthly";
            this.tabMonthly.Size = new System.Drawing.Size(772, 400);
            this.tabMonthly.TabIndex = 2;
            this.tabMonthly.Text = "Monthly";
            this.tabMonthly.UseVisualStyleBackColor = true;
            // 
            // LogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabLogView);
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(15, 15);
            this.Name = "LogForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LogForm_FormClosing);
            this.tabLogView.ResumeLayout(false);
            this.tabDaily.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TabPage tabDaily;

        private System.Windows.Forms.TabControl tabLogView;
        private System.Windows.Forms.TabPage tabMonthly;
        private System.Windows.Forms.TabPage tabWeekly;

        private System.Windows.Forms.ListView listLogViewer;

        #endregion
    }
}