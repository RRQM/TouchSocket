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
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 命名管道客户端
    /// </summary>
    public class NamedPipeClient : NamedPipeClientBase, INamedPipeClient
    {
        #region 事件

        /// <inheritdoc/>
        public ClosedEventHandler<INamedPipeClient> Closed { get; set; }

        /// <inheritdoc/>
        public ClosingEventHandler<INamedPipeClient> Closing { get; set; }

        /// <inheritdoc/>
        public ConnectedEventHandler<INamedPipeClient> Connected { get; set; }

        /// <inheritdoc/>
        public ConnectingEventHandler<INamedPipeClient> Connecting { get; set; }

        /// <summary>
        /// 接收到数据
        /// </summary>
        public ReceivedEventHandler<INamedPipeClient> Received { get; set; }

        /// <summary>
        /// 断开连接。在客户端未设置连接状态时，不会触发
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnNamedPipeClosed(ClosedEventArgs e)
        {
            try
            {
                if (this.Closed != null)
                {
                    await this.Closed.Invoke(this, e).ConfigureFalseAwait();
                    if (e.Handled)
                    {
                        return;
                    }
                }

                await this.PluginManager.RaiseAsync(typeof(INamedPipeClosedPlugin), this, e).ConfigureFalseAwait();
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.Closed)}中发生错误。", ex);
            }
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnNamedPipeClosing(ClosingEventArgs e)
        {
            if (this.Closing != null)
            {
                await this.Closing.Invoke(this, e).ConfigureFalseAwait();
                if (e.Handled)
                {
                    return;
                }
            }
            await this.PluginManager.RaiseAsync(typeof(INamedPipeClosingPlugin), this, e).ConfigureFalseAwait();
        }

        /// <summary>
        /// 已经建立管道连接
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnNamedPipeConnected(ConnectedEventArgs e)
        {
            try
            {
                if (this.Connected != null)
                {
                    await this.Connected.Invoke(this, e).ConfigureFalseAwait();
                    if (e.Handled)
                    {
                        return;
                    }
                }

                await this.PluginManager.RaiseAsync(typeof(INamedPipeConnectedPlugin), this, e).ConfigureFalseAwait();
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.Connected)}中发生错误。", ex);
            }
        }

        /// <summary>
        /// 准备连接的时候，此时已初始化Socket，但是并未建立Tcp连接
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnNamedPipeConnecting(ConnectingEventArgs e)
        {
            try
            {
                if (this.Connecting != null)
                {
                    await this.Connecting.Invoke(this, e).ConfigureFalseAwait();
                    if (e.Handled)
                    {
                        return;
                    }
                }

                await this.PluginManager.RaiseAsync(typeof(INamedPipeConnectingPlugin), this, e).ConfigureFalseAwait();
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.OnNamedPipeConnecting)}中发生错误。", ex);
            }
        }

        /// <inheritdoc/>
        protected override async Task OnNamedPipeReceived(ReceivedDataEventArgs e)
        {
            if (this.Received != null)
            {
                await this.Received.Invoke(this, e).ConfigureFalseAwait();
                if (e.Handled)
                {
                    return;
                }
            }
            await this.PluginManager.RaiseAsync(typeof(INamedPipeReceivedPlugin), this, e).ConfigureFalseAwait();
        }

        #endregion 事件

        #region 属性

        /// <inheritdoc/>
        public SingleStreamDataHandlingAdapter DataHandlingAdapter => this.DataHandlingAdapter;

        #endregion 属性

        #region Connect

        /// <inheritdoc/>
        public virtual Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            return this.PipeConnectAsync(millisecondsTimeout, token);
        }

        #endregion Connect

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
        /// 当即将发送时，如果覆盖父类方法，则不会触发插件。
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <returns>返回值表示是否允许发送</returns>
        protected override async ValueTask<bool> OnNamedPipeSending(ReadOnlyMemory<byte> memory)
        {
            if (this.PluginManager.GetPluginCount(typeof(INamedPipeSendingPlugin)) == 0)
            {
                return true;
            }
            var args = new SendingEventArgs(memory);
            await this.PluginManager.RaiseAsync(typeof(INamedPipeSendingPlugin), this, args)
                 .ConfigureFalseAwait();
            return args.IsPermitOperation;
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
    }
}