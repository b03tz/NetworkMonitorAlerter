using System.Collections.Generic;
using System.Windows.Forms;

namespace NetworkMonitorAlerter.Library
{
    public static class TextBoxLogger
    {
        public static TextBox TextBox { get; set; }
        private static List<string> LogMessages { get; set; } = new List<string>();
        
        public static void Log(string logMessage)
        {
            if (TextBox == null)
            {
                LogMessages.Add(logMessage);
                return;
            }
            
            foreach(var message in LogMessages)
            {
                TextBox.AppendText("\r\n" + message);
            }

            LogMessages.Clear();
            
            TextBox.AppendText("\r\n" + logMessage);
        }
        
        public static string ToFixedString(string text, int length)
        {
            if (text.Length >= length)
                return text;
            
            var spaces = length - text.Length;
            for (var i = 0; i < spaces; i++)
            {
                text = " " + text;
            }

            return text;
        }
    }
}