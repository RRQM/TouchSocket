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

        /// <inheritdoc/>
        public SingleStreamDataHandlingAdapter DataHandlingAdapter => this.ProtectedDataHandlingAdapter;

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
            if (await this.PluginManager.RaiseAsync(typeof(INamedPipeConnectedPlugin), this, e).ConfigureFalseAwait())
            {
                return;
            }
            await this.m_onClientConnected(this, e).ConfigureFalseAwait();
        }

        /// <summary>
        /// 客户端正在连接。
        /// </summary>
        protected override async Task OnNamedPipeConnecting(ConnectingEventArgs e)
        {
            if (await this.PluginManager.RaiseAsync(typeof(INamedPipeConnectingPlugin), this, e).ConfigureFalseAwait())
            {
                return;
            }
            await this.m_onClientConnecting(this, e).ConfigureFalseAwait();
        }

        /// <summary>
        /// 客户端已断开连接。
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnNamedPipeClosed(ClosedEventArgs e)
        {
            if (this.Closed != null)
            {
                await this.Closed.Invoke(this, e).ConfigureFalseAwait();
                if (e.Handled)
                {
                    return;
                }
            }

            if (await this.PluginManager.RaiseAsync(typeof(INamedPipeClosedPlugin), this, e).ConfigureFalseAwait())
            {
                return;
            }
            await this.m_onClientDisconnected(this, e).ConfigureFalseAwait();
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
                    await this.Closing.Invoke(this, e).ConfigureFalseAwait();
                    if (e.Handled)
                    {
                        return;
                    }
                }

                if (await this.PluginManager.RaiseAsync(typeof(INamedPipeClosingPlugin), this, e).ConfigureFalseAwait())
                {
                    return;
                }
                await this.m_onClientClosing(this, e).ConfigureFalseAwait();
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.Closing)}中发生错误。", ex);
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
            await this.m_onClientReceivedData(this, e).ConfigureFalseAwait();
            if (e.Handled)
            {
                return;
            }
            await this.PluginManager.RaiseAsync(typeof(INamedPipeReceivedPlugin), this, e).ConfigureFalseAwait();
        }

        /// <summary>
        /// 当收到原始数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected override async ValueTask<bool> OnNamedPipeReceiving(ByteBlock byteBlock)
        {
            if (this.PluginManager.GetPluginCount(typeof(INamedPipeReceivingPlugin)) > 0)
            {
                return await this.PluginManager.RaiseAsync(typeof(INamedPipeReceivingPlugin), this, new ByteBlockEventArgs(byteBlock)).ConfigureFalseAwait();
            }
            return false;
        }

        /// <summary>
        /// 当即将发送时。
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <returns>返回值表示是否允许发送</returns>
        protected override async ValueTask<bool> OnNamedPipeSending(ReadOnlyMemory<byte> memory)
        {
            if (this.PluginManager.GetPluginCount(typeof(INamedPipeSendingPlugin)) > 0)
            {
                var args = new SendingEventArgs(memory);
                await this.PluginManager.RaiseAsync(typeof(INamedPipeSendingPlugin), this, args).ConfigureAwait(false);
                return args.IsPermitOperation;
            }
            return true;
        }

        //#region 同步发送

        ///// <inheritdoc/>
        //public virtual void 123Send(IRequestInfo requestInfo)
        //{
        //    this.ProtectedSend(requestInfo);
        //}

        ///// <inheritdoc/>
        //public virtual void 123Send(byte[] buffer, int offset, int length)
        //{
        //    this.ProtectedSend(buffer, offset, length);
        //}

        ///// <inheritdoc/>
        //public virtual void 123Send(IList<ArraySegment<byte>> transferBytes)
        //{
        //    this.ProtectedSend(transferBytes);
        //}

        //#endregion 同步发送

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

        ///// <inheritdoc/>
        //public void 123Send(string id, byte[] buffer, int offset, int length)
        //{
        //    this.GetClientOrThrow(id).ProtectedSend(buffer, offset, length);
        //}

        ///// <inheritdoc/>
        //public void 123Send(string id, IRequestInfo requestInfo)
        //{
        //    this.GetClientOrThrow(id).ProtectedSend(requestInfo);
        //}

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
            if (this.ProtectedTryGetClient(id, out var socketClient))
            {
                return (NamedPipeSessionClient)socketClient;
            }

            throw new ClientNotFindException();
        }

        #endregion Id发送
    }
}