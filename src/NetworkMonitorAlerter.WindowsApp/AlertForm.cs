using System;
using System.Diagnostics;
using System.Windows.Forms;
using NetworkMonitorAlerter.Library;

namespace NetworkMonitorAlerter.WindowsApp
{
    public partial class AlertForm : Form
    {
        private MainAppForm _mainAppForm;
        private DownloadOrUpload _downloadOrUpload;
        private Process _process;

        public AlertForm(MainAppForm mainAppForm, DownloadOrUpload downloadOrUpload, Process process)
        {
            _mainAppForm = mainAppForm;
            _downloadOrUpload = downloadOrUpload;
            _process = process;

            InitializeComponent();
            
            switch (downloadOrUpload)
            {
                case DownloadOrUpload.Download:
                    textBoxInfo.Text = $"The process '{mainAppForm.GetProcessTitle(process)}' has downloaded more than {_mainAppForm.Configuration.MaxMbDownloadInWindow} MB of data in the last {_mainAppForm.Configuration.RollingWindowSeconds} seconds.";
                    break;
                case DownloadOrUpload.Upload:
                    textBoxInfo.Text = $"The process '{mainAppForm.GetProcessTitle(process)}' has uploaded more than {_mainAppForm.Configuration.MaxMbUploadInWindow} MB of data in the last {_mainAppForm.Configuration.RollingWindowSeconds} seconds.";
                    break;
            }
        }

        private void buttonWhitelistHour_Click(object sender, EventArgs e)
        {
            _mainAppForm.WhitelistApplication(_process, DateTimeOffset.Now.AddHours(1), _downloadOrUpload);
            this.Close();
        }

        private void buttonWhitelistDay_Click(object sender, EventArgs e)
        {
            _mainAppForm.WhitelistApplication(_process, DateTimeOffset.Now.AddDays(1), _downloadOrUpload);
            this.Close();
        }

        private void buttonWhitelistForever_Click(object sender, EventArgs e)
        {
            _mainAppForm.WhitelistApplication(_process, DateTimeOffset.Now.AddYears(25), _downloadOrUpload);
            this.Close();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}