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
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 命名管道客户端客户端基类
    /// </summary>
    public abstract class NamedPipeClientBase : SetupConfigObject, INamedPipeSession
    {
        /// <summary>
        /// 命名管道客户端客户端基类
        /// </summary>
        public NamedPipeClientBase()
        {
            this.Protocol = Protocol.NamedPipe;
            this.m_receiveCounter = new ValueCounter
            {
                Period = TimeSpan.FromSeconds(1),
                OnPeriod = this.OnPeriod
            };
        }

        #region 变量

        private readonly SemaphoreSlim m_semaphoreSlimForConnect = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim m_semaphoreSlimForSend = new SemaphoreSlim(1, 1);
        private volatile bool m_online;
        private Task<Task> receiveTask;
        private NamedPipeClientStream m_pipeStream;
        private int m_receiveBufferSize = 1024 * 10;
        private ValueCounter m_receiveCounter;
        private InternalReceiver m_receiver;
        private SingleStreamDataHandlingAdapter m_dataHandlingAdapter;

        #endregion 变量

        #region 事件

        /// <summary>
        /// 已经建立管道连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual Task OnNamedPipeConnected(ConnectedEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 准备连接的时候
        /// </summary>
        /// <param name="e"></param>
        protected virtual Task OnNamedPipeConnecting(ConnectingEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 断开连接。在客户端未设置连接状态时，不会触发
        /// </summary>
        /// <param name="e"></param>
        protected virtual Task OnNamedPipeClosed(ClosedEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="e"></param>
        protected virtual Task OnNamedPipeClosing(ClosingEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        private Task PrivateOnNamedPipeConnected(object o)
        {
            var e = (ConnectedEventArgs)o;
            return this.OnNamedPipeConnected(e);
        }

        private async Task PrivateOnNamedPipeConnecting(object obj)
        {
            var e = (ConnectingEventArgs)obj;

            await this.OnNamedPipeConnecting(e).ConfigureFalseAwait();
            if (this.ProtectedDataHandlingAdapter == null)
            {
                var adapter = this.Config.GetValue(NamedPipeConfigExtension.NamedPipeDataHandlingAdapterProperty)?.Invoke();
                if (adapter != null)
                {
                    this.SetAdapter(adapter);
                }
            }
        }

        private async Task PrivateOnNamedPipeClosed(ClosedEventArgs e)
        {
            await this.receiveTask.ConfigureFalseAwait();
            var receiver = this.m_receiver;
            if (receiver != null)
            {
                await receiver.Complete(e.Message).ConfigureFalseAwait();
            }

            await this.OnNamedPipeClosed(e).ConfigureFalseAwait();
        }

        private async Task PrivateOnNamedPipeClosing(ClosingEventArgs e)
        {
            await this.OnNamedPipeClosing(e).ConfigureFalseAwait();
        }

        #endregion 事件

        #region 属性

        /// <inheritdoc/>
        public bool IsClient => true;

        /// <inheritdoc/>
        public DateTime LastReceivedTime { get; private set; }

        /// <inheritdoc/>
        public DateTime LastSentTime { get; private set; }

        /// <inheritdoc/>
        public PipeStream PipeStream => this.m_pipeStream;

        /// <inheritdoc/>
        public Protocol Protocol { get; protected set; }

        /// <inheritdoc/>
        protected SingleStreamDataHandlingAdapter ProtectedDataHandlingAdapter => this.m_dataHandlingAdapter;

        /// <inheritdoc/>
        public virtual bool Online => this.m_online;

        #endregion 属性

        #region 断开操作

        /// <summary>
        /// 中断链接
        /// </summary>
        /// <param name="manual"></param>
        /// <param name="msg"></param>
        protected void Abort(bool manual, string msg)
        {
            lock (this.m_semaphoreSlimForConnect)
            {
                if (this.m_online)
                {
                    this.m_online = false;
                    this.m_pipeStream.SafeDispose();
                    this.ProtectedDataHandlingAdapter.SafeDispose();

                    _ = this.PrivateOnNamedPipeClosed(new ClosedEventArgs(manual, msg));
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.Abort(true, $"{nameof(Dispose)}主动断开");
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public virtual async Task CloseAsync(string msg)
        {
            if (this.m_online)
            {
                await this.PrivateOnNamedPipeClosing(new ClosedEventArgs(true, msg)).ConfigureFalseAwait();
                this.m_pipeStream.Close();
                this.Abort(true, msg);
            }
        }

        #endregion 断开操作

        #region Connect

        /// <summary>
        /// 建立管道的连接。
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="token"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        /// <exception cref="TimeoutException"></exception>
        protected async Task PipeConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            await this.m_semaphoreSlimForConnect.WaitTimeAsync(millisecondsTimeout, token).ConfigureFalseAwait();
            try
            {
                if (this.m_online)
                {
                    return;
                }
                if (this.DisposedValue)
                {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }
                if (this.Config == null)
                {
                    throw new ArgumentNullException(nameof(this.Config), "配置文件不能为空。");
                }
                var pipeName = this.Config.GetValue(NamedPipeConfigExtension.PipeNameProperty) ?? throw new ArgumentNullException("PipeName", "命名管道连接参数不能为空。");

                var serverName = this.Config.GetValue(NamedPipeConfigExtension.PipeServerNameProperty);
                this.m_pipeStream.SafeDispose();

                var namedPipe = CreatePipeClient(serverName, pipeName);
                await this.PrivateOnNamedPipeConnecting(new ConnectingEventArgs()).ConfigureFalseAwait();

#if NET45
                namedPipe.Connect(millisecondsTimeout);
#else
                await namedPipe.ConnectAsync(millisecondsTimeout, token).ConfigureFalseAwait();
#endif

                if (namedPipe.IsConnected)
                {
                    this.m_pipeStream = namedPipe;
                    this.m_online = true;
                    this.receiveTask = Task.Factory.StartNew(this.BeginReceive, TaskCreationOptions.LongRunning);
                    _ = Task.Factory.StartNew(this.PrivateOnNamedPipeConnected, new ConnectedEventArgs());
                    return;
                }
                throw new Exception("未知异常");
            }
            finally
            {
                this.m_semaphoreSlimForConnect.Release();
            }
        }

        #endregion Connect

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

        /// <summary>
        /// 处理已接收到的数据。
        /// <para>根据不同的数据处理适配器，会传递不同的数据</para>
        /// </summary>
        /// <param name="e"></param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual Task OnNamedPipeReceived(ReceivedDataEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 当收到原始数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual ValueTask<bool> OnNamedPipeReceiving(ByteBlock byteBlock)
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
        protected virtual ValueTask<bool> OnNamedPipeSending(ReadOnlyMemory<byte> memory)
        {
            return EasyValueTask.FromResult(true);
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

        private static NamedPipeClientStream CreatePipeClient(string serverName, string pipeName)
        {
            var pipeClient = new NamedPipeClientStream(serverName, pipeName, PipeDirection.InOut, PipeOptions.Asynchronous, TokenImpersonationLevel.None);
            return pipeClient;
        }

        private async Task BeginReceive()
        {
            while (true)
            {
                using (var byteBlock = new ByteBlock(this.GetReceiveBufferSize()))
                {
                    try
                    {
                        var r = await this.m_pipeStream.ReadAsync(byteBlock.TotalMemory, CancellationToken.None).ConfigureAwait(false);
                        if (r == 0)
                        {
                            this.Abort(false, "远程终端主动关闭");
                            return;
                        }

                        byteBlock.SetLength(r);
                        await this.HandleReceivingData(byteBlock).ConfigureFalseAwait();
                    }
                    catch (Exception ex)
                    {
                        this.Abort(false, ex.Message);
                        return;
                    }
                }
            }
        }

        private int GetReceiveBufferSize()
        {
            var minBufferSize = this.Config.GetValue(TouchSocketConfigExtension.MinBufferSizeProperty) ?? 1024 * 10;
            var maxBufferSize = this.Config.GetValue(TouchSocketConfigExtension.MaxBufferSizeProperty) ?? 1024 * 512;
            return Math.Min(Math.Max(this.m_receiveBufferSize, minBufferSize), maxBufferSize);
        }

        private async Task HandleReceivingData(ByteBlock byteBlock)
        {
            try
            {
                if (this.DisposedValue)
                {
                    return;
                }

                this.m_receiveCounter.Increment(byteBlock.Length);

                if (await this.OnNamedPipeReceiving(byteBlock).ConfigureAwait(false))
                {
                    return;
                }

                if (this.ProtectedDataHandlingAdapter == null)
                {
                    await this.PrivateHandleReceivedData(byteBlock, default).ConfigureFalseAwait();
                }
                else
                {
                    await this.ProtectedDataHandlingAdapter.ReceivedInputAsync(byteBlock).ConfigureFalseAwait();
                }
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, "在处理数据时发生错误", ex);
            }
        }

        private void OnPeriod(long value)
        {
            this.m_receiveBufferSize = TouchSocketCoreUtility.HitBufferLength(value);
        }

        private async Task PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.m_receiver != null)
            {
                await this.m_receiver.InputReceive(byteBlock, requestInfo).ConfigureFalseAwait();
                return;
            }
            await this.OnNamedPipeReceived(new ReceivedDataEventArgs(byteBlock, requestInfo)).ConfigureFalseAwait();
        }

        #region Throw

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfCannotSendRequestInfo()
        {
            if (this.ProtectedDataHandlingAdapter == null || !this.ProtectedDataHandlingAdapter.CanSendRequestInfo)
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

        #region 直接发送

        ///// <inheritdoc/>
        //protected void ProtectedDefaultSend(byte[] buffer, int offset, int length)
        //{
        //    this.ThrowIfDisposed();
        //    this.ThrowIfClientNotConnected();
        //    if (this.OnNamedPipeSending(buffer, offset, length).GetFalseAwaitResult())
        //    {
        //        try
        //        {
        //            this.m_semaphoreSlimForSend.Wait();
        //            this.m_pipeStream.Write(buffer, offset, length);
        //            this.LastSendTime = DateTime.Now;
        //        }
        //        finally
        //        {
        //            this.m_semaphoreSlimForSend.Release();
        //        }
        //    }
        //}

        /// <inheritdoc/>
        protected async Task ProtectedDefaultSendAsync(ReadOnlyMemory<byte> memory)
        {
            this.ThrowIfDisposed();
            this.ThrowIfClientNotConnected();
            if (await this.OnNamedPipeSending(memory).ConfigureAwait(false))
            {
                try
                {
                    await this.m_semaphoreSlimForSend.WaitAsync().ConfigureFalseAwait();
#if NET6_0_OR_GREATER
                    await this.m_pipeStream.WriteAsync(memory, CancellationToken.None).ConfigureAwait(false);
#else
                    var segment = memory.GetArray();
                    await this.m_pipeStream.WriteAsync(segment.Array, segment.Offset, segment.Count).ConfigureFalseAwait();
#endif
                    this.LastSentTime = DateTime.Now;
                }
                finally
                {
                    this.m_semaphoreSlimForSend.Release();
                }
            }
        }

        #endregion 直接发送

        //#region 同步发送

        ///// <summary>
        ///// <inheritdoc/>
        ///// </summary>
        ///// <param name="requestInfo"></param>
        ///// <exception cref="ClientNotConnectedException"></exception>
        ///// <exception cref="OverlengthException"></exception>
        ///// <exception cref="Exception"></exception>
        //protected void ProtectedSend(IRequestInfo requestInfo)
        //{
        //    this.ThrowIfCannotSendRequestInfo();
        //    this.ProtectedDataHandlingAdapter.SendInput(requestInfo);
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
        //protected void ProtectedSend(byte[] buffer, int offset, int length)
        //{
        //    if (this.ProtectedDataHandlingAdapter == null)
        //    {
        //        this.ProtectedDefaultSend(buffer, offset, length);
        //    }
        //    else
        //    {
        //        this.ProtectedDataHandlingAdapter.SendInput(buffer, offset, length);
        //    }
        //}

        ///// <summary>
        ///// <inheritdoc/>
        ///// </summary>
        ///// <param name="transferBytes"></param>
        //protected void ProtectedSend(IList<ArraySegment<byte>> transferBytes)
        //{
        //    if (this.ProtectedDataHandlingAdapter == null || !this.ProtectedDataHandlingAdapter.CanSplicingSend)
        //    {
        //        var length = 0;
        //        foreach (var item in transferBytes)
        //        {
        //            length += item.Count;
        //        }
        //        using (var byteBlock = new ByteBlock(length))
        //        {
        //            foreach (var item in transferBytes)
        //            {
        //                byteBlock.Write(item.Array, item.Offset, item.Count);
        //            }

        //            if (this.ProtectedDataHandlingAdapter == null)
        //            {
        //                this.ProtectedDefaultSend(byteBlock.Buffer, 0, byteBlock.Length);
        //            }
        //            else
        //            {
        //                this.ProtectedDataHandlingAdapter.SendInput(byteBlock.Buffer, 0, byteBlock.Length);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        this.ProtectedDataHandlingAdapter.SendInput(transferBytes);
        //    }
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
        protected Task ProtectedSendAsync(ReadOnlyMemory<byte> memory)
        {
            if (this.ProtectedDataHandlingAdapter == null)
            {
                return this.ProtectedDefaultSendAsync(memory);
            }
            else
            {
                return this.ProtectedDataHandlingAdapter.SendInputAsync(memory);
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
            return this.ProtectedDataHandlingAdapter.SendInputAsync(requestInfo);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected async Task ProtectedSendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            if (this.ProtectedDataHandlingAdapter == null || !this.ProtectedDataHandlingAdapter.CanSplicingSend)
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
                        byteBlock.Write(item.Array, item.Offset, item.Count);
                    }
                    if (this.ProtectedDataHandlingAdapter == null)
                    {
                        await this.ProtectedDefaultSendAsync(byteBlock.Memory).ConfigureFalseAwait();
                    }
                    else
                    {
                        await this.ProtectedDataHandlingAdapter.SendInputAsync(byteBlock.Memory).ConfigureFalseAwait();
                    }
                }
            }
            else
            {
                await this.ProtectedDataHandlingAdapter.SendInputAsync(transferBytes).ConfigureFalseAwait();
            }
        }

        #endregion 异步发送
    }
}