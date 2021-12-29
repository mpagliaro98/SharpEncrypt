using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SharpEncrypt
{
    public class TextBlockBufferable : TextBlock, IBufferable
    {
        protected const int MAX_LENGTH = 10000;
        private const string DIR_NAME = "logs";

        private bool logToFile = false;
        private bool operationComplete = false;
        private StreamWriter sw = null;

        public bool LogToFile
        {
            get { return logToFile; }
            set { logToFile = value; }
        }

        public bool OperationComplete
        {
            get { return operationComplete; }
            set
            {
                operationComplete = value;
                if (sw != null && operationComplete)
                {
                    sw.Close();
                    sw = null;
                }
            }
        }

        static TextBlockBufferable()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBlockBufferable), new FrameworkPropertyMetadata(typeof(TextBlockBufferable)));
        }

        public void InvokeOutputThread(Action callback)
        {
            Dispatcher.Invoke(callback);
        }

        public virtual void AppendText(string text)
        {
            if (Text.Length + text.Length >= MAX_LENGTH)
                Text = Text.Substring(MAX_LENGTH / 10);
            Text += text;
            if (LogToFile)
            {
                if (sw == null)
                {
                    string filename = GetFilename();
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filename));
                    sw = File.CreateText(filename);
                }
                sw.Write(text);
            }
        }

        public virtual void ClearText()
        {
            Text = "";
        }

        private string GetFilename()
        {
            string dir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string result = System.IO.Path.Combine(dir, DIR_NAME);
            string filename = "log_" + DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss") + ".txt";
            result = System.IO.Path.Combine(result, filename);
            return result;
        }
    }
}
