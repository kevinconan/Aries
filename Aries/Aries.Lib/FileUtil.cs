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
                using (var fileStream = File.Create(filePath))
                {
                    byte[] fileBytes = new byte[fileStream.Length];
                    fileStream.Read(fileBytes, 0, fileBytes.Length);
                    return md5Str == EncryptUtil.ToMD5(fileBytes);
                }
            }
            else
            {
                return false;
            }
        }
    }
}
