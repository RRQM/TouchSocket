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
using System.Runtime.InteropServices;
using System.Threading;

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
        public event UdpReceivedEventHandler Received;

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
        private RRQMConfig m_config;
        private UdpDataHandlingAdapter m_adapter;
        private NetworkMonitor m_monitor;
        private IPHost m_remoteIPHost;
        private ServerState m_serverState;
        private bool m_usePlugin;

        /// <summary>
        /// 构造函数
        /// </summary>
        public UdpSessionBase()
        {
            this.Protocol = Protocol.UDP;
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.ReceiveBufferSize = this.BufferLength;
            socket.SendBufferSize = this.BufferLength;
            this.m_monitor = new NetworkMonitor(null, socket);
            this.SetAdapter(new NormalUdpDataHandlingAdapter());
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
        public bool CanSend => this.m_serverState == ServerState.Running;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool CanSetDataHandlingAdapter => true;

        /// <summary>
        /// 获取配置
        /// </summary>
        public RRQMConfig Config => this.m_config;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IContainer Container => this.m_config?.Container;

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        public UdpDataHandlingAdapter DataHandlingAdapter => this.m_adapter;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int MaxPackageSize => this.m_config != null ? this.m_config.GetValue<int>(RRQMConfigExtensions.MaxPackageSizeProperty) : 0;

        /// <summary>
        /// 监听器
        /// </summary>
        public NetworkMonitor Monitor => this.m_monitor;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IPluginsManager PluginsManager => this.m_config?.PluginsManager;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual Protocol Protocol { get; set; }

        /// <summary>
        /// 默认远程节点
        /// </summary>
        public IPHost RemoteIPHost => this.m_remoteIPHost;

        /// <summary>
        /// 服务器名称
        /// </summary>
        public string ServerName => this.Config == null ? null : this.Config.GetValue<string>(RRQMConfigExtensions.ServerNameProperty);

        /// <summary>
        /// 获取服务器状态
        /// </summary>
        public ServerState ServerState => this.m_serverState;

        /// <summary>
        /// 是否已启用插件
        /// </summary>
        public bool UsePlugin => this.m_usePlugin;

        /// <summary>
        /// 退出组播
        /// </summary>
        /// <param name="multicastAddr"></param>
        public void DropMulticastGroup(IPAddress multicastAddr)
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (multicastAddr is null)
            {
                throw new ArgumentNullException(nameof(multicastAddr));
            }

            if (this.m_monitor.Socket.AddressFamily == AddressFamily.InterNetwork)
            {
                MulticastOption optionValue = new MulticastOption(multicastAddr);
                this.m_monitor.Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, optionValue);
            }
            else
            {
                IPv6MulticastOption optionValue2 = new IPv6MulticastOption(multicastAddr);
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
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (this.m_monitor.Socket.AddressFamily == AddressFamily.InterNetwork)
            {
                MulticastOption optionValue = new MulticastOption(multicastAddr);
                this.m_monitor.Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, optionValue);
            }
            else
            {
                IPv6MulticastOption optionValue2 = new IPv6MulticastOption(multicastAddr);
                this.m_monitor.Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, optionValue2);
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
            this.m_config = serverConfig;
            this.LoadConfig(this.m_config);
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
            if (this.m_serverState == ServerState.Disposed)
            {
                throw new RRQMException("无法重新利用已释放对象");
            }

            switch (this.m_serverState)
            {
                case ServerState.None:
                    {
                        if (this.m_config.GetValue<IPHost>(RRQMConfigExtensions.BindIPHostProperty) is IPHost iPHost)
                        {
                            this.BeginReceive(iPHost);
                        }

                        break;
                    }
                case ServerState.Running:
                    break;

                case ServerState.Stopped:
                    {
                        if (this.m_config.GetValue<IPHost>(RRQMConfigExtensions.BindIPHostProperty) is IPHost iPHost)
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

            this.m_serverState = ServerState.Running;
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
            if (this.m_config == null)
            {
                throw new ArgumentNullException(nameof(this.Config), "请在SetUp后添加插件。");
            }
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
            if (this.m_config == null)
            {
                throw new ArgumentNullException(nameof(this.Config), "请在SetUp后添加插件。");
            }
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
            if (this.m_monitor != null)
            {
                this.m_monitor.Socket.Dispose();
            }
            this.m_monitor = null;
            this.m_serverState = ServerState.Stopped;
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.Stop();
            this.m_serverState = ServerState.Disposed;
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
            if (this.Logger == null)
            {
                this.Logger = this.Container.Resolve<ILog>();
            }
            this.m_remoteIPHost = config.GetValue<IPHost>(RRQMConfigExtensions.RemoteIPHostProperty);
            this.BufferLength = config.BufferLength;
            this.m_usePlugin = config.IsUsePlugin;

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
            this.m_adapter = adapter;
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
                        this.m_monitor.Socket.BeginSendTo(buffer, offset, length, SocketFlags.None, endPoint, null, null);
                    }
                    else
                    {
                        this.m_monitor.Socket.SendTo(buffer, offset, length, SocketFlags.None, endPoint);
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
            socket.EnableBroadcast = this.m_config.GetValue<bool>(RRQMConfigExtensions.EnableBroadcastProperty);
            this.PreviewBind(socket);
            socket.Bind(iPHost.EndPoint);

            this.m_monitor = new NetworkMonitor(iPHost, socket);

            switch (this.m_config.ReceiveType)
            {
                case ReceiveType.Auto:
                    {
#if NET45_OR_GREATER||NET5_0_OR_GREATER
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
#else
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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
                        }
                        else
                        {
                            Thread thread = new Thread(this.Received);
                            thread.IsBackground = true;
                            thread.Start();
                        }
#endif
                        break;
                    }
                default:
                    throw new RRQMException("UDP中只支持Auto模式");
            }
        }

        private void Received()
        {
            while (true)
            {
                try
                {
                    EndPoint endPoint = this.m_monitor.IPHost.EndPoint;
                    ByteBlock byteBlock = new ByteBlock();
                    int r = this.m_monitor.Socket.ReceiveFrom(byteBlock.Buffer, ref endPoint);
                    byteBlock.SetLength(r);
                    this.HandleBuffer(endPoint, byteBlock);
                }
                catch (Exception ex)
                {
                    this.Logger.Debug(LogType.Error, this, ex.Message, ex);
                    break;
                }
            }
        }

        private void HandleBuffer(EndPoint endPoint, ByteBlock byteBlock)
        {
            try
            {
                if (this.OnHandleRawBuffer?.Invoke(byteBlock) == false)
                {
                    return;
                }
                if (this.disposedValue)
                {
                    return;
                }
                if (this.m_adapter == null)
                {
                    this.Logger.Debug(LogType.Error, this, ResType.NullDataAdapter.GetDescription());
                    return;
                }
                this.m_adapter.ReceivedInput(endPoint, byteBlock);
            }
            catch (Exception ex)
            {
                this.Logger.Debug(LogType.Error, this, "在处理数据时发生错误", ex);
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

            if (this.m_usePlugin)
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
            if (this.m_remoteIPHost == null)
            {
                throw new RRQMException("默认终结点为空");
            }
            this.Send(this.m_remoteIPHost.EndPoint, buffer, offset, length);
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
            if (this.m_remoteIPHost == null)
            {
                throw new RRQMException("默认终结点为空");
            }
            this.SendAsync(this.m_remoteIPHost.EndPoint, buffer, offset, length);
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
            this.m_adapter.SendInput(remoteEP, buffer, offset, length, false);
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
            this.m_adapter.SendInput(remoteEP, buffer, offset, length, true);
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
            if (this.m_serverState == ServerState.Running && e.SocketError == SocketError.Success)
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
                    this.Logger.Debug(LogType.Error, this, ex.Message, ex);
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
            this.SocketSend(this.m_remoteIPHost.EndPoint, buffer, offset, length, false);
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

        #region DefaultSendAsync

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void DefaultSendAsync(byte[] buffer, int offset, int length)
        {
            this.SocketSend(this.m_remoteIPHost.EndPoint, buffer, offset, length, true);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        public void DefaultSendAsync(byte[] buffer)
        {
            this.DefaultSendAsync(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        public void DefaultSendAsync(ByteBlock byteBlock)
        {
            this.DefaultSendAsync(byteBlock.Buffer, 0, byteBlock.Len);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void DefaultSendAsync(EndPoint endPoint, byte[] buffer, int offset, int length)
        {
            this.SocketSend(endPoint, buffer, offset, length, true);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="buffer"></param>
        public void DefaultSendAsync(EndPoint endPoint, byte[] buffer)
        {
            this.DefaultSendAsync(endPoint, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="byteBlock"></param>
        public void DefaultSendAsync(EndPoint endPoint, ByteBlock byteBlock)
        {
            this.DefaultSendAsync(endPoint, byteBlock.Buffer, 0, byteBlock.Len);
        }

        #endregion DefaultSendAsync

        #region 组合发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public void Send(IList<TransferByte> transferBytes)
        {
            if (this.m_adapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), ResType.NullDataAdapter.GetDescription());
            }

            if (this.m_adapter.CanSplicingSend)
            {
                this.m_adapter.SendInput(this.m_remoteIPHost.EndPoint, transferBytes, false);
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
                    this.m_adapter.SendInput(this.m_remoteIPHost.EndPoint, byteBlock.Buffer, 0, byteBlock.Len, false);
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
            if (this.m_adapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), ResType.NullDataAdapter.GetDescription());
            }

            if (this.m_adapter.CanSplicingSend)
            {
                this.m_adapter.SendInput(endPoint, transferBytes, false);
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
                    this.m_adapter.SendInput(endPoint, byteBlock.Buffer, 0, byteBlock.Len, false);
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
            if (this.m_adapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), ResType.NullDataAdapter.GetDescription());
            }

            if (this.m_adapter.CanSplicingSend)
            {
                this.m_adapter.SendInput(this.m_remoteIPHost.EndPoint, transferBytes, true);
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
                    this.m_adapter.SendInput(this.m_remoteIPHost.EndPoint, byteBlock.Buffer, 0, byteBlock.Len, true);
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
            if (this.m_adapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), ResType.NullDataAdapter.GetDescription());
            }

            if (this.m_adapter.CanSplicingSend)
            {
                this.m_adapter.SendInput(endPoint, transferBytes, true);
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
                    this.m_adapter.SendInput(endPoint, byteBlock.Buffer, 0, byteBlock.Len, true);
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