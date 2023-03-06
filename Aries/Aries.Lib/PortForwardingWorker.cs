using System;
using System.Net;
using System.Net.Sockets;
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
        public bool IsRunning { get; set; } = false;

        private CancellationTokenSource CancellationTokenSource { get; set; }
        private TcpListener listener;

        public string RemoteHost { get; private set; }
        public int RemotePort { get; private set; }
        public int LocalPort { get; private set; }

        public Action<string> show;

        public PortForwardingWorker(int localPort, string host, int port)
        {
            RemoteHost = host;
            RemotePort = port;
            LocalPort = localPort;
        }

        private async Task PortForwardWithIOCP(IPAddress localIP)
        {
            var token = CancellationTokenSource.Token;

            listener = new TcpListener(localIP, LocalPort);
            listener.Start();
            OnStart();

            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Console.Out.WriteLineAsync($"[{localIP}:{LocalPort}]开始侦听……");

                    TcpClient client = null;
                    TcpClient server = null;

                    try
                    {
                        client = await listener.AcceptTcpClientAsync();
                    }
                    catch
                    {
                        // 停止之后会多一次循环，这里会报错，没找到好的判断方式
                        await Console.Out.WriteLineAsync($"[{localIP}:{LocalPort}]侦听已停止……");
                        break;
                    }

                    ThreadPool.QueueUserWorkItem(async delegate
                    {
                        using (client)
                        {
                            server = new TcpClient();
                            await server.ConnectAsync(RemoteHost, RemotePort);

                            using (server)
                            {
                                // 这个方法抛不出异常，在里面处理完
                                await Task.WhenAny(PortForward(client, server, token), PortForward(server, client, token));
                            }
                        }
                    });
                }
                catch (Exception ex) { OnError(ex); }
            }

            OnStop();
        }

        private async Task ConnectWithRetryAsync(TcpClient client, CancellationToken token)
        {
            var endpoint = GetClientIpEndPoint(client);
            while (!token.IsCancellationRequested)
            {
                await Console.Out.WriteLineAsync($"[{endpoint.Address}:{endpoint.Port}]正在建立连接……");
                try
                {
                    await client.ConnectAsync(endpoint.Address, endpoint.Port).ConfigureAwait(false);
                    return;
                }
                catch
                {
                    if (token.IsCancellationRequested)
                    {
                        throw;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                }
            }
        }

        private async Task PortForward(TcpClient inClient, TcpClient outClient, CancellationToken token)
        {
            var endpoint = GetClientIpEndPoint(inClient);
            var bufferSize = 8;
            await Console.Out.WriteLineAsync($"[{endpoint}] buffer size: {bufferSize}");

            using (var inStream = inClient.GetStream())
            using (var outStream = outClient.GetStream())
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await inStream.CopyToAsync(outStream, bufferSize, token);
                    }
                    catch (Exception ex)
                    {
                        await Console.Out.WriteLineAsync($"[{endpoint}]连接中断：{ex.Message}");
                        inStream.Flush(); // 清空缓冲区
                        outStream.Flush(); // 清空缓冲区
                        break;
                    }
                }
            }

        }

        private IPEndPoint GetClientIpEndPoint(TcpClient client)
        {
            return client.Client.RemoteEndPoint as IPEndPoint;
        }

        public void Stop()
        {
            try
            {
                IsRunning = false;
                CancellationTokenSource?.Cancel();
                listener?.Stop(); // TODO 不关无法开启新的，关又一定会抛异常，关的方式不对，需要解决
            }
            catch (Exception ex)
            {
                Console.WriteLine($"停止端口映射时出错：{ex}");
            }

        }

        public async void Start()
        {
            IsRunning = true;
            CancellationTokenSource = new CancellationTokenSource();
            await PortForwardWithIOCP(IPAddress.Parse("0.0.0.0"));
        }

        void OnStart()
        {
            show?.Invoke($"端口[221.231.130.70:{LocalPort}->{RemoteHost}:{RemotePort}]映射成功");
        }

        void OnStop()
        {
            //Console.WriteLine("Port forwarding stopped.");
        }


        void OnError(Exception ex)
        {
            if (!(ex is ObjectDisposedException) && !(ex is InvalidOperationException))
            {
                show?.Invoke($"端口映射启动失败,原因:{ex.Message}");
            }
        }
    }
}
