using System;
using System.Linq;
using System.Text;

namespace LPayments.Utils
{
    internal static class HexCoding
    {
        private static readonly char[] HexChar = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        private static readonly byte[] ReverseHexChar = new byte[128];

        static HexCoding()
        {
            SetReverseHexChar('0', '9', 0);
            SetReverseHexChar('A', 'F', 10);
            SetReverseHexChar('a', 'f', 10);
        }

        private static void SetReverseHexChar(char a, char b, int add)
        {
            for (int i = a; i <= b; i++)
            {
                ReverseHexChar[i] = (byte)(i - a + add);
            }
        }

        public static byte[] Decode(string src)
        {
            int n = src.Length;
            if ((n % 2) == 0)
            {
                int m = n / 2;
                var ret = new byte[m];
                unsafe
                {
                    fixed (char* p = src)
                    fixed (byte* r = ReverseHexChar)
                    {
                        for (int i = 0, j = 0; i < m; i++)
                        {
                            var b1 = (byte)(r[p[j++]] << 4);
                            byte b2 = r[p[j++]];
                            ret[i] = (byte)(b1 | b2);
                        }
                    }
                }
                return ret;
            }
            throw new ArgumentException("String length error!");
        }

        public static string Encode(byte[] src)
        {
            int n = src.Length;
            var ret = new string((char)0, n + n);
            unsafe
            {
                fixed (char* p = ret, h = HexChar)
                {
                    for (int i = 0, j = 0; i < n; i++)
                    {
                        p[j + 1] = h[src[i] & 15];
                        p[j] = h[src[i] >> 4];
                        j += 2;
                    }
                }
            }
            return ret;
        }
        
        //以下方法效率较低

        //public static byte[] HexStringToBytes(string hexStr)
        //{
        //    if (string.IsNullOrEmpty(hexStr))
        //    {
        //        return new byte[0];
        //    }

        //    if (hexStr.StartsWith("0x"))
        //    {
        //        hexStr = hexStr.Remove(0, 2);
        //    }

        //    var count = hexStr.Length;

        //    if (count % 2 == 1)
        //    {
        //        throw new ArgumentException("Invalid length of bytes:" + count);
        //    }

        //    var byteCount = count / 2;
        //    var result = new byte[byteCount];
        //    for (int ii = 0; ii < byteCount; ++ii)
        //    {
        //        var tempBytes = Byte.Parse(hexStr.Substring(2 * ii, 2), System.Globalization.NumberStyles.HexNumber);
        //        result[ii] = tempBytes;
        //    }

        //    return result;
        //}

        //public static string BytesToHexString(byte[] bytes)
        //{
        //    if (bytes == null || bytes.Count() < 1)
        //    {
        //        return string.Empty;
        //    }

        //    var count = bytes.Count();

        //    var cache = new StringBuilder();
        //    //cache.Append("0x");
        //    for (int ii = 0; ii < count; ++ii)
        //    {
        //        var tempHex = Convert.ToString(bytes[ii], 16).ToUpper();
        //        cache.Append(tempHex.Length == 1 ? "0" + tempHex : tempHex);
        //    }

        //    return cache.ToString();
        //}
    }
}
