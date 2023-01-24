using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows.Forms;
using NetworkMonitorAlerter.Library;
using NetworkMonitorAlerter.WindowsApp.Helpers;
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
        public Button ShowLogButton => buttonLogs;
        private ListViewColumnSorter _columnSorter;

        public MainAppForm()
        {
            _columnSorter = new ListViewColumnSorter();
            InitializeComponent();
            InitializeMonitor();
        }

        public void InitializeMonitor()
        {
            listProcesses.View = View.Details;
            listProcesses.Columns.Add(new ColumnHeader
            {
                Text = "Process",
                Name = "col1",
                Width = 220,
            });
            listProcesses.Columns.Add(new ColumnHeader
            {
                Text = "Downloaded (MB)",
                Name = "col2",
                Width = 120
            });
            listProcesses.Columns.Add(new ColumnHeader
            {
                Text = "Uploaded (MB)",
                Name = "col3",
                Width = 120
            });
            listProcesses.ListViewItemSorter = _columnSorter;
            listProcesses.ColumnClick += ListProcessesOnColumnClick;
            _columnSorter.Column = 0;
            _columnSorter.Order = SortOrder.Ascending;
            listProcesses.Sort();
            
            _loggers.Add(new BandwidthLogger(LoggerType.Daily));
            _loggers.Add(new BandwidthLogger(LoggerType.Weekly));
            _loggers.Add(new BandwidthLogger(LoggerType.Monthly));
            
            timerLogger.Interval = 3 * 60;
            timerLogger.Enabled = true;
            
            systemTrayIcon.Visible = true;
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

        private void ListProcessesOnColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == _columnSorter.Column)
            {
                // Reverse the current sort direction for this column.
                if (_columnSorter.Order == SortOrder.Ascending)
                {
                    _columnSorter.Order = SortOrder.Descending;
                    listProcesses.Sort();
                    return;
                }

                _columnSorter.Order = SortOrder.Ascending;
                listProcesses.Sort();
                return;
            }

            // Set the column number that is to be sorted; default to ascending.
            _columnSorter.Column = e.Column;
            _columnSorter.Order = SortOrder.Ascending;
            listProcesses.Sort();
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
            try
            {
                File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(Configuration, Formatting.Indented));
            }
            catch 
            {
                MessageBox.Show("Configuration could not be written, try again!", "Failed");
            }
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

        public string GetProcessTitle(Process process) => process.ProcessName;

        private void MainAppForm_SizeChanged(object sender, EventArgs e)
        {
            listProcesses.Height = Height - 205;
        }

        private void MainAppForm_Resize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized) 
                return;
            
            Hide();
            systemTrayIcon.Visible = true;
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
            try
            {
                foreach (var logger in _loggers)
                {
                    logger.WriteLogFile();
                }
            }
            catch
            {
                // Ignored
            }
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
        
        private void timerData_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_networkMonitor == null)
                return;
            
            var performanceData = _networkMonitor.GetNetworkPerformanceData();
            UpdateProcessList(performanceData);
            performanceData = performanceData.OrderBy(x => x.Process.ProcessName).ToList();

            labelMonitoringValue.Text = performanceData.Count + " processes";
            foreach (var data in performanceData)
            {
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

        private void UpdateProcessList(List<NetworkPerformanceData> performanceData)
        {
            if (!this.Visible)
                return;

            var added = false;
            foreach (var data in performanceData)
            {
                if (data.BandwidthSent == 0 && data.BandwidthReceived == 0)
                    continue;

                var currentItem = GetListProcessItem(data.Process.ProcessName);
                if (currentItem != null)
                {
                    currentItem.SubItems[1].Text = StringHelpers.ToMegabytes(data.BytesReceived);
                    currentItem.SubItems[2].Text = StringHelpers.ToMegabytes(data.BytesSent);
                    continue;
                }

                added = true;
                var listItem = new ListViewItem(data.Process.ProcessName);
                listItem.SubItems.Add(StringHelpers.ToMegabytes(data.BytesReceived));
                listItem.SubItems.Add(StringHelpers.ToMegabytes(data.BytesSent));
                listProcesses.Items.Add(listItem);
            }

            if (added)
                listProcesses.Sort();
        }

        private ListViewItem? GetListProcessItem(string processName)
        {
            foreach (ListViewItem item in listProcesses.Items)
            {
                if (item.Text == processName)
                    return item;
            }

            return null;
        }

        private void buttonLogs_Click(object sender, EventArgs e)
        {
            buttonLogs.Enabled = false;
            new LogForm(_loggers, this).Show();
        }

        private void listProcesses_DoubleClick(object sender, EventArgs e)
        {
            if (listProcesses.SelectedItems.Count == 0)
                return;

            var item = listProcesses.SelectedItems[0];
            var process = Process.GetProcesses().FirstOrDefault(x => x.ProcessName == item.Text);

            var message = $"Processname: {process.ProcessName}\n";
            message += $"Started: {process.StartTime}\n";
            message += $"Filename: {process.MainModule?.FileName}";

            MessageBox.Show(message, $"Process info: {item.Text}");
        }
    }
}