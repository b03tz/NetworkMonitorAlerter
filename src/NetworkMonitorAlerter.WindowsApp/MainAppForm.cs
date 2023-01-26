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
        public readonly List<LiveProcessView> LiveProcessViews = new List<LiveProcessView>();
        private List<NetworkPerformanceData> _latestNetworkPerformanceData = new List<NetworkPerformanceData>();

        public MainAppForm()
        {
            _columnSorter = new ListViewColumnSorter();
            InitializeComponent();
            InitializeMonitor();
        }

        public void InitializeMonitor()
        {
            listProcesses.View = View.Details;
            listProcesses.Items.Add("Loading...");
            listProcesses.Columns.Add(new ColumnHeader
            {
                Text = "Process",
                Name = "col1",
                Width = 230
            });
            listProcesses.Columns.Add(new ColumnHeader
            {
                Text = "▼ internet",
                Name = "col2",
                Width = 100,
                TextAlign = HorizontalAlignment.Right
            });
            listProcesses.Columns.Add(new ColumnHeader
            {
                Text = "▲ internet",
                Name = "col3",
                Width = 100,
                TextAlign = HorizontalAlignment.Right
            });
            listProcesses.Columns.Add(new ColumnHeader
            {
                Text = "▼ local",
                Name = "col4",
                Width = 100,
                TextAlign = HorizontalAlignment.Right
            });
            listProcesses.Columns.Add(new ColumnHeader
            {
                Text = "▲ local",
                Name = "col5",
                Width = 100,
                TextAlign = HorizontalAlignment.Right
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
            
            _latestNetworkPerformanceData = _networkMonitor.GetNetworkPerformanceData();
            UpdateProcessList(_latestNetworkPerformanceData);
            _latestNetworkPerformanceData = _latestNetworkPerformanceData.OrderBy(x => x.Process.ProcessName).ToList();

            labelMonitoringValue.Text = _latestNetworkPerformanceData.Count + " processes";
            foreach (var data in _latestNetworkPerformanceData)
            {
                var processName = GetProcessTitle(data.Process);
        
                var uploadFormName = processName + "_u";
                var downloadFormName = processName + "_d";
                if (data.Remote.TotalUploadInWindow > Configuration.MaxBytesUploadInWindow &&
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

                if (data.Remote.TotalDownloadInWindow > Configuration.MaxBytesDownloadInWindow &&
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
                
                foreach (var logger in _loggers)
                {
                    logger.AddBandwidth(processName, data.Remote.BandwidthReceived, DownloadOrUpload.Download);
                    logger.AddBandwidth(processName, data.Remote.BandwidthSent, DownloadOrUpload.Upload);
                }

                var liveView = LiveProcessViews.FirstOrDefault(x => x.ProcessName == processName);
                if (liveView != null)
                {
                    liveView.AddValue(data.Remote.BandwidthReceived, DownloadOrUpload.Download);
                    liveView.AddValue(data.Remote.BandwidthSent, DownloadOrUpload.Upload);
                    liveView.AddValueNetwork(data.Local.BandwidthReceived, DownloadOrUpload.Download);
                    liveView.AddValueNetwork(data.Local.BandwidthSent, DownloadOrUpload.Upload);
                }
            }
        }

        private void UpdateProcessList(List<NetworkPerformanceData> performanceData)
        {
            if (!this.Visible)
            {
                LiveProcessViews.Clear();
                listProcesses.Items.Clear();
                listProcesses.Items.Add("Loading...");
                return;
            }

            var added = false;
            foreach (var data in performanceData)
            {
                if (data.Remote.BandwidthSent == 0 && data.Remote.BandwidthReceived == 0 && data.Local.BandwidthSent == 0 && data.Local.BandwidthReceived == 0)
                    continue;

                var currentItem = GetListProcessItem(data.Process.ProcessName);
                if (currentItem != null)
                {
                    currentItem.SubItems[1].Text = StringHelpers.ToMegabytes(data.Remote.BytesReceived);
                    currentItem.SubItems[2].Text = StringHelpers.ToMegabytes(data.Remote.BytesSent);
                    currentItem.SubItems[3].Text = StringHelpers.ToMegabytes(data.Local.BandwidthReceived);
                    currentItem.SubItems[4].Text = StringHelpers.ToMegabytes(data.Local.BytesSent);
                    continue;
                }

                added = true;
                var listItem = new ListViewItem(data.Process.ProcessName);
                listItem.SubItems.Add(StringHelpers.ToMegabytes(data.Remote.BytesReceived));
                listItem.SubItems.Add(StringHelpers.ToMegabytes(data.Remote.BytesSent));
                listItem.SubItems.Add(StringHelpers.ToMegabytes(data.Local.BytesReceived));
                listItem.SubItems.Add(StringHelpers.ToMegabytes(data.Local.BytesSent));
                listProcesses.Items.Add(listItem);
            }

            foreach (ListViewItem item in listProcesses.Items)
            {
                if (performanceData.All(x =>
                        !string.Equals(x.Process.ProcessName, item.Text, StringComparison.InvariantCultureIgnoreCase)))
                {
                    listProcesses.Items.Remove(item);
                }
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

            if (process == null)
            {
                MessageBox.Show($"Could not find process: {process}. It might have quit?", "Process not found");
                return;
            }
            
            var form = new LiveProcessView(this, GetProcessTitle(process));
            form.WindowState = FormWindowState.Minimized;
            form.Show();
            form.WindowState = FormWindowState.Normal;

            var performanceData = _latestNetworkPerformanceData
                .FirstOrDefault(x => GetProcessTitle(x.Process) == GetProcessTitle(process));
            
            if (performanceData != null)
            {
                long downloadStart = 0;
                long uploadStart = 0;
                long downloadStartLocally = 0;
                long uploadStartLocally = 0;
                foreach (var data in performanceData.Remote.BytesReceivedLog)
                {
                    if (downloadStart == 0)
                    {
                        downloadStart = data.Bytes;
                        continue;
                    }
                    form.AddValue(data.Bytes - downloadStart, DownloadOrUpload.Download);
                    downloadStart = data.Bytes;
                }
            
                foreach (var data in performanceData.Remote.BytesSentLog)
                {
                    if (uploadStart == 0)
                    {
                        uploadStart = data.Bytes;
                        continue;
                    }
                    form.AddValue(data.Bytes - uploadStart, DownloadOrUpload.Upload);
                    uploadStart = data.Bytes;
                }
                
                foreach (var data in performanceData.Local.BytesReceivedLog)
                {
                    if (downloadStartLocally == 0)
                    {
                        downloadStartLocally = data.Bytes;
                        continue;
                    }
                    form.AddValueNetwork(data.Bytes - downloadStartLocally, DownloadOrUpload.Download);
                    downloadStartLocally = data.Bytes;
                }
            
                foreach (var data in performanceData.Local.BytesSentLog)
                {
                    if (uploadStartLocally == 0)
                    {
                        uploadStartLocally = data.Bytes;
                        continue;
                    }
                    form.AddValueNetwork(data.Bytes - uploadStartLocally, DownloadOrUpload.Upload);
                    uploadStartLocally = data.Bytes;
                }
            }
            
            LiveProcessViews.Add(form);
            form.Show();
        }
    }
}