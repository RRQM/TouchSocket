using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 命名管道服务器基类
    /// </summary>
    public abstract class NamedPipeServiceBase : SetupConfigObject, INamedPipeService
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Count => this.SocketClients.Count;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract IEnumerable<NamedPipeMonitor> Monitors { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract string ServerName { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract ServerState ServerState { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract INamedPipeSocketClientCollection SocketClients { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract int MaxCount { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetIds()
        {
            return this.SocketClients.GetIds();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="oldId"></param>
        /// <param name="newId"></param>
        public abstract void ResetId(string oldId, string newId);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public abstract IService Start();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public abstract IService Stop();

        internal Task OnInternalConnected(INamedPipeSocketClient socketClient, ConnectedEventArgs e)
        {
            return this.OnClientConnected(socketClient, e);
        }

        internal Task OnInternalConnecting(INamedPipeSocketClient socketClient, ConnectingEventArgs e)
        {
            return this.OnClientConnecting(socketClient, e);
        }

        internal Task OnInternalDisconnected(INamedPipeSocketClient socketClient, DisconnectEventArgs e)
        {
            return this.OnClientDisconnected(socketClient, e);
        }

        internal Task OnInternalDisconnecting(INamedPipeSocketClient socketClient, DisconnectEventArgs e)
        {
            return this.OnClientDisconnecting(socketClient, e);
        }

        internal Task OnInternalReceivedData(INamedPipeSocketClient socketClient, ReceivedDataEventArgs e)
        {
            return this.OnClientReceivedData(socketClient, e);
        }

        /// <summary>
        /// 客户端连接完成
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected abstract Task OnClientConnected(INamedPipeSocketClient socketClient, ConnectedEventArgs e);

        /// <summary>
        /// 客户端请求连接
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected abstract Task OnClientConnecting(INamedPipeSocketClient socketClient, ConnectingEventArgs e);

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected abstract Task OnClientDisconnected(INamedPipeSocketClient socketClient, DisconnectEventArgs e);

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// <para>
        /// </para>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected abstract Task OnClientDisconnecting(INamedPipeSocketClient socketClient, DisconnectEventArgs e);

        /// <summary>
        /// 收到数据时
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected abstract Task OnClientReceivedData(INamedPipeSocketClient socketClient, ReceivedDataEventArgs e);

        #region Id发送

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestInfo"></param>
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

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestInfo"></param>
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract bool SocketClientExist(string id);

        /// <summary>
        /// 添加一个地址监听。支持在服务器运行过程中动态添加。
        /// </summary>
        /// <param name="option"></param>
        public abstract void AddListen(NamedPipeListenOption option);

        /// <summary>
        /// 移除一个地址监听。支持在服务器运行过程中动态移除。
        /// </summary>
        /// <param name="monitor">监听器</param>
        /// <returns>返回是否已成功移除</returns>
        public abstract bool RemoveListen(NamedPipeMonitor monitor);
    }
}