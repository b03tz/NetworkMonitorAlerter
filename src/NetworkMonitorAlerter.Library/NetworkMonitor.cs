using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;

namespace NetworkMonitorAlerter.Library
{
    public class NetworkMonitor : IDisposable
    {
        private DateTime m_EtwStartTime;
        private TraceEventSession m_EtwSession;

        private readonly List<Counters> processCounters = new List<Counters>();
        private readonly Dictionary<int, NetworkPerformanceData> monitors = new Dictionary<int, NetworkPerformanceData>();

        private class Counters
        {
            public Process Process;
            public long Received;
            public long Sent;
        }

        public static NetworkMonitor Create(List<Process> processes)
        {
            var networkPerformancePresenter = new NetworkMonitor();
            networkPerformancePresenter.Initialize(processes);
            return networkPerformancePresenter;
        }

        private void Initialize(List<Process> processes)
        {
            foreach (var process in processes)
            {
                processCounters.Add(new Counters()
                {
                    Process = process
                });

                monitors[process.Id] = new NetworkPerformanceData
                {
                    Process = process
                };
            }

            // Note that the ETW class blocks processing messages, so should be run on a different thread if you want the application to remain responsive.
            Task.Run(() => StartEtwSession(processes));
            //StartEtwSession();
        }

        private void StartEtwSession(List<Process> processes)
        {
            try
            {
                ResetCounters();

                using (m_EtwSession =
                           new TraceEventSession(nameof(NetworkMonitor), TraceEventSessionOptions.Create))
                {
                    m_EtwSession.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP);
                    m_EtwSession.EnableProvider("Microsoft-Windows-TCPIP");
                    m_EtwSession.Source.Kernel.TcpIpRecv += data =>
                    {
                        var process = processCounters.FirstOrDefault(x => x.Process.Id == data.ProcessID);

                        lock (processCounters)
                        {
                            if (process != null)
                                process.Received += data.size;
                        }
                    };

                    m_EtwSession.Source.Kernel.TcpIpSend += data =>
                    {
                        var process = processCounters.FirstOrDefault(x => x.Process.Id == data.ProcessID);

                        lock (processCounters)
                        {
                            if (process != null)
                                process.Sent += data.size;
                        }
                    };

                    m_EtwSession.Source.Process();
                }
            }
            catch (Exception e)
            {
                ResetCounters(); // Stop reporting figures
                // Probably should log the exception
            }
        }

        public List<NetworkPerformanceData> GetNetworkPerformanceData()
        {
            var timeDifferenceInSeconds = (DateTime.Now - m_EtwStartTime).TotalSeconds;

            lock (processCounters)
            {
                foreach (var counter in processCounters)
                {
                    var receivedDiff = counter.Received - monitors[counter.Process.Id].BytesReceived;
                    var sentDiff = counter.Sent - monitors[counter.Process.Id].BytesSent;
                    monitors[counter.Process.Id].BytesReceived = counter.Received;
                    monitors[counter.Process.Id].BytesSent = counter.Sent;
                    monitors[counter.Process.Id].BandwidthReceived = receivedDiff;
                    monitors[counter.Process.Id].BandwidthSent = sentDiff;
                }
            }

            // Reset the counters to get a fresh reading for next time this is called.
            //ResetCounters();

            return monitors.Values.ToList();
        }

        private void ResetCounters()
        {
            lock (processCounters)
            {
                foreach (var counter in processCounters)
                {
                    counter.Sent = 0;
                    counter.Received = 0;
                }
            }

            m_EtwStartTime = DateTime.Now;
        }

        public void Dispose()
        {
            m_EtwSession?.Dispose();
        }
    }

    public sealed class NetworkPerformanceData
    {
        public Process Process { get; set; }
        public long BytesReceived { get; set; }
        public long BytesSent { get; set; }
        public long BandwidthReceived { get; set; }
        public long BandwidthSent { get; set; }
    }
}