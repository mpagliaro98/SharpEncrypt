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

        public bool ContainsFile(string filepath)
        {
            foreach (FileEncryptor fileEncryptor in fileEncryptors)
            {
                if (fileEncryptor.Filepath.Equals(filepath))
                    return true;
            }
            return false;
        }

        public void RemoveFile(int idx)
        {
            fileEncryptors.RemoveAt(idx);
        }

        public void EncryptAllFiles(string password, bool encryptFilename, WorkTracker tracker = null)
        {
            foreach (FileEncryptor fileEncryptor in fileEncryptors)
            {
                if (tracker != null)
                    tracker.OutputBuffer.AppendText("Encrypting " + System.IO.Path.GetFileName(fileEncryptor.Filepath) + "... ");
                Stopwatch sw = new Stopwatch();
                sw.Start();
                bool result = fileEncryptor.EncryptFile(password, encryptFilename, tracker);
                sw.Stop();
                TimeSpan ts = sw.Elapsed;
                string timeElapsed = string.Format("{0}:{1}", ((int)Math.Floor(ts.TotalMinutes)).ToString("D2"), ts.ToString("ss\\.fff"));
                if (result)
                {
                    if (tracker != null)
                        tracker.OutputBuffer.AppendText("Success. (" + timeElapsed + ") --- " + fileEncryptor.Message);
                }
                else
                {
                    if (tracker != null)
                        tracker.OutputBuffer.AppendText(fileEncryptor.Message);
                }
                if (tracker != null)
                    tracker.OutputBuffer.AppendText("\n");
            }
        }

        public void DecryptAllFiles(string password, WorkTracker tracker = null)
        {
            foreach (FileEncryptor fileEncryptor in fileEncryptors)
            {
                if (tracker != null)
                    tracker.OutputBuffer.AppendText("Decrypting " + System.IO.Path.GetFileName(fileEncryptor.Filepath) + "... ");
                Stopwatch sw = new Stopwatch();
                sw.Start();
                bool result = fileEncryptor.DecryptFile(password, tracker);
                sw.Stop();
                TimeSpan ts = sw.Elapsed;
                string timeElapsed = string.Format("{0}:{1}", ((int)Math.Floor(ts.TotalMinutes)).ToString("D2"), ts.ToString("ss\\.fff"));
                if (result)
                {
                    if (tracker != null)
                        tracker.OutputBuffer.AppendText("Success. (" + timeElapsed + ") --- " + fileEncryptor.Message);
                }
                else
                {
                    if (tracker != null)
                        tracker.OutputBuffer.AppendText(fileEncryptor.Message);
                }
                if (tracker != null)
                    tracker.OutputBuffer.AppendText("\n");
            }
        }

        public void RemoveCompleteFiles()
        {
            List<FileEncryptor> toRemove = new List<FileEncryptor>();
            foreach (FileEncryptor fileEncryptor in fileEncryptors)
            {
                if (fileEncryptor.WorkComplete)
                    toRemove.Add(fileEncryptor);
            }
            foreach (var item in toRemove)
                fileEncryptors.Remove(item);
        }
    }
}
