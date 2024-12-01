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
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// TcpClientBase类是作为一个抽象基类设计的，它继承自SetupConfigObject，并实现了ITcpSession接口。
    /// 这个类的主要目的是为TCP会话相关的操作提供一个基础框架，同时整合了配置设定的功能。
    /// </summary>
    public abstract partial class TcpClientBase : SetupConfigObject, ITcpSession
    {
        /// <summary>
        /// 构造函数用于初始化TcpClientBase类的实例。
        /// </summary>
        public TcpClientBase()
        {
            // 设置协议为TCP
            this.Protocol = Protocol.Tcp;
        }

        #region 变量

        private readonly object m_lockForAbort = new object();
        private readonly SemaphoreSlim m_semaphoreForConnect = new SemaphoreSlim(1, 1);
        private readonly TcpCore m_tcpCore = new TcpCore();
        private Task m_beginReceiveTask;
        private SingleStreamDataHandlingAdapter m_dataHandlingAdapter;
        private string m_iP;
        private Socket m_mainSocket;
        private volatile bool m_online;
        private int m_port;
        private InternalReceiver m_receiver;

        #endregion 变量

        #region 事件

        /// <summary>
        /// 在连接断开时触发。
        /// <para>
        /// 如果重写此方法，则不会触发<see cref="ITcpClosedPlugin"/>插件。
        /// </para>
        /// </summary>
        /// <param name="e">包含断开连接相关信息的事件参数</param>
        protected virtual async Task OnTcpClosed(ClosedEventArgs e)
        {
            // 调用插件管理器，触发所有ITcpClosedPlugin类型的插件
            await this.PluginManager.RaiseAsync(typeof(ITcpClosedPlugin), this.Resolver, this, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// <para>
        /// 覆盖父类方法，将不会触发<see cref="ITcpClosingPlugin"/>插件。
        /// </para>
        /// </summary>
        /// <param name="e">包含断开连接相关信息的事件参数</param>
        protected virtual async Task OnTcpClosing(ClosingEventArgs e)
        {
            // 调用插件管理器，触发所有ITcpClosingPlugin类型的插件事件
            await this.PluginManager.RaiseAsync(typeof(ITcpClosingPlugin), this.Resolver, this, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 建立Tcp连接时触发。
        /// <para>
        /// 覆盖父类方法，将不会触发<see cref="ITcpConnectedPlugin"/>插件。
        /// </para>
        /// </summary>
        /// <param name="e">连接事件参数</param>
        protected virtual async Task OnTcpConnected(ConnectedEventArgs e)
        {
            // 调用插件管理器，异步触发所有ITcpConnectedPlugin类型的插件
            await this.PluginManager.RaiseAsync(typeof(ITcpConnectedPlugin), this.Resolver, this, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 准备连接的时候，此时已初始化Socket，但是并未建立Tcp连接。
        /// <para>
        /// 覆盖父类方法，将不会触发<see cref="ITcpConnectingPlugin"/>插件。
        /// </para>
        /// </summary>
        /// <param name="e">连接事件参数</param>
        protected virtual async Task OnTcpConnecting(ConnectingEventArgs e)
        {
            // 调用插件管理器，触发ITcpConnectingPlugin类型的插件进行相应操作
            await this.PluginManager.RaiseAsync(typeof(ITcpConnectingPlugin), this.Resolver, this, e).ConfigureAwait(false);
        }

        private async Task PrivateOnTcpClosed(object obj)
        {
            try
            {
                await this.m_beginReceiveTask.ConfigureAwait(false);

                var e = (ClosedEventArgs)obj;

                var receiver = this.m_receiver;
                if (receiver != null)
                {
                    await receiver.Complete(e.Message).ConfigureAwait(false);
                }
                await this.OnTcpClosed(e).ConfigureAwait(false);
            }
            catch
            {
            }
        }

        private Task PrivateOnTcpClosing(ClosingEventArgs e)
        {
            return this.OnTcpClosing(e);
        }

        private async Task PrivateOnTcpConnected(object o)
        {
            try
            {
                this.m_beginReceiveTask = Task.Run(this.BeginReceive);

                this.m_beginReceiveTask.FireAndForget();
                await this.OnTcpConnected((ConnectedEventArgs)o).ConfigureAwait(false);
            }
            catch
            {
            }
        }

        private async Task PrivateOnTcpConnecting(ConnectingEventArgs e)
        {
            await this.OnTcpConnecting(e).ConfigureAwait(false);
            if (this.m_dataHandlingAdapter == null)
            {
                var adapter = this.Config.GetValue(TouchSocketConfigExtension.TcpDataHandlingAdapterProperty)?.Invoke();
                if (adapter != null)
                {
                    this.SetAdapter(adapter);
                }
            }
        }

        #endregion 事件

        #region 属性

        /// <inheritdoc/>
        public SingleStreamDataHandlingAdapter DataHandlingAdapter => this.m_dataHandlingAdapter;

        /// <inheritdoc/>
        public string IP => this.m_iP;

        /// <inheritdoc/>
        public bool IsClient => true;

        /// <inheritdoc/>
        public DateTime LastReceivedTime => this.m_tcpCore.ReceiveCounter.LastIncrement;

        /// <inheritdoc/>
        public DateTime LastSentTime => this.m_tcpCore.SendCounter.LastIncrement;

        /// <inheritdoc/>
        public Socket MainSocket => this.m_mainSocket;

        /// <inheritdoc/>
        public virtual bool Online => this.m_online;

        /// <inheritdoc/>
        public int Port => this.m_port;

        /// <inheritdoc/>
        public Protocol Protocol { get; protected set; }

        /// <inheritdoc/>
        public IPHost RemoteIPHost => this.Config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);

        /// <inheritdoc/>
        public bool UseSsl => this.m_tcpCore.UseSsl;

        #endregion 属性

        #region 断开操作

        /// <inheritdoc/>
        public virtual async Task CloseAsync(string msg)
        {
            if (this.m_online)
            {
                await this.PrivateOnTcpClosing(new ClosingEventArgs(msg)).ConfigureAwait(false);
                lock (this.m_lockForAbort)
                {
                    //https://gitee.com/RRQM_Home/TouchSocket/issues/IASH1A
                    this.MainSocket.TryClose();
                    this.Abort(true, msg);
                }
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Abort(true, TouchSocketResource.DisposeClose);
                this.m_tcpCore.SafeDispose();
            }

            base.Dispose(disposing);
        }

        #endregion 断开操作

        /// <summary>
        /// 中止连接。
        /// </summary>
        /// <param name="manual">是否为手动中止。</param>
        /// <param name="msg">中止的消息。</param>
        protected void Abort(bool manual, string msg)
        {
            // 锁定对象以确保线程安全
            lock (this.m_lockForAbort)
            {
                // 检查是否在线状态
                if (this.m_online)
                {
                    // 将在线状态设置为false
                    this.m_online = false;
                    // 安全地释放主套接字资源
                    this.MainSocket.SafeDispose();

                    // 安全地释放数据处理适配器资源
                    var adapter = this.m_dataHandlingAdapter;
                    this.m_dataHandlingAdapter = default;
                    adapter.SafeDispose();

                    // 启动一个新任务来处理连接关闭事件
                    Task.Factory.StartNew(this.PrivateOnTcpClosed, new ClosedEventArgs(manual, msg));
                }
            }
        }

        /// <summary>
        /// 当收到适配器处理的数据时。
        /// </summary>
        /// <param name="e">包含接收到的数据事件的相关信息。</param>
        protected virtual async Task OnTcpReceived(ReceivedDataEventArgs e)
        {
            // 提高插件管理器，让所有实现ITcpReceivedPlugin接口的插件处理接收到的数据。
            await this.PluginManager.RaiseAsync(typeof(ITcpReceivedPlugin), this.Resolver, this, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 当收到原始数据时，触发相关插件进行处理。
        /// </summary>
        /// <param name="byteBlock">包含收到的原始数据的字节块。</param>
        /// <returns>
        /// 如果返回<see langword="true"/>，则表示数据已被处理，且不会再向下传递。
        /// </returns>
        protected virtual ValueTask<bool> OnTcpReceiving(ByteBlock byteBlock)
        {
            // 提交异步事件，通知所有实现了ITcpReceivingPlugin接口的插件进行数据处理。
            return this.PluginManager.RaiseAsync(typeof(ITcpReceivingPlugin), this.Resolver, this, new ByteBlockEventArgs(byteBlock));
        }

        /// <summary>
        /// 当即将发送时，如果覆盖父类方法，则不会触发插件。
        /// </summary>
        /// <param name="memory">待发送的数据，以只读内存形式提供。</param>
        /// <returns>返回值意义：表示是否继续发送数据的指示，true为继续，false为取消发送。</returns>
        protected virtual ValueTask<bool> OnTcpSending(ReadOnlyMemory<byte> memory)
        {
            // 提高插件管理器，让所有实现ITcpSendingPlugin接口的插件决定是否继续发送数据。
            return this.PluginManager.RaiseAsync(typeof(ITcpSendingPlugin), this.Resolver, this, new SendingEventArgs(memory));
        }

        /// <summary>
        /// 设置适配器。
        /// </summary>
        /// <param name="adapter">要设置的适配器实例。</param>
        protected void SetAdapter(SingleStreamDataHandlingAdapter adapter)
        {
            // 检查当前实例是否已被释放
            this.ThrowIfDisposed();

            // 如果适配器参数为null，抛出ArgumentNullException异常
            ThrowHelper.ThrowArgumentNullExceptionIf(adapter, nameof(adapter));

            // 如果当前实例的配置对象不为空，则将配置应用到适配器上
            if (this.Config != null)
            {
                adapter.Config(this.Config);
            }

            // 设置适配器的日志记录器为当前实例的日志记录器
            adapter.Logger = this.Logger;
            // 通知适配器当前实例已加载
            adapter.OnLoaded(this);
            // 设置适配器接收到数据时的回调方法
            adapter.ReceivedAsyncCallBack = this.PrivateHandleReceivedData;
            // 设置适配器发送数据时的异步回调方法
            adapter.SendAsyncCallBack = this.ProtectedDefaultSendAsync;
            // 将当前适配器设置为实例的适配器
            this.m_dataHandlingAdapter = adapter;
        }

        private Task AuthenticateAsync()
        {
            if (this.Config.GetValue(TouchSocketConfigExtension.SslOptionProperty) is ClientSslOption sslOption)
            {
                return this.m_tcpCore.AuthenticateAsync(sslOption);
            }
            return EasyTask.CompletedTask;
        }

        private async Task BeginReceive()
        {
            var byteBlock = new ByteBlock(this.m_tcpCore.ReceiveBufferSize);
            while (true)
            {
                try
                {
                    var result = await this.m_tcpCore.ReadAsync(byteBlock.TotalMemory).ConfigureAwait(false);

                    if (this.DisposedValue)
                    {
                        byteBlock.Dispose();
                        return;
                    }

                    if (result.BytesTransferred > 0)
                    {
                        byteBlock.SetLength(result.BytesTransferred);
                        try
                        {
                            if (await this.OnTcpReceiving(byteBlock).ConfigureAwait(false))
                            {
                                continue;
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
                            this.Logger?.Exception(ex);
                        }
                        finally
                        {
                            if (byteBlock.Holding || byteBlock.DisposedValue)
                            {
                                byteBlock.Dispose();//释放上个内存
                                byteBlock = new ByteBlock(this.m_tcpCore.ReceiveBufferSize);
                            }
                            else
                            {
                                byteBlock.Reset();
                                if (this.m_tcpCore.ReceiveBufferSize > byteBlock.Capacity)
                                {
                                    byteBlock.SetCapacity(this.m_tcpCore.ReceiveBufferSize);
                                }
                            }
                        }
                    }
                    else if (result.SocketError != null)
                    {
                        byteBlock.Dispose();
                        this.Abort(false, result.SocketError.Message);
                        return;
                    }
                    else
                    {
                        byteBlock.Dispose();
                        this.Abort(false, TouchSocketResource.RemoteDisconnects);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    byteBlock.Dispose();
                    this.Abort(false, ex.Message);
                    return;
                }
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
            var receiver = this.m_receiver;
            if (receiver != null)
            {
                await receiver.InputReceiveAsync(byteBlock, requestInfo).ConfigureAwait(false);
                return;
            }
            await this.OnTcpReceived(new ReceivedDataEventArgs(byteBlock, requestInfo)).ConfigureAwait(false);
        }

        private void SetSocket(Socket socket)
        {
            if (socket == null)
            {
                this.m_iP = null;
                this.m_port = -1;
                return;
            }

            this.m_iP = socket.RemoteEndPoint.GetIP();
            this.m_port = socket.RemoteEndPoint.GetPort();
            this.m_mainSocket = socket;
            this.m_tcpCore.Reset(socket);
            //this.m_tcpCore.OnReceived = this.HandleReceived;
            //this.m_tcpCore.OnAbort = this.TcpCoreAbort;
            if (this.Config.GetValue(TouchSocketConfigExtension.MinBufferSizeProperty) is int minValue)
            {
                this.m_tcpCore.MinBufferSize = minValue;
            }

            if (this.Config.GetValue(TouchSocketConfigExtension.MaxBufferSizeProperty) is int maxValue)
            {
                this.m_tcpCore.MaxBufferSize = maxValue;
            }
        }

        #region Throw

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfCannotSendRequestInfo()
        {
            if (this.m_dataHandlingAdapter == null || !this.m_dataHandlingAdapter.CanSendRequestInfo)
            {
                ThrowHelper.ThrowNotSupportedException(TouchSocketResource.CannotSendRequestInfo);
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

        /// <summary>
        /// 异步发送数据，保护方法。
        /// 此方法用于在已建立的TCP连接上异步发送数据。
        /// 它首先检查当前实例是否已被弃用，然后检查客户端是否已连接。
        /// 如果这些检查通过，它将调用OnTcpSending事件处理程序进行预发送处理，
        /// 最后通过TCP核心发送数据。
        /// </summary>
        /// <param name="memory">要发送的数据，存储在只读内存区中。</param>
        /// <returns>一个Task对象，表示异步操作的结果。</returns>
        /// <exception cref="ObjectDisposedException">如果当前实例已被弃用，则抛出此异常。</exception>
        /// <exception cref="InvalidOperationException">如果客户端未连接，则抛出此异常。</exception>
        protected async Task ProtectedDefaultSendAsync(ReadOnlyMemory<byte> memory)
        {
            // 检查当前实例是否已被弃用
            this.ThrowIfDisposed();
            // 检查客户端是否已连接
            this.ThrowIfClientNotConnected();
            // 调用OnTcpSending事件处理程序进行预发送处理
            await this.OnTcpSending(memory).ConfigureAwait(false);
            // 通过TCP核心发送数据
            await this.m_tcpCore.SendAsync(memory).ConfigureAwait(false);
        }

        #endregion 直接发送

        #region 异步发送

        /// <summary>
        /// 异步发送数据，通过适配器模式灵活处理数据发送。
        /// </summary>
        /// <param name="memory">待发送的只读字节内存块。</param>
        /// <returns>一个异步任务，表示发送操作。</returns>
        protected Task ProtectedSendAsync(in ReadOnlyMemory<byte> memory)
        {
            // 如果数据处理适配器未设置，则使用默认发送方式。
            if (this.m_dataHandlingAdapter == null)
            {
                return this.ProtectedDefaultSendAsync(memory);
            }
            else
            {
                // 否则，使用适配器的发送方法进行数据发送。
                return this.m_dataHandlingAdapter.SendInputAsync(memory);
            }
        }

        /// <summary>
        /// 异步发送请求信息的受保护方法。
        ///
        /// 此方法首先检查当前对象是否能够发送请求信息，如果不能，则抛出异常。
        /// 如果可以发送，它将使用数据处理适配器来异步发送输入请求。
        /// </summary>
        /// <param name="requestInfo">要发送的请求信息。</param>
        /// <returns>返回一个任务，该任务代表异步操作的结果。</returns>
        protected Task ProtectedSendAsync(in IRequestInfo requestInfo)
        {
            // 检查是否具备发送请求的条件，如果不具备则抛出异常
            this.ThrowIfCannotSendRequestInfo();

            // 使用数据处理适配器异步发送输入请求
            return this.m_dataHandlingAdapter.SendInputAsync(requestInfo);
        }

        /// <summary>
        /// 异步发送数据。
        /// 如果数据处理适配器不存在或无法拼接发送，则将所有传输字节合并到一个连续的内存块中发送。
        /// 如果数据处理适配器存在且支持拼接发送，则直接发送传输字节列表。
        /// </summary>
        /// <param name="transferBytes">要发送的字节数据列表，每个项代表一个字节片段。</param>
        /// <returns>发送任务。</returns>
        protected async Task ProtectedSendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            // 检查数据处理适配器是否存在且支持拼接发送
            if (this.m_dataHandlingAdapter == null || !this.m_dataHandlingAdapter.CanSplicingSend)
            {
                // 如果不支持拼接发送，则计算所有字节片段的总长度
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
                    // 根据数据处理适配器的存在与否，选择不同的发送方式
                    if (this.m_dataHandlingAdapter == null)
                    {
                        // 如果没有数据处理适配器，则使用默认方式发送
                        await this.ProtectedDefaultSendAsync(byteBlock.Memory).ConfigureAwait(false);
                    }
                    else
                    {
                        // 如果有数据处理适配器，则通过适配器发送
                        await this.m_dataHandlingAdapter.SendInputAsync(byteBlock.Memory).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                // 如果数据处理适配器支持拼接发送，则直接发送字节列表
                await this.m_dataHandlingAdapter.SendInputAsync(transferBytes).ConfigureAwait(false);
            }
        }

        #endregion 异步发送
    }
}