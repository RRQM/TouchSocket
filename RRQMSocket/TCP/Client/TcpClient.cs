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
    public abstract class TcpClient : BaseSocket, ITcpClient
    {
        /// <summary>
        /// 设置在线状态。
        /// </summary>
        protected bool online;
        private AsyncSender asyncSender;
        private TcpClientConfig clientConfig;
        private DataHandlingAdapter dataHandlingAdapter;
        private Socket mainSocket;
        private NetworkStream networkStream;
        private bool onlySend;
        private bool working;

        private bool separateThreadSend;

        /// <summary>
        /// 成功连接到服务器
        /// </summary>
        public event RRQMMessageEventHandler<ITcpClient> Connected;

        /// <summary>
        /// 准备连接的时候，此时已初始化Socket，但是并未建立Tcp连接
        /// </summary>
        public event RRQMTcpClientConnectingEventHandler<ITcpClient> Connecting;

        /// <summary>
        /// 断开连接
        /// </summary>
        public event RRQMMessageEventHandler<ITcpClient> Disconnected;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool CanSetDataHandlingAdapter { get => true; }

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
        public bool Online
        { get { return this.online; } }

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
        public bool Working => this.working;

        #region 断开操作

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual void Close()
        {
            this.Disconnect();
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public virtual ITcpClient Disconnect()
        {
            this.online = false;
            if (this.mainSocket != null)
            {
                this.mainSocket.Close();
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
            this.Disconnect();
            base.Dispose();
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
        }
        #endregion 断开操作

        /// <summary>
        /// 请求连接到服务器。
        /// <para>当该函数返回时，则表示已经正确建立Tcp连接。且已经触发<see cref="PreviewConnected(ClientConnectingEventArgs)"/></para>
        /// </summary>
        public virtual ITcpClient Connect()
        {
            if (!this.online)
            {
                PreviewConnected(this.ConnectService());
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
        private void HandleBuffer(ByteBlock byteBlock)
        {
            try
            {
                this.PreviewHandleReceivedData(byteBlock);
                if (this.dataHandlingAdapter == null)
                {
                    Logger.Debug(LogType.Error, this, "数据处理适配器为空", null);
                    return;
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

        /// <summary>
        /// 设置数据处理适配器
        /// </summary>
        /// <param name="adapter"></param>
        public virtual void SetDataHandlingAdapter(DataHandlingAdapter adapter)
        {
            if (!this.CanSetDataHandlingAdapter)
            {
                throw new RRQMException($"不允许自由调用{nameof(SetDataHandlingAdapter)}进行赋值。");
            }

            this.SetAdapter(adapter);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="ipHost"></param>
        /// <returns></returns>
        public ITcpClient Setup(string ipHost)
        {
            TcpClientConfig config = new TcpClientConfig();
            config.RemoteIPHost = new IPHost(ipHost);
            return this.Setup(config);
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
        /// 建立Tcp连接。
        /// <para>注意：调用该方法之前应当判断当前连接状态，不然当前Socket会被覆盖。</para>
        /// </summary>
        /// <returns></returns>
        protected ClientConnectingEventArgs ConnectService()
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
            Socket socket = new Socket(iPHost.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            ClientConnectingEventArgs args = new ClientConnectingEventArgs(socket);
            this.OnConnecting(args);
            socket.Connect(iPHost.EndPoint);
            this.LoadSocketAndReadIpPort(socket);
            return args;
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
            this.logger = (ILog)clientConfig.GetValue(RRQMConfig.LoggerProperty);
            this.BufferLength = (int)clientConfig.GetValue(RRQMConfig.BufferLengthProperty);
            this.onlySend = clientConfig.OnlySend;
            this.separateThreadSend = clientConfig.SeparateThreadSend;
        }

        /// <summary>
        /// 投递接收请求
        /// </summary>
        protected void MakeClientReceive()
        {
            lock (this)
            {
                if (this.working)
                {
                    throw new RRQMException("该客户端已经处于接收。");
                }
                this.working = true;
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
        }

        /// <summary>
        /// 已经建立Tcp连接，但还未投递接收申请。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnConnected(MesEventArgs e)
        {
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
        /// 准备连接的时候，此时已初始化Socket，但是并未建立Tcp连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnConnecting(ClientConnectingEventArgs e)
        {
            this.Connecting?.Invoke(this, e);
            if (this.CanSetDataHandlingAdapter && this.dataHandlingAdapter == null)
            {
                if (e.DataHandlingAdapter == null)
                {
                    this.SetDataHandlingAdapter(new NormalDataHandlingAdapter());
                }
                else
                {
                    this.SetDataHandlingAdapter(e.DataHandlingAdapter);
                }
            }

        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(MesEventArgs e)
        {
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
        /// 已经建立Tcp，但未触发<see cref="OnConnected(MesEventArgs)"/>，
        /// <para>重写该方法时，如果不调用基方法，应当：</para>
        /// <para>对<see cref="online"/>进行赋值。</para>
        /// <para>需要接收时调用<see cref="MakeClientReceive"/>。</para>
        /// <para>需要触发连接事件调用<see cref="OnConnected(MesEventArgs)"/>。</para>
        /// </summary>
        /// <param name="e"></param>
        protected virtual void PreviewConnected(ClientConnectingEventArgs e)
        {
            this.online = true;
            this.MakeClientReceive();
            this.OnConnected(new MesEventArgs());
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
                ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
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
                ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
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
        /// 预处理收到数据，
        /// 一般用于调试检验数据
        /// </summary>
        /// <param name="byteBlock"></param>
        protected virtual void PreviewHandleReceivedData(ByteBlock byteBlock)
        {
        }

        /// <summary>
        /// 设置适配器，该方法不会检验<see cref="CanSetDataHandlingAdapter"/>的值。
        /// </summary>
        /// <param name="adapter"></param>
        protected void SetAdapter(DataHandlingAdapter adapter)
        {
            if (adapter == null)
            {
                throw new RRQMException("数据处理适配器为空");
            }
            if (adapter.owner != null)
            {
                throw new RRQMException("此适配器已被其他终端使用，请重新创建对象。");
            }
            adapter.owner = this;
            adapter.ReceivedCallBack = this.HandleReceivedData;
            adapter.SendCallBack = this.Sent;
            this.dataHandlingAdapter = adapter;
        }

        /// <summary>
        /// 启动消息接收
        /// </summary>
        private void BeginReceive()
        {
            if (this.onlySend)
            {
                return;
            }
            else if (this.clientConfig.ReceiveType == ReceiveType.NetworkStream)
            {
                try
                {
                    networkStream = new NetworkStream(this.mainSocket, true);
                }
                catch (Exception ex)
                {
                    this.PreviewOnDisconnected(new MesEventArgs(ex.Message), null);
                }

            }
            else if (this.clientConfig.ReceiveType == ReceiveType.IOCP)
            {
                SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
                try
                {
                    eventArgs.Completed += EventArgs_Completed;

                    ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
                    eventArgs.UserToken = byteBlock;
                    eventArgs.SetBuffer(byteBlock.Buffer, 0, byteBlock.Buffer.Length);
                    if (!this.MainSocket.ReceiveAsync(eventArgs))
                    {
                        ProcessReceived(eventArgs);
                    }
                }
                catch (Exception ex)
                {
                    this.PreviewOnDisconnected(new MesEventArgs(ex.Message), eventArgs);
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

        private void BIOReceive()
        {
            while (true)
            {
                ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);

                try
                {
                    int r = this.mainSocket.Receive(byteBlock.Buffer);
                    if (r == 0)
                    {
                        byteBlock.Dispose();
                        break;
                    }
                    byteBlock.SetLength(r);
                    this.HandleBuffer(byteBlock);
                }
                catch (Exception ex)
                {
                    this.PreviewOnDisconnected(new MesEventArgs(ex.Message), null);
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
                    this.PreviewOnDisconnected(new MesEventArgs("BreakOut"), e);
                }
            }
            catch (Exception ex)
            {
                this.PreviewOnDisconnected(new MesEventArgs(ex.Message), e);
            }
        }

        private void LoadSocketAndReadIpPort(Socket socket)
        {
            this.mainSocket = socket;
            if (this.mainSocket == null)
            {
                this.IP = null;
                this.Port = -1;
                return;
            }

            string ipport;
            if (this.mainSocket.Connected && this.mainSocket.RemoteEndPoint != null)
            {
                ipport = this.mainSocket.RemoteEndPoint.ToString();
            }
            else if (this.mainSocket.IsBound && this.mainSocket.LocalEndPoint != null)
            {
                ipport = this.mainSocket.LocalEndPoint.ToString();
            }
            else
            {
                return;
            }

            int r = ipport.LastIndexOf(":");
            this.IP = ipport.Substring(0, r);
            this.Port = Convert.ToInt32(ipport.Substring(r + 1, ipport.Length - (r + 1)));
        }

        private void PreviewOnDisconnected(MesEventArgs e, SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs != null)
            {
                eventArgs.Dispose();
            }
            this.dataHandlingAdapter = null;
            this.online = false;
            this.working = false;
            this.OnDisconnected(e);
        }

        private void ProcessReceived(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                ByteBlock byteBlock = (ByteBlock)e.UserToken;
                byteBlock.SetLength(e.BytesTransferred);
                this.HandleBuffer(byteBlock);
                try
                {
                    ByteBlock newByteBlock = BytePool.GetByteBlock(this.BufferLength);
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
                this.PreviewOnDisconnected(new MesEventArgs("BreakOut"), e);
            }
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