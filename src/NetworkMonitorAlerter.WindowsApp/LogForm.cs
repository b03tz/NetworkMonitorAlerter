using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NetworkMonitorAlerter.Library;
using NetworkMonitorAlerter.WindowsApp.Helpers;

namespace NetworkMonitorAlerter.WindowsApp
{
    public partial class LogForm : Form
    {
        private readonly List<BandwidthLogger> _loggers;
        private readonly MainAppForm _mainForm;
        private readonly ListViewColumnSorter columnSorter;

        public LogForm(List<BandwidthLogger> loggers, MainAppForm mainForm)
        {
            _loggers = loggers;
            _mainForm = mainForm;
            InitializeComponent();

            listLogViewer.View = View.Details;
            listLogViewer.Columns.Add(new ColumnHeader
            {
                Text = "Process",
                Name = "col1",
                Width = 250,
            });
            listLogViewer.Columns.Add(new ColumnHeader
            {
                Text = "Bandwidth downloaded (MB)",
                Name = "col2",
                Width = 200
            });
            listLogViewer.Columns.Add(new ColumnHeader
            {
                Text = "Bandwidth uploaded (MB)",
                Name = "col3",
                Width = 200
            });

            columnSorter = new ListViewColumnSorter();
            listLogViewer.ListViewItemSorter = columnSorter;
            listLogViewer.ColumnClick += ListLogViewerOnColumnClick;            
            this.Text = "Logviewer - Daily";
            ReadLog(LoggerType.Daily);
        }

        private void ListLogViewerOnColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == columnSorter.Column)
            {
                // Reverse the current sort direction for this column.
                if (columnSorter.Order == SortOrder.Ascending)
                {
                    columnSorter.Order = SortOrder.Descending;
                    listLogViewer.Sort();
                    return;
                }

                columnSorter.Order = SortOrder.Ascending;
                listLogViewer.Sort();
                return;
            }

            // Set the column number that is to be sorted; default to ascending.
            columnSorter.Column = e.Column;
            columnSorter.Order = SortOrder.Ascending;
            listLogViewer.Sort();
        }

        private void ReadLog(LoggerType type)
        {
            listLogViewer.Items.Clear();
            var logger = _loggers.First(x => x.Type == type);
            foreach (var application in logger.GetLog().Applications)
            {
                var listItem = new ListViewItem(application.ApplicationName);
                listItem.SubItems.Add(StringHelpers.ToMegabytes(application.TotalBytesDownloaded));
                listItem.SubItems.Add(StringHelpers.ToMegabytes(application.TotalBytesUploaded));
                listLogViewer.Items.Add(listItem);
            }
        }

        private void LogForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _mainForm.ShowLogButton.Enabled = true;
        }

        private void tabLogView_SelectedIndexChanged(object sender, EventArgs e)
        {
            var index = tabLogView.SelectedIndex;
            //tabLogView.TabPages[0].Controls.Add(listLogViewer);
            listLogViewer.Parent = tabLogView.TabPages[index];

            switch (index)
            {
                case 0:
                    this.Text = "Logviewer - Daily";
                    ReadLog(LoggerType.Daily);
                    break;
                case 1:
                    this.Text = "Logviewer - Weekly";
                    ReadLog(LoggerType.Weekly);
                    break;
                case 2:
                    this.Text = "Logviewer - Monthly";
                    ReadLog(LoggerType.Monthly);
                    break;
            }
        }
    }
}