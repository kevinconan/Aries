using Aries.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aris.Lib
{
    static class TcpClientExtention
    {
        public static bool Connected(this TcpClient c)
        {
            var s = c.Client;
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
    }

    class PortForwardingWorker
    {

        public WarpMessage WarpMessage;

        public bool IsRunning { get; set; } = false;

        string host;
        int port;
        int localPort;
        Dictionary<NetworkStream, bool> first = new Dictionary<NetworkStream, bool>();
        TcpListener listener;

        public PortForwardingWorker(int localPort, string host, int port)
        {
            this.host = host;
            this.port = port;
            this.localPort = localPort;

            listener = new TcpListener(IPAddress.Parse("221.231.130.70"), localPort);
        }

        void Pipe(NetworkStream a, NetworkStream b)
        {
            a.CopyToAsync(b);
            b.CopyToAsync(a);
        }

        public void stop()
        {
            IsRunning = false;
            listener.Stop();
        }

        public async void start()
        {
            IsRunning = true;
            try
            {
                listener.Start();
            }
            catch (Exception)
            {
                SendErrorMessage($"端口映射启动失败,请检查端口[{port}]是否被占用");
            }
            

            while (IsRunning)
            {
                try
                {
                    var outgoing = await listener.AcceptTcpClientAsync();

                    var remote = new TcpClient(host, port);

                    var localStream = new NetworkStream(outgoing.Client);
                    var remoteStream = new NetworkStream(remote.Client);
                    first[localStream] = true;

                    Pipe(localStream, remoteStream);
                    //CopyTo("client：",localStream, remoteStream);
                    //CopyTo("server：",remoteStream, localStream);

                    new Thread(() =>
                    {
                        while (true)
                        {
                            if (!remote.Connected() || !outgoing.Connected())
                            {
                                localStream.Close();
                                remoteStream.Close();
                                remote.Close();
                                outgoing.Close();

                                return;
                            }
                            Thread.Sleep(1000);
                        }
                    })
                    { IsBackground = true }.Start();
                }
                catch { }
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="Msg"></param>
        private void SendMessage(string Msg)
        {
            WarpMessage?.Invoke(MessageType.Tips, Msg);
        }
        /// <summary>
        /// 发送错误消息
        /// </summary>
        /// <param name="Msg"></param>
        private void SendErrorMessage(string Msg)
        {
            WarpMessage?.Invoke(MessageType.Error, Msg);
        }

    }
}
