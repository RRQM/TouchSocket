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

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 端口转发辅助
    /// </summary>
    public class NATSocketClient : SocketClient, INATSocketClient
    {
        internal Action<NATSocketClient, ITcpClient, ClientDisconnectedEventArgs> m_internalDis;
        internal Func<NATSocketClient, ITcpClient, ByteBlock, IRequestInfo, byte[]> m_internalTargetClientRev;
        private readonly ConcurrentList<ITcpClient> m_targetClients = new ConcurrentList<ITcpClient>();

        /// <inheritdoc/>
        public ITcpClient AddTargetClient(TouchSocketConfig config, Action<ITcpClient> setupAction = default)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Disconnected += TcpClient_Disconnected;
            tcpClient.Received += TcpClient_Received;
            tcpClient.Setup(config);
            setupAction?.Invoke(tcpClient);
            tcpClient.Connect();

            m_targetClients.Add(tcpClient);
            return tcpClient;
        }

        /// <inheritdoc/>
        public Task<ITcpClient> AddTargetClientAsync(TouchSocketConfig config, Action<ITcpClient> setupAction = default)
        {
            return EasyTask.Run(() =>
            {
                return AddTargetClient(config, setupAction);
            });
        }

        /// <inheritdoc/>
        public ITcpClient[] GetTargetClients()
        {
            return m_targetClients.ToArray();
        }

        /// <inheritdoc/>
        public void SendToTargetClient(byte[] buffer, int offset, int length)
        {
            foreach (var socket in m_targetClients)
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

        /// <inheritdoc/>
        protected override void OnDisconnected(ClientDisconnectedEventArgs e)
        {
            foreach (var client in m_targetClients)
            {
                client.TryShutdown();
                client.SafeDispose();
            }
            base.OnDisconnected(e);
        }

        private void TcpClient_Disconnected(ITcpClientBase client, ClientDisconnectedEventArgs e)
        {
            client.Dispose();
            m_targetClients.Remove((ITcpClient)client);
            m_internalDis?.Invoke(this, (ITcpClient)client, e);
        }

        private void TcpClient_Received(TcpClient client, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (DisposedValue)
            {
                return;
            }

            try
            {
                var data = m_internalTargetClientRev?.Invoke(this, client, byteBlock, requestInfo);
                if (data != null)
                {
                    if (Online)
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