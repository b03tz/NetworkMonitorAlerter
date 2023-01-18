using System;
using System.Collections.Generic;

namespace NetworkMonitorAlerter.Library
{
    public class Configuration
    {
        public int MonitorEveryXSeconds = 5;
        public int RollingWindowSeconds = 5 * 60;
        public long MaxMbUploadInWindow = 15;
        public long MaxMbDownloadInWindow = 15;
        public List<NetworkApplication> Applications { get; set; } = new List<NetworkApplication>();
        public long MaxKbUploadInWindow => MaxMbUploadInWindow * 1024;
        public long MaxBytesUploadInWindow => MaxKbUploadInWindow * 1024;
        public long MaxKbDownloadInWindow => MaxMbDownloadInWindow * 1024;
        public long MaxBytesDownloadInWindow => MaxKbDownloadInWindow * 1024;
    }

    public class NetworkApplication
    {
        public string ApplicationName { get; set; }
        public string ProcessName { get; set; }
        public DateTimeOffset UploadWhitelistedUntil { get; set; }
        public DateTimeOffset DownloadWhitelistedUntil { get; set; }
    }
}