//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// Tcp服务器基类
    /// </summary>
    public abstract class TcpServiceBase : ServiceBase, ITcpServiceBase
    {
        private readonly ConcurrentStack<TcpCore> m_tcpCores = new ConcurrentStack<TcpCore>();

        /// <inheritdoc/>
        public int Count => this.SocketClients.Count;

        /// <inheritdoc/>
        public abstract int MaxCount { get; }

        /// <inheritdoc/>
        public abstract IEnumerable<TcpNetworkMonitor> Monitors { get; }

        /// <inheritdoc/>
        public abstract ISocketClientCollection SocketClients { get; }

        /// <inheritdoc/>
        public abstract void AddListen(TcpListenOption options);

        /// <inheritdoc/>
        public abstract void Clear();

        /// <inheritdoc/>
        public IEnumerable<string> GetIds()
        {
            return this.SocketClients.GetIds();
        }

        /// <inheritdoc/>
        public abstract bool RemoveListen(TcpNetworkMonitor monitor);

        /// <summary>
        /// 租用TcpCore
        /// </summary>
        /// <returns></returns>
        public TcpCore RentTcpCore()
        {
            if (this.m_tcpCores.TryPop(out var tcpCore))
            {
                if (!tcpCore.DisposedValue)
                {
                    return tcpCore;
                }
            }

            return new InternalTcpCore();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="oldId"></param>
        /// <param name="newId"></param>
        public abstract void ResetId(string oldId, string newId);

        /// <summary>
        /// 归还TcpCore
        /// </summary>
        /// <param name="tcpCore"></param>
        public void ReturnTcpCore(TcpCore tcpCore)
        {
            if (tcpCore.DisposedValue)
            {
                return;
            }

            if (this.DisposedValue)
            {
                tcpCore.SafeDispose();
                return;
            }
            this.m_tcpCores.Push(tcpCore);
        }

        /// <inheritdoc/>
        public abstract bool SocketClientExist(string id);

        internal Task OnInternalConnected(ISocketClient socketClient, ConnectedEventArgs e)
        {
            return this.OnClientConnected(socketClient, e);
        }

        internal Task OnInternalConnecting(ISocketClient socketClient, ConnectingEventArgs e)
        {
            return this.OnClientConnecting(socketClient, e);
        }

        internal Task OnInternalDisconnected(ISocketClient socketClient, DisconnectEventArgs e)
        {
            return this.OnClientDisconnected(socketClient, e);
        }

        internal Task OnInternalDisconnecting(ISocketClient socketClient, DisconnectEventArgs e)
        {
            return this.OnClientDisconnecting(socketClient, e);
        }

        internal Task OnInternalReceivedData(ISocketClient socketClient, ReceivedDataEventArgs e)
        {
            return this.OnClientReceivedData(socketClient, e);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            while (this.m_tcpCores.TryPop(out var tcpCore))
            {
                tcpCore.SafeDispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 客户端连接完成
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected abstract Task OnClientConnected(ISocketClient socketClient, ConnectedEventArgs e);

        /// <summary>
        /// 客户端请求连接
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected abstract Task OnClientConnecting(ISocketClient socketClient, ConnectingEventArgs e);

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected abstract Task OnClientDisconnected(ISocketClient socketClient, DisconnectEventArgs e);

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected abstract Task OnClientDisconnecting(ISocketClient socketClient, DisconnectEventArgs e);

        /// <summary>
        /// 收到数据时
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected abstract Task OnClientReceivedData(ISocketClient socketClient, ReceivedDataEventArgs e);

        #region Id发送

        /// <inheritdoc/>
        public void Send(string id, byte[] buffer, int offset, int length)
        {
            if (this.SocketClients.TryGetSocketClient(id, out var client))
            {
                client.Send(buffer, offset, length);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketResource.ClientNotFind.GetDescription(id));
            }
        }

        /// <inheritdoc/>
        public void Send(string id, IRequestInfo requestInfo)
        {
            if (this.SocketClients.TryGetSocketClient(id, out var client))
            {
                client.Send(requestInfo);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketResource.ClientNotFind.GetDescription(id));
            }
        }

        /// <inheritdoc/>
        public Task SendAsync(string id, byte[] buffer, int offset, int length)
        {
            if (this.SocketClients.TryGetSocketClient(id, out var client))
            {
                return client.SendAsync(buffer, offset, length);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketResource.ClientNotFind.GetDescription(id));
            }
        }

        /// <inheritdoc/>
        public Task SendAsync(string id, IRequestInfo requestInfo)
        {
            if (this.SocketClients.TryGetSocketClient(id, out var client))
            {
                return client.SendAsync(requestInfo);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketResource.ClientNotFind.GetDescription(id));
            }
        }

        #endregion Id发送
    }
}