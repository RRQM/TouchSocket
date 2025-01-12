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
using System.IO.Ports;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.SerialPorts
{
    /// <inheritdoc cref="SerialPortClientBase"/>
    public class SerialPortClient : SerialPortClientBase, ISerialPortClient
    {
        #region 属性

        /// <inheritdoc/>
        public SerialPort MainSerialPort => this.ProtectedMainSerialPort;

        #endregion 属性

        #region 事件

        /// <inheritdoc/>
        public ClosedEventHandler<ISerialPortClient> Closed { get; set; }

        /// <inheritdoc/>
        public ClosingEventHandler<ISerialPortClient> Closing { get; set; }

        /// <inheritdoc/>
        public ConnectedEventHandler<ISerialPortClient> Connected { get; set; }

        /// <inheritdoc/>
        public ConnectingEventHandler<ISerialPortClient> Connecting { get; set; }

        /// <inheritdoc/>
        public ReceivedEventHandler<ISerialPortClient> Received { get; set; }

        /// <summary>
        /// 断开连接。在客户端未设置连接状态时，不会触发
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnSerialClosed(ClosedEventArgs e)
        {
            try
            {
                if (this.Closed != null)
                {
                    await this.Closed.Invoke(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    if (e.Handled)
                    {
                        return;
                    }
                }

                await base.OnSerialClosed(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Error, this, $"在事件{nameof(this.Closed)}中发生错误。", ex);
            }
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnSerialClosing(ClosingEventArgs e)
        {
            try
            {
                if (this.Closing != null)
                {
                    await this.Closing.Invoke(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    if (e.Handled)
                    {
                        return;
                    }
                }

                await base.OnSerialClosing(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Error, this, $"在事件{nameof(this.Closing)}中发生错误。", ex);
            }
        }

        /// <summary>
        /// 已经建立连接
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnSerialConnected(ConnectedEventArgs e)
        {
            try
            {
                if (this.Connected != null)
                {
                    await this.Connected.Invoke(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    if (e.Handled)
                    {
                        return;
                    }
                }
                await base.OnSerialConnected(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Error, this, $"在事件{nameof(this.Connected)}中发生错误。", ex);
            }
        }

        /// <summary>
        /// 准备连接的时候，此时并未建立连接
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnSerialConnecting(ConnectingEventArgs e)
        {
            try
            {
                if (this.Connecting != null)
                {
                    await this.Connecting.Invoke(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    if (e.Handled)
                    {
                        return;
                    }
                }

                await base.OnSerialConnecting(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Error, this, $"在事件{nameof(this.OnSerialConnecting)}中发生错误。", ex);
            }
        }

        /// <inheritdoc/>
        protected override async Task OnSerialReceived(ReceivedDataEventArgs e)
        {
            if (this.Received != null)
            {
                await this.Received.Invoke(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                if (e.Handled)
                {
                    return;
                }
            }

            await base.OnSerialReceived(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }

        #endregion 事件

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