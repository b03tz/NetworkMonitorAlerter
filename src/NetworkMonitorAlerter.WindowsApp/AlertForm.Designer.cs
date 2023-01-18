using System.ComponentModel;

namespace NetworkMonitorAlerter.WindowsApp
{
    partial class AlertForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AlertForm));
            this.textBoxInfo = new System.Windows.Forms.TextBox();
            this.buttonWhitelistHour = new System.Windows.Forms.Button();
            this.buttonWhitelistDay = new System.Windows.Forms.Button();
            this.buttonWhitelistForever = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxInfo
            // 
            this.textBoxInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxInfo.Location = new System.Drawing.Point(12, 12);
            this.textBoxInfo.Multiline = true;
            this.textBoxInfo.Name = "textBoxInfo";
            this.textBoxInfo.ReadOnly = true;
            this.textBoxInfo.Size = new System.Drawing.Size(603, 32);
            this.textBoxInfo.TabIndex = 0;
            this.textBoxInfo.Text = resources.GetString("textBoxInfo.Text");
            // 
            // buttonWhitelistHour
            // 
            this.buttonWhitelistHour.BackColor = System.Drawing.Color.Gold;
            this.buttonWhitelistHour.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.buttonWhitelistHour.Location = new System.Drawing.Point(12, 50);
            this.buttonWhitelistHour.Name = "buttonWhitelistHour";
            this.buttonWhitelistHour.Size = new System.Drawing.Size(195, 38);
            this.buttonWhitelistHour.TabIndex = 1;
            this.buttonWhitelistHour.Text = "Whitelist for 1 hour";
            this.buttonWhitelistHour.UseVisualStyleBackColor = false;
            this.buttonWhitelistHour.Click += new System.EventHandler(this.buttonWhitelistHour_Click);
            // 
            // buttonWhitelistDay
            // 
            this.buttonWhitelistDay.BackColor = System.Drawing.Color.DarkOrange;
            this.buttonWhitelistDay.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.buttonWhitelistDay.Location = new System.Drawing.Point(12, 94);
            this.buttonWhitelistDay.Name = "buttonWhitelistDay";
            this.buttonWhitelistDay.Size = new System.Drawing.Size(195, 38);
            this.buttonWhitelistDay.TabIndex = 2;
            this.buttonWhitelistDay.Text = "Whitelist for 1 day";
            this.buttonWhitelistDay.UseVisualStyleBackColor = false;
            this.buttonWhitelistDay.Click += new System.EventHandler(this.buttonWhitelistDay_Click);
            // 
            // buttonWhitelistForever
            // 
            this.buttonWhitelistForever.BackColor = System.Drawing.Color.Green;
            this.buttonWhitelistForever.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.buttonWhitelistForever.ForeColor = System.Drawing.Color.White;
            this.buttonWhitelistForever.Location = new System.Drawing.Point(213, 50);
            this.buttonWhitelistForever.Name = "buttonWhitelistForever";
            this.buttonWhitelistForever.Size = new System.Drawing.Size(195, 81);
            this.buttonWhitelistForever.TabIndex = 3;
            this.buttonWhitelistForever.Text = "Whitelist forever";
            this.buttonWhitelistForever.UseVisualStyleBackColor = false;
            this.buttonWhitelistForever.Click += new System.EventHandler(this.buttonWhitelistForever_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.BackColor = System.Drawing.Color.DarkRed;
            this.buttonClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.buttonClose.ForeColor = System.Drawing.Color.White;
            this.buttonClose.Location = new System.Drawing.Point(420, 50);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(195, 81);
            this.buttonClose.TabIndex = 4;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = false;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // AlertForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(627, 143);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonWhitelistForever);
            this.Controls.Add(this.buttonWhitelistDay);
            this.Controls.Add(this.buttonWhitelistHour);
            this.Controls.Add(this.textBoxInfo);
            this.Name = "AlertForm";
            this.Text = "Alert";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button buttonWhitelistHour;
        private System.Windows.Forms.Button buttonWhitelistDay;
        private System.Windows.Forms.Button buttonWhitelistForever;
        private System.Windows.Forms.Button buttonClose;

        private System.Windows.Forms.TextBox textBoxInfo;

        #endregion
    }
}