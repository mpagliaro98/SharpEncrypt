using System;
using System.Collections.Generic;
using System.Linq;
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
        }

        public virtual void ClearText()
        {
            Text = "";
        }
    }
}
