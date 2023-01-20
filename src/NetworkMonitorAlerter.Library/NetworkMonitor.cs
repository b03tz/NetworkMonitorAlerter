using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing.Parsers;
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

        private readonly List<Process> _processes = new List<Process>();
        private readonly HashSet<Process> _allProcesses = new HashSet<Process>();
        private readonly bool _isContinuous;
        private readonly int _updateProcessInterval;
        private readonly int _maxLogAge;

        private class Counters
        {
            public Process Process;
            public long Received;
            public long Sent;
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
            lock (_processes)
            {
                return GetProcessName(_allProcesses.FirstOrDefault(x => x.Id == processId));
            }
        }

        public void AddProcess(Process process)
        {
            if (process == null)
                throw new ArgumentNullException(nameof(process));

            lock (_processCounters)
            {
                if (_processCounters.ContainsKey(GetProcessName(process)))
                    return;
            }

            lock (_processes)
            {
                _processes.Add(process);
            }

            lock (_processCounters)
            {
                _processCounters.Add(GetProcessName(process), new Counters
                {
                    Process = process,
                    Received = 0,
                    Sent = 0
                });
            }

            lock (_monitors)
            {
                _monitors.Add(GetProcessName(process), new NetworkPerformanceData
                {
                    Process = process
                });
            }
        }

        public void RemoveProcess(string processName)
        {
            lock (_processes)
            {
                _processes.RemoveAll(x => GetProcessName(x) == processName);
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
                    _mEtwSession.Source.Kernel.TcpIpRecv += data =>
                    {
                        lock (_processCounters)
                        {
                            if (!_processCounters.ContainsKey(GetProcessName(data.ProcessID))) return;
                            
                            _processCounters[GetProcessName(data.ProcessID)].Received += Convert.ToInt64(data.size);
                        }
                    };

                    _mEtwSession.Source.Kernel.TcpIpSend += (data) => LogSentData(GetProcessName(data.ProcessID), data.size);
                    _mEtwSession.Source.Kernel.TcpIpRecv += (data) => LogReceivedData(GetProcessName(data.ProcessID), data.size);
                    _mEtwSession.Source.Kernel.TcpIpSendIPV6 += (data) => LogSentData(GetProcessName(data.ProcessID), data.size);
                    _mEtwSession.Source.Kernel.TcpIpRecvIPV6 += (data) => LogReceivedData(GetProcessName(data.ProcessID), data.size);
                    _mEtwSession.Source.Kernel.UdpIpSend += (data) => LogSentData(GetProcessName(data.ProcessID), data.size);
                    _mEtwSession.Source.Kernel.UdpIpRecv += (data) => LogReceivedData(GetProcessName(data.ProcessID), data.size);
                    _mEtwSession.Source.Kernel.UdpIpSendIPV6 += (data) => LogSentData(GetProcessName(data.ProcessID), data.size);
                    _mEtwSession.Source.Kernel.UdpIpRecvIPV6 += (data) => LogReceivedData(GetProcessName(data.ProcessID), data.size);

                    _mEtwSession.Source.Process();
                }
            }
            catch
            {
                ResetCounters(); // Stop reporting figures
                // Probably should log the exception
            }
        }
        
        private void LogSentData(string processName, int size)
        {
            lock (_processCounters)
            {
                if (!_processCounters.ContainsKey(processName)) return;
                            
                _processCounters[processName].Sent += Convert.ToInt64(size);
            }
        }

        private void LogReceivedData(string processName, int size)
        {
            lock (_processCounters)
            {
                if (!_processCounters.ContainsKey(processName)) return;
                            
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
                    var receivedDiff = counter.Received - _monitors[GetProcessName(counter.Process)].BytesReceived;
                    var sentDiff = counter.Sent - _monitors[GetProcessName(counter.Process)].BytesSent;
                    
                    _monitors[GetProcessName(counter.Process)].BytesReceived = counter.Received;
                    _monitors[GetProcessName(counter.Process)].BytesSent = counter.Sent;
                    _monitors[GetProcessName(counter.Process)].BandwidthReceived = receivedDiff;
                    _monitors[GetProcessName(counter.Process)].BandwidthSent = sentDiff;
                    _monitors[GetProcessName(counter.Process)].BytesReceivedLog.Add((DateTimeOffset.Now, counter.Received));
                    _monitors[GetProcessName(counter.Process)].BytesSentLog.Add((DateTimeOffset.Now, counter.Sent));
                }
            }

            if (!_isContinuous)
                lock (_monitors)
                {
                    return _monitors.Values.ToList();
                }

            if ((DateTime.Now - _lastProcessUpdateTime).TotalSeconds >= _updateProcessInterval)
                UpdateProcesses();

            lock (_monitors)
            {
                CleanLogData();
                return _monitors.Values.ToList();
            }
        }

        private void CleanLogData()
        {
            foreach (var monitor in _monitors.Values)
            {
                monitor.BytesReceivedLog.RemoveAll(x => (DateTimeOffset.Now - x.Time).TotalSeconds > _maxLogAge);
                monitor.BytesSentLog.RemoveAll(x => (DateTimeOffset.Now - x.Time).TotalSeconds > _maxLogAge);
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

            lock (_processes)
            {
                foreach (var process in processes)
                {
                    if (_processes.Any(x => GetProcessName(x) == GetProcessName(process)))
                        continue;

                    AddProcess(process);
                }
                
                var processesToRemove = _processes
                    .Where(x => processes.All(p => GetProcessName(p) != GetProcessName(x)))
                    .Select(GetProcessName)
                    .Distinct()
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