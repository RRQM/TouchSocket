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
using RRQMCore.Run;
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
    public abstract class TcpService<T> : BaseSocket, IService where T : TcpSocketClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpService()
        {
            this.IsCheckClientAlive = true;
            this.SocketClients = new SocketCliectCollection<T>();
            this.clientSocketQueue = new ConcurrentQueue<Socket>();
            this.BytePool = new BytePool(1024 * 1024 * 1000, 1024 *1024 *20);
        }

        /// <summary>
        /// 获取绑定状态
        /// </summary>
        public bool IsBind { get; private set; }

        /// <summary>
        /// 检验客户端活性（避免异常而导致的失活）
        /// </summary>
        public bool IsCheckClientAlive { get; set; }

        /// <summary>
        /// 获取内存池实例
        /// </summary>
        public BytePool BytePool { get; private set; }

        /// <summary>
        /// 获取当前连接的所有客户端
        /// </summary>
        public SocketCliectCollection<T> SocketClients { get; private set; }

        private BufferQueueGroup[] bufferQueueGroups;
        private ConcurrentQueue<Socket> clientSocketQueue;
        private Thread threadStartUpReceive;
        #region 事件

        /// <summary>
        /// 有用户连接的时候
        /// </summary>
        public event RRQMShowMesEventHandler ClientConnected;

        /// <summary>
        /// 有用户断开连接的时候
        /// </summary>
        public event RRQMShowMesEventHandler ClientDisconnected;

        private void ClientConnectedMethod(object sender, MesEventArgs e)
        {
            ClientConnected?.Invoke(sender, e);
        }

        private void ClientDisconnectedMethod(object sender, MesEventArgs e)
        {
            ClientDisconnected?.Invoke(sender, e);
        }

        #endregion 事件

        /// <summary>
        /// 绑定TCP服务
        /// </summary>
        /// <param name="setting"></param>
        public void Bind(BindSetting setting)
        {
            MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            if (!IsBind)
            {
                disposable = false;
                EndPoint endPoint = new IPEndPoint(IPAddress.Parse(setting.IP), setting.Port);
                try
                {
                    MainSocket.Bind(endPoint);
                    this.IP = setting.IP;
                    this.Port = setting.Port;
                }
                catch (Exception e)
                {
                    throw new RRQMException(e.Message);
                }

                MainSocket.Listen(30);
                MainSocket.BeginAccept(new AsyncCallback(AcceptSocket), null);

                threadStartUpReceive = new Thread(StartUpReceive);
                threadStartUpReceive.IsBackground = true;
                threadStartUpReceive.Name = "启动接收消息线程";
                threadStartUpReceive.Start();

                bufferQueueGroups = new BufferQueueGroup[setting.MultithreadThreadCount];
                for (int i = 0; i < setting.MultithreadThreadCount; i++)
                {
                    BufferQueueGroup bufferQueueGroup = new BufferQueueGroup();
                    bufferQueueGroups[i] = bufferQueueGroup;
                    bufferQueueGroup.Thread = new Thread(Handle);//处理用户的消息
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

        private void AcceptSocket(IAsyncResult ar)
        {
            if (!this.disposable)
            {
                try
                {
                    Socket clientCocket = this.MainSocket.EndAccept(ar);
                    this.clientSocketQueue.Enqueue(clientCocket);
                    this.MainSocket.BeginAccept(new AsyncCallback(AcceptSocket), null);
                }
                catch (Exception e)
                {
                    Logger.Debug(LogType.Error, this, e.Message);
                }
            }
        }

        /// <summary>
        /// 单线程启动接收信息并检验活性
        /// </summary>
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
                            try
                            {
                                T client = CreatSocketCliect();
                                if (client != null)
                                {
                                    client.queueGroup = this.bufferQueueGroups[this.SocketClients.Count % this.bufferQueueGroups.Length];
                                    client.MainSocket = socket;
                                    client.BufferLength = this.BufferLength;
                                    client.Service = this;
                                    client.Initialize();
                                    client.BeginReceive();
                                    this.SocketClients.Add(client);
                                    ClientConnectedMethod(client, null);
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Debug(LogType.Error, this, $"在接收客户端时发生错误，信息：{e.Message}");
                            }
                           
                        }
                    }


                    int length = this.SocketClients.Count;
                    for (int i = 0; i < length; i++)
                    {
                        TcpSocketClient client = this.SocketClients[i];
                        if (client.breakOut)
                        {
                            this.SocketClients.RemoveAt(i);
                            i--;
                            length--;
                            ClientDisconnectedMethod(client, new MesEventArgs("断开连接"));
                            client.Dispose();
                        }
                        else if (this.IsCheckClientAlive)
                        {
                            this.SocketClients[i].SendOnline();
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
                        clientBuffer.client.HandleBuffer(clientBuffer.byteBlock);
                    }
                    catch (Exception e)
                    {
                        Logger.Debug(LogType.Error, this, $"在处理数据时发生错误，信息：{e.Message}");
                    }
                    finally
                    {
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
        /// 成功连接后创建辅助类
        /// </summary>
        protected abstract T CreatSocketCliect();

        /// <summary>
        /// 关闭服务器并释放服务器资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            for (int i = 0; i < this.SocketClients.Count; i++)
            {
                this.SocketClients[i].Dispose();
            }
            this.SocketClients.Clear();
            if (threadStartUpReceive != null)
            {
                threadStartUpReceive.Abort();
                threadStartUpReceive = null;
            }
            foreach (var item in bufferQueueGroups)
            {
                item.Thread.Abort();
            }
        }
    }
}