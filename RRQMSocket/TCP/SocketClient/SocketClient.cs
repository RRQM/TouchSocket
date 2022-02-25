//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore;
using RRQMCore.ByteManager;

using RRQMCore.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 服务器辅助类
    /// </summary>
    public abstract class SocketClient : BaseSocket, ISocketClient
    {
        internal string id;
        internal long lastTick;
        internal ReceiveType receiveType;
        internal TcpServiceBase service;
        internal ServiceConfig serviceConfig;
        internal bool useSsl;

        /// <summary>
        /// 设置在线状态
        /// </summary>
        protected bool online;

        private ClearType clearType;
        private DataHandlingAdapter dataHandlingAdapter;
        private SocketAsyncEventArgs eventArgs;
        private string ip;
        private Socket mainSocket;
        private int port;
        private bool receiving;
        private Stream workStream;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool CanSetDataHandlingAdapter { get => true; }

        /// <summary>
        /// 选择清理类型
        /// </summary>
        public ClearType ClearType
        {
            get { return this.clearType; }
            set { this.clearType = value; }
        }

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        public DataHandlingAdapter DataHandlingAdapter
        {
            get
            {
                return this.dataHandlingAdapter;
            }
        }

        /// <summary>
        /// 用于索引的ID
        /// </summary>
        public string ID
        {
            get { return this.id; }
        }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IP
        {
            get { return this.ip; }
        }

        /// <summary>
        /// 主通信器
        /// </summary>
        public Socket MainSocket
        {
            get { return this.mainSocket; }
        }

        /// <summary>
        /// IP及端口
        /// </summary>
        public string Name => $"{this.ip}:{this.port}";

        /// <summary>
        /// 判断该实例是否还在线
        /// </summary>
        public bool Online => this.online;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Port
        {
            get { return this.port; }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ReceiveType ReceiveType
        {
            get { return this.receiveType; }
        }

        /// <summary>
        /// 端口号
        /// </summary>
        /// <summary>
        /// 包含此辅助类的主服务器类
        /// </summary>
        public TcpServiceBase Service
        {
            get { return this.service; }
        }

        /// <summary>
        /// 服务配置
        /// </summary>
        public ServiceConfig ServiceConfig
        {
            get { return this.serviceConfig; }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool UseSsl
        {
            get { return this.useSsl; }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual void Close()
        {
            this.BreakOut($"主动调用{nameof(Close)}");
        }

        /// <summary>
        /// 完全释放资源
        /// </summary>
        public override void Dispose()
        {
            if (this.disposable)
            {
                return;
            }
            this.BreakOut($"主动调用{nameof(Dispose)}");
            base.Dispose();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            if (this.workStream == null)
            {
                this.workStream = new NetworkStream(this.mainSocket, true);
            }
            return this.workStream;
        }

        /// <summary>
        /// 重新设置ID
        /// </summary>
        /// <param name="newID"></param>
        public void ResetID(string newID)
        {
            if (this.id == newID)
            {
                return;
            }
            this.ResetID(new WaitSetID() { OldID = this.id, NewID = newID });
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
        /// 禁用发送或接收
        /// </summary>
        /// <param name="how"></param>
        public void Shutdown(SocketShutdown how)
        {
            this.mainSocket.Shutdown(how);
        }

        internal void GetTimeout(int time, long nowTick)
        {
            if (nowTick - (this.lastTick / 10000000.0) > time)
            {
                this.BreakOut($"超时无数据交互，被主动清理");
            }
        }

        /// <summary>
        /// 读取IP、Port
        /// </summary>
        internal void LoadSocketAndReadIpPort(Socket socket)
        {
            this.mainSocket = socket;

            if (this.mainSocket == null)
            {
                this.ip = null;
                this.port = -1;
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
            this.ip = ipport.Substring(0, r);
            this.port = Convert.ToInt32(ipport.Substring(r + 1, ipport.Length - (r + 1)));
        }

        internal void OnEvent(int type, MesEventArgs e)
        {
            switch (type)
            {
                case 1:
                    {
                        this.OnConnecting((ClientOperationEventArgs)e);
                        break;
                    }
                case 2:
                    {
                        this.PreviewConnected(e);
                        break;
                    }
            }
        }

        internal long TryReceive()
        {
            if (this.receiving)
            {
                try
                {
                    if (this.mainSocket.Poll(-1, SelectMode.SelectRead))
                    {
                        int available = this.mainSocket.Available;
                        if (available > 0)
                        {
                            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
                            int r = this.GetStream().Read(byteBlock.Buffer, 0, byteBlock.Capacity);
                            if (r == 0)
                            {
                                byteBlock.Dispose();
                                this.BreakOut("远程终端主动断开");
                            }
                            byteBlock.SetLength(r);
                            this.HandleBuffer(byteBlock);
                            return available;
                        }
                        else
                        {
                            this.BreakOut("客户端主动断开连接");
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.BreakOut(ex.Message);
                }

            }
            return default;
        }

        /// <summary>
        /// 启动消息接收，并且如果需要Ssl验证，则会验证，一般的，该方法会由系统自动调用。
        /// </summary>
        protected void BeginReceive()
        {
            lock (this)
            {
                try
                {
                    if (this.receiving)
                    {
                        return;
                    }

                    if (this.useSsl)
                    {
                        ServiceSslOption sslOption = this.serviceConfig.GetValue<ServiceSslOption>(TcpServiceConfig.SslOptionProperty);
                        SslStream sslStream = (sslOption.CertificateValidationCallback != null) ? new SslStream(new NetworkStream(this.mainSocket, false), false, sslOption.CertificateValidationCallback) : new SslStream(new NetworkStream(this.mainSocket, false), false);
                        sslStream.AuthenticateAsServer(sslOption.Certificate, sslOption.ClientCertificateRequired, sslOption.SslProtocols, sslOption.CheckCertificateRevocation);
                        this.workStream = sslStream;
                    }
                    switch (this.receiveType)
                    {
                        case ReceiveType.IOCP:
                            {
                                this.eventArgs = new SocketAsyncEventArgs();
                                this.eventArgs.Completed += this.EventArgs_Completed;
                                ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
                                this.eventArgs.UserToken = byteBlock;
                                this.eventArgs.SetBuffer(byteBlock.Buffer, 0, byteBlock.Buffer.Length);
                                if (!this.MainSocket.ReceiveAsync(this.eventArgs))
                                {
                                    this.ProcessReceived(this.eventArgs);
                                }
                                break;
                            }
                        case ReceiveType.BIO:
                            {
                                Thread thread;
                                if (this.useSsl)
                                {
                                    thread = new Thread(this.BIOSslStreamReceive);
                                }
                                else
                                {
                                    thread = new Thread(this.BIOReceive);
                                }
                                thread.IsBackground = true;
                                thread.Name = $"{this.id}客户端接收线程";
                                thread.Start();
                                break;
                            }
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    this.BreakOut(ex.Message);
                    this.logger.Debug(LogType.Error, this, $"在{nameof(BeginReceive)}发生错误", ex);
                }
                finally
                {
                    this.receiving = true;
                }
            }
        }

        /// <summary>
        /// 中断终端，传递中断消息，但不实际作用Socket，如需关闭Socket，请手动释放。
        /// </summary>
        /// <param name="msg"></param>
        protected void BreakOut(string msg)
        {
            Task.Run(() =>
            {
                lock (this)
                {
                    if (this.disposable)
                    {
                        return;
                    }
                    this.disposable = true;
                    this.receiving = false;
                    if (this.eventArgs != null)
                    {
                        this.eventArgs.Dispose();
                    }
                    if (this.mainSocket != null)
                    {
                        this.mainSocket.Dispose();
                    }
                    if (this.workStream != null)
                    {
                        this.workStream.Dispose();
                    }

                    if (this.service != null)
                    {
                        this.service.SocketClients.TryRemove(this.id, out _);
                    }

                    if (this.online)
                    {
                        this.online = false;
                        this.OnDisconnected(new MesEventArgs(msg));
                    }
                    this.dataHandlingAdapter = null;
                    this.serviceConfig = null;
                    this.OnBreakOut();
                }
            });
        }

        /// <summary>
        /// 处理已接收到的数据。
        /// <para>根据不同的数据处理适配器，会传递不同的数据</para>
        /// </summary>
        /// <param name="byteBlock">以二进制流形式传递</param>
        /// <param name="requestInfo">以解析的数据对象传递</param>
        protected abstract void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo);

        /// <summary>
        /// 当调用<see cref="BreakOut(string)"/>
        /// </summary>
        protected virtual void OnBreakOut()
        {
        }

        /// <summary>
        /// 当客户端完整建立TCP连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnConnected(MesEventArgs e)
        {
            this.service.OnInternalConnected(this, e);
        }

        /// <summary>
        /// 客户端正在连接
        /// </summary>
        protected virtual void OnConnecting(ClientOperationEventArgs e)
        {
            this.service.OnInternalConnecting(this, e);
            if (e.IsPermitOperation)
            {
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
            else
            {
                this.BreakOut("拒绝连接");
            }
        }

        /// <summary>
        /// 客户端已断开连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(MesEventArgs e)
        {
            this.service.OnInternalDisconnected(this, e);
        }

        /// <summary>
        ///  预处理收到数据，
        /// 一般用于调试检验数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns>返回值标识该数据是否继续向下执行</returns>
        protected virtual bool OnPreviewHandleReceivedData(ByteBlock byteBlock)
        {
            return true;
        }

        /// <summary>
        /// 准备连接前设置。
        /// <para>1.<see cref="online"/>设置在线状态</para>
        /// <para>2.<see cref="BeginReceive"/>启动接收</para>
        /// <para>3.<see cref="OnConnected"/>回调用连接完成</para>
        /// </summary>
        /// <param name="e"></param>
        protected virtual void PreviewConnected(MesEventArgs e)
        {
            this.online = true;
            this.BeginReceive();
            this.OnConnected(e);
        }

        /// <summary>
        /// 重新设置ID
        /// </summary>
        /// <param name="waitSetID"></param>
        protected virtual void ResetID(WaitSetID waitSetID)
        {
            this.Service.ResetID(waitSetID);
        }

        /// <summary>
        /// 设置适配器，该方法不会检验<see cref="CanSetDataHandlingAdapter"/>的值。
        /// </summary>
        /// <param name="adapter"></param>
        protected void SetAdapter(DataHandlingAdapter adapter)
        {
            if (adapter is null)
            {
                throw new ArgumentNullException(nameof(adapter));
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

        private void BIOReceive()
        {
            while (true)
            {
                if (this.disposable)
                {
                    this.BreakOut(null);
                    break;
                }
                ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);

                try
                {
                    int r = this.mainSocket.Receive(byteBlock.Buffer);
                    if (r == 0)
                    {
                        byteBlock.Dispose();
                        this.BreakOut("远程终端主动断开");
                        break;
                    }
                    byteBlock.SetLength(r);
                    this.HandleBuffer(byteBlock);
                }
                catch (Exception ex)
                {
                    this.BreakOut(ex.Message);
                    break;
                }
            }
        }

        private void BIOSslStreamReceive()
        {
            while (true)
            {
                if (this.disposable)
                {
                    this.BreakOut(null);
                    break;
                }

                try
                {
                    ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
                    int r = this.workStream.Read(byteBlock.Buffer, 0, byteBlock.Capacity);
                    if (r == 0)
                    {
                        byteBlock.Dispose();
                        this.BreakOut("远程终端主动断开");
                        break;
                    }
                    byteBlock.SetLength(r);
                    this.HandleBuffer(byteBlock);
                }
                catch (Exception ex)
                {
                    this.BreakOut(ex.Message);
                    break;
                }
            }
        }

        private void EventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                this.ProcessReceived(e);
            }
            catch (Exception ex)
            {
                this.BreakOut(ex.Message);
            }
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="byteBlock"></param>
        private void HandleBuffer(ByteBlock byteBlock)
        {
            try
            {
                if (this.clearType.HasFlag(ClearType.Receive))
                {
                    this.lastTick = DateTime.Now.Ticks;
                }

                if (this.OnPreviewHandleReceivedData(byteBlock))
                {
                    if (this.disposable)
                    {
                        return;
                    }
                    if (this.dataHandlingAdapter == null)
                    {
                        this.logger.Debug(LogType.Error, this, ResType.NullDataAdapter.GetResString());
                        return;
                    }
                    this.dataHandlingAdapter.Received(byteBlock);
                }
            }
            catch (Exception ex)
            {
                this.Logger.Debug(LogType.Error, this, "在处理数据时发生错误", ex);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        private void ProcessReceived(SocketAsyncEventArgs e)
        {
            if (!this.disposable)
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
                            this.ProcessReceived(e);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.BreakOut(ex.Message);
                    }
                }
                else
                {
                    this.BreakOut("远程主机主动断开连接");
                }
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
            if (this.disposable)
            {
                return;
            }
            if (this.dataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), ResType.NullDataAdapter.GetResString());
            }
            this.dataHandlingAdapter.Send(buffer, offset, length, false);
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
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public virtual void Send(IList<TransferByte> transferBytes)
        {
            if (this.disposable)
            {
                return;
            }
            if (this.dataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), ResType.NullDataAdapter.GetResString());
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
            if (this.disposable)
            {
                return;
            }
            if (this.dataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), ResType.NullDataAdapter.GetResString());
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
            if (this.disposable)
            {
                return;
            }
            if (this.dataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), ResType.NullDataAdapter.GetResString());
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

        #region ID发送

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="buffer"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Send(string id, byte[] buffer)
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
        public void Send(string id, byte[] buffer, int offset, int length)
        {
            this.service.Send(id, buffer, offset, length);
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
        public void Send(string id, ByteBlock byteBlock)
        {
            this.Send(id, byteBlock.Buffer, 0, byteBlock.Len);
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
        public void SendAsync(string id, byte[] buffer)
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
        public void SendAsync(string id, byte[] buffer, int offset, int length)
        {
            this.service.SendAsync(id, buffer, offset, length);
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
        public void SendAsync(string id, ByteBlock byteBlock)
        {
            this.SendAsync(id, byteBlock.Buffer, 0, byteBlock.Len);
        }

        #endregion ID发送

        private void Sent(byte[] buffer, int offset, int length, bool isAsync)
        {
            if (this.useSsl)
            {
                this.workStream.Write(buffer, offset, length);
            }
            else
            {
                if (isAsync)
                {
                    this.mainSocket.BeginSend(buffer, offset, length, SocketFlags.None, null, null);
                }
                else
                {
                    while (length > 0)
                    {
                        int r = this.MainSocket.Send(buffer, offset, length, SocketFlags.None);
                        if (r == 0 && length > 0)
                        {
                            throw new RRQMException("发送数据不完全");
                        }
                        offset += r;
                        length -= r;
                    }
                }
            }
            if (this.clearType.HasFlag(ClearType.Send))
            {
                this.lastTick = DateTime.Now.Ticks;
            }
        }
    }
}