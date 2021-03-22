//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.Log;
using RRQMCore.Pool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RRQMSocket
{
    /// <summary>
    /// TCP服务器
    /// </summary>
    public abstract class TcpService<T> : BaseSocket, ITcpService where T : TcpSocketClient, new()
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpService():this(new BytePool(1024 * 1024 * 1000, 1024 * 1024 * 20))
        {
           
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bytePool">内存池实例</param>
        public TcpService(BytePool bytePool)
        {
            this.Logger = new Log();
            this.IsCheckClientAlive = true;
            this.SocketClients = new SocketCliectCollection<T>();
            this.IDTokenFormat = "{0}-TCP";
            this.clientSocketQueue = new ConcurrentQueue<Socket>();
            this.BytePool = bytePool;
            this.SocketClientPool = new ObjectPool<T>();
            this.MaxCount = 10000;
        }

        /// <summary>
        /// 获取或设置分配IDToken的格式，
        /// 格式必须符合字符串格式，至少包含一个补位，
        /// 初始值为“{0}-TCP”
        /// </summary>
        public string IDTokenFormat { get { return this.SocketClients.IDTokenFormat; } set { this.SocketClients.IDTokenFormat = value; } }

        /// <summary>
        /// 获取绑定状态
        /// </summary>
        public bool IsBind { get; private set; }

        /// <summary>
        /// 检验客户端活性（避免异常而导致的失活）
        /// </summary>
        public bool IsCheckClientAlive { get; set; }

        private int maxCount;

        /// <summary>
        /// 最大可连接数
        /// </summary>
        public int MaxCount
        {
            get { return maxCount; }
            set
            {
                this.SocketClientPool.Capacity = value;

                if (this.bufferQueueGroups != null && this.bufferQueueGroups.Length > 0)
                {
                    foreach (var item in this.bufferQueueGroups)
                    {
                        item.clientBufferPool.Capacity = value / this.bufferQueueGroups.Length;
                    }
                }
                maxCount = value;
            }
        }

        /// <summary>
        /// 获取内存池实例
        /// </summary>
        public BytePool BytePool { get; private set; }

        /// <summary>
        /// 获取对象池实例
        /// </summary>
        public ObjectPool<T> SocketClientPool { get; private set; }

        /// <summary>
        /// 获取当前连接的所有客户端
        /// </summary>
        public SocketCliectCollection<T> SocketClients { get; private set; }

        private BufferQueueGroup[] bufferQueueGroups;
        private ConcurrentQueue<Socket> clientSocketQueue;
        private Thread threadStartUpReceive;
        private Thread threadAccept;

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
        /// 绑定TCP服务
        /// </summary>
        /// <param name="setting"></param>
        /// <exception cref="RRQMException"></exception>
        public void Bind(BindSetting setting)
        {
            EndPoint endPoint = new IPEndPoint(IPAddress.Parse(setting.IP), setting.Port);
            this.Bind(endPoint, setting.MultithreadThreadCount);
        }

        /// <summary>
        /// 绑定TCP服务
        /// </summary>
        /// <param name="endPoint">节点</param>
        /// <param name="threadCount">多线程数量</param>
        /// <exception cref="RRQMException"></exception>
        public void Bind(EndPoint endPoint, int threadCount)
        {
            if (this.disposable)
            {
                throw new RRQMException("无法重新利用已释放对象");
            }
            if (threadCount < 1)
            {
                throw new RRQMException("逻辑线程数量不能小于1");
            }
            if (!IsBind)
            {
                try
                {
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Bind(endPoint);
                    this.MainSocket = socket;
                }
                catch (Exception e)
                {
                    throw new RRQMException(e.Message);
                }

                MainSocket.Listen(30);

                threadStartUpReceive = new Thread(StartUpReceive);
                threadStartUpReceive.IsBackground = true;
                threadStartUpReceive.Name = "启动接收消息线程";
                threadStartUpReceive.Start();

                threadAccept = new Thread(StartAccept);
                threadAccept.IsBackground = true;
                threadAccept.Name = "接收新客户端线程";
                threadAccept.Start();

                bufferQueueGroups = new BufferQueueGroup[threadCount];
                for (int i = 0; i < threadCount; i++)
                {
                    BufferQueueGroup bufferQueueGroup = new BufferQueueGroup();
                    bufferQueueGroups[i] = bufferQueueGroup;
                    bufferQueueGroup.Thread = new Thread(Handle);//处理用户的消息
                    bufferQueueGroup.clientBufferPool = new ObjectPool<ClientBuffer>(this.maxCount);//处理用户的消息
                    bufferQueueGroup.waitHandleBuffer = new AutoResetEvent(false);
                    bufferQueueGroup.bufferAndClient = new BufferQueue();
                    bufferQueueGroup.Thread.IsBackground = true;
                    bufferQueueGroup.Thread.Name = i + "号服务器处理线程";
                    bufferQueueGroup.Thread.Start(bufferQueueGroup);
                }
            }
            else
            {
                throw new RRQMException("重复绑定");
            }

            IsBind = true;
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
                    Socket clientCocket = this.MainSocket.Accept();
                    this.clientSocketQueue.Enqueue(clientCocket);
                }
                catch (Exception e)
                {
                    Logger.Debug(LogType.Error, this, e.Message);
                }
            }
        }

        private void StartUpReceive()
        {
            while (true)
            {
                Thread.Sleep(100);
                if (disposable)
                {
                    break;
                }
                else
                {
                    while (this.clientSocketQueue.Count > 0)
                    {
                        Socket socket;
                        if (this.clientSocketQueue.TryDequeue(out socket))
                        {
                            PreviewCreatSocketCliect(socket, this.bufferQueueGroups[this.SocketClients.Count % this.bufferQueueGroups.Length]);
                        }
                    }

                    ICollection<string> collection = this.SocketClients.GetTokens();
                    foreach (var token in collection)
                    {
                        T client = this.SocketClients[token];
                        if (client == null)
                        {
                            continue;
                        }
                        if (client.breakOut)
                        {
                            ClientDisconnectedMethod(client, new MesEventArgs("断开连接"));
                            client.Dispose();
                            this.SocketClientPool.DestroyObject(client);
                            this.SocketClients.Remove(token);
                        }
                        else if (this.IsCheckClientAlive)
                        {
                            client.SendOnline();
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
                    catch (Exception e)
                    {
                        Logger.Debug(LogType.Error, this, $"在处理数据时发生错误，信息：{e.Message}");
                    }
                    finally
                    {
                        queueGroup.clientBufferPool.DestroyObject(clientBuffer);
                        clientBuffer.byteBlock.Dispose();
                    }
                }
                else
                {
                    queueGroup.waitHandleBuffer.WaitOne();
                }
            }
        }

        /// <summary>
        /// 成功连接后创建（或从对象池中获得）辅助类,
        /// 用户可以在该方法中再进行自定义设置，
        /// 但是如果该对象是从对象池获得的话，为避免重复设定某些值，
        /// 例如事件等，请先判断NewCreat值再做处理。
        /// </summary>
        /// <param name="tcpSocketClient"></param>
        /// <param name="newCreat">true：首次创建。false：从对象池获得</param>
        protected abstract void OnCreatSocketCliect(T tcpSocketClient,bool newCreat);

        internal virtual void PreviewCreatSocketCliect(Socket socket, BufferQueueGroup queueGroup)
        {
            try
            {
                if (this.SocketClients.Count > this.maxCount)
                {
                    this.Logger.Debug(LogType.Error, this, "连接客户端数量已达到设定最大值");
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Dispose();
                    return;
                }

                T client = this.SocketClientPool.GetObject();
                if (client.NewCreat)
                {
                    client.queueGroup = queueGroup;
                    client.Service = this;
                    client.BytePool = this.BytePool;
                    client.Logger = this.Logger;
                }

                client.MainSocket = socket;
                client.BufferLength = this.BufferLength;
                OnCreatSocketCliect(client, client.NewCreat);

                client.BeginReceive();
                this.SocketClients.Add(client, client.NewCreat);
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
            base.Dispose();
            foreach (var item in this.SocketClients)
            {
                item.Dispose();
            }

            this.SocketClients.Clear();
            if (threadStartUpReceive != null)
            {
                threadStartUpReceive.Abort();
                threadStartUpReceive = null;
            }

            if (threadAccept != null)
            {
                threadAccept.Abort();
                threadAccept = null;
            }
            foreach (var item in bufferQueueGroups)
            {
                item.Dispose();
            }
        }
    }
}