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

        /// <inheritdoc/>
        protected override async Task ReceivedData(ReceivedDataEventArgs e)
        {
            if (this.Received != null)
            {
                await this.Received.Invoke(this, e);
                if (e.Handled)
                {
                    return;
                }
            }
            await base.ReceivedData(e);
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
            this.m_receiveCounter = new ValueCounter
            {
                Period = TimeSpan.FromSeconds(1),
                OnPeriod = this.OnPeriod
            };
        }

        #region 变量

        private int m_receiveBufferSize = 1024 * 10;
        private volatile bool m_online;
        private NamedPipeClientStream m_pipeStream;
        private ValueCounter m_receiveCounter;
        private SemaphoreSlim m_semaphoreSlimForConnect = new SemaphoreSlim(1, 1);

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

        private Task PrivateOnConnected(object o)
        {
            var e = (ConnectedEventArgs)o;
            return this.OnConnected(e);
        }

        /// <summary>
        /// 已经建立管道连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnConnected(ConnectedEventArgs e)
        {
            try
            {
                if (this.Connected != null)
                {
                    await this.Connected.Invoke(this, e);
                    if (e.Handled)
                    {
                        return;
                    }
                }

                await this.PluginsManager.RaiseAsync(nameof(INamedPipeConnectedPlugin.OnNamedPipeConnected), this, e);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.Connected)}中发生错误。", ex);
            }
        }

        private Task PrivateOnConnecting(object obj)
        {
            var e = (ConnectingEventArgs)obj;

            if (this.CanSetDataHandlingAdapter)
            {
                this.SetDataHandlingAdapter(this.Config.GetValue(TouchSocketConfigExtension.TcpDataHandlingAdapterProperty).Invoke());
            }

            return this.OnConnecting(e);
        }

        /// <summary>
        /// 准备连接的时候，此时已初始化Socket，但是并未建立Tcp连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnConnecting(ConnectingEventArgs e)
        {
            try
            {
                if (this.Connecting != null)
                {
                    await this.Connecting.Invoke(this, e);
                    if (e.Handled)
                    {
                        return;
                    }
                }

                await this.PluginsManager.RaiseAsync(nameof(INamedPipeConnectingPlugin.OnNamedPipeConnecting), this, e);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.OnConnecting)}中发生错误。", ex);
            }
        }

        private Task PrivateOnDisconnected(DisconnectEventArgs e)
        {
            this.m_receiver?.TryInputReceive(default, default);
            return this.OnDisconnected(e);
        }

        /// <summary>
        /// 断开连接。在客户端未设置连接状态时，不会触发
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnDisconnected(DisconnectEventArgs e)
        {
            try
            {
                if (this.Disconnected != null)
                {
                    await this.Disconnected.Invoke(this, e);
                    if (e.Handled)
                    {
                        return;
                    }
                }

                await this.PluginsManager.RaiseAsync(nameof(INamedPipeDisconnectedPlugin.OnNamedPipeDisconnected), this, e);
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
        public override int ReceiveBufferSize => this.m_receiveBufferSize;

        /// <inheritdoc/>
        public DateTime LastReceivedTime { get; private set; }

        /// <inheritdoc/>
        public DateTime LastSendTime { get; private set; }

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

        /// <inheritdoc/>
        public override int SendBufferSize => this.m_pipeStream.InBufferSize;

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
                    this.m_pipeStream.Close();
                    this.BreakOut(true, msg);
                }
            }
        }

        /// <summary>
        /// 中断链接
        /// </summary>
        /// <param name="manual"></param>
        /// <param name="msg"></param>
        protected void BreakOut(bool manual, string msg)
        {
            lock (this.SyncRoot)
            {
                if (this.m_online)
                {
                    this.m_online = false;
                    this.m_pipeStream.SafeDispose();
                    this.DataHandlingAdapter.SafeDispose();

                    _ = this.PrivateOnDisconnected(new DisconnectEventArgs(manual, msg));
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
                    this.PrivateOnDisconnecting(new DisconnectEventArgs(true, $"{nameof(Dispose)}主动断开"));
                    this.BreakOut(true, $"{nameof(Dispose)}主动断开");
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
            try
            {
                this.m_semaphoreSlimForConnect.Wait();

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
                this.PrivateOnConnecting(new ConnectingEventArgs(null)).GetFalseAwaitResult();

                namedPipe.Connect(timeout);
                if (namedPipe.IsConnected)
                {
                    this.m_pipeStream = namedPipe;
                    this.m_online = true;
                    this.BeginReceive();
                    Task.Factory.StartNew(this.PrivateOnConnected, new ConnectedEventArgs());
                    return;
                }
                throw new Exception("未知异常");
            }
            finally
            {
                this.m_semaphoreSlimForConnect.Release();
            }
        }

        /// <summary>
        /// 建立管道的连接。
        /// </summary>
        /// <param name="timeout"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        /// <exception cref="TimeoutException"></exception>
        protected async Task PipeConnectAsync(int timeout)
        {
            try
            {
                await this.m_semaphoreSlimForConnect.WaitAsync();

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
                await this.PrivateOnConnecting(new ConnectingEventArgs(null));
#if NET45
                namedPipe.Connect(timeout);
#else
                await namedPipe.ConnectAsync(timeout);
#endif

                if (namedPipe.IsConnected)
                {
                    this.m_pipeStream = namedPipe;
                    this.m_online = true;
                    this.BeginReceive();
                    _ = Task.Factory.StartNew(this.PrivateOnConnected, new ConnectedEventArgs());
                    return;
                }
                throw new Exception("未知异常");
            }
            finally
            {
                this.m_semaphoreSlimForConnect.Release();
            }
        }

        /// <inheritdoc/>
        public virtual INamedPipeClient Connect(int timeout = 5000)
        {
            this.PipeConnect(timeout);
            return this;
        }

        /// <inheritdoc/>
        public async Task<INamedPipeClient> ConnectAsync(int timeout = 5000)
        {
            await this.PipeConnectAsync(timeout);
            return this;
        }

        #endregion Connect

        #region Receiver

        private Receiver m_receiver;

        /// <inheritdoc/>
        public IReceiver CreateReceiver()
        {
            return this.m_receiver ??= new Receiver(this);
        }

        /// <inheritdoc/>
        public void ClearReceiver()
        {
            this.m_receiver = null;
        }

        #endregion Receiver

        private void OnPeriod(long value)
        {
            this.m_receiveBufferSize = TouchSocketUtility.HitBufferLength(value);
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
            if (this.m_receiver != null)
            {
                if (this.m_receiver.TryInputReceive(byteBlock, requestInfo))
                {
                    return;
                }
            }
            this.ReceivedData(new ReceivedDataEventArgs(byteBlock, requestInfo)).GetFalseAwaitResult();
        }

        /// <summary>
        /// 处理已接收到的数据。
        /// <para>根据不同的数据处理适配器，会传递不同的数据</para>
        /// </summary>
        /// <param name="e"></param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual Task ReceivedData(ReceivedDataEventArgs e)
        {
            return this.PluginsManager.RaiseAsync(nameof(INamedPipeReceivedPlugin.OnNamedPipeReceived), this, e);
        }

        /// <summary>
        /// 当即将发送时，如果覆盖父类方法，则不会触发插件。
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <returns>返回值表示是否允许发送</returns>
        protected virtual async Task<bool> SendingData(byte[] buffer, int offset, int length)
        {
            if (this.PluginsManager.Enable)
            {
                var args = new SendingEventArgs(buffer, offset, length);
                await this.PluginsManager.RaiseAsync(nameof(INamedPipeSendingPlugin.OnNamedPipeSending), this, args)
                     .ConfigureAwait(false);
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
            adapter.SendAsyncCallBack = this.DefaultSendAsync;
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
                        this.BreakOut(false, "远程终端主动关闭");
                        return;
                    }

                    byteBlock.SetLength(r);
                    this.HandleBuffer(byteBlock);
                }
                catch (Exception ex)
                {
                    this.BreakOut(false, ex.Message);
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
        /// 当收到原始数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual Task<bool> ReceivingData(ByteBlock byteBlock)
        {
            if (this.PluginsManager.GetPluginCount(nameof(INamedPipeReceivingPlugin.OnNamedPipeReceiving)) > 0)
            {
                return this.PluginsManager.RaiseAsync(nameof(INamedPipeReceivingPlugin.OnNamedPipeReceiving), this, new ByteBlockEventArgs(byteBlock));
            }
            return Task.FromResult(false);
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

                this.m_receiveCounter.Increment(byteBlock.Length);
                if (this.ReceivingData(byteBlock).ConfigureAwait(false).GetAwaiter().GetResult())
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

        /// <inheritdoc/>
        public virtual async Task SendAsync(byte[] buffer, int offset, int length)
        {
            if (this.DisposedValue)
            {
                return;
            }
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }
            await this.DataHandlingAdapter.SendInputAsync(buffer, offset, length);
        }

        /// <inheritdoc/>
        public virtual async Task SendAsync(IRequestInfo requestInfo)
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
            await this.DataHandlingAdapter.SendInputAsync(requestInfo);
        }

        /// <inheritdoc/>
        public virtual async Task SendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            this.ThrowIfDisposed();
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
                    await this.DataHandlingAdapter.SendInputAsync(byteBlock.Buffer, 0, byteBlock.Len);
                }
            }
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
            if (this.SendingData(buffer, offset, length).GetFalseAwaitResult())
            {
                this.m_pipeStream.Write(buffer, offset, length);
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
        public async Task DefaultSendAsync(byte[] buffer, int offset, int length)
        {
            if (await this.SendingData(buffer, offset, length))
            {
                await this.m_pipeStream.WriteAsync(buffer, offset, length);
                this.LastSendTime = DateTime.Now;
            }
        }

        #endregion 发送
    }
}