using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SharpEncrypt
{
    public class OutputBuffer
    {
        private IBufferable outputObject;

        public OutputBuffer(IBufferable outputObject)
        {
            this.outputObject = outputObject;
        }

        public void AppendText(string text)
        {
            outputObject.InvokeOutputThread(new Action(() => outputObject.AppendText(text)));
        }

        public void ClearText()
        {
            outputObject.InvokeOutputThread(new Action(() => outputObject.ClearText()));
        }
    }
}
