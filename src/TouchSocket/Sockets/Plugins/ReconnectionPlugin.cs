//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
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
    public sealed class ReconnectionPlugin<TClient> : TcpPluginBase where TClient : class, ITcpClient
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