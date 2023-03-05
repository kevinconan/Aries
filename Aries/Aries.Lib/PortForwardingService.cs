using Aris.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aries.Lib
{
    public class PortForwardingService
    {
        public WarpMessage WarpMessage;

        private Dictionary<int, PortForwardingWorker> workers;

        public PortForwardingService()
        {
            this.workers = new Dictionary<int, PortForwardingWorker>();
        }

        #region LifeCycle

        public async void Launch(Action<bool> callback)
        {
            await Task.Run(() =>
            {
                SendMessage("正在开启端口映射...");
                int count = 0;
                foreach (PortForwardingWorker worker in workers.Values)
                {
                    if (!worker.IsRunning)
                    {
                        worker.Start();
                    }
                }
                callback(true);
                //if (count > 0)
                //{
                //    Stop();
                //    callback(false);
                //}
                //else
                //{
                //    callback(true);
                //}
            });

        }

        public void Stop()
        {
            SendMessage("正在停止端口映射...");
            foreach (PortForwardingWorker worker in workers.Values)
            {
                try
                {
                    worker.Stop();
                }
                catch (Exception ex)
                {
                    SendErrorMessage($"停止端口映射出错：{ex}");
                }
            }
            workers.Clear();
            SendMessage("端口映射已停止");
        }

        #endregion

        #region Manage

        public void AddForwarding(int localPort, string host, int port)
        {
            var worker = workers.ContainsKey(localPort) ? workers[localPort] : null;
            if (worker == null)
            {
                worker = new PortForwardingWorker(localPort, host, port);
                worker.show += SendMessage;
                workers.Add(localPort, worker);
            }
        }


        #endregion


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
