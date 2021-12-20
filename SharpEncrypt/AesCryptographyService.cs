using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SharpEncrypt
{
    public class AesCryptographyService
    {
        public static int BLOCK_SIZE = 16;
        private readonly byte[] masterKey = new byte[16] { 0x76, 0x4E, 0x12, 0x08, 0x4A, 0xFF, 0x93, 0xA1, 0xE5, 0x79, 0x35, 0x30, 0x60, 0x8B, 0x95, 0x2B };
        private readonly byte[] iv = new byte[16] { 0x23, 0x1C, 0xCE, 0x8F, 0x1D, 0x05, 0x47, 0x53, 0x8E, 0x98, 0x1D, 0x3F, 0xBE, 0xF2, 0xA8, 0x9D };

        public byte[] Encrypt(byte[] data)
        {
            return Encrypt(data, masterKey, iv);
        }

        public byte[] Encrypt(byte[] data, string password)
        {
            byte[] key = Util.HashString(password);
            return Encrypt(data, key, iv);
        }

        public byte[] Encrypt(byte[] data, byte[] key)
        {
            var encrypted = Encrypt(data, key, iv);
            return encrypted;
        }

        public byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = BLOCK_SIZE * 8;
                aes.BlockSize = BLOCK_SIZE * 8;
                aes.Padding = PaddingMode.Zeros;

                aes.Key = key;
                aes.IV = iv;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(data, encryptor);
                }
            }
        }

        public byte[] Decrypt(byte[] data)
        {
            return Decrypt(data, masterKey, iv);
        }

        public byte[] Decrypt(byte[] data, string password)
        {
            byte[] key = Util.HashString(password);
            return Decrypt(data, key, iv);
        }

        public byte[] Decrypt(byte[] data, byte[] key)
        {
            var decrypted = Decrypt(data, key, iv);
            return decrypted;
        }

        public byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = BLOCK_SIZE * 8;
                aes.BlockSize = BLOCK_SIZE * 8;
                aes.Padding = PaddingMode.Zeros;

                aes.Key = key;
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(data, decryptor);
                }
            }
        }

        private byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using (var ms = new MemoryStream())
            using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();

                return ms.ToArray();
            }
        }
    }
}
