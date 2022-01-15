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
        private List<FileEncryptorBase> fileEncryptors = new List<FileEncryptorBase>();

        public List<FileInfo> Files
        {
            get
            {
                List<FileInfo> fileList = new List<FileInfo>();
                foreach (FileEncryptorBase file in fileEncryptors)
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

        public void AddFolder(string path)
        {
            fileEncryptors.Add(new FolderEncryptor(path));
        }

        public bool ContainsFile(string filepath)
        {
            foreach (FileEncryptorBase fileEncryptor in fileEncryptors)
            {
                if (fileEncryptor.ContainsFile(filepath))
                    return true;
            }
            return false;
        }

        public void RemoveFile(int idx)
        {
            fileEncryptors.RemoveAt(idx);
        }

        public void EncryptAllFiles(byte[] masterKey, string password, EncryptOptions options, WorkTracker tracker = null)
        {
            foreach (FileEncryptorBase fileEncryptor in fileEncryptors)
                fileEncryptor.Encrypt(masterKey, password, options, tracker);
        }

        public void DecryptAllFiles(byte[] masterKey, string password, EncryptOptions options, WorkTracker tracker = null)
        {
            foreach (FileEncryptorBase fileEncryptor in fileEncryptors)
                fileEncryptor.Decrypt(masterKey, password, options, tracker);
        }

        public void RemoveCompleteFiles()
        {
            List<FileEncryptorBase> toRemove = new List<FileEncryptorBase>();
            foreach (FileEncryptorBase fileEncryptor in fileEncryptors)
            {
                if (fileEncryptor.WorkComplete)
                    toRemove.Add(fileEncryptor);
            }
            foreach (var item in toRemove)
                fileEncryptors.Remove(item);
        }
    }
}
