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
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;
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
        private readonly Lock m_lockForAbort = LockFactory.Create();
        #endregion 变量

        #region 事件

        /// <summary>
        /// 已经建立管道连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnNamedPipeConnected(ConnectedEventArgs e)
        {
            await this.PluginManager.RaiseAsync(typeof(INamedPipeConnectedPlugin), this.Resolver, this, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 准备连接的时候
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnNamedPipeConnecting(ConnectingEventArgs e)
        {
            await this.PluginManager.RaiseAsync(typeof(INamedPipeConnectingPlugin), this.Resolver, this, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 断开连接。在客户端未设置连接状态时，不会触发
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnNamedPipeClosed(ClosedEventArgs e)
        {
            await this.PluginManager.RaiseAsync(typeof(INamedPipeClosedPlugin), this.Resolver, this, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnNamedPipeClosing(ClosingEventArgs e)
        {
            await this.PluginManager.RaiseAsync(typeof(INamedPipeClosingPlugin), this.Resolver, this, e).ConfigureAwait(false);
        }

        private async Task PrivateOnNamedPipeConnected(object o)
        {
            try
            {
                var e = (ConnectedEventArgs)o;
                await this.OnNamedPipeConnected(e).ConfigureAwait(false);
            }
            catch
            {
            }

        }

        private async Task PrivateOnNamedPipeConnecting(object obj)
        {
            var e = (ConnectingEventArgs)obj;

            await this.OnNamedPipeConnecting(e).ConfigureAwait(false);
            if (this.m_dataHandlingAdapter == null)
            {
                var adapter = this.Config.GetValue(NamedPipeConfigExtension.NamedPipeDataHandlingAdapterProperty)?.Invoke();
                if (adapter != null)
                {
                    this.SetAdapter(adapter);
                }
            }
        }

        private async Task PrivateOnNamedPipeClosed(object o)
        {
            try
            {
                var e = (ClosedEventArgs)o;

                await this.receiveTask.ConfigureAwait(false);
                var receiver = this.m_receiver;
                if (receiver != null)
                {
                    await receiver.Complete(e.Message).ConfigureAwait(false);
                }

                await this.OnNamedPipeClosed(e).ConfigureAwait(false);
            }
            catch
            {
            }
        }

        private async Task PrivateOnNamedPipeClosing(ClosingEventArgs e)
        {
            await this.OnNamedPipeClosing(e).ConfigureAwait(false);
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
        public SingleStreamDataHandlingAdapter DataHandlingAdapter => this.m_dataHandlingAdapter;

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
            lock (this.m_lockForAbort)
            {
                if (this.m_online)
                {
                    this.m_online = false;
                    this.m_pipeStream.SafeDispose();

                    this.m_dataHandlingAdapter.SafeDispose();
                    this.m_dataHandlingAdapter = default;

                    _ = Task.Factory.StartNew(this.PrivateOnNamedPipeClosed, new ClosedEventArgs(manual, msg));
                }
            }
        }


        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.DisposedValue)
            {
                return;
            }

            if (disposing)
            {
                this.Abort(true, TouchSocketResource.DisposeClose);
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public virtual async Task CloseAsync(string msg)
        {
            if (this.m_online)
            {
                await this.PrivateOnNamedPipeClosing(new ClosedEventArgs(true, msg)).ConfigureAwait(false);

                lock (this.m_lockForAbort)
                {
                    this.m_pipeStream.Close();
                    this.Abort(true, msg);
                }
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
            await this.m_semaphoreSlimForConnect.WaitTimeAsync(millisecondsTimeout, token).ConfigureAwait(false);
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
                await this.PrivateOnNamedPipeConnecting(new ConnectingEventArgs()).ConfigureAwait(false);

#if NET45
                namedPipe.Connect(millisecondsTimeout);
#else
                await namedPipe.ConnectAsync(millisecondsTimeout, token).ConfigureAwait(false);
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
        protected virtual async Task OnNamedPipeReceived(ReceivedDataEventArgs e)
        {
            await this.PluginManager.RaiseAsync(typeof(INamedPipeReceivedPlugin), this.Resolver, this, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 当收到原始数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual ValueTask<bool> OnNamedPipeReceiving(ByteBlock byteBlock)
        {
            return this.PluginManager.RaiseAsync(typeof(INamedPipeReceivingPlugin), this.Resolver, this, new ByteBlockEventArgs(byteBlock));
        }


        /// <summary>
        /// 触发命名管道发送事件的异步方法。
        /// </summary>
        /// <param name="memory">待发送的字节内存。</param>
        /// <returns>一个等待任务，结果指示发送操作是否成功。</returns>
        protected virtual ValueTask<bool> OnNamedPipeSending(ReadOnlyMemory<byte> memory)
        {
            // 将发送任务委托给插件管理器，以便在所有相关的插件中引发命名管道发送事件
            return this.PluginManager.RaiseAsync(typeof(INamedPipeSendingPlugin), this.Resolver, this, new SendingEventArgs(memory));
        }

        /// <summary>
        /// 设置适配器
        /// </summary>
        /// <param name="adapter">要设置的适配器实例</param>
        protected void SetAdapter(SingleStreamDataHandlingAdapter adapter)
        {
            // 检查当前实例是否已被释放
            this.ThrowIfDisposed();

            // 如果传入的适配器为空，则抛出参数为空的异常
            if (adapter is null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            // 如果当前实例已有配置，则将配置应用到新适配器上
            if (this.Config != null)
            {
                adapter.Config(this.Config);
            }

            // 将当前实例的日志记录器设置到适配器上
            adapter.Logger = this.Logger;
            // 调用适配器的OnLoaded方法，通知适配器已被加载
            adapter.OnLoaded(this);
            // 设置适配器接收数据时的回调方法
            adapter.ReceivedAsyncCallBack = this.PrivateHandleReceivedData;
            // 设置适配器发送数据时的异步回调方法
            adapter.SendAsyncCallBack = this.ProtectedDefaultSendAsync;
            // 将适配器实例设置为当前数据处理适配器
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
                        await this.HandleReceivingData(byteBlock).ConfigureAwait(false);
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

                if (this.m_dataHandlingAdapter == null)
                {
                    await this.PrivateHandleReceivedData(byteBlock, default).ConfigureAwait(false);
                }
                else
                {
                    await this.m_dataHandlingAdapter.ReceivedInputAsync(byteBlock).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Error, this, "在处理数据时发生错误", ex);
            }
        }

        private void OnPeriod(long value)
        {
            this.m_receiveBufferSize = TouchSocketCoreUtility.HitBufferLength(value);
        }

        private async Task PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            var receiver = this.m_receiver;
            if (receiver != null)
            {
                await receiver.InputReceiveAsync(byteBlock, requestInfo).ConfigureAwait(false);
                return;
            }
            await this.OnNamedPipeReceived(new ReceivedDataEventArgs(byteBlock, requestInfo)).ConfigureAwait(false);
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

        #region 直接发送
        /// <inheritdoc/>
        protected async Task ProtectedDefaultSendAsync(ReadOnlyMemory<byte> memory)
        {
            this.ThrowIfDisposed();
            this.ThrowIfClientNotConnected();
            await this.OnNamedPipeSending(memory).ConfigureAwait(false);
            try
            {
                await this.m_semaphoreSlimForSend.WaitAsync().ConfigureAwait(false);
                await this.m_pipeStream.WriteAsync(memory, CancellationToken.None).ConfigureAwait(false);
                this.LastSentTime = DateTime.UtcNow;
            }
            finally
            {
                this.m_semaphoreSlimForSend.Release();
            }
        }

        #endregion 直接发送

        #region 异步发送

        /// <summary>
        /// 异步发送数据，根据是否配置了数据处理适配器来决定数据的发送方式。
        /// </summary>
        /// <param name="memory">待发送的字节内存。</param>
        /// <returns>一个异步任务，表示发送操作。</returns>
        protected Task ProtectedSendAsync(in ReadOnlyMemory<byte> memory)
        {
            // 如果未配置数据处理适配器，则使用默认的发送方式。
            if (this.m_dataHandlingAdapter == null)
            {
                return this.ProtectedDefaultSendAsync(memory);
            }
            else
            {
                // 如果配置了数据处理适配器，则使用适配器指定的发送方式。
                return this.m_dataHandlingAdapter.SendInputAsync(memory);
            }
        }


        /// <summary>
        /// 异步安全发送请求信息。
        /// </summary>
        /// <param name="requestInfo">请求信息对象，包含要发送的数据。</param>
        /// <returns>返回一个任务，表示异步操作的结果。</returns>
        /// <remarks>
        /// 此方法用于在发送请求之前验证是否可以发送请求信息，
        /// 并通过<see cref="DataHandlingAdapter"/>适配器安全处理发送过程。
        /// </remarks>
        protected Task ProtectedSendAsync(IRequestInfo requestInfo)
        {
            // 验证当前状态是否允许发送请求信息，如果不允许则抛出异常。
            this.ThrowIfCannotSendRequestInfo();
            // 调用ProtectedDataHandlingAdapter的SendInputAsync方法异步发送请求信息。
            return this.m_dataHandlingAdapter.SendInputAsync(requestInfo);
        }

        /// <summary>
        /// 异步发送经过处理的数据。
        /// 如果ProtectedDataHandlingAdapter未设置或者不支持拼接发送，则将transferBytes合并到一个连续的内存块中再发送。
        /// 如果ProtectedDataHandlingAdapter已设置且支持拼接发送，则直接发送transferBytes。
        /// </summary>
        /// <param name="transferBytes">待发送的字节数据列表，每个元素包含要传输的字节片段。</param>
        /// <returns>发送任务。</returns>
        protected async Task ProtectedSendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            // 检查ProtectedDataHandlingAdapter是否已设置且支持拼接发送
            if (this.m_dataHandlingAdapter == null || !this.m_dataHandlingAdapter.CanSplicingSend)
            {
                // 如果不支持拼接发送，计算所有字节片段的总长度
                var length = 0;
                foreach (var item in transferBytes)
                {
                    length += item.Count;
                }
                // 使用计算出的总长度创建一个连续的内存块
                using (var byteBlock = new ByteBlock(length))
                {
                    // 将每个字节片段写入连续的内存块
                    foreach (var item in transferBytes)
                    {
                        byteBlock.Write(new ReadOnlySpan<byte>(item.Array, item.Offset, item.Count));
                    }
                    // 根据ProtectedDataHandlingAdapter的状态选择发送方法
                    if (this.m_dataHandlingAdapter == null)
                    {
                        // 如果未设置ProtectedDataHandlingAdapter，使用默认发送方法
                        await this.ProtectedDefaultSendAsync(byteBlock.Memory).ConfigureAwait(false);
                    }
                    else
                    {
                        // 如果已设置ProtectedDataHandlingAdapter但不支持拼接发送，使用Adapter的发送方法
                        await this.m_dataHandlingAdapter.SendInputAsync(byteBlock.Memory).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                // 如果已设置ProtectedDataHandlingAdapter且支持拼接发送，直接使用Adapter的发送方法
                await this.m_dataHandlingAdapter.SendInputAsync(transferBytes).ConfigureAwait(false);
            }
        }

        #endregion 异步发送
    }
}