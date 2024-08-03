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
    /// Tcp客户端
    /// </summary>
    public abstract class TcpClientBase : SetupConfigObject, ITcpSession
    {
        /// <summary>
        /// Tcp客户端
        /// </summary>
        public TcpClientBase()
        {
            this.Protocol = Protocol.Tcp;
        }

        #region 变量

        private volatile bool m_online;
        private readonly SemaphoreSlim m_semaphoreForConnect = new SemaphoreSlim(1, 1);
        private readonly TcpCore m_tcpCore = new TcpCore();
        private readonly object m_lock = new object();
        private InternalReceiver m_receiver;
        private SingleStreamDataHandlingAdapter m_dataHandlingAdapter;
        private Socket m_mainSocket;
        private string m_iP;
        private int m_port;
        private Task m_beginReceiveTask;

        #endregion 变量

        #region 事件

        private async Task PrivateOnTcpConnected(object o)
        {
            this.m_beginReceiveTask = Task.Run(this.BeginReceive);

            this.m_beginReceiveTask.FireAndForget();
            await this.OnTcpConnected((ConnectedEventArgs)o).ConfigureAwait(false);
        }

        /// <summary>
        /// 建立Tcp连接时触发。
        /// <para>
        /// 覆盖父类方法，将不会触发<see cref="ITcpConnectedPlugin"/>插件。
        /// </para>
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnTcpConnected(ConnectedEventArgs e)
        {
            await this.PluginManager.RaiseAsync(typeof(ITcpConnectedPlugin), this, e).ConfigureAwait(false);
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

        /// <summary>
        /// 准备连接的时候，此时已初始化Socket，但是并未建立Tcp连接。
        /// <para>
        /// 覆盖父类方法，将不会触发<see cref="ITcpConnectingPlugin"/>插件。
        /// </para>
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnTcpConnecting(ConnectingEventArgs e)
        {
            await this.PluginManager.RaiseAsync(typeof(ITcpConnectingPlugin), this, e).ConfigureAwait(false);
        }

        private async Task PrivateOnTcpClosed(object obj)
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

        /// <summary>
        /// 连接断开时触发。
        /// <para>
        /// 覆盖父类方法将不触发<see cref="ITcpClosedPlugin"/>插件。
        /// </para>
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnTcpClosed(ClosedEventArgs e)
        {
            await this.PluginManager.RaiseAsync(typeof(ITcpClosedPlugin), this, e).ConfigureAwait(false);
        }

        private Task PrivateOnTcpClosing(ClosingEventArgs e)
        {
            return this.OnTcpClosing(e);
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// <para>
        /// 覆盖父类方法，将不会触发<see cref="ITcpClosingPlugin"/>插件。
        /// </para>
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnTcpClosing(ClosingEventArgs e)
        {
            await this.PluginManager.RaiseAsync(typeof(ITcpClosingPlugin), this, e).ConfigureAwait(false);
        }

        #endregion 事件

        #region 属性

        /// <inheritdoc/>
        public DateTime LastReceivedTime => this.m_tcpCore.ReceiveCounter.LastIncrement;

        /// <inheritdoc/>
        public DateTime LastSentTime => this.m_tcpCore.SendCounter.LastIncrement;

        /// <inheritdoc/>
        public SingleStreamDataHandlingAdapter DataHandlingAdapter => this.m_dataHandlingAdapter;

        /// <inheritdoc/>
        public string IP => this.m_iP;

        /// <inheritdoc/>
        public Socket MainSocket => this.m_mainSocket;

        /// <inheritdoc/>
        public virtual bool Online => this.m_online;

        /// <inheritdoc/>
        public int Port => this.m_port;

        /// <inheritdoc/>
        public bool UseSsl => this.m_tcpCore.UseSsl;

        /// <inheritdoc/>
        public Protocol Protocol { get; protected set; }

        /// <inheritdoc/>
        public IPHost RemoteIPHost => this.Config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);

        /// <inheritdoc/>
        public bool IsClient => true;

        #endregion 属性

        #region 断开操作

        /// <inheritdoc/>
        public virtual async Task CloseAsync(string msg)
        {
            if (this.m_online)
            {
                await this.PrivateOnTcpClosing(new ClosingEventArgs(msg)).ConfigureAwait(false);
                this.MainSocket.TryClose();
                this.Abort(true, msg);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
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

        #region Connect

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
                    else if (result.HasError)
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

        private void Authenticate()
        {
            if (this.Config.GetValue(TouchSocketConfigExtension.SslOptionProperty) is ClientSslOption sslOption)
            {
                this.m_tcpCore.Authenticate(sslOption);
            }
        }

#if NET6_0_OR_GREATER

        /// <summary>
        /// 异步连接服务器
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TimeoutException"></exception>
        protected async Task TcpConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            this.ThrowIfDisposed();
            this.ThrowIfConfigIsNull();
            await this.m_semaphoreForConnect.WaitTimeAsync(millisecondsTimeout, token).ConfigureAwait(false);
            try
            {
                if (this.m_online)
                {
                    return;
                }

                var iPHost = ThrowHelper.ThrowArgumentNullExceptionIf(this.RemoteIPHost, nameof(this.RemoteIPHost));
                this.MainSocket.SafeDispose();
                var socket = this.CreateSocket(iPHost);
                var args = new ConnectingEventArgs();
                await this.PrivateOnTcpConnecting(args).ConfigureAwait(false);
                if (token.CanBeCanceled)
                {
                    await socket.ConnectAsync(iPHost.Host, iPHost.Port, token).ConfigureAwait(false);
                }
                else
                {
                    using (var tokenSource = new CancellationTokenSource(millisecondsTimeout))
                    {
                        try
                        {
                            await socket.ConnectAsync(iPHost.Host, iPHost.Port, tokenSource.Token).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            throw new TimeoutException();
                        }
                    }
                }
                this.m_online = true;
                this.SetSocket(socket);
                this.Authenticate();
                _ = Task.Factory.StartNew(this.PrivateOnTcpConnected, new ConnectedEventArgs());
            }
            finally
            {
                this.m_semaphoreForConnect.Release();
            }
        }
#else

        /// <summary>
        /// 异步连接服务器
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected async Task TcpConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            this.ThrowIfDisposed();
            this.ThrowIfConfigIsNull();
            await this.m_semaphoreForConnect.WaitTimeAsync(millisecondsTimeout, token).ConfigureAwait(false);

            try
            {
                if (this.m_online)
                {
                    return;
                }
                var iPHost = ThrowHelper.ThrowArgumentNullExceptionIf(this.RemoteIPHost, nameof(this.RemoteIPHost));
                this.MainSocket.SafeDispose();
                var socket = this.CreateSocket(iPHost);
                await this.PrivateOnTcpConnecting(new ConnectingEventArgs()).ConfigureAwait(false);
                var task = Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, iPHost.Host, iPHost.Port, null);
                await task.WaitAsync(TimeSpan.FromMilliseconds(millisecondsTimeout)).ConfigureAwait(false);
                this.m_online = true;
                this.SetSocket(socket);
                this.Authenticate();
                _ = Task.Factory.StartNew(this.PrivateOnTcpConnected, new ConnectedEventArgs());
            }
            finally
            {
                this.m_semaphoreForConnect.Release();
            }
        }

#endif

        #endregion Connect

        /// <summary>
        /// Abort
        /// </summary>
        /// <param name="manual"></param>
        /// <param name="msg"></param>
        protected void Abort(bool manual, string msg)
        {
            lock (this.m_lock)
            {
                if (this.m_online)
                {
                    this.m_online = false;
                    this.MainSocket.SafeDispose();
                    this.DataHandlingAdapter.SafeDispose();
                    Task.Factory.StartNew(this.PrivateOnTcpClosed, new ClosedEventArgs(manual, msg));
                }
            }
        }

        #region Receiver

        /// <inheritdoc/>
        protected IReceiver<IReceiverResult> ProtectedCreateReceiver(IReceiverClient<IReceiverResult> receiverObject)
        {
            return this.m_receiver ??= new InternalReceiver(receiverObject);
        }

        /// <inheritdoc/>
        protected void ProtectedClearReceiver()
        {
            this.m_receiver = null;
        }

        #endregion Receiver

        private async Task PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.m_receiver != null)
            {
                await this.m_receiver.InputReceive(byteBlock, requestInfo).ConfigureAwait(false);
                return;
            }
            await this.OnTcpReceived(new ReceivedDataEventArgs(byteBlock, requestInfo)).ConfigureAwait(false);
        }

        /// <summary>
        /// 当收到适配器处理的数据时。
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnTcpReceived(ReceivedDataEventArgs e)
        {
            await this.PluginManager.RaiseAsync(typeof(ITcpReceivedPlugin), this, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 当即将发送时，如果覆盖父类方法，则不会触发插件。
        /// </summary>
        /// <param name="memory"></param>
        /// <returns>返回值意义</returns>
        protected virtual ValueTask<bool> OnTcpSending(ReadOnlyMemory<byte> memory)
        {
            return this.PluginManager.RaiseAsync(typeof(ITcpSendingPlugin), this, new SendingEventArgs(memory));
        }

        /// <summary>
        /// 设置适配器。
        /// </summary>
        /// <param name="adapter"></param>
        protected void SetAdapter(SingleStreamDataHandlingAdapter adapter)
        {
            this.ThrowIfDisposed();

            ThrowHelper.ThrowArgumentNullExceptionIf(adapter, nameof(adapter));

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

        private Socket CreateSocket(IPHost iPHost)
        {
            Socket socket;
            if (iPHost.HostNameType == UriHostNameType.Dns)
            {
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = this.Config.GetValue(TouchSocketConfigExtension.SendTimeoutProperty)
                };
            }
            else
            {
                socket = new Socket(iPHost.EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = this.Config.GetValue(TouchSocketConfigExtension.SendTimeoutProperty)
                };
            }

            if (this.Config.GetValue(TouchSocketConfigExtension.KeepAliveValueProperty) is KeepAliveValue keepAliveValue)
            {
#if NET45_OR_GREATER

                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                socket.IOControl(IOControlCode.KeepAliveValues, keepAliveValue.KeepAliveTime, null);
#else
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    socket.IOControl(IOControlCode.KeepAliveValues, keepAliveValue.KeepAliveTime, null);
                }
#endif
            }

            var noDelay = this.Config.GetValue(TouchSocketConfigExtension.NoDelayProperty);
            if (noDelay.HasValue)
            {
                socket.NoDelay = noDelay.Value;
            }

            if (this.Config.GetValue(TouchSocketConfigExtension.BindIPHostProperty) != null)
            {
                if (this.Config.GetValue(TouchSocketConfigExtension.ReuseAddressProperty))
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                }
                socket.Bind(this.Config.GetValue(TouchSocketConfigExtension.BindIPHostProperty).EndPoint);
            }
            return socket;
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

        /// <summary>
        /// 当收到原始数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual ValueTask<bool> OnTcpReceiving(ByteBlock byteBlock)
        {
            return this.PluginManager.RaiseAsync(typeof(ITcpReceivingPlugin), this, new ByteBlockEventArgs(byteBlock));
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

        /// <inheritdoc/>
        protected async Task ProtectedDefaultSendAsync(ReadOnlyMemory<byte> memory)
        {
            this.ThrowIfDisposed();
            this.ThrowIfClientNotConnected();
            await this.OnTcpSending(memory).ConfigureAwait(false);
            await this.m_tcpCore.SendAsync(memory).ConfigureAwait(false);
        }

        #endregion 直接发送

        #region 异步发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <exception cref="ClientNotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        protected Task ProtectedSendAsync(in ReadOnlyMemory<byte> memory)
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
        protected Task ProtectedSendAsync(in IRequestInfo requestInfo)
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
                        await this.ProtectedDefaultSendAsync(byteBlock.Memory).ConfigureAwait(false);
                    }
                    else
                    {
                        await this.m_dataHandlingAdapter.SendInputAsync(byteBlock.Memory).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                await this.m_dataHandlingAdapter.SendInputAsync(transferBytes).ConfigureAwait(false);
            }
        }

        #endregion 异步发送
    }
}