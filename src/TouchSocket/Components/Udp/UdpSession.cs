//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
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
    /// 简单UDP会话。
    /// </summary>
    public class UdpSession : UdpSessionBase
    {
        /// <summary>
        /// 当收到数据时
        /// </summary>
        public UdpReceivedEventHandler<UdpSession> Received { get; set; }

        /// <inheritdoc/>
        protected override async Task ReceivedData(UdpReceivedDataEventArgs e)
        {
            if (this.Received != null)
            {
                await this.Received.Invoke(this, e);
                if (e.Handled)
                {
                    return;
                }
            }
            await base.ReceivedData(e);
        }
    }

    /// <summary>
    /// UDP基类服务器。
    /// </summary>
    public class UdpSessionBase : ServiceBase, IUdpSession, IPluginObject
    {
        private readonly ConcurrentList<SocketAsyncEventArgs> m_socketAsyncs;
        private ServerState m_serverState;

        /// <summary>
        /// 构造函数
        /// </summary>
        public UdpSessionBase()
        {
            this.m_socketAsyncs = new ConcurrentList<SocketAsyncEventArgs>();
            this.Protocol = Protocol.Udp;
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.Monitor = new UdpNetworkMonitor(null, socket);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool CanSend => this.m_serverState == ServerState.Running;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool CanSetDataHandlingAdapter => true;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DateTime LastReceivedTime { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DateTime LastSendTime { get; private set; }

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        public UdpDataHandlingAdapter DataHandlingAdapter { get; private set; }

        /// <summary>
        /// 监听器
        /// </summary>
        public UdpNetworkMonitor Monitor { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual Protocol Protocol { get; set; }

        /// <summary>
        /// 默认远程节点
        /// </summary>
        public IPHost RemoteIPHost { get; private set; }

        /// <summary>
        /// 服务器名称
        /// </summary>
        public override string ServerName => this.Config?.GetValue(TouchSocketConfigExtension.ServerNameProperty);

        /// <summary>
        /// 获取服务器状态
        /// </summary>
        public override ServerState ServerState => this.m_serverState;

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

            if (this.Monitor.Socket.AddressFamily == AddressFamily.InterNetwork)
            {
                var optionValue = new MulticastOption(multicastAddr);
                this.Monitor.Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, optionValue);
            }
            else
            {
                var optionValue2 = new IPv6MulticastOption(multicastAddr);
                this.Monitor.Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.DropMembership, optionValue2);
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

            if (this.Monitor.Socket.AddressFamily == AddressFamily.InterNetwork)
            {
                var optionValue = new MulticastOption(multicastAddr);
                this.Monitor.Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, optionValue);
            }
            else
            {
                var optionValue2 = new IPv6MulticastOption(multicastAddr);
                this.Monitor.Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, optionValue2);
            }
        }

        /// <summary>
        /// 设置数据处理适配器
        /// </summary>
        /// <param name="adapter"></param>
        public virtual void SetDataHandlingAdapter(UdpDataHandlingAdapter adapter)
        {
            if (!this.CanSetDataHandlingAdapter)
            {
                throw new Exception($"不允许自由调用{nameof(SetDataHandlingAdapter)}进行赋值。");
            }

            this.SetAdapter(adapter);
        }

        /// <inheritdoc/>
        public override void Start()
        {
            try
            {
                if (this.m_serverState == ServerState.Disposed)
                {
                    throw new Exception("无法重新利用已释放对象");
                }

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
                    case ServerState.Disposed:
                        {
                            throw new Exception("无法再次利用已释放对象");
                        }
                }

                this.m_serverState = ServerState.Running;

                this.PluginManager.Raise(nameof(IServerStartedPlugin.OnServerStarted), this, new ServiceStateEventArgs(this.m_serverState, default));
                return;
            }
            catch (Exception ex)
            {
                this.m_serverState = ServerState.Exception;
                this.PluginManager.Raise(nameof(IServerStartedPlugin.OnServerStarted), this, new ServiceStateEventArgs(this.m_serverState, ex) { Message = ex.Message });
                throw;
            }
        }

        /// <inheritdoc/>
        public override async Task StartAsync()
        {
            try
            {
                if (this.m_serverState == ServerState.Disposed)
                {
                    throw new Exception("无法重新利用已释放对象");
                }

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
                    case ServerState.Disposed:
                        {
                            throw new Exception("无法再次利用已释放对象");
                        }
                }

                this.m_serverState = ServerState.Running;

                await this.PluginManager.RaiseAsync(nameof(IServerStartedPlugin.OnServerStarted), this, new ServiceStateEventArgs(this.m_serverState, default)).ConfigureFalseAwait();
                return;
            }
            catch (Exception ex)
            {
                this.m_serverState = ServerState.Exception;
                await this.PluginManager.RaiseAsync(nameof(IServerStartedPlugin.OnServerStarted), this, new ServiceStateEventArgs(this.m_serverState, ex) { Message = ex.Message }).ConfigureFalseAwait();
                throw;
            }
        }

        /// <inheritdoc/>
        public override void Stop()
        {
            this.Monitor?.Socket.Dispose();
            this.Monitor = null;
            this.m_serverState = ServerState.Stopped;
            foreach (var item in this.m_socketAsyncs)
            {
                item.SafeDispose();
            }
            this.m_socketAsyncs.Clear();

            this.PluginManager.Raise(nameof(IServerStartedPlugin.OnServerStarted), this, new ServiceStateEventArgs(this.m_serverState, default));
        }

        /// <inheritdoc/>
        public override async Task StopAsync()
        {
            this.Monitor?.Socket.Dispose();
            this.Monitor = null;
            this.m_serverState = ServerState.Stopped;
            foreach (var item in this.m_socketAsyncs)
            {
                item.SafeDispose();
            }
            this.m_socketAsyncs.Clear();

            await this.PluginManager.RaiseAsync(nameof(IServerStartedPlugin.OnServerStarted), this, new ServiceStateEventArgs(this.m_serverState, default)).ConfigureFalseAwait();
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
                    this.Monitor?.Socket.Dispose();
                    this.Monitor = null;
                    this.m_serverState = ServerState.Disposed;
                    foreach (var item in this.m_socketAsyncs)
                    {
                        item.SafeDispose();
                    }
                    this.m_socketAsyncs.Clear();

                    this.PluginManager.Raise(nameof(IServerStartedPlugin.OnServerStarted), this, new ServiceStateEventArgs(this.m_serverState, default));
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 处理已接收到的数据。
        /// </summary>
        protected virtual Task ReceivedData(UdpReceivedDataEventArgs e)
        {
            return this.PluginManager.RaiseAsync(nameof(IUdpReceivedPlugin.OnUdpReceived), this, e);
        }

        /// <summary>
        /// 当即将发送时，如果覆盖父类方法，则不会触发插件。
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <returns>返回值表示是否允许发送</returns>
        protected virtual bool HandleSendingData(EndPoint endPoint, byte[] buffer, int offset, int length)
        {
            return true;
        }

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            this.RemoteIPHost = config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
            if (this.CanSetDataHandlingAdapter)
            {
                this.SetDataHandlingAdapter(this.Config.GetValue(TouchSocketConfigExtension.UdpDataHandlingAdapterProperty).Invoke());
            }
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
        /// 设置适配器，该方法不会检验<see cref="CanSetDataHandlingAdapter"/>的值。
        /// </summary>
        /// <param name="adapter"></param>
        protected void SetAdapter(UdpDataHandlingAdapter adapter)
        {
            this.ThrowIfDisposed();
            if (adapter is null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            if (adapter.Owner != null)
            {
                throw new Exception("此适配器已被其他终端使用，请重新创建对象。");
            }


            if (this.Config != null)
            {
                adapter.Config(this.Config);
            }
            adapter.Logger = this.Logger;
            adapter.OnLoaded(this);
            adapter.ReceivedCallBack = this.PrivateHandleReceivedData;
            adapter.SendCallBack = this.DefaultSend;
            adapter.SendCallBackAsync = this.DefaultSendAsync;
            this.DataHandlingAdapter = adapter;
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
            const int SIP_UDP_CONNRESET = -1744830452;
            socket.IOControl(SIP_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                const int SIP_UDP_CONNRESET = -1744830452;
                socket.IOControl(SIP_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
            }
#endif

            #endregion Windows下UDP连接被重置错误10054

            this.PreviewBind(socket);

            socket.Bind(iPHost.EndPoint);

            this.Monitor = new UdpNetworkMonitor(iPHost, socket);

#if NET45_OR_GREATER || NET6_0_OR_GREATER
            for (var i = 0; i < threadCount; i++)
            {
                var eventArg = new SocketAsyncEventArgs();
                this.m_socketAsyncs.Add(eventArg);
                eventArg.Completed += this.IO_Completed;
                var byteBlock = new ByteBlock(1024 * 64);
                eventArg.UserToken = byteBlock;
                eventArg.SetBuffer(byteBlock.Buffer, 0, byteBlock.Capacity);
                eventArg.RemoteEndPoint = iPHost.EndPoint;
                if (!socket.ReceiveFromAsync(eventArg))
                {
                    this.ProcessReceive(socket, eventArg);
                }
            }
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                for (var i = 0; i < threadCount; i++)
                {
                    var eventArg = new SocketAsyncEventArgs();
                    this.m_socketAsyncs.Add(eventArg);
                    eventArg.Completed += this.IO_Completed;
                    var byteBlock = new ByteBlock(1024 * 64);
                    eventArg.UserToken = byteBlock;
                    eventArg.SetBuffer(byteBlock.Buffer, 0, byteBlock.Capacity);
                    eventArg.RemoteEndPoint = iPHost.EndPoint;
                    if (!socket.ReceiveFromAsync(eventArg))
                    {
                        this.ProcessReceive(socket, eventArg);
                    }
                }
            }
            else
            {
                var thread = new Thread(this.Received);
                thread.IsBackground = true;
                thread.Start();
            }
#endif
        }

        private void Received()
        {
            while (true)
            {
                var byteBlock = new ByteBlock();
                try
                {
                    var endPoint = this.Monitor.IPHost.EndPoint;
                    var r = this.Monitor.Socket.ReceiveFrom(byteBlock.Buffer, ref endPoint);
                    byteBlock.SetLength(r);
                    this.HandleBuffer(endPoint, byteBlock);
                }
                catch (Exception ex)
                {
                    byteBlock.Dispose();
                    this.Logger.Log(LogLevel.Error, this, ex.Message, ex);
                    break;
                }
            }
        }

        private void HandleBuffer(EndPoint endPoint, ByteBlock byteBlock)
        {
            try
            {
                this.LastReceivedTime = DateTime.Now;
                if (this.DisposedValue)
                {
                    return;
                }
                if (this.DataHandlingAdapter == null)
                {
                    this.Logger.Error(this, TouchSocketResource.NullDataAdapter.GetDescription());
                    return;
                }
                this.DataHandlingAdapter.ReceivedInput(endPoint, byteBlock);
            }
            catch (Exception ex)
            {
                this.Logger.Exception(this, "在处理数据时发生错误", ex);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            this.ProcessReceive((Socket)sender, e);
        }

        private void PrivateHandleReceivedData(EndPoint remoteEndPoint, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.m_receiver != null)
            {
                if (this.m_receiver.TryInputReceive(byteBlock, requestInfo))
                {
                    return;
                }
            }
            this.ReceivedData(new UdpReceivedDataEventArgs(remoteEndPoint, byteBlock, requestInfo));
        }

        #region Throw

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThorwIfRemoteIPHostNull()
        {
            if (this.RemoteIPHost == null)
            {
                throw new Exception("默认终结点为空");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThorwIfDataHandlingAdapterNull()
        {
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }
        }
        #endregion

        #region 向默认远程同步发送

        /// <summary>
        /// 向默认终结点发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public virtual void Send(byte[] buffer, int offset, int length)
        {
            this.ThorwIfRemoteIPHostNull();
            this.Send(this.RemoteIPHost.EndPoint, buffer, offset, length);
        }

        /// <summary>
        /// 向默认终结点发送
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual void Send(IRequestInfo requestInfo)
        {
            this.ThorwIfRemoteIPHostNull();
            this.Send(this.RemoteIPHost.EndPoint, requestInfo);
        }

        #endregion 向默认远程同步发送

        #region 向默认远程异步发送

        /// <inheritdoc/>
        public virtual Task SendAsync(byte[] buffer, int offset, int length)
        {
            this.ThorwIfRemoteIPHostNull();
            return this.SendAsync(this.RemoteIPHost.EndPoint, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual Task SendAsync(IRequestInfo requestInfo)
        {
            this.ThorwIfRemoteIPHostNull();
            return this.SendAsync(this.RemoteIPHost.EndPoint, requestInfo);
        }

        #endregion 向默认远程异步发送

        #region 向设置的远程同步发送

        /// <summary>
        /// 向设置的远程同步发送
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual void Send(EndPoint remoteEP, byte[] buffer, int offset, int length)
        {
            this.DataHandlingAdapter.SendInput(remoteEP, buffer, offset, length);
        }

        /// <summary>
        /// 向设置终结点发送
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="requestInfo"></param>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual void Send(EndPoint endPoint, IRequestInfo requestInfo)
        {
            ThorwIfDataHandlingAdapterNull();
            if (!this.DataHandlingAdapter.CanSendRequestInfo)
            {
                throw new NotSupportedException($"当前适配器不支持对象发送。");
            }
            this.DataHandlingAdapter.SendInput(endPoint, requestInfo);
        }

        #endregion 向设置的远程同步发送

        #region 向设置的远程异步发送

        /// <summary>
        /// 向设置的远程异步发送
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual Task SendAsync(EndPoint endPoint, byte[] buffer, int offset, int length)
        {
            ThorwIfDataHandlingAdapterNull();
            return this.DataHandlingAdapter.SendInputAsync(endPoint, buffer, offset, length);
        }

        public virtual Task SendAsync(EndPoint endPoint, IRequestInfo requestInfo)
        {
            ThorwIfDataHandlingAdapterNull();
            return this.DataHandlingAdapter.SendInputAsync(this.RemoteIPHost.EndPoint, requestInfo);
        }
        #endregion 向设置的远程异步发送

        private void ProcessReceive(Socket socket, SocketAsyncEventArgs e)
        {
            if (this.m_serverState != ServerState.Running)
            {
                e.SafeDispose();
                return;
            }
            if (e.SocketError == SocketError.Success)
            {
                var byteBlock = (ByteBlock)e.UserToken;
                byteBlock.SetLength(e.BytesTransferred);

                this.HandleBuffer(e.RemoteEndPoint, byteBlock);

                var newByteBlock = new ByteBlock(1024 * 64);
                e.UserToken = newByteBlock;
                e.SetBuffer(newByteBlock.Buffer, 0, newByteBlock.Buffer.Length);

                try
                {
                    if (!socket.ReceiveFromAsync(e))
                    {
                        this.ProcessReceive(socket, e);
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.Log(LogLevel.Error, this, ex.Message, ex);
                    e.SafeDispose();
                }
            }
            else
            {
                this.Logger.Error(this, $"接收出现错误：{e.SocketError}，错误代码：{(int)e.SocketError}");
                e.SafeDispose();
            }
        }

        #region DefaultSend

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void DefaultSend(byte[] buffer, int offset, int length)
        {
            this.ThorwIfRemoteIPHostNull();
            this.DefaultSend(this.RemoteIPHost.EndPoint, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void DefaultSend(EndPoint endPoint, byte[] buffer, int offset, int length)
        {
            ThrowIfDisposed();
            if (this.HandleSendingData(endPoint, buffer, offset, length))
            {
                this.Monitor.Socket.SendTo(buffer, offset, length, SocketFlags.None, endPoint);
                this.LastSendTime = DateTime.Now;
            }
        }

        #endregion DefaultSend

        #region DefaultSendAsync

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public Task DefaultSendAsync(byte[] buffer, int offset, int length)
        {
            this.ThorwIfRemoteIPHostNull();
            return this.DefaultSendAsync(this.RemoteIPHost.EndPoint, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
#if NET6_0_OR_GREATER
        public async Task DefaultSendAsync(EndPoint endPoint, byte[] buffer, int offset, int length)
        {
            ThrowIfDisposed();
            if (this.HandleSendingData(endPoint, buffer, offset, length))
            {
                if (this.CanSend)
                {
                    await this.Monitor.Socket.SendToAsync(new ReadOnlyMemory<byte>(buffer, offset, length), SocketFlags.None, endPoint);
                }

                this.LastSendTime = DateTime.Now;
            }
        }
#else
        public async Task DefaultSendAsync(EndPoint endPoint, byte[] buffer, int offset, int length)
        {
            ThrowIfDisposed();
            await Task.Run(() =>
            {
                this.DefaultSend(endPoint, buffer, offset, length);
            });
        }
#endif

        #endregion DefaultSendAsync

        #region 组合发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public void Send(IList<ArraySegment<byte>> transferBytes)
        {
            this.ThorwIfRemoteIPHostNull();
            this.Send(this.RemoteIPHost.EndPoint, transferBytes);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="transferBytes"></param>
        public void Send(EndPoint endPoint, IList<ArraySegment<byte>> transferBytes)
        {
            ThorwIfDataHandlingAdapterNull();

            if (!this.DataHandlingAdapter.CanSplicingSend)
            {
                throw new NotSupportedException("该适配器不支持拼接发送");
            }
            this.DataHandlingAdapter.SendInput(endPoint, transferBytes);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public Task SendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            this.ThorwIfRemoteIPHostNull();
            return this.SendAsync(this.RemoteIPHost.EndPoint, transferBytes);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="transferBytes"></param>
        public Task SendAsync(EndPoint endPoint, IList<ArraySegment<byte>> transferBytes)
        {
            ThorwIfDataHandlingAdapterNull();
            return this.DataHandlingAdapter.SendInputAsync(endPoint, transferBytes);
        }

        #endregion 组合发送

        #region Receiver

        private Receiver m_receiver;

        /// <inheritdoc/>
        public IReceiver CreateReceiver()
        {
            return this.m_receiver ??= new Receiver(this);
        }

        /// <inheritdoc/>
        public void ClearReceiver()
        {
            this.m_receiver = null;
        }

        #endregion Receiver
    }
}