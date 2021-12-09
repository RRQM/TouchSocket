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
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// TCP客户端
    /// </summary>
    public abstract class TcpClient : BaseSocket, ITcpClient, IHandleBuffer
    {
        private AsyncSender asyncSender;
        private BytePool bytePool;
        private TcpClientConfig clientConfig;
        private DataHandlingAdapter dataHandlingAdapter;
        private Socket mainSocket;
        private NetworkStream networkStream;
        private bool online;
        private bool onlySend;
        private SocketAsyncEventArgs receiveEventArgs;
        private bool separateThreadSend;
        /// <summary>
        /// 成功连接到服务器
        /// </summary>
        public event RRQMMessageEventHandler<ITcpClient> Connected;

        /// <summary>
        /// 断开连接
        /// </summary>
        public event RRQMMessageEventHandler<ITcpClient> Disconnected;

        /// <summary>
        /// 获取内存池实例
        /// </summary>
        public BytePool BytePool
        {
            get { return bytePool; }
        }

        /// <summary>
        /// 客户端配置
        /// </summary>
        public TcpClientConfig ClientConfig
        {
            get { return this.clientConfig; }
        }

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        public DataHandlingAdapter DataHandlingAdapter
        {
            get { return dataHandlingAdapter; }
        }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IP { get; private set; }

        /// <summary>
        /// 主通信器
        /// </summary>
        public Socket MainSocket
        {
            get { return mainSocket; }
            internal set
            {
                mainSocket = value;
            }
        }

        /// <summary>
        /// IP及端口
        /// </summary>
        public string Name => $"{this.IP}:{this.Port}";

        /// <summary>
        /// 判断是否已连接
        /// </summary>
        public bool Online { get { return this.online; } }

        /// <summary>
        /// 仅发送，即不会开启接收线程。
        /// </summary>
        public bool OnlySend
        {
            get { return onlySend; }
        }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// 在异步发送时，使用独立线程发送
        /// </summary>
        public bool SeparateThreadSend
        {
            get { return separateThreadSend; }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Close()
        {
            if (this.mainSocket != null)
            {
                this.mainSocket.Close();
            }
            this.Dispose();
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        public virtual ITcpClient Connect()
        {
            if (this.disposable)
            {
                throw new RRQMException("无法利用已释放对象");
            }
            if (this.clientConfig == null)
            {
                throw new ArgumentNullException("配置文件不能为空。");
            }
            IPHost iPHost = this.clientConfig.RemoteIPHost;
            if (iPHost == null)
            {
                throw new ArgumentNullException("iPHost不能为空。");
            }
            if (!this.online)
            {
                Socket socket = new Socket(iPHost.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                PreviewConnect(socket);
                socket.Connect(iPHost.EndPoint);
                this.mainSocket = socket;
                InitConnect();
            }
            return this;
        }

        /// <summary>
        /// 异步连接服务器
        /// </summary>
        public async Task<ITcpClient> ConnectAsync()
        {
            return await Task.Run(() =>
            {
                return this.Connect();
            });
        }


        /// <summary>
        /// 断开连接
        /// </summary>
        public virtual ITcpClient Disconnect()
        {
            this.online = false;
            if (this.mainSocket != null)
            {
                this.mainSocket.Dispose();
            }
            if (this.asyncSender != null)
            {
                this.asyncSender.Dispose();
                this.asyncSender = null;
            }
            if (this.networkStream != null)
            {
                this.networkStream.Dispose();
                this.networkStream = null;
            }
            return this;
        }

        /// <summary>
        /// 断开链接并释放资源
        /// </summary>
        public override void Dispose()
        {
            this.online = false;
            if (this.mainSocket != null)
            {
                this.mainSocket.Dispose();
            }
            if (this.bytePool != null)
            {
                this.bytePool.Clear();
            }
            if (this.asyncSender != null)
            {
                this.asyncSender.Dispose();
                this.asyncSender = null;
            }
            if (this.networkStream != null)
            {
                this.networkStream.Dispose();
                this.networkStream = null;
            }
            base.Dispose();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public NetworkStream GetNetworkStream()
        {
            if (this.networkStream == null)
            {
                throw new RRQMException($"请在连接成功，且在{ReceiveType.NetworkStream}模式下获取流");
            }
            return networkStream;
        }
        /// <summary>
        /// 处理数据
        /// </summary>
        void IHandleBuffer.HandleBuffer(ByteBlock byteBlock)
        {
            try
            {
                this.PreviewHandleReceivedData(byteBlock);
                if (this.dataHandlingAdapter == null)
                {
                    throw new RRQMException("数据处理适配器为空");
                }
                this.dataHandlingAdapter.Received(byteBlock);
            }
            catch (Exception ex)
            {
                Logger.Debug(LogType.Error, this, "在处理数据时发生错误", ex);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        #region 同步发送

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
            this.Send(byteBlock.Buffer, 0, byteBlock.Len);
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
            if (this.dataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), "数据处理适配器为空");
            }
            this.dataHandlingAdapter.Send(buffer, offset, length, false);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public virtual void Send(IList<TransferByte> transferBytes)
        {
            if (this.dataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), "数据处理适配器为空");
            }

            if (this.dataHandlingAdapter.CanSplicingSend)
            {
                this.dataHandlingAdapter.Send(transferBytes, false);
            }
            else
            {
                ByteBlock byteBlock = this.bytePool.GetByteBlock(this.BufferLength);
                try
                {
                    foreach (var item in transferBytes)
                    {
                        byteBlock.Write(item.Buffer, item.Offset, item.Length);
                    }
                    this.dataHandlingAdapter.Send(byteBlock.Buffer, 0, byteBlock.Len, false);
                }
                finally
                {
                    byteBlock.Dispose();
                }
            }
        }

        #endregion 同步发送

        #region 异步发送

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
            if (this.dataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), "数据处理适配器为空");
            }
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
            this.SendAsync(byteBlock.Buffer, 0, byteBlock.Len);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public virtual void SendAsync(IList<TransferByte> transferBytes)
        {
            if (this.dataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), "数据处理适配器为空");
            }
            if (this.dataHandlingAdapter.CanSplicingSend)
            {
                this.dataHandlingAdapter.Send(transferBytes, true);
            }
            else
            {
                ByteBlock byteBlock = this.bytePool.GetByteBlock(this.BufferLength);
                try
                {
                    foreach (var item in transferBytes)
                    {
                        byteBlock.Write(item.Buffer, item.Offset, item.Length);
                    }
                    this.dataHandlingAdapter.Send(byteBlock.Buffer, 0, byteBlock.Len, true);
                }
                finally
                {
                    byteBlock.Dispose();
                }
            }
        }

        #endregion 异步发送

        /// <summary>
        /// 设置数据处理适配器
        /// </summary>
        /// <param name="adapter"></param>
        public virtual void SetDataHandlingAdapter(DataHandlingAdapter adapter)
        {
            if (adapter == null)
            {
                throw new RRQMException("数据处理适配器为空");
            }
            if (adapter.locked)
            {
                throw new RRQMException("此适配器已被其他终端使用，请重新创建对象。");
            }
            adapter.locked = true;
            adapter.BytePool = this.bytePool;
            adapter.Logger = this.Logger;
            adapter.ReceivedCallBack = this.HandleReceivedData;
            adapter.SendCallBack = this.Sent;
            this.dataHandlingAdapter = adapter;
        }

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="clientConfig"></param>
        /// <exception cref="RRQMException"></exception>
        public ITcpClient Setup(TcpClientConfig clientConfig)
        {
            this.clientConfig = clientConfig;
            this.LoadConfig(this.clientConfig);
            return this;
        }

        /// <summary>
        /// 禁用发送或接收
        /// </summary>
        /// <param name="how"></param>
        public void Shutdown(SocketShutdown how)
        {
            if (this.mainSocket != null)
            {
                mainSocket.Shutdown(how);
            }
            if (this.networkStream != null)
            {
                this.networkStream.Dispose();
                this.networkStream = null;
            }
        }

        internal void InitConnect()
        {
            this.ReadIpPort();

            this.OnConnected(new MesEventArgs());

            this.BeginReceive();

            if (this.separateThreadSend)
            {
                if (this.asyncSender != null)
                {
                    this.asyncSender.Dispose();
                }
                this.asyncSender = new AsyncSender(this.MainSocket, this.MainSocket.RemoteEndPoint, this.OnSeparateThreadSendError);
            }


        }

        /// <summary>
        /// 处理已接收到的数据。
        /// 覆盖父类方法将不触发OnReceived事件。
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected abstract void HandleReceivedData(ByteBlock byteBlock, object obj);

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

            if (clientConfig.DataHandlingAdapter != null)
            {
                this.SetDataHandlingAdapter(clientConfig.DataHandlingAdapter);
            }
            this.logger = (ILog)clientConfig.GetValue(RRQMConfig.LoggerProperty);
            this.BufferLength = (int)clientConfig.GetValue(RRQMConfig.BufferLengthProperty);
            this.onlySend = clientConfig.OnlySend;
            this.separateThreadSend = clientConfig.SeparateThreadSend;
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnConnected(MesEventArgs e)
        {
            this.online = true;
            try
            {
                this.Connected?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                this.logger.Debug(LogType.Error, this, $"在事件{nameof(this.Connected)}中发生错误。", ex);
            }

        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(MesEventArgs e)
        {
            this.online = false;
            try
            {
                this.Disconnected?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                this.logger.Debug(LogType.Error, this, $"在事件{nameof(this.Disconnected)}中发生错误。", ex);
            }
        }

        /// <summary>
        /// 在独立发送线程中发生错误
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void OnSeparateThreadSendError(Exception ex)
        {
            this.logger.Debug(LogType.Error, this, "独立线程发送错误", ex);
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
        /// <summary>
        /// 预处理收到数据，
        /// 一般用于调试检验数据
        /// </summary>
        /// <param name="byteBlock"></param>
        protected virtual void PreviewHandleReceivedData(ByteBlock byteBlock)
        {

        }

        /// <summary>
        /// 启动消息接收
        /// </summary>
        private void BeginReceive()
        {
            try
            {
                if (this.clientConfig.ReceiveType == ReceiveType.NetworkStream)
                {
                    networkStream = new NetworkStream(this.mainSocket, true);
                }
                else if (this.onlySend)
                {
                    return;
                }
                else if (this.clientConfig.ReceiveType == ReceiveType.IOCP)
                {
                    this.receiveEventArgs = new SocketAsyncEventArgs();
                    this.receiveEventArgs.Completed += EventArgs_Completed;

                    ByteBlock byteBlock = this.bytePool.GetByteBlock(this.BufferLength);
                    this.receiveEventArgs.UserToken = byteBlock;
                    this.receiveEventArgs.SetBuffer(byteBlock.Buffer, 0, byteBlock.Buffer.Length);
                    if (!this.MainSocket.ReceiveAsync(this.receiveEventArgs))
                    {
                        ProcessReceived(this.receiveEventArgs);
                    }
                }
                else
                {
                    Thread thread = new Thread(this.BIOReceive);
                    thread.IsBackground = true;
                    thread.Name = "客户端接收线程";
                    thread.Start();
                }
            }
            catch (Exception ex)
            {
                this.OnDisconnected(new MesEventArgs(ex.Message));
            }
        }

        private void BIOReceive()
        {
            while (true)
            {
                ByteBlock byteBlock = bytePool.GetByteBlock(this.BufferLength);

                try
                {
                    int r = this.mainSocket.Receive(byteBlock.Buffer);
                    if (r == 0)
                    {
                        byteBlock.Dispose();
                        break;
                    }
                    byteBlock.SetLength(r);
                    ((IHandleBuffer)this).HandleBuffer(byteBlock);
                }
                catch (Exception ex)
                {
                    this.OnDisconnected(new MesEventArgs(ex.Message));
                    break;
                }

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
                    this.OnDisconnected(new MesEventArgs("BreakOut"));
                }
            }
            catch (Exception ex)
            {
                this.OnDisconnected(new MesEventArgs(ex.Message));
            }
        }
        private void ProcessReceived(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                ByteBlock byteBlock = (ByteBlock)e.UserToken;
                byteBlock.SetLength(e.BytesTransferred);
                ((IHandleBuffer)this).HandleBuffer(byteBlock);
                try
                {
                    ByteBlock newByteBlock = this.bytePool.GetByteBlock(this.BufferLength);
                    e.UserToken = newByteBlock;
                    e.SetBuffer(newByteBlock.Buffer, 0, newByteBlock.Buffer.Length);

                    if (!this.MainSocket.ReceiveAsync(e))
                    {
                        ProcessReceived(e);
                    }
                }
                catch
                {
                }
            }
            else
            {
                this.OnDisconnected(new MesEventArgs("BreakOut"));
            }
        }

        private void ReadIpPort()
        {
            if (MainSocket == null)
            {
                this.IP = null;
                this.Port = -1;
                return;
            }

            string ipport;
            if (MainSocket.Connected && MainSocket.RemoteEndPoint != null)
            {
                ipport = MainSocket.RemoteEndPoint.ToString();
            }
            else if (MainSocket.IsBound && MainSocket.LocalEndPoint != null)
            {
                ipport = MainSocket.LocalEndPoint.ToString();
            }
            else
            {
                return;
            }

            int r = ipport.LastIndexOf(":");
            this.IP = ipport.Substring(0, r);
            this.Port = Convert.ToInt32(ipport.Substring(r + 1, ipport.Length - (r + 1)));
        }

        private void Sent(byte[] buffer, int offset, int length, bool isAsync)
        {
            if (!this.Online)
            {
                throw new RRQMNotConnectedException("该实例已断开");
            }

            if (isAsync)
            {
                if (separateThreadSend)
                {
                    this.asyncSender.AsyncSend(buffer, offset, length);
                }
                else
                {
                    this.mainSocket.BeginSend(buffer, offset, length, SocketFlags.None, null, null);
                }
            }
            else
            {
                while (length > 0)
                {
                    int r = MainSocket.Send(buffer, offset, length, SocketFlags.None);
                    if (r == 0 && length > 0)
                    {
                        throw new RRQMException("发送数据不完全");
                    }
                    offset += r;
                    length -= r;
                }
            }
        }
    }
}