using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;

namespace NetworkMonitorAlerter.Library
{
    public class NetworkMonitor : IDisposable
    {
        //private DateTime _mEtwStartTime;
        private DateTime _lastProcessUpdateTime;
        private TraceEventSession _mEtwSession;

        private readonly Dictionary<string, Counters> _processCounters = new Dictionary<string, Counters>();

        private readonly Dictionary<string, NetworkPerformanceData> _monitors =
            new Dictionary<string, NetworkPerformanceData>();

        private readonly List<string> _monitoredProcesses = new List<string>();
        private readonly HashSet<Process> _allProcesses = new HashSet<Process>();
        private readonly bool _isContinuous;
        private readonly int _updateProcessInterval;
        private readonly int _maxLogAge;
        private readonly IPAddress localHostIP4 = IPAddress.Parse("127.0.0.1");
        private readonly IPAddress localHostIP6 = IPAddress.Parse("::1");

        private class Counters
        {
            public Process Process;
            public long Received;
            public long Sent;
            public long ReceivedLocally;
            public long SentLocally;
        }

        private NetworkMonitor(bool isContinuous, int updateInterval = 30, int maxLogAge = 300)
        {
            _updateProcessInterval = updateInterval;
            _maxLogAge = maxLogAge;
            _isContinuous = isContinuous;
        }

        public static NetworkMonitor Create(List<Process> processes, int maxLogAge = 300)
        {
            var networkPerformancePresenter = new NetworkMonitor(false, 30, maxLogAge);

            foreach (var process in processes)
            {
                networkPerformancePresenter.AddProcess(process);
            }

            networkPerformancePresenter.Initialize();
            return networkPerformancePresenter;
        }

        public static NetworkMonitor CreateContinuousMonitor(int updateInterval = 30, int maxLogAge = 300)
        {
            var networkPerformancePresenter = new NetworkMonitor(true, updateInterval, maxLogAge);
            networkPerformancePresenter.Initialize();
            return networkPerformancePresenter;
        }

        private string GetProcessName(Process process) => process == null ? "" : process.ProcessName.ToLower();

        private string GetProcessName(int processId)
        {
            lock (_allProcesses)
            {
                return GetProcessName(_allProcesses.FirstOrDefault(x => x.Id == processId));
            }
        }

        public void AddProcess(Process process)
        {
            if (process == null)
                throw new ArgumentNullException(nameof(process));

            var processName = GetProcessName(process);
            
            lock (_processCounters)
            {
                if (_processCounters.ContainsKey(processName))
                    return;
            }

            lock (_monitoredProcesses)
            {
                _monitoredProcesses.Add(processName);
            }

            lock (_processCounters)
            {
                _processCounters.Add(processName, new Counters
                {
                    Process = process,
                    Received = 0,
                    Sent = 0
                });
            }

            lock (_monitors)
            {
                _monitors.Add(processName, new NetworkPerformanceData
                {
                    Process = process
                });
            }
        }

        public void RemoveProcess(string processName)
        {
            lock (_monitoredProcesses)
            {
                _monitoredProcesses.RemoveAll(x => x == processName);
            }

            lock (_processCounters)
            {
                _processCounters.Remove(processName);
            }

            lock (_monitors)
            {
                _monitors.Remove(processName);
            }
        }

        private void Initialize()
        {
            if (_isContinuous)
                UpdateProcesses();
            
            Task.Run(StartEtwSession);
        }

        private void StartEtwSession()
        {
            try
            {
                ResetCounters();

                using (_mEtwSession = new TraceEventSession(nameof(NetworkMonitor)))
                {
                    _mEtwSession.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP);
                    _mEtwSession.EnableProvider("Microsoft-Windows-TCPIP");
                    
                    _mEtwSession.Source.Kernel.TcpIpSend += (data) => 
                        LogSentData(GetProcessName(data.ProcessID), data.size, IsLocal(data.saddr, data.daddr));
                    _mEtwSession.Source.Kernel.TcpIpRecv += (data) => 
                        LogReceivedData(GetProcessName(data.ProcessID), data.size, IsLocal(data.saddr, data.daddr));
                    _mEtwSession.Source.Kernel.TcpIpSendIPV6 += (data) => 
                        LogSentData(GetProcessName(data.ProcessID), data.size, IsLocal(data.saddr, data.daddr));
                    _mEtwSession.Source.Kernel.TcpIpRecvIPV6 += (data) => 
                        LogReceivedData(GetProcessName(data.ProcessID), data.size, IsLocal(data.saddr, data.daddr));
                    _mEtwSession.Source.Kernel.UdpIpSend += (data) => 
                        LogSentData(GetProcessName(data.ProcessID), data.size, IsLocal(data.saddr, data.daddr));
                    _mEtwSession.Source.Kernel.UdpIpRecv += (data) => 
                        LogReceivedData(GetProcessName(data.ProcessID), data.size, IsLocal(data.saddr, data.daddr));
                    _mEtwSession.Source.Kernel.UdpIpSendIPV6 += (data) => 
                        LogSentData(GetProcessName(data.ProcessID), data.size, IsLocal(data.saddr, data.daddr));
                    _mEtwSession.Source.Kernel.UdpIpRecvIPV6 += (data) => 
                        LogReceivedData(GetProcessName(data.ProcessID), data.size, IsLocal(data.saddr, data.daddr));

                    _mEtwSession.Source.Kernel.TcpIpRecv += DebugPacket;
                    _mEtwSession.Source.Kernel.UdpIpRecv += DebugPacket;

                    _mEtwSession.Source.Process();
                }
            }
            catch
            {
                ResetCounters(); // Stop reporting figures
                // Probably should log the exception
            }
        }

        private void DebugPacket(UdpIpTraceData obj)
        {
            if (obj.size > 10000)
            {
                var local = IsLocal(obj.saddr, obj.daddr);
                if (local)
                {
                    
                }
            }
        }

        private void DebugPacket(TcpIpTraceData obj)
        {
            if (obj.size > 10000)
            {
                var local = IsLocal(obj.saddr, obj.daddr);
                if (local)
                {
                    
                }
            }
        }

        private bool IsLocal(IPAddress src, IPAddress dest)
        {
            return (Equals(src, localHostIP4) || Equals(src, localHostIP4)) && (Equals(dest, localHostIP6) || Equals(dest, localHostIP6));
        }
        
        private void LogSentData(string processName, int size, bool isLocal)
        {
            lock (_processCounters)
            {
                if (!_processCounters.ContainsKey(processName)) return;

                
                if (isLocal)
                {
                    _processCounters[processName].SentLocally += Convert.ToInt64(size);
                    return;
                }
                
                _processCounters[processName].Sent += Convert.ToInt64(size);
            }
        }

        private void LogReceivedData(string processName, int size, bool isLocal)
        {
            lock (_processCounters)
            {
                if (!_processCounters.ContainsKey(processName)) return;

                if (isLocal)
                {
                    _processCounters[processName].ReceivedLocally += Convert.ToInt64(size);
                    return;
                }
                
                _processCounters[processName].Received += Convert.ToInt64(size);
            }
        }

        public List<NetworkPerformanceData> GetNetworkPerformanceData()
        {
            //var timeDifferenceInSeconds = (DateTime.Now - _mEtwStartTime).TotalSeconds;

            lock (_processCounters)
            {
                foreach (var counter in _processCounters.Values)
                {
                    var receivedDiff = counter.Received - _monitors[GetProcessName(counter.Process)].Remote.BytesReceived;
                    var sentDiff = counter.Sent - _monitors[GetProcessName(counter.Process)].Remote.BytesSent;
                    
                    _monitors[GetProcessName(counter.Process)].Remote.BytesReceived = counter.Received;
                    _monitors[GetProcessName(counter.Process)].Remote.BytesSent = counter.Sent;
                    _monitors[GetProcessName(counter.Process)].Remote.BandwidthReceived = receivedDiff;
                    _monitors[GetProcessName(counter.Process)].Remote.BandwidthSent = sentDiff;
                    _monitors[GetProcessName(counter.Process)].Remote.BytesReceivedLog.Add((DateTimeOffset.Now, counter.Received));
                    _monitors[GetProcessName(counter.Process)].Remote.BytesSentLog.Add((DateTimeOffset.Now, counter.Sent));
                    
                    var receivedDiffLocal = counter.ReceivedLocally - _monitors[GetProcessName(counter.Process)].Local.BytesReceived;
                    var sentDiffLocal = counter.SentLocally - _monitors[GetProcessName(counter.Process)].Local.BytesSent;
                    
                    _monitors[GetProcessName(counter.Process)].Local.BytesReceived = counter.ReceivedLocally;
                    _monitors[GetProcessName(counter.Process)].Local.BytesSent = counter.SentLocally;
                    _monitors[GetProcessName(counter.Process)].Local.BandwidthReceived = receivedDiffLocal;
                    _monitors[GetProcessName(counter.Process)].Local.BandwidthSent = sentDiffLocal;
                    _monitors[GetProcessName(counter.Process)].Local.BytesReceivedLog.Add((DateTimeOffset.Now, counter.ReceivedLocally));
                    _monitors[GetProcessName(counter.Process)].Local.BytesSentLog.Add((DateTimeOffset.Now, counter.SentLocally));
                }
            }

            CleanLogData();

            if (!_isContinuous)
                lock (_monitors)
                {
                    return _monitors.Values.ToList();
                }

            if ((DateTime.Now - _lastProcessUpdateTime).TotalSeconds >= _updateProcessInterval)
                UpdateProcesses();

            lock (_monitors)
            {
                return _monitors.Values.ToList();
            }
        }

        private void CleanLogData()
        {
            lock (_monitors)
            {
                foreach (var monitor in _monitors.Values)
                {
                    monitor.Remote.BytesReceivedLog.RemoveAll(x => (DateTimeOffset.Now - x.Time).TotalSeconds > _maxLogAge);
                    monitor.Remote.BytesSentLog.RemoveAll(x => (DateTimeOffset.Now - x.Time).TotalSeconds > _maxLogAge);
                    monitor.Local.BytesReceivedLog.RemoveAll(x => (DateTimeOffset.Now - x.Time).TotalSeconds > _maxLogAge);
                    monitor.Local.BytesSentLog.RemoveAll(x => (DateTimeOffset.Now - x.Time).TotalSeconds > _maxLogAge);
                }
            }
        }

        private void UpdateProcesses()
        {
            var processes = Process.GetProcesses();

            lock (_allProcesses)
            {
                _allProcesses.Clear();
                foreach (var p in processes)
                    _allProcesses.Add(p);
            }

            lock (_monitoredProcesses)
            {
                foreach (var process in processes)
                {
                    if (_monitoredProcesses.Contains(GetProcessName(process)))
                        continue;

                    AddProcess(process);
                }
                
                var processesToRemove = _monitoredProcesses
                    .Where(x => processes.All(p => x != GetProcessName(p)))
                    .ToList();
                
                foreach (var processName in processesToRemove)
                {
                    RemoveProcess(processName);
                }
            }

            _lastProcessUpdateTime = DateTime.Now;
        }

        private void ResetCounters()
        {
            lock (_processCounters)
            {
                foreach (var counter in _processCounters.Values)
                {
                    counter.Sent = 0;
                    counter.Received = 0;
                    counter.ReceivedLocally = 0;
                    counter.SentLocally = 0;
                }
            }

            //_mEtwStartTime = DateTime.Now;
        }

        public void Dispose()
        {
            _mEtwSession?.Dispose();
        }
    }

    public sealed class NetworkPerformanceData
    {
        public Process Process { get; set; }
        public TrafficData Local { get; set; } = new TrafficData();
        public TrafficData Remote { get; set; } = new TrafficData();
    }

    public sealed class TrafficData
    {
        public long BytesReceived { get; set; }
        public long BytesSent { get; set; }
        public long BandwidthReceived { get; set; }
        public long BandwidthSent { get; set; }
        public List<(DateTimeOffset Time, long Bytes)> BytesReceivedLog { get; set; } = new List<(DateTimeOffset Time, long Bytes)>();
        public List<(DateTimeOffset Time, long Bytes)> BytesSentLog { get; set; } = new List<(DateTimeOffset Time, long Bytes)>();

        public long TotalDownloadInWindow => 
            BytesReceivedLog.Count == 0 ? 0 : BytesReceivedLog.Last().Bytes - BytesReceivedLog.First().Bytes;
        public long TotalUploadInWindow => 
            BytesSentLog.Count == 0 ? 0 : BytesSentLog.Last().Bytes - BytesSentLog.First().Bytes;
    }
}