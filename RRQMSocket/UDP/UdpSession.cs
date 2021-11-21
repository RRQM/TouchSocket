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
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RRQMSocket
{
    /// <summary>
    /// TCP服务器
    /// </summary>
    public abstract class UdpSession : BaseSocket, IService, IClient
    {
        private BufferQueueGroup[] bufferQueueGroups;
        private EndPoint defaultRemotePoint;
        private NetworkMonitor[] monitors;
        private string name;
        private long recivedCount;
        private Socket sendSocket;
        private bool separateThreadReceive;
        private ServerState serverState;
        private ServiceConfig serviceConfig;
        private bool study;

        /// <summary>
        /// 获取默认内存池
        /// </summary>
        public BytePool BytePool
        { get { return BytePool.Default; } }

        /// <summary>
        /// 默认远程节点
        /// </summary>
        public EndPoint DefaultRemotePoint
        {
            get { return defaultRemotePoint; }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public NetworkMonitor[] Monitors
        {
            get { return monitors; }
        }

        /// <summary>
        /// 服务器名称
        /// </summary>
        public string ServerName
        {
            get { return name; }
        }

        /// <summary>
        /// 获取服务器状态
        /// </summary>
        public ServerState ServerState
        {
            get { return serverState; }
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        public ServiceConfig ServiceConfig
        {
            get { return serviceConfig; }
        }

        /// <summary>
        /// 关闭服务器并释放服务器资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            this.Stop();
            if (this.bufferQueueGroups != null)
            {
                foreach (var item in bufferQueueGroups)
                {
                    item.Dispose();
                }
                bufferQueueGroups = null;
            }
            this.serverState = ServerState.Disposed;
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
            if (this.DefaultRemotePoint == null)
            {
                throw new RRQMException("默认终结点为空");
            }
            this.Send(this.defaultRemotePoint, buffer, offset, length);
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
        public virtual void SendAsync(byte[] buffer, int offset, int length)
        {
            this.SendAsync(this.defaultRemotePoint, buffer, offset, length);
        }

        /// <summary>
        /// IOCP发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public virtual void SendAsync(byte[] buffer)
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
        public virtual void SendAsync(ByteBlock byteBlock)
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
            this.sendSocket.SendTo(buffer, offset, length, SocketFlags.None, remoteEP);
        }

        /// <summary>
        /// 向设置的远程同步发送
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <param name="buffer"></param>
        public virtual void Send(EndPoint remoteEP, byte[] buffer)
        {
            this.sendSocket.SendTo(buffer, 0, buffer.Length, SocketFlags.None, remoteEP);
        }

        /// <summary>
        /// 向设置的远程同步发送
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <param name="byteBlock"></param>
        public virtual void Send(EndPoint remoteEP, ByteBlock byteBlock)
        {
            this.sendSocket.SendTo(byteBlock.Buffer, 0, byteBlock.Len, SocketFlags.None, remoteEP);
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
            this.sendSocket.BeginSendTo(buffer, offset, length, SocketFlags.None, remoteEP, null, null);
        }

        /// <summary>
        /// 向设置的远程异步发送
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <param name="buffer"></param>
        public virtual void SendAsync(EndPoint remoteEP, byte[] buffer)
        {
            this.sendSocket.BeginSendTo(buffer, 0, buffer.Length, SocketFlags.None, remoteEP, null, null);
        }

        /// <summary>
        /// 向设置的远程异步发送
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <param name="byteBlock"></param>
        public virtual void SendAsync(EndPoint remoteEP, ByteBlock byteBlock)
        {
            this.sendSocket.BeginSendTo(byteBlock.Buffer, 0, byteBlock.Len, SocketFlags.None, remoteEP, null, null);
        }
        #endregion 向设置的远程异步发送

        /// <summary>
        /// 配置服务
        /// </summary>
        /// <param name="serverConfig"></param>
        public IService Setup(ServiceConfig serverConfig)
        {
            this.serviceConfig = serverConfig;
            this.LoadConfig(this.serviceConfig);
            return this;
        }

        /// <summary>
        /// 通过端口配置
        /// </summary>
        /// <param name="port"></param>
        public IService Setup(int port)
        {
            UdpSessionConfig serverConfig = new UdpSessionConfig();
            serverConfig.ListenIPHosts = new IPHost[] { new IPHost(port) };
            return this.Setup(serverConfig);
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public IService Start()
        {
            if (this.serverState == ServerState.Disposed)
            {
                throw new RRQMException("无法重新利用已释放对象");
            }
            this.sendSocket = new Socket(SocketType.Dgram, ProtocolType.Udp);
            bool useBind = this.serviceConfig.GetValue<bool>(UdpSessionConfig.UseBindProperty);
            IPHost[] iPHosts = this.serviceConfig.GetValue<IPHost[]>(ServiceConfig.ListenIPHostsProperty);
            if (iPHosts == null || iPHosts.Length == 0)
            {
                throw new RRQMException("ListenIPHosts为空，无法绑定");
            }

            switch (this.serverState)
            {
                case ServerState.None:
                    {
                        if (useBind)
                        {
                            this.BeginReceive(iPHosts);
                            BeginThread();
                        }

                        break;
                    }
                case ServerState.Running:
                    break;

                case ServerState.Stopped:
                    {
                        if (useBind)
                        {
                            this.BeginReceive(iPHosts);
                        }
                        break;
                    }
                case ServerState.Disposed:
                    {
                        throw new RRQMException("无法再次利用已释放对象");
                    }
            }

            this.serverState = ServerState.Running;
            return this;
        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        public IService Stop()
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
            this.serverState = ServerState.Stopped;
            return this;
        }

        /// <summary>
        /// 处理已接收到的数据
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="byteBlock"></param>
        protected abstract void HandleReceivedData(EndPoint remoteEndPoint, ByteBlock byteBlock);

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="serverConfig"></param>
        protected virtual void LoadConfig(ServiceConfig serverConfig)
        {
            if (serverConfig == null)
            {
                throw new RRQMException("配置文件为空");
            }
            this.logger = serverConfig.Logger;
            this.defaultRemotePoint = (EndPoint)serverConfig.GetValue(UdpSessionConfig.DefaultRemotePointProperty);
            this.BufferLength = serverConfig.BufferLength;
            this.name = serverConfig.ServerName;
            this.separateThreadReceive = (bool)serverConfig.GetValue(UdpSessionConfig.SeparateThreadReceiveProperty);
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

        private void BeginReceive(IPHost[] iPHosts)
        {
            List<NetworkMonitor> networkMonitors = new List<NetworkMonitor>();
            foreach (var iPHost in iPHosts)
            {
                try
                {
                    Socket socket = new Socket(iPHost.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                    PreviewBind(socket);
                    socket.Bind(iPHost.EndPoint);

                    SocketAsyncEventArgs eventArg = new SocketAsyncEventArgs();
                    eventArg.Completed += this.IO_Completed;
                    ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                    eventArg.UserToken = byteBlock;
                    eventArg.SetBuffer(byteBlock.Buffer, 0, byteBlock.Capacity);
                    eventArg.RemoteEndPoint = iPHost.EndPoint;
                    if (!socket.ReceiveFromAsync(eventArg))
                    {
                        ProcessReceive(socket, eventArg);
                    }
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
        }

        private void BeginThread()
        {
            bufferQueueGroups = new BufferQueueGroup[serviceConfig.ThreadCount];
            for (int i = 0; i < serviceConfig.ThreadCount; i++)
            {
                BufferQueueGroup bufferQueueGroup = new BufferQueueGroup();
                bufferQueueGroups[i] = bufferQueueGroup;
                bufferQueueGroup.bytePool = new BytePool(this.serviceConfig.BytePoolMaxSize, this.serviceConfig.BytePoolMaxBlockSize);
                bufferQueueGroup.waitHandleBuffer = new AutoResetEvent(false);
                bufferQueueGroup.bufferAndClient = new BufferQueue();

                if (this.separateThreadReceive)
                {
                    bufferQueueGroup.Thread = new Thread(Handle);//处理用户的消息
                    bufferQueueGroup.Thread.IsBackground = true;
                    bufferQueueGroup.Thread.Name = i + "-Num Handler";
                    bufferQueueGroup.Thread.Start(bufferQueueGroup);
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
                    this.HandleBuffer(clientBuffer.endPoint, clientBuffer.byteBlock);
                }
                else
                {
                    queueGroup.waitHandleBuffer.WaitOne();
                }
            }
        }

        private void HandleBuffer(EndPoint endPoint, ByteBlock byteBlock)
        {
            try
            {
                HandleReceivedData(endPoint, byteBlock);
            }
            catch (Exception e)
            {
                Logger.Debug(LogType.Error, this, $"在处理数据时发生错误，信息：{e.Message}");
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive((Socket)sender, e);
        }

        private void ProcessReceive(Socket socket, SocketAsyncEventArgs e)
        {
            if (!this.disposable)
            {
                if (e.SocketError == SocketError.Success)
                {
                    ByteBlock byteBlock = (ByteBlock)e.UserToken;
                    byteBlock.SetLength(e.BytesTransferred);

                    BufferQueueGroup queueGroup = this.bufferQueueGroups[++this.recivedCount % this.bufferQueueGroups.Length];

                    if (this.separateThreadReceive)
                    {
                        ClientBuffer clientBuffer = new ClientBuffer();
                        clientBuffer.endPoint = e.RemoteEndPoint;
                        clientBuffer.byteBlock = byteBlock;
                        queueGroup.bufferAndClient.Enqueue(clientBuffer);
                        queueGroup.waitHandleBuffer.Set();
                    }
                    else
                    {
                        this.HandleBuffer(e.RemoteEndPoint, byteBlock);
                    }

                    ByteBlock newByteBlock = queueGroup.bytePool.GetByteBlock(this.BufferLength);
                    e.UserToken = newByteBlock;
                    e.SetBuffer(newByteBlock.Buffer, 0, newByteBlock.Buffer.Length);
                    if (!socket.ReceiveFromAsync(e))
                    {
                        ProcessReceive(socket, e);
                    }
                }
            }
        }
    }
}