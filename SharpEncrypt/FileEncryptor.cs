﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpEncrypt
{
    public class FileEncryptor
    {
        private const string EXT_ENCRYPTED = ".senc";

        private string filepath = "";
        private byte[] loadedFile;
        private Header header = new Header();
        private string message = "";
        private bool workComplete = false;

        public string Filepath
        {
            get { return filepath; }
        }

        public Header Header
        {
            get { return header; }
        }

        public string Message
        {
            get { return message; }
        }

        public bool WorkComplete
        {
            get { return workComplete; }
        }

        public FileEncryptor() { }

        public FileEncryptor(string filepath)
        {
            if (File.Exists(filepath))
                loadedFile = File.ReadAllBytes(filepath);
            this.filepath = filepath;
        }

        private bool ValidateChecksum(string password)
        {
            AesCryptographyService aes = new AesCryptographyService();
            byte[] encrypted = aes.Encrypt(Util.StringEncoding.GetBytes(password));
            return encrypted.SequenceEqual(header.Checksum);
        }

        public bool EncryptFile(string password, bool encryptFilename, WorkTracker tracker)
        {
            workComplete = false;
            try
            {
                // Make sure encrypted result is a size divisible by the block size
                byte[] result = new byte[loadedFile.Length % AesCryptographyService.DEFAULT_BLOCK_SIZE == 0 ? loadedFile.Length :
                    loadedFile.Length + (AesCryptographyService.DEFAULT_BLOCK_SIZE - (loadedFile.Length % AesCryptographyService.DEFAULT_BLOCK_SIZE))];

                header.SetPassword(password);
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
                string resultFilename = encryptFilename ? Util.GenerateRandomString(16) : header.FileName;
                byte[] headerBytes = header.BuildHeader();
                File.WriteAllBytes(Path.Combine(directory, resultFilename + EXT_ENCRYPTED), Util.ConcatByteArrays(headerBytes, result));
                File.Delete(filepath);
                message = string.Format("Encrypted {0} bytes as {1}", headerBytes.Length + result.Length, resultFilename + EXT_ENCRYPTED);
            }
            catch (Exception e)
            {
                message = e.Message;
                return false;
            }
            workComplete = true;
            return true;
        }

        public bool DecryptFile(string password, WorkTracker tracker)
        {
            workComplete = false;
            try
            {
                header.ParseHeader(loadedFile, password);

                if (!ValidateChecksum(password))
                {
                    message = "Checksum mismatch. Check that you entered the correct password and try again.";
                    return false;
                }

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
                File.Delete(filepath);
                message = string.Format("Decrypted {0} bytes as {1}", result.Length, header.FileName + header.FileExtension);
            }
            catch (Exception e)
            {
                message = e.Message;
                return false;
            }
            workComplete = true;
            return true;
        }
    }
}
