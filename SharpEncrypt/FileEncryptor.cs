using System;
using System.Collections.Generic;
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
            byte[] encrypted = aes.Encrypt(Encoding.UTF8.GetBytes(password));
            return encrypted.SequenceEqual(header.Checksum);
        }

        public bool EncryptFile(string password, bool encryptFilename)
        {
            // Make sure encrypted result is a size divisible by the block size
            byte[] result = new byte[loadedFile.Length % AesCryptographyService.BLOCK_SIZE == 0 ? loadedFile.Length :
                loadedFile.Length + (AesCryptographyService.BLOCK_SIZE - (loadedFile.Length % AesCryptographyService.BLOCK_SIZE))];

            header.SetPassword(password);
            header.OriginalFilesize = loadedFile.Length;
            string filename = Path.GetFileName(filepath);
            header.SetFileExtension(Path.GetExtension(filename), password);
            header.SetFileName(Path.GetFileNameWithoutExtension(filename), password);

            var aes = new AesCryptographyService();
            for (int i = 0; i < loadedFile.Length; i += AesCryptographyService.BLOCK_SIZE)
            {
                // Get next block
                byte[] current = new byte[AesCryptographyService.BLOCK_SIZE];
                for (int j = 0; j < AesCryptographyService.BLOCK_SIZE; ++j)
                {
                    if (i + j < loadedFile.Length)
                        current[j] = loadedFile[i + j];
                    else
                        current[j] = 0;  // pad final block with zeroes
                }

                // Encrypt the block
                byte[] encrypted = aes.Encrypt(current, password);

                // Write the encrypted block to the result
                for (int j = 0; j < AesCryptographyService.BLOCK_SIZE; ++j)
                {
                    if (i + j < result.Length)
                        result[i + j] = encrypted[j];
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("WARNING: Final block is not " + AesCryptographyService.BLOCK_SIZE.ToString() + " bytes.");
                        break;
                    }
                }
            }

            string directory = Path.GetDirectoryName(filepath);
            string resultFilename = encryptFilename ? Util.GenerateRandomString(16) : header.FileName;
            File.WriteAllBytes(Path.Combine(directory, resultFilename + EXT_ENCRYPTED), Util.ConcatByteArrays(header.GetHeader(), result));
            File.Delete(filepath);
            return true;
        }

        public bool DecryptFile(string password)
        {
            try
            {
                header.ParseHeader(loadedFile, password);
            }
            catch (Exception e)
            {
                message = "Something went wrong when reading the file:\n" + e.Message;
                return false;
            }
            
            if (!ValidateChecksum(password))
            {
                message = "Checksum mismatch. Check that you entered the correct password and try again.";
                return false;
            }

            byte[] result = new byte[header.OriginalFilesize];
            int headerSize = header.HeaderSize;

            var aes = new AesCryptographyService();
            for (int i = headerSize; i < loadedFile.Length; i += AesCryptographyService.BLOCK_SIZE)
            {
                // Get the next block
                byte[] current = new byte[AesCryptographyService.BLOCK_SIZE];
                for (int j = 0; j < AesCryptographyService.BLOCK_SIZE; ++j)
                {
                    if (i + j < loadedFile.Length)
                        current[j] = loadedFile[i + j];
                    else
                        current[j] = 0;
                }

                // Decrypt the block
                byte[] decrypted = aes.Decrypt(current, password);

                // Write the decrypted block to the result
                for (int j = 0; j < AesCryptographyService.BLOCK_SIZE; ++j)
                {
                    if (i + j - headerSize < result.Length)
                        result[i + j - headerSize] = decrypted[j];
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("WARNING: Final block is not " + AesCryptographyService.BLOCK_SIZE.ToString() + " bytes.");
                        break;
                    }
                }
            }

            string directory = Path.GetDirectoryName(filepath);
            File.WriteAllBytes(Path.Combine(directory, header.FileName + header.FileExtension), result);
            File.Delete(filepath);
            return true;
        }
    }
}
