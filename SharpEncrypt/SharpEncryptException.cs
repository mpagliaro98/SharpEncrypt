using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpEncrypt
{
    public class SharpEncryptException : Exception
    {
        public SharpEncryptException() : base() { }

        public SharpEncryptException(string message) : base(message) { }

        public SharpEncryptException(string message, Exception innerException) : base(message, innerException) { }
    }
}
