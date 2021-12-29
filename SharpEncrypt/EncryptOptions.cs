using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpEncrypt
{
    public class EncryptOptions
    {
        private bool encryptFilename = true;
        private bool encryptDirname = true;

        public bool EncryptFilename
        {
            get { return encryptFilename; }
            set { encryptFilename = value; }
        }

        public bool EncryptDirectoryName
        {
            get { return encryptDirname; }
            set { encryptDirname = value; }
        }

        public EncryptOptions() { }
    }
}
