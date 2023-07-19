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
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
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
        public UdpReceivedEventHandler Received { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected override void HandleReceivedData(EndPoint remoteEndPoint, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.Received?.Invoke(remoteEndPoint, byteBlock, requestInfo);
        }
    }

    /// <summary>
    /// UDP基类服务器。
    /// </summary>
    public class UdpSessionBase : BaseSocket, IUdpSession, IPluginObject
    {
        private readonly ConcurrentList<SocketAsyncEventArgs> m_socketAsyncs;

        /// <summary>
        /// 构造函数
        /// </summary>
        public UdpSessionBase()
        {
            this.m_socketAsyncs = new ConcurrentList<SocketAsyncEventArgs>();
            this.Protocol = Protocol.UDP;
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.Monitor = new UdpNetworkMonitor(null, socket);
        }

        /// <summary>
        /// 处理未经过适配器的数据。返回值表示是否继续向下传递。
        /// </summary>
        public Func<ByteBlock, bool> OnHandleRawBuffer { get; set; }

        /// <summary>
        /// 处理经过适配器后的数据。返回值表示是否继续向下传递。
        /// </summary>
        public Func<ByteBlock, IRequestInfo, bool> OnHandleReceivedData { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool CanSend => this.ServerState == ServerState.Running;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool CanSetDataHandlingAdapter => true;

        /// <summary>
        /// 获取配置
        /// </summary>
        public TouchSocketConfig Config { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IContainer Container { get; private set; }

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
        public IPluginsManager PluginsManager { get; private set; }

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
        public string ServerName => this.Config?.GetValue(TouchSocketConfigExtension.ServerNameProperty);

        /// <summary>
        /// 获取服务器状态
        /// </summary>
        public ServerState ServerState { get; private set; }

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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public IService Setup(TouchSocketConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            this.ThrowIfDisposed();

            this.BuildConfig(config);

            this.PluginsManager.Raise(nameof(ILoadingConfigPlugin.OnLoadingConfig), this, new ConfigEventArgs(config));
            this.LoadConfig(this.Config);
            this.PluginsManager.Raise(nameof(ILoadedConfigPlugin.OnLoadedConfig), this, new ConfigEventArgs(config));
            return this;
        }

        private void BuildConfig(TouchSocketConfig config)
        {
            this.Config = config;

            if (!(config.GetValue(TouchSocketCoreConfigExtension.ContainerProperty) is IContainer container))
            {
                container = new Container();
            }

            if (!container.IsRegistered(typeof(ILog)))
            {
                container.RegisterSingleton<ILog, LoggerGroup>();
            }

            if (!(config.GetValue(TouchSocketCoreConfigExtension.PluginsManagerProperty) is IPluginsManager pluginsManager))
            {
                pluginsManager = new PluginsManager(container);
            }

            if (container.IsRegistered(typeof(IPluginsManager)))
            {
                pluginsManager = container.Resolve<IPluginsManager>();
            }
            else
            {
                container.RegisterSingleton<IPluginsManager>(pluginsManager);
            }

            if (config.GetValue(TouchSocketCoreConfigExtension.ConfigureContainerProperty) is Action<IContainer> actionContainer)
            {
                actionContainer.Invoke(container);
            }

            if (config.GetValue(TouchSocketCoreConfigExtension.ConfigurePluginsProperty) is Action<IPluginsManager> actionPluginsManager)
            {
                pluginsManager.Enable = true;
                actionPluginsManager.Invoke(pluginsManager);
            }

            this.Container = container;
            this.PluginsManager = pluginsManager;
        }

        /// <summary>
        /// 通过端口配置
        /// </summary>
        /// <param name="port"></param>
        public IService Setup(int port)
        {
            var serverConfig = new TouchSocketConfig();
            serverConfig.SetBindIPHost(new IPHost(port));
            return this.Setup(serverConfig);
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public IService Start()
        {
            try
            {
                if (this.ServerState == ServerState.Disposed)
                {
                    throw new Exception("无法重新利用已释放对象");
                }

                switch (this.ServerState)
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
                        return this;

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

                this.ServerState = ServerState.Running;

                this.PluginsManager.Raise(nameof(IServerStartedPlugin.OnServerStarted), this, new ServiceStateEventArgs(this.ServerState, default));
                return this;
            }
            catch (Exception ex)
            {
                this.ServerState = ServerState.Exception;
                this.PluginsManager.Raise(nameof(IServerStartedPlugin.OnServerStarted), this, new ServiceStateEventArgs(this.ServerState, ex) { Message = ex.Message });
                throw;
            }

        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        public IService Stop()
        {
            this.Monitor?.Socket.Dispose();
            this.Monitor = null;
            this.ServerState = ServerState.Stopped;
            foreach (var item in this.m_socketAsyncs)
            {
                item.SafeDispose();
            }
            this.m_socketAsyncs.Clear();

            this.PluginsManager.Raise(nameof(IServerStartedPlugin.OnServerStarted), this, new ServiceStateEventArgs(this.ServerState, default));
            return this;
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
                    this.ServerState = ServerState.Disposed;
                    foreach (var item in this.m_socketAsyncs)
                    {
                        item.SafeDispose();
                    }
                    this.m_socketAsyncs.Clear();

                    this.PluginsManager.Raise(nameof(IServerStartedPlugin.OnServerStarted), this, new ServiceStateEventArgs(this.ServerState, default));
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 处理已接收到的数据。
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="byteBlock">以二进制流形式传递</param>
        /// <param name="requestInfo">以解析的数据对象传递</param>
        protected virtual void HandleReceivedData(EndPoint remoteEndPoint, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
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

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="config"></param>
        protected virtual void LoadConfig(TouchSocketConfig config)
        {
            this.Logger = this.Container.Resolve<ILog>();
            this.RemoteIPHost = config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
            if (config.GetValue(TouchSocketConfigExtension.BufferLengthProperty) is int value)
            {
                this.SetBufferLength(value);
            }

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
                if (this.Config.GetValue(TouchSocketConfigExtension.MaxPackageSizeProperty) is int v1)
                {
                    adapter.MaxPackageSize = v1;
                }
            }
            adapter.Logger = this.Logger;
            adapter.OnLoaded(this);
            adapter.ReceivedCallBack = this.PrivateHandleReceivedData;
            adapter.SendCallBack = this.DefaultSend;
            this.DataHandlingAdapter = adapter;
        }

        private void BeginReceive(IPHost iPHost)
        {
            var threadCount = this.Config.GetValue(TouchSocketConfigExtension.ThreadCountProperty);
            threadCount = threadCount < 0 ? 1 : threadCount;
            var socket = new Socket( SocketType.Dgram, ProtocolType.Udp)
            {
                ReceiveBufferSize = BufferLength,
                SendBufferSize = BufferLength,
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

            #endregion

            this.PreviewBind(socket);

            socket.Bind(iPHost.EndPoint);

            this.Monitor = new UdpNetworkMonitor(iPHost, socket);

            switch (this.Config.GetValue(TouchSocketConfigExtension.ReceiveTypeProperty))
            {
                case ReceiveType.Auto:
                    {
#if NET45_OR_GREATER||NET5_0_OR_GREATER
                        for (var i = 0; i < threadCount; i++)
                        {
                            var eventArg = new SocketAsyncEventArgs();
                            this.m_socketAsyncs.Add(eventArg);
                            eventArg.Completed += this.IO_Completed;
                            var byteBlock = new ByteBlock(this.BufferLength);
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
                            for (int i = 0; i < threadCount; i++)
                            {
                                SocketAsyncEventArgs eventArg = new SocketAsyncEventArgs();
                                m_socketAsyncs.Add(eventArg);
                                eventArg.Completed += IO_Completed;
                                ByteBlock byteBlock = new ByteBlock(BufferLength);
                                eventArg.UserToken = byteBlock;
                                eventArg.SetBuffer(byteBlock.Buffer, 0, byteBlock.Capacity);
                                eventArg.RemoteEndPoint = iPHost.EndPoint;
                                if (!socket.ReceiveFromAsync(eventArg))
                                {
                                    ProcessReceive(socket, eventArg);
                                }
                            }
                        }
                        else
                        {
                            Thread thread = new Thread(Received);
                            thread.IsBackground = true;
                            thread.Start();
                        }
#endif
                        break;
                    }
                default:
                    throw new Exception("UDP中只支持Auto模式");
            }
        }

        private void Received()
        {
            while (true)
            {
                var byteBlock = new ByteBlock();
                try
                {
                    EndPoint endPoint = this.Monitor.IPHost.EndPoint;
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
                if (this.OnHandleRawBuffer?.Invoke(byteBlock) == false)
                {
                    return;
                }
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
            if (this.OnHandleReceivedData?.Invoke(byteBlock, requestInfo) == false)
            {
                return;
            }

            if (this.PluginsManager.Enable)
            {
                var args = new UdpReceivedDataEventArgs(remoteEndPoint, byteBlock, requestInfo);
                this.PluginsManager.Raise(nameof(IUdpReceivedPlugin.OnUdpReceived), this, args);
                if (args.Handled)
                {
                    return;
                }
            }
            this.HandleReceivedData(remoteEndPoint, byteBlock, requestInfo);
        }

        #region 向默认远程同步发送

        /// <summary>
        /// 向默认终结点发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public virtual void Send(byte[] buffer, int offset, int length)
        {
            if (this.RemoteIPHost == null)
            {
                throw new Exception("默认终结点为空");
            }
            this.Send(this.RemoteIPHost.EndPoint, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual void Send(IRequestInfo requestInfo)
        {
            if (this.DisposedValue)
            {
                return;
            }
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }
            if (!this.DataHandlingAdapter.CanSendRequestInfo)
            {
                throw new NotSupportedException($"当前适配器不支持对象发送。");
            }
            this.DataHandlingAdapter.SendInput(requestInfo);
        }

        #endregion 向默认远程同步发送

        #region 向默认远程异步发送

        /// <summary>
        /// IOCP发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual Task SendAsync(byte[] buffer, int offset, int length)
        {
            return Task.Run(() =>
              {
                  this.Send(buffer, offset, length);
              });
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual Task SendAsync(IRequestInfo requestInfo)
        {
            return Task.Run(() =>
            {
                this.Send(requestInfo);
            });
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

        #endregion 向设置的远程同步发送

        #region 向设置的远程异步发送

        /// <summary>
        /// 向设置的远程异步发送
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual Task SendAsync(EndPoint remoteEP, byte[] buffer, int offset, int length)
        {
            return Task.Run(() =>
             {
                 this.Send(remoteEP, buffer, offset, length);
             });
        }

        #endregion 向设置的远程异步发送

        private void ProcessReceive(Socket socket, SocketAsyncEventArgs e)
        {
            if (this.ServerState != ServerState.Running)
            {
                e.SafeDispose();
                return;
            }
            if (e.SocketError == SocketError.Success)
            {
                var byteBlock = (ByteBlock)e.UserToken;
                byteBlock.SetLength(e.BytesTransferred);

                this.HandleBuffer(e.RemoteEndPoint, byteBlock);

                var newByteBlock = new ByteBlock(this.BufferLength);
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
            if (this.HandleSendingData(endPoint, buffer, offset, length))
            {
                if (this.CanSend)
                {
                    this.Monitor.Socket.SendTo(buffer, offset, length, SocketFlags.None, endPoint);
                }

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
            return Task.Run(() =>
            {
                this.DefaultSend(buffer, offset, length);
            });
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public Task DefaultSendAsync(EndPoint endPoint, byte[] buffer, int offset, int length)
        {
            return Task.Run(() =>
            {
                this.DefaultSend(buffer, offset, length);
            });
        }

        #endregion DefaultSendAsync

        #region 组合发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public void Send(IList<ArraySegment<byte>> transferBytes)
        {
            this.Send(this.RemoteIPHost.EndPoint, transferBytes);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="transferBytes"></param>
        public void Send(EndPoint endPoint, IList<ArraySegment<byte>> transferBytes)
        {
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }

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
            return Task.Run(() =>
            {
                this.Send(transferBytes);
            });
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="transferBytes"></param>
        public Task SendAsync(EndPoint endPoint, IList<ArraySegment<byte>> transferBytes)
        {
            return Task.Run(() =>
            {
                this.Send(endPoint, transferBytes);
            });
        }

        #endregion 组合发送
    }
}