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
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// TCP客户端
    /// </summary>
    public class TcpClient : BaseSocket, IUserTcpClient, IClient, IHandleBuffer
    {
        /// <summary>
        /// 判断是否已连接
        /// </summary>
        public virtual bool Online { get { return MainSocket == null ? false : MainSocket.Connected; } }

        private DataHandlingAdapter dataHandlingAdapter;

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        public DataHandlingAdapter DataHandlingAdapter
        {
            get { return dataHandlingAdapter; }
            set { this.SetDataHandlingAdapter(value); }
        }

        private BytePool bytePool;
        /// <summary>
        /// 获取内存池实例
        /// </summary>
        public BytePool BytePool
        {
            get { return bytePool; }
        }


        private bool onlySend;
        /// <summary>
        /// 仅发送，即不会开启接收线程。
        /// </summary>
        public bool OnlySend
        {
            get { return onlySend; }
        }
        /// <summary>
        /// 客户端配置
        /// </summary>
        protected TcpClientConfig clientConfig;
        /// <summary>
        /// 客户端配置
        /// </summary>
        public TcpClientConfig ClientConfig
        {
            get { return clientConfig; }
        }


        //private bool useSeparateThreadSend;
        ///// <summary>
        ///// 在异步发送时，使用独立线程发送
        ///// </summary>
        //public bool UseSeparateThreadSend
        //{
        //    get { return useSeparateThreadSend; }
        //    set { useSeparateThreadSend = value; }
        //}


        private BufferQueueGroup queueGroup;
        private SocketAsyncEventArgs receiveEventArgs;
        /// <summary>
        /// 标识是否处于断开连接
        /// </summary>
        protected bool disconnect;

        /// <summary>
        /// 成功连接到服务器
        /// </summary>
        public event RRQMMessageEventHandler ConnectedService;

        /// <summary>
        /// 断开连接
        /// </summary>
        public event RRQMMessageEventHandler DisconnectedService;

        /// <summary>
        /// 处理数据
        /// </summary>
        public event Action<TcpClient, ByteBlock, object> OnReceived;

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="clientConfig"></param>
        /// <exception cref="RRQMException"></exception>
        public void Setup(TcpClientConfig clientConfig)
        {
            this.clientConfig = clientConfig;
            this.LoadConfig(this.clientConfig);
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="clientConfig"></param>
        protected virtual void LoadConfig(TcpClientConfig clientConfig)
        {
            if (clientConfig == null)
            {
                throw new RRQMException("配置文件为空");
            }
            if (clientConfig.BytePool != null)
            {
                clientConfig.BytePool.MaxSize = clientConfig.BytePoolMaxSize;
                clientConfig.BytePool.MaxBlockSize = clientConfig.BytePoolMaxBlockSize;
                this.bytePool = clientConfig.BytePool;
            }
            else
            {
                throw new ArgumentNullException("内存池不能为空");
            }
            this.Logger = (ILog)clientConfig.GetValue(RRQMConfig.LoggerProperty);
            this.BufferLength = (int)clientConfig.GetValue(RRQMConfig.BufferLengthProperty);
            this.SetDataHandlingAdapter(clientConfig.DataHandlingAdapter);
            this.onlySend = clientConfig.OnlySend;

        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        public virtual void Connect()
        {
            if (this.clientConfig == null)
            {
                throw new ArgumentNullException("配置文件不能为空。");
            }
            IPHost iPHost = this.clientConfig.RemoteIPHost;
            if (iPHost == null)
            {
                throw new ArgumentNullException("iPHost不能为空。");
            }
            if (!this.Online)
            {
                try
                {
                    Socket socket = new Socket(iPHost.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    PreviewConnect(socket);
                    socket.Connect(iPHost.EndPoint);
                    this.MainSocket = socket;
                    Start();
                }
                catch (Exception e)
                {
                    throw new RRQMException(e.Message);
                }
            }
        }

        /// <summary>
        /// 异步连接服务器
        /// </summary>
        /// <param name="callback"></param>
        public async void ConnectAsync(Action<AsyncResult> callback = null)
        {
            await Task.Run(() =>
            {
                try
                {
                    this.Connect();
                    if (callback != null)
                    {
                        AsyncResult result = new AsyncResult();
                        result.Status = true;
                        callback.Invoke(result);
                    }
                }
                catch (Exception ex)
                {
                    if (callback != null)
                    {
                        AsyncResult result = new AsyncResult();
                        result.Status = false;
                        result.Message = ex.Message;
                        callback.Invoke(result);
                    }
                }

            });
        }

        /// <summary>
        /// 在Socket初始化对象后，Connect之前调用。
        /// 可用于设置Socket参数。
        /// 父类方法可覆盖。
        /// </summary>
        /// <param name="socket"></param>
        protected virtual void PreviewConnect(Socket socket)
        {
        }

        private void SetDataHandlingAdapter(DataHandlingAdapter adapter)
        {
            if (adapter == null)
            {
                throw new RRQMException("数据处理适配器为空");
            }
            adapter.BytePool = this.bytePool;
            adapter.Logger = this.Logger;
            adapter.ReceivedCallBack = this.HandleReceivedData;
            adapter.SendCallBack = this.Sent;
            this.dataHandlingAdapter = adapter;
        }
        internal void Start()
        {
            this.ReadIpPort();
            if (!this.onlySend)
            {
                queueGroup = new BufferQueueGroup();
                queueGroup.Thread = new Thread(HandleBuffer);//处理用户的消息
                queueGroup.waitHandleBuffer = new AutoResetEvent(false);
                queueGroup.bufferAndClient = new BufferQueue();
                queueGroup.clientBufferPool = new RRQMCore.Pool.ObjectPool<ClientBuffer>();
                queueGroup.Thread.IsBackground = true;
                queueGroup.Thread.Name = "客户端处理线程";
                queueGroup.Thread.Start();

                this.receiveEventArgs = new SocketAsyncEventArgs();
                this.receiveEventArgs.Completed += EventArgs_Completed;
                BeginReceive();
                //this.SeparateThreadSend();
            }
            this.ConnectedService?.Invoke(this, new MesEventArgs("SuccessConnection"));
        }

        /// <summary>
        /// 启动消息接收
        /// </summary>
        private void BeginReceive()
        {
            try
            {
                ByteBlock byteBlock = this.bytePool.GetByteBlock(this.BufferLength);
                this.receiveEventArgs.UserToken = byteBlock;
                this.receiveEventArgs.SetBuffer(byteBlock.Buffer, 0, byteBlock.Buffer.Length);
                if (!this.MainSocket.ReceiveAsync(this.receiveEventArgs))
                {
                    ProcessReceived(this.receiveEventArgs);
                }
            }
            catch (Exception ex)
            {
                this.DisconnectedService?.Invoke(this, new MesEventArgs(ex.Message));
            }
        }

        private void EventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.LastOperation == SocketAsyncOperation.Receive)
                {
                    ProcessReceived(e);
                }
                else if (e.LastOperation == SocketAsyncOperation.Send)
                {
                    ProcessSend(e);
                }
                else
                {
                    this.DisconnectedService?.Invoke(this, new MesEventArgs("BreakOut"));
                }
            }
            catch (Exception ex)
            {
                this.DisconnectedService?.Invoke(this, new MesEventArgs(ex.Message));
            }
        }

        private void ProcessReceived(SocketAsyncEventArgs e)
        {
            if (!this.disposable)
            {
                if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
                {
                    ByteBlock byteBlock = (ByteBlock)e.UserToken;

                    if (byteBlock.Using)
                    {
                        byteBlock.Position = e.BytesTransferred;
                        byteBlock.SetLength(e.BytesTransferred);
                    }
                    else
                    {
                        byte[] buffer = new byte[e.BytesTransferred];
                        Array.Copy(byteBlock.Buffer, buffer, buffer.Length);
                        while (!byteBlock.Using)
                        {
                            byteBlock.Dispose();
                            byteBlock = this.bytePool.GetByteBlock(this.BufferLength);
                        }
                        byteBlock.Write(buffer);
                    }

                    ClientBuffer clientBuffer = this.queueGroup.clientBufferPool.GetObject();
                    clientBuffer.client = this;
                    clientBuffer.byteBlock = byteBlock;
                    queueGroup.bufferAndClient.Enqueue(clientBuffer);
                    queueGroup.waitHandleBuffer.Set();

                    ByteBlock newByteBlock = this.bytePool.GetByteBlock(this.BufferLength);
                    e.UserToken = newByteBlock;
                    e.SetBuffer(newByteBlock.Buffer, 0, newByteBlock.Buffer.Length);

                    if (!this.MainSocket.ReceiveAsync(e))
                    {
                        ProcessReceived(e);
                    }
                }
                else
                {
                    this.DisconnectedService?.Invoke(this, new MesEventArgs("BreakOut"));
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

        private void HandleBuffer()
        {
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
                        this.queueGroup.clientBufferPool.DestroyObject(clientBuffer);
                    }
                }
                else
                {
                    queueGroup.waitHandleBuffer.WaitOne();
                }
            }
        }

        /// <summary>
        /// 处理已接收到的数据。
        /// 覆盖父类方法将不触发OnReceived事件。
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected virtual void HandleReceivedData(ByteBlock byteBlock, object obj)
        {
            this.OnReceived?.Invoke(this, byteBlock, obj);
        }

        ///// <summary>
        ///// 在异步发送时，使用独立线程发送
        ///// </summary>
        //private void SeparateThreadSend()
        //{
        //    if (this.useSeparateThreadSend)
        //    {
        //        if (this.asyncSender != null)
        //        {
        //            this.asyncSender.Dispose();
        //        }
        //        this.asyncSender = new AsyncSender();
        //        this.asyncSender.Load(this.MainSocket, this.MainSocket.RemoteEndPoint);
        //    }
        //    else
        //    {
        //        if (this.asyncSender != null)
        //        {
        //            this.asyncSender.Dispose();
        //            this.asyncSender = null;
        //        }
        //    }
        //}

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public virtual void Send(byte[] buffer)
        {
            this.Send(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 发送流中的有效数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public virtual void Send(ByteBlock byteBlock)
        {
            this.Send(byteBlock.Buffer, 0, (int)byteBlock.Length);
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public virtual void Send(byte[] buffer, int offset, int length)
        {
            this.dataHandlingAdapter.Send(buffer, offset, length, false);
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
            this.dataHandlingAdapter.Send(buffer, offset, length, true);
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

        private void Sent(byte[] buffer, int offset, int length, bool isAsync)
        {
            if (!this.Online)
            {
                throw new RRQMNotConnectedException("该实例已断开");
            }

            if (isAsync)
            {
                SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
                sendEventArgs.Completed += EventArgs_Completed;
                sendEventArgs.SetBuffer(buffer, offset, length);
                sendEventArgs.RemoteEndPoint = this.MainSocket.RemoteEndPoint;
                if (!this.MainSocket.SendAsync(sendEventArgs))
                {
                    // 同步发送时处理发送完成事件
                    this.ProcessSend(sendEventArgs);
                }
            }
            else
            {
                int r = 0;
                while (length > 0)
                {
                    r = MainSocket.Send(buffer, offset, length, SocketFlags.None);
                    if (r == 0 && length > 0)
                    {
                        throw new RRQMException("发送数据不完全");
                    }
                    offset += r;
                    length -= r;
                }
            }

        }

        /// <summary>
        /// 禁用发送或接收
        /// </summary>
        /// <param name="how"></param>
        public void Shutdown(SocketShutdown how)
        {
            if (this.MainSocket != null)
            {
                MainSocket.Shutdown(how);
            }
        }

        /// <summary>
        /// 断开链接并释放资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            if (this.queueGroup != null)
            {
                this.queueGroup.Dispose();
            }
        }

        void IHandleBuffer.HandleBuffer(ClientBuffer clientBuffer)
        {
            if (this.dataHandlingAdapter == null)
            {
                throw new RRQMException("数据处理适配器为空");
            }
            this.dataHandlingAdapter.Received(clientBuffer.byteBlock);
        }


    }
}