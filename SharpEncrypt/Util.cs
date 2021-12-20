using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SharpEncrypt
{
    public static class Util
    {
        public static byte[] ConcatByteArrays(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        public static byte[] HashString(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                return md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            }
        }

        public static bool MatchByteSequence(byte[] bytes, int startIdx, byte[] toMatch)
        {
            for (int i = 0; i < toMatch.Length; ++i)
            {
                if (toMatch[i] != bytes[i + startIdx])
                    return false;
            }
            return true;
        }
    }
}
