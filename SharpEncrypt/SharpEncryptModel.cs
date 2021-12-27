using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

        public void EncryptAllFiles(string password, bool encryptFilename, WorkTracker tracker = null, OutputBuffer buffer = null)
        {
            foreach (FileEncryptor fileEncryptor in fileEncryptors)
            {
                if (buffer != null)
                    buffer.AppendText("Encrypting " + System.IO.Path.GetFileName(fileEncryptor.Filepath) + "... ");
                Stopwatch sw = new Stopwatch();
                sw.Start();
                bool result = fileEncryptor.EncryptFile(password, encryptFilename, tracker);
                sw.Stop();
                TimeSpan ts = sw.Elapsed;
                string timeElapsed = string.Format("{0}:{1}", Math.Floor(ts.TotalMinutes), ts.ToString("ss\\.ff"));
                if (result)
                {
                    if (buffer != null)
                        buffer.AppendText("Success. (" + timeElapsed + ")");
                }
                else
                {
                    if (buffer != null)
                        buffer.AppendText("Something went wrong.");
                }
                if (buffer != null)
                    buffer.AppendText("\n");
            }
        }

        public void DecryptAllFiles(string password, WorkTracker tracker = null, OutputBuffer buffer = null)
        {
            foreach (FileEncryptor fileEncryptor in fileEncryptors)
            {
                if (buffer != null)
                    buffer.AppendText("Decrypting " + System.IO.Path.GetFileName(fileEncryptor.Filepath) + "... ");
                Stopwatch sw = new Stopwatch();
                sw.Start();
                bool result = fileEncryptor.DecryptFile(password, tracker);
                sw.Stop();
                TimeSpan ts = sw.Elapsed;
                string timeElapsed = string.Format("{0}:{1}", Math.Floor(ts.TotalMinutes), ts.ToString("ss\\.ff"));
                if (result)
                {
                    if (buffer != null)
                        buffer.AppendText("Success. (" + timeElapsed + ")");
                }
                else
                {
                    if (buffer != null)
                        buffer.AppendText(fileEncryptor.Message);
                }
                if (buffer != null)
                    buffer.AppendText("\n");
            }
        }
    }
}
