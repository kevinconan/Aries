using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aries.Lib
{
    public enum MapleStoryWindowType
    {
        AdvStart = 0,
        MainWindow = 1,
        AdvEnd = 2
    }

    public enum MessageType
    {
        Tips = 0,
        Error = 1
    }

    public delegate void OnMapleStoryWindowChange(MapleStoryWindowType windowType);

    public delegate void OnMapleStoryShutdown();

    public delegate void OnMapleStoryStartFail();

    public delegate void OnMapleStoryStartSuccess();

    public delegate void WarpMessage(MessageType type, string message);

    public delegate string GetMapleMainPath();

    public class MapleStoryInspector
    {
        #region Win32API
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        private static extern int PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("User32.dll", EntryPoint = "SetWindowText", CharSet = CharSet.Ansi)]
        private static extern int SetWindowText(IntPtr hWnd, string lpString);
        #endregion

        public OnMapleStoryWindowChange OnMapleStoryWindowChange;

        public OnMapleStoryShutdown OnMapleStoryShutdown;

        public OnMapleStoryStartFail OnMapleStoryStartFail;

        public OnMapleStoryStartSuccess OnMapleStoryStartSuccess;

        public GetMapleMainPath GetMapleMainPath;

        public WarpMessage WarpMessage;

        public string MapleStoryExe;

        private Process MapleProcess;

        #region 冒险岛监视线程

        private Thread MainInspectingThread;

        #endregion

        #region 构造
        public MapleStoryInspector() { }
        public MapleStoryInspector(string filePath)
        {
            this.MapleStoryExe = filePath;
        }
        #endregion

        #region LifeCycle

        public void Launch()
        {
            SendMessage("正在启动冒险岛....");
            StartMaple();
        }

        public void Stop()
        {
            try
            {
                
                MainInspectingThread.Abort();
                MapleProcess.Kill();
            }
            catch (Exception)
            {

            }

        }

        #endregion

        #region 线程方法
        private void MaingInspectingWorking()
        {
            while (MainInspectingThread.IsAlive)
            {
                while (true)
                {
                    Process process = findExistMapleProccess();
                    if (process != null && !process.HasExited)
                    {
                        if (process.MainWindowHandle != IntPtr.Zero)
                        {
                            if (process.MainWindowTitle == "MapleStory")
                            {
                                int res = SetWindowText(process.MainWindowHandle, "冒险岛私服登录器 By Kevinconan");
                                if (res == 1)
                                {
                                    return;
                                }
                            }
                        }
#if DEBUG
                        Console.WriteLine("冒险岛窗口句柄----->>>>>"+process.MainWindowHandle);

#endif

                    }

                    Thread.Sleep(100);
                }
            }
        }


        #endregion

        #region WorkingMethods

        private void StartMaple()
        {
            MapleProcess = findExistMapleProccess();

            if (MapleProcess == null)
            {
                Thread launchNewMapleThread = new Thread(LaunchNewMaple) { IsBackground = true };
                launchNewMapleThread.Start();

            }else
            {
                HookMapleProcess();
            }

            
        }

        private void LaunchNewMaple()
        {
            if (!File.Exists(MapleStoryExe))
            {
                if (GetMapleMainPath != null)
                {
                    SendMessage("当前设置的冒险岛主程序路径错误，请选择...");
                    MapleStoryExe = GetMapleMainPath();

                    if (MapleStoryExe == "" || MapleStoryExe == null)
                    {
                        SendMessage("用户已取消！");
                        if (OnMapleStoryStartFail != null)
                        {
                            OnMapleStoryStartFail();
                        }
                        return;
                    }
                }
                else
                {
                    SendErrorMessage("找不到冒险岛主程序，启动失败！");
                    if (OnMapleStoryStartFail != null)
                    {
                        OnMapleStoryStartFail();
                    }
                    return;
                }
            }

            MapleProcess = Process.Start(MapleStoryExe, "221.231.130.70 8484");

            DateTime start = DateTime.Now;
            IntPtr handle = IntPtr.Zero;

            while (handle == IntPtr.Zero && DateTime.Now - start <= TimeSpan.FromSeconds(15))
            {
                try
                {
                    // sleep a while to allow the MainWindow to open...
                    System.Threading.Thread.Sleep(50);
                    handle = MapleProcess.MainWindowHandle;
                }
                catch (Exception) {
                   
                    return ;
                }
            }

            if (handle == IntPtr.Zero)
            {
                SendErrorMessage("检测冒险岛进程超时...启动失败");
                if (OnMapleStoryStartFail != null)
                {
                    OnMapleStoryStartFail();
                }
            }

            MapleProcess.CloseMainWindow();
            SendMessage("已跳过引导页....");

            HookMapleProcess();
        }

        private void HookMapleProcess()
        {
            MapleProcess.EnableRaisingEvents = true;
            MapleProcess.Exited += new EventHandler(ProcessExited);
            SendMessage("冒险岛进程Hook完毕...");

            if (OnMapleStoryStartSuccess != null)
            {
                OnMapleStoryStartSuccess();
                SendMessage("启动成功！");
            }

            MainInspectingThread = new Thread(MaingInspectingWorking) { IsBackground = true };
            MainInspectingThread.Start();
            
        }

        private void ProcessExited(object sender, EventArgs e)
        {
            SendMessage("冒险岛已退出...");
            OnMapleStoryShutdown();
        }



        /// <summary>
        /// 查找冒险岛进程
        /// </summary>
        /// <returns></returns>
        private Process findExistMapleProccess()
        {
            Process proc = null;
            Process[] procs = Process.GetProcessesByName("MapleStory");
            foreach (Process process in procs)
            {
                proc = process;
            }
            return proc;
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="Msg"></param>
        private void SendMessage(string Msg)
        {
            if (WarpMessage != null)
            {
                WarpMessage(MessageType.Tips, Msg);
            }
        }
        /// <summary>
        /// 发送错误消息
        /// </summary>
        /// <param name="Msg"></param>
        private void SendErrorMessage(string Msg)
        {
            if (WarpMessage != null)
            {
                WarpMessage(MessageType.Error, Msg);
            }
            {

            }
        }

        #endregion
    }
}
