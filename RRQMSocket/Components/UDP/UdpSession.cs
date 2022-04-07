//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore;
using RRQMCore.ByteManager;
using RRQMCore.Dependency;
using RRQMCore.Log;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace RRQMSocket
{
    /// <summary>
    /// 简单UDP会话。
    /// </summary>
    public class UdpSession : UdpSessionBase
    {
        /// <summary>
        /// 当收到数据时
        /// </summary>
        public event RRQMUDPByteBlockEventHandler Received;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected override void HandleReceivedData(EndPoint remoteEndPoint, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            Received?.Invoke(remoteEndPoint, byteBlock, requestInfo);
        }
    }

    /// <summary>
    /// UDP基类服务器。
    /// </summary>
    public abstract class UdpSessionBase : BaseSocket, IUdpSession, IPlguinObject
    {
        private RRQMConfig config;
        private UdpDataHandlingAdapter dataHandlingAdapter;
        private NetworkMonitor monitor;
        private IPHost remoteIPHost;
        private ServerState serverState;
        private bool usePlugin;

        /// <summary>
        /// 构造函数
        /// </summary>
        public UdpSessionBase()
        {
            this.Protocol = Protocol.UDP;
            this.Container = new Container();
            this.Container.RegisterTransient<ILog, ConsoleLogger>();
            this.Container.RegisterTransient<UdpDataHandlingAdapter, NormalUdpDataHandlingAdapter>();
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.ReceiveBufferSize = this.BufferLength;
            socket.SendBufferSize = this.BufferLength;
            this.monitor = new NetworkMonitor(null, socket);
            this.PluginsManager = new PluginsManager(this.Container);
            this.SetAdapter(new NormalUdpDataHandlingAdapter());
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool CanSend => this.serverState == ServerState.Running;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool CanSetDataHandlingAdapter => true;

        /// <summary>
        /// 获取配置
        /// </summary>
        public RRQMConfig Config => this.config;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IContainer Container { get; set; }

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        public UdpDataHandlingAdapter DataHandlingAdapter => this.dataHandlingAdapter;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int MaxPackageSize => this.config != null ? this.config.GetValue<int>(RRQMConfigExtensions.MaxPackageSizeProperty) : 0;

        /// <summary>
        /// 监听器
        /// </summary>
        public NetworkMonitor Monitor => this.monitor;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IPluginsManager PluginsManager { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual Protocol Protocol { get; set; }

        /// <summary>
        /// 默认远程节点
        /// </summary>
        public IPHost RemoteIPHost => this.remoteIPHost;

        /// <summary>
        /// 服务器名称
        /// </summary>
        public string ServerName => this.Config == null ? null : this.Config.GetValue<string>(RRQMConfigExtensions.ServerNameProperty);

        /// <summary>
        /// 获取服务器状态
        /// </summary>
        public ServerState ServerState => this.serverState;

        /// <summary>
        /// 是否已启用插件
        /// </summary>
        public bool UsePlugin => this.usePlugin;

        /// <summary>
        /// 退出组播
        /// </summary>
        /// <param name="multicastAddr"></param>
        public void DropMulticastGroup(IPAddress multicastAddr)
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (multicastAddr is null)
            {
                throw new ArgumentNullException(nameof(multicastAddr));
            }

            if (this.monitor.Socket.AddressFamily == AddressFamily.InterNetwork)
            {
                MulticastOption optionValue = new MulticastOption(multicastAddr);
                this.monitor.Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, optionValue);
            }
            else
            {
                IPv6MulticastOption optionValue2 = new IPv6MulticastOption(multicastAddr);
                this.monitor.Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.DropMembership, optionValue2);
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
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (this.monitor.Socket.AddressFamily == AddressFamily.InterNetwork)
            {
                MulticastOption optionValue = new MulticastOption(multicastAddr);
                this.monitor.Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, optionValue);
            }
            else
            {
                IPv6MulticastOption optionValue2 = new IPv6MulticastOption(multicastAddr);
                this.monitor.Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, optionValue2);
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
                throw new RRQMException($"不允许自由调用{nameof(SetDataHandlingAdapter)}进行赋值。");
            }

            this.SetAdapter(adapter);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="serverConfig"></param>
        /// <returns></returns>
        public IService Setup(RRQMConfig serverConfig)
        {
            this.config = serverConfig;
            this.LoadConfig(this.config);
            return this;
        }

        /// <summary>
        /// 通过端口配置
        /// </summary>
        /// <param name="port"></param>
        public IService Setup(int port)
        {
            RRQMConfig serverConfig = new RRQMConfig();
            serverConfig.SetBindIPHost(new IPHost(port));
            return this.Setup(serverConfig);
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public IService Start()
        {
            if (this.serverState == ServerState.Disposed)
            {
                throw new RRQMException("无法重新利用已释放对象");
            }

            switch (this.serverState)
            {
                case ServerState.None:
                    {
                        if (this.config.GetValue<IPHost>(RRQMConfigExtensions.BindIPHostProperty) is IPHost iPHost)
                        {
                            this.BeginReceive(iPHost);
                        }

                        break;
                    }
                case ServerState.Running:
                    break;

                case ServerState.Stopped:
                    {
                        if (this.config.GetValue<IPHost>(RRQMConfigExtensions.BindIPHostProperty) is IPHost iPHost)
                        {
                            this.BeginReceive(iPHost);
                        }
                        break;
                    }
                case ServerState.Disposed:
                    {
                        throw new RRQMException("无法再次利用已释放对象");
                    }
            }

            this.serverState = ServerState.Running;
            return this;
        }

        #region 插件

        /// <summary>
        /// 添加插件
        /// </summary>
        /// <typeparam name="TPlugin">插件类型</typeparam>
        /// <returns>插件类型实例</returns>
        public TPlugin AddPlugin<TPlugin>() where TPlugin : IPlugin
        {
            var plugin = this.Container.Resolve<TPlugin>();
            this.AddPlugin(plugin);
            return plugin;
        }

        /// <summary>
        /// 添加插件
        /// </summary>
        /// <param name="plugin">插件</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddPlugin(IPlugin plugin)
        {
            if (plugin.Logger == default)
            {
                plugin.Logger = this.Container.Resolve<ILog>();
            }
            this.PluginsManager.Add(plugin);
        }

        /// <summary>
        /// 清空插件
        /// </summary>
        public void ClearPlugins()
        {
            this.PluginsManager.Clear();
        }

        /// <summary>
        /// 移除插件
        /// </summary>
        /// <param name="plugin"></param>
        public void RemovePlugin(IPlugin plugin)
        {
            this.PluginsManager.Remove(plugin);
        }

        /// <summary>
        /// 移除插件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RemovePlugin<T>() where T : IPlugin
        {
            this.PluginsManager.Remove(typeof(T));
        }

        #endregion 插件

        /// <summary>
        /// 停止服务器
        /// </summary>
        public IService Stop()
        {
            if (this.monitor != null)
            {
                this.monitor.Socket.Dispose();
            }
            this.monitor = null;
            this.serverState = ServerState.Stopped;
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.Stop();
            this.serverState = ServerState.Disposed;
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
        protected virtual void LoadConfig(RRQMConfig config)
        {
            if (config == null)
            {
                throw new RRQMException("配置文件为空");
            }
            this.logger = this.Container.Resolve<ILog>();
            this.remoteIPHost = config.GetValue<IPHost>(RRQMConfigExtensions.RemoteIPHostProperty);
            this.BufferLength = config.BufferLength;
            this.usePlugin = config.IsUsePlugin;

            if (config.GetValue<int>(RRQMConfigExtensions.ThreadCountProperty) <= 0)
            {
                throw new RRQMException("线程数量必须大于0");
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
            if (adapter is null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            if (adapter.owner != null)
            {
                throw new RRQMException("此适配器已被其他终端使用，请重新创建对象。");
            }
            adapter.owner = this;
            adapter.ReceivedCallBack = this.PrivateHandleReceivedData;
            adapter.SendCallBack = this.SocketSend;
            this.dataHandlingAdapter = adapter;
        }

        /// <summary>
        /// 绕过适配器直接发送。<see cref="ByteBlock.Buffer"/>作为数据时，仅可同步发送。
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="isAsync">是否异步发送</param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        protected void SocketSend(EndPoint endPoint, byte[] buffer, int offset, int length, bool isAsync)
        {
            if (this.HandleSendingData(endPoint, buffer, offset, length))
            {
                if (this.CanSend)
                {
                    if (isAsync)
                    {
                        this.monitor.Socket.BeginSendTo(buffer, offset, length, SocketFlags.None, endPoint, null, null);
                    }
                    else
                    {
                        this.monitor.Socket.SendTo(buffer, offset, length, SocketFlags.None, endPoint);
                    }
                }
            }
        }

        private void BeginReceive(IPHost iPHost)
        {
            int threadCount = this.Config.GetValue<int>(RRQMConfigExtensions.ThreadCountProperty);
            Socket socket = new Socket(iPHost.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.ReceiveBufferSize = this.BufferLength;
            socket.SendBufferSize = this.BufferLength;
            if (threadCount > 1)
            {
                socket.UseOnlyOverlappedIO = true;
            }
            socket.EnableBroadcast = this.config.GetValue<bool>(RRQMConfigExtensions.EnableBroadcastProperty);
            this.PreviewBind(socket);
            socket.Bind(iPHost.EndPoint);

            this.monitor = new NetworkMonitor(iPHost, socket);

            switch (this.config.ReceiveType)
            {
                case ReceiveType.Auto:
                    {
                        for (int i = 0; i < threadCount; i++)
                        {
                            SocketAsyncEventArgs eventArg = new SocketAsyncEventArgs();
                            eventArg.Completed += this.IO_Completed;
                            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
                            eventArg.UserToken = byteBlock;
                            eventArg.SetBuffer(byteBlock.Buffer, 0, byteBlock.Capacity);
                            eventArg.RemoteEndPoint = iPHost.EndPoint;
                            if (!socket.ReceiveFromAsync(eventArg))
                            {
                                this.ProcessReceive(socket, eventArg);
                            }
                        }

                        break;
                    }
                default:
                    throw new RRQMException("UDP中只支持Auto模式");
            }
        }

        private void HandleBuffer(EndPoint endPoint, ByteBlock byteBlock)
        {
            try
            {
                this.dataHandlingAdapter.ReceivedInput(endPoint, byteBlock);
            }
            catch (Exception e)
            {
                this.Logger.Debug(LogType.Error, this, $"在处理数据时发生错误，信息：{e.Message}");
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
            if (this.usePlugin)
            {
                UdpReceivedDataEventArgs args = new UdpReceivedDataEventArgs(remoteEndPoint, byteBlock, requestInfo);
                this.PluginsManager.Raise<IUdpSessionPlugin>("OnReceivedData", this, args);
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
        public void Send(byte[] buffer, int offset, int length)
        {
            if (this.remoteIPHost == null)
            {
                throw new RRQMException("默认终结点为空");
            }
            this.Send(this.remoteIPHost.EndPoint, buffer, offset, length);
        }

        /// <summary>
        /// 向默认终结点发送
        /// </summary>
        /// <param name="buffer"></param>
        public void Send(byte[] buffer)
        {
            this.Send(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 向默认终结点发送
        /// </summary>
        /// <param name="byteBlock"></param>
        public void Send(ByteBlock byteBlock)
        {
            this.Send(byteBlock.Buffer, 0, byteBlock.Len);
        }

        #endregion 向默认远程同步发送

        #region 向默认远程异步发送

        /// <summary>
        /// IOCP发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void SendAsync(byte[] buffer, int offset, int length)
        {
            if (this.remoteIPHost == null)
            {
                throw new RRQMException("默认终结点为空");
            }
            this.SendAsync(this.remoteIPHost.EndPoint, buffer, offset, length);
        }

        /// <summary>
        /// IOCP发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void SendAsync(byte[] buffer)
        {
            this.SendAsync(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// IOCP发送流中的有效数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void SendAsync(ByteBlock byteBlock)
        {
            this.SendAsync(byteBlock.Buffer, 0, byteBlock.Len);
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
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public virtual void Send(EndPoint remoteEP, byte[] buffer, int offset, int length)
        {
            this.dataHandlingAdapter.SendInput(remoteEP, buffer, offset, length, false);
        }

        /// <summary>
        /// 向设置的远程同步发送
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <param name="buffer"></param>
        public void Send(EndPoint remoteEP, byte[] buffer)
        {
            this.Send(remoteEP, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 向设置的远程同步发送
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <param name="byteBlock"></param>
        public void Send(EndPoint remoteEP, ByteBlock byteBlock)
        {
            this.Send(remoteEP, byteBlock.Buffer, 0, byteBlock.Len);
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
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public virtual void SendAsync(EndPoint remoteEP, byte[] buffer, int offset, int length)
        {
            this.dataHandlingAdapter.SendInput(remoteEP, buffer, offset, length, true);
        }

        /// <summary>
        /// 向设置的远程异步发送
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <param name="buffer"></param>
        public void SendAsync(EndPoint remoteEP, byte[] buffer)
        {
            this.SendAsync(remoteEP, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 向设置的远程异步发送
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <param name="byteBlock"></param>
        public void SendAsync(EndPoint remoteEP, ByteBlock byteBlock)
        {
            this.SendAsync(remoteEP, byteBlock.Buffer, 0, byteBlock.Len);
        }

        #endregion 向设置的远程异步发送

        private void ProcessReceive(Socket socket, SocketAsyncEventArgs e)
        {
            if (this.serverState == ServerState.Running && e.SocketError == SocketError.Success)
            {
                ByteBlock byteBlock = (ByteBlock)e.UserToken;
                byteBlock.SetLength(e.BytesTransferred);

                this.HandleBuffer(e.RemoteEndPoint, byteBlock);

                ByteBlock newByteBlock = BytePool.GetByteBlock(this.BufferLength);
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
                    this.logger.Debug(LogType.Error, this, ex.Message, ex);
                }
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
            this.SocketSend(this.remoteIPHost.EndPoint, buffer, offset, length, false);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        public void DefaultSend(byte[] buffer)
        {
            this.DefaultSend(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        public void DefaultSend(ByteBlock byteBlock)
        {
            this.DefaultSend(byteBlock.Buffer, 0, byteBlock.Len);
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
            this.SocketSend(endPoint, buffer, offset, length, false);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="buffer"></param>
        public void DefaultSend(EndPoint endPoint, byte[] buffer)
        {
            this.DefaultSend(endPoint, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="byteBlock"></param>
        public void DefaultSend(EndPoint endPoint, ByteBlock byteBlock)
        {
            this.DefaultSend(endPoint, byteBlock.Buffer, 0, byteBlock.Len);
        }

        #endregion DefaultSend

        #region 组合发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public void Send(IList<TransferByte> transferBytes)
        {
            if (this.dataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), ResType.NullDataAdapter.GetResString());
            }

            if (this.dataHandlingAdapter.CanSplicingSend)
            {
                this.dataHandlingAdapter.SendInput(this.remoteIPHost.EndPoint, transferBytes, false);
            }
            else
            {
                ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
                try
                {
                    foreach (var item in transferBytes)
                    {
                        byteBlock.Write(item.Buffer, item.Offset, item.Length);
                    }
                    this.dataHandlingAdapter.SendInput(this.remoteIPHost.EndPoint, byteBlock.Buffer, 0, byteBlock.Len, false);
                }
                finally
                {
                    byteBlock.Dispose();
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="transferBytes"></param>
        public void Send(EndPoint endPoint, IList<TransferByte> transferBytes)
        {
            if (this.dataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), ResType.NullDataAdapter.GetResString());
            }

            if (this.dataHandlingAdapter.CanSplicingSend)
            {
                this.dataHandlingAdapter.SendInput(endPoint, transferBytes, false);
            }
            else
            {
                ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
                try
                {
                    foreach (var item in transferBytes)
                    {
                        byteBlock.Write(item.Buffer, item.Offset, item.Length);
                    }
                    this.dataHandlingAdapter.SendInput(endPoint, byteBlock.Buffer, 0, byteBlock.Len, false);
                }
                finally
                {
                    byteBlock.Dispose();
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public void SendAsync(IList<TransferByte> transferBytes)
        {
            if (this.dataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), ResType.NullDataAdapter.GetResString());
            }

            if (this.dataHandlingAdapter.CanSplicingSend)
            {
                this.dataHandlingAdapter.SendInput(this.remoteIPHost.EndPoint, transferBytes, true);
            }
            else
            {
                ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
                try
                {
                    foreach (var item in transferBytes)
                    {
                        byteBlock.Write(item.Buffer, item.Offset, item.Length);
                    }
                    this.dataHandlingAdapter.SendInput(this.remoteIPHost.EndPoint, byteBlock.Buffer, 0, byteBlock.Len, true);
                }
                finally
                {
                    byteBlock.Dispose();
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="transferBytes"></param>
        public void SendAsync(EndPoint endPoint, IList<TransferByte> transferBytes)
        {
            if (this.dataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), ResType.NullDataAdapter.GetResString());
            }

            if (this.dataHandlingAdapter.CanSplicingSend)
            {
                this.dataHandlingAdapter.SendInput(endPoint, transferBytes, true);
            }
            else
            {
                ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
                try
                {
                    foreach (var item in transferBytes)
                    {
                        byteBlock.Write(item.Buffer, item.Offset, item.Length);
                    }
                    this.dataHandlingAdapter.SendInput(endPoint, byteBlock.Buffer, 0, byteBlock.Len, true);
                }
                finally
                {
                    byteBlock.Dispose();
                }
            }
        }

        #endregion 组合发送
    }
}