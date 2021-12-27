using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpEncrypt
{
    public interface IBufferable
    {
        void InvokeOutputThread(Action callback);
    }
}
