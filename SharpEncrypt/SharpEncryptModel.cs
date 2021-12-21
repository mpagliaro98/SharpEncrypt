using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpEncrypt
{
    public class SharpEncryptModel
    {
        private List<FileEncryptor> fileEncryptors = new List<FileEncryptor>();

        public List<FileInfo> Files
        {
            get
            {
                List<FileInfo> fileList = new List<FileInfo>();
                foreach (FileEncryptor file in fileEncryptors)
                {
                    fileList.Add(new FileInfo(file.Filepath));
                }
                return fileList;
            }
        }

        public int NumFiles
        {
            get { return fileEncryptors.Count; }
        }

        public void ClearFiles()
        {
            fileEncryptors.Clear();
        }

        public void AddFile(string filepath)
        {
            fileEncryptors.Add(new FileEncryptor(filepath));
        }

        public string EncryptAllFiles(string password, bool encryptFilename, BackgroundWorker worker = null)
        {
            string output = "";
            foreach (FileEncryptor fileEncryptor in fileEncryptors)
            {
                output += System.IO.Path.GetFileName(fileEncryptor.Filepath) + ": ";
                var result = fileEncryptor.EncryptFile(password, encryptFilename, worker);
                if (result)
                    output += "Successfully encrypted.";
                else
                    output += "Something went wrong.";
                output += "\n";
            }
            return output;
        }

        public string DecryptAllFiles(string password, BackgroundWorker worker = null)
        {
            string output = "";
            foreach (FileEncryptor fileEncryptor in fileEncryptors)
            {
                output += System.IO.Path.GetFileName(fileEncryptor.Filepath) + ": ";
                var result = fileEncryptor.DecryptFile(password, worker);
                if (result)
                    output += "Successfully decrypted.";
                else
                    output += fileEncryptor.Message;
                output += "\n";
            }
            return output;
        }
    }
}
