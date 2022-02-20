//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore;
using RRQMCore.ByteManager;

using System.Collections.Generic;

namespace RRQMSocket
{
    /// <summary>
    /// Tcp服务器基类
    /// </summary>
    public abstract class TcpServiceBase : BaseSocket, ITcpServiceBase, IIDSender
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract NetworkMonitor[] Monitors { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract ServerState ServerState { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract ServiceConfig ServiceConfig { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract string ServerName { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract SocketClientCollection SocketClients { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract bool UseSsl { get; }

        /// <summary>
        /// 客户端连接完成
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected abstract void OnClientConnected(ISocketClient socketClient, MesEventArgs e);

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected abstract void OnClientDisconnected(ISocketClient socketClient, MesEventArgs e);

        /// <summary>
        /// 客户端请求连接
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected abstract void OnClientConnecting(ISocketClient socketClient, ClientOperationEventArgs e);

        internal void OnInternalConnected(ISocketClient socketClient, MesEventArgs e)
        {
            this.OnClientConnected(socketClient, e);
        }

        internal void OnInternalDisconnected(ISocketClient socketClient, MesEventArgs e)
        {
            this.OnClientDisconnected(socketClient, e);
        }

        internal void OnInternalConnecting(ISocketClient socketClient, ClientOperationEventArgs e)
        {
            this.OnClientConnecting(socketClient, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="waitSetID"></param>
        public abstract void ResetID(WaitSetID waitSetID);

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
        public abstract void Clear();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="serverConfig"></param>
        /// <returns></returns>
        public abstract IService Setup(ServiceConfig serverConfig);

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

        #region ID发送

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="buffer"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Send(string id, byte[] buffer)
        {
            this.Send(id, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Send(string id, byte[] buffer, int offset, int length)
        {
            if (this.SocketClients.TryGetSocketClient(id, out ISocketClient client))
            {
                client.Send(buffer, offset, length);
            }
            else
            {
                throw new ClientNotFindException(ResType.ClientNotFind.GetResString(id));
            }
        }

        /// <summary>
        /// 发送流中的有效数据
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="byteBlock"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Send(string id, ByteBlock byteBlock)
        {
            this.Send(id, byteBlock.Buffer, 0, byteBlock.Len);
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="buffer"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void SendAsync(string id, byte[] buffer)
        {
            this.SendAsync(id, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void SendAsync(string id, byte[] buffer, int offset, int length)
        {
            if (this.SocketClients.TryGetSocketClient(id, out ISocketClient client))
            {
                client.SendAsync(buffer, offset, length);
            }
            else
            {
                throw new ClientNotFindException(ResType.ClientNotFind.GetResString(id));
            }
        }

        /// <summary>
        /// 发送流中的有效数据
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="byteBlock"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void SendAsync(string id, ByteBlock byteBlock)
        {
            this.SendAsync(id, byteBlock.Buffer, 0, byteBlock.Len);
        }

        #endregion ID发送
    }
}