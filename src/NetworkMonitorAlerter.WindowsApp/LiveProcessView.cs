using System;
using System.Diagnostics;
using System.Windows.Forms;
using LiveCharts;
using LiveCharts.Wpf;
using NetworkMonitorAlerter.Library;
using CartesianChart = LiveCharts.WinForms.CartesianChart;

namespace NetworkMonitorAlerter.WindowsApp
{
    public sealed partial class LiveProcessView : Form
    {
        private readonly MainAppForm _mainAppForm;
        public readonly string ProcessName;
        private readonly CartesianChart _chart;
        private readonly ChartValues<decimal> _uploadValues = new ChartValues<decimal>();
        private readonly ChartValues<decimal> _downloadValues = new ChartValues<decimal>();
        private readonly ChartValues<decimal> _uploadValuesNetwork = new ChartValues<decimal>();
        private readonly ChartValues<decimal> _downloadValuesNetwork = new ChartValues<decimal>();

        public LiveProcessView(MainAppForm mainAppForm, string processName)
        {
            _mainAppForm = mainAppForm;
            ProcessName = processName;
            InitializeComponent();

            _chart = new CartesianChart
            {
                AxisY = new AxesCollection
                {
                    new Axis
                    {
                        MinValue = 0,
                        LabelFormatter = d =>
                        {
                            return d.ToString("#,0") + "kb";
                        } 
                    }
                }
            };
            
            _chart.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Download internet",
                    Values = _downloadValues,
                },
                new LineSeries
                {
                    Title = "Upload internet",
                    Values = _uploadValues
                },
                new LineSeries
                {
                    Title = "Download local",
                    Values = _downloadValuesNetwork
                },
                new LineSeries
                {
                    Title = "Upload local",
                    Values = _uploadValuesNetwork
                },
            };

            _chart.Left = 10;
            _chart.Top = 10;
            _chart.Width = Width - 30;
            _chart.Height = Height - 60;
            this.Controls.Add(_chart);
            this.Text = "Watching " + processName;
        }

        public void AddValue(long bytes, DownloadOrUpload type)
        {
            var kb = Math.Round(Convert.ToDecimal(bytes) / 1024, 2);
            if (type == DownloadOrUpload.Download)
            {
                _downloadValues.Add(kb);
                return;
            }
            
            _uploadValues.Add(kb);
        }
        
        public void AddValueNetwork(long bytes, DownloadOrUpload type)
        {
            var kb = Math.Round(Convert.ToDecimal(bytes) / 1024, 2);
            if (type == DownloadOrUpload.Download)
            {
                _downloadValuesNetwork.Add(kb);
                return;
            }
            
            _uploadValuesNetwork.Add(kb);
        }

        private void LiveProcessView_FormClosing(object sender, FormClosingEventArgs e)
        {
            _mainAppForm.LiveProcessViews.Remove(this);
        }

        private void LiveProcessView_SizeChanged(object sender, EventArgs e)
        {
            _chart.Width = Width - 30;
            _chart.Height = Height - 60;
        }
    }
}