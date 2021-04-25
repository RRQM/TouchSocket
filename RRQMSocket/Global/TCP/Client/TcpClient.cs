//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.Log;

namespace RRQMSocket
{
    /// <summary>
    /// TCP客户端
    /// </summary>
    public abstract class TcpClient : BaseSocket, IUserTcpClient, IHandleBuffer
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpClient() : this(new BytePool(1024 * 1024 * 1000, 1024 * 1024 * 20))
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bytePool">设置内存池实例</param>
        public TcpClient(BytePool bytePool) : base(bytePool)
        {
            this.DataHandlingAdapter = new NormalDataHandlingAdapter();
        }

        /// <summary>
        /// 判断是否已连接
        /// </summary>
        public bool Online { get { return MainSocket == null ? false : MainSocket.Connected; } }

        private DataHandlingAdapter dataHandlingAdapter;

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        public DataHandlingAdapter DataHandlingAdapter
        {
            get { return dataHandlingAdapter; }
            set
            {
                dataHandlingAdapter = value;
                if (dataHandlingAdapter != null)
                {
                    dataHandlingAdapter.BytePool = this.BytePool;
                    dataHandlingAdapter.Logger = this.Logger;
                    dataHandlingAdapter.ReceivedCallBack = this.HandleReceivedData;
                    dataHandlingAdapter.SendCallBack = this.Sent;
                }
            }
        }

        private BufferQueueGroup queueGroup;
        private SocketAsyncEventArgs eventArgs;

        /// <summary>
        /// 成功连接到服务器
        /// </summary>
        public event RRQMMessageEventHandler ConnectedService;

        /// <summary>
        /// 断开连接
        /// </summary>
        public event RRQMMessageEventHandler DisconnectedService;

        private void ConnectedServiceMethod(object sender, MesEventArgs e)
        {
            ConnectedService?.Invoke(sender, e);
        }

        private void DisconnectedServiceMethod(object sender, MesEventArgs e)
        {
            DisconnectedService?.Invoke(sender, e);
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="iPHost">ip和端口号，格式如“127.0.0.1:7789”。IP可输入Ipv6</param>
        public void Connect(IPHost iPHost)
        {
            if (iPHost == null)
            {
                throw new ArgumentNullException("iPHost不能为空。");
            }
            this.Connect(iPHost.AddressFamily,iPHost.EndPoint);
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="addressFamily">寻址方案，支持IPv6</param>
        /// <param name="endPoint">节点</param>
        /// <exception cref="RRQMException"></exception>
        public virtual void Connect(AddressFamily addressFamily, EndPoint endPoint)
        {
            if (this.disposable)
            {
                throw new RRQMException("无法重新利用已释放对象");
            }
            Socket socket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                PreviewConnect(socket);
                socket.Connect(endPoint);
                this.MainSocket = socket;
                Start();
            }
            catch (Exception e)
            {
                throw new RRQMException(e.Message);
            }
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

        internal void Start()
        {
            queueGroup = new BufferQueueGroup();
            queueGroup.Thread = new Thread(HandleBuffer);//处理用户的消息
            queueGroup.waitHandleBuffer = new AutoResetEvent(false);
            queueGroup.bufferAndClient = new BufferQueue();
            queueGroup.clientBufferPool = new RRQMCore.Pool.ObjectPool<ClientBuffer>();
            queueGroup.Thread.IsBackground = true;
            queueGroup.Thread.Name = "客户端处理线程";
            queueGroup.Thread.Start();
            this.eventArgs = new SocketAsyncEventArgs();
            this.eventArgs.Completed += EventArgs_Completed;
            BeginReceive();
            ConnectedServiceMethod(this, new MesEventArgs("SuccessConnection"));
        }

        /// <summary>
        /// 启动消息接收
        /// </summary>
        private void BeginReceive()
        {
            try
            {
                ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                this.eventArgs.UserToken = byteBlock;
                this.eventArgs.SetBuffer(byteBlock.Buffer, 0, byteBlock.Buffer.Length);
                if (!this.MainSocket.ReceiveAsync(this.eventArgs))
                {
                    ProcessReceived(this.eventArgs);
                }
            }
            catch
            {
                DisconnectedServiceMethod(this, new MesEventArgs("BreakOut"));
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
                else
                {
                    DisconnectedServiceMethod(this, new MesEventArgs("BreakOut"));
                }
            }
            catch (Exception ex)
            {
                DisconnectedServiceMethod(this, new MesEventArgs(ex.Message));
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
                            byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                        }
                        byteBlock.Write(buffer);
                    }

                    ClientBuffer clientBuffer = this.queueGroup.clientBufferPool.GetObject();
                    clientBuffer.client = this;
                    clientBuffer.byteBlock = byteBlock;
                    queueGroup.bufferAndClient.Enqueue(clientBuffer);
                    queueGroup.waitHandleBuffer.Set();

                    ByteBlock newByteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                    e.UserToken = newByteBlock;
                    e.SetBuffer(newByteBlock.Buffer, 0, newByteBlock.Buffer.Length);

                    if (!this.MainSocket.ReceiveAsync(e))
                    {
                        ProcessReceived(e);
                    }
                }
                else
                {
                    DisconnectedServiceMethod(this, new MesEventArgs("BreakOut"));
                }
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
                        clientBuffer.byteBlock.Dispose();
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
        /// 处理已接收到的数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected abstract void HandleReceivedData(ByteBlock byteBlock, object obj);

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
            this.dataHandlingAdapter.Send(buffer, offset, length);
        }

        private void Sent(byte[] buffer, int offset, int length)
        {
            if (!this.Online)
            {
                throw new RRQMNotConnectedException("该实例已断开");
            }
            try
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
            catch (Exception e)
            {
                throw new RRQMException(e.Message);
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