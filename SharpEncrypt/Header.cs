using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpEncrypt
{
    public class Header
    {
        private static readonly byte[] IDENTIFIER = new byte[] { 0x22, 0x6B, 0x16, 0x59 };
        private static readonly byte[] DELIM = new byte[] { 0x93, 0x6D, 0xE6 };
        private static readonly byte[] END = new byte[] { 0x1F, 0xAA, 0x9C };

        // Header codes
        private const byte CODE_CHECKSUM = 0x01;
        private const byte CODE_FILESIZE = 0x02;
        private const byte CODE_EXTENSION = 0x03;
        private const byte CODE_FILENAME = 0x04;

        // Software versions
        private const byte VERSION_INDEV1 = 0x01;
        private const byte VERSION_INDEV2 = 0x02;
        private static readonly byte CURRENT_VERSION = VERSION_INDEV2;

        private byte[] checksum = new byte[AesCryptographyService.BLOCK_SIZE];
        private int filesize = 0;
        private string extension = "";
        private byte[] extensionEncrypted = new byte[AesCryptographyService.BLOCK_SIZE];
        private string filename = "";
        private byte[] filenameEncrypted = new byte[AesCryptographyService.BLOCK_SIZE];
        private int contentStartIdx = 0;

        public ushort HeaderSize
        {
            get
            {
                return (ushort)(IDENTIFIER.Length + sizeof(ushort) + sizeof(byte) +
                    DELIM.Length + sizeof(byte) + sizeof(ushort) + checksum.Length +
                    DELIM.Length + sizeof(byte) + sizeof(ushort) + sizeof(int) +
                    DELIM.Length + sizeof(byte) + sizeof(ushort) + (CURRENT_VERSION >= VERSION_INDEV2 ? extensionEncrypted.Length : Encoding.UTF8.GetByteCount(extension)) +
                    DELIM.Length + sizeof(byte) + sizeof(ushort) + (CURRENT_VERSION >= VERSION_INDEV2 ? filenameEncrypted.Length : Encoding.UTF8.GetByteCount(filename)) +
                    END.Length);
            }
        }

        public byte[] Checksum
        {
            get { return checksum; }
        }

        public int OriginalFilesize
        {
            get { return filesize; }
            set { filesize = value; }
        }

        public string FileExtension
        {
            get { return extension; }
        }

        public string FileName
        {
            get { return filename; }
        }

        public int ContentStartIdx
        {
            get { return contentStartIdx; }
        }

        public byte[] GetHeader()
        {
            // 4 bytes - identifier
            // 2 bytes - size of entire header
            // 1 byte - software version
            // Repeat:
            //     3 bytes - delimiter
            //     1 byte - header code
            //     2 bytes - data size
            //     X bytes - data for that header code
            // 3 bytes - end

            int idx = 0;
            byte[] result = new byte[HeaderSize];
            Array.Copy(IDENTIFIER, 0, result, 0, IDENTIFIER.Length);
            idx += IDENTIFIER.Length;
            byte[] headerSize = BitConverter.GetBytes(HeaderSize);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(headerSize);
            Array.Copy(headerSize, 0, result, idx, sizeof(ushort));
            idx += sizeof(ushort);
            result[idx++] = CURRENT_VERSION;

            // Checksum
            Array.Copy(DELIM, 0, result, idx, DELIM.Length);
            idx += DELIM.Length;
            result[idx++] = CODE_CHECKSUM;
            byte[] checksumSize = BitConverter.GetBytes((ushort)checksum.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(checksumSize);
            Array.Copy(checksumSize, 0, result, idx, sizeof(ushort));
            idx += sizeof(ushort);
            Array.Copy(checksum, 0, result, idx, checksum.Length);
            idx += checksum.Length;

            // Filesize
            Array.Copy(DELIM, 0, result, idx, DELIM.Length);
            idx += DELIM.Length;
            result[idx++] = CODE_FILESIZE;
            byte[] filesizeSize = BitConverter.GetBytes((ushort)sizeof(int));
            if (BitConverter.IsLittleEndian)
                Array.Reverse(filesizeSize);
            Array.Copy(filesizeSize, 0, result, idx, sizeof(ushort));
            idx += sizeof(ushort);
            byte[] filesizeBytes = BitConverter.GetBytes(filesize);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(filesizeBytes);
            Array.Copy(filesizeBytes, 0, result, idx, sizeof(int));
            idx += sizeof(int);

            // Extension
            Array.Copy(DELIM, 0, result, idx, DELIM.Length);
            idx += DELIM.Length;
            result[idx++] = CODE_EXTENSION;
            byte[] extensionSize = BitConverter.GetBytes((ushort)extensionEncrypted.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(extensionSize);
            Array.Copy(extensionSize, 0, result, idx, sizeof(ushort));
            idx += sizeof(ushort);
            Array.Copy(extensionEncrypted, 0, result, idx, extensionEncrypted.Length);
            idx += extensionEncrypted.Length;

            // Filename
            Array.Copy(DELIM, 0, result, idx, DELIM.Length);
            idx += DELIM.Length;
            result[idx++] = CODE_FILENAME;
            byte[] filenameSize = BitConverter.GetBytes((ushort)filenameEncrypted.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(filenameSize);
            Array.Copy(filenameSize, 0, result, idx, sizeof(ushort));
            idx += sizeof(ushort);
            Array.Copy(filenameEncrypted, 0, result, idx, filenameEncrypted.Length);
            idx += filenameEncrypted.Length;

            Array.Copy(END, 0, result, idx, END.Length);
            return result;
        }

        public void ParseHeader(byte[] loadedFile, string password)
        {
            if (!Util.MatchByteSequence(loadedFile, 0, IDENTIFIER))
                throw new Exception("This file is not recognized as an encrypted file and cannot be decrypted.");

            int idx = IDENTIFIER.Length;
            byte[] contentStartIdxBytes = new byte[sizeof(ushort)];
            Array.Copy(loadedFile, 0, contentStartIdxBytes, 0, sizeof(ushort));
            if (BitConverter.IsLittleEndian)
                Array.Reverse(contentStartIdxBytes);
            contentStartIdx = BitConverter.ToUInt16(contentStartIdxBytes, 0);
            idx += sizeof(ushort);
            byte version = loadedFile[idx++];
            if (version > CURRENT_VERSION)
                throw new Exception("This file cannot be opened with this version of SharpEncrypt. Please use a later version.");

            while (idx < loadedFile.Length)
            {
                if (NextBytesAreEnd(loadedFile, idx))
                {
                    break;
                }
                else if (NextBytesAreDelim(loadedFile, idx))
                {
                    idx += DELIM.Length;
                    byte headerCode = loadedFile[idx++];
                    byte[] dataSizeBytes = new byte[sizeof(ushort)];
                    Array.Copy(loadedFile, idx, dataSizeBytes, 0, sizeof(ushort));
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(dataSizeBytes);
                    ushort dataSize = BitConverter.ToUInt16(dataSizeBytes, 0);
                    idx += sizeof(ushort);
                    switch (headerCode)
                    {
                        case CODE_CHECKSUM:
                            Array.Copy(loadedFile, idx, checksum, 0, dataSize);
                            break;
                        case CODE_FILESIZE:
                            byte[] filesizeBytes = new byte[sizeof(int)];
                            Array.Copy(loadedFile, idx, filesizeBytes, 0, sizeof(int));
                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(filesizeBytes);
                            filesize = BitConverter.ToInt32(filesizeBytes, 0);
                            break;
                        case CODE_EXTENSION:
                            if (CURRENT_VERSION >= VERSION_INDEV2)
                            {
                                Array.Copy(loadedFile, idx, extensionEncrypted, 0, dataSize);
                                AesCryptographyService aes = new AesCryptographyService();
                                byte[] decrypted = aes.Decrypt(extensionEncrypted, password);
                                extension = Encoding.UTF8.GetString(decrypted).Replace("\0", String.Empty);
                            }
                            else
                                extension = Encoding.UTF8.GetString(loadedFile, idx, dataSize);
                            break;
                        case CODE_FILENAME:
                            if (CURRENT_VERSION >= VERSION_INDEV2)
                            {
                                Array.Copy(loadedFile, idx, filenameEncrypted, 0, dataSize);
                                AesCryptographyService aes = new AesCryptographyService();
                                byte[] decrypted = aes.Decrypt(filenameEncrypted, password);
                                filename = Encoding.UTF8.GetString(decrypted).Replace("\0", String.Empty);
                            }
                            else
                                filename = Encoding.UTF8.GetString(loadedFile, idx, dataSize);
                            break;
                        default:
                            System.Diagnostics.Debug.WriteLine("WARNING: Unsupported header code " + headerCode.ToString());
                            break;
                    }
                    idx += dataSize;
                }
                else
                {
                    idx++;
                }
            }
        }

        public void SetPassword(string password)
        {
            AesCryptographyService aes = new AesCryptographyService();
            checksum = aes.Encrypt(Encoding.UTF8.GetBytes(password));
        }

        public void SetFileExtension(string extension, string password)
        {
            this.extension = extension;
            AesCryptographyService aes = new AesCryptographyService();
            extensionEncrypted = aes.Encrypt(Encoding.UTF8.GetBytes(extension), password);
        }

        public void SetFileName(string filename, string password)
        {
            this.filename = filename;
            AesCryptographyService aes = new AesCryptographyService();
            filenameEncrypted = aes.Encrypt(Encoding.UTF8.GetBytes(filename), password);
        }

        private bool NextBytesAreDelim(byte[] bytes, int startIdx)
        {
            return Util.MatchByteSequence(bytes, startIdx, DELIM);
        }

        private bool NextBytesAreEnd(byte[] bytes, int startIdx)
        {
            return Util.MatchByteSequence(bytes, startIdx, END);
        }
    }
}
