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
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// TCP泛型服务器，由客户自己指定<see cref="SocketClient"/>类型。
    /// </summary>
    public class TcpService<TClient> : TcpServiceBase, ITcpService<TClient>, IPlguinObject where TClient : SocketClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpService()
        {
            this.m_iDGenerator = new SnowflakeIDGenerator(4);
            this.m_socketClients = new SocketClientCollection();
            this.GetDefaultNewID = () => { return this.m_iDGenerator.NextID().ToString(); };
        }

        #region 变量

        private readonly SnowflakeIDGenerator m_iDGenerator;
        private int m_backlog;
        private int m_clearInterval;
        private ClearType m_clearType;
        private int m_maxCount;
        private NetworkMonitor[] m_monitors;
        private ReceiveType m_receiveType;
        private ServerState m_serverState;
        private RRQMConfig m_config;
        private SocketClientCollection m_socketClients;
        private bool m_usePlugin;
        private bool m_useSsl;
        #endregion 变量

        #region 属性

        /// <summary>
        /// 获取默认新ID。
        /// </summary>
        public Func<string> GetDefaultNewID { get;private set; }

        /// <summary>
        /// 获取清理无数据交互的SocketClient，默认60。如果不想清除，可使用-1。
        /// </summary>
        public int ClearInterval => this.m_clearInterval;

        /// <summary>
        /// 清理类型
        /// </summary>
        public ClearType ClearType => this.m_clearType;

        /// <summary>
        /// 获取服务器配置
        /// </summary>
        public override RRQMConfig Config => this.m_config;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override IContainer Container => this.Config?.Container;

        /// <summary>
        /// 最大可连接数
        /// </summary>
        public int MaxCount => this.m_maxCount;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override NetworkMonitor[] Monitors => this.m_monitors;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override IPluginsManager PluginsManager => this.Config?.PluginsManager;

        /// <summary>
        /// 服务器名称
        /// </summary>
        public override string ServerName => this.Config == null ? null : this.Config.GetValue<string>(RRQMConfigExtensions.ServerNameProperty);

        /// <summary>
        /// 服务器状态
        /// </summary>
        public override ServerState ServerState => this.m_serverState;

        /// <summary>
        /// 获取当前连接的所有客户端
        /// </summary>
        public override SocketClientCollection SocketClients => this.m_socketClients;

        /// <summary>
        /// 是否已启动插件
        /// </summary>
        public bool UsePlugin => this.m_usePlugin;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool UseSsl => this.m_useSsl;

        #endregion 属性

        #region 事件

        /// <summary>
        /// 用户连接完成
        /// </summary>
        public event EventHandler<TClient> Connected;

        /// <summary>
        /// 有用户连接的时候
        /// </summary>
        public event ClientOperationEventHandler<TClient> Connecting;

        /// <summary>
        /// 有用户断开连接
        /// </summary>
        public event ClientDisconnectedEventHandler<TClient> Disconnected;

        /// <summary>
        /// 当客户端ID被修改时触发。
        /// </summary>
        public event EventHandler<TClient> IDChanged;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected sealed override void OnClientConnected(ISocketClient socketClient, RRQMEventArgs e)
        {
            this.OnConnected((TClient)socketClient, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected sealed override void OnClientConnecting(ISocketClient socketClient, ClientOperationEventArgs e)
        {
            this.OnConnecting((TClient)socketClient, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected sealed override void OnClientDisconnected(ISocketClient socketClient, ClientDisconnectedEventArgs e)
        {
            this.OnDisconnected((TClient)socketClient, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected sealed override void OnClientReceivedData(ISocketClient socketClient, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.OnReceived((TClient)socketClient, byteBlock, requestInfo);
        }

        /// <summary>
        /// 客户端连接完成，覆盖父类方法将不会触发事件。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual void OnConnected(TClient socketClient, RRQMEventArgs e)
        {
            this.Connected?.Invoke(socketClient, e);
        }

        /// <summary>
        /// 客户端请求连接，覆盖父类方法将不会触发事件。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual void OnConnecting(TClient socketClient, ClientOperationEventArgs e)
        {
            this.Connecting?.Invoke(socketClient, e);
        }

        /// <summary>
        /// 客户端断开连接，覆盖父类方法将不会触发事件。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(TClient socketClient, ClientDisconnectedEventArgs e)
        {
            this.Disconnected?.Invoke(socketClient, e);
        }

        /// <summary>
        /// 当客户端ID被修改时触发，覆盖父类方法将不会触发事件和插件。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual void OnIDChanged(TClient socketClient, RRQMEventArgs e)
        {
            if (this.m_usePlugin)
            {
                this.PluginsManager.Raise<ITcpPlugin>("OnIDChanged", socketClient, e);
                if (e.Handled)
                {
                    return;
                }
            }
            this.IDChanged?.Invoke(socketClient, e);
        }

        /// <summary>
        /// 当收到适配器数据，父类方法为空。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected virtual void OnReceived(TClient socketClient, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
        }

        #endregion 事件

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
        /// <inheritdoc/>
        /// </summary>
        public override void Clear()
        {
            string[] ids = this.GetIDs();
            foreach (var item in ids)
            {
                if (this.TryGetSocketClient(item, out TClient client))
                {
                    client.Dispose();
                }
            }
        }

        /// <summary>
        /// 获取当前在线的所有客户端
        /// </summary>
        /// <returns></returns>
        public TClient[] GetClients()
        {
            var clients = this.m_socketClients.GetClients().ToArray();

            TClient[] clients1 = new TClient[clients.Length];
            for (int i = 0; i < clients.Length; i++)
            {
                clients1[i] = (TClient)clients[i];
            }
            return clients1;
        }

        /// <summary>
        /// 重置ID
        /// </summary>
        /// <param name="oldID"></param>
        /// <param name="newID"></param>
        /// <exception cref="ClientNotFindException"></exception>
        /// <exception cref="RRQMException"></exception>
        public override void ResetID(string oldID, string newID)
        {
            if (string.IsNullOrEmpty(oldID))
            {
                throw new ArgumentException($"“{nameof(oldID)}”不能为 null 或空。", nameof(oldID));
            }

            if (string.IsNullOrEmpty(newID))
            {
                throw new ArgumentException($"“{nameof(newID)}”不能为 null 或空。", nameof(newID));
            }

            if (oldID == newID)
            {
                return;
            }
            if (this.m_socketClients.TryRemove(oldID, out TClient socketClient))
            {
                socketClient.m_id = newID;
                if (this.m_socketClients.TryAdd(socketClient))
                {
                    this.OnIDChanged(socketClient, new RRQMEventArgs());
                    return;
                }
                else
                {
                    socketClient.m_id = oldID;
                    this.m_socketClients.TryAdd(socketClient);
                    throw new RRQMException("ID重复");
                }
            }
            else
            {
                throw new ClientNotFindException(ResType.ClientNotFind.GetDescription(oldID));
            }
        }

       
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"></param>
        public override IService Setup(RRQMConfig config)
        {
            this.m_config = config;
            this.LoadConfig(config);
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="port"></param>
        public override IService Setup(int port)
        {
            RRQMConfig serviceConfig = new RRQMConfig();
            serviceConfig.SetListenIPHosts(new IPHost[] { new IPHost(port) });
            return this.Setup(serviceConfig);
        }

        /// <summary>
        ///<inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool SocketClientExist(string id)
        {
            return this.SocketClients.SocketClientExist(id);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public override IService Start()
        {
            IPHost[] iPHosts = this.Config.GetValue<IPHost[]>(RRQMConfigExtensions.ListenIPHostsProperty);
            if (iPHosts == null || iPHosts.Length == 0)
            {
                throw new RRQMException("IPHosts为空，无法绑定");
            }
            switch (this.m_serverState)
            {
                case ServerState.None:
                    {
                        this.BeginClearAndHandle();
                        this.BeginListen(iPHosts);
                        break;
                    }
                case ServerState.Running:
                    {
                        return this;
                    }
                case ServerState.Stopped:
                    {
                        this.BeginListen(iPHosts);
                        break;
                    }
                case ServerState.Disposed:
                    {
                        throw new RRQMException("无法重新利用已释放对象");
                    }
            }
            this.m_serverState = ServerState.Running;
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override IService Stop()
        {
            if (this.m_monitors != null)
            {
                foreach (var item in this.m_monitors)
                {
                    if (item.Socket != null)
                    {
                        item.Socket.Dispose();
                    }
                }
            }
            this.m_monitors = null;

            this.Clear();

            this.m_serverState = ServerState.Stopped;

            return this;
        }

        /// <summary>
        /// 尝试获取TClient
        /// </summary>
        /// <param name="id">ID</param>
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
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    if (this.m_monitors != null)
                    {
                        foreach (var item in this.m_monitors)
                        {
                            if (item.Socket != null)
                            {
                                item.Socket.Dispose();
                            }
                        }
                        this.m_monitors = null;
                    }
                    this.Clear();
                    this.m_serverState = ServerState.Disposed;
                    this.PluginsManager.Clear();
                }
                this.disposedValue = true;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// 初始化客户端实例，默认实现为<see cref="Container.Resolve(Type, object[], string)"/>
        /// </summary>
        /// <returns></returns>
        protected virtual TClient GetClientInstence()
        {
            return this.Container.Resolve<TClient>();
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
            if (config.GetValue<int>(RRQMConfigExtensions.ThreadCountProperty) <= 0)
            {
                throw new RRQMException("线程数量必须大于0");
            }
            if (config.GetValue<Func<string>>(RRQMConfigExtensions.GetDefaultNewIDProperty) is Func<string> fun)
            {
                this.GetDefaultNewID = fun;
            }
            this.m_usePlugin = config.IsUsePlugin;
            this.m_maxCount = config.GetValue<int>(RRQMConfigExtensions.MaxCountProperty);
            this.m_clearInterval = config.GetValue<int>(RRQMConfigExtensions.ClearIntervalProperty);
            this.m_backlog = config.GetValue<int>(RRQMConfigExtensions.BacklogProperty);
            this.BufferLength = config.GetValue<int>(RRQMConfig.BufferLengthProperty);
            this.m_clearType = config.GetValue<ClearType>(RRQMConfigExtensions.ClearTypeProperty);
            this.m_receiveType = config.ReceiveType;

            if (this.Logger == null)
            {
                this.Logger = this.Container.Resolve<ILog>();
            }
            if (config.GetValue(RRQMConfigExtensions.SslOptionProperty) != null)
            {
                this.m_useSsl = true;
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

        private void Args_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Accept)
            {
                this.OnAccepted(e);
            }
        }

        private void BeginClearAndHandle()
        {
            Thread thread = new Thread(this.CheckClient);
            thread.IsBackground = true;
            thread.Name = "CheckClient";
            thread.Start();
            int threadCount = this.Config.GetValue<int>(RRQMConfigExtensions.ThreadCountProperty);
            while (!ThreadPool.SetMinThreads(threadCount, threadCount))
            {
                threadCount--;
            }
        }

        private void BeginListen(IPHost[] iPHosts)
        {
            List<NetworkMonitor> networkMonitors = new List<NetworkMonitor>();
            foreach (var iPHost in iPHosts)
            {
                try
                {
                    Socket socket = new Socket(iPHost.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    socket.ReceiveBufferSize = this.BufferLength;
                    socket.SendBufferSize = this.BufferLength;
                    this.PreviewBind(socket);
                    socket.Bind(iPHost.EndPoint);
                    socket.Listen(this.m_backlog);

                    networkMonitors.Add(new NetworkMonitor(iPHost, socket));
                }
                catch (Exception ex)
                {
                    this.Logger.Debug(LogType.Error, this, $"在监听{iPHost.ToString()}时发送错误。", ex);
                }
            }

            if (networkMonitors.Count > 0)
            {
                this.m_monitors = networkMonitors.ToArray();
            }
            else
            {
                throw new RRQMException("监听地址全都不可用。");
            }

            foreach (var networkMonitor in this.m_monitors)
            {
                SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                e.UserToken = networkMonitor.Socket;
                e.Completed += this.Args_Completed;
                if (!networkMonitor.Socket.AcceptAsync(e))
                {
                    this.OnAccepted(e);
                }
            }
        }

        private void CheckClient()
        {
            while (true)
            {
                Thread.Sleep(1000);
                long tick = DateTime.Now.Ticks / 10000000;
                string[] collection = this.SocketClients.GetIDs();
                if (collection.Length == 0 && this.disposedValue)
                {
                    return;
                }

                foreach (var token in collection)
                {
                    if (this.SocketClients.TryGetSocketClient(token, out TClient client))
                    {
                        if (this.m_clearInterval > 0)
                        {
                            client.GetTimeout((int)(this.m_clearInterval / 1000.0), tick);
                        }
                    }
                }
            }
        }

        private void OnAccepted(SocketAsyncEventArgs e)
        {
            if (!this.disposedValue)
            {
                if (e.SocketError == SocketError.Success && e.AcceptSocket != null)
                {
                    Socket socket = e.AcceptSocket;
                    socket.ReceiveBufferSize = this.BufferLength;
                    socket.SendBufferSize = this.BufferLength;
                    if (this.SocketClients.Count > this.m_maxCount)
                    {
                        this.Logger.Debug(LogType.Warning, this, "连接客户端数量已达到设定最大值");
                        socket.Close();
                        socket.Dispose();
                    }
                    this.OnTask(socket);
                }
                e.AcceptSocket = null;

                try
                {
                    if (!((Socket)e.UserToken).AcceptAsync(e))
                    {
                        this.OnAccepted(e);
                    }
                }
                catch
                {
                    e.Dispose();
                    return;
                }
            }
        }

        private void SetClientConfiguration(SocketClient client)
        {
            client.m_usePlugin = this.m_usePlugin;
            client.m_lastTick = DateTime.Now.Ticks;
            client.m_config = this.m_config;
            client.m_service = this;
            if (client.Logger == null)
            {
                client.Logger = this.Container.Resolve<ILog>();
            }
            client.ClearType = this.m_clearType;
            client.m_receiveType = this.m_receiveType;
            client.BufferLength = this.BufferLength;
        }

        /// <summary>
        /// 在验证Ssl发送错误时。
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void OnAuthenticatingError(Exception ex)
        {

        }

        private void OnTask(Socket socket)
        {
            Task.Run(() =>
            {
                try
                {
                    socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, this.Config.GetValue<bool>(RRQMConfigExtensions.NoDelayProperty));
                    TClient client = this.GetClientInstence();
                    this.SetClientConfiguration(client);
                    client.SetSocket(socket);

                    ClientOperationEventArgs args = new ClientOperationEventArgs();
                    args.ID = this.GetDefaultNewID();
                    client.InternalConnecting(args);//Connecting
                    if (args.Operation.HasFlag(Operation.Permit))
                    {
                        client.m_id = args.ID;
                        if (this.SocketClients.TryAdd(client))
                        {
                            client.InternalConnected(new MesEventArgs("客户端成功连接"));
                            if (this.m_useSsl)
                            {
                                try
                                {
                                    client.BeginReceiveSsl(this.m_receiveType, this.m_config.GetValue<ServiceSslOption>(RRQMConfigExtensions.SslOptionProperty));
                                }
                                catch (Exception ex)
                                {
                                    this.OnAuthenticatingError(ex);
                                    return;
                                }
                            }
                            else
                            {
                                client.BeginReceive(this.m_receiveType);
                            }
                        }
                        else
                        {
                            throw new RRQMException($"ID={client.m_id}重复");
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
                    this.Logger.Debug(LogType.Error, this, "接收新连接错误", ex);
                }
            });
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
        public event ReceivedEventHandler<SocketClient> Received;

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