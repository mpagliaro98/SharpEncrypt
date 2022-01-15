using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpEncrypt
{
    public abstract class FileEncryptorBase
    {
        protected const string EXT_ENCRYPTED = ".senc";

        public abstract string Filepath { get; }
        public abstract bool WorkComplete { get; }

        public abstract bool ContainsFile(string filepath);
        public abstract bool Encrypt(byte[] masterKey, string password, EncryptOptions options, WorkTracker tracker);
        public abstract bool Decrypt(byte[] masterKey, string password, EncryptOptions options, WorkTracker tracker);
    }
}
