//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// WebsocketDmtpClient
    /// </summary>
    public class WebsocketDmtpClient : BaseSocket, IWebsocketDmtpClient
    {
        #region 字段

        private bool m_allowRoute;
        private CancellationTokenSource m_cancellationTokenSource;
        private ClientWebSocket m_client;
        private Func<string, IDmtpActor> m_findDmtpActor;
        private DmtpActor m_smtpActor;
        private TcpDmtpAdapter m_smtpAdapter;

        #endregion 字段

        /// <inheritdoc/>
        public bool CanSend => this.m_client.State == WebSocketState.Open;

        /// <summary>
        /// 客户端配置
        /// </summary>
        public TouchSocketConfig Config { get; private set; }

        /// <inheritdoc/>
        public IContainer Container { get; private set; }

        /// <summary>
        /// 断开连接
        /// </summary>
        public DisconnectEventHandler<WebsocketDmtpClient> Disconnected { get; set; }

        /// <inheritdoc/>
        public string Id => this.DmtpActor.Id;

        /// <inheritdoc/>
        public bool IsHandshaked { get; private set; }

        /// <inheritdoc/>
        public IPluginsManager PluginsManager { get; private set; }

        /// <inheritdoc/>
        public IPHost RemoteIPHost { get; private set; }

        /// <inheritdoc/>
        public IDmtpActor DmtpActor { get => this.m_smtpActor; }
        public Func<ByteBlock, bool> OnHandleRawBuffer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Func<ByteBlock, IRequestInfo, bool> OnHandleReceivedData { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Protocol Protocol { get; set; } = DmtpUtility.DmtpProtocol;

        public DateTime LastReceivedTime { get;private set; }

        public DateTime LastSendTime { get; private set; }

        /// <summary>
        /// 发送<see cref="IDmtpActor"/>关闭消息。
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public void Close(string msg = "")
        {
            this.DmtpActor.Close(true, msg);
            this.PrivateClose(msg);
        }

        /// <inheritdoc/>
        public async Task ConnectAsync(int timeout = 5000)
        {
            if (this.IsHandshaked)
            {
                return;
            }
            m_cancellationTokenSource = new CancellationTokenSource();
            if (this.m_client == null || this.m_client.State != WebSocketState.Open)
            {
                this.m_client.SafeDispose();
                this.m_client = new ClientWebSocket();
                await this.m_client.ConnectAsync(this.RemoteIPHost, default);

                this.m_smtpActor = new DmtpActor(false)
                {
                    OutputSend = OnDmtpActorSend,
                    OnRouting = OnDmtpActorRouting,
                    OnHandshaking = this.OnDmtpActorHandshaking,
                    OnHandshaked = OnDmtpActorHandshaked,
                    OnClose = OnDmtpActorClose,
                    Logger = this.Logger,
                    Client = this,
                    OnFindDmtpActor = this.m_findDmtpActor,
                    OnCreateChannel = this.OnDmtpActorCreateChannel
                };

                this.m_smtpAdapter = new TcpDmtpAdapter()
                {
                    ReceivedCallBack = PrivateHandleReceivedData
                };
                _=this.BeginReceive();
            }

            this.m_smtpActor.Handshake(this.Config.GetValue(DmtpConfigExtension.VerifyTokenProperty),
                this.Config.GetValue(DmtpConfigExtension.DefaultIdProperty),
                timeout, this.Config.GetValue(DmtpConfigExtension.MetadataProperty), CancellationToken.None);
            this.IsHandshaked = true;
        }

        /// <inheritdoc/>
        public bool Ping(string targetId, int timeout = 5000)
        {
            return this.DmtpActor.Ping(targetId, timeout);
        }

        /// <inheritdoc/>
        public bool Ping(int timeout = 5000)
        {
            return this.DmtpActor.Ping(timeout);
        }

        /// <inheritdoc/>
        public void ResetId(string newId)
        {
            this.DmtpActor.ResetId(newId);
        }

        /// <summary>
        /// 配置
        /// </summary>
        /// <param name="ipHost"></param>
        /// <returns></returns>
        public IWebsocketDmtpClient Setup(string ipHost)
        {
            var config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost(ipHost));
            return this.Setup(config);
        }

        /// <summary>
        /// 配置
        /// </summary>
        /// <param name="config"></param>
        /// <exception cref="Exception"></exception>
        public IWebsocketDmtpClient Setup(TouchSocketConfig config)
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

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.DisposedValue)
            {
                return;
            }
            if (disposing)
            {
                this.BreakOut($"调用{nameof(Dispose)}", true);
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="config"></param>
        protected virtual void LoadConfig(TouchSocketConfig config)
        {
            this.RemoteIPHost = config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
            this.Logger ??= this.Container.Resolve<ILog>();

            this.SetBufferLength(this.Config.GetValue(TouchSocketConfigExtension.BufferLengthProperty) ?? 1024 * 64);

            if (this.Container.IsRegistered(typeof(IDmtpRouteService)))
            {
                this.m_allowRoute = true;
                this.m_findDmtpActor = this.Container.Resolve<IDmtpRouteService>().FindDmtpActor;
            }
        }

        /// <summary>
        /// 已断开连接。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(DisconnectEventArgs e)
        {
            if (this.PluginsManager.Raise(nameof(ITcpDisconnectedPlguin.OnTcpDisconnected), this, e))
            {
                return;
            }
            this.Disconnected?.Invoke(this, e);
        }

        private async Task BeginReceive()
        {
            try
            {
                while (true)
                {
                    using (var byteBlock = new ByteBlock(this.BufferLength))
                    {
                        var result = await this.m_client.ReceiveAsync(new ArraySegment<byte>(byteBlock.Buffer, 0, byteBlock.Capacity), default);
                        if (result.Count == 0)
                        {
                            break;
                        }
                        byteBlock.SetLength(result.Count);
                        this.LastReceivedTime = DateTime.Now;

                        this.m_smtpAdapter.ReceivedInput(byteBlock);
                    }
                }

                this.BreakOut("远程终端主动关闭", false);
            }
            catch (Exception ex)
            {
                this.BreakOut(ex.Message, false);
            }
        }

        private void BreakOut(string msg, bool manual)
        {
            lock (this.SyncRoot)
            {
                if (this.IsHandshaked)
                {
                    this.IsHandshaked = false;
                    m_cancellationTokenSource?.Cancel();
                    this.m_client.CloseAsync(WebSocketCloseStatus.NormalClosure, msg, CancellationToken.None);
                    this.m_client.SafeDispose();
                    this.DmtpActor.SafeDispose();
                    this.m_smtpAdapter.SafeDispose();
                    this.OnDisconnected(new DisconnectEventArgs(manual, msg));
                }
            }
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

        private void PrivateClose(string msg)
        {
            this.BreakOut($"调用{nameof(Close)}", true);
        }

        private void PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            var message = (DmtpMessage)requestInfo;
            if (!this.m_smtpActor.InputReceivedData(message))
            {
                if (this.PluginsManager.Enable)
                {
                    this.PluginsManager.Raise(nameof(IDmtpReceivedPlugin.OnDmtpReceived), this, new DmtpMessageEventArgs(message));
                }
            }
        }

        #region 内部委托绑定

        private void OnDmtpActorClose(DmtpActor actor, string arg2)
        {
            this.PrivateClose(arg2);
        }

        private void OnDmtpActorCreateChannel(DmtpActor actor, CreateChannelEventArgs e)
        {
            this.OnCreateChannel(e);
            if (e.Handled)
            {
                return;
            }

            this.PluginsManager.Raise(nameof(IDmtpCreateChannelPlugin.OnCreateChannel), this, e);
        }

        private void OnDmtpActorHandshaked(DmtpActor actor, DmtpVerifyEventArgs e)
        {
            this.OnHandshaked(e);
            if (e.Handled)
            {
                return;
            }
            if (this.PluginsManager.Raise(nameof(IDmtpHandshakedPlugin.OnDmtpHandshaked), this, e))
            {
                return;
            }
        }

        private void OnDmtpActorHandshaking(DmtpActor actor, DmtpVerifyEventArgs e)
        {
            this.OnHandshaking(e);
            if (e.Handled)
            {
                return;
            }
            this.PluginsManager.Raise(nameof(IDmtpHandshakingPlugin.OnDmtpHandshaking), this, e);
        }

        private void OnDmtpActorRouting(DmtpActor actor, PackageRouterEventArgs e)
        {
            if (this.PluginsManager.Raise(nameof(IDmtpRoutingPlugin.OnDmtpRouting), this, e))
            {
                return;
            }
            this.OnRouting(e);
        }

        private void OnDmtpActorSend(DmtpActor actor, ArraySegment<byte>[] transferBytes)
        {
            for (var i = 0; i < transferBytes.Length; i++)
            {
                Task task;
                if (i == transferBytes.Length - 1)
                {
                    task = this.m_client.SendAsync(transferBytes[i], WebSocketMessageType.Binary, true, CancellationToken.None);
                }
                else
                {
                    task = this.m_client.SendAsync(transferBytes[i], WebSocketMessageType.Binary, false, CancellationToken.None);
                }
                task.ConfigureAwait(false);
                task.GetAwaiter().GetResult();
            }
            this.LastSendTime = DateTime.Now;
        }

        #endregion 内部委托绑定

        #region 事件触发

        /// <summary>
        /// 当创建通道
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCreateChannel(CreateChannelEventArgs e)
        {
        }

        /// <summary>
        /// 在完成握手连接时
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnHandshaked(DmtpVerifyEventArgs e)
        {
        }

        /// <summary>
        /// 即将握手连接时
        /// </summary>
        /// <param name="e">参数</param>
        protected virtual void OnHandshaking(DmtpVerifyEventArgs e)
        {
        }

        /// <summary>
        /// 当需要转发路由包时
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnRouting(PackageRouterEventArgs e)
        {
        }

        #endregion 事件触发
    }
}