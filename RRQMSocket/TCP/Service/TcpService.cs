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
        }

        #region 属性

        private int clearInterval;
        private NetworkMonitor[] monitors;
        private int maxCount;
        private string name;
        private ServerState serverState;
        private ServiceConfig serviceConfig;
        private SocketClientCollection<TClient> socketClients;

        /// <summary>
        /// 获取默认内存池
        /// </summary>
        public BytePool BytePool { get { return BytePool.Default; } }

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
        public ServiceConfig ServiceConfig { get { return serviceConfig; } }

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
        #endregion 属性

        #region 变量

        internal ClearType clearType;
        internal bool separateThreadReceive;
        private int backlog;
        private BufferQueueGroup[] bufferQueueGroups;
        private Thread threadClearClient;

        #endregion 变量

        #region 事件

        /// <summary>
        /// 有用户连接的时候
        /// </summary>
        public event RRQMMessageEventHandler<TClient> Connected;

        /// <summary>
        /// 有用户断开连接的时候
        /// </summary>
        public event RRQMMessageEventHandler<TClient> Disconnected;

        /// <summary>
        /// 在客户端连接时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnClientConnected(TClient client, MesEventArgs e)
        {
            this.Connected?.Invoke(client, e);
            client.OnEvent(1, e);
        }

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnClientDisconnected(TClient client, MesEventArgs e)
        {
            this.Disconnected?.Invoke(client, e);
            client.OnEvent(2, e);
        }

        #endregion 事件

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
            if (bufferQueueGroups != null)
            {
                foreach (var item in bufferQueueGroups)
                {
                    item.Dispose();
                }
            }

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
            IPHost[] iPHosts = (IPHost[])this.ServiceConfig.GetValue(ServiceConfig.ListenIPHostsProperty);
            if (iPHosts == null || iPHosts.Length == 0)
            {
                throw new RRQMException("IPHosts为空，无法绑定");
            }
            switch (this.serverState)
            {
                case ServerState.None:
                    {
                        this.BeginListen(iPHosts);
                        this.BeginClearAndHandle();
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
            this.separateThreadReceive = serviceConfig.SeparateThreadReceive;
        }

        /// <summary>
        /// 成功连接后。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="createOption"></param>
        protected abstract void OnCreateSocketClient(TClient socketClient, CreateOption createOption);

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
        /// 创建客户端之前
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="queueGroup"></param>
        protected virtual void PreviewCreateSocketClient(Socket socket, BufferQueueGroup queueGroup)
        {
            try
            {
                if (this.SocketClients.Count > this.maxCount)
                {
                    this.Logger.Debug(LogType.Error, this, "连接客户端数量已达到设定最大值");
                    socket.Close();
                    socket.Dispose();
                    return;
                }

                TClient client = (TClient)Activator.CreateInstance(typeof(TClient));

                client.serviceConfig = this.serviceConfig;

                client.queueGroup = queueGroup;
                client.service = this;
                client.Logger = this.Logger;
                client.ClearType = this.clearType;
                client.separateThreadReceive = this.separateThreadReceive;

                client.MainSocket = socket;
                client.ReadIpPort();
                client.BufferLength = this.BufferLength;

                CreateOption creatOption = new CreateOption();
                creatOption.ID = this.SocketClients.GetDefaultID();
                this.OnCreateSocketClient(client, creatOption);
                client.id = creatOption.ID;

                if (!this.socketClients.TryAdd(client))
                {
                    throw new RRQMException("ID重复");
                }

                client.BeginReceive();
                OnClientConnected(client, new MesEventArgs("新客户端连接"));
            }
            catch (Exception ex)
            {
                Logger.Debug(LogType.Error, this, $"在接收客户端时发生错误，信息：{ex.Message}");
            }
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
            threadClearClient = new Thread(ClearClient);
            threadClearClient.IsBackground = true;
            threadClearClient.Name = "ClearClient";
            threadClearClient.Start();

            this.bufferQueueGroups = new BufferQueueGroup[this.ServiceConfig.ThreadCount];
            for (int i = 0; i < this.serviceConfig.ThreadCount; i++)
            {
                BufferQueueGroup bufferQueueGroup = new BufferQueueGroup();
                bufferQueueGroup.bytePool = new BytePool(this.ServiceConfig.BytePoolMaxSize, this.ServiceConfig.BytePoolMaxBlockSize);
                bufferQueueGroups[i] = bufferQueueGroup;

                if (this.separateThreadReceive)
                {
                    bufferQueueGroup.Thread = new Thread(Handle);//处理用户的消息
                    bufferQueueGroup.waitHandleBuffer = new AutoResetEvent(false);
                    bufferQueueGroup.bufferAndClient = new BufferQueue();
                    bufferQueueGroup.Thread.IsBackground = true;
                    bufferQueueGroup.Thread.Name = i + "-Num Handler";
                    bufferQueueGroup.Thread.Start(bufferQueueGroup);
                }
                else
                {
                    ThreadPool.SetMinThreads(this.serviceConfig.ThreadCount, this.serviceConfig.ThreadCount);
                }
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

        private void ClearClient()
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
                            client.GetTimeout(this.clearInterval / 1000, tick);
                        }

                        if (client.breakOut)
                        {
                            try
                            {
                                client.Dispose();
                                if (this.SocketClients.TryRemove(token))
                                {
                                    this.OnClientDisconnected(client, new MesEventArgs("breakOut"));
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Debug(LogType.Error, this, $"在检验客户端时发生错误，信息：{ex.Message}");
                            }
                        }
                    }
                }
            }
        }

        private void Handle(object o)
        {
            BufferQueueGroup queueGroup = (BufferQueueGroup)o;
            while (true)
            {
                if (disposable)
                {
                    break;
                }
                ClientBuffer clientBuffer;
                if (queueGroup.bufferAndClient.TryDequeue(out clientBuffer))
                {
                    clientBuffer.client.HandleBuffer(clientBuffer.byteBlock);
                }
                else
                {
                    queueGroup.waitHandleBuffer.WaitOne();
                }
            }
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
                            PreviewCreateSocketClient(newSocket, this.bufferQueueGroups[this.SocketClients.Count % this.bufferQueueGroups.Length]);
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