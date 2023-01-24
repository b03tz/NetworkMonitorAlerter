using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace NetworkMonitorAlerter.Library
{
    public class BandwidthLogger
    {
        private readonly string _logDirectory = "logs";
        private LogFile _logFileContents;
        public readonly LoggerType Type;
        private string _logFile = "";
        
        public BandwidthLogger(LoggerType type)
        {
            Type = type;
            _logDirectory = Path.Combine(_logDirectory, type.ToString());
            
            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);

            ReadLogFile();
        }

        public LogFile GetLog() => _logFileContents;

        private void ReadLogFile()
        {
            var logFile = GetLogfileLocation();
            if (logFile == _logFile)
                return;

            _logFile = logFile;
            _logFileContents = new LogFile();
            if (!File.Exists(logFile))
            {
                try
                {
                    File.WriteAllText(logFile, "{}");
                }
                catch
                {
                    // Ignored
                }
            }

            try
            {
                _logFileContents = JsonConvert.DeserializeObject<LogFile>(File.ReadAllText(logFile)) ?? new LogFile();
            }
            catch
            {
                _logFileContents = new LogFile();
            }
        }

        public void WriteLogFile()
        {
            // Don't write logfile before it's loaded
            if (_logFileContents == null)
                return;
            
            var logFileLocation = GetLogfileLocation();
            File.WriteAllText(logFileLocation, JsonConvert.SerializeObject(_logFileContents, Formatting.Indented));
        }

        public void AddBandwidth(string processName, long bandwidth, DownloadOrUpload type)
        {
            if (bandwidth == 0)
                return;

            ReadLogFile();
            
            processName = processName.ToLower();

            var application = _logFileContents.Applications.FirstOrDefault(x => x.ApplicationName == processName);
            if (application == null)
            {
                application = new LogApplication
                {
                    ApplicationName = processName
                };
                _logFileContents.Applications.Add(application);
            }

            switch (type)
            {
                case DownloadOrUpload.Download:
                    application.TotalBytesDownloaded += bandwidth;
                    break;
                case DownloadOrUpload.Upload:
                    application.TotalBytesUploaded += bandwidth;
                    break;
            }
        }

        private string GetLogfileLocation()
        {
            var dateFormat = "";
            
            switch (Type)
            {
                case LoggerType.Daily:
                    dateFormat = DateTime.Now.ToString("yyMMdd");
                    break;
                case LoggerType.Weekly:
                    Calendar cal = new CultureInfo("en-US").Calendar;
                    var week = cal.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday).ToString();
                    if (week.Length == 1)
                        week = $"0{week}";
                    
                    dateFormat = DateTime.Now.ToString($"yy{week}");
                    break;
                case LoggerType.Monthly:
                    dateFormat = DateTime.Now.ToString("yyMM");
                    break;
            }
            
            return Path.Combine(_logDirectory, $"{dateFormat}.json");
        }
    }

    public class LogFile
    {
        public List<LogApplication> Applications { get; set; } = new List<LogApplication>();
    }

    public class LogApplication
    {
        public string ApplicationName { get; set; }
        public long TotalBytesDownloaded { get; set; }
        public long TotalBytesUploaded { get; set; }
    }

    public enum LoggerType
    {
        Daily,
        Weekly,
        Monthly
    }
}