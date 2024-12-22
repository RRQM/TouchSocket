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
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 命名管道服务器辅助客户端类
    /// </summary>
    public class NamedPipeSessionClient : NamedPipeSessionClientBase, INamedPipeSessionClient
    {
        #region 字段

        private Func<NamedPipeSessionClient, ConnectedEventArgs, Task> m_onClientConnected;
        private Func<NamedPipeSessionClient, ConnectingEventArgs, Task> m_onClientConnecting;
        private Func<NamedPipeSessionClient, ClosedEventArgs, Task> m_onClientDisconnected;
        private Func<NamedPipeSessionClient, ClosingEventArgs, Task> m_onClientClosing;
        private Func<NamedPipeSessionClient, ReceivedDataEventArgs, Task> m_onClientReceivedData;

        #endregion 字段

        #region 属性

        #endregion 属性

        #region Internal

        internal void SetAction(Func<NamedPipeSessionClient, ConnectingEventArgs, Task> onClientConnecting, Func<NamedPipeSessionClient, ConnectedEventArgs, Task> onClientConnected, Func<NamedPipeSessionClient, ClosingEventArgs, Task> onClientClosing, Func<NamedPipeSessionClient, ClosedEventArgs, Task> onClientDisconnected, Func<NamedPipeSessionClient, ReceivedDataEventArgs, Task> onClientReceivedData)
        {
            this.m_onClientConnecting = onClientConnecting;
            this.m_onClientConnected = onClientConnected;
            this.m_onClientClosing = onClientClosing;
            this.m_onClientDisconnected = onClientDisconnected;
            this.m_onClientReceivedData = onClientReceivedData;
        }

        #endregion Internal

        #region 事件&委托

        /// <inheritdoc/>
        public ClosedEventHandler<INamedPipeSessionClient> Closed { get; set; }

        /// <inheritdoc/>
        public ClosingEventHandler<INamedPipeSessionClient> Closing { get; set; }

        /// <summary>
        /// 当客户端完整建立Tcp连接。
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnNamedPipeConnected(ConnectedEventArgs e)
        {
            await this.m_onClientConnected(this, e).ConfigureAwait(false);
            if (e.Handled)
            {
                return;
            }

            await base.OnNamedPipeConnected(e).ConfigureAwait(false);
        }

        /// <summary>
        /// 客户端正在连接。
        /// </summary>
        protected override async Task OnNamedPipeConnecting(ConnectingEventArgs e)
        {
            await this.m_onClientConnecting(this, e).ConfigureAwait(false);
            if (e.Handled)
            {
                return;
            }

            await base.OnNamedPipeConnecting(e).ConfigureAwait(false);
        }

        /// <summary>
        /// 客户端已断开连接。
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnNamedPipeClosed(ClosedEventArgs e)
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

            await base.OnNamedPipeClosed(e).ConfigureAwait(false);
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnNamedPipeClosing(ClosingEventArgs e)
        {
            try
            {
                if (this.Closing != null)
                {
                    await this.Closing.Invoke(this, e).ConfigureAwait(false);
                    if (e.Handled)
                    {
                        return;
                    }
                }

                await this.m_onClientClosing(this, e).ConfigureAwait(false);
                if (e.Handled)
                {
                    return;
                }
                await base.OnNamedPipeClosing(e).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Error, this, $"在事件{nameof(this.Closing)}中发生错误。", ex);
            }
        }

        #endregion 事件&委托

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

        /// <summary>
        /// 处理已接收到的数据。
        /// <para>根据不同的数据处理适配器，会传递不同的数据</para>
        /// </summary>
        protected override async Task OnNamedPipeReceived(ReceivedDataEventArgs e)
        {
            await this.m_onClientReceivedData(this, e).ConfigureAwait(false);
            if (e.Handled)
            {
                return;
            }
            await base.OnNamedPipeReceived(e).ConfigureAwait(false);
        }

        #region 异步发送

        /// <inheritdoc/>
        public virtual Task SendAsync(ReadOnlyMemory<byte> memory)
        {
            return this.ProtectedSendAsync(memory);
        }

        /// <inheritdoc/>
        public virtual Task SendAsync(IRequestInfo requestInfo)
        {
            return this.ProtectedSendAsync(requestInfo);
        }

        /// <inheritdoc/>
        public virtual Task SendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            return this.ProtectedSendAsync(transferBytes);
        }

        #endregion 异步发送

        #region Id发送

        /// <inheritdoc/>
        public Task SendAsync(string id, ReadOnlyMemory<byte> memory)
        {
            return this.GetClientOrThrow(id).ProtectedSendAsync(memory);
        }

        /// <inheritdoc/>
        public Task SendAsync(string id, IRequestInfo requestInfo)
        {
            return this.GetClientOrThrow(id).ProtectedSendAsync(requestInfo);
        }

        private NamedPipeSessionClient GetClientOrThrow(string id)
        {
            if (this.ProtectedTryGetClient(id, out var sessionClient))
            {
                return (NamedPipeSessionClient)sessionClient;
            }

            throw new ClientNotFindException();
        }

        #endregion Id发送
    }
}