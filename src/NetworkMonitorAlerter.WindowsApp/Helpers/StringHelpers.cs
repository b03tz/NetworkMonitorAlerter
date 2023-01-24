using System;

namespace NetworkMonitorAlerter.WindowsApp.Helpers
{
    public static class StringHelpers
    {
        public static string ToMegabytes(long totalBytes)
        {
            var mb = Convert.ToDecimal(totalBytes);
            var converted = Math.Round(mb / 1024 / 1024, 2, MidpointRounding.ToEven);
            return converted.ToString().Replace(",", ".") + " MB";
        }
    }
}