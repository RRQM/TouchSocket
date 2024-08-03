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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// SessionClient
    /// </summary>
    [DebuggerDisplay("Id={Id},IPAdress={IP}:{Port}")]
    public abstract class TcpSessionClient : TcpSessionClientBase, ITcpSessionClient
    {
        #region 变量

        private Func<TcpSessionClient, ConnectedEventArgs, Task> m_onClientConnected;
        private Func<TcpSessionClient, ConnectingEventArgs, Task> m_onClientConnecting;
        private Func<TcpSessionClient, ClosedEventArgs, Task> m_onClientDisconnected;
        private Func<TcpSessionClient, ClosingEventArgs, Task> m_onClientDisconnecting;
        private Func<TcpSessionClient, ReceivedDataEventArgs, Task> m_onClientReceivedData;

        #endregion 变量

        internal void SetAction(Func<TcpSessionClient, ConnectingEventArgs, Task> onClientConnecting, Func<TcpSessionClient, ConnectedEventArgs, Task> onClientConnected, Func<TcpSessionClient, ClosingEventArgs, Task> onClientDisconnecting, Func<TcpSessionClient, ClosedEventArgs, Task> onClientDisconnected, Func<TcpSessionClient, ReceivedDataEventArgs, Task> onClientReceivedData)
        {
            this.m_onClientConnecting = onClientConnecting;
            this.m_onClientConnected = onClientConnected;
            this.m_onClientDisconnecting = onClientDisconnecting;
            this.m_onClientDisconnected = onClientDisconnected;
            this.m_onClientReceivedData = onClientReceivedData;
        }

        #region 事件&委托

        /// <inheritdoc/>
        public ClosedEventHandler<ITcpSessionClient> Closed { get; set; }

        /// <inheritdoc/>
        public ClosingEventHandler<ITcpSessionClient> Closing { get; set; }

        /// <summary>
        /// 当客户端完整建立Tcp连接。
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnTcpConnected(ConnectedEventArgs e)
        {
            await this.m_onClientConnected(this, e).ConfigureAwait(false);
            if (e.Handled)
            {
                return;
            }

            await base.OnTcpConnected(e).ConfigureAwait(false);
        }

        /// <summary>
        /// 客户端正在连接。
        /// </summary>
        protected override async Task OnTcpConnecting(ConnectingEventArgs e)
        {
            await this.m_onClientConnecting(this, e).ConfigureAwait(false);
            if (e.Handled)
            {
                return;
            }

            await base.OnTcpConnecting(e).ConfigureAwait(false);
        }

        /// <summary>
        /// 客户端已断开连接。
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnTcpClosed(ClosedEventArgs e)
        {
            if (this.Closed != null)
            {
                await this.Closed.Invoke(this, e).ConfigureAwait(false);
                if (e.Handled)
                {
                    return;
                }
            }

            await this.m_onClientDisconnected(this, e).ConfigureAwait(false);
            if (e.Handled)
            {
                return;
            }

            await base.OnTcpClosed(e).ConfigureAwait(false);
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnTcpClosing(ClosingEventArgs e)
        {
            if (this.Closing != null)
            {
                await this.Closing.Invoke(this, e).ConfigureAwait(false);
                if (e.Handled)
                {
                    return;
                }
            }

            await this.m_onClientDisconnecting(this, e).ConfigureAwait(false);
            if (e.Handled)
            {
                return;
            }
            await base.OnTcpClosing(e).ConfigureAwait(false);
        }

        #endregion 事件&委托

        /// <summary>
        /// 当收到适配器处理的数据时。
        /// </summary>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
        {
            await this.m_onClientReceivedData(this, e).ConfigureAwait(false);
            if (e.Handled)
            {
                return;
            }

            await base.OnTcpReceived(e).ConfigureAwait(false);
        }

        #region 异步发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="ClientNotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual Task SendAsync(ReadOnlyMemory<byte> memory)
        {
            return this.ProtectedSendAsync(memory);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <exception cref="ClientNotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual Task SendAsync(IRequestInfo requestInfo)
        {
            return this.ProtectedSendAsync(requestInfo);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task SendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            return this.ProtectedSendAsync(transferBytes);
        }

        #endregion 异步发送

        #region Id发送

        ///// <summary>
        ///// 发送字节流
        ///// </summary>
        ///// <param name="id">用于检索TcpSessionClient</param>
        ///// <param name="buffer"></param>
        ///// <param name="offset"></param>
        ///// <param name="length"></param>
        ///// <exception cref="KeyNotFoundException"></exception>
        ///// <exception cref="ClientNotConnectedException"></exception>
        ///// <exception cref="OverlengthException"></exception>
        ///// <exception cref="Exception"></exception>
        //public void 123Send(string id, byte[] buffer, int offset, int length)
        //{
        //    this.GetClientOrThrow(id).ProtectedSend(buffer, offset, length);
        //}

        ///// <summary>
        ///// <inheritdoc/>
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="requestInfo"></param>
        //public void 123Send(string id, IRequestInfo requestInfo)
        //{
        //    this.GetClientOrThrow(id).ProtectedSend(requestInfo);
        //}

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="id">用于检索TcpSessionClient</param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="ClientNotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public Task SendAsync(string id, ReadOnlyMemory<byte> memory)
        {
            return this.GetClientOrThrow(id).ProtectedSendAsync(memory);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestInfo"></param>
        public Task SendAsync(string id, IRequestInfo requestInfo)
        {
            return this.GetClientOrThrow(id).ProtectedSendAsync(requestInfo);
        }

        private TcpSessionClient GetClientOrThrow(string id)
        {
            if (this.ProtectedTryGetClient(id, out var socketClient))
            {
                return (TcpSessionClient)socketClient;
            }
            ThrowHelper.ThrowClientNotFindException(id);
            return default;
        }

        #endregion Id发送

        #region Receiver

        /// <inheritdoc/>
        public void ClearReceiver()
        {
            this.ProtectedClearReceiver();
        }

        /// <inheritdoc/>
        public IReceiver<IReceiverResult> CreateReceiver()
        {
            return this.ProtectedCreateReceiver(this);
        }

        #endregion Receiver
    }
}