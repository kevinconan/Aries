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
            var cancellationToken = CancellationTokenSource.Token;
            IPHostEntry remoteHost = await Dns.GetHostEntryAsync(RemoteHost);
            if (remoteHost.AddressList.Length == 0)
            {
                OnError(new ArgumentException($"Cannot resolve remote host: {RemoteHost}"));
                return;
            }

            listener = new TcpListener(localIP, LocalPort);
            listener.Start();
            OnStart();
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    ThreadPool.QueueUserWorkItem(async delegate
                    {
                        using (client)
                        {
                            TcpClient server = new TcpClient();
                            await ConnectWithRetryAsync(server, remoteHost.AddressList[0], RemotePort, cancellationToken);

                            using (server)
                            using (NetworkStream clientStream = client.GetStream())
                            using (NetworkStream serverStream = server.GetStream())
                            {
                                await Task.WhenAny(
                                    PortForward(client, clientStream, serverStream, localIP, LocalPort, cancellationToken),
                                    PortForward(server, serverStream, clientStream, remoteHost.AddressList[0], RemotePort, cancellationToken));
                            }
                        }
                    });
                }
                catch (Exception ex) { OnError(ex); }
            }
            OnStop();
        }

        private async Task ConnectWithRetryAsync(TcpClient client, IPAddress address, int port, CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    await client.ConnectAsync(address, port).ConfigureAwait(false);
                    return;
                }
                catch
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
        }

        private async Task PortForward(TcpClient client, NetworkStream from, NetworkStream to, IPAddress ip, int port, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await from.CopyToAsync(to, 1024, token);
                }
                catch (Exception ex)
                {
                    OnError(ex);

                    await ConnectWithRetryAsync(client, ip, port, token);
                    from.Flush(); // 清空缓冲区
                    to.Flush(); // 清空缓冲区
                }
            }
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
            Console.WriteLine("Port forwarding stopped.");
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
