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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.SerialPorts
{
    /// <summary>
    /// 串口客户端基类
    /// </summary>
    public class SerialPortClientBase : SetupConfigObject, IClient, IPluginObject, ISetupConfigObject, IOnlineClient, IConnectableClient, IClosableClient
    {
        /// <summary>
        /// 串口客户端基类
        /// </summary>
        public SerialPortClientBase()
        {
            this.Protocol = SerialPortUtility.SerialPort;
        }

        #region 变量

        private readonly SemaphoreSlim m_semaphoreForConnect = new SemaphoreSlim(1, 1);
        private SingleStreamDataHandlingAdapter m_dataHandlingAdapter;
        private bool m_online;
        private InternalReceiver m_receiver;
        private SerialCore m_serialCore;
        private Task m_taskReceive;

        #endregion 变量

        #region 事件

        /// <summary>
        /// 断开连接。在客户端未设置连接状态时，不会触发
        /// </summary>
        /// <param name="e"></param>
        protected virtual Task OnSerialClosed(ClosedEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="e"></param>
        protected virtual Task OnSerialClosing(ClosingEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 已经建立连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual Task OnSerialConnected(ConnectedEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 准备连接的时候，此时并未建立连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual Task OnSerialConnecting(ConnectingEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 当收到适配器处理的数据时。
        /// </summary>
        /// <param name="e"></param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual async Task OnSerialReceived(ReceivedDataEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            await this.PluginManager.RaiseAsync(typeof(ISerialReceivedPlugin), this, e).ConfigureFalseAwait();
        }

        /// <summary>
        /// 当收到原始数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual ValueTask<bool> OnSerialReceiving(ByteBlock byteBlock)
        {
            return EasyValueTask.FromResult(false);
        }

        /// <summary>
        /// 当即将发送时，如果覆盖父类方法，则不会触发插件。
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <returns>返回值表示是否允许发送</returns>
        protected virtual ValueTask<bool> OnSerialSending(ReadOnlyMemory<byte> memory)
        {
            return EasyValueTask.FromResult(true);
        }

        private async Task PrivateOnClosing(ClosingEventArgs e)
        {
            await this.OnSerialClosing(e).ConfigureFalseAwait();
        }

        private async Task PrivateOnSerialClosed(object obj)
        {
            var e = (ClosedEventArgs)obj;
            var receiver = this.m_receiver;
            if (receiver != null)
            {
                await receiver.Complete(e.Message).ConfigureFalseAwait();
            }
            await this.OnSerialClosed(e).ConfigureFalseAwait();
        }

        private async Task PrivateOnSerialConnected(object o)
        {
            await this.OnSerialConnected((ConnectedEventArgs)o).ConfigureFalseAwait();
        }

        private async Task PrivateOnSerialConnecting(ConnectingEventArgs e)
        {
            await this.OnSerialConnecting(e).ConfigureFalseAwait();
            if (this.m_dataHandlingAdapter == null)
            {
                var adapter = this.Config.GetValue(SerialPortConfigExtension.SerialDataHandlingAdapterProperty)?.Invoke();
                if (adapter != null)
                {
                    this.SetAdapter(adapter);
                }
            }
        }

        #endregion 事件

        #region 属性

        /// <inheritdoc/>
        public bool IsClient => true;

        /// <inheritdoc/>
        public DateTime LastReceivedTime => this.m_serialCore.ReceiveCounter.LastIncrement;

        /// <inheritdoc/>
        public DateTime LastSentTime => this.m_serialCore.SendCounter.LastIncrement;

        /// <inheritdoc/>
        public bool Online => this.m_online && this.m_serialCore != null && this.m_serialCore.IsOpen;

        /// <inheritdoc/>
        public Protocol Protocol { get; protected set; }

        /// <inheritdoc/>
        protected SingleStreamDataHandlingAdapter ProtectedDataHandlingAdapter => this.m_dataHandlingAdapter;

        /// <inheritdoc/>
        protected SerialPort ProtectedMainSerialPort => this.m_serialCore;

        #endregion 属性

        #region 断开操作

        /// <inheritdoc/>
        public virtual async Task CloseAsync(string msg)
        {
            if (this.m_online)
            {
                await this.PrivateOnClosing(new ClosingEventArgs(msg)).ConfigureFalseAwait();
                this.m_serialCore.TryClose();
                this.Abort(true, msg);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            lock (this.m_semaphoreForConnect)
            {
                if (this.m_online)
                {
                    this.PrivateOnClosing(new ClosingEventArgs($"{nameof(Dispose)}主动断开")).GetFalseAwaitResult();
                    this.Abort(true, $"{nameof(Dispose)}主动断开");
                }
            }
            base.Dispose(disposing);
        }

        #endregion 断开操作

        #region Connect

        /// <inheritdoc/>
        public async Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            this.ThrowIfDisposed();
            this.ThrowIfConfigIsNull();
            await this.m_semaphoreForConnect.WaitTimeAsync(millisecondsTimeout, token).ConfigureFalseAwait();

            try
            {
                if (this.m_online)
                {
                    return;
                }
                var serialPortOption = this.Config.GetValue(SerialPortConfigExtension.SerialPortOptionProperty) ?? throw new ArgumentNullException("串口配置不能为空。");

                this.m_serialCore.SafeDispose();

                var serialCore = CreateSerial(serialPortOption);
                await this.PrivateOnSerialConnecting(new ConnectingEventArgs()).ConfigureFalseAwait();

                serialCore.Open();

                this.m_online = true;

                this.m_serialCore = serialCore;

                this.m_taskReceive = Task.Run(this.BeginReceive);
                this.m_taskReceive.FireAndForget();

                _ = Task.Factory.StartNew(this.PrivateOnSerialConnected, new ConnectedEventArgs());
            }
            finally
            {
                this.m_semaphoreForConnect.Release();
            }
        }

        #endregion Connect

        /// <summary>
        /// Abort。
        /// </summary>
        /// <param name="manual"></param>
        /// <param name="msg"></param>
        protected void Abort(bool manual, string msg)
        {
            lock (this.m_semaphoreForConnect)
            {
                if (this.m_online)
                {
                    this.m_online = false;
                    this.m_serialCore.SafeDispose();
                    this.m_dataHandlingAdapter.SafeDispose();
                    Task.Factory.StartNew(this.PrivateOnSerialClosed, new ClosedEventArgs(manual, msg));
                }
            }
        }

        /// <summary>
        /// 设置适配器
        /// </summary>
        /// <param name="adapter"></param>
        protected void SetAdapter(SingleStreamDataHandlingAdapter adapter)
        {
            this.ThrowIfDisposed();
            if (adapter is null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            if (this.Config != null)
            {
                adapter.Config(this.Config);
            }

            adapter.Logger = this.Logger;
            adapter.OnLoaded(this);
            adapter.ReceivedAsyncCallBack = this.PrivateHandleReceivedData;
            //adapter.SendCallBack = this.ProtectedDefaultSend;
            adapter.SendAsyncCallBack = this.ProtectedDefaultSendAsync;
            this.m_dataHandlingAdapter = adapter;
        }

        private static SerialCore CreateSerial(SerialPortOption option)
        {
            var serialPort = new SerialCore(option.PortName, option.BaudRate, option.Parity, option.DataBits, option.StopBits)
            {
                Handshake = option.Handshake,
                RtsEnable = option.RtsEnable,
                DtrEnable = option.DtrEnable
            };

            return serialPort;
        }

        private async Task BeginReceive()
        {
            while (true)
            {
                try
                {
                    using (var byteBlock = new ByteBlock(1024 * 64))
                    {
                        var result = await this.m_serialCore.ReceiveAsync(byteBlock).ConfigureAwait(false);

                        if (result.BytesTransferred > 0)
                        {
                            byteBlock.SetLength(result.BytesTransferred);
                            await this.HandleReceivingData(byteBlock).ConfigureFalseAwait();
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Abort(false, ex.Message);
                    return;
                }
            }
        }

        private async Task HandleReceivingData(ByteBlock byteBlock)
        {
            try
            {
                if (this.DisposedValue)
                {
                    return;
                }

                if (await this.OnSerialReceiving(byteBlock).ConfigureAwait(false))
                {
                    return;
                }

                if (this.m_dataHandlingAdapter == null)
                {
                    await this.PrivateHandleReceivedData(byteBlock, default).ConfigureFalseAwait();
                }
                else
                {
                    await this.m_dataHandlingAdapter.ReceivedInputAsync(byteBlock).ConfigureFalseAwait();
                }
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, "在处理数据时发生错误", ex);
            }
        }

        #region Receiver

        /// <inheritdoc/>
        protected void ProtectedClearReceiver()
        {
            this.m_receiver = null;
        }

        /// <inheritdoc/>
        protected IReceiver<IReceiverResult> ProtectedCreateReceiver(IReceiverClient<IReceiverResult> receiverObject)
        {
            return this.m_receiver ??= new InternalReceiver(receiverObject);
        }

        #endregion Receiver

        private async Task PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.m_receiver != null)
            {
                await this.m_receiver.InputReceive(byteBlock, requestInfo).ConfigureFalseAwait();
                return;
            }
            await this.OnSerialReceived(new ReceivedDataEventArgs(byteBlock, requestInfo)).ConfigureFalseAwait();
        }

        #region Throw

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfCannotSendRequestInfo()
        {
            if (this.m_dataHandlingAdapter == null || !this.m_dataHandlingAdapter.CanSendRequestInfo)
            {
                throw new NotSupportedException($"当前适配器为空或者不支持对象发送。");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfClientNotConnected()
        {
            if (this.m_online)
            {
                return;
            }

            ThrowHelper.ThrowClientNotConnectedException();
        }

        #endregion Throw

        #region 发送

        ///// <inheritdoc/>
        //protected void ProtectedDefaultSend(byte[] buffer, int offset, int length)
        //{
        //    this.ThrowIfDisposed();
        //    this.ThrowIfClientNotConnected();
        //    if (this.OnSerialSending(buffer, offset, length).GetFalseAwaitResult())
        //    {
        //        this.m_serialCore.Send(buffer, offset, length);
        //    }
        //}

        /// <inheritdoc/>
        protected async Task ProtectedDefaultSendAsync(ReadOnlyMemory<byte> memory)
        {
            this.ThrowIfDisposed();
            this.ThrowIfClientNotConnected();
            if (await this.OnSerialSending(memory).ConfigureAwait(false))
            {
                await this.m_serialCore.SendAsync(memory).ConfigureFalseAwait();
            }
        }

        #endregion 发送

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
        protected Task ProtectedSendAsync(ReadOnlyMemory<byte> memory)
        {
            if (this.m_dataHandlingAdapter == null)
            {
                return this.ProtectedDefaultSendAsync(memory);
            }
            else
            {
                return this.m_dataHandlingAdapter.SendInputAsync(memory);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <exception cref="ClientNotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        protected Task ProtectedSendAsync(IRequestInfo requestInfo)
        {
            this.ThrowIfCannotSendRequestInfo();
            return this.m_dataHandlingAdapter.SendInputAsync(requestInfo);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected async Task ProtectedSendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            if (this.m_dataHandlingAdapter == null || !this.m_dataHandlingAdapter.CanSplicingSend)
            {
                var length = 0;
                foreach (var item in transferBytes)
                {
                    length += item.Count;
                }
                using (var byteBlock = new ByteBlock(length))
                {
                    foreach (var item in transferBytes)
                    {
                        byteBlock.Write(new ReadOnlySpan<byte>(item.Array, item.Offset, item.Count));
                    }
                    if (this.m_dataHandlingAdapter == null)
                    {
                        await this.ProtectedDefaultSendAsync(byteBlock.Memory).ConfigureFalseAwait();
                    }
                    else
                    {
                        await this.m_dataHandlingAdapter.SendInputAsync(byteBlock.Memory).ConfigureFalseAwait();
                    }
                }
            }
            else
            {
                await this.m_dataHandlingAdapter.SendInputAsync(transferBytes).ConfigureFalseAwait();
            }
        }

        #endregion 异步发送
    }
}