using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Threading;

namespace Aries.Lib
{

    public static class NetworkAdapterInstaller
    {

        public static readonly string X86FILE = "devcon_x86";
        public static readonly string X64FILE = "devcon_x64";
        public static readonly string ARIESDIR = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Aries\";

        public static readonly string OUTFILE =ARIESDIR + "devcon.exe";

        public static WarpMessage WarpMessage;
        
        public static void CheckAndInstallAdapter(Action<bool> callback)
        {

        }
        
        private static bool CheckAndOpenAdapter()
        {

            return false;
        }

        #region 释放Devcon

        
        private static bool ReleaseDevcon()
        {
            if (!File.Exists(OUTFILE))
            {
                string fileName = Environment.Is64BitOperatingSystem ? X64FILE : X86FILE;
                Assembly assembly = Assembly.GetExecutingAssembly();
                string resName = Assembly.GetExecutingAssembly().GetName().Name + ".Resources." + fileName;
                using (Stream devconStream = assembly.GetManifestResourceStream(resName))
                {
                    if (devconStream != null)
                    {
                        if (!Directory.Exists(ARIESDIR))
                        {
                            try
                            {
                                Directory.CreateDirectory(ARIESDIR);
                            }
                            catch (Exception)
                            {
                                WarpMessage(MessageType.Error, "[错误]目录创建失败！");
                                return false;
                            }
                            
                        }
                        try
                        {
                            var tmpBytes = new Byte[devconStream.Length];
                            devconStream.Read(tmpBytes, 0, tmpBytes.Length);
                            using (var fs = File.Create(OUTFILE))
                            {
                                using (var bw = new BinaryWriter(fs))
                                {
                                    bw.Write(tmpBytes);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            WarpMessage(MessageType.Error, "[错误]无法解压文件！");
                            return false;
                        }
                        
                    }
                }
            }
            return true;
        }
        #endregion
    }
}
