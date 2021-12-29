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

        public bool IsDirectory
        {
            get { return Util.IsDirectory(filepath); }
        }

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
            get { return IsDirectory ?
                    Util.DirectorySize(new DirectoryInfo(FullPath)) :
                    new System.IO.FileInfo(filepath).Length; }
        }

        public double FileSizeKB
        {
            get { return (double)FileSizeBytes / 1024; }
        }

        public double FileSizeMB
        {
            get { return (double)FileSizeBytes / (1024 * 1024); }
        }

        public int NumChildFiles
        {
            get { return Directory.EnumerateFiles(filepath, "*", SearchOption.AllDirectories).Count(); }
        }

        public FileInfo(string filepath)
        {
            this.filepath = filepath;
        }

        public string GeneratePreviewText()
        {
            if (IsDirectory)
                return FullPath + " (" + NumChildFiles + " files, " + string.Format("{0:0.00}", FileSizeMB) + "MB)";
            else
                return FileName + " (" + string.Format("{0:0.00}", FileSizeMB) + "MB)";
        }
    }
}
