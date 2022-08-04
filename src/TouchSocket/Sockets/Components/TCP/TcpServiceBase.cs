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
using System.Collections.Generic;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Plugins;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// Tcp服务器基类
    /// </summary>
    public abstract class TcpServiceBase : BaseSocket, ITcpService
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract TouchSocketConfig Config { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract IContainer Container { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Count => this.SocketClients.Count;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract NetworkMonitor[] Monitors { get; }

        /// <summary>
        /// 插件管理器
        /// </summary>
        public abstract IPluginsManager PluginsManager { get; }

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
        public abstract SocketClientCollection SocketClients { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract bool UseSsl { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract int ClearInterval { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract ClearType ClearType { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract Func<string> GetDefaultNewID { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract int MaxCount { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract bool UsePlugin { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public string[] GetIDs()
        {
            return this.SocketClients.GetIDs();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="oldID"></param>
        /// <param name="newID"></param>
        public abstract void ResetID(string oldID, string newID);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="serverConfig"></param>
        /// <returns></returns>
        public abstract IService Setup(TouchSocketConfig serverConfig);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public abstract IService Setup(int port);

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

        internal void OnInternalConnected(ISocketClient socketClient, TouchSocketEventArgs e)
        {
            this.OnClientConnected(socketClient, e);
        }

        internal void OnInternalConnecting(ISocketClient socketClient, ClientOperationEventArgs e)
        {
            this.OnClientConnecting(socketClient, e);
        }

        internal void OnInternalDisconnected(ISocketClient socketClient, ClientDisconnectedEventArgs e)
        {
            this.OnClientDisconnected(socketClient, e);
        }

        internal void OnInternalReceivedData(ISocketClient socketClient, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.OnClientReceivedData(socketClient, byteBlock, requestInfo);
        }

        /// <summary>
        /// 客户端连接完成
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected abstract void OnClientConnected(ISocketClient socketClient, TouchSocketEventArgs e);

        /// <summary>
        /// 客户端请求连接
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected abstract void OnClientConnecting(ISocketClient socketClient, ClientOperationEventArgs e);

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected abstract void OnClientDisconnected(ISocketClient socketClient, ClientDisconnectedEventArgs e);

        /// <summary>
        /// 收到数据时
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected abstract void OnClientReceivedData(ISocketClient socketClient, ByteBlock byteBlock, IRequestInfo requestInfo);

        #region ID发送

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
            if (this.SocketClients.TryGetSocketClient(id, out ISocketClient client))
            {
                client.Send(buffer, offset, length);
            }
            else
            {
                throw new ClientNotFindException(ResType.ClientNotFind.GetDescription(id));
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestInfo"></param>
        public void Send(string id, IRequestInfo requestInfo)
        {
            if (this.SocketClients.TryGetSocketClient(id, out ISocketClient client))
            {
                client.Send(requestInfo);
            }
            else
            {
                throw new ClientNotFindException(ResType.ClientNotFind.GetDescription(id));
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
        public void SendAsync(string id, byte[] buffer, int offset, int length)
        {
            if (this.SocketClients.TryGetSocketClient(id, out ISocketClient client))
            {
                client.SendAsync(buffer, offset, length);
            }
            else
            {
                throw new ClientNotFindException(ResType.ClientNotFind.GetDescription(id));
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestInfo"></param>
        public void SendAsync(string id, IRequestInfo requestInfo)
        {
            if (this.SocketClients.TryGetSocketClient(id, out ISocketClient client))
            {
                client.SendAsync(requestInfo);
            }
            else
            {
                throw new ClientNotFindException(ResType.ClientNotFind.GetDescription(id));
            }
        }

        public abstract bool SocketClientExist(string id);

        #endregion ID发送
    }
}