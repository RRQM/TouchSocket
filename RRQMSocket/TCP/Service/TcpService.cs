//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
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
using RRQMCore.Pool;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// TCP服务器
    /// </summary>
    public abstract class TcpService<TClient> : BaseSocket, ITcpService<TClient>, _ITcpService where TClient : SocketClient, new()
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpService()
        {
            this.socketClients = new SocketCliectCollection<TClient>();
            this.socketClientPool = new ObjectPool<TClient>();
        }

        #region 属性

        private int clearInterval;

        private IPHost[] listenIPHosts;
        private Socket[] listenSockets;
        private int maxCount;

        private string name;
        private ServerConfig serverConfig;

        private ServerState serverState;

        private SocketCliectCollection<TClient> socketClients;

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
        /// 获取监听的地址组
        /// </summary>
        public IPHost[] ListenIPHosts
        {
            get { return listenIPHosts; }
        }

        /// <summary>
        /// 获取正在监听的socket组
        /// </summary>
        public Socket[] ListenSockets
        {
            get { return listenSockets; }
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
        /// 获取服务器配置
        /// </summary>
        public ServerConfig ServerConfig { get { return serverConfig; } }

        /// <summary>
        /// 服务器状态
        /// </summary>
        public ServerState ServerState
        {
            get { return serverState; }
        }

        /// <summary>
        /// 获取当前连接的所有客户端
        /// </summary>
        public SocketCliectCollection<TClient> SocketClients
        {
            get { return socketClients; }
        }

        #endregion 属性

        #region 变量

        internal ObjectPool<TClient> socketClientPool;
        private int backlog;
        private BufferQueueGroup[] bufferQueueGroups;
        private Thread threadClearClient;
        private ClearType clearType;
        #endregion 变量

        #region 事件

        /// <summary>
        /// 有用户连接的时候
        /// </summary>
        public event RRQMMessageEventHandler ClientConnected;

        /// <summary>
        /// 有用户断开连接的时候
        /// </summary>
        public event RRQMMessageEventHandler ClientDisconnected;

        internal void ClientConnectedMethod(object sender, MesEventArgs e)
        {
            ClientConnected?.Invoke(sender, e);
        }

        internal void ClientDisconnectedMethod(object sender, MesEventArgs e)
        {
            ClientDisconnected?.Invoke(sender, e);
        }

        #endregion 事件

        /// <summary>
        /// 关闭服务器并释放服务器资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            if (this.listenSockets != null)
            {
                foreach (var item in this.listenSockets)
                {
                    item.Dispose();
                }
            }
            this.SocketClients.Dispose();
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
        /// <param name="oldID"></param>
        /// <param name="newID"></param>
        /// <returns></returns>
        public virtual void ResetID(string oldID, string newID)
        {
            if (!this.socketClients.TryGetSocketClient(oldID, out TClient client))
            {
                throw new RRQMException("oldID不存在");
            }
            if (this.socketClients.TryRemove(oldID))
            {
                client.id = newID;
                if (!this.socketClients.TryAdd(client))
                {
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
            this.Send(id, byteBlock.Buffer, 0, (int)byteBlock.Length);
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
            this.SendAsync(id, byteBlock.Buffer, 0, (int)byteBlock.Length);
        }

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="serverConfig"></param>
        public virtual void Setup(ServerConfig serverConfig)
        {
            this.serverConfig = serverConfig;
            this.LoadConfig(this.serverConfig);
        }

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="port"></param>
        public virtual void Setup(int port)
        {
            TcpServerConfig serverConfig = new TcpServerConfig();
            serverConfig.ListenIPHosts = new IPHost[] { new IPHost(port) };
            this.Setup(serverConfig);
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
        public virtual void Start()
        {
            IPHost[] iPHosts = (IPHost[])this.serverConfig.GetValue(ServerConfig.ListenIPHostsProperty);
            if (iPHosts == null)
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
                        return;
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
            this.listenIPHosts = iPHosts;
            this.serverState = ServerState.Running;
        }

        /// <summary>
        /// 停止服务器，可重新启动
        /// </summary>
        public virtual void Stop()
        {
            if (listenSockets != null)
            {
                foreach (var item in listenSockets)
                {
                    item.Dispose();
                }
            }
            this.SocketClients.Dispose();

            this.serverState = ServerState.Stopped;
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

        internal virtual void PreviewCreateSocketCliect(Socket socket, BufferQueueGroup queueGroup)
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

                TClient client = this.socketClientPool.GetObject();
                if (client.NewCreate)
                {
                    client.queueGroup = queueGroup;
                    client.Service = this;
                    client.Logger = this.Logger;
                    client.clearType = this.clearType;
                }

                client.MainSocket = socket;
                client.ReadIpPort();
                client.SetBufferLength(this.BufferLength);

                CreateOption creatOption = new CreateOption();
                creatOption.NewCreate = client.NewCreate;

                creatOption.ID = this.SocketClients.GetDefaultID();

                this.OnCreateSocketCliect(client, creatOption);
                client.id = creatOption.ID;

                if (!this.socketClients.TryAdd(client))
                {
                    throw new RRQMException("ID重复");
                }

                client.BeginReceive();
                ClientConnectedMethod(client, null);
            }
            catch (Exception ex)
            {
                Logger.Debug(LogType.Error, this, $"在接收客户端时发生错误，信息：{ex.Message}");
            }
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="serverConfig"></param>
        protected virtual void LoadConfig(ServerConfig serverConfig)
        {
            if (serverConfig == null)
            {
                throw new RRQMException("配置文件为空");
            }
            this.maxCount = (int)serverConfig.GetValue(TcpServerConfig.MaxCountProperty);
            this.socketClientPool.Capacity = this.maxCount;
            this.clearInterval = (int)serverConfig.GetValue(TcpServerConfig.ClearIntervalProperty);
            this.backlog = (int)serverConfig.GetValue(TcpServerConfig.BacklogProperty);
            this.Logger = (ILog)serverConfig.GetValue(ServerConfig.LoggerProperty);
            this.SetBufferLength((int)serverConfig.GetValue(ServerConfig.BufferLengthProperty));
            this.name = serverConfig.ServerName;
            this.clearType = (ClearType)serverConfig.GetValue(TcpServerConfig.ClearTypeProperty);
        }

        /// <summary>
        /// 成功连接后创建（或从对象池中获得）辅助类,
        /// 用户可以在该方法中再进行自定义设置，
        /// 但是如果该对象是从对象池获得的话，为避免重复设定某些值，
        /// 例如事件等，请先判断CreatOption.NewCreate值再做处理。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="createOption"></param>
        protected abstract void OnCreateSocketCliect(TClient socketClient, CreateOption createOption);

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
                ProcessAccept(e);
            }
        }

        private void BeginClearAndHandle()
        {
            threadClearClient = new Thread(ClearClient);
            threadClearClient.IsBackground = true;
            threadClearClient.Name = "ClearClient";
            threadClearClient.Start();

            bufferQueueGroups = new BufferQueueGroup[this.serverConfig.ThreadCount];
            for (int i = 0; i < this.serverConfig.ThreadCount; i++)
            {
                BufferQueueGroup bufferQueueGroup = new BufferQueueGroup();
                bufferQueueGroup.bytePool = new BytePool(this.serverConfig.BytePoolMaxSize, this.serverConfig.BytePoolMaxBlockSize);
                bufferQueueGroups[i] = bufferQueueGroup;
                bufferQueueGroup.Thread = new Thread(Handle);//处理用户的消息
                bufferQueueGroup.waitHandleBuffer = new AutoResetEvent(false);
                bufferQueueGroup.bufferAndClient = new BufferQueue();
                bufferQueueGroup.Thread.IsBackground = true;
                bufferQueueGroup.Thread.Name = i + "-Num Handler";
                bufferQueueGroup.Thread.Start(bufferQueueGroup);
            }
        }

        private void BeginListen(IPHost[] iPHosts)
        {
            try
            {
                this.listenSockets = new Socket[iPHosts.Length];
                int i = 0;
                foreach (var iPHost in iPHosts)
                {
                    Socket socket = new Socket(iPHost.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    this.listenSockets[i++] = socket;
                    PreviewBind(socket);
                    socket.Bind(iPHost.EndPoint);
                    socket.Listen(this.backlog);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            foreach (var socket in this.listenSockets)
            {
                SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                e.UserToken = socket;
                e.Completed += this.Args_Completed;
                if (!socket.AcceptAsync(e))
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
                if (disposable)
                {
                    break;
                }
                else
                {
                    long tick = DateTime.Now.Ticks / 10000000;
                    IEnumerable<string> collection = this.SocketClients.GetIDs();
                    foreach (var token in collection)
                    {
                        if (this.SocketClients.TryGetSocketClient(token, out TClient client))
                        {
                            if (this.clearInterval > 0)
                            {
                                client.GetTimeout(this.clearInterval, tick);
                            }

                            if (client.breakOut)
                            {
                                try
                                {
                                    client.Dispose();
                                    if (this.SocketClients.TryRemove(token))
                                    {
                                        this.socketClientPool.DestroyObject(client);
                                        ClientDisconnectedMethod(client, new MesEventArgs("breakOut"));
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
                    try
                    {
                        clientBuffer.client.HandleBuffer(clientBuffer);
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug(LogType.Error, this, "在处理数据时发生错误", ex);
                    }
                    finally
                    {
                        clientBuffer.byteBlock.Dispose();
                    }
                }
                else
                {
                    queueGroup.isWait = true;
                    queueGroup.waitHandleBuffer.WaitOne();
                    queueGroup.isWait = false;
                }
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            if (!this.disposable)
            {
                if (e.SocketError == SocketError.Success && e.AcceptSocket != null)
                {
                    try
                    {
                        Socket newSocket = e.AcceptSocket;
                        Task.Run(() =>
                        {
                            PreviewCreateSocketCliect(newSocket, this.bufferQueueGroups[this.SocketClients.Count % this.bufferQueueGroups.Length]);
                        });
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
    }
}