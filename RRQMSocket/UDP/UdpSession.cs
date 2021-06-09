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
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RRQMSocket
{
    /// <summary>
    /// TCP服务器
    /// </summary>
    public abstract class UdpSession : BaseSocket, IService, IClient, IHandleBuffer
    {
        private EndPoint defaultRemotePoint;
        /// <summary>
        /// 默认远程节点
        /// </summary>
        public EndPoint DefaultRemotePoint
        {
            get { return defaultRemotePoint; }
        }

        /// <summary>
        /// 已接收数据次数
        /// </summary>
        public long RecivedCount { get { return this.recivedCount; } }


        private ServerState serverState;
        /// <summary>
        /// 获取服务器状态
        /// </summary>
        public ServerState ServerState
        {
            get { return serverState; }
        }


        /// <summary>
        /// 获取默认内存池
        /// </summary>
        public BytePool BytePool { get { return BytePool.Default; } }

        private ServerConfig serverConfig;
        /// <summary>
        /// 获取配置
        /// </summary>
        public ServerConfig ServerConfig
        {
            get { return serverConfig; }
        }

        private BufferQueueGroup[] bufferQueueGroups;
        private SocketAsyncEventArgs recviveEventArg;
        private long recivedCount;

        /// <summary>
        /// 在Socket初始化对象后，Bind之前调用。
        /// 可用于设置Socket参数。
        /// 父类方法可覆盖。
        /// </summary>
        /// <param name="socket"></param>
        protected virtual void PreviewBind(Socket socket)
        {
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.ReceiveFrom)
            {
                ProcessReceive(e);
            }
            else if (e.LastOperation == SocketAsyncOperation.SendTo)
            {
                ProcessSend(e);
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (!this.disposable)
            {
                if (this.recviveEventArg.SocketError == SocketError.Success)
                {
                    ByteBlock byteBlock = (ByteBlock)e.UserToken;
                    byteBlock.Position = e.BytesTransferred;
                    byteBlock.SetLength(e.BytesTransferred);

                    BufferQueueGroup queueGroup = this.bufferQueueGroups[++this.recivedCount % this.bufferQueueGroups.Length];
                    ClientBuffer clientBuffer = new ClientBuffer();
                    clientBuffer.endPoint = e.RemoteEndPoint;
                    clientBuffer.byteBlock = byteBlock;
                    queueGroup.bufferAndClient.Enqueue(clientBuffer);
                    queueGroup.waitHandleBuffer.Set();
                    ByteBlock newByteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                    e.UserToken = newByteBlock;
                    e.SetBuffer(newByteBlock.Buffer, 0, newByteBlock.Buffer.Length);
                    if (!this.MainSocket.ReceiveFromAsync(this.recviveEventArg))
                    {
                        ProcessReceive(e);
                    }
                }
            }
        }

        /// <summary>
        /// 发送完成时处理函数
        /// </summary>
        /// <param name="e">与发送完成操作相关联的SocketAsyncEventArg对象</param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                e.Dispose();
            }
            else
            {
                this.Logger.Debug(LogType.Error, this, "异步发送错误。");
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
                        HandleBuffer(clientBuffer);
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

        #region 发送
        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="remoteEP"></param>
        public void SendTo(byte[] buffer, int offset, int length, EndPoint remoteEP)
        {
            this.MainSocket.SendTo(buffer, offset, length, SocketFlags.None, remoteEP);
        }

        /// <summary>
        /// 处理已接收到的数据
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="byteBlock"></param>
        protected abstract void HandleReceivedData(EndPoint remoteEndPoint, ByteBlock byteBlock);

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
            this.SendTo(buffer, offset, length, this.defaultRemotePoint);
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
            this.Send(byteBlock.Buffer, 0, (int)byteBlock.Length);
        }

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
            SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
            sendEventArgs.Completed += this.IO_Completed;
            sendEventArgs.SetBuffer(buffer, offset, length);
            sendEventArgs.RemoteEndPoint = this.defaultRemotePoint;

            if (!this.MainSocket.SendToAsync(sendEventArgs))
            {
                this.ProcessSend(sendEventArgs);
            }
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
            this.SendAsync(byteBlock.Buffer, 0, (int)byteBlock.Length);
        }

        #endregion

        private void HandleBuffer(ClientBuffer clientBuffer)
        {
            HandleReceivedData(clientBuffer.endPoint, clientBuffer.byteBlock);
        }

        void IHandleBuffer.HandleBuffer(ClientBuffer clientBuffer)
        {
        }

        /// <summary>
        /// 配置服务
        /// </summary>
        /// <param name="serverConfig"></param>
        public void Setup(ServerConfig serverConfig)
        {
            this.serverConfig = serverConfig;
            this.LoadConfig(this.serverConfig);
        }

        /// <summary>
        /// 通过端口配置
        /// </summary>
        /// <param name="port"></param>
        public void Setup(int port)
        {
            UdpSessionConfig serverConfig = new UdpSessionConfig();
            serverConfig.BindIPHost = new IPHost(port);
            this.Setup(serverConfig);
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
            this.defaultRemotePoint = (EndPoint)serverConfig.GetValue(UdpSessionConfig.DefaultRemotePointProperty);
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public void Start()
        {
            if (this.serverState == ServerState.Disposed)
            {
                throw new RRQMException("无法重新利用已释放对象");
            }
           
            bool useBind = (bool)this.serverConfig.GetValue(UdpSessionConfig.UseBindProperty);
            if (useBind)
            {
                IPHost iPHost = (IPHost)this.serverConfig.GetValue(ServerConfig.BindIPHostProperty);
                if (iPHost == null)
                {
                    throw new RRQMException("IPHost为空，无法绑定");
                }
                if (this.serverState == ServerState.None || this.serverState == ServerState.Stopped)
                {
                    try
                    {
                        Socket socket = new Socket(iPHost.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                        PreviewBind(socket);
                        socket.Bind(iPHost.EndPoint);
                        this.MainSocket = socket;

                        this.recviveEventArg = new SocketAsyncEventArgs();
                        this.recviveEventArg.Completed += this.IO_Completed;
                        ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                        this.recviveEventArg.UserToken = byteBlock;
                        this.recviveEventArg.SetBuffer(byteBlock.Buffer, 0, byteBlock.Buffer.Length);
                        this.recviveEventArg.RemoteEndPoint = iPHost.EndPoint;
                        this.MainSocket.ReceiveFromAsync(this.recviveEventArg);
                    }
                    catch (Exception e)
                    {
                        throw new RRQMException(e.Message);
                    }

                    bufferQueueGroups = new BufferQueueGroup[serverConfig.ThreadCount];
                    for (int i = 0; i < serverConfig.ThreadCount; i++)
                    {
                        BufferQueueGroup bufferQueueGroup = new BufferQueueGroup();
                        bufferQueueGroups[i] = bufferQueueGroup;
                        bufferQueueGroup.Thread = new Thread(Handle);//处理用户的消息
                        bufferQueueGroup.waitHandleBuffer = new AutoResetEvent(false);
                        bufferQueueGroup.bufferAndClient = new BufferQueue();
                        bufferQueueGroup.Thread.IsBackground = true;
                        bufferQueueGroup.Thread.Name = i + "-Num Handler";
                        bufferQueueGroup.Thread.Start(bufferQueueGroup);
                    }
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

            foreach (var item in bufferQueueGroups)
            {
                item.Dispose();
            }
            this.serverState = ServerState.Stopped;
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