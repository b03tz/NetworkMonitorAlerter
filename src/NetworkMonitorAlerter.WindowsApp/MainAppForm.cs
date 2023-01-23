using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows.Forms;
using NetworkMonitorAlerter.Library;
using Newtonsoft.Json;

namespace NetworkMonitorAlerter.WindowsApp
{
    public partial class MainAppForm : Form
    {
        private const string ConfigFile = "configuration.json";
        public Configuration Configuration = new Configuration();
        private NetworkMonitor? _networkMonitor;
        private readonly List<string> _openForms = new List<string>();
        private readonly List<BandwidthLogger> _loggers = new List<BandwidthLogger>();

        public MainAppForm()
        {
            InitializeComponent();
            InitializeMonitor();

            TextBoxLogger.TextBox = textBoxConsole;
        }

        private void GetConfiguration()
        {
            Configuration = new Configuration();
            if (!File.Exists(ConfigFile))
            {
                File.Create(ConfigFile);
                WriteConfiguration();
            }

            var configContent = File.ReadAllText(ConfigFile);
            Configuration = JsonConvert.DeserializeObject<Configuration>(configContent) ?? new Configuration();

            if (Configuration == null)
                throw new Exception("Configuration file could not be loaded");
        }

        public void InitializeMonitor()
        {
            _loggers.Add(new BandwidthLogger(LoggerType.Daily));
            _loggers.Add(new BandwidthLogger(LoggerType.Weekly));
            _loggers.Add(new BandwidthLogger(LoggerType.Monthly));
            
            timerLogger.Interval = (3 * 60);
            timerLogger.Enabled = true;
            
            systemTrayIcon.Visible = true;
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
            if (_networkMonitor == null)
                return;
            
            textBoxConsole.Text = "";
            var performanceData = _networkMonitor.GetNetworkPerformanceData();
            performanceData = performanceData.OrderBy(x => x.Process.ProcessName).ToList();

            labelMonitoringValue.Text = performanceData.Count + " processes";
            foreach (var data in performanceData)
            {
                if (data.BytesReceived > 0)
                {
                    TextBoxLogger.Log(
                        $"D: {TextBoxLogger.ToFixedString(data.TotalDownloadInWindow.ToString(), 10)}      U: {TextBoxLogger.ToFixedString(data.TotalUploadInWindow.ToString(), 10)} - {data.Process.ProcessName}");
                }

                var processName = data.Process.ProcessName.ToLower();

                foreach (var logger in _loggers)
                {
                    logger.AddBandwidth(processName, data.BandwidthReceived, DownloadOrUpload.Download);
                    logger.AddBandwidth(processName, data.BandwidthSent, DownloadOrUpload.Upload);
                }
        
                var uploadFormName = processName + "_u";
                var downloadFormName = processName + "_d";
                if (data.TotalUploadInWindow > Configuration.MaxBytesUploadInWindow &&
                    !IsApplicationWhitelisted(data.Process, DownloadOrUpload.Upload) &&
                    !_openForms.Contains(uploadFormName))
                {
                    _openForms.Add(uploadFormName);
                    var k = new AlertForm(this, DownloadOrUpload.Upload, data.Process);
                    k.Closed += (_, _) => { _openForms.Remove(uploadFormName); };
                    k.WindowState = FormWindowState.Minimized;
                    k.Show();
                    k.WindowState = FormWindowState.Normal;
                    k.Activate();
                }

                if (data.TotalDownloadInWindow > Configuration.MaxBytesDownloadInWindow &&
                    !IsApplicationWhitelisted(data.Process, DownloadOrUpload.Download) &&
                    !_openForms.Contains(downloadFormName))
                {
                    _openForms.Add(downloadFormName);
                    var k = new AlertForm(this, DownloadOrUpload.Download, data.Process);
                    k.Closed += (_, _) => { _openForms.Remove(downloadFormName); };
                    k.WindowState = FormWindowState.Minimized;
                    k.Show();
                    k.WindowState = FormWindowState.Normal;
                    k.Activate();
                }
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
                    x.ProcessName.ToLower() == process.ProcessName.ToLower());

            if (configurationApplication == null)
                configurationApplication = new NetworkApplication
                {
                    ApplicationName = GetProcessTitle(process),
                    ProcessName = process.ProcessName.ToLower(),
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
            File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(Configuration, Formatting.Indented));
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

        public string GetProcessTitle(Process process)
        {
            return process.ProcessName;
        }

        private void MainAppForm_SizeChanged(object sender, EventArgs e)
        {
            textBoxConsole.Height = Height - 205;
        }

        private void MainAppForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                systemTrayIcon.Visible = true;
            }
        }

        private void systemTrayIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void MainAppForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing) 
                return;
            
            systemTrayIcon.Visible = true;
            Hide();
            e.Cancel = true;
        }

        private void systemTrayMenuQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void timerLogger_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach(var logger in _loggers)
                logger.WriteLogFile();
        }
    }
}