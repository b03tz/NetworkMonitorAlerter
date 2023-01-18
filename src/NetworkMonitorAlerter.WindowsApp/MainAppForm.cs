using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using NetworkMonitorAlerter.Library;
using Newtonsoft.Json;

namespace NetworkMonitorAlerter.WindowsApp
{
    public partial class MainAppForm : Form
    {
        private string _configFile = "configuration.json";
        public Configuration Configuration;
        private NetworkMonitor _networkMonitor;

        public MainAppForm()
        {
            InitializeComponent();
            InitializeMonitor();

            Logger.TextBox = textBoxConsole;
        }

        private void GetConfiguration()
        {
            Configuration = new Configuration();
            if (!File.Exists(_configFile))
            {
                File.Create(_configFile);
                WriteConfiguration();
            }

            var configContent = File.ReadAllText(_configFile);
            Configuration = JsonConvert.DeserializeObject<Configuration>(configContent) ?? new Configuration();

            if (Configuration == null)
                throw new Exception("Configuration file could not be loaded");
        }

        public async void InitializeMonitor()
        {
            textBoxConsole.Text = "";
            GetConfiguration();

            if (Configuration.MonitorEveryXSeconds == 0)
                Configuration.MonitorEveryXSeconds = 5;
            
            textBoxConfigRollingWindowSeconds.Text = Configuration.RollingWindowSeconds.ToString();
            textBoxMaxMbDownloadInWindow.Text = Configuration.MaxMbDownloadInWindow.ToString();
            textBoxMaxMbUploadInWindow.Text = Configuration.MaxMbUploadInWindow.ToString();

            _networkMonitor = NetworkMonitor.CreateContinuousMonitor();

            timerData.Enabled = true;
            timerData.Interval = Configuration.MonitorEveryXSeconds * 1000;
            timerData.Start();
        }

        private void timerData_Elapsed(object sender, ElapsedEventArgs e)
        {
            textBoxConsole.Text = "";
            var performanceData = _networkMonitor.GetNetworkPerformanceData();
            performanceData = performanceData.OrderBy(x => x.Process.ProcessName).ToList();
            
            labelMonitoringValue.Text = performanceData.Count + " processes";
            foreach (var data in performanceData)
            {
                if (data.BytesReceived > 0)
                    Logger.Log($"D: {Logger.ToFixedString(data.BytesReceived.ToString(), 10)}      U: {Logger.ToFixedString(data.BytesSent.ToString(), 10)} - {data.Process.ProcessName}");

                if (data.BandwidthSent > Configuration.MaxBytesUploadInWindow &&
                    !IsApplicationWhitelisted(data.Process, DownloadOrUpload.Upload))
                {
                    new AlertForm(this, DownloadOrUpload.Upload, data.Process).Show();
                }

                if (data.BandwidthReceived > Configuration.MaxBytesDownloadInWindow &&
                    !IsApplicationWhitelisted(data.Process, DownloadOrUpload.Download))
                    new AlertForm(this, DownloadOrUpload.Download, data.Process).Show();
            }
        }
        
        private void buttonSaveConfiguration_Click(object sender, EventArgs e)
        {
            Configuration.RollingWindowSeconds = int.Parse(textBoxConfigRollingWindowSeconds.Text);
            Configuration.MaxMbDownloadInWindow = int.Parse(textBoxMaxMbDownloadInWindow.Text);
            Configuration.MaxMbUploadInWindow = int.Parse(textBoxMaxMbUploadInWindow.Text);
            WriteConfiguration();
        }
        
                public void WhitelistApplication(Process process, DateTimeOffset until, DownloadOrUpload type)
        {
            var configurationApplication =
                Configuration.Applications.FirstOrDefault(x =>
                    x.ProcessName == process.ProcessName.ToLower());

            if (configurationApplication == null)
                configurationApplication = new NetworkApplication
                {
                    ApplicationName = process.ProcessName,
                    ProcessName = process.ProcessName,
                    DownloadWhitelistedUntil = DateTimeOffset.Now,
                    UploadWhitelistedUntil = DateTimeOffset.Now
                };

            switch (type)
            {
                case DownloadOrUpload.Download:
                    configurationApplication.DownloadWhitelistedUntil = until;
                    break;
                case DownloadOrUpload.Upload:
                    configurationApplication.UploadWhitelistedUntil = until;
                    break;
            }

            Configuration.Applications.RemoveAll(x => x.ProcessName == process.ProcessName.ToLower());
            Configuration.Applications.Add(configurationApplication);

            WriteConfiguration();
        }

        private void WriteConfiguration()
        {
            File.WriteAllText(_configFile, JsonConvert.SerializeObject(Configuration));
        }

        private bool IsApplicationWhitelisted(Process process, DownloadOrUpload type)
        {
            var configurationApplication =
                Configuration.Applications.FirstOrDefault(x =>
                    x.ProcessName == process.ProcessName.ToLower());

            if (configurationApplication == null)
                return false;

            if (type == DownloadOrUpload.Download &&
                configurationApplication.DownloadWhitelistedUntil > DateTimeOffset.Now)
                return true;

            if (type == DownloadOrUpload.Upload &&
                configurationApplication.UploadWhitelistedUntil > DateTimeOffset.Now)
                return true;

            return false;
        }

        public enum DownloadOrUpload
        {
            Download,
            Upload
        }
    }
}