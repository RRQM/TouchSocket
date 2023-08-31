using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 命名管道客户端
    /// </summary>
    public class NamedPipeClient : NamedPipeClientBase
    {
        /// <summary>
        /// 接收到数据
        /// </summary>
        public ReceivedEventHandler<NamedPipeClient> Received { get; set; }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected override bool HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.Received?.Invoke(this, byteBlock, requestInfo);
            return false;
        }
    }

    /// <summary>
    /// 命名管道客户端客户端基类
    /// </summary>
    public class NamedPipeClientBase : BaseSocket, INamedPipeClient
    {
        /// <summary>
        /// 命名管道客户端客户端基类
        /// </summary>
        public NamedPipeClientBase()
        {
            this.Protocol = Protocol.NamedPipe;
            this.m_valueCounter = new ValueCounter
            {
                Period = TimeSpan.FromSeconds(1),
                OnPeriod = this.OnPeriod
            };
        }

        #region 变量
        private DelaySender m_delaySender;
        private volatile bool m_online;
        private NamedPipeClientStream m_pipeStream;
        ValueCounter m_valueCounter;
        #endregion 变量

        #region 事件

        /// <inheritdoc/>
        public ConnectedEventHandler<INamedPipeClient> Connected { get; set; }
        /// <inheritdoc/>
        public ConnectingEventHandler<INamedPipeClient> Connecting { get; set; }

        /// <inheritdoc/>
        public DisconnectEventHandler<INamedPipeClientBase> Disconnected { get; set; }

        /// <inheritdoc/>
        public DisconnectEventHandler<INamedPipeClientBase> Disconnecting { get; set; }

        private void PrivateOnConnected(object o)
        {
            var e = (ConnectedEventArgs)o;
            this.OnConnected(e);
            if (e.Handled)
            {
                return;
            }
            this.PluginsManager.Raise(nameof(INamedPipeConnectedPlugin.OnNamedPipeConnected), this, e);
        }

        /// <summary>
        /// 已经建立Tcp连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnConnected(ConnectedEventArgs e)
        {
            try
            {
                this.Connected?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.Connected)}中发生错误。", ex);
            }
        }

        private void PrivateOnConnecting(ConnectingEventArgs e)
        {
            this.LastReceivedTime = DateTime.Now;
            this.LastSendTime = DateTime.Now;
            if (this.CanSetDataHandlingAdapter)
            {
                this.SetDataHandlingAdapter(this.Config.GetValue(TouchSocketConfigExtension.TcpDataHandlingAdapterProperty).Invoke());
            }

            this.OnConnecting(e);
            if (e.Handled)
            {
                return;
            }
            this.PluginsManager.Raise(nameof(INamedPipeConnectingPlugin.OnNamedPipeConnecting), this, e);
        }

        /// <summary>
        /// 准备连接的时候，此时已初始化Socket，但是并未建立Tcp连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnConnecting(ConnectingEventArgs e)
        {
            try
            {
                this.Connecting?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.OnConnecting)}中发生错误。", ex);
            }
        }

        private void PrivateOnDisconnected(DisconnectEventArgs e)
        {
            this.OnDisconnected(e);
            if (e.Handled)
            {
                return;
            }
            this.PluginsManager.Raise(nameof(INamedPipeDisconnectedPlugin.OnNamedPipeDisconnected), this, e);
        }

        /// <summary>
        /// 断开连接。在客户端未设置连接状态时，不会触发
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(DisconnectEventArgs e)
        {
            try
            {
                this.Disconnected?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.Disconnected)}中发生错误。", ex);
            }
        }

        private void PrivateOnDisconnecting(DisconnectEventArgs e)
        {
            this.OnDisconnecting(e);
            if (e.Handled)
            {
                return;
            }
            this.PluginsManager.Raise(nameof(INamedPipeDisconnectingPlugin.OnNamedPipeDisconnecting), this, e);
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// <para>
        /// 当主动调用Close断开时。
        /// </para>
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDisconnecting(DisconnectEventArgs e)
        {
            try
            {
                this.Disconnecting?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.Disconnecting)}中发生错误。", ex);
            }
        }

        #endregion 事件

        #region 属性

        /// <inheritdoc/>
        public DateTime LastReceivedTime { get; private set; }

        /// <inheritdoc/>
        public DateTime LastSendTime { get; private set; }

        /// <inheritdoc/>
        public Func<ByteBlock, bool> OnHandleRawBuffer { get; set; }

        /// <inheritdoc/>
        public Func<ByteBlock, IRequestInfo, bool> OnHandleReceivedData { get; set; }

        /// <inheritdoc/>
        public IContainer Container { get; private set; }

        /// <inheritdoc/>
        public virtual bool CanSetDataHandlingAdapter => true;

        /// <inheritdoc/>
        public TouchSocketConfig Config { get; private set; }

        /// <inheritdoc/>
        public SingleStreamDataHandlingAdapter DataHandlingAdapter { get; private set; }


        /// <inheritdoc/>
        public PipeStream PipeStream => this.m_pipeStream;

        /// <inheritdoc/>
        public bool Online { get => this.m_online; }

        /// <inheritdoc/>
        public bool CanSend => this.m_online;

        /// <inheritdoc/>
        public IPluginsManager PluginsManager { get; private set; }

        /// <inheritdoc/>
        public Protocol Protocol { get; set; }

        /// <inheritdoc/>
        public bool IsClient => true;

        #endregion 属性

        #region 断开操作

        /// <inheritdoc/>
        public virtual void Close(string msg = TouchSocketCoreUtility.Empty)
        {
            lock (this.SyncRoot)
            {
                if (this.m_online)
                {
                    this.PrivateOnDisconnecting(new DisconnectEventArgs(true, msg));

                    this.m_online = false;
                    this.m_pipeStream.SafeDispose();
                    this.m_delaySender.SafeDispose();
                    this.DataHandlingAdapter.SafeDispose();
                    this.PrivateOnDisconnected(new DisconnectEventArgs(true, msg));
                }
            }
        }

        private void BreakOut(string msg)
        {
            lock (this.SyncRoot)
            {
                if (this.m_online)
                {
                    this.m_online = false;
                    this.m_pipeStream.SafeDispose();
                    this.m_delaySender.SafeDispose();
                    this.DataHandlingAdapter.SafeDispose();
                    this.PrivateOnDisconnected(new DisconnectEventArgs(false, msg));
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            lock (this.SyncRoot)
            {
                if (this.m_online)
                {
                    this.m_online = false;
                    this.m_pipeStream.SafeDispose();
                    this.PrivateOnDisconnecting(new DisconnectEventArgs(true, $"{nameof(Dispose)}主动断开"));

                    this.m_delaySender.SafeDispose();
                    this.DataHandlingAdapter.SafeDispose();
                    this.PluginsManager.SafeDispose();
                    this.PrivateOnDisconnected(new DisconnectEventArgs(true, $"{nameof(Dispose)}主动断开"));
                }
            }
            base.Dispose(disposing);
        }

        #endregion 断开操作

        #region Connect
        /// <summary>
        /// 建立管道的连接。
        /// </summary>
        /// <param name="timeout"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        /// <exception cref="TimeoutException"></exception>
        protected void PipeConnect(int timeout)
        {
            lock (this.SyncRoot)
            {
                if (this.m_online)
                {
                    return;
                }
                if (this.DisposedValue)
                {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }
                if (this.Config == null)
                {
                    throw new ArgumentNullException(nameof(this.Config), "配置文件不能为空。");
                }
                var pipeName = this.Config.GetValue(NamedPipeConfigExtension.PipeNameProperty) ?? throw new ArgumentNullException("PipeName", "命名管道连接参数不能为空。");

                var serverName = this.Config.GetValue(NamedPipeConfigExtension.PipeServerNameProperty);
                this.m_pipeStream.SafeDispose();


                var namedPipe = CreatePipeClient(serverName, pipeName);
                this.PrivateOnConnecting(new ConnectingEventArgs(null));
                namedPipe.Connect(timeout);
                if (namedPipe.IsConnected)
                {
                    this.m_pipeStream = namedPipe;
                    this.m_online = true;
                    this.BeginReceive();
                    this.PrivateOnConnected(new ConnectedEventArgs());
                    return;
                }
                throw new Exception("未知异常");
            }
        }


        /// <inheritdoc/>
        public virtual INamedPipeClient Connect(int timeout = 5000)
        {
            this.PipeConnect(timeout);
            return this;
        }

        /// <inheritdoc/>
        public Task<INamedPipeClient> ConnectAsync(int timeout = 5000)
        {
            return Task.Run(() =>
            {
                return this.Connect(timeout);
            });
        }
        #endregion

        private void OnPeriod(long value)
        {
            this.ReceiveBufferSize = TouchSocketUtility.HitBufferLength(value);
        }

        /// <inheritdoc/>
        public virtual void SetDataHandlingAdapter(SingleStreamDataHandlingAdapter adapter)
        {
            if (!this.CanSetDataHandlingAdapter)
            {
                throw new Exception($"不允许自由调用{nameof(SetDataHandlingAdapter)}进行赋值。");
            }

            this.SetAdapter(adapter);
        }

        /// <inheritdoc/>
        public INamedPipeClient Setup(TouchSocketConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            this.ThrowIfDisposed();

            this.BuildConfig(config);

            this.PluginsManager.Raise(nameof(ILoadingConfigPlugin.OnLoadingConfig), this, new ConfigEventArgs(config));
            this.LoadConfig(this.Config);
            this.PluginsManager.Raise(nameof(ILoadedConfigPlugin.OnLoadedConfig), this, new ConfigEventArgs(config));

            return this;
        }

        private void BuildConfig(TouchSocketConfig config)
        {
            this.Config = config;

            if (!(config.GetValue(TouchSocketCoreConfigExtension.ContainerProperty) is IContainer container))
            {
                container = new Container();
            }

            if (!container.IsRegistered(typeof(ILog)))
            {
                container.RegisterSingleton<ILog, LoggerGroup>();
            }

            if (!(config.GetValue(TouchSocketCoreConfigExtension.PluginsManagerProperty) is IPluginsManager pluginsManager))
            {
                pluginsManager = new PluginsManager(container);
            }

            if (container.IsRegistered(typeof(IPluginsManager)))
            {
                pluginsManager = container.Resolve<IPluginsManager>();
            }
            else
            {
                container.RegisterSingleton<IPluginsManager>(pluginsManager);
            }

            if (config.GetValue(TouchSocketCoreConfigExtension.ConfigureContainerProperty) is Action<IContainer> actionContainer)
            {
                actionContainer.Invoke(container);
            }

            if (config.GetValue(TouchSocketCoreConfigExtension.ConfigurePluginsProperty) is Action<IPluginsManager> actionPluginsManager)
            {
                pluginsManager.Enable = true;
                actionPluginsManager.Invoke(pluginsManager);
            }
            this.Container = container;
            this.PluginsManager = pluginsManager;
        }

        private void PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.OnHandleReceivedData?.Invoke(byteBlock, requestInfo) == false)
            {
                return;
            }

            if (this.HandleReceivedData(byteBlock, requestInfo))
            {
                return;
            }

            if (this.PluginsManager.Enable)
            {
                var args = new ReceivedDataEventArgs(byteBlock, requestInfo);
                this.PluginsManager.Raise(nameof(INamedPipeReceivedPlugin.OnNamedPipeReceived), this, args);
            }
        }

        /// <summary>
        /// 处理已接收到的数据。
        /// </summary>
        /// <param name="byteBlock">以二进制流形式传递</param>
        /// <param name="requestInfo">以解析的数据对象传递</param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual bool HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            return false;
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
            if (this.PluginsManager.Enable)
            {
                var args = new SendingEventArgs(buffer, offset, length);
                this.PluginsManager.Raise(nameof(INamedPipeSendingPlugin.OnNamedPipeSending), this, args);
                return args.IsPermitOperation;
            }
            return true;
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="config"></param>
        protected virtual void LoadConfig(TouchSocketConfig config)
        {
            this.Logger ??= this.Container.Resolve<ILog>();
        }

        /// <summary>
        /// 在延迟发生错误
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void OnDelaySenderError(Exception ex)
        {
            this.Logger.Log(LogLevel.Error, this, "发送错误", ex);
        }

        /// <summary>
        /// 设置适配器，该方法不会检验<see cref="CanSetDataHandlingAdapter"/>的值。
        /// </summary>
        /// <param name="adapter"></param>
        protected void SetAdapter(SingleStreamDataHandlingAdapter adapter)
        {
            this.ThrowIfDisposed();
            if (adapter is null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            if (this.Config != null)
            {
                adapter.Config(this.Config);
            }

            adapter.Logger = this.Logger;
            adapter.OnLoaded(this);
            adapter.ReceivedCallBack = this.PrivateHandleReceivedData;
            adapter.SendCallBack = this.DefaultSend;
            this.DataHandlingAdapter = adapter;
        }

        private void BeginReceive()
        {
            Task.Factory.StartNew(this.BeginBio, TaskCreationOptions.LongRunning);
        }

        private async Task BeginBio()
        {
            while (true)
            {
                var byteBlock = new ByteBlock(this.ReceiveBufferSize);
                try
                {
                   
                    var r = await Task<int>.Factory.FromAsync(this.m_pipeStream.BeginRead, this.m_pipeStream.EndRead, byteBlock.Buffer, 0, byteBlock.Capacity, null);
                    if (r == 0)
                    {
                        this.BreakOut("远程终端主动关闭");
                        return;
                    }

                    byteBlock.SetLength(r);
                    this.HandleBuffer(byteBlock);
                }
                catch (Exception ex)
                {
                    this.BreakOut(ex.Message);
                    return;
                }
            }
        }

        private static NamedPipeClientStream CreatePipeClient(string serverName, string pipeName)
        {
            var pipeClient = new NamedPipeClientStream(serverName, pipeName, PipeDirection.InOut, PipeOptions.Asynchronous, TokenImpersonationLevel.None);
            return pipeClient;
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        private void HandleBuffer(ByteBlock byteBlock)
        {
            try
            {
                if (this.DisposedValue)
                {
                    return;
                }
                this.m_valueCounter.Increment(byteBlock.Length);
                this.LastReceivedTime = DateTime.Now;
                if (this.OnHandleRawBuffer?.Invoke(byteBlock) == false)
                {
                    return;
                }
                if (this.DisposedValue)
                {
                    return;
                }
                if (this.PluginsManager.Enable && this.PluginsManager.Raise(nameof(INamedPipeReceivingPlugin.OnNamedPipeReceiving), this, new ByteBlockEventArgs(byteBlock)))
                {
                    return;
                }
                if (this.DataHandlingAdapter == null)
                {
                    this.Logger.Error(this, TouchSocketResource.NullDataAdapter.GetDescription());
                    return;
                }
                this.DataHandlingAdapter.ReceivedInput(byteBlock);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, "在处理数据时发生错误", ex);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        #region 发送

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
            if (this.DisposedValue)
            {
                return;
            }
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }
            if (!this.DataHandlingAdapter.CanSendRequestInfo)
            {
                throw new NotSupportedException($"当前适配器不支持对象发送。");
            }
            this.DataHandlingAdapter.SendInput(requestInfo);
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
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }
            this.DataHandlingAdapter.SendInput(buffer, offset, length);
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
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }

            if (this.DataHandlingAdapter.CanSplicingSend)
            {
                this.DataHandlingAdapter.SendInput(transferBytes);
            }
            else
            {
                var length = 0;
                foreach (var item in transferBytes)
                {
                    length += item.Count;
                }
                using (var byteBlock = new ByteBlock(length))
                {
                    foreach (var item in transferBytes)
                    {
                        byteBlock.Write(item.Array, item.Offset, item.Count);
                    }
                    this.DataHandlingAdapter.SendInput(byteBlock.Buffer, 0, byteBlock.Len);
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
        public virtual Task SendAsync(byte[] buffer, int offset, int length)
        {
            return Task.Run(() =>
            {
                this.Send(buffer, offset, length);
            });
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual Task SendAsync(IRequestInfo requestInfo)
        {
            return Task.Run(() =>
            {
                this.Send(requestInfo);
            });
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"><inheritdoc/></param>
        /// <exception cref="NotConnectedException"><inheritdoc/></exception>
        /// <exception cref="OverlengthException"><inheritdoc/></exception>
        /// <exception cref="Exception"><inheritdoc/></exception>
        public virtual Task SendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            return Task.Run(() =>
            {
                this.Send(transferBytes);
            });
        }

        #endregion 异步发送

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
            if (!this.m_online)
            {
                throw new NotConnectedException(TouchSocketResource.NotConnected.GetDescription());
            }
            if (this.HandleSendingData(buffer, offset, length))
            {
                this.m_pipeStream.Write(buffer, offset, length);
                //if (this.UseSsl)
                //{
                //    this.m_workStream.Write(buffer, offset, length);
                //}
                //else
                //{
                //    if (this.m_delaySender != null && length < m_delaySender.DelayLength)
                //    {
                //        this.m_delaySender.Send(QueueDataBytes.CreateNew(buffer, offset, length));
                //    }
                //    else
                //    {
                //        this.MainSocket.AbsoluteSend(buffer, offset, length);
                //    }
                //}

                this.LastSendTime = DateTime.Now;
            }
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
        public Task DefaultSendAsync(byte[] buffer, int offset, int length)
        {
            return Task.Run(() =>
            {
                this.DefaultSend(buffer, offset, length);
            });
        }

        #endregion 发送
    }
}
