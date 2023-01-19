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

        private readonly Dictionary<int, Counters> _processCounters = new Dictionary<int, Counters>();

        private readonly Dictionary<int, NetworkPerformanceData> _monitors =
            new Dictionary<int, NetworkPerformanceData>();

        private readonly List<Process> _processes = new List<Process>();
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
                networkPerformancePresenter.AddProcess(process);

            networkPerformancePresenter.Initialize();
            return networkPerformancePresenter;
        }

        public static NetworkMonitor CreateContinuousMonitor(int updateInterval = 30, int maxLogAge = 300)
        {
            var networkPerformancePresenter = new NetworkMonitor(true, updateInterval, maxLogAge);
            networkPerformancePresenter.Initialize();
            return networkPerformancePresenter;
        }

        public void AddProcess(Process process)
        {
            if (process == null)
                throw new ArgumentNullException(nameof(process));

            lock (_processCounters)
            {
                if (_processCounters.ContainsKey(process.Id))
                    return;
            }

            lock (_processes)
            {
                _processes.Add(process);
            }

            lock (_processCounters)
            {
                _processCounters.Add(process.Id, new Counters
                {
                    Process = process,
                    Received = 0,
                    Sent = 0
                });
            }

            lock (_monitors)
            {
                _monitors.Add(process.Id, new NetworkPerformanceData
                {
                    Process = process
                });
            }
        }

        public void RemoveProcess(int processId)
        {
            lock (_processes)
            {
                var p = _processes.FirstOrDefault(x => x.Id == processId);
                if (p != null)
                    _processes.Remove(p);
            }

            lock (_processCounters)
            {
                _processCounters.Remove(processId);
            }

            lock (_monitors)
            {
                _monitors.Remove(processId);
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
                            if (!_processCounters.ContainsKey(data.ProcessID)) return;
                            
                            _processCounters[data.ProcessID].Received += Convert.ToInt64(data.size);
                        }
                    };

                    _mEtwSession.Source.Kernel.TcpIpSend += data =>
                    {
                        
                        lock (_processCounters)
                        {
                            if (!_processCounters.ContainsKey(data.ProcessID)) return;
                            
                            _processCounters[data.ProcessID].Sent += Convert.ToInt64(data.size);
                        }
                    };

                    _mEtwSession.Source.Process();
                }
            }
            catch
            {
                ResetCounters(); // Stop reporting figures
                // Probably should log the exception
            }
        }
        
        public List<NetworkPerformanceData> GetNetworkPerformanceData()
        {
            //var timeDifferenceInSeconds = (DateTime.Now - _mEtwStartTime).TotalSeconds;

            lock (_processCounters)
            {
                foreach (var counter in _processCounters.Values)
                {
                    var receivedDiff = counter.Received - _monitors[counter.Process.Id].BytesReceived;
                    var sentDiff = counter.Sent - _monitors[counter.Process.Id].BytesSent;
                    
                    _monitors[counter.Process.Id].BytesReceived = counter.Received;
                    _monitors[counter.Process.Id].BytesSent = counter.Sent;
                    _monitors[counter.Process.Id].BandwidthReceived = receivedDiff;
                    _monitors[counter.Process.Id].BandwidthSent = sentDiff;
                    _monitors[counter.Process.Id].BytesReceivedLog.Add((DateTimeOffset.Now, counter.Received));
                    _monitors[counter.Process.Id].BytesSentLog.Add((DateTimeOffset.Now, counter.Sent));
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

            lock (_processes)
            {
                foreach (var process in processes)
                {
                    if (_processes.Any(x => x.Id == process.Id))
                        continue;

                    AddProcess(process);
                }

                var processesToRemove = _processes
                    .Where(x => processes.All(p => p.Id != x.Id))
                    .Select(p => p.Id)
                    .ToList();
                
                foreach (var processId in processesToRemove)
                {
                    RemoveProcess(processId);
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