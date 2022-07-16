using System;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Plugins;

namespace TouchSocket.Sockets.Plugins
{
    /// <summary>
    /// 重连插件
    /// </summary>
    [SingletonPlugin]
    public sealed class ReconnectionPlugin<TClient> : TcpPluginBase
    {
        private readonly bool m_printLog;
        private readonly int m_sleepTime;
        private readonly Action<TClient> m_successCallback;
        private readonly int m_tryCount;

        /// <summary>
        /// 初始化一个重连插件
        /// </summary>
        /// <param name="tryCount"></param>
        /// <param name="successCallback"></param>
        /// <param name="printLog"></param>
        /// <param name="sleepTime"></param>
        [DependencyInject(10, true, 1000, null)]
        public ReconnectionPlugin(int tryCount, bool printLog, int sleepTime, Action<TClient> successCallback)
        {
            this.m_tryCount = tryCount;
            this.m_successCallback = successCallback;
            this.m_printLog = printLog;
            this.m_sleepTime = sleepTime;
            this.Order = int.MinValue;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnDisconnected(ITcpClientBase client, ClientDisconnectedEventArgs e)
        {
            base.OnDisconnected(client, e);

            if (client is ITcpClient tcpClient)
            {
                if (e.Manual)
                {
                    return;
                }
                Task.Run(() =>
                {
                    int tryT = this.m_tryCount;
                    while (this.m_tryCount < 0 || tryT-- > 0)
                    {
                        try
                        {
                            if (tcpClient.Online)
                            {
                                return;
                            }
                            else
                            {
                                tcpClient.Connect();
                            }

                            this.m_successCallback?.Invoke((TClient)tcpClient);
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (this.m_printLog)
                            {
                                client.Logger.Debug(TouchSocket.Core.Log.LogType.Error, client, "断线重连失败。", ex);
                            }
                            Thread.Sleep(this.m_sleepTime);
                        }
                    }
                });
            }
        }
    }
}