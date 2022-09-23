//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Collections.Concurrent;
using TouchSocket.Core.Config;
using TouchSocket.Core.Data.Security;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Core.Plugins;
using TouchSocket.Resources;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 简单TCP客户端
    /// </summary>
    public class TcpClient : TcpClientBase
    {
        /// <summary>
        /// 接收到数据
        /// </summary>
        public  ReceivedEventHandler<TcpClient> Received { get; set; }

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
    public class TcpClientBase : BaseSocket, ITcpClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpClientBase()
        {
            this.Protocol = Protocol.TCP;
        }

        #region 变量

        private DelaySender m_delaySender;
        private TouchSocketConfig m_config;
        private DataHandlingAdapter m_adapter;
        private Socket m_mainSocket;
        private bool m_online;
        private ReceiveType m_receiveType;
        private bool m_useDelaySender;
        private bool m_usePlugin;
        private Stream m_workStream;
        private int m_maxPackageSize;
        private IPHost m_remoteIPHost;
        #endregion 变量

        #region 事件

        /// <summary>
        /// 成功连接到服务器
        /// </summary>
        public MessageEventHandler<ITcpClient> Connected { get; set; }

        /// <summary>
        /// 准备连接的时候，此时已初始化Socket，但是并未建立Tcp连接
        /// </summary>
        public ClientConnectingEventHandler<ITcpClient> Connecting { get; set; }

        /// <summary>
        /// 断开连接。在客户端未设置连接状态时，不会触发
        /// </summary>
        public ClientDisconnectedEventHandler<ITcpClientBase> Disconnected { get; set; }

        private void PrivateOnConnected(MsgEventArgs e)
        {
            if (this.m_usePlugin)
            {
                this.PluginsManager.Raise<ITcpPlugin>(nameof(ITcpPlugin.OnConnected), this, e);
                if (e.Handled)
                {
                    return;
                }
            }
            this.OnConnected(e);
        }

        /// <summary>
        /// 已经建立Tcp连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnConnected(MsgEventArgs e)
        {
            try
            {
                this.Connected?.Invoke(this, e);
            }
            catch (System.Exception ex)
            {
                this.Logger.Log(LogType.Error, this, $"在事件{nameof(this.Connected)}中发生错误。", ex);
            }
        }

        private void PrivateOnConnecting(ClientConnectingEventArgs e)
        {
            if (this.CanSetDataHandlingAdapter)
            {
                this.SetDataHandlingAdapter(this.Config.GetValue<Func<DataHandlingAdapter>>(TouchSocketConfigExtension.DataHandlingAdapterProperty).Invoke());
            }
            if (this.m_usePlugin)
            {
                this.PluginsManager.Raise<ITcpPlugin>(nameof(ITcpPlugin.OnConnecting), this, e);
                if (e.Handled)
                {
                    return;
                }
            }
            this.OnConnecting(e);
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
                this.Logger.Log(LogType.Error, this, $"在事件{nameof(this.OnConnecting)}中发生错误。", ex);
            }
        }

        private void PrivateOnDisconnected(ClientDisconnectedEventArgs e)
        {
            if (this.m_usePlugin)
            {
                this.PluginsManager.Raise<ITcpPlugin>(nameof(ITcpPlugin.OnDisconnected), this, e);
                if (e.Handled)
                {
                    return;
                }
            }
            this.OnDisconnected(e);
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
                this.Logger.Log(LogType.Error, this, $"在事件{nameof(this.Disconnected)}中发生错误。", ex);
            }
        }

        #endregion 事件

        #region 属性

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DateTime LastReceivedTime { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DateTime LastSendTime { get; private set; }

        /// <summary>
        /// 处理未经过适配器的数据。返回值表示是否继续向下传递。
        /// </summary>
        public Func<ByteBlock, bool> OnHandleRawBuffer { get; set; }

        /// <summary>
        /// 处理经过适配器后的数据。返回值表示是否继续向下传递。
        /// </summary>
        public Func<ByteBlock, IRequestInfo, bool> OnHandleReceivedData { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IContainer Container => this.m_config?.Container;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool CanSetDataHandlingAdapter => true;

        /// <summary>
        /// 客户端配置
        /// </summary>
        public TouchSocketConfig Config => this.m_config;

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
        public IPluginsManager PluginsManager => this.m_config?.PluginsManager;

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ReceiveType ReceiveType => this.m_receiveType;

        /// <summary>
        /// 是否已启用插件
        /// </summary>
        public bool UsePlugin => this.m_usePlugin;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool UseSsl { get; private set; }

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
        public IPHost RemoteIPHost => this.m_remoteIPHost;

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
            this.BreakOut(msg, true);
        }

        private void BreakOut(string msg, bool manual)
        {
            lock (this)
            {
                if (this.m_online)
                {
                    this.m_online = false;
                    this.SafeShutdown();
                    this.m_mainSocket.SafeDispose();
                    this.m_delaySender.SafeDispose();
                    this.m_workStream.SafeDispose();
                    this.m_adapter.SafeDispose();
                    this.PrivateOnDisconnected(new ClientDisconnectedEventArgs(manual, msg));
                }
            }
        }

        /// <summary>
        /// 禁用发送或接收
        /// </summary>
        /// <param name="how"></param>
        public void Shutdown(SocketShutdown how)
        {
            if (this.m_mainSocket == null)
            {
                return;
            }
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
            if (disposing)
            {
                this.PluginsManager.Clear();
                this.m_config = default;
                this.m_adapter.SafeDispose();
                this.m_adapter = default;
            }
        }

        #endregion 断开操作

        /// <summary>
        /// 请求连接到服务器。
        /// </summary>
        public virtual ITcpClient Connect(int timeout = 5000)
        {
            if (this.m_online)
            {
                return this;
            }
            if (this.m_disposedValue)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (this.m_config == null)
            {
                throw new ArgumentNullException("配置文件不能为空。");
            }
            IPHost iPHost = this.m_config.GetValue<IPHost>(TouchSocketConfigExtension.RemoteIPHostProperty);
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
            this.PrivateOnConnecting(args);

            var result = this.m_mainSocket.BeginConnect(iPHost.EndPoint, null, null);
            if (result.AsyncWaitHandle.WaitOne(timeout))
            {
                if (this.m_mainSocket.Connected)
                {
                    this.m_mainSocket.EndConnect(result);
                    this.LoadSocketAndReadIpPort();

                    if (this.m_config.GetValue<DelaySenderOption>(TouchSocketConfigExtension.DelaySenderProperty) is DelaySenderOption senderOption)
                    {
                        this.m_useDelaySender = true;
                        this.m_delaySender.SafeDispose();
                        this.m_delaySender = new DelaySender(this.m_mainSocket, senderOption.QueueLength, this.OnDelaySenderError)
                        {
                            DelayLength = senderOption.DelayLength
                        };
                    }

                    this.BeginReceive();
                    this.m_online = true;
                    this.PrivateOnConnected(new MsgEventArgs("连接成功"));
                    return this;
                }
            }
            this.m_mainSocket.Dispose();
            throw new TimeoutException();
        }

        /// <summary>
        /// 异步连接服务器
        /// </summary>
        public Task<ITcpClient> ConnectAsync(int timeout = 5000)
        {
            return Task.Run(() =>
            {
                return this.Connect(timeout);
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
                throw new Exception($"不允许自由调用{nameof(SetDataHandlingAdapter)}进行赋值。");
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
            TouchSocketConfig config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost(ipHost));
            return this.Setup(config);
        }

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="config"></param>
        /// <exception cref="Exception"></exception>
        public ITcpClient Setup(TouchSocketConfig config)
        {
            this.m_config = config;
            if (config.IsUsePlugin)
            {
                this.PluginsManager.Raise<IConfigPlugin>(nameof(IConfigPlugin.OnLoadingConfig), this, new ConfigEventArgs(config));
            }
            this.LoadConfig(this.m_config);
            if (this.UsePlugin)
            {
                this.PluginsManager.Raise<IConfigPlugin>(nameof(IConfigPlugin.OnLoadedConfig), this, new ConfigEventArgs(config));
            }
            return this;
        }

        private void PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.OnHandleReceivedData?.Invoke(byteBlock, requestInfo) == false)
            {
                return;
            }
            if (this.m_usePlugin)
            {
                ReceivedDataEventArgs args = new ReceivedDataEventArgs(byteBlock, requestInfo);
                this.PluginsManager.Raise<ITcpPlugin>(nameof(ITcpPlugin.OnReceivedData), this, args);
                if (args.Handled)
                {
                    return;
                }
            }

            this.HandleReceivedData(byteBlock, requestInfo);
        }

        /// <summary>
        /// 处理已接收到的数据。
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
                this.PluginsManager.Raise<ITcpPlugin>(nameof(ITcpPlugin.OnSendingData), this, args);
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
        protected virtual void LoadConfig(TouchSocketConfig config)
        {
            if (config == null)
            {
                throw new Exception("配置文件为空");
            }
            this.m_remoteIPHost = config.GetValue<IPHost>(TouchSocketConfigExtension.RemoteIPHostProperty);
            this.m_maxPackageSize = config.GetValue<int>(TouchSocketConfigExtension.MaxPackageSizeProperty);
            this.BufferLength = config.GetValue<int>(TouchSocketConfigExtension.BufferLengthProperty);
            this.m_receiveType = config.GetValue<ReceiveType>(TouchSocketConfigExtension.ReceiveTypeProperty);
            this.m_usePlugin = config.IsUsePlugin;
            this.Logger = this.Container.Resolve<ILog>();
            if (config.GetValue(TouchSocketConfigExtension.SslOptionProperty) != null)
            {
                this.UseSsl = true;
            }
        }

        /// <summary>
        /// 在延迟发生错误
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void OnDelaySenderError(Exception ex)
        {
            this.Logger.Log(LogType.Error, this, "发送错误", ex);
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

            adapter.OnLoaded(this);
            adapter.ReceivedCallBack = this.PrivateHandleReceivedData;
            adapter.SendCallBack = this.SocketSend;
            if (this.Config != null)
            {
                adapter.MaxPackageSize = Math.Max(adapter.MaxPackageSize, this.Config.GetValue<int>(TouchSocketConfigExtension.MaxPackageSizeProperty));
            }
            this.m_adapter = adapter;
        }

        private void BeginReceive()
        {
            if (this.m_workStream != null)
            {
                this.m_workStream.Dispose();
            }

            if (this.UseSsl)
            {
                ClientSslOption sslOption = this.m_config.GetValue<ClientSslOption>(TouchSocketConfigExtension.SslOptionProperty);
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
                if (r == 0)
                {
                    this.BreakOut("远程终端主动关闭", false);
                }
                byteBlock.SetLength(r);
                this.HandleBuffer(byteBlock);
                this.BeginSsl();
            }
            catch (System.Exception ex)
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
#if NET45_OR_GREATER
            KeepAliveValue keepAliveValue = this.m_config.GetValue<KeepAliveValue>(TouchSocketConfigExtension.KeepAliveValueProperty);
            if (keepAliveValue.Enable)
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                socket.IOControl(IOControlCode.KeepAliveValues, keepAliveValue.KeepAliveTime, null);
            }
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                KeepAliveValue keepAliveValue = this.m_config.GetValue<KeepAliveValue>(TouchSocketConfigExtension.KeepAliveValueProperty);
                if (keepAliveValue.Enable)
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    socket.IOControl(IOControlCode.KeepAliveValues, keepAliveValue.KeepAliveTime, null);
                }
            }
#endif
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, this.m_config.GetValue<bool>(TouchSocketConfigExtension.NoDelayProperty));
            if (this.m_config.GetValue<IPHost>(TouchSocketConfigExtension.BindIPHostProperty) != null)
            {
                if (this.m_config.GetValue<bool>(TouchSocketConfigExtension.ReuseAddressProperty))
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                }
                socket.Bind(this.m_config.GetValue<IPHost>(TouchSocketConfigExtension.BindIPHostProperty).EndPoint);
            }
            return socket;
        }

        private void EventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                this.ProcessReceived(e);
            }
            catch (System.Exception ex)
            {
                e.Dispose();
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
                this.LastReceivedTime = DateTime.Now;
                if (this.OnHandleRawBuffer?.Invoke(byteBlock) == false)
                {
                    return;
                }
                if (this.m_disposedValue)
                {
                    return;
                }
                if (this.UsePlugin && this.PluginsManager.Raise<ITcpPlugin>(nameof(ITcpPlugin.OnReceivingData), this, new ByteBlockEventArgs(byteBlock)))
                {
                    return;
                }
                if (this.m_adapter == null)
                {
                    this.Logger.Error(this, TouchSocketRes.NullDataAdapter.GetDescription());
                    return;
                }
                this.m_adapter.ReceivedInput(byteBlock);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogType.Error, this, "在处理数据时发生错误", ex);
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
        /// <param name="requestInfo"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public void Send(IRequestInfo requestInfo)
        {
            if (this.m_disposedValue)
            {
                return;
            }
            if (this.m_adapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketRes.NullDataAdapter.GetDescription());
            }
            if (!this.m_adapter.CanSendRequestInfo)
            {
                throw new NotSupportedException($"当前适配器不支持对象发送。");
            }
            this.m_adapter.SendInput(requestInfo, false);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"><inheritdoc/></param>
        /// <param name="offset"><inheritdoc/></param>
        /// <param name="length"><inheritdoc/></param>
        /// <exception cref="NotConnectedException"><inheritdoc/></exception>
        /// <exception cref="OverlengthException"><inheritdoc/></exception>
        /// <exception cref="Exception"><inheritdoc/></exception>
        public virtual void Send(byte[] buffer, int offset, int length)
        {
            if (this.m_adapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketRes.NullDataAdapter.GetDescription());
            }
            this.m_adapter.SendInput(buffer, offset, length, false);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"><inheritdoc/></param>
        /// <exception cref="NotConnectedException"><inheritdoc/></exception>
        /// <exception cref="OverlengthException"><inheritdoc/></exception>
        /// <exception cref="Exception"><inheritdoc/></exception>
        public virtual void Send(IList<ArraySegment<byte>> transferBytes)
        {
            if (this.m_adapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketRes.NullDataAdapter.GetDescription());
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
                        byteBlock.Write(item.Array, item.Offset, item.Count);
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
        /// <exception cref="NotConnectedException"><inheritdoc/></exception>
        /// <exception cref="OverlengthException"><inheritdoc/></exception>
        /// <exception cref="Exception"><inheritdoc/></exception>
        public virtual void SendAsync(byte[] buffer, int offset, int length)
        {
            if (this.m_disposedValue)
            {
                return;
            }
            if (this.m_adapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketRes.NullDataAdapter.GetDescription());
            }
            this.m_adapter.SendInput(buffer, offset, length, true);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public void SendAsync(IRequestInfo requestInfo)
        {
            if (this.m_disposedValue)
            {
                return;
            }
            if (this.m_adapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketRes.NullDataAdapter.GetDescription());
            }
            if (!this.m_adapter.CanSendRequestInfo)
            {
                throw new NotSupportedException($"当前适配器不支持对象发送。");
            }
            this.m_adapter.SendInput(requestInfo, true);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"><inheritdoc/></param>
        /// <exception cref="NotConnectedException"><inheritdoc/></exception>
        /// <exception cref="OverlengthException"><inheritdoc/></exception>
        /// <exception cref="Exception"><inheritdoc/></exception>
        public virtual void SendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            if (this.m_disposedValue)
            {
                return;
            }
            if (this.m_adapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketRes.NullDataAdapter.GetDescription());
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
                        byteBlock.Write(item.Array, item.Offset, item.Count);
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
        /// <exception cref="NotConnectedException"><inheritdoc/></exception>
        /// <exception cref="OverlengthException"><inheritdoc/></exception>
        /// <exception cref="Exception"><inheritdoc/></exception>
        public void DefaultSend(byte[] buffer, int offset, int length)
        {
            this.SocketSend(buffer, offset, length, false);
        }

        #endregion 默认发送

        #region 异步默认发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"><inheritdoc/></param>
        /// <param name="offset"><inheritdoc/></param>
        /// <param name="length"><inheritdoc/></param>
        /// <exception cref="NotConnectedException"><inheritdoc/></exception>
        /// <exception cref="OverlengthException"><inheritdoc/></exception>
        /// <exception cref="Exception"><inheritdoc/></exception>
        public void DefaultSendAsync(byte[] buffer, int offset, int length)
        {
            this.SocketSend(buffer, offset, length, true);
        }

        #endregion 异步默认发送

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
            if (!this.m_online)
            {
                e.Dispose();
                return;
            }
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
                catch (System.Exception ex)
                {
                    e.Dispose();
                    this.BreakOut(ex.Message, false);
                }
            }
            else
            {
                e.Dispose();
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
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        protected void SocketSend(byte[] buffer, int offset, int length, bool isAsync)
        {
            if (!this.m_online)
            {
                throw new NotConnectedException(TouchSocketRes.NotConnected.GetDescription());
            }
            if (this.HandleSendingData(buffer, offset, length))
            {
                if (this.UseSsl)
                {
                    if (isAsync)
                    {
                        this.m_workStream.WriteAsync(buffer, offset, length);
                    }
                    else
                    {
                        this.m_workStream.Write(buffer, offset, length);
                    }

                }
                else
                {
                    if (this.m_useDelaySender && length < TouchSocketUtility.BigDataBoundary)
                    {
                        if (isAsync)
                        {
                            this.m_delaySender.Send(new QueueDataBytes(buffer, offset, length));
                        }
                        else
                        {
                            this.m_delaySender.Send(QueueDataBytes.CreateNew(buffer, offset, length));
                        }
                    }
                    else
                    {
                        if (isAsync)
                        {
                            this.m_mainSocket.BeginSend(buffer, offset, length, SocketFlags.None, null, null);
                        }
                        else
                        {
                            this.m_mainSocket.AbsoluteSend(buffer, offset, length);
                        }
                    }
                }

                this.LastSendTime = DateTime.Now;
            }
        }
    }
}