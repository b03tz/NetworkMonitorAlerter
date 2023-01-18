using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetworkMonitorAlerter.Library;
using Newtonsoft.Json;

namespace NetworkMonitorAlerter.WindowsApp
{
    public partial class MainAppForm : Form
    {
        private string _configFile = "configuration.json";
        private Configuration _configuration;
        public MainAppForm()
        {
            InitializeComponent();
            InitializeMonitor();
        }

        private void GetConfiguration()
        {
            _configuration = new Configuration();
            if (!File.Exists(_configFile))
            {
                File.Create(_configFile);
                WriteConfiguration();
            }

            var configContent = File.ReadAllText(_configFile);
            _configuration = JsonConvert.DeserializeObject<Configuration>(configContent) ?? new Configuration();

            if (_configuration == null)
                throw new Exception("Configuration file could not be loaded");
        }

        public async void InitializeMonitor()
        {
            GetConfiguration();
            
            var processes = Process.GetProcesses();
            var monitors = NetworkMonitor.Create(processes.ToList());

            while (true)
            {
                var performanceData = monitors.GetNetworkPerformanceData();
                foreach (var data in performanceData)
                {
                    var triggerValue = 5 * 1024 * 1024;
                    
                    Console.WriteLine($"{data.Process.ProcessName}: Whitelisted: {(IsApplicationWhitelisted(data.Process, DownloadOrUpload.Upload) ? "Yes" : "No")}");
                    Console.WriteLine($"{data.Process.ProcessName}: Triggered U: {data.BandwidthSent > triggerValue}");
                    Console.WriteLine($"{data.Process.ProcessName}: Triggered D: {data.BandwidthReceived > triggerValue}");
                    Console.WriteLine("_________________");
                    
                    if (!IsApplicationWhitelisted(data.Process, DownloadOrUpload.Upload) && data.BandwidthSent > triggerValue)
                        if (MessageBox.Show(
                                $"{data.Process.ProcessName} has sent more than {triggerValue / 1024 / 1024}MB",
                                "Bandwidth alert", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            WhitelistApplication(data.Process, DateTimeOffset.Now.AddHours(2), DownloadOrUpload.Upload);
                        }
                    
                    if (!IsApplicationWhitelisted(data.Process, DownloadOrUpload.Download) && data.BandwidthReceived > triggerValue)
                        if (MessageBox.Show(
                                $"{data.Process.ProcessName} has received more than {triggerValue / 1024 / 1024}MB",
                                "Bandwidth alert", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            WhitelistApplication(data.Process, DateTimeOffset.Now.AddHours(2), DownloadOrUpload.Download);
                        }
                }
                
                await Task.Delay(5000);
            }
        }

        private void WhitelistApplication(Process process, DateTimeOffset until, DownloadOrUpload type)
        {
            var configurationApplication =
                _configuration.Applications.FirstOrDefault(x =>
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

            _configuration.Applications.RemoveAll(x => x.ProcessName == process.ProcessName.ToLower());
            _configuration.Applications.Add(configurationApplication);

            WriteConfiguration();
        }

        private void WriteConfiguration()
        {
            File.WriteAllText(_configFile, JsonConvert.SerializeObject(_configuration));
        }

        private bool IsApplicationWhitelisted(Process process, DownloadOrUpload type)
        {
            var configurationApplication =
                _configuration.Applications.FirstOrDefault(x =>
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

    internal enum DownloadOrUpload
    {
        Download,
        Upload
    }
}
}