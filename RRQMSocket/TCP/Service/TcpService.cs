//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.Log;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace RRQMSocket
{
    /// <summary>
    /// TCP服务器
    /// </summary>
    public abstract class TcpService<TClient> : BaseSocket, ITcpService<TClient>, ITcpServiceBase where TClient : SocketClient, new()
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpService()
        {
            this.socketClients = new SocketClientCollection<TClient>();
            this.rawClients = new System.Collections.Concurrent.ConcurrentQueue<TClient>();
        }

        #region 属性

        private int clearInterval;
        private NetworkMonitor[] monitors;
        private int maxCount;
        private string name;
        private ServerState serverState;
        private ServiceConfig serviceConfig;
        private SocketClientCollection<TClient> socketClients;
        private System.Collections.Concurrent.ConcurrentQueue<TClient> rawClients;
        private static RRQMCore.SnowflakeIDGenerator iDGenerator = new RRQMCore.SnowflakeIDGenerator(4);

        /// <summary>
        /// 获取清理无数据交互的SocketClient，默认60。如果不想清除，可使用-1。
        /// </summary>
        public int ClearInterval
        {
            get { return clearInterval; }
        }

        /// <summary>
        /// 最大可连接数
        /// </summary>
        public int MaxCount
        {
            get { return maxCount; }
        }

        /// <summary>
        /// 服务器名称
        /// </summary>
        public string ServerName
        {
            get { return name; }
        }

        /// <summary>
        /// 服务器状态
        /// </summary>
        public ServerState ServerState
        {
            get { return serverState; }
        }

        /// <summary>
        /// 获取服务器配置
        /// </summary>
        public ServiceConfig ServiceConfig
        { get { return serviceConfig; } }

        /// <summary>
        /// 获取当前连接的所有客户端
        /// </summary>
        public SocketClientCollection<TClient> SocketClients
        {
            get { return socketClients; }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public NetworkMonitor[] Monitors
        {
            get { return monitors; }
        }

        /// <summary>
        /// 清理选择类型
        /// </summary>
        public ClearType ClearType { get => this.clearType; set => this.clearType = value; }

        #endregion 属性

        #region 变量

        private ClearType clearType;
        private int backlog;
        private Thread threadClearClient;
        private ReceiveType receiveType;

        #endregion 变量

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
        /// 用户连接完成
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual void OnConnected(TClient socketClient, MesEventArgs e)
        {
            this.Connected?.Invoke(socketClient, e);
            socketClient.OnEvent(1, e);
        }

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(TClient socketClient, MesEventArgs e)
        {
            this.Disconnected?.Invoke(socketClient, e);
            socketClient.OnEvent(2, e);
        }

        /// <summary>
        /// 有用户连接的时候
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual void OnConnecting(TClient socketClient, ClientOperationEventArgs e)
        {
            this.Connecting?.Invoke(socketClient, e);
            socketClient.OnEvent(3, e);
        }

        #endregion 事件

        /// <summary>
        /// 获取默认的新ID
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultNewID()
        {
            return iDGenerator.NextID().ToString();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Clear()
        {
            this.socketClients.Clear();
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
        /// 重新设置ID
        /// </summary>
        /// <param name="waitSetID"></param>
        /// <returns></returns>
        public virtual void ResetID(WaitSetID waitSetID)
        {
            if (!this.socketClients.TryGetSocketClient(waitSetID.OldID, out TClient client))
            {
                throw new RRQMException("oldID不存在");
            }
            if (this.socketClients.TryRemove(waitSetID.OldID))
            {
                client.id = waitSetID.NewID;
                if (!this.socketClients.TryAdd(client))
                {
                    client.id = waitSetID.OldID;
                    this.socketClients.TryAdd(client);
                    throw new RRQMException("ID重复");
                }
            }
            else
            {
                throw new RRQMException("oldID不存在");
            }
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="buffer"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public virtual void Send(string id, byte[] buffer)
        {
            this.Send(id, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public virtual void Send(string id, byte[] buffer, int offset, int length)
        {
            this.SocketClients[id].Send(buffer, offset, length);
        }

        /// <summary>
        /// 发送流中的有效数据
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="byteBlock"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public virtual void Send(string id, ByteBlock byteBlock)
        {
            this.Send(id, byteBlock.Buffer, 0, byteBlock.Len);
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="buffer"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public virtual void SendAsync(string id, byte[] buffer)
        {
            this.SendAsync(id, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public virtual void SendAsync(string id, byte[] buffer, int offset, int length)
        {
            this.SocketClients[id].SendAsync(buffer, offset, length);
        }

        /// <summary>
        /// 发送流中的有效数据
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="byteBlock"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public virtual void SendAsync(string id, ByteBlock byteBlock)
        {
            this.SendAsync(id, byteBlock.Buffer, 0, byteBlock.Len);
        }

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="serviceConfig"></param>
        public virtual IService Setup(ServiceConfig serviceConfig)
        {
            this.serviceConfig = serviceConfig;
            this.LoadConfig(serviceConfig);
            return this;
        }

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="port"></param>
        public virtual IService Setup(int port)
        {
            TcpServiceConfig serviceConfig = new TcpServiceConfig();
            serviceConfig.ListenIPHosts = new IPHost[] { new IPHost(port) };
            return this.Setup(serviceConfig);
        }

        /// <summary>
        /// 根据ID判断SocketClient是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool SocketClientExist(string id)
        {
            return this.SocketClients.SocketClientExist(id);
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual IService Start()
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
        /// 停止服务器，可重新启动
        /// </summary>
        public virtual IService Stop()
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
        /// 加载配置
        /// </summary>
        /// <param name="serviceConfig"></param>
        protected virtual void LoadConfig(ServiceConfig serviceConfig)
        {
            if (serviceConfig == null)
            {
                throw new RRQMException("配置文件为空");
            }
            this.maxCount = (int)serviceConfig.GetValue(TcpServiceConfig.MaxCountProperty);
            this.clearInterval = (int)serviceConfig.GetValue(TcpServiceConfig.ClearIntervalProperty);
            this.backlog = (int)serviceConfig.GetValue(TcpServiceConfig.BacklogProperty);
            this.logger = (ILog)serviceConfig.GetValue(RRQMConfig.LoggerProperty);
            this.BufferLength = (int)serviceConfig.GetValue(RRQMConfig.BufferLengthProperty);
            this.name = serviceConfig.ServerName;
            this.clearType = (ClearType)serviceConfig.GetValue(TcpServiceConfig.ClearTypeProperty);
            this.receiveType = serviceConfig.GetValue<ReceiveType>(TcpServiceConfig.ReceiveTypeProperty);
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
        /// 成功建立Tcp连接，但还未在<see cref="OnConnecting(TClient, ClientOperationEventArgs)"/>中进行连接筛选。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <returns></returns>
        protected virtual void PreviewConnecting(TClient socketClient)
        {
            ClientOperationEventArgs clientArgs = new ClientOperationEventArgs();
            clientArgs.ID = GetDefaultNewID();
            this.OnConnecting(socketClient, clientArgs);
            if (clientArgs.IsPermitOperation)
            {
                MakeClientReceive(socketClient, clientArgs.ID);
                OnConnected(socketClient, new MesEventArgs("新客户端连接"));
            }
            else
            {
                socketClient.Dispose();
            }
        }

        /// <summary>
        /// 添加客户端辅助类，然后开始接收
        /// </summary>
        /// <param name="client"></param>
        /// <param name="id"></param>
        protected void MakeClientReceive(TClient client, string id)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException($"“{nameof(id)}”不能为 null 或空。", nameof(id));
            }

            client.id = id;
            if (!this.socketClients.TryAdd(client))
            {
                throw new RRQMException("ID重复");
            }
            client.BeginReceive();
        }

        private void Args_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Accept)
            {
                ProcessAccept(e);
            }
        }

        private void BeginClearAndHandle()
        {
            threadClearClient = new Thread(CheckClient);
            threadClearClient.IsBackground = true;
            threadClearClient.Name = "ClearClient";
            threadClearClient.Start();

            int threadCount = this.ServiceConfig.ThreadCount;
            ThreadPool.SetMinThreads(this.serviceConfig.ThreadCount, this.serviceConfig.ThreadCount);
        }

        private void BeginListen(IPHost[] iPHosts)
        {
            List<NetworkMonitor> networkMonitors = new List<NetworkMonitor>();
            foreach (var iPHost in iPHosts)
            {
                try
                {
                    Socket socket = new Socket(iPHost.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    PreviewBind(socket);
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
                    ProcessAccept(e);
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
                if (collection.Length == 0 && disposable)
                {
                    return;
                }
                foreach (var token in collection)
                {
                    if (this.SocketClients.TryGetSocketClient(token, out TClient client))
                    {
                        if (this.clearInterval > 0)
                        {
                            client.GetTimeout((int)(this.clearInterval / 1000.0), tick);
                        }

                        if (client.breakOut)
                        {
                            try
                            {
                                client.Dispose();
                                if (this.SocketClients.TryRemove(token))
                                {
                                    this.OnDisconnected(client, new MesEventArgs("breakOut"));
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Debug(LogType.Error, this, $"在检验客户端时发生错误，信息：{ex.Message}");
                            }
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
                    Logger.Debug(LogType.Error, this, $"在新建客户端时发生错误，信息：{ex.Message}");
                }
            }
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

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            try
            {
                if (!this.disposable)
                {
                    if (e.SocketError == SocketError.Success && e.AcceptSocket != null)
                    {
                        try
                        {
                            Socket newSocket = e.AcceptSocket;
                            if (this.SocketClients.Count > this.maxCount)
                            {
                                this.Logger.Debug(LogType.Warning, this, "连接客户端数量已达到设定最大值");
                                newSocket.Close();
                                newSocket.Dispose();
                            }

                            TClient client = this.GetRawClient();
                            client.lastTick = DateTime.Now.Ticks;
                            client.serviceConfig = this.serviceConfig;
                            client.service = this;
                            client.Logger = this.Logger;
                            client.ClearType = this.clearType;
                            client.MainSocket = newSocket;
                            client.ReadIpPort();
                            client.BufferLength = this.BufferLength;
                            client.receiveType = this.receiveType;

                            PreviewConnecting(client);
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, "接收新连接错误", ex);
                        }
                    }
                    e.AcceptSocket = null;
                    if (!((Socket)e.UserToken).AcceptAsync(e))
                    {
                        ProcessAccept(e);
                    }
                }
            }
            catch
            {
            }
        }
    }
}