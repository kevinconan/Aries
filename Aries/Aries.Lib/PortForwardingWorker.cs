using System;
using System.Collections.Generic;
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

        public CancellationTokenSource cancellationTokenSource { get; set; }

        string host;
        int port;
        int localPort;

        public Action<string> show;

        public PortForwardingWorker(int localPort, string host, int port)
        {
            this.host = host;
            this.port = port;
            this.localPort = localPort;

        }

        public static async Task PortForwardWithIOCP(IPAddress localIP, int localPort, string remoteHostName, int remotePort, Action onStart, Action onStop, Action<Exception> onError, CancellationToken cancellationToken)
        {
            IPHostEntry remoteHost = await Dns.GetHostEntryAsync(remoteHostName);
            if (remoteHost.AddressList.Length == 0)
            {
                onError?.Invoke(new ArgumentException($"Cannot resolve remote host: {remoteHostName}"));
                return ;
            }

            TcpListener listener = new TcpListener(localIP, localPort);
            listener.Start();
            onStart?.Invoke();
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    ThreadPool.QueueUserWorkItem(async delegate {
                        using (client)
                        {
                            TcpClient server = new TcpClient();
                            await server.ConnectAsync(remoteHost.AddressList[0], remotePort);
                            using (server)
                            using (NetworkStream clientStream = client.GetStream())
                            using (NetworkStream serverStream = server.GetStream())
                            {
                                var clientToServer = Task.Run(async () => {
                                    byte[] buffer = new byte[1024];
                                    try
                                    {
                                        while (!cancellationToken.IsCancellationRequested)
                                        {
                                            int bytesRead = await clientStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                                            await serverStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        onError?.Invoke(ex);
                                    }
                                }, cancellationToken);

                                var serverToClient = Task.Run(async () => {
                                    byte[] buffer = new byte[1024];
                                    try
                                    {
                                        while (!cancellationToken.IsCancellationRequested)
                                        {
                                            int bytesRead = await serverStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                                            await clientStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        onError?.Invoke(ex);
                                    }
                                }, cancellationToken);

                                await Task.WhenAny(clientToServer, serverToClient);
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                }
            }
            onStop?.Invoke();
        }

        public void stop()
        {
            IsRunning = false;
            cancellationTokenSource.Cancel();
        }

        public async void start()
        {
            IsRunning = true;


            Action onStart = () => { show?.Invoke($"端口[221.231.130.70:{localPort}->{host}:{port}]映射成功"); };
            Action onStop = () => Console.WriteLine("Port forwarding stopped.");
            Action<Exception> onError = ex => show?.Invoke($"端口映射启动失败,原因:{ex.Message}");

            cancellationTokenSource = new CancellationTokenSource();
            var task = PortForwardWithIOCP(IPAddress.Parse("0.0.0.0"), localPort, host, port, onStart, onStop, onError, cancellationTokenSource.Token);


        }

    }
}
