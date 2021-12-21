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
        private static Random random = new Random();

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

        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static T[] RangeSubset<T>(this T[] array, int startIndex, int length)
        {
            T[] subset = new T[length];
            Array.Copy(array, startIndex, subset, 0, Math.Min(length, array.Length - startIndex));  // pad with zeroes if not enough data to fill new array
            return subset;
        }

        public static T[] Subset<T>(this T[] array, params int[] indices)
        {
            T[] subset = new T[indices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                subset[i] = array[indices[i]];
            }
            return subset;
        }

        public static void OverwriteSubset<T>(this T[] array, T[] newData, int startIndex)
        {
            Array.Copy(newData, 0, array, startIndex, Math.Min(newData.Length, array.Length - startIndex));
        }
    }
}
