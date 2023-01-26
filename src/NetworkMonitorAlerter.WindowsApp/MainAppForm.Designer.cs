namespace NetworkMonitorAlerter.WindowsApp
{
    partial class MainAppForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainAppForm));
            this.timerData = new System.Timers.Timer();
            this.labelInfoMonitoring = new System.Windows.Forms.Label();
            this.labelMonitoringValue = new System.Windows.Forms.Label();
            this.textBoxConfigRollingWindowSeconds = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxMaxMbDownloadInWindow = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxMaxMbUploadInWindow = new System.Windows.Forms.TextBox();
            this.buttonSaveConfiguration = new System.Windows.Forms.Button();
            this.systemTrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.systemTrayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.systemTrayMenuQuit = new System.Windows.Forms.ToolStripMenuItem();
            this.timerLogger = new System.Timers.Timer();
            this.buttonLogs = new System.Windows.Forms.Button();
            this.listProcesses = new System.Windows.Forms.ListView();
            ((System.ComponentModel.ISupportInitialize) (this.timerData)).BeginInit();
            this.systemTrayMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.timerLogger)).BeginInit();
            this.SuspendLayout();
            // 
            // timerData
            // 
            this.timerData.Interval = 5000D;
            this.timerData.SynchronizingObject = this;
            this.timerData.Elapsed += new System.Timers.ElapsedEventHandler(this.timerData_Elapsed);
            // 
            // labelInfoMonitoring
            // 
            this.labelInfoMonitoring.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelInfoMonitoring.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.labelInfoMonitoring.Location = new System.Drawing.Point(12, 95);
            this.labelInfoMonitoring.Name = "labelInfoMonitoring";
            this.labelInfoMonitoring.Size = new System.Drawing.Size(142, 21);
            this.labelInfoMonitoring.TabIndex = 1;
            this.labelInfoMonitoring.Text = "Monitoring:";
            this.labelInfoMonitoring.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelMonitoringValue
            // 
            this.labelMonitoringValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelMonitoringValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.labelMonitoringValue.Location = new System.Drawing.Point(153, 95);
            this.labelMonitoringValue.Name = "labelMonitoringValue";
            this.labelMonitoringValue.Size = new System.Drawing.Size(423, 21);
            this.labelMonitoringValue.TabIndex = 2;
            this.labelMonitoringValue.Text = "0 processes";
            this.labelMonitoringValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxConfigRollingWindowSeconds
            // 
            this.textBoxConfigRollingWindowSeconds.Location = new System.Drawing.Point(506, 13);
            this.textBoxConfigRollingWindowSeconds.Name = "textBoxConfigRollingWindowSeconds";
            this.textBoxConfigRollingWindowSeconds.Size = new System.Drawing.Size(70, 20);
            this.textBoxConfigRollingWindowSeconds.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.label1.Location = new System.Drawing.Point(254, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(246, 21);
            this.label1.TabIndex = 4;
            this.label1.Text = "Bandwidth testing window (seconds)";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.label2.Location = new System.Drawing.Point(254, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(246, 21);
            this.label2.TabIndex = 6;
            this.label2.Text = "Max download in window (MB)";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxMaxMbDownloadInWindow
            // 
            this.textBoxMaxMbDownloadInWindow.Location = new System.Drawing.Point(506, 39);
            this.textBoxMaxMbDownloadInWindow.Name = "textBoxMaxMbDownloadInWindow";
            this.textBoxMaxMbDownloadInWindow.Size = new System.Drawing.Size(70, 20);
            this.textBoxMaxMbDownloadInWindow.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.label3.Location = new System.Drawing.Point(254, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(246, 21);
            this.label3.TabIndex = 8;
            this.label3.Text = "Max upload in window (MB)";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxMaxMbUploadInWindow
            // 
            this.textBoxMaxMbUploadInWindow.Location = new System.Drawing.Point(506, 65);
            this.textBoxMaxMbUploadInWindow.Name = "textBoxMaxMbUploadInWindow";
            this.textBoxMaxMbUploadInWindow.Size = new System.Drawing.Size(70, 20);
            this.textBoxMaxMbUploadInWindow.TabIndex = 7;
            // 
            // buttonSaveConfiguration
            // 
            this.buttonSaveConfiguration.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.buttonSaveConfiguration.Location = new System.Drawing.Point(582, 13);
            this.buttonSaveConfiguration.Name = "buttonSaveConfiguration";
            this.buttonSaveConfiguration.Size = new System.Drawing.Size(96, 72);
            this.buttonSaveConfiguration.TabIndex = 9;
            this.buttonSaveConfiguration.Text = "Save";
            this.buttonSaveConfiguration.UseVisualStyleBackColor = true;
            this.buttonSaveConfiguration.Click += new System.EventHandler(this.buttonSaveConfiguration_Click);
            // 
            // systemTrayIcon
            // 
            this.systemTrayIcon.BalloonTipText = "NetworkMonitorAlerter";
            this.systemTrayIcon.BalloonTipTitle = "Alerter";
            this.systemTrayIcon.ContextMenuStrip = this.systemTrayMenu;
            this.systemTrayIcon.Icon = ((System.Drawing.Icon) (resources.GetObject("systemTrayIcon.Icon")));
            this.systemTrayIcon.Text = "NetworkMonitorAlerter";
            this.systemTrayIcon.DoubleClick += new System.EventHandler(this.systemTrayIcon_DoubleClick);
            // 
            // systemTrayMenu
            // 
            this.systemTrayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {this.systemTrayMenuQuit});
            this.systemTrayMenu.Name = "systemTrayMenu";
            this.systemTrayMenu.Size = new System.Drawing.Size(160, 26);
            // 
            // systemTrayMenuQuit
            // 
            this.systemTrayMenuQuit.Name = "systemTrayMenuQuit";
            this.systemTrayMenuQuit.Size = new System.Drawing.Size(159, 22);
            this.systemTrayMenuQuit.Text = "Quit application";
            this.systemTrayMenuQuit.Click += new System.EventHandler(this.systemTrayMenuQuit_Click);
            // 
            // timerLogger
            // 
            this.timerLogger.Interval = 30000D;
            this.timerLogger.SynchronizingObject = this;
            this.timerLogger.Elapsed += new System.Timers.ElapsedEventHandler(this.timerLogger_Elapsed);
            // 
            // buttonLogs
            // 
            this.buttonLogs.BackColor = System.Drawing.Color.MediumTurquoise;
            this.buttonLogs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.buttonLogs.Location = new System.Drawing.Point(582, 94);
            this.buttonLogs.Name = "buttonLogs";
            this.buttonLogs.Size = new System.Drawing.Size(96, 23);
            this.buttonLogs.TabIndex = 10;
            this.buttonLogs.Text = "Show logs";
            this.buttonLogs.UseVisualStyleBackColor = false;
            this.buttonLogs.Click += new System.EventHandler(this.buttonLogs_Click);
            // 
            // listProcesses
            // 
            this.listProcesses.FullRowSelect = true;
            this.listProcesses.GridLines = true;
            this.listProcesses.Location = new System.Drawing.Point(12, 123);
            this.listProcesses.Name = "listProcesses";
            this.listProcesses.Size = new System.Drawing.Size(666, 418);
            this.listProcesses.TabIndex = 11;
            this.listProcesses.UseCompatibleStateImageBehavior = false;
            this.listProcesses.View = System.Windows.Forms.View.Details;
            this.listProcesses.DoubleClick += new System.EventHandler(this.listProcesses_DoubleClick);
            // 
            // MainAppForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(690, 553);
            this.Controls.Add(this.listProcesses);
            this.Controls.Add(this.buttonLogs);
            this.Controls.Add(this.buttonSaveConfiguration);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxMaxMbUploadInWindow);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxMaxMbDownloadInWindow);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxConfigRollingWindowSeconds);
            this.Controls.Add(this.labelMonitoringValue);
            this.Controls.Add(this.labelInfoMonitoring);
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.Name = "MainAppForm";
            this.Text = "Network Monitor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainAppForm_FormClosing);
            this.SizeChanged += new System.EventHandler(this.MainAppForm_SizeChanged);
            this.Resize += new System.EventHandler(this.MainAppForm_Resize);
            ((System.ComponentModel.ISupportInitialize) (this.timerData)).EndInit();
            this.systemTrayMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.timerLogger)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ListView listProcesses;

        private System.Windows.Forms.Button buttonLogs;

        private System.Timers.Timer timerLogger;

        private System.Windows.Forms.ToolStripMenuItem systemTrayMenuQuit;

        private System.Windows.Forms.ContextMenuStrip systemTrayMenu;

        private System.Windows.Forms.NotifyIcon systemTrayIcon;

        private System.Windows.Forms.Button buttonSaveConfiguration;

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxMaxMbUploadInWindow;

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxMaxMbDownloadInWindow;

        private System.Windows.Forms.TextBox textBoxConfigRollingWindowSeconds;
        private System.Windows.Forms.Label label1;

        private System.Windows.Forms.Label labelMonitoringValue;

        private System.Windows.Forms.Label labelInfoMonitoring;

        private System.Timers.Timer timerData;

        #endregion
    }
}