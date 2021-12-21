using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpEncrypt
{
    public class FileInfo
    {
        private string filepath = "";

        public string FullPath
        {
            get { return filepath; }
        }

        public string FileName
        {
            get { return Path.GetFileName(filepath); }
        }

        public string FileNameWithoutExtension
        {
            get { return Path.GetFileNameWithoutExtension(filepath); }
        }

        public string FileDirectory
        {
            get { return Path.GetDirectoryName(filepath); }
        }

        public string Extension
        {
            get { return Path.GetExtension(filepath); }
        }

        public long FileSizeBytes
        {
            get { return new System.IO.FileInfo(filepath).Length; }
        }

        public double FileSizeKB
        {
            get { return (double)FileSizeBytes / 1024; }
        }

        public double FileSizeMB
        {
            get { return (double)FileSizeBytes / (1024 * 1024); }
        }

        public FileInfo(string filepath)
        {
            this.filepath = filepath;
        }
    }
}
