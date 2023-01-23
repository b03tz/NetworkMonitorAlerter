using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NetworkMonitorAlerter.Library;

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
                listItem.SubItems.Add(ToMegabytes(application.TotalBytesDownloaded));
                listItem.SubItems.Add(ToMegabytes(application.TotalBytesUploaded));
                listLogViewer.Items.Add(listItem);
            }
        }

        private string ToMegabytes(long totalBytes)
        {
            var mb = Convert.ToDecimal(totalBytes);
            var converted = Math.Round(mb / 1024 / 1024, 2, MidpointRounding.ToEven);
            return converted.ToString().Replace(",", ".") + " MB";
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

    public class ListViewColumnSorter : IComparer
    {
        public int Column { get; set; }
        public SortOrder Order { get; set; }
        private CaseInsensitiveComparer _objectCompare;
        public ListViewColumnSorter()
        {
            Column = 0;
            Order = SortOrder.None;
            _objectCompare = new CaseInsensitiveComparer();
        }

        public int Compare(object x, object y)
        {
            int compareResult;
            ListViewItem listviewX, listviewY;

            // Cast the objects to be compared to ListViewItem objects
            listviewX = (ListViewItem) x;
            listviewY = (ListViewItem) y;

            if (Column > 0)
            {
                var itemA = Convert.ToDecimal(listviewX.SubItems[Column].Text.Replace(" MB", ""));
                var itemB = Convert.ToDecimal(listviewY.SubItems[Column].Text.Replace(" MB", ""));
                
                if (itemA > itemB)
                    return Order == SortOrder.Ascending ? 1 : -1;
                if (itemA < itemB)
                    return Order == SortOrder.Ascending ? -1 : 1;

                return 0;
            }
            
            var a = listviewX.SubItems[Column].Text;
            var b = listviewY.SubItems[Column].Text;
            
            // Compare the two items
            compareResult = _objectCompare.Compare(a, b);

            // Calculate correct return value based on object comparison
            if (Order == SortOrder.Ascending)
            {
                // Ascending sort is selected, return normal result of compare operation
                return compareResult;
            }

            if (Order == SortOrder.Descending)
            {
                // Descending sort is selected, return negative result of compare operation
                return (-compareResult);
            }

            // Return '0' to indicate they are equal
            return 0;
        }
    }
}