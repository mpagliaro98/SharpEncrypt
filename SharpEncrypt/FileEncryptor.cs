using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpEncrypt
{
    public class FileEncryptor : FileEncryptorBase
    {
        private string filepath = "";
        private byte[] loadedFile;
        private Header header = new Header();
        private bool workComplete = false;

        public override string Filepath
        {
            get { return filepath; }
        }

        public Header Header
        {
            get { return header; }
        }

        public override bool WorkComplete
        {
            get { return workComplete; }
        }

        public FileEncryptor(string filepath)
        {
            if (File.Exists(filepath))
                loadedFile = File.ReadAllBytes(filepath);
            this.filepath = filepath;
        }

        private bool ValidateChecksum(byte[] masterKey, string password)
        {
            AesCryptographyService aes = new AesCryptographyService();
            byte[] encrypted = aes.Encrypt(Util.StringEncoding.GetBytes(password), masterKey);
            return encrypted.SequenceEqual(header.Checksum);
        }

        public override bool ContainsFile(string filepath)
        {
            return this.filepath.Equals(filepath);
        }

        public override bool Encrypt(byte[] masterKey, string password, EncryptOptions options, WorkTracker tracker)
        {
            workComplete = false;
            if (tracker != null)
                tracker.OutputBuffer.AppendText("Encrypting " + Path.GetFileName(filepath) + "... ");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                // Make sure encrypted result is a size divisible by the block size
                byte[] result = new byte[loadedFile.Length % AesCryptographyService.DEFAULT_BLOCK_SIZE == 0 ? loadedFile.Length :
                    loadedFile.Length + (AesCryptographyService.DEFAULT_BLOCK_SIZE - (loadedFile.Length % AesCryptographyService.DEFAULT_BLOCK_SIZE))];

                header.SetPassword(masterKey, password);
                header.OriginalFilesize = loadedFile.Length;
                string filename = Path.GetFileName(filepath);
                header.SetFileExtension(Path.GetExtension(filename), password);
                header.SetFileName(Path.GetFileNameWithoutExtension(filename), password);

                var aes = new AesCryptographyService();
                for (int i = 0; i < loadedFile.Length; i += AesCryptographyService.DEFAULT_BLOCK_SIZE)
                {
                    byte[] current = loadedFile.RangeSubset(i, AesCryptographyService.DEFAULT_BLOCK_SIZE);
                    byte[] encrypted = aes.Encrypt(current, password);
                    result.OverwriteSubset(encrypted, i);

                    if (tracker != null)
                        tracker.ReportProgress((double)(i + AesCryptographyService.DEFAULT_BLOCK_SIZE) / loadedFile.Length * 100);
                }

                string directory = Path.GetDirectoryName(filepath);
                string resultFilename = options.EncryptFilename ? Util.GenerateRandomString(16) : header.FileName;
                byte[] headerBytes = header.BuildHeader();
                File.WriteAllBytes(Path.Combine(directory, resultFilename + EXT_ENCRYPTED), Util.ConcatByteArrays(headerBytes, result));
                if (!(resultFilename + EXT_ENCRYPTED).Equals(Path.GetFileName(filepath)))
                    File.Delete(filepath);
                if (tracker != null)
                    tracker.OutputBuffer.AppendText(string.Format("Encrypted {0} bytes as {1}", headerBytes.Length + result.Length, resultFilename + EXT_ENCRYPTED));
            }
            catch (SharpEncryptException e)
            {
                if (tracker != null)
                    tracker.OutputBuffer.AppendText(e.Message + "\n");
                return false;
            }
            catch (Exception e)
            {
                if (tracker != null)
                {
                    tracker.OutputBuffer.AppendText("\n====================================\n");
                    tracker.OutputBuffer.AppendText("ERROR --- " + e.Message + "\n");
                    tracker.OutputBuffer.AppendText("====================================\n");
                }
                return false;
            }

            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            string timeElapsed = string.Format("{0}:{1}", ((int)Math.Floor(ts.TotalMinutes)).ToString("D2"), ts.ToString("ss\\.fff"));
            if (tracker != null)
                tracker.OutputBuffer.AppendText(" --- Success. (" + timeElapsed + ")\n");
            workComplete = true;
            return true;
        }

        public override bool Decrypt(byte[] masterKey, string password, EncryptOptions options, WorkTracker tracker)
        {
            workComplete = false;
            if (tracker != null)
                tracker.OutputBuffer.AppendText("Decrypting " + System.IO.Path.GetFileName(filepath) + "... ");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                header.ParseHeader(loadedFile, password);

                if (!ValidateChecksum(masterKey, password))
                    throw new SharpEncryptException("Checksum mismatch. Check that you entered the correct password and try again.");

                byte[] result = new byte[header.OriginalFilesize];
                int headerSize = header.HeaderSize;

                var aes = new AesCryptographyService();
                for (int i = headerSize; i < loadedFile.Length; i += AesCryptographyService.DEFAULT_BLOCK_SIZE)
                {
                    byte[] current = loadedFile.RangeSubset(i, AesCryptographyService.DEFAULT_BLOCK_SIZE);
                    byte[] decrypted = aes.Decrypt(current, password);
                    result.OverwriteSubset(decrypted, i - headerSize);

                    if (tracker != null)
                        tracker.ReportProgress((double)(i + AesCryptographyService.DEFAULT_BLOCK_SIZE - headerSize) / (loadedFile.Length - headerSize) * 100);
                }

                string directory = Path.GetDirectoryName(filepath);
                File.WriteAllBytes(Path.Combine(directory, header.FileName + header.FileExtension), result);
                if (!(header.FileName + header.FileExtension).Equals(Path.GetFileName(filepath)))
                    File.Delete(filepath);
                if (tracker != null)
                    tracker.OutputBuffer.AppendText(string.Format("Decrypted {0} bytes as {1}", result.Length, header.FileName + header.FileExtension));
            }
            catch (SharpEncryptException e)
            {
                if (tracker != null)
                    tracker.OutputBuffer.AppendText(e.Message + "\n");
                return false;
            }
            catch (Exception e)
            {
                if (tracker != null)
                {
                    tracker.OutputBuffer.AppendText("\n====================================\n");
                    tracker.OutputBuffer.AppendText("ERROR --- " + e.Message + "\n");
                    tracker.OutputBuffer.AppendText("====================================\n");
                }
                return false;
            }

            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            string timeElapsed = string.Format("{0}:{1}", ((int)Math.Floor(ts.TotalMinutes)).ToString("D2"), ts.ToString("ss\\.fff"));
            if (tracker != null)
                tracker.OutputBuffer.AppendText(" --- Success. (" + timeElapsed + ")\n");
            workComplete = true;
            return true;
        }
    }
}
