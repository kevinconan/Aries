using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Aries.Lib
{
    public static class EncryptUtil
    {
        public static string ToMD5(byte[] input)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(input);
            return BitConverter.ToString(output).Replace("-", "");
        }

        public static string ToMD5(string input)
        {
            return ToMD5(Encoding.Default.GetBytes(input));
        }
    }
}
