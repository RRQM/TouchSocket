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
    /// 简单TCP客户端
    /// </summary>
    public class TcpClient : TcpClientBase
    {
        /// <summary>
        /// 接收到数据
        /// </summary>
        public event RRQMReceivedEventHandler<TcpClient> Received;

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.Received?.Invoke(this, byteBlock, requestInfo);
            base.HandleReceivedData(byteBlock, requestInfo);
        }
    }

    /// <summary>
    /// TCP客户端
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{IP}:{Port}")]
    public class TcpClientBase : BaseSocket, ITcpClient, IPlguinObject
    {

        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpClientBase()
        {
            this.Protocol = Protocol.TCP;
            this.Container = new Container();
            this.PluginsManager = new PluginsManager(this.Container);
            this.Container.RegisterTransient<ILog, ConsoleLogger>();
        }

        #region 变量
        private AsyncSender m_asyncSender;
        private RRQMConfig m_config;
        private DataHandlingAdapter m_adapter;
        private Socket m_mainSocket;
        private bool m_online;
        private ReceiveType m_receiveType;
        private bool m_separateThreadSend;
        private bool m_usePlugin;
        private bool m_useSsl;
        private Stream m_workStream;
        private int m_maxPackageSize;
        private IPHost m_remoteIPHost;
        #endregion 变量

        #region 事件

        /// <summary>
        /// 成功连接到服务器
        /// </summary>
        public event RRQMMessageEventHandler<ITcpClient> Connected;

        /// <summary>
        /// 准备连接的时候，此时已初始化Socket，但是并未建立Tcp连接
        /// </summary>
        public event RRQMTcpClientConnectingEventHandler<ITcpClient> Connecting;

        /// <summary>
        /// 断开连接。在客户端未设置连接状态时，不会触发
        /// </summary>
        public event RRQMTcpClientDisconnectedEventHandler<ITcpClientBase> Disconnected;

        /// <summary>
        /// 已经建立Tcp连接
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
                this.Logger.Debug(LogType.Error, this, $"在事件{nameof(this.Connected)}中发生错误。", ex);
            }
            if (this.m_usePlugin)
            {
                this.PluginsManager.Raise<ITcpPlugin>("OnConnected", this, e);
            }

        }

        /// <summary>
        /// 准备连接的时候，此时已初始化Socket，但是并未建立Tcp连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnConnecting(ClientConnectingEventArgs e)
        {
            try
            {
                this.Connecting?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                this.Logger.Debug(LogType.Error, this, $"在事件{nameof(this.OnConnecting)}中发生错误。", ex);
            }

            if (this.m_usePlugin)
            {
                this.PluginsManager.Raise<ITcpPlugin>("OnConnecting", this, e);
            }

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

        /// <summary>
        /// 断开连接。在客户端未设置连接状态时，不会触发
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(ClientDisconnectedEventArgs e)
        {
            try
            {
                this.Disconnected?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                this.Logger.Debug(LogType.Error, this, $"在事件{nameof(this.Disconnected)}中发生错误。", ex);
            }

            if (this.m_usePlugin)
            {
                this.PluginsManager.Raise<ITcpPlugin>("OnDisconnected", this, e);
            }
        }

        #endregion 事件

        #region 属性

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IContainer Container { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool CanSetDataHandlingAdapter => true;

        /// <summary>
        /// 客户端配置
        /// </summary>
        public RRQMConfig Config => this.m_config;

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        public DataHandlingAdapter DataHandlingAdapter => this.m_adapter;

        /// <summary>
        /// IP地址
        /// </summary>
        public string IP { get; private set; }

        /// <summary>
        /// 主通信器
        /// </summary>
        public Socket MainSocket => this.m_mainSocket;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool CanSend => this.m_online;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool Online => this.m_online;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IPluginsManager PluginsManager { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ReceiveType ReceiveType => this.m_receiveType;

        /// <summary>
        /// 在异步发送时，使用独立线程发送
        /// </summary>
        public bool SeparateThreadSend => this.m_separateThreadSend;

        /// <summary>
        /// 是否已启用插件
        /// </summary>
        public bool UsePlugin => this.m_usePlugin;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool UseSsl => this.m_useSsl;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Protocol Protocol { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int MaxPackageSize => this.m_maxPackageSize;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IPHost RemoteIPHost => m_remoteIPHost;

        #endregion 属性

        #region 断开操作

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual void Close()
        {
            this.Close($"{nameof(Close)}主动断开");
        }

        /// <summary>
        /// 中断终端，传递中断消息。
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
                    this.m_mainSocket?.Dispose();
                    this.m_asyncSender?.Dispose();
                    this.m_workStream?.Dispose();
                    this.m_online = false;
                    this.OnDisconnected(new ClientDisconnectedEventArgs(manual, msg));
                }
            }
        }

        /// <summary>
        /// 禁用发送或接收
        /// </summary>
        /// <param name="how"></param>
        public void Shutdown(SocketShutdown how)
        {
            this.m_mainSocket.Shutdown(how);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.Close($"{nameof(Dispose)}主动断开");
            base.Dispose(disposing);
        }

        #endregion 断开操作

        #region 插件

        /// <summary>
        /// 添加插件
        /// </summary>
        /// <typeparam name="TPlugin">插件类型</typeparam>
        /// <returns>插件类型实例</returns>
        public TPlugin AddPlugin<TPlugin>() where TPlugin : IPlugin
        {
            var plugin = this.Container.Resolve<TPlugin>();
            this.AddPlugin(plugin);
            return plugin;
        }

        /// <summary>
        /// 添加插件
        /// </summary>
        /// <param name="plugin">插件</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddPlugin(IPlugin plugin)
        {
            if (plugin.Logger == default)
            {
                plugin.Logger = this.Container.Resolve<ILog>();
            }
            this.PluginsManager.Add(plugin);
        }

        /// <summary>
        /// 清空插件
        /// </summary>
        public void ClearPlugins()
        {
            this.PluginsManager.Clear();
        }

        /// <summary>
        /// 移除插件
        /// </summary>
        /// <param name="plugin"></param>
        public void RemovePlugin(IPlugin plugin)
        {
            this.PluginsManager.Remove(plugin);
        }

        /// <summary>
        /// 移除插件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RemovePlugin<T>() where T : IPlugin
        {
            this.PluginsManager.Remove(typeof(T));
        }

        #endregion 插件

        /// <summary>
        /// 请求连接到服务器。
        /// </summary>
        public virtual ITcpClient Connect()
        {
            if (this.m_online)
            {
                return this;
            }
            if (this.disposedValue)
            {
                throw new RRQMException("无法利用已释放对象");
            }
            if (this.m_config == null)
            {
                throw new ArgumentNullException("配置文件不能为空。");
            }
            IPHost iPHost = this.m_config.GetValue<IPHost>(RRQMConfigExtensions.RemoteIPHostProperty);
            if (iPHost == null)
            {
                throw new ArgumentNullException("iPHost不能为空。");
            }
            if (this.m_mainSocket != null)
            {
                this.m_mainSocket.Dispose();
            }
            this.m_mainSocket = this.CreateSocket(iPHost);

            ClientConnectingEventArgs args = new ClientConnectingEventArgs(this.m_mainSocket);
            this.OnConnecting(args);
            this.m_mainSocket.Connect(iPHost.EndPoint);
            this.LoadSocketAndReadIpPort();

            if (this.m_separateThreadSend)
            {
                if (this.m_asyncSender == null)
                {
                    this.m_asyncSender = new AsyncSender(this.m_mainSocket, this.m_mainSocket.RemoteEndPoint, this.OnSeparateThreadSendError);
                }
            }
            this.BeginReceive();
            this.m_online = true;
            Task.Run(() =>
            {
                this.OnConnected(new MesEventArgs("连接成功"));
            });
            return this;
        }

        /// <summary>
        /// 异步连接服务器
        /// </summary>
        public Task<ITcpClient> ConnectAsync()
        {
            return Task.Run(() =>
            {
                return this.Connect();
            });
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
            RRQMConfig config = new RRQMConfig();
            config.SetRemoteIPHost(new IPHost(ipHost));
            return this.Setup(config);
        }

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="clientConfig"></param>
        /// <exception cref="RRQMException"></exception>
        public ITcpClient Setup(RRQMConfig clientConfig)
        {
            this.m_config = clientConfig;
            this.LoadConfig(this.m_config);
            return this;
        }

        private void PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.m_usePlugin)
            {
                ReceivedDataEventArgs args = new ReceivedDataEventArgs(byteBlock, requestInfo);
                this.PluginsManager.Raise<ITcpPlugin>("OnReceivedData", this, args);
                if (args.Handled)
                {
                    return;
                }
            }

            this.HandleReceivedData(byteBlock, requestInfo);
        }

        /// <summary>
        /// 处理已接收到的数据。覆盖父类方法，将不会触发插件。
        /// </summary>
        /// <param name="byteBlock">以二进制流形式传递</param>
        /// <param name="requestInfo">以解析的数据对象传递</param>
        protected virtual void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {

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
            if (this.m_usePlugin)
            {
                SendingEventArgs args = new SendingEventArgs(buffer, offset, length);
                this.PluginsManager.Raise<ITcpPlugin>("OnSendingData", this, args);
                if (args.Operation.HasFlag(Operation.Permit))
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="config"></param>
        protected virtual void LoadConfig(RRQMConfig config)
        {
            if (config == null)
            {
                throw new RRQMException("配置文件为空");
            }
            this.m_remoteIPHost = config.GetValue<IPHost>(RRQMConfigExtensions.RemoteIPHostProperty);
            this.m_maxPackageSize = config.GetValue<int>(RRQMConfigExtensions.MaxPackageSizeProperty);
            this.BufferLength = config.BufferLength;
            this.m_separateThreadSend = config.GetValue<bool>(RRQMConfigExtensions.SeparateThreadSendProperty);
            this.m_receiveType = config.ReceiveType;
            this.m_usePlugin = config.IsUsePlugin;
            if (this.Logger == null)
            {
                this.Logger = this.Container.Resolve<ILog>();
            }

            if (config.GetValue(RRQMConfigExtensions.SslOptionProperty) != null)
            {
                if (this.m_separateThreadSend)
                {
                    throw new RRQMException("Ssl配置下，不允许独立线程发送。");
                }
                this.m_useSsl = true;
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
        /// 在独立发送线程中发生错误
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void OnSeparateThreadSendError(Exception ex)
        {
            this.Logger.Debug(LogType.Error, this, "独立线程发送错误", ex);
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
            adapter.ReceivedCallBack = this.PrivateHandleReceivedData;
            adapter.SendCallBack = this.SocketSend;
            if (this.Config != null)
            {
                adapter.MaxPackageSize = Math.Max(adapter.MaxPackageSize, this.Config.GetValue<int>(RRQMConfigExtensions.MaxPackageSizeProperty));
            }
            this.m_adapter = adapter;
        }

        private void BeginReceive()
        {
            if (this.m_workStream != null)
            {
                this.m_workStream.Dispose();
            }

            if (this.m_useSsl)
            {
                ClientSslOption sslOption = this.m_config.GetValue<ClientSslOption>(RRQMConfigExtensions.SslOptionProperty);
                SslStream sslStream = (sslOption.CertificateValidationCallback != null) ? new SslStream(new NetworkStream(this.m_mainSocket, false), false, sslOption.CertificateValidationCallback) : new SslStream(new NetworkStream(this.m_mainSocket, false), false);
                if (sslOption.ClientCertificates == null)
                {
                    sslStream.AuthenticateAsClient(sslOption.TargetHost);
                }
                else
                {
                    sslStream.AuthenticateAsClient(sslOption.TargetHost, sslOption.ClientCertificates, sslOption.SslProtocols, sslOption.CheckCertificateRevocation);
                }
                this.m_workStream = sslStream;
                if (this.m_receiveType != ReceiveType.None)
                {
                    this.BeginSsl();
                }
            }
            else
            {
                if (this.m_receiveType == ReceiveType.Auto)
                {
                    SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
                    eventArgs.Completed += this.EventArgs_Completed;

                    ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
                    eventArgs.UserToken = byteBlock;
                    eventArgs.SetBuffer(byteBlock.Buffer, 0, byteBlock.Buffer.Length);
                    if (!this.m_mainSocket.ReceiveAsync(eventArgs))
                    {
                        this.ProcessReceived(eventArgs);
                    }
                }
            }
        }

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

        private Socket CreateSocket(IPHost iPHost)
        {
            Socket socket = new Socket(iPHost.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.ReceiveBufferSize = this.BufferLength;
            socket.SendBufferSize = this.BufferLength;
            KeepAliveValue keepAliveValue = this.m_config.GetValue<KeepAliveValue>(RRQMConfigExtensions.KeepAliveValueProperty);
            if (keepAliveValue.Enable)
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                socket.IOControl(IOControlCode.KeepAliveValues, keepAliveValue.KeepAliveTime, null);
            }
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, this.m_config.GetValue<bool>(RRQMConfigExtensions.NoDelayProperty));
            if (this.m_config.GetValue<IPHost>(RRQMConfigExtensions.BindIPHostProperty) != null)
            {
                socket.Bind(this.m_config.GetValue<IPHost>(RRQMConfigExtensions.BindIPHostProperty).EndPoint);
            }
            return socket;
        }

        private void EventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                this.ProcessReceived(e);
            }
            catch (Exception ex)
            {
                this.BreakOut(ex.Message, false);
            }
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        private void HandleBuffer(ByteBlock byteBlock)
        {
            try
            {
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

        #region 同步发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"><inheritdoc/></param>
        /// <exception cref="RRQMNotConnectedException"><inheritdoc/></exception>
        /// <exception cref="RRQMOverlengthException"><inheritdoc/></exception>
        /// <exception cref="RRQMException"><inheritdoc/></exception>
        public virtual void Send(byte[] buffer)
        {
            this.Send(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"><inheritdoc/></param>
        /// <exception cref="RRQMNotConnectedException"><inheritdoc/></exception>
        /// <exception cref="RRQMOverlengthException"><inheritdoc/></exception>
        /// <exception cref="RRQMException"><inheritdoc/></exception>
        public virtual void Send(ByteBlock byteBlock)
        {
            this.Send(byteBlock.Buffer, 0, byteBlock.Len);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"><inheritdoc/></param>
        /// <param name="offset"><inheritdoc/></param>
        /// <param name="length"><inheritdoc/></param>
        /// <exception cref="RRQMNotConnectedException"><inheritdoc/></exception>
        /// <exception cref="RRQMOverlengthException"><inheritdoc/></exception>
        /// <exception cref="RRQMException"><inheritdoc/></exception>
        public virtual void Send(byte[] buffer, int offset, int length)
        {
            if (this.m_adapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), ResType.NullDataAdapter.GetResString());
            }
            this.m_adapter.SendInput(buffer, offset, length, false);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"><inheritdoc/></param>
        /// <exception cref="RRQMNotConnectedException"><inheritdoc/></exception>
        /// <exception cref="RRQMOverlengthException"><inheritdoc/></exception>
        /// <exception cref="RRQMException"><inheritdoc/></exception>
        public virtual void Send(IList<TransferByte> transferBytes)
        {
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
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"><inheritdoc/></param>
        /// <param name="offset"><inheritdoc/></param>
        /// <param name="length"><inheritdoc/></param>
        /// <exception cref="RRQMNotConnectedException"><inheritdoc/></exception>
        /// <exception cref="RRQMOverlengthException"><inheritdoc/></exception>
        /// <exception cref="RRQMException"><inheritdoc/></exception>
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
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"><inheritdoc/></param>
        /// <exception cref="RRQMNotConnectedException"><inheritdoc/></exception>
        /// <exception cref="RRQMOverlengthException"><inheritdoc/></exception>
        /// <exception cref="RRQMException"><inheritdoc/></exception>
        public virtual void SendAsync(byte[] buffer)
        {
            this.SendAsync(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"><inheritdoc/></param>
        /// <exception cref="RRQMNotConnectedException"><inheritdoc/></exception>
        /// <exception cref="RRQMOverlengthException"><inheritdoc/></exception>
        /// <exception cref="RRQMException"><inheritdoc/></exception>
        public virtual void SendAsync(ByteBlock byteBlock)
        {
            this.SendAsync(byteBlock.Buffer, 0, byteBlock.Len);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"><inheritdoc/></param>
        /// <exception cref="RRQMNotConnectedException"><inheritdoc/></exception>
        /// <exception cref="RRQMOverlengthException"><inheritdoc/></exception>
        /// <exception cref="RRQMException"><inheritdoc/></exception>
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

        #region 默认发送
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"><inheritdoc/></param>
        /// <param name="offset"><inheritdoc/></param>
        /// <param name="length"><inheritdoc/></param>
        /// <exception cref="RRQMNotConnectedException"><inheritdoc/></exception>
        /// <exception cref="RRQMOverlengthException"><inheritdoc/></exception>
        /// <exception cref="RRQMException"><inheritdoc/></exception>
        public void DefaultSend(byte[] buffer, int offset, int length)
        {
            this.SocketSend(buffer, offset, length, false);
        }

        /// <summary>
        ///<inheritdoc/>
        /// </summary>
        /// <param name="buffer"><inheritdoc/></param>
        /// <exception cref="RRQMNotConnectedException"><inheritdoc/></exception>
        /// <exception cref="RRQMOverlengthException"><inheritdoc/></exception>
        /// <exception cref="RRQMException"><inheritdoc/></exception>
        public void DefaultSend(byte[] buffer)
        {
            this.DefaultSend(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"><inheritdoc/></param>
        /// <exception cref="RRQMNotConnectedException"><inheritdoc/></exception>
        /// <exception cref="RRQMOverlengthException"><inheritdoc/></exception>
        /// <exception cref="RRQMException"><inheritdoc/></exception>
        public void DefaultSend(ByteBlock byteBlock)
        {
            this.DefaultSend(byteBlock.Buffer, 0, byteBlock.Len);
        }
        #endregion

        private void LoadSocketAndReadIpPort()
        {
            if (this.m_mainSocket == null)
            {
                this.IP = null;
                this.Port = -1;
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
            this.IP = ipport.Substring(0, r);
            this.Port = Convert.ToInt32(ipport.Substring(r + 1, ipport.Length - (r + 1)));
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
                this.BreakOut("远程终端主动关闭", false);
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
                    if (this.m_useSsl)
                    {
                        this.m_workStream.Write(buffer, offset, length);
                    }
                    else
                    {
                        if (isAsync)
                        {
                            if (this.m_separateThreadSend)
                            {
                                this.m_asyncSender.AsyncSend(buffer, offset, length);
                            }
                            else
                            {
                                this.m_mainSocket.BeginSend(buffer, offset, length, SocketFlags.None, null, null);
                            }
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
                }
            }
        }
    }
}