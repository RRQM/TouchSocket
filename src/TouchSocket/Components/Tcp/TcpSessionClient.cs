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
            await this.m_onClientConnected(this, e).ConfigureFalseAwait();
            if (e.Handled)
            {
                return;
            }
            await this.PluginManager.RaiseAsync(typeof(ITcpConnectedPlugin), this, e).ConfigureFalseAwait();
        }

        /// <summary>
        /// 客户端正在连接。
        /// </summary>
        protected override async Task OnTcpConnecting(ConnectingEventArgs e)
        {
            await this.m_onClientConnecting(this, e).ConfigureFalseAwait();
            if (e.Handled)
            {
                return;
            }
            await this.PluginManager.RaiseAsync(typeof(ITcpConnectingPlugin), this, e).ConfigureFalseAwait();
        }

        /// <summary>
        /// 客户端已断开连接。
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnTcpClosed(ClosedEventArgs e)
        {
            if (this.Closed != null)
            {
                await this.Closed.Invoke(this, e).ConfigureFalseAwait();
                if (e.Handled)
                {
                    return;
                }
            }

            await this.m_onClientDisconnected(this, e).ConfigureFalseAwait();
            if (e.Handled)
            {
                return;
            }
            await this.PluginManager.RaiseAsync(typeof(ITcpClosedPlugin), this, e).ConfigureFalseAwait();
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnTcpClosing(ClosingEventArgs e)
        {
            if (this.Closing != null)
            {
                await this.Closing.Invoke(this, e).ConfigureFalseAwait();
                if (e.Handled)
                {
                    return;
                }
            }

            await this.m_onClientDisconnecting(this, e).ConfigureFalseAwait();

            await this.PluginManager.RaiseAsync(typeof(ITcpClosingPlugin), this, e).ConfigureFalseAwait();
        }

        #endregion 事件&委托

        /// <summary>
        /// 当收到适配器处理的数据时。
        /// </summary>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
        {
            await this.m_onClientReceivedData(this, e).ConfigureFalseAwait();
            if (e.Handled)
            {
                return;
            }
            await this.PluginManager.RaiseAsync(typeof(ITcpReceivedPlugin), this, e).ConfigureFalseAwait();
        }

        /// <summary>
        /// 当收到原始数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected override async ValueTask<bool> OnTcpReceiving(ByteBlock byteBlock)
        {
            return this.PluginManager.GetPluginCount(typeof(ITcpReceivingPlugin)) > 0 && await this.PluginManager.RaiseAsync(typeof(ITcpReceivingPlugin), this, new ByteBlockEventArgs(byteBlock)).ConfigureFalseAwait();
        }

        /// <summary>
        /// 当即将发送时，如果覆盖父类方法，则不会触发插件。
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <returns>返回值表示是否允许发送</returns>
        protected override async ValueTask<bool> OnTcpSending(ReadOnlyMemory<byte> memory)
        {
            if (this.PluginManager.GetPluginCount(typeof(ITcpSendingPlugin)) > 0)
            {
                var args = new SendingEventArgs(memory);
                await this.PluginManager.RaiseAsync(typeof(ITcpSendingPlugin), this, args).ConfigureFalseAwait();
                return args.IsPermitOperation;
            }
            return true;
        }

        //#region 同步发送

        ///// <summary>
        ///// <inheritdoc/>
        ///// </summary>
        ///// <param name="requestInfo"></param>
        ///// <exception cref="ClientNotConnectedException"></exception>
        ///// <exception cref="OverlengthException"></exception>
        ///// <exception cref="Exception"></exception>
        //public virtual void 123Send(IRequestInfo requestInfo)
        //{
        //    this.ProtectedSend(requestInfo);
        //}

        ///// <summary>
        ///// 发送字节流
        ///// </summary>
        ///// <param name="buffer"></param>
        ///// <param name="offset"></param>
        ///// <param name="length"></param>
        ///// <exception cref="ClientNotConnectedException"></exception>
        ///// <exception cref="OverlengthException"></exception>
        ///// <exception cref="Exception"></exception>
        //public virtual void 123Send(byte[] buffer, int offset, int length)
        //{
        //    this.ProtectedSend(buffer, offset, length);
        //}

        ///// <summary>
        ///// <inheritdoc/>
        ///// </summary>
        ///// <param name="transferBytes"></param>
        //public virtual void 123Send(IList<ArraySegment<byte>> transferBytes)
        //{
        //    this.ProtectedSend(transferBytes);
        //}

        //#endregion 同步发送

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
        public virtual Task SendAsync(ReadOnlyMemory<byte>  memory)
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