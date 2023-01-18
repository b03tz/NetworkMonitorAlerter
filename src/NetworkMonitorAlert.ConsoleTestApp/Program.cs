using System;
using System.Threading.Tasks;

namespace NetworkMonitorAlert.ConsoleTestApp
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var a = NetworkPerformanceReporter.Create();

            while (true)
            {
                var result = a.GetNetworkPerformanceData();
                
                Console.WriteLine(result.BytesReceived + " / " + result.BytesSent);
                await Task.Delay(1000);
            }
        }
    }
}