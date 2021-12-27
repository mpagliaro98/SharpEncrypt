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
            Text += text;
        }

        public virtual void ClearText()
        {
            Text = "";
        }
    }
}
