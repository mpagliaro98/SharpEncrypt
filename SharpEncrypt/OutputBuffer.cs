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
        private Action<IBufferable, string> updateObject;

        public OutputBuffer(IBufferable outputObject, Action<IBufferable, string> updateObject)
        {
            this.outputObject = outputObject;
            this.updateObject = updateObject;
        }

        public void AppendText(string text)
        {
            outputObject.InvokeOutputThread(new Action(() => updateObject(outputObject, text)));
        }
    }
}
