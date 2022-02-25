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
using RRQMCore.Collections.Concurrent;
using RRQMCore.Log;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace RRQMSocket
{
    /// <summary>
    /// TCP泛型服务器，由客户自己指定<see cref="SocketClient"/>类型。
    /// </summary>
    public class TcpService<TClient> : TcpServiceBase, ITcpService<TClient>, ITcpServiceBase where TClient : SocketClient, new()
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpService()
        {
            this.iDGenerator = new RRQMCore.SnowflakeIDGenerator(4);
            this.socketClients = new SocketClientCollection();
            this.rawClients = new System.Collections.Concurrent.ConcurrentQueue<TClient>();
        }

        #region 变量

        private readonly SnowflakeIDGenerator iDGenerator;
        private int backlog;
        private int clearInterval;
        private ClearType clearType;
        private int maxCount;
        private NetworkMonitor[] monitors;
        private string name;
        private System.Collections.Concurrent.ConcurrentQueue<TClient> rawClients;
        private ReceiveType receiveType;
        private ServerState serverState;
        private ServiceConfig serviceConfig;
        private SocketClientCollection socketClients;
        private bool useSsl;
        private ConcurrentList<string>[] selectIDs;
        private ConcurrentList<string> freeIDs;
        #endregion 变量

        #region 属性

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool UseSsl
        {
            get { return this.useSsl; }
        }

        /// <summary>
        /// 获取清理无数据交互的SocketClient，默认60。如果不想清除，可使用-1。
        /// </summary>
        public int ClearInterval
        {
            get { return this.clearInterval; }
        }

        /// <summary>
        /// 清理类型
        /// </summary>
        public ClearType ClearType => this.clearType;

        /// <summary>
        /// 最大可连接数
        /// </summary>
        public int MaxCount
        {
            get { return this.maxCount; }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override NetworkMonitor[] Monitors
        {
            get { return this.monitors; }
        }

        /// <summary>
        /// 服务器名称
        /// </summary>
        public override string ServerName
        {
            get { return this.name; }
        }

        /// <summary>
        /// 服务器状态
        /// </summary>
        public override ServerState ServerState
        {
            get { return this.serverState; }
        }

        /// <summary>
        /// 获取服务器配置
        /// </summary>
        public override ServiceConfig ServiceConfig
        { get { return this.serviceConfig; } }

        /// <summary>
        /// 获取当前连接的所有客户端
        /// </summary>
        public override SocketClientCollection SocketClients
        {
            get { return this.socketClients; }
        }

        #endregion 属性

        #region 事件

        /// <summary>
        /// 用户连接完成
        /// </summary>
        public event RRQMMessageEventHandler<TClient> Connected;

        /// <summary>
        /// 有用户连接的时候
        /// </summary>
        public event RRQMClientOperationEventHandler<TClient> Connecting;

        /// <summary>
        /// 有用户断开连接
        /// </summary>
        public event RRQMMessageEventHandler<TClient> Disconnected;

        /// <summary>
        /// 当客户端ID被修改时触发。
        /// </summary>
        public event RRQMEventHandler<TClient> IDChanged;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override sealed void OnClientConnected(ISocketClient socketClient, MesEventArgs e)
        {
            this.OnConnected((TClient)socketClient, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override sealed void OnClientConnecting(ISocketClient socketClient, ClientOperationEventArgs e)
        {
            this.OnConnecting((TClient)socketClient, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override sealed void OnClientDisconnected(ISocketClient socketClient, MesEventArgs e)
        {
            this.OnDisconnected((TClient)socketClient, e);
        }

        /// <summary>
        /// 客户端连接完成
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual void OnConnected(TClient socketClient, MesEventArgs e)
        {
            this.Connected?.Invoke(socketClient, e);
        }

        /// <summary>
        /// 客户端请求连接
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual void OnConnecting(TClient socketClient, ClientOperationEventArgs e)
        {
            this.Connecting?.Invoke(socketClient, e);
        }

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(TClient socketClient, MesEventArgs e)
        {
            this.Disconnected?.Invoke(socketClient, e);
        }

        /// <summary>
        /// 当客户端ID被修改时触发
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual void OnIDChanged(TClient socketClient, RRQMEventArgs e)
        {
            this.IDChanged?.Invoke(socketClient, e);
        }

        #endregion 事件

        /// <summary>
        /// 获取默认的新ID
        /// </summary>
        /// <returns></returns>
        public string GetDefaultNewID()
        {
            return this.iDGenerator.NextID().ToString();
        }

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
        /// 关闭服务器并释放服务器资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            if (this.monitors != null)
            {
                foreach (var item in this.monitors)
                {
                    if (item.Socket != null)
                    {
                        item.Socket.Dispose();
                    }
                }
            }
            this.monitors = null;
            this.SocketClients.Clear();
            this.serverState = ServerState.Disposed;
        }

        /// <summary>
        /// 获取当前在线的所有客户端
        /// </summary>
        /// <returns></returns>
        public TClient[] GetClients()
        {
            var clients = this.socketClients.GetClients();

            TClient[] clients1 = new TClient[clients.Length];
            for (int i = 0; i < clients.Length; i++)
            {
                clients1[i] = (TClient)clients[i];
            }
            return clients1;
        }

        /// <summary>
        ///  重新设置ID
        /// </summary>
        /// <param name="waitSetID"></param>
        /// <exception cref="ClientNotFindException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="RRQMException"></exception>
        public override void ResetID(WaitSetID waitSetID)
        {
            if (waitSetID is null)
            {
                throw new ArgumentNullException(nameof(waitSetID));
            }

            if (string.IsNullOrEmpty(waitSetID.NewID))
            {
                throw new ArgumentNullException(nameof(waitSetID.NewID));
            }
            if (waitSetID.OldID == waitSetID.NewID)
            {
                return;
            }
            if (this.socketClients.TryRemove(waitSetID.OldID, out TClient socketClient))
            {
                socketClient.id = waitSetID.NewID;
                if (this.socketClients.TryAdd(socketClient))
                {
                    if (this.receiveType== ReceiveType.Select)
                    {
                        this.freeIDs.Add(socketClient.id);
                    }
                    this.OnIDChanged(socketClient, new RRQMEventArgs());
                    return;
                }
                else
                {
                    socketClient.id = waitSetID.OldID;
                    this.socketClients.TryAdd(socketClient);
                    throw new RRQMException("ID重复");
                }
            }
            else
            {
                throw new ClientNotFindException(ResType.ClientNotFind.GetResString(waitSetID.OldID));
            }
        }

        /// <summary>
        /// 重置ID
        /// </summary>
        /// <param name="oldID"></param>
        /// <param name="newID"></param>
        /// <exception cref="ClientNotFindException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void ResetID(string oldID, string newID)
        {
            WaitSetID waitSetID = new WaitSetID() { OldID = oldID, NewID = newID, Sign = -1 };
            this.ResetID(waitSetID);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="serviceConfig"></param>
        public override IService Setup(ServiceConfig serviceConfig)
        {
            this.serviceConfig = serviceConfig;
            this.LoadConfig(serviceConfig);
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="port"></param>
        public override IService Setup(int port)
        {
            TcpServiceConfig serviceConfig = new TcpServiceConfig();
            serviceConfig.ListenIPHosts = new IPHost[] { new IPHost(port) };
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
            IPHost[] iPHosts = (IPHost[])this.ServiceConfig.GetValue(TcpServiceConfig.ListenIPHostsProperty);
            if (iPHosts == null || iPHosts.Length == 0)
            {
                throw new RRQMException("IPHosts为空，无法绑定");
            }
            switch (this.serverState)
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
            this.serverState = ServerState.Running;
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override IService Stop()
        {
            if (this.monitors != null)
            {
                foreach (var item in this.monitors)
                {
                    if (item.Socket != null)
                    {
                        item.Socket.Dispose();
                    }
                }
            }
            this.monitors = null;

            this.SocketClients.Clear();

            this.serverState = ServerState.Stopped;

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
            return this.socketClients.TryGetSocketClient(id, out socketClient);
        }

        /// <summary>
        /// 获取新初始的客户端
        /// </summary>
        /// <returns></returns>
        protected virtual TClient GetRawClient()
        {
            if (this.rawClients.TryDequeue(out TClient client))
            {
                return client;
            }
            return (TClient)Activator.CreateInstance(typeof(TClient));
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="serviceConfig"></param>
        protected virtual void LoadConfig(ServiceConfig serviceConfig)
        {
            if (serviceConfig == null)
            {
                throw new RRQMException("配置文件为空");
            }
            if (serviceConfig.ThreadCount <= 0)
            {
                throw new RRQMException("线程数量必须大于0");
            }
            this.maxCount = (int)serviceConfig.GetValue(TcpServiceConfig.MaxCountProperty);
            this.clearInterval = (int)serviceConfig.GetValue(TcpServiceConfig.ClearIntervalProperty);
            this.backlog = (int)serviceConfig.GetValue(TcpServiceConfig.BacklogProperty);
            this.logger = (ILog)serviceConfig.GetValue(RRQMConfig.LoggerProperty);
            this.BufferLength = (int)serviceConfig.GetValue(RRQMConfig.BufferLengthProperty);
            this.name = serviceConfig.ServerName;
            this.clearType = (ClearType)serviceConfig.GetValue(TcpServiceConfig.ClearTypeProperty);
            this.receiveType = serviceConfig.ReceiveType;
            if (serviceConfig.GetValue(TcpServiceConfig.SslOptionProperty) != null)
            {
                this.useSsl = true;
            }
            if (this.useSsl && this.receiveType == ReceiveType.IOCP)
            {
                throw new RRQMException($"Ssl模式下只能使用{ReceiveType.BIO}或{ReceiveType.Select}模式");
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
            Thread thread1 = new Thread(this.CheckClient);
            thread1.IsBackground = true;
            thread1.Name = "CheckClient";
            thread1.Start();
            int threadCount = this.ServiceConfig.ThreadCount;
            if (this.receiveType == ReceiveType.Select)
            {
                this.selectIDs = new ConcurrentList<string>[threadCount];
                for (int i = 0; i < threadCount; i++)
                {
                    ConcurrentList<string> list = new ConcurrentList<string>();
                    this.selectIDs[i] = list;
                    Thread thread2 = new Thread(this.SelectClient);
                    thread2.IsBackground = true;
                    thread2.Name = "SelectThread" + i;
                    thread2.Start(list);
                }
                this.freeIDs = this.selectIDs[0];
            }
            else
            {
                ThreadPool.SetMinThreads(threadCount, threadCount);
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
                    this.PreviewBind(socket);
                    socket.Bind(iPHost.EndPoint);
                    socket.Listen(this.backlog);

                    networkMonitors.Add(new NetworkMonitor(iPHost, socket));
                }
                catch (Exception ex)
                {
                    this.logger.Debug(LogType.Error, this, $"在监听{iPHost.ToString()}时发送错误。", ex);
                }
            }

            if (networkMonitors.Count > 0)
            {
                this.monitors = networkMonitors.ToArray();
            }
            else
            {
                throw new RRQMException("监听地址全都不可用。");
            }

            foreach (var networkMonitor in this.monitors)
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
                if (collection.Length == 0 && this.disposable)
                {
                    return;
                }
                if (this.receiveType== ReceiveType.Select)
                {
                    var v = this.selectIDs[0];
                    foreach (var item in this.selectIDs)
                    {
                        if (v.Count > item.Count)
                        {
                            v = item;
                        }
                    }
                    this.freeIDs = v;
                }
                foreach (var token in collection)
                {
                    if (this.SocketClients.TryGetSocketClient(token, out TClient client))
                    {
                        if (this.clearInterval > 0)
                        {
                            client.GetTimeout((int)(this.clearInterval / 1000.0), tick);
                        }
                    }
                }

                try
                {
                    int needCount = 1000 - this.rawClients.Count;
                    for (int i = 0; i < needCount; i++)
                    {
                        TClient client = (TClient)Activator.CreateInstance(typeof(TClient));
                        this.rawClients.Enqueue(client);
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.Debug(LogType.Error, this, $"在新建客户端时发生错误，信息：{ex.Message}");
                }
            }
        }

        private void SelectClient(object o)
        {
            ConcurrentList<string> ids = (ConcurrentList<string>)o;
            int sleep = 0;
            long dataSize = 0;
            while (true)
            {
                if (this.disposable && ids.Count == 0)
                {
                    break;
                }
                if (dataSize > 0)
                {
                    dataSize = 0;
                    sleep = 1;
                }
                else
                {
                    if (++sleep > 100)
                    {
                        sleep = 100;
                    }
                    Thread.Sleep(sleep);
                }
                int count = ids.Count;
                for (int i = count-1; i >=0; i--)
                {
                    string id = ids[i];
                    if (this.TryGetSocketClient(id, out TClient client))
                    {
                        dataSize += client.TryReceive();
                    }
                    else
                    {
                        ids.Remove(id);
                    }
                }
            }
        }

        private void OnAccepted(SocketAsyncEventArgs e)
        {
            if (!this.disposable)
            {
                if (e.SocketError == SocketError.Success && e.AcceptSocket != null)
                {
                    Socket newSocket = e.AcceptSocket;
                    if (this.SocketClients.Count > this.maxCount)
                    {
                        this.Logger.Debug(LogType.Warning, this, "连接客户端数量已达到设定最大值");
                        newSocket.Close();
                        newSocket.Dispose();
                    }
                    this.OnTask(newSocket);
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

        private void OnTask(Socket socket)
        {
            Task.Run(() =>
            {
                try
                {
                    TClient client = this.GetRawClient();
                    client.lastTick = DateTime.Now.Ticks;
                    client.serviceConfig = this.serviceConfig;
                    client.service = this;
                    client.Logger = this.Logger;
                    client.ClearType = this.clearType;
                    client.receiveType = this.receiveType;
                    client.BufferLength = this.BufferLength;
                    client.useSsl = this.useSsl;
                    client.LoadSocketAndReadIpPort(socket);
                    ClientOperationEventArgs clientArgs = new ClientOperationEventArgs();
                    clientArgs.ID = this.GetDefaultNewID();
                    client.OnEvent(1, clientArgs);//Connecting
                    if (clientArgs.IsPermitOperation)
                    {
                        client.id = clientArgs.ID;
                        if (this.SocketClients.TryAdd(client))
                        {
                            if (this.receiveType == ReceiveType.Select)
                            {
                                this.freeIDs.Add(client.id);
                            }
                            client.OnEvent(2, new MesEventArgs("新客户端连接"));
                        }
                        else
                        {
                            throw new RRQMException($"ID={client.id}重复");
                        }
                    }
                    else
                    {
                        socket.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    socket.Dispose();
                    this.Logger.Debug(LogType.Error, this, "接收新连接错误", ex);
                }
            });
        }
    }

    /// <summary>
    /// TCP服务器
    /// </summary>
    public class TcpService : TcpService<SimpleSocketClient>
    {
        /// <summary>
        /// 处理数据
        /// </summary>
        public event RRQMReceivedEventHandler<SimpleSocketClient> Received;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override void OnConnecting(SimpleSocketClient socketClient, ClientOperationEventArgs e)
        {
            socketClient.Received += this.OnReceive;
            base.OnConnecting(socketClient, e);
        }

        private void OnReceive(SimpleSocketClient socketClient, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.Received?.Invoke(socketClient, byteBlock, requestInfo);
        }
    }
}