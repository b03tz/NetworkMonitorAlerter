using System.ComponentModel;

namespace NetworkMonitorAlerter.WindowsApp
{
    partial class LiveProcessView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LiveProcessView));
            this.SuspendLayout();
            // 
            // LiveProcessView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.Name = "LiveProcessView";
            this.Text = "LiveProcessView";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LiveProcessView_FormClosing);
            this.SizeChanged += new System.EventHandler(this.LiveProcessView_SizeChanged);
            this.ResumeLayout(false);
        }

        #endregion
    }
}