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
using System.Linq;
using System.Net;
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
    /// UDP基类服务器。
    /// </summary>
    public abstract class UdpSessionBase : ServiceBase, IUdpSessionBase
    {
        #region 字段
        private ServerState m_serverState;
        private InternalUdpReceiver m_receiver;
        private UdpDataHandlingAdapter m_dataHandlingAdapter;
        private DateTime m_lastReceivedTime;
        private DateTime m_lastSendTime;
        private UdpNetworkMonitor m_monitor;
        private readonly List<Task> m_receiveTasks = new List<Task>();
        private readonly SemaphoreSlim m_semaphoreSlimForReceiver = new SemaphoreSlim(1, 1);
        #endregion 字段

        /// <summary>
        /// 构造函数
        /// </summary>
        public UdpSessionBase()
        {
            this.Protocol = Protocol.Udp;
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.m_monitor = new UdpNetworkMonitor(null, socket);
        }

        /// <inheritdoc/>
        public DateTime LastReceivedTime => this.m_lastReceivedTime;

        /// <inheritdoc/>
        public DateTime LastSentTime => this.m_lastSendTime;

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        protected UdpDataHandlingAdapter ProtectedDataHandlingAdapter => this.m_dataHandlingAdapter;

        /// <summary>
        /// 监听器
        /// </summary>
        public UdpNetworkMonitor Monitor => this.m_monitor;

        /// <inheritdoc/>
        public Protocol Protocol { get; protected set; }

        /// <summary>
        /// 默认远程节点
        /// </summary>
        public IPHost RemoteIPHost => this.Config?.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);

        /// <inheritdoc/>
        public override ServerState ServerState => this.m_serverState;

        /// <summary>
        /// 在Udp中，该值没有意义，且可以自由赋值。
        /// </summary>
        public bool IsClient { get; protected set; }

        /// <summary>
        /// 退出组播
        /// </summary>
        /// <param name="multicastAddr"></param>
        public void DropMulticastGroup(IPAddress multicastAddr)
        {
            if (this.DisposedValue)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (multicastAddr is null)
            {
                throw new ArgumentNullException(nameof(multicastAddr));
            }

            if (this.m_monitor.Socket.AddressFamily == AddressFamily.InterNetwork)
            {
                var optionValue = new MulticastOption(multicastAddr);
                this.m_monitor.Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, optionValue);
            }
            else
            {
                var optionValue2 = new IPv6MulticastOption(multicastAddr);
                this.m_monitor.Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.DropMembership, optionValue2);
            }
        }

        /// <summary>
        /// 加入组播。
        /// <para>组播地址为 224.0.0.0 ~ 239.255.255.255，其中 224.0.0.0~224.255.255.255 不建议在用户程序中使用，因为它们一般都有特殊用途。</para>
        /// </summary>
        /// <param name="multicastAddr"></param>
        public void JoinMulticastGroup(IPAddress multicastAddr)
        {
            if (multicastAddr is null)
            {
                throw new ArgumentNullException(nameof(multicastAddr));
            }
            if (this.DisposedValue)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (this.m_monitor.Socket.AddressFamily == AddressFamily.InterNetwork)
            {
                var optionValue = new MulticastOption(multicastAddr);
                this.m_monitor.Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, optionValue);
            }
            else
            {
                var optionValue2 = new IPv6MulticastOption(multicastAddr);
                this.m_monitor.Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, optionValue2);
            }
        }

        /// <inheritdoc/>
        public override async Task StartAsync()
        {
            this.ThrowIfDisposed();
            try
            {
                switch (this.m_serverState)
                {
                    case ServerState.None:
                        {
                            if (this.Config.GetValue(TouchSocketConfigExtension.BindIPHostProperty) is IPHost iPHost)
                            {
                                this.BeginReceive(iPHost);
                            }

                            break;
                        }
                    case ServerState.Running:
                        return;

                    case ServerState.Stopped:
                        {
                            if (this.Config.GetValue(TouchSocketConfigExtension.BindIPHostProperty) is IPHost iPHost)
                            {
                                this.BeginReceive(iPHost);
                            }
                            break;
                        }
                    default:
                        {
                            ThrowHelper.ThrowInvalidEnumArgumentException(this.m_serverState);
                            return;
                        }
                }

                this.m_serverState = ServerState.Running;

                await this.PluginManager.RaiseAsync(typeof(IServerStartedPlugin), this, new ServiceStateEventArgs(this.m_serverState, default)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.m_serverState = ServerState.Exception;
                await this.PluginManager.RaiseAsync(typeof(IServerStartedPlugin), this, new ServiceStateEventArgs(this.m_serverState, ex) { Message = ex.Message }).ConfigureAwait(false);
                throw;
            }
        }

        /// <inheritdoc/>
        public override async Task StopAsync()
        {
            this.m_monitor?.Socket.Dispose();
            this.m_monitor = null;
            this.m_serverState = ServerState.Stopped;
            await Task.WhenAll(this.m_receiveTasks.ToArray()).ConfigureAwait(false);
            this.m_receiveTasks.Clear();

            if (this.m_receiver != null)
            {
                await this.m_receiver.Complete(default).ConfigureAwait(false);
            }
            await this.PluginManager.RaiseAsync(typeof(IServerStartedPlugin), this, new ServiceStateEventArgs(this.m_serverState, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!this.DisposedValue)
            {
                if (disposing)
                {
                    this.StopAsync().GetFalseAwaitResult();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 处理已接收到的数据。
        /// </summary>
        protected virtual async Task OnUdpReceived(UdpReceivedDataEventArgs e)
        {
            await this.PluginManager.RaiseAsync(typeof(IUdpReceivedPlugin), this, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 当即将发送时，如果覆盖父类方法，则不会触发插件。
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="memory"></param>
        /// <returns></returns>
        protected virtual ValueTask<bool> OnUdpSending(EndPoint endPoint, ReadOnlyMemory<byte> memory)
        {
            return this.PluginManager.RaiseAsync(typeof(IUdpSendingPlugin), this, new UdpSendingEventArgs(memory, endPoint));
        }

        /// <summary>
        /// 在Socket初始化对象后，Bind之前调用。
        /// 可用于设置Socket参数。
        /// 父类方法可覆盖。
        /// </summary>
        /// <param name="socket"></param>
        protected virtual void PreviewBind(Socket socket)
        {
        }

        /// <summary>
        /// 设置适配器
        /// </summary>
        /// <param name="adapter"></param>
        protected void SetAdapter(UdpDataHandlingAdapter adapter)
        {
            this.ThrowIfDisposed();
            ThrowHelper.ThrowArgumentNullExceptionIf(adapter, nameof(adapter));

            if (this.Config != null)
            {
                adapter.Config(this.Config);
            }
            adapter.Logger = this.Logger;
            adapter.OnLoaded(this);
            adapter.ReceivedCallBack = this.PrivateHandleReceivedData;
            //adapter.SendCallBack = this.ProtectedDefaultSend;
            adapter.SendCallBackAsync = this.ProtectedDefaultSendAsync;
            this.m_dataHandlingAdapter = adapter;
        }

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            base.LoadConfig(config);
            if (this.m_dataHandlingAdapter == null)
            {
                var adapter = this.Config.GetValue(TouchSocketConfigExtension.UdpDataHandlingAdapterProperty)?.Invoke();
                if (adapter != null)
                {
                    this.SetAdapter(adapter);
                }
            }
        }

        private void BeginReceive(IPHost iPHost)
        {
            var threadCount = this.Config.GetValue(TouchSocketConfigExtension.ThreadCountProperty);
            threadCount = threadCount < 0 ? 1 : threadCount;
            var socket = new Socket(iPHost.EndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp)
            {
                EnableBroadcast = this.Config.GetValue(TouchSocketConfigExtension.EnableBroadcastProperty)
            };
            if (this.Config.GetValue(TouchSocketConfigExtension.ReuseAddressProperty))
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }

            #region Windows下UDP连接被重置错误10054

#if NET45_OR_GREATER
            if (this.Config.GetValue(TouchSocketConfigExtension.UdpConnResetProperty))
            {
                const int SIP_UDP_CONNRESET = -1744830452;
                socket.IOControl(SIP_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
            }
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (this.Config.GetValue(TouchSocketConfigExtension.UdpConnResetProperty))
                {
                    const int SIP_UDP_CONNRESET = -1744830452;
                    socket.IOControl(SIP_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
                }
            }
#endif

            #endregion Windows下UDP连接被重置错误10054

            this.PreviewBind(socket);

            socket.Bind(iPHost.EndPoint);

            this.m_monitor = new UdpNetworkMonitor(iPHost, socket);

            for (var i = 0; i < threadCount; i++)
            {
                var task = Task.Run(this.RunReceive);
                task.FireAndForget();
                this.m_receiveTasks.Add(task);
            }

            //#if NET45_OR_GREATER || NET6_0_OR_GREATER
            //            for (var i = 0; i < threadCount; i++)
            //            {
            //                var eventArg = new SocketAsyncEventArgs();
            //                this.m_socketAsyncs.Add(eventArg);
            //                eventArg.Completed += this.IO_Completed;
            //                var byteBlock = new ByteBlock(1024 * 64);
            //                eventArg.UserToken = byteBlock;
            //                eventArg.SetBuffer(byteBlock.Buffer, 0, byteBlock.Capacity);
            //                eventArg.RemoteEndPoint = iPHost.EndPoint;
            //                if (!socket.ReceiveFromAsync(eventArg))
            //                {
            //                    this.ProcessReceive(socket, eventArg);
            //                }
            //            }
            //#else
            //            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            //            {
            //                for (var i = 0; i < threadCount; i++)
            //                {
            //                    var eventArg = new SocketAsyncEventArgs();
            //                    this.m_socketAsyncs.Add(eventArg);
            //                    eventArg.Completed += this.IO_Completed;
            //                    var byteBlock = new ByteBlock(1024 * 64);
            //                    eventArg.UserToken = byteBlock;
            //                    eventArg.SetBuffer(byteBlock.Buffer, 0, byteBlock.Capacity);
            //                    eventArg.RemoteEndPoint = iPHost.EndPoint;
            //                    if (!socket.ReceiveFromAsync(eventArg))
            //                    {
            //                        this.ProcessReceive(socket, eventArg);
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                var thread = new Thread(this.Received);
            //                thread.IsBackground = true;
            //                thread.Start();
            //            }
            //#endif
        }

        private async Task RunReceive()
        {
            using (var udpSocketReceiver = new UdpSocketReceiver())
            {
                while (true)
                {
                    using (var byteBlock = new ByteBlock(1024 * 64))
                    {
                        try
                        {
                            if (this.m_serverState != ServerState.Running)
                            {
                                return;
                            }
                            var result = await udpSocketReceiver.ReceiveAsync(this.m_monitor.Socket, this.m_monitor.IPHost.EndPoint, byteBlock.TotalMemory).ConfigureAwait(false);

                            if (result.BytesTransferred > 0)
                            {
                                byteBlock.SetLength(result.BytesTransferred);
                                await this.HandleReceivingData(byteBlock, result.RemoteEndPoint).ConfigureAwait(false);
                            }
                            else if (result.HasError)
                            {
                                this.Logger?.Error(this, result.SocketError.Message);
                                return;
                            }
                            else
                            {
                                this.Logger?.Error(this, TouchSocketCoreResource.UnknownError);
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger?.Exception(ex);
                            return;
                        }
                    }
                }
            }
        }

        private async Task HandleReceivingData(ByteBlock byteBlock, EndPoint remoteEndPoint)
        {
            try
            {
                if (this.DisposedValue)
                {
                    return;
                }

                this.m_lastReceivedTime = DateTime.Now;

                if (await this.ReceivingData(byteBlock).ConfigureAwait(false))
                {
                    return;
                }

                if (this.m_dataHandlingAdapter == null)
                {
                    await this.PrivateHandleReceivedData(remoteEndPoint, byteBlock, default).ConfigureAwait(false);
                }
                else
                {
                    await this.m_dataHandlingAdapter.ReceivedInput(remoteEndPoint, byteBlock).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                this.Logger?.Exception(ex);
            }
        }

        /// <summary>
        /// 当收到原始数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual ValueTask<bool> ReceivingData(ByteBlock byteBlock)
        {
            return EasyValueTask.FromResult(false);
        }

        private async Task PrivateHandleReceivedData(EndPoint remoteEndPoint, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.m_receiver != null)
            {
                await this.m_semaphoreSlimForReceiver.WaitAsync().ConfigureAwait(false);
                try
                {
                    await this.m_receiver.InputReceive(remoteEndPoint, byteBlock, requestInfo).ConfigureAwait(false);
                    return;
                }
                finally
                {
                    this.m_semaphoreSlimForReceiver.Release();
                }
            }
            await this.OnUdpReceived(new UdpReceivedDataEventArgs(remoteEndPoint, byteBlock, requestInfo)).ConfigureAwait(false);
        }

        #region Throw

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThorwIfRemoteIPHostNull()
        {
            ThrowHelper.ThrowArgumentNullExceptionIf(this.RemoteIPHost, nameof(this.RemoteIPHost));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThorwIfCannotSend()
        {
            if (this.m_serverState != ServerState.Running)
            {
                ThrowHelper.ThrowException(TouchSocketResource.UdpStopped);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfCannotSendRequestInfo()
        {
            if (this.m_dataHandlingAdapter == null || !this.m_dataHandlingAdapter.CanSendRequestInfo)
            {
                ThrowHelper.ThrowNotSupportedException(TouchSocketResource.CannotSendRequestInfo);
            }
        }

        #endregion Throw

        //#region 向默认远程同步发送

        ///// <summary>
        ///// 向默认终结点发送
        ///// </summary>
        ///// <param name="buffer"></param>
        ///// <param name="offset"></param>
        ///// <param name="length"></param>
        //protected virtual void ProtectedSend(byte[] buffer, int offset, int length)
        //{
        //    this.ThorwIfRemoteIPHostNull();
        //    this.ProtectedSend(this.RemoteIPHost.EndPoint, buffer, offset, length);
        //}

        ///// <summary>
        ///// 向默认终结点发送
        ///// </summary>
        ///// <param name="requestInfo"></param>
        ///// <exception cref="OverlengthException"></exception>
        ///// <exception cref="Exception"></exception>
        //protected virtual void ProtectedSend(IRequestInfo requestInfo)
        //{
        //    this.ThorwIfRemoteIPHostNull();
        //    this.ProtectedSend(this.RemoteIPHost.EndPoint, requestInfo);
        //}

        //#endregion 向默认远程同步发送

        #region 向默认远程异步发送

        /// <inheritdoc/>
        protected virtual Task ProtectedSendAsync(ReadOnlyMemory<byte> memory)
        {
            this.ThorwIfRemoteIPHostNull();
            return this.ProtectedSendAsync(this.RemoteIPHost.EndPoint, memory);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        protected virtual Task ProtectedSendAsync(IRequestInfo requestInfo)
        {
            this.ThorwIfRemoteIPHostNull();
            return this.ProtectedSendAsync(this.RemoteIPHost.EndPoint, requestInfo);
        }

        #endregion 向默认远程异步发送

        //#region 向设置的远程同步发送

        ///// <summary>
        ///// 向设置的远程同步发送
        ///// </summary>
        ///// <param name="remoteEP"></param>
        ///// <param name="buffer"></param>
        ///// <param name="offset"></param>
        ///// <param name="length"></param>
        ///// <exception cref="ClientNotConnectedException"></exception>
        ///// <exception cref="OverlengthException"></exception>
        ///// <exception cref="Exception"></exception>
        //protected virtual void ProtectedSend(EndPoint remoteEP, byte[] buffer, int offset, int length)
        //{
        //    if (this.m_dataHandlingAdapter == null)
        //    {
        //        this.ProtectedDefaultSend(remoteEP, buffer, offset, length);
        //    }
        //    else
        //    {
        //        this.m_dataHandlingAdapter.SendInput(remoteEP, buffer, offset, length);
        //    }
        //}

        ///// <summary>
        ///// 向设置终结点发送
        ///// </summary>
        ///// <param name="endPoint"></param>
        ///// <param name="requestInfo"></param>
        ///// <exception cref="OverlengthException"></exception>
        ///// <exception cref="Exception"></exception>
        //protected virtual void ProtectedSend(EndPoint endPoint, IRequestInfo requestInfo)
        //{
        //    this.ThrowIfCannotSendRequestInfo();
        //    this.m_dataHandlingAdapter.SendInput(endPoint, requestInfo);
        //}

        //#endregion 向设置的远程同步发送

        #region 向设置的远程异步发送

        /// <summary>
        /// 向设置的远程异步发送
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="memory"></param>
        /// <exception cref="ClientNotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        protected virtual Task ProtectedSendAsync(EndPoint endPoint, ReadOnlyMemory<byte> memory)
        {
            return this.m_dataHandlingAdapter == null
                ? this.ProtectedDefaultSendAsync(endPoint, memory)
                : this.m_dataHandlingAdapter.SendInputAsync(endPoint, memory);
        }

        /// <inheritdoc/>
        protected virtual Task ProtectedSendAsync(EndPoint endPoint, IRequestInfo requestInfo)
        {
            this.ThrowIfCannotSendRequestInfo();
            return this.m_dataHandlingAdapter.SendInputAsync(endPoint, requestInfo);
        }

        #endregion 向设置的远程异步发送


        #region DefaultSendAsync

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        protected async Task ProtectedDefaultSendAsync(ReadOnlyMemory<byte> memory)
        {
            this.ThorwIfCannotSend();
            this.ThorwIfRemoteIPHostNull();
            await this.ProtectedDefaultSendAsync(this.RemoteIPHost.EndPoint, memory).ConfigureAwait(false);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="memory"></param>
#if NET6_0_OR_GREATER
        protected async Task ProtectedDefaultSendAsync(EndPoint endPoint, ReadOnlyMemory<byte> memory)
        {
            this.ThrowIfDisposed();
            this.ThorwIfCannotSend();
            await this.OnUdpSending(endPoint, memory).ConfigureAwait(false);
            await this.Monitor.Socket.SendToAsync(memory, SocketFlags.None, endPoint);
            this.m_lastSendTime = DateTime.Now;
        }
#else

        protected async Task ProtectedDefaultSendAsync(EndPoint endPoint, ReadOnlyMemory<byte> memory)
        {
            this.ThorwIfCannotSend();
            this.ThrowIfDisposed();

            await this.OnUdpSending(endPoint, memory).ConfigureAwait(false);
            if (MemoryMarshal.TryGetArray(memory, out var segment))
            {
                this.Monitor.Socket.SendTo(segment.Array, segment.Offset, segment.Count, SocketFlags.None, endPoint);
            }
            else
            {
                var array = memory.ToArray();

                this.Monitor.Socket.SendTo(array, 0, array.Length, SocketFlags.None, endPoint);
            }
            this.m_lastSendTime = DateTime.Now;
        }

#endif

        #endregion DefaultSendAsync

        #region 组合发送

        ///// <summary>
        ///// <inheritdoc/>
        ///// </summary>
        ///// <param name="transferBytes"></param>
        //protected void ProtectedSend(IList<ArraySegment<byte>> transferBytes)
        //{
        //    this.ThorwIfRemoteIPHostNull();
        //    this.ProtectedSend(this.RemoteIPHost.EndPoint, transferBytes);
        //}

        ///// <summary>
        ///// <inheritdoc/>
        ///// </summary>
        ///// <param name="endPoint"></param>
        ///// <param name="transferBytes"></param>
        //protected void ProtectedSend(EndPoint endPoint, IList<ArraySegment<byte>> transferBytes)
        //{
        //    this.ThrowIfDisposed();
        //    if (this.m_dataHandlingAdapter == null || !this.m_dataHandlingAdapter.CanSplicingSend)
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
        //            if (this.m_dataHandlingAdapter == null)
        //            {
        //                this.ProtectedDefaultSend(endPoint, byteBlock.Buffer, 0, byteBlock.Len);
        //            }
        //            else
        //            {
        //                this.m_dataHandlingAdapter.SendInput(endPoint, byteBlock.Buffer, 0, byteBlock.Len);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        this.m_dataHandlingAdapter.SendInput(endPoint, transferBytes);
        //    }
        //}

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        protected Task ProtectedSendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            this.ThorwIfRemoteIPHostNull();
            return this.ProtectedSendAsync(this.RemoteIPHost.EndPoint, transferBytes);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="transferBytes"></param>
        protected async Task ProtectedSendAsync(EndPoint endPoint, IList<ArraySegment<byte>> transferBytes)
        {
            this.ThrowIfDisposed();
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
                        await this.ProtectedDefaultSendAsync(endPoint, byteBlock.Memory).ConfigureAwait(false);
                    }
                    else
                    {
                        await this.m_dataHandlingAdapter.SendInputAsync(endPoint, byteBlock.Memory).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                await this.m_dataHandlingAdapter.SendInputAsync(endPoint, transferBytes).ConfigureAwait(false);
            }
        }

        #endregion 组合发送

        #region Receiver

        /// <inheritdoc/>
        protected IReceiver<IUdpReceiverResult> ProtectedCreateReceiver(IReceiverClient<IUdpReceiverResult> receiverObject)
        {
            return this.m_receiver ??= new InternalUdpReceiver(receiverObject);
        }

        /// <inheritdoc/>
        protected void ProtectedClearReceiver()
        {
            this.m_receiver = null;
        }

        #endregion Receiver
    }
}