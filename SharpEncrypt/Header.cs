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
        private const byte VERSION_RELEASE1 = 0x03;
        private static readonly byte CURRENT_VERSION = VERSION_RELEASE1;

        private byte[] checksum = new byte[AesCryptographyService.DEFAULT_BLOCK_SIZE];
        private int filesize = 0;
        private string extension = "";
        private byte[] extensionEncrypted = new byte[AesCryptographyService.DEFAULT_BLOCK_SIZE];
        private string filename = "";
        private byte[] filenameEncrypted = new byte[AesCryptographyService.DEFAULT_BLOCK_SIZE];
        private int contentStartIdx = 0;

        public ushort HeaderSize
        {
            get
            {
                return (ushort)(IDENTIFIER.Length + sizeof(ushort) + sizeof(byte) +
                    DELIM.Length + sizeof(byte) + sizeof(ushort) + checksum.Length +
                    DELIM.Length + sizeof(byte) + sizeof(ushort) + sizeof(int) +
                    DELIM.Length + sizeof(byte) + sizeof(ushort) + (CURRENT_VERSION >= VERSION_INDEV2 ? extensionEncrypted.Length : Util.StringEncoding.GetByteCount(extension)) +
                    DELIM.Length + sizeof(byte) + sizeof(ushort) + (CURRENT_VERSION >= VERSION_INDEV2 ? filenameEncrypted.Length : Util.StringEncoding.GetByteCount(filename)) +
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

        public byte[] BuildHeader()
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

            LoadIntoByteArray(result, ref idx, IDENTIFIER);
            LoadIntoByteArray(result, ref idx, HeaderSize);
            result[idx++] = CURRENT_VERSION;

            // Checksum
            LoadIntoByteArray(result, ref idx, DELIM);
            result[idx++] = CODE_CHECKSUM;
            LoadIntoByteArray(result, ref idx, (ushort)checksum.Length);
            LoadIntoByteArray(result, ref idx, checksum);

            // Filesize
            LoadIntoByteArray(result, ref idx, DELIM);
            result[idx++] = CODE_FILESIZE;
            LoadIntoByteArray(result, ref idx, (ushort)sizeof(int));
            LoadIntoByteArray(result, ref idx, filesize);

            // Extension
            LoadIntoByteArray(result, ref idx, DELIM);
            result[idx++] = CODE_EXTENSION;
            LoadIntoByteArray(result, ref idx, (ushort)extensionEncrypted.Length);
            LoadIntoByteArray(result, ref idx, extensionEncrypted);

            // Filename
            LoadIntoByteArray(result, ref idx, DELIM);
            result[idx++] = CODE_FILENAME;
            LoadIntoByteArray(result, ref idx, (ushort)filenameEncrypted.Length);
            LoadIntoByteArray(result, ref idx, filenameEncrypted);

            LoadIntoByteArray(result, ref idx, END);
            return result;
        }

        public void ParseHeader(byte[] loadedFile, string password)
        {
            if (!Util.MatchByteSequence(loadedFile, 0, IDENTIFIER))
                throw new SharpEncryptException("This file is not recognized as an encrypted file and cannot be decrypted.");

            int idx = IDENTIFIER.Length;
            contentStartIdx = ReadUInt16FromByteArray(loadedFile, ref idx);
            byte version = loadedFile[idx++];
            if (version > CURRENT_VERSION)
                throw new SharpEncryptException("This file cannot be opened with this version of SharpEncrypt. Please use a later version.");

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
                    ushort dataSize = ReadUInt16FromByteArray(loadedFile, ref idx);
                    switch (headerCode)
                    {
                        case CODE_CHECKSUM:
                            ReadBytesFromByteArray(loadedFile, ref idx, checksum, dataSize);
                            break;
                        case CODE_FILESIZE:
                            filesize = ReadInt32FromByteArray(loadedFile, ref idx);
                            break;
                        case CODE_EXTENSION:
                            if (CURRENT_VERSION >= VERSION_INDEV2)
                            {
                                extensionEncrypted = new byte[dataSize];
                                ReadBytesFromByteArray(loadedFile, ref idx, extensionEncrypted, dataSize);
                                extension = DecryptHeaderBytes(extensionEncrypted, password);
                            }
                            else
                                extension = ReadStringFromByteArray(loadedFile, ref idx, dataSize);
                            break;
                        case CODE_FILENAME:
                            if (CURRENT_VERSION >= VERSION_INDEV2)
                            {
                                filenameEncrypted = new byte[dataSize];
                                ReadBytesFromByteArray(loadedFile, ref idx, filenameEncrypted, dataSize);
                                filename = DecryptHeaderBytes(filenameEncrypted, password);
                            }
                            else
                                filename = ReadStringFromByteArray(loadedFile, ref idx, dataSize);
                            break;
                        default:
                            System.Diagnostics.Debug.WriteLine("WARNING: Unsupported header code " + headerCode.ToString());
                            idx += dataSize;
                            break;
                    }
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
            checksum = aes.Encrypt(Util.StringEncoding.GetBytes(password));
        }

        public void SetFileExtension(string extension, string password)
        {
            this.extension = extension;
            AesCryptographyService aes = new AesCryptographyService();
            extensionEncrypted = aes.Encrypt(Util.StringEncoding.GetBytes(extension), password);
            if (extensionEncrypted.Length <= 0)
                extensionEncrypted = new byte[AesCryptographyService.DEFAULT_BLOCK_SIZE];
        }

        public void SetFileName(string filename, string password)
        {
            this.filename = filename;
            AesCryptographyService aes = new AesCryptographyService();
            filenameEncrypted = aes.Encrypt(Util.StringEncoding.GetBytes(filename), password);
        }

        private bool NextBytesAreDelim(byte[] bytes, int startIdx)
        {
            return Util.MatchByteSequence(bytes, startIdx, DELIM);
        }

        private bool NextBytesAreEnd(byte[] bytes, int startIdx)
        {
            return Util.MatchByteSequence(bytes, startIdx, END);
        }

        private string DecryptHeaderBytes(byte[] source, string password)
        {
            if (source.All(b => b.Equals(0x0)))
                return "";
            AesCryptographyService aes = new AesCryptographyService();
            byte[] decrypted = aes.Decrypt(source, password);
            return Util.StringEncoding.GetString(decrypted).Replace("\0", String.Empty);
        }

        private void LoadIntoByteArray(byte[] dest, ref int idx, byte[] source)
        {
            LoadIntoByteArray(dest, ref idx, source, 0, source.Length);
        }

        private void LoadIntoByteArray(byte[] dest, ref int idx, byte[] source, int sourceStart, int len)
        {
            Array.Copy(source, sourceStart, dest, idx, len);
            idx += len;
        }

        private void LoadIntoByteArray(byte[] dest, ref int idx, ushort data)
        {
            byte[] dataBytes = BitConverter.GetBytes(data);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(dataBytes);
            Array.Copy(dataBytes, 0, dest, idx, sizeof(ushort));
            idx += sizeof(ushort);
        }

        private void LoadIntoByteArray(byte[] dest, ref int idx, int data)
        {
            byte[] dataBytes = BitConverter.GetBytes(data);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(dataBytes);
            Array.Copy(dataBytes, 0, dest, idx, sizeof(int));
            idx += sizeof(int);
        }

        private ushort ReadUInt16FromByteArray(byte[] source, ref int idx)
        {
            byte[] dataBytes = new byte[sizeof(ushort)];
            Array.Copy(source, idx, dataBytes, 0, sizeof(ushort));
            if (BitConverter.IsLittleEndian)
                Array.Reverse(dataBytes);
            ushort data = BitConverter.ToUInt16(dataBytes, 0);
            idx += sizeof(ushort);
            return data;
        }

        private int ReadInt32FromByteArray(byte[] source, ref int idx)
        {
            byte[] dataBytes = new byte[sizeof(int)];
            Array.Copy(source, idx, dataBytes, 0, sizeof(int));
            if (BitConverter.IsLittleEndian)
                Array.Reverse(dataBytes);
            int data = BitConverter.ToInt32(dataBytes, 0);
            idx += sizeof(int);
            return data;
        }

        private string ReadStringFromByteArray(byte[] source, ref int idx, int len)
        {
            string str = Util.StringEncoding.GetString(source, idx, len);
            idx += len;
            return str;
        }

        private void ReadBytesFromByteArray(byte[] source, ref int idx, byte[] dest, int len)
        {
            Array.Copy(source, idx, dest, 0, len);
            idx += len;
        }
    }
}
