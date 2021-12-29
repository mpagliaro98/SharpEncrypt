using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpEncrypt
{
    public class FolderEncryptor : FileEncryptorBase
    {
        private const string DIR_NAME_PREFIX = "senc%";

        private List<FileEncryptorBase> children = new List<FileEncryptorBase>();
        private string filepath = "";
        private bool workComplete = false;

        public override string Filepath
        {
            get { return filepath; }
        }

        public string DirectoryName
        {
            get { return new DirectoryInfo(filepath).Name; }
        }

        public override bool WorkComplete
        {
            get { return workComplete; }
        }

        public FolderEncryptor(string path)
        {
            filepath = path;
            foreach (string childpath in Directory.GetFiles(path))
                children.Add(new FileEncryptor(childpath));
            foreach (string childpath in Directory.GetDirectories(path))
                children.Add(new FolderEncryptor(childpath));
        }

        public override bool ContainsFile(string filepath)
        {
            if (this.filepath.Equals(filepath))
                return true;
            foreach (FileEncryptorBase fileEncryptor in children)
            {
                if (fileEncryptor.ContainsFile(filepath))
                    return true;
            }
            return false;
        }

        public override bool Encrypt(string password, bool encryptFilename, WorkTracker tracker)
        {
            workComplete = false;
            if (tracker != null)
                tracker.OutputBuffer.AppendText("Encrypting directory " + filepath + "...\n");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            string encDirName = DirectoryName;
            try
            {
                foreach (FileEncryptorBase fileEncryptor in children)
                    fileEncryptor.Encrypt(password, encryptFilename, tracker);

                AesCryptographyService aes = new AesCryptographyService();
                byte[] encrypted = aes.Encrypt(Util.StringEncoding.GetBytes(DirectoryName), password);
                encDirName = DIR_NAME_PREFIX + Util.ToBase64StringSafe(encrypted);
                Directory.Move(Filepath, Path.Combine(Directory.GetParent(Filepath).FullName, encDirName));
            }
            catch (Exception e)
            {
                if (tracker != null)
                {
                    tracker.OutputBuffer.AppendText("====================================\n");
                    tracker.OutputBuffer.AppendText("ERROR --- " + e.Message + "\n");
                    tracker.OutputBuffer.AppendText("====================================\n");
                }
                return false;
            }

            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            string timeElapsed = string.Format("{0}:{1}", ((int)Math.Floor(ts.TotalMinutes)).ToString("D2"), ts.ToString("ss\\.fff"));
            if (tracker != null)
                tracker.OutputBuffer.AppendText("Finished encrypting directory " + filepath + " as " + encDirName + " (" + timeElapsed + ")\n");
            workComplete = true;
            return true;
        }

        public override bool Decrypt(string password, WorkTracker tracker)
        {
            workComplete = false;
            if (tracker != null)
                tracker.OutputBuffer.AppendText("Decrypting directory " + filepath + "...\n");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            string dirName = DirectoryName;
            try
            {
                foreach (FileEncryptorBase fileEncryptor in children)
                    fileEncryptor.Decrypt(password, tracker);
                
                if (dirName.StartsWith(DIR_NAME_PREFIX))
                {
                    string encDirName = dirName.Substring(DIR_NAME_PREFIX.Length);
                    byte[] encrypted = Util.FromBase64StringSafe(encDirName);
                    AesCryptographyService aes = new AesCryptographyService();
                    byte[] decrypted = aes.Decrypt(encrypted, password);
                    dirName = Util.StringEncoding.GetString(decrypted).Replace("\0", String.Empty);
                    Directory.Move(Filepath, Path.Combine(Directory.GetParent(Filepath).FullName, dirName));
                }
            }
            catch (Exception e)
            {
                if (tracker != null)
                {
                    tracker.OutputBuffer.AppendText("====================================\n");
                    tracker.OutputBuffer.AppendText("ERROR --- " + e.Message + "\n");
                    tracker.OutputBuffer.AppendText("====================================\n");
                }
                return false;
            }
            
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            string timeElapsed = string.Format("{0}:{1}", ((int)Math.Floor(ts.TotalMinutes)).ToString("D2"), ts.ToString("ss\\.fff"));
            if (tracker != null)
                tracker.OutputBuffer.AppendText("Finished decrypting directory " + filepath + " as " + dirName + " (" + timeElapsed + ")\n");
            workComplete = true;
            return true;
        }
    }
}
