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
using RRQMCore.Dependency;
using RRQMCore.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 服务器辅助类
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{IP}:{Port}")]
    public class SocketClient : BaseSocket, ISocketClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public SocketClient()
        {
            this.Protocol = Protocol.TCP;
        }

        #region 变量
        internal string id;
        internal long lastTick;
        internal IPluginsManager pluginsManager;
        internal ReceiveType receiveType;
        internal TcpServiceBase service;
        internal RRQMConfig config;
        internal bool usePlugin;
        internal bool useSsl;
        private int m_maxPackageSize;
        private ClearType m_clearType;
        private DataHandlingAdapter m_adapter;
        private string m_ip;
        private Socket m_mainSocket;
        private bool m_online;
        private int m_port;
        private Stream m_workStream;
        #endregion 变量

        #region 属性

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int MaxPackageSize => this.m_maxPackageSize;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IContainer Container => this.service == null ? null : this.service.Container;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Protocol Protocol { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool CanSetDataHandlingAdapter => true;

        /// <summary>
        /// 选择清理类型
        /// </summary>
        public ClearType ClearType
        {
            get => this.m_clearType;
            set => this.m_clearType = value;
        }

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        public DataHandlingAdapter DataHandlingAdapter => this.m_adapter;

        /// <summary>
        /// 用于索引的ID
        /// </summary>
        public string ID => this.id;

        /// <summary>
        /// IP地址
        /// </summary>
        public string IP => this.m_ip;

        /// <summary>
        /// 主通信器
        /// </summary>
        public Socket MainSocket => this.m_mainSocket;


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool Online => this.m_online;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IPluginsManager PluginsManager => this.pluginsManager;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Port => this.m_port;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ReceiveType ReceiveType => this.receiveType;

        /// <summary>
        /// 端口号
        /// </summary>
        /// <summary>
        /// 包含此辅助类的主服务器类
        /// </summary>
        public TcpServiceBase Service => this.service;

        /// <summary>
        /// 服务配置
        /// </summary>
        public RRQMConfig Config => this.config;

        /// <summary>
        /// 是否已启动插件
        /// </summary>
        public bool UsePlugin => this.usePlugin;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool UseSsl => this.useSsl;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool CanSend => this.m_online;
        #endregion 属性

        #region 事件&委托
        /// <summary>
        /// 断开连接
        /// </summary>
        public event RRQMTcpClientDisconnectedEventHandler<ITcpClientBase> Disconnected;
        #endregion 事件&委托

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual void Close()
        {
            this.Close($"主动调用{nameof(Close)}");
        }

        /// <summary>
        /// 中断终端，传递中断消息
        /// </summary>
        /// <param name="msg"></param>
        public virtual void Close(string msg)
        {
            Task.Run(() =>
            {
                this.BreakOut(msg, true);
            });
        }

        private void BreakOut(string msg, bool manual)
        {
            lock (this)
            {
                if (this.m_online)
                {
                    if (this.m_mainSocket != null)
                    {
                        this.m_mainSocket.Dispose();
                    }
                    if (this.m_workStream != null)
                    {
                        this.m_workStream.Dispose();
                    }

                    if (this.service != null)
                    {
                        this.service.SocketClients.TryRemove(this.id, out _);
                    }

                    this.m_online = false;
                    this.OnDisconnected(new ClientDisconnectedEventArgs(manual, msg));
                    this.OnClose();
                    this.config = default;
                    this.m_adapter = default;
                    this.pluginsManager = default;
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            if (this.m_workStream == null)
            {
                this.m_workStream = new NetworkStream(this.m_mainSocket, true);
            }
            return this.m_workStream;
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
            this.m_mainSocket.Shutdown(how);
        }

        internal void GetTimeout(int time, long nowTick)
        {
            if (nowTick - (this.lastTick / 10000000.0) > time)
            {
                Task.Run(() =>
                {
                    this.BreakOut($"超时无数据交互，被主动清理", false);
                });
            }
        }

        /// <summary>
        /// 读取IP、Port
        /// </summary>
        internal void LoadSocketAndReadIpPort(Socket socket)
        {
            this.m_mainSocket = socket;

            if (this.m_mainSocket == null)
            {
                this.m_ip = null;
                this.m_port = -1;
                return;
            }

            string ipport;
            if (this.m_mainSocket.Connected && this.m_mainSocket.RemoteEndPoint != null)
            {
                ipport = this.m_mainSocket.RemoteEndPoint.ToString();
            }
            else if (this.m_mainSocket.IsBound && this.m_mainSocket.LocalEndPoint != null)
            {
                ipport = this.m_mainSocket.LocalEndPoint.ToString();
            }
            else
            {
                return;
            }

            int r = ipport.LastIndexOf(":");
            this.m_ip = ipport.Substring(0, r);
            this.m_port = Convert.ToInt32(ipport.Substring(r + 1, ipport.Length - (r + 1)));
        }

        internal void OnEvent(int type, RRQMEventArgs e)
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
                        this.m_online = true;
                        this.BeginReceive();
                        this.OnConnected(e);
                        break;
                    }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.Close($"主动调用{nameof(Dispose)}");
            base.Dispose(disposing);
        }

        /// <summary>
        /// 处理已接收到的数据。如果覆盖父类方法，则不会触发服务器方法和插件。
        /// <para>根据不同的数据处理适配器，会传递不同的数据</para>
        /// </summary>
        /// <param name="byteBlock">以二进制流形式传递</param>
        /// <param name="requestInfo">以解析的数据对象传递</param>
        protected virtual void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.usePlugin)
            {
                ReceivedDataEventArgs args = new ReceivedDataEventArgs(byteBlock, requestInfo);
                this.pluginsManager.Raise<ITcpPlugin>("OnReceivedData", this, args);
                if (args.Handled)
                {
                    return;
                }
            }

            this.service.OnInternalReceivedData(this, byteBlock, requestInfo);
        }

        /// <summary>
        /// 当即将发送时，如果覆盖父类方法，则不会触发插件。
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <returns>返回值表示是否允许发送</returns>
        protected virtual bool HandleSendingData(byte[] buffer, int offset, int length)
        {
            if (this.usePlugin)
            {
                SendingEventArgs args = new SendingEventArgs(buffer, offset, length);
                this.pluginsManager.Raise<ITcpPlugin>("OnSendingData", this, args);
                if (args.Operation.HasFlag(Operation.Permit))
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// 一旦断开TCP连接，该方法则必被调用。
        /// </summary>
        protected virtual void OnClose()
        {
        }

        /// <summary>
        /// 当客户端完整建立TCP连接，如果覆盖父类方法，则不会触发插件。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnConnected(RRQMEventArgs e)
        {
            if (this.usePlugin)
            {
                this.pluginsManager.Raise<ITcpPlugin>("OnConnected", this, e);
                if (e.Handled)
                {
                    return;
                }
            }

            this.service.OnInternalConnected(this, e);
        }

        /// <summary>
        /// 客户端正在连接，如果覆盖父类方法，则不会触发插件。
        /// </summary>
        protected virtual void OnConnecting(ClientOperationEventArgs e)
        {
            if (this.usePlugin)
            {
                this.pluginsManager.Raise<ITcpPlugin>("OnConnecting", this, e);
            }
            if (!e.Handled)
            {
                this.service.OnInternalConnecting(this, e);
            }

            if (e.Operation.HasFlag(Operation.Permit))
            {
                if (this.CanSetDataHandlingAdapter)
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
        }

        /// <summary>
        /// 客户端已断开连接，如果从Connecting中拒绝连接，则不会触发。如果覆盖父类方法，则不会触发插件。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(ClientDisconnectedEventArgs e)
        {
            if (this.usePlugin)
            {
                this.pluginsManager.Raise<ITcpPlugin>("OnDisconnected", this, e);
                if (e.Handled)
                {
                    return;
                }
            }
            this.Disconnected?.Invoke(this, e);
            if (!e.Operation.HasFlag(Operation.Handled))
            {
                this.service.OnInternalDisconnected(this, e);
            }
        }

        /// <summary>
        ///  处理收到的最原始数据，该方法在适配器之前调用
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns>返回值标识该数据是否继续向下执行</returns>
        protected virtual bool OnHandleBuffer(ByteBlock byteBlock)
        {
            return true;
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

            if (adapter.client != null)
            {
                throw new RRQMException("此适配器已被其他终端使用，请重新创建对象。");
            }
            adapter.client = this;
            adapter.ReceivedCallBack = this.HandleReceivedData;
            adapter.SendCallBack = this.SocketSend;
            if (this.config!=null)
            {
                this.m_maxPackageSize = Math.Max(adapter.MaxPackageSize, this.Config.GetValue<int>(RRQMConfigExtensions.MaxPackageSizeProperty));
                adapter.MaxPackageSize = this.m_maxPackageSize;
            }
           
            this.m_adapter = adapter;
        }

        private void BeginReceive()
        {
            try
            {
                if (this.useSsl)
                {
                    ServiceSslOption sslOption = this.config.GetValue<ServiceSslOption>(RRQMConfigExtensions.SslOptionProperty);
                    SslStream sslStream = (sslOption.CertificateValidationCallback != null) ? new SslStream(new NetworkStream(this.m_mainSocket, false), false, sslOption.CertificateValidationCallback) : new SslStream(new NetworkStream(this.m_mainSocket, false), false);
                    sslStream.AuthenticateAsServer(sslOption.Certificate, sslOption.ClientCertificateRequired, sslOption.SslProtocols, sslOption.CheckCertificateRevocation);
                    this.m_workStream = sslStream;

                    if (this.receiveType == ReceiveType.Auto)
                    {
                        this.BeginSsl();
                    }
                }
                else
                {
                    if (this.receiveType == ReceiveType.Auto)
                    {
                        SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
                        eventArgs.Completed += this.EventArgs_Completed;
                        ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
                        eventArgs.UserToken = byteBlock;
                        eventArgs.SetBuffer(byteBlock.Buffer, 0, byteBlock.Capacity);
                        if (!this.m_mainSocket.ReceiveAsync(eventArgs))
                        {
                            this.ProcessReceived(eventArgs);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                this.BreakOut(ex.Message, false);
                this.Logger.Debug(LogType.Error, this, $"在{nameof(BeginReceive)}发生错误", ex);
            }
        }

        private void EndSsl(IAsyncResult result)
        {
            ByteBlock byteBlock = (ByteBlock)result.AsyncState;
            try
            {
                int r = this.m_workStream.EndRead(result);
                byteBlock.SetLength(r);
                this.HandleBuffer(byteBlock);
                this.BeginSsl();
            }
            catch (Exception ex)
            {
                byteBlock.Dispose();
                this.BreakOut(ex.Message, false);
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
                e.Dispose();
                this.BreakOut(ex.Message, false);
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
                if (this.m_clearType.HasFlag(ClearType.Receive))
                {
                    this.lastTick = DateTime.Now.Ticks;
                }

                if (this.OnHandleBuffer(byteBlock))
                {
                    if (this.disposedValue)
                    {
                        return;
                    }
                    if (this.m_adapter == null)
                    {
                        this.Logger.Debug(LogType.Error, this, ResType.NullDataAdapter.GetResString());
                        return;
                    }
                    this.m_adapter.ReceivedInput(byteBlock);
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
            if (this.disposedValue)
            {
                e.Dispose();
            }
            else
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
                        e.SetBuffer(newByteBlock.Buffer, 0, newByteBlock.Capacity);

                        if (!this.m_mainSocket.ReceiveAsync(e))
                        {
                            this.ProcessReceived(e);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.BreakOut(ex.Message, false);
                    }
                }
                else
                {
                    e.Dispose();
                    this.BreakOut("远程主机主动断开连接", false);
                }
            }
        }

        /// <summary>
        /// 绕过适配器直接发送。<see cref="ByteBlock.Buffer"/>作为数据时，仅可同步发送。
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="isAsync">是否异步发送</param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        protected void SocketSend(byte[] buffer, int offset, int length, bool isAsync)
        {
            if (!this.m_online)
            {
                throw new RRQMNotConnectedException(ResType.NotConnected.GetResString());
            }
            if (this.HandleSendingData(buffer, offset, length))
            {
                lock (this)
                {
                    if (this.useSsl)
                    {
                        this.m_workStream.Write(buffer, offset, length);
                    }
                    else
                    {
                        if (isAsync)
                        {
                            this.m_mainSocket.BeginSend(buffer, offset, length, SocketFlags.None, null, null);
                        }
                        else
                        {
                            while (length > 0)
                            {
                                int r = this.m_mainSocket.Send(buffer, offset, length, SocketFlags.None);
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
                if (this.m_clearType.HasFlag(ClearType.Send))
                {
                    this.lastTick = DateTime.Now.Ticks;
                }
            }
        }

        #region 默认发送
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void DefaultSend(byte[] buffer, int offset, int length)
        {
            this.SocketSend(buffer, offset, length, false);
        }

        /// <summary>
        ///<inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void DefaultSend(byte[] buffer)
        {
            this.DefaultSend(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void DefaultSend(ByteBlock byteBlock)
        {
            this.DefaultSend(byteBlock.Buffer, 0, byteBlock.Len);
        }
        #endregion

        #region 异步默认发送
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"><inheritdoc/></param>
        /// <param name="offset"><inheritdoc/></param>
        /// <param name="length"><inheritdoc/></param>
        /// <exception cref="RRQMNotConnectedException"><inheritdoc/></exception>
        /// <exception cref="RRQMOverlengthException"><inheritdoc/></exception>
        /// <exception cref="RRQMException"><inheritdoc/></exception>
        public void DefaultSendAsync(byte[] buffer, int offset, int length)
        {
            this.SocketSend(buffer, offset, length, true);
        }

        /// <summary>
        ///<inheritdoc/>
        /// </summary>
        /// <param name="buffer"><inheritdoc/></param>
        /// <exception cref="RRQMNotConnectedException"><inheritdoc/></exception>
        /// <exception cref="RRQMOverlengthException"><inheritdoc/></exception>
        /// <exception cref="RRQMException"><inheritdoc/></exception>
        public void DefaultSendAsync(byte[] buffer)
        {
            this.DefaultSendAsync(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"><inheritdoc/></param>
        /// <exception cref="RRQMNotConnectedException"><inheritdoc/></exception>
        /// <exception cref="RRQMOverlengthException"><inheritdoc/></exception>
        /// <exception cref="RRQMException"><inheritdoc/></exception>
        public void DefaultSendAsync(ByteBlock byteBlock)
        {
            this.DefaultSendAsync(byteBlock.Buffer, 0, byteBlock.Len);
        }
        #endregion

        private void BeginSsl()
        {
            ByteBlock byteBlock = new ByteBlock(this.BufferLength);
            try
            {
                this.m_workStream.BeginRead(byteBlock.Buffer, 0, byteBlock.Capacity, this.EndSsl, byteBlock);
            }
            catch (Exception ex)
            {
                byteBlock.Dispose();
                this.BreakOut(ex.Message, false);
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
            if (this.disposedValue)
            {
                return;
            }
            if (this.m_adapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), ResType.NullDataAdapter.GetResString());
            }
            this.m_adapter.SendInput(buffer, offset, length, false);
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
            if (this.disposedValue)
            {
                return;
            }
            if (this.m_adapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), ResType.NullDataAdapter.GetResString());
            }
            if (this.m_adapter.CanSplicingSend)
            {
                this.m_adapter.SendInput(transferBytes, false);
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
                    this.m_adapter.SendInput(byteBlock.Buffer, 0, byteBlock.Len, false);
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
            if (this.disposedValue)
            {
                return;
            }
            if (this.m_adapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), ResType.NullDataAdapter.GetResString());
            }
            this.m_adapter.SendInput(buffer, offset, length, true);
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
            if (this.disposedValue)
            {
                return;
            }
            if (this.m_adapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), ResType.NullDataAdapter.GetResString());
            }
            if (this.m_adapter.CanSplicingSend)
            {
                this.m_adapter.SendInput(transferBytes, true);
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
                    this.m_adapter.SendInput(byteBlock.Buffer, 0, byteBlock.Len, true);
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
    }
}