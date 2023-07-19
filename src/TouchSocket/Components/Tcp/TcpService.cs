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
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// TCP泛型服务器，由使用者自己指定<see cref="SocketClient"/>类型。
    /// </summary>
    public class TcpService<TClient> : TcpServiceBase, ITcpService<TClient> where TClient : SocketClient, new()
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpService()
        {
            this.m_socketClients = new SocketClientCollection();
            this.m_getDefaultNewId = () =>
            {
                return Interlocked.Increment(ref this.m_nextId).ToString();
            };
        }

        #region 变量

        private readonly List<TcpNetworkMonitor> m_monitors = new List<TcpNetworkMonitor>();
        private readonly SocketClientCollection m_socketClients;
        private TouchSocketConfig m_config;
        private IContainer m_container;
        private Func<string> m_getDefaultNewId;
        private int m_maxCount;
        private long m_nextId;
        private IPluginsManager m_pluginsManager;
        private ServerState m_serverState;

        #endregion 变量

        #region 属性

        /// <summary>
        /// 获取服务器配置
        /// </summary>
        public override TouchSocketConfig Config => this.m_config;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override IContainer Container => this.m_container;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override Func<string> GetDefaultNewId => this.m_getDefaultNewId;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override int MaxCount => this.m_maxCount;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override IEnumerable<TcpNetworkMonitor> Monitors => this.m_monitors.ToArray();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override IPluginsManager PluginsManager => this.m_pluginsManager;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string ServerName => this.Config?.GetValue(TouchSocketConfigExtension.ServerNameProperty);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override ServerState ServerState => this.m_serverState;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override ISocketClientCollection SocketClients => this.m_socketClients;

        #endregion 属性

        #region 事件

        /// <summary>
        /// 用户连接完成
        /// </summary>
        public ConnectedEventHandler<TClient> Connected { get; set; }

        /// <summary>
        /// 有用户连接的时候
        /// </summary>
        public ConnectingEventHandler<TClient> Connecting { get; set; }

        /// <summary>
        /// 有用户断开连接
        /// </summary>
        public DisconnectEventHandler<TClient> Disconnected { get; set; }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// <para>
        /// 当主动调用Close断开时，可通过<see cref="MsgPermitEventArgs.IsPermitOperation"/>终止断开行为。
        /// </para>
        /// </summary>
        public DisconnectEventHandler<TClient> Disconnecting { get; set; }

        /// <summary>
        /// 当客户端Id被修改时触发。
        /// </summary>
        public IdChangedEventHandler<TClient> IdChanged { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override sealed void OnClientConnected(ISocketClient socketClient, ConnectedEventArgs e)
        {
            this.OnConnected((TClient)socketClient, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override sealed void OnClientConnecting(ISocketClient socketClient, ConnectingEventArgs e)
        {
            this.OnConnecting((TClient)socketClient, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override sealed void OnClientDisconnected(ISocketClient socketClient, DisconnectEventArgs e)
        {
            this.OnDisconnected((TClient)socketClient, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override sealed void OnClientDisconnecting(ISocketClient socketClient, DisconnectEventArgs e)
        {
            this.OnDisconnecting((TClient)socketClient, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected override sealed void OnClientReceivedData(ISocketClient socketClient, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.OnReceived((TClient)socketClient, byteBlock, requestInfo);
        }

        /// <summary>
        /// 客户端连接完成，覆盖父类方法将不会触发事件。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual void OnConnected(TClient socketClient, ConnectedEventArgs e)
        {
            this.Connected?.Invoke(socketClient, e);
        }

        /// <summary>
        /// 客户端请求连接，覆盖父类方法将不会触发事件。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual void OnConnecting(TClient socketClient, ConnectingEventArgs e)
        {
            this.Connecting?.Invoke(socketClient, e);
        }

        /// <summary>
        /// 客户端断开连接，覆盖父类方法将不会触发事件。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(TClient socketClient, DisconnectEventArgs e)
        {
            this.Disconnected?.Invoke(socketClient, e);
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// <para>
        /// 当主动调用Close断开时，可通过<see cref="MsgPermitEventArgs.IsPermitOperation"/>终止断开行为。
        /// </para>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual void OnDisconnecting(TClient socketClient, DisconnectEventArgs e)
        {
            this.Disconnecting?.Invoke(socketClient, e);
        }

        /// <summary>
        /// 当收到适配器数据。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected virtual void OnReceived(TClient socketClient, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
        }

        #endregion 事件

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public override void AddListen(ListenOption option)
        {
            if (option is null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            if (this.DisposedValue)
            {
                throw new ObjectDisposedException(nameof(TcpService));
            }

            if (option.IpHost is null)
            {
                throw new ArgumentNullException(nameof(option.IpHost));
            }

            var socket = new Socket(option.IpHost.EndPoint.AddressFamily,SocketType.Stream, ProtocolType.Tcp);

            if (option.ReuseAddress)
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }

            var e = new SocketAsyncEventArgs();

            var networkMonitor = new TcpNetworkMonitor(option, socket, e);

            this.PreviewBind(networkMonitor);
            socket.Bind(option.IpHost.EndPoint);
            socket.Listen(option.Backlog);

            e.UserToken = networkMonitor;
            e.Completed += this.Args_Completed;
            if (!networkMonitor.Socket.AcceptAsync(e))
            {
                this.OnAccepted(e);
            }
            this.m_monitors.Add(networkMonitor);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Clear()
        {
            foreach (var item in this.GetIds())
            {
                if (this.TryGetSocketClient(item, out var client))
                {
                    client.SafeDispose();
                }
            }
        }

        /// <summary>
        /// 获取当前在线的所有客户端
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TClient> GetClients()
        {
            return this.m_socketClients.GetClients()
                  .Select(a => (TClient)a);
        }

        /// <inheritdoc/>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public override bool RemoveListen(TcpNetworkMonitor monitor)
        {
            if (monitor is null)
            {
                throw new ArgumentNullException(nameof(monitor));
            }

            if (this.m_monitors.Remove(monitor))
            {
                monitor.SocketAsyncEvent.SafeDispose();
                monitor.Socket.SafeDispose();
                return true;
            }
            return false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="oldId"></param>
        /// <param name="newId"></param>
        /// <exception cref="ClientNotFindException"></exception>
        /// <exception cref="Exception"></exception>
        public override void ResetId(string oldId, string newId)
        {
            if (string.IsNullOrEmpty(oldId))
            {
                throw new ArgumentException($"“{nameof(oldId)}”不能为 null 或空。", nameof(oldId));
            }

            if (string.IsNullOrEmpty(newId))
            {
                throw new ArgumentException($"“{nameof(newId)}”不能为 null 或空。", nameof(newId));
            }

            if (oldId == newId)
            {
                return;
            }
            if (this.m_socketClients.TryGetSocketClient(oldId, out TClient socketClient))
            {
                socketClient.ResetId(newId);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketResource.ClientNotFind.GetDescription(oldId));
            }
        }

        /// <inheritdoc/>
        public override IService Setup(TouchSocketConfig config)
        {
           
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            this.ThrowIfDisposed();

            this.BuildConfig(config);

            this.PluginsManager.Raise(nameof(ILoadingConfigPlugin.OnLoadingConfig), this, new ConfigEventArgs(config));
            this.LoadConfig(this.m_config);
            this.PluginsManager.Raise(nameof(ILoadedConfigPlugin.OnLoadedConfig), this, new ConfigEventArgs(config));

            this.Logger ??= this.m_container.Resolve<ILog>();
            this.Container.RegisterTransient<TClient>();
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="port"></param>
        public override IService Setup(int port)
        {
            var serviceConfig = new TouchSocketConfig();
            serviceConfig.SetListenIPHosts(new IPHost[] { new IPHost(port) });
            return this.Setup(serviceConfig);
        }

        /// <summary>
        ///<inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override bool SocketClientExist(string id)
        {
            return this.SocketClients.SocketClientExist(id);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="System.Exception"></exception>
        public override IService Start()
        {
            if (this.m_config is null)
            {
                throw new ArgumentNullException(nameof(this.Config), "Config为null，请先执行Setup");
            }
            try
            {
                var optionList = new List<ListenOption>();
                if (this.Config.GetValue(TouchSocketConfigExtension.ListenOptionsProperty) is Action<List<ListenOption>> action)
                {
                    action.Invoke(optionList);
                }

                var iPHosts = this.Config.GetValue(TouchSocketConfigExtension.ListenIPHostsProperty);
                if (iPHosts != null)
                {
                    foreach (var item in iPHosts)
                    {
                        var option = new ListenOption
                        {
                            IpHost = item,
                            BufferLength = this.BufferLength,
                            ServiceSslOption = this.Config.GetValue(TouchSocketConfigExtension.SslOptionProperty) as ServiceSslOption,
                            ReuseAddress = this.Config.GetValue(TouchSocketConfigExtension.ReuseAddressProperty),
                            NoDelay = this.Config.GetValue(TouchSocketConfigExtension.NoDelayProperty),
                            TcpAdapter = this.Config.GetValue(TouchSocketConfigExtension.TcpDataHandlingAdapterProperty),
                            ReceiveType = this.Config.GetValue(TouchSocketConfigExtension.ReceiveTypeProperty)
                        };
                        option.Backlog = this.Config.GetValue(TouchSocketConfigExtension.BacklogProperty) ?? option.Backlog;
                        option.SendTimeout = this.Config.GetValue(TouchSocketConfigExtension.SendTimeoutProperty);

                        optionList.Add(option);
                    }
                }

                if (optionList.Count == 0)
                {
                    return this;
                }
                switch (this.m_serverState)
                {
                    case ServerState.None:
                        {
                            this.BeginListen(optionList);
                            break;
                        }
                    case ServerState.Running:
                        {
                            return this;
                        }
                    case ServerState.Stopped:
                        {
                            this.BeginListen(optionList);
                            break;
                        }
                    case ServerState.Disposed:
                        {
                            throw new ObjectDisposedException(this.GetType().Name);
                        }
                }
                this.m_serverState = ServerState.Running;

                this.PluginsManager.Raise(nameof(IServerStartedPlugin.OnServerStarted), this, new ServiceStateEventArgs(this.m_serverState, default));
                return this;
            }
            catch (Exception ex)
            {
                this.m_serverState = ServerState.Exception;

                this.PluginsManager.Raise(nameof(IServerStartedPlugin.OnServerStarted), this, new ServiceStateEventArgs(this.m_serverState, ex) { Message = ex.Message });
                throw;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override IService Stop()
        {
            foreach (var item in this.m_monitors)
            {
                item.Socket.SafeDispose();
                item.SocketAsyncEvent.SafeDispose();
            }

            this.m_monitors.Clear();

            this.Clear();

            this.m_serverState = ServerState.Stopped;
            if (this.PluginsManager.Enable)
            {
                this.PluginsManager.Raise(nameof(IServerStopedPlugin.OnServerStoped), this, new ServiceStateEventArgs(this.m_serverState, default));
            }
            return this;
        }

        /// <summary>
        /// 尝试获取TClient
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="socketClient">TClient</param>
        /// <returns></returns>
        public bool TryGetSocketClient(string id, out TClient socketClient)
        {
            return this.m_socketClients.TryGetSocketClient(id, out socketClient);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (this.DisposedValue)
            {
                return;
            }

            if (disposing)
            {
                foreach (var item in this.m_monitors)
                {
                    item.Socket.SafeDispose();
                    item.SocketAsyncEvent.SafeDispose();
                }

                this.m_monitors.Clear();

                this.Clear();

                this.m_serverState = ServerState.Disposed;
                if (this.PluginsManager.Enable)
                {
                    this.PluginsManager.Raise(nameof(IServerStopedPlugin.OnServerStoped), this, new ServiceStateEventArgs(this.m_serverState, default));
                }
                this.PluginsManager.SafeDispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 初始化客户端实例。
        /// </summary>
        /// <returns></returns>
        protected virtual TClient GetClientInstence()
        {
            return new TClient();
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="config"></param>
        protected virtual void LoadConfig(TouchSocketConfig config)
        {
            if (config.GetValue(TouchSocketConfigExtension.GetDefaultNewIdProperty) is Func<string> fun)
            {
                this.m_getDefaultNewId = fun;
            }
            this.m_maxCount = config.GetValue(TouchSocketConfigExtension.MaxCountProperty);

            if (config.GetValue(TouchSocketConfigExtension.BufferLengthProperty) is int value)
            {
                this.SetBufferLength(value);
            }
        }

        /// <summary>
        /// 在验证Ssl发送错误时。
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void OnAuthenticatingError(Exception ex)
        {
        }

        /// <inheritdoc/>
        protected virtual void PreviewBind(TcpNetworkMonitor monitor)
        {
        }

        private void Args_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Accept)
            {
                this.OnAccepted(e);
            }
        }

        private void BeginListen(List<ListenOption> optionList)
        {
            foreach (var item in optionList)
            {
                this.AddListen(item);
            }
        }

        private void BuildConfig(TouchSocketConfig config)
        {
            this.m_config = config;

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

            this.m_container = container;
            this.m_pluginsManager = pluginsManager;
        }

        private void OnAccepted(SocketAsyncEventArgs e)
        {
            if (!this.DisposedValue)
            {
                if (e.SocketError == SocketError.Success && e.AcceptSocket != null)
                {
                    var socket = e.AcceptSocket;
                    if (this.SocketClients.Count < this.m_maxCount)
                    {
                        this.OnClientSocketInit(socket, (TcpNetworkMonitor)e.UserToken);
                    }
                    else
                    {
                        socket.SafeDispose();
                        this.Logger.Warning(this, "连接客户端数量已达到设定最大值");
                    }
                }
                e.AcceptSocket = null;

                try
                {
                    if (!((TcpNetworkMonitor)e.UserToken).Socket.AcceptAsync(e))
                    {
                        this.OnAccepted(e);
                    }
                }
                catch
                {
                    e.SafeDispose();
                    return;
                }
            }
        }

        private void OnClientSocketInit(Socket socket, TcpNetworkMonitor monitor)
        {
            try
            {
                if (monitor.Options.NoDelay)
                {
                    socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                }
                socket.ReceiveBufferSize = monitor.Options.BufferLength;
                socket.SendBufferSize = monitor.Options.BufferLength;
                socket.SendTimeout = monitor.Options.SendTimeout;

                var client = this.GetClientInstence();
                client.InternalSetConfig(this.m_config);
                client.InternalSetContainer(this.m_container);
                client.InternalSetService(this);
                client.InternalSetReceiveType(monitor.Options.ReceiveType);
                client.InternalSetSocket(socket);
                client.InternalSetPluginsManager(this.m_pluginsManager);
                client.SetBufferLength(monitor.Options.BufferLength);
                if (client.CanSetDataHandlingAdapter)
                {
                    client.SetDataHandlingAdapter(monitor.Options.TcpAdapter.Invoke());
                }
                client.InternalInitialized();

                var args = new ConnectingEventArgs(socket)
                {
                    Id = this.GetDefaultNewId()
                };
                client.InternalConnecting(args);//Connecting
                if (args.IsPermitOperation)
                {
                    client.InternalSetId(args.Id);
                    if (!client.MainSocket.Connected)
                    {
                        client.MainSocket.SafeDispose();
                        return;
                    }
                    if (this.m_socketClients.TryAdd(client))
                    {
                        client.InternalConnected(new ConnectedEventArgs());

                        if (monitor.Options.UseSsl)
                        {
                            try
                            {
                                client.BeginReceiveSsl(monitor.Options.ServiceSslOption);
                            }
                            catch (Exception ex)
                            {
                                this.OnAuthenticatingError(ex);
                                throw;
                            }
                        }
                        else
                        {
                            client.BeginReceive();
                        }
                    }
                    else
                    {
                        throw new Exception($"Id={client.Id}重复");
                    }
                }
                else
                {
                    socket.SafeDispose();
                }
            }
            catch (Exception ex)
            {
                socket.SafeDispose();
                this.Logger.Log(LogLevel.Error, this, "接收新连接错误", ex);
            }
        }
    }

    /// <summary>
    /// TCP服务器
    /// </summary>
    public class TcpService : TcpService<SocketClient>
    {
        /// <summary>
        /// 处理数据
        /// </summary>
        public ReceivedEventHandler<SocketClient> Received { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected override void OnReceived(SocketClient socketClient, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.Received?.Invoke(socketClient, byteBlock, requestInfo);
            base.OnReceived(socketClient, byteBlock, requestInfo);
        }
    }
}