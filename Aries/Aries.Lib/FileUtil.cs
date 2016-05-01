using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aries.Lib
{
    public static class FileUtil
    {
        public static bool FileMD5Validation(string md5Str,string filePath)
        {
            if (File.Exists(filePath))
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    byte[] fileBytes = new byte[fileStream.Length];
                    fileStream.Read(fileBytes, 0, fileBytes.Length);
                    string fileMd5 = EncryptUtil.ToMD5(fileBytes);
                    Console.WriteLine(fileMd5);
                    return md5Str == fileMd5;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
