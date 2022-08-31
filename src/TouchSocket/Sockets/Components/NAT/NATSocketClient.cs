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
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Collections.Concurrent;
using TouchSocket.Core.Config;
using TouchSocket.Sockets.Plugins;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 端口转发辅助
    /// </summary>
    public class NATSocketClient : SocketClient
    {
        internal Action<NATSocketClient, ITcpClient, ClientDisconnectedEventArgs> m_internalDis;
        internal Func<NATSocketClient, ITcpClient, ByteBlock, IRequestInfo, byte[]> m_internalTargetClientRev;
        private readonly ConcurrentList<ITcpClient> m_targetClients = new ConcurrentList<ITcpClient>();

        /// <summary>
        /// 添加转发客户端。
        /// </summary>
        /// <param name="config">配置文件</param>
        /// <param name="setupAction">当完成配置，但是还未连接时回调。</param>
        /// <returns></returns>
        public ITcpClient AddTargetClient(TouchSocketConfig config, Action<ITcpClient> setupAction = default)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Disconnected += this.TcpClient_Disconnected;
            tcpClient.Received += this.TcpClient_Received;
            tcpClient.Setup(config);
            setupAction?.Invoke(tcpClient);
            tcpClient.Connect();

            this.m_targetClients.Add(tcpClient);
            return tcpClient;
        }

        /// <summary>
        /// 添加转发客户端。
        /// </summary>
        /// <param name="config">配置文件</param>
        /// <param name="setupAction">当完成配置，但是还未连接时回调。</param>
        /// <returns></returns>
        public Task<ITcpClient> AddTargetClientAsync(TouchSocketConfig config, Action<ITcpClient> setupAction = default)
        {
            return Task.Run(() =>
            {
                return this.AddTargetClient(config, setupAction);
            });
        }

        /// <summary>
        /// 获取所有目标客户端
        /// </summary>
        /// <returns></returns>
        public ITcpClient[] GetTargetClients()
        {
            return this.m_targetClients.ToArray();
        }

        /// <summary>
        /// 发送数据到全部转发端。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void SendToTargetClient(byte[] buffer, int offset, int length)
        {
            foreach (var socket in this.m_targetClients)
            {
                try
                {
                    socket.Send(buffer, offset, length);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDisconnected(ClientDisconnectedEventArgs e)
        {
            foreach (var client in this.m_targetClients)
            {
                client.SafeShutdown();
                client.SafeDispose();
            }
            base.OnDisconnected(e);
        }

        private void TcpClient_Disconnected(ITcpClientBase client, ClientDisconnectedEventArgs e)
        {
            client.SafeDispose();
            this.m_targetClients.Remove((ITcpClient)client);
            this.m_internalDis?.Invoke(this, (ITcpClient)client, e);
        }

        private void TcpClient_Received(TcpClient client, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.m_disposedValue)
            {
                return;
            }

            try
            {
                var data = this.m_internalTargetClientRev?.Invoke(this, client, byteBlock, requestInfo);
                if (data != null)
                {
                    if (this.Online)
                    {
                        this.Send(data);
                    }
                }
            }
            catch
            {
            }
        }
    }
}