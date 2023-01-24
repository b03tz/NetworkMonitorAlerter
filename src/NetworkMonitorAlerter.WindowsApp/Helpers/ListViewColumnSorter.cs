using System;
using System.Collections;
using System.Windows.Forms;

namespace NetworkMonitorAlerter.WindowsApp.Helpers
{
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