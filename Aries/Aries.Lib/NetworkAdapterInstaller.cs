using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Aries.Lib
{
    public enum NetForwardMode
    {
        Adapter = 0,
        Route = 1
    }
    public static class NetworkAdapterInstaller
    {

        public static readonly string X86FILE = "devcon_x86";
        public static readonly string X64FILE = "devcon_x64";

        public static readonly string X86MD5 = "7EB69E1F3BC96DE3E79299BA96890C80";
        public static readonly string X64MD5 = "48E5B0185208D7B0DF5D29EB9A0BA24C";

        public static readonly string ARIESDIR = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Aries\";

        public static readonly string OUTFILE = ARIESDIR + "devcon.exe";

        public static WarpMessage WarpMessage;

        public static NetForwardMode InstallMode = NetForwardMode.Adapter;


        public async static void ChangeMode(NetForwardMode mode, Action<bool> callback)
        {
            await Task.Run(() =>
            {
                InstallMode = mode;
                if (mode == NetForwardMode.Adapter)
                {
                    SendMessage("正在启用虚拟网卡模式..");
                    callback(DisableRedirection());


                }
                else
                {
                    SendMessage("正在启用路由模式..");
                    callback(DisableLoopAdapters());
                    ;
                }

            });
        }

        public async static void CheckAndInstallAdapter(Action<bool> callback)
        {
            await Task.Run(() =>
            {
                if (InstallMode == NetForwardMode.Adapter)
                {
                    callback(ReleaseDevcon() && CheckAndOpenAdapter());
                }
                else
                {
                    callback(OpenSuperMode());
                }

            });

        }


        private static bool OpenSuperMode()
        {
            SendMessage("开始设置IP重定向...");
            RunCmd($"netsh int ip add addr 1 221.231.130.70/32 st=ac sk=tr", DEBUGWriteToConsole);
            if (Ping())
            {
                SendMessage("IP重定向设置成功！");
                return true;
            }
            else
            {
                SendMessage("IP重定向设置失败！");
                return false;
            }
        }


        private static bool CheckAndOpenAdapter()
        {
            SendMessage("开始检查虚拟网卡..");
            var adapterList = QueryInstalledLoopbackAdapters();
            if (adapterList.Count > 0)
            {
                if (adapterList.Count == 1)
                {
                    if (EnableAdapter(adapterList[0].Properties["PNPDeviceID"].Value.ToString()))
                    {
                        SendMessage("开始设置网卡信息");
                        return SetAdapter();
                    }
                    else
                    {
                        SendErrorMessage("网卡启动失败！");
                        return false;

                    }
                }
                else
                {
                    SendMessage("找到多个虚拟网卡，处理中");
                    for (int i = 0; i < adapterList.Count; i++)
                    {
                        if (i != 0)
                        {
                            DeleteAdapter(adapterList[i].Properties["PNPDeviceID"].Value.ToString());
                        }

                    }
                    SendMessage("开始设置网卡信息");
                    return SetAdapter();
                }
            }
            else
            {
                SendMessage("未找到已安装的虚拟网卡...开始安装..");
                if (InstallAdapter())
                {
                    SendMessage("开始设置网卡信息");
                    return SetAdapter();
                }
                else
                {
                    return false;
                }
            }

        }

        private static bool DeleteAdapter(string PNPDeviceID)
        {
            var adapterList = QueryAdapterByPnp(PNPDeviceID);
            if (adapterList.Count > 0)
            {
                RunCmd($"devcon.exe /r remove @{PNPDeviceID}", DEBUGWriteToConsole);
                var list2 = QueryAdapterByPnp(PNPDeviceID);
                if (list2.Count > 0)
                {
                    SendErrorMessage($"网卡{adapterList[0]["Caption"]}删除失败！");
                    return false;
                }
                else
                {
                    SendMessage($"网卡{adapterList[0]["Caption"]}删除成功！");
                    return true;
                }
            }
            else
            {
                SendErrorMessage($"未找到网卡{PNPDeviceID},删除失败");
                return false;
            }
        }

        private static bool InstallAdapter()
        {
            RunCmd("devcon.exe install %windir%/inf/netloop.inf *msloop", DEBUGWriteToConsole);
            var adapterList = QueryInstalledLoopbackAdapters();
            if (adapterList.Count > 0)
            {
                SendMessage($"虚拟网卡安装成功：{adapterList[0]["Caption"]}");
                return true;
            }
            else
            {
                SendErrorMessage("虚拟网卡安装失败！");
                return false;
            }
        }

        private static bool EnableAdapter(string PNPDeviceID)
        {
            var adapterList = QueryAdapterByPnp(PNPDeviceID);
            if (adapterList.Count > 0)
            {
                SendMessage($"找到虚拟网卡{adapterList[0]["Caption"]}");
                RunCmd($"devcon.exe /r enable @{PNPDeviceID}", DEBUGWriteToConsole);
                return true;
            }
            else
            {
                SendErrorMessage($"未找到网卡{PNPDeviceID},网卡启动失败");
                return false;
            }
        }

        public async static void CloseNetwork()
        {
            await Task.Run(() =>
            {
                try
                {
                    DisableRedirection();
                    DisableLoopAdapters();

                }
                catch { }
            });



        }

        private static bool DisableRedirection()
        {
            SendMessage("开始取消重定向..");
            StringBuilder sb = new StringBuilder();
            sb.Append("netsh int ip delete addr 1 221.231.130.70 \n");
            sb.Append("route delete 221.231.130.70");

            RunCmd(sb.ToString(), DEBUGWriteToConsole);
            SendMessage("重定向已取消");
            return true;
        }

        private static bool DisableLoopAdapters()
        {
            SendMessage("开始禁用虚拟网卡");
            var adapterList = QuerySysinfo("Win32_NetworkAdapter", $"Caption like '%Loopback Adapter%'");
            if (adapterList.Count > 0)
            {
                foreach (var item in adapterList)
                {
                    SendMessage($"找到虚拟网卡{item["Name"]}");
                    RunCmd($"devcon.exe /r disable @{item["PNPDeviceID"]}", DEBUGWriteToConsole);

                }
                SendMessage("虚拟网卡已禁用");
                return true;

            }
            else
            {
                SendMessage($"未找到可禁用的网卡");
                return false;
            }
        }

        private static bool DeleteAllLoopAdapters()
        {
            SendMessage("开始删除虚拟网卡");
            var adapterList = QuerySysinfo("Win32_NetworkAdapter", $"Caption like '%Loopback Adapter%'");
            if (adapterList.Count > 0)
            {
                foreach (var item in adapterList)
                {
                    SendMessage($"找到虚拟网卡{item["Name"]}");
                    RunCmd($"devcon.exe /r remove @{item["PNPDeviceID"]}", DEBUGWriteToConsole);

                }
                if (QuerySysinfo("Win32_NetworkAdapter", $"Caption like '%Loopback Adapter%'").Count == 0)
                {
                    SendMessage("删除成功！");
                    return false;
                }
                else
                {
                    SendMessage("删除失败！");
                    return false;
                }

            }
            else
            {
                SendErrorMessage($"没有可删除的虚拟网卡");
                return true;
            }
        }

        private static bool SetAdapter()
        {
            ManagementBaseObject inPar = null;
            ManagementObjectSearcher query = new ManagementObjectSearcher($"select * from Win32_NetworkAdapterConfiguration where IPEnabled = 1 and Caption like '%Loopback Adapter%'");
            foreach (ManagementObject mo in query.Get())
            {
                if (!(bool)mo["IPEnabled"]) continue;
                inPar = mo.GetMethodParameters("EnableStatic");
                inPar["IPAddress"] = new string[] { "221.231.130.70" };//ip地址  
                inPar["SubnetMask"] = new string[] { "255.255.255.0" }; //子网掩码   
                mo.InvokeMethod("EnableStatic", inPar, null);//执行 
                SendMessage("网卡信息设置成功！");
                return true;
            }
            SendErrorMessage("未找到可设置的虚拟网卡！");
            return false;
        }


        private static List<ManagementBaseObject> QueryAdapterByPnp(string PNPDeviceID)
        {
            var query = new ManagementObjectSearcher($"select * from Win32_NetworkAdapter where PNPDeviceID = '{PNPDeviceID.Replace(@"\", @"\\")}'");

            List<ManagementBaseObject> adapterIds = new List<ManagementBaseObject>();
            foreach (ManagementBaseObject item in query.Get())
            {
                adapterIds.Add(item);
            }

            return adapterIds;
        }


        private static List<ManagementBaseObject> QuerySysinfo(string Table, string whereClause)
        {

            var query = new ManagementObjectSearcher($"select * from {Table} where {whereClause}");

            List<ManagementBaseObject> adapterIds = new List<ManagementBaseObject>();
            foreach (var item in query.Get())
            {
                adapterIds.Add(item);
            }
            return adapterIds;
        }

        private static List<ManagementBaseObject> QueryInstalledLoopbackAdapters()
        {
            return QuerySysinfo("Win32_NetworkAdapter", "Caption like '%Loopback Adapter%'");
        }

        private static void RunCmd(string cmd, Action<Process> action)
        {

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序
            p.StandardInput.AutoFlush = true;
            p.StandardInput.WriteLine($"cd {ARIESDIR}\n{ARIESDIR[0] + ":"}");

            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(cmd + "&exit");

            //p.StandardInput.WriteLine("exit");
            //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
            //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令


            action?.Invoke(p);
            //获取cmd窗口的输出信息
            //string output = p.StandardOutput.ReadToEnd();

            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
            return;
        }

        #region 释放Devcon

        #region 重置网络
        public async static void ResetNetworkSettings()
        {

            await Task.Run(() =>
            {
                SendMessage("正在重置网络..,");
                SendMessage("正在清空重定向设置...");
                int[] ports = new int[13] { 8484, 8600, 8700, 7575, 7576, 7577, 7578, 7579, 7580, 7581, 7582, 7583, 7584, };
                StringBuilder sb = new StringBuilder();
                foreach (int port in ports)
                {
                    sb.Append($"netsh interface portproxy delete v4tov4 {port} 221.231.130.70 \n");
                    sb.Append($"netsh interface portproxy delete v4tov4 {port} 127.0.0.1 \n");
                }
                sb.Append("netsh int ip delete addr 1 221.231.130.70 \n");
                sb.Append("route delete 221.231.130.70");
                RunCmd(sb.ToString(), null);
                SendMessage("重定向设置已清除!");
                DeleteAllLoopAdapters();

                SendMessage("网络配置重置成功！");
            });


        }
        #endregion


        private static bool ReleaseDevcon()
        {
            Directory.CreateDirectory(ARIESDIR);
            string fileName = Environment.Is64BitOperatingSystem ? X64FILE : X86FILE;
            string validMD5 = Environment.Is64BitOperatingSystem ? X64MD5 : X86MD5;
            if (!FileUtil.FileMD5Validation(validMD5, OUTFILE))
            {

                byte[] devcon = Environment.Is64BitOperatingSystem ? Resource.devcon_x64 : Resource.devcon_x86;
                try
                {

                    using (var fs = File.Create(OUTFILE))
                    {
                        using (var bw = new BinaryWriter(fs))
                        {
                            bw.Write(devcon);
                        }
                    }
                }
                catch (Exception)
                {
                    SendErrorMessage("无法解压文件！");
                    return false;
                }
            }
            return true;
        }

        private static void DEBUGWriteToConsole(Process p)
        {
#if DEBUG
            Console.WriteLine(p.StandardOutput.ReadToEnd());
#endif

        }

        private static bool Ping()
        {
            int count = 0;
            while (!PingTest())
            {
                count++;
            }
            return count < 5 || PingTest();
        }

        private static bool PingTest()
        {
            using (var p = new Ping())
            {
                var options = new PingOptions
                {
                    DontFragment = true
                };
                string data = "Test Data!";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                int timeout = 1000; // Timeout 时间，单位：毫秒  
                var reply = p.Send("221.231.130.70", timeout, buffer, options);
                if (reply.Status == IPStatus.Success)
                    return true;
                else
                    return false;
            }
        }


        #endregion

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="Msg"></param>
        private static void SendMessage(string Msg)
        {
            WarpMessage?.Invoke(MessageType.Tips, Msg);
        }
        /// <summary>
        /// 发送错误消息
        /// </summary>
        /// <param name="Msg"></param>
        private static void SendErrorMessage(string Msg)
        {
            WarpMessage?.Invoke(MessageType.Error, Msg);
        }

    }
}
