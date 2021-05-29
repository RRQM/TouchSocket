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
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RRQMSocket
{
    /// <summary>
    /// TCP服务器
    /// </summary>
    public class TcpService<TClient> : BaseSocket, ITcpService where TClient : TcpSocketClient, new()
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpService()
        {
            this.socketClients = new SocketCliectCollection<TClient>();
            this.socketClientPool = new ObjectPool<TClient>();
        }

        private int clearInterval;
        /// <summary>
        /// 获取清理无数据交互的SocketClient，默认60。如果不想清除，可使用-1。
        /// </summary>
        public int ClearInterval
        {
            get { return clearInterval; }
        }

        private int maxCount;
        /// <summary>
        /// 最大可连接数
        /// </summary>
        public int MaxCount
        {
            get { return maxCount; }
        }

        private SocketCliectCollection<TClient> socketClients;
        /// <summary>
        /// 获取当前连接的所有客户端
        /// </summary>
        public SocketCliectCollection<TClient> SocketClients
        {
            get { return socketClients; }
        }

        private ServerState serverState;
        /// <summary>
        /// 服务器状态
        /// </summary>
        public ServerState ServerState
        {
            get { return serverState; }
        }

        internal ObjectPool<TClient> socketClientPool;
        private BufferQueueGroup[] bufferQueueGroups;
        private Thread threadClearClient;
        private Thread threadAccept;
        private TcpServerConfig serverConfig;
        #region 事件

        /// <summary>
        /// 有用户连接的时候
        /// </summary>
        public event RRQMMessageEventHandler ClientConnected;

        /// <summary>
        /// 有用户断开连接的时候
        /// </summary>
        public event RRQMMessageEventHandler ClientDisconnected;

        /// <summary>
        /// 创建泛型T时
        /// </summary>
        public event Action<TClient, CreateOption> CreatSocketCliect;

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
        /// 启动服务
        /// </summary>
        public void Start()
        {
            if (this.serverState == ServerState.Disposed)
            {
                throw new RRQMException("无法重新利用已释放对象");
            }
            if (this.serverState == ServerState.None || this.serverState == ServerState.Stopped)
            {
                try
                {
                    Socket socket = new Socket(this.serverConfig.IPHost.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    PreviewBind(socket);
                    socket.Bind(this.serverConfig.IPHost.EndPoint);
                    this.MainSocket = socket;
                }
                catch (Exception e)
                {
                    throw new RRQMException(e.Message);
                }

                MainSocket.Listen(this.serverConfig.Backlog);

                threadClearClient = new Thread(ClearClient);
                threadClearClient.IsBackground = true;
                threadClearClient.Name = "CheckAlive";
                threadClearClient.Start();

                threadAccept = new Thread(StartAccept);
                threadAccept.IsBackground = true;
                threadAccept.Name = "AcceptClient";
                threadAccept.Start();

                bufferQueueGroups = new BufferQueueGroup[this.serverConfig.ThreadCount];
                for (int i = 0; i < this.serverConfig.ThreadCount; i++)
                {
                    BufferQueueGroup bufferQueueGroup = new BufferQueueGroup();
                    bufferQueueGroup.bytePool = new BytePool(this.serverConfig.BytePoolMaxSize, this.serverConfig.BytePoolMaxBlockSize);
                    bufferQueueGroups[i] = bufferQueueGroup;
                    bufferQueueGroup.Thread = new Thread(Handle);//处理用户的消息
                    bufferQueueGroup.clientBufferPool = new ObjectPool<ClientBuffer>(this.maxCount);//处理用户的消息
                    bufferQueueGroup.waitHandleBuffer = new AutoResetEvent(false);
                    bufferQueueGroup.bufferAndClient = new BufferQueue();
                    bufferQueueGroup.Thread.IsBackground = true;
                    bufferQueueGroup.Thread.Name = i + "-Num Handler";
                    bufferQueueGroup.Thread.Start(bufferQueueGroup);
                }
            }
            this.serverState = ServerState.Running;
        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        public void Stop()
        {
            base.Dispose();
            foreach (var item in this.SocketClients)
            {
                item.Dispose();
            }

            this.SocketClients.Clear();

            foreach (var item in bufferQueueGroups)
            {
                item.Dispose();
            }

            this.serverState = ServerState.Stopped;
        }

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="serverConfig"></param>
        public void Setup(IServerConfig serverConfig)
        {
            if (serverConfig is TcpServerConfig config)
            {
                this.serverConfig = config;
            }
            else
            {
                throw new RRQMException($"适用于此处的配置应当继承自{nameof(TcpServerConfig)}");
            }
        }

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="port"></param>
        public void Setup(int port)
        {
            if (this.serverConfig == null)
            {
                this.serverConfig = new TcpServerConfig();
                this.serverConfig.IPHost = new IPHost(port);
            }
            else
            {

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
        /// 根据ID判断SocketClient是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool SocketClientExist(string id)
        {
            return this.SocketClients.SocketClientExist(id);
        }

        private void StartAccept()
        {
            while (true)
            {
                if (this.disposable)
                {
                    break;
                }
                try
                {
                    Socket socket = this.MainSocket.Accept();
                    PreviewCreatSocketCliect(socket, this.bufferQueueGroups[this.SocketClients.Count % this.bufferQueueGroups.Length]);
                }
                catch (Exception e)
                {
                    Logger.Debug(LogType.Error, this, e.Message);
                }
            }
        }

        private void ClearClient()
        {
            while (true)
            {
                Thread.Sleep(10);
                if (disposable)
                {
                    break;
                }
                else
                {
                    ICollection<string> collection = this.SocketClients.GetTokens();
                    foreach (var token in collection)
                    {
                        if (this.SocketClients.TryGetSocketClient(token, out TClient client))
                        {
                            if (client.breakOut)
                            {
                                ClientDisconnectedMethod(client, new MesEventArgs("断开连接"));
                                client.Dispose();
                                this.SocketClients.Remove(token);
                                this.socketClientPool.DestroyObject(client);
                            }
                            else
                            {
                                client.SendOnline();
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
                        queueGroup.clientBufferPool.DestroyObject(clientBuffer);
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

        /// <summary>
        /// 成功连接后创建（或从对象池中获得）辅助类,
        /// 用户可以在该方法中再进行自定义设置，
        /// 但是如果该对象是从对象池获得的话，为避免重复设定某些值，
        /// 例如事件等，请先判断CreatOption.NewCreat值再做处理。
        /// </summary>
        /// <param name="tcpSocketClient"></param>
        /// <param name="creatOption"></param>
        protected virtual void OnCreatSocketCliect(TClient tcpSocketClient, CreateOption creatOption)
        {
            CreatSocketCliect?.Invoke(tcpSocketClient, creatOption);
        }

        internal virtual void PreviewCreatSocketCliect(Socket socket, BufferQueueGroup queueGroup)
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
                }

                client.MainSocket = socket;
                client.BufferLength = this.BufferLength;

                CreateOption creatOption = new CreateOption();
                creatOption.NewCreate = client.NewCreate;
                if (client.NewCreate)
                {
                    creatOption.ID = this.SocketClients.GetDefaultID();
                }
                else
                {
                    creatOption.ID = client.ID;
                }
                OnCreatSocketCliect(client, creatOption);
                client.ID = creatOption.ID;

                this.SocketClients.Add(client);
                client.BeginReceive();
                ClientConnectedMethod(client, null);
            }
            catch (Exception e)
            {
                Logger.Debug(LogType.Error, this, $"在接收客户端时发生错误，信息：{e.Message}");
            }
        }

        /// <summary>
        /// 关闭服务器并释放服务器资源
        /// </summary>
        public override void Dispose()
        {
            this.Stop();
            this.serverState = ServerState.Disposed;
        }
    }
}