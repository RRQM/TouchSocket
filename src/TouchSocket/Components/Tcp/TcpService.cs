//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// Tcp泛型服务器，由使用者自己指定<see cref="SocketClient"/>类型。
    /// </summary>
    public class TcpService<TClient> : TcpServiceBase, ITcpService<TClient> where TClient : SocketClient, new()
    {
        /// <summary>
        /// Tcp泛型服务器，由使用者自己指定<see cref="SocketClient"/>类型。
        /// </summary>
        public TcpService()
        {
            this.m_getDefaultNewId = this.GetDefaultNewId;
        }

        #region 变量

        private readonly List<TcpNetworkMonitor> m_monitors = new List<TcpNetworkMonitor>();
        private readonly SocketClientCollection m_socketClients = new SocketClientCollection();
        private Func<string> m_getDefaultNewId;
        private int m_maxCount;
        private long m_nextId;
        private ServerState m_serverState;

        #endregion 变量

        #region 属性

        /// <inheritdoc/>
        public override int MaxCount => this.m_maxCount;

        /// <inheritdoc/>
        public override IEnumerable<TcpNetworkMonitor> Monitors => this.m_monitors.ToArray();

        /// <inheritdoc/>
        public override string ServerName => this.Config?.GetValue(TouchSocketConfigExtension.ServerNameProperty);

        /// <inheritdoc/>
        public override ServerState ServerState => this.m_serverState;

        /// <inheritdoc/>
        public override ISocketClientCollection SocketClients => this.m_socketClients;

        #endregion 属性

        #region 事件

        /// <summary>
        /// 用户连接完成
        /// </summary>
        public ConnectedEventHandler<TClient> Connected { get; set; }

        /// <summary>
        /// 有用户连接的时候
        /// </summary>
        public ConnectingEventHandler<TClient> Connecting { get; set; }

        /// <summary>
        /// 有用户断开连接
        /// </summary>
        public DisconnectEventHandler<TClient> Disconnected { get; set; }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        public DisconnectEventHandler<TClient> Disconnecting { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override sealed Task OnClientConnected(ISocketClient socketClient, ConnectedEventArgs e)
        {
            return this.OnConnected((TClient)socketClient, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override sealed Task OnClientConnecting(ISocketClient socketClient, ConnectingEventArgs e)
        {
            return this.OnConnecting((TClient)socketClient, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override sealed Task OnClientDisconnected(ISocketClient socketClient, DisconnectEventArgs e)
        {
            return this.OnDisconnected((TClient)socketClient, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override sealed Task OnClientDisconnecting(ISocketClient socketClient, DisconnectEventArgs e)
        {
            return this.OnDisconnecting((TClient)socketClient, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override sealed Task OnClientReceivedData(ISocketClient socketClient, ReceivedDataEventArgs e)
        {
            return this.OnReceived((TClient)socketClient, e);
        }

        /// <summary>
        /// 客户端连接完成，覆盖父类方法将不会触发事件。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual async Task OnConnected(TClient socketClient, ConnectedEventArgs e)
        {
            if (this.Connected != null)
            {
                await this.Connected.Invoke(socketClient, e).ConfigureFalseAwait();
            }
        }

        /// <summary>
        /// 客户端请求连接，覆盖父类方法将不会触发事件。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual async Task OnConnecting(TClient socketClient, ConnectingEventArgs e)
        {
            if (this.Connecting != null)
            {
                await this.Connecting.Invoke(socketClient, e).ConfigureFalseAwait();
            }
        }

        /// <summary>
        /// 客户端断开连接，覆盖父类方法将不会触发事件。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual async Task OnDisconnected(TClient socketClient, DisconnectEventArgs e)
        {
            if (this.Disconnected != null)
            {
                await this.Disconnected.Invoke(socketClient, e).ConfigureFalseAwait();
            }
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual async Task OnDisconnecting(TClient socketClient, DisconnectEventArgs e)
        {
            if (this.Disconnecting != null)
            {
                await this.Disconnecting.Invoke(socketClient, e).ConfigureFalseAwait();
            }
        }

        /// <summary>
        /// 当收到适配器数据。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual Task OnReceived(TClient socketClient, ReceivedDataEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        #endregion 事件

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public override void AddListen(TcpListenOption option)
        {
            if (option is null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            this.ThrowIfDisposed();

            if (option.IpHost is null)
            {
                throw new ArgumentNullException(nameof(option.IpHost));
            }

            var socket = new Socket(option.IpHost.EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            if (option.ReuseAddress)
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }

            var e = new SocketAsyncEventArgs();

            var networkMonitor = new TcpNetworkMonitor(option, socket, e);

            this.PreviewBind(networkMonitor);
            socket.Bind(option.IpHost.EndPoint);
            socket.Listen(option.Backlog);

            e.UserToken = networkMonitor;
            e.Completed += this.Args_Completed;
            if (!networkMonitor.Socket.AcceptAsync(e))
            {
                this.OnAccepted(e);
            }
            this.m_monitors.Add(networkMonitor);
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            foreach (var item in this.GetIds())
            {
                if (this.TryGetSocketClient(item, out var client))
                {
                    client.SafeDispose();
                }
            }
        }

        /// <summary>
        /// 获取当前在线的所有客户端
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TClient> GetClients()
        {
            return this.m_socketClients.GetClients()
                  .Select(a => (TClient)a);
        }

        /// <inheritdoc/>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public override bool RemoveListen(TcpNetworkMonitor monitor)
        {
            if (monitor is null)
            {
                throw new ArgumentNullException(nameof(monitor));
            }

            if (this.m_monitors.Remove(monitor))
            {
                monitor.SocketAsyncEvent.SafeDispose();
                monitor.Socket.SafeDispose();
                return true;
            }
            return false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="oldId"></param>
        /// <param name="newId"></param>
        /// <exception cref="ClientNotFindException"></exception>
        /// <exception cref="Exception"></exception>
        public override void ResetId(string oldId, string newId)
        {
            if (string.IsNullOrEmpty(oldId))
            {
                throw new ArgumentException($"“{nameof(oldId)}”不能为 null 或空。", nameof(oldId));
            }

            if (string.IsNullOrEmpty(newId))
            {
                throw new ArgumentException($"“{nameof(newId)}”不能为 null 或空。", nameof(newId));
            }

            if (oldId == newId)
            {
                return;
            }
            if (this.m_socketClients.TryGetSocketClient(oldId, out TClient socketClient))
            {
                socketClient.ResetId(newId);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketResource.ClientNotFind.GetDescription(oldId));
            }
        }

        /// <summary>
        ///<inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override bool SocketClientExist(string id)
        {
            return this.SocketClients.SocketClientExist(id);
        }

        /// <inheritdoc/>
        public override void Start()
        {
            if (this.Config is null)
            {
                throw new ArgumentNullException(nameof(this.Config), "Config为null，请先执行Setup");
            }
            try
            {
                var optionList = new List<TcpListenOption>();
                if (this.Config.GetValue(TouchSocketConfigExtension.ListenOptionsProperty) is Action<List<TcpListenOption>> action)
                {
                    action.Invoke(optionList);
                }

                var iPHosts = this.Config.GetValue(TouchSocketConfigExtension.ListenIPHostsProperty);
                if (iPHosts != null)
                {
                    foreach (var item in iPHosts)
                    {
                        var option = new TcpListenOption
                        {
                            IpHost = item,
                            ServiceSslOption = this.Config.GetValue(TouchSocketConfigExtension.SslOptionProperty) as ServiceSslOption,
                            ReuseAddress = this.Config.GetValue(TouchSocketConfigExtension.ReuseAddressProperty),
                            NoDelay = this.Config.GetValue(TouchSocketConfigExtension.NoDelayProperty),
                            Adapter = this.Config.GetValue(TouchSocketConfigExtension.TcpDataHandlingAdapterProperty),
                        };
                        option.Backlog = this.Config.GetValue(TouchSocketConfigExtension.BacklogProperty) ?? option.Backlog;
                        option.SendTimeout = this.Config.GetValue(TouchSocketConfigExtension.SendTimeoutProperty);

                        optionList.Add(option);
                    }
                }

                switch (this.m_serverState)
                {
                    case ServerState.None:
                        {
                            this.BeginListen(optionList);
                            break;
                        }
                    case ServerState.Running:
                        {
                            return;
                        }
                    case ServerState.Stopped:
                        {
                            this.BeginListen(optionList);
                            break;
                        }
                    case ServerState.Disposed:
                        {
                            throw new ObjectDisposedException(this.GetType().Name);
                        }
                }
                this.m_serverState = ServerState.Running;

                this.PluginManager.Raise(nameof(IServerStartedPlugin.OnServerStarted), this, new ServiceStateEventArgs(this.m_serverState, default));
                return;
            }
            catch (Exception ex)
            {
                this.m_serverState = ServerState.Exception;

                this.PluginManager.Raise(nameof(IServerStartedPlugin.OnServerStarted), this, new ServiceStateEventArgs(this.m_serverState, ex) { Message = ex.Message });
                throw;
            }
        }

        /// <inheritdoc/>
        public override async Task StartAsync()
        {
            if (this.Config is null)
            {
                throw new ArgumentNullException(nameof(this.Config), "Config为null，请先执行Setup");
            }
            try
            {
                var optionList = new List<TcpListenOption>();
                if (this.Config.GetValue(TouchSocketConfigExtension.ListenOptionsProperty) is Action<List<TcpListenOption>> action)
                {
                    action.Invoke(optionList);
                }

                var iPHosts = this.Config.GetValue(TouchSocketConfigExtension.ListenIPHostsProperty);
                if (iPHosts != null)
                {
                    foreach (var item in iPHosts)
                    {
                        var option = new TcpListenOption
                        {
                            IpHost = item,
                            ServiceSslOption = this.Config.GetValue(TouchSocketConfigExtension.SslOptionProperty) as ServiceSslOption,
                            ReuseAddress = this.Config.GetValue(TouchSocketConfigExtension.ReuseAddressProperty),
                            NoDelay = this.Config.GetValue(TouchSocketConfigExtension.NoDelayProperty),
                            Adapter = this.Config.GetValue(TouchSocketConfigExtension.TcpDataHandlingAdapterProperty),
                        };
                        option.Backlog = this.Config.GetValue(TouchSocketConfigExtension.BacklogProperty) ?? option.Backlog;
                        option.SendTimeout = this.Config.GetValue(TouchSocketConfigExtension.SendTimeoutProperty);

                        optionList.Add(option);
                    }
                }

                switch (this.m_serverState)
                {
                    case ServerState.None:
                        {
                            this.BeginListen(optionList);
                            break;
                        }
                    case ServerState.Running:
                        {
                            return;
                        }
                    case ServerState.Stopped:
                        {
                            this.BeginListen(optionList);
                            break;
                        }
                    case ServerState.Disposed:
                        {
                            throw new ObjectDisposedException(this.GetType().Name);
                        }
                }
                this.m_serverState = ServerState.Running;

                await this.PluginManager.RaiseAsync(nameof(IServerStartedPlugin.OnServerStarted), this, new ServiceStateEventArgs(this.m_serverState, default)).ConfigureFalseAwait();
                return;
            }
            catch (Exception ex)
            {
                this.m_serverState = ServerState.Exception;

                await this.PluginManager.RaiseAsync(nameof(IServerStartedPlugin.OnServerStarted), this, new ServiceStateEventArgs(this.m_serverState, ex) { Message = ex.Message }).ConfigureFalseAwait();
                throw;
            }
        }

        /// <inheritdoc/>
        public override void Stop()
        {
            this.m_serverState = ServerState.Stopped;

            this.ReleaseAll();

            this.PluginManager.Raise(nameof(IServerStopedPlugin.OnServerStoped), this, new ServiceStateEventArgs(this.m_serverState, default));
            return;
        }

        private void ReleaseAll()
        {
            foreach (var item in this.m_monitors)
            {
                item.Socket.SafeDispose();
                item.SocketAsyncEvent.SafeDispose();
            }

            this.m_monitors.Clear();

            this.Clear();
        }

        /// <inheritdoc/>
        public override async Task StopAsync()
        {
            this.m_serverState = ServerState.Stopped;
            this.ReleaseAll();
            await this.PluginManager.RaiseAsync(nameof(IServerStopedPlugin.OnServerStoped), this, new ServiceStateEventArgs(this.m_serverState, default)).ConfigureFalseAwait();
        }

        /// <summary>
        /// 尝试获取TClient
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="socketClient">TClient</param>
        /// <returns></returns>
        public bool TryGetSocketClient(string id, out TClient socketClient)
        {
            return this.m_socketClients.TryGetSocketClient(id, out socketClient);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (this.DisposedValue)
            {
                return;
            }

            if (disposing)
            {
                this.m_serverState = ServerState.Disposed;
                this.ReleaseAll();
                this.PluginManager.Raise(nameof(IServerStopedPlugin.OnServerStoped), this, new ServiceStateEventArgs(this.m_serverState, default));
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 初始化客户端实例。
        /// </summary>
        /// <returns></returns>
        protected virtual TClient GetClientInstence(Socket socket, TcpNetworkMonitor monitor)
        {
            return new TClient();
        }

        /// <summary>
        /// 获取下一个新Id
        /// </summary>
        /// <returns></returns>
        protected string GetNextNewId()
        {
            try
            {
                return this.m_getDefaultNewId.Invoke();
            }
            catch (Exception ex)
            {
                this.Logger?.Exception(ex);
            }
            return this.GetDefaultNewId();
        }

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            if (config.GetValue(TouchSocketConfigExtension.GetDefaultNewIdProperty) is Func<string> fun)
            {
                this.m_getDefaultNewId = fun;
            }
            this.m_maxCount = config.GetValue(TouchSocketConfigExtension.MaxCountProperty);
        }

        /// <summary>
        /// 在验证Ssl发送错误时。
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void OnAuthenticatingError(Exception ex)
        {
        }

        /// <inheritdoc/>
        protected virtual void PreviewBind(TcpNetworkMonitor monitor)
        {
        }

        private void Args_Completed(object sender, SocketAsyncEventArgs e)
        {
            this.OnAccepted(e);
        }

        private void BeginListen(List<TcpListenOption> optionList)
        {
            foreach (var item in optionList)
            {
                this.AddListen(item);
            }
        }

        private SingleStreamDataHandlingAdapter GetAdapter(TcpNetworkMonitor monitor)
        {
            try
            {
                return monitor.Option.Adapter.Invoke();
            }
            catch (Exception ex)
            {
                this.Logger?.Exception(ex);
            }
            return new NormalDataHandlingAdapter();
        }

        private string GetDefaultNewId()
        {
            return Interlocked.Increment(ref this.m_nextId).ToString();
        }

        private void OnAccepted(SocketAsyncEventArgs e)
        {
            if (this.DisposedValue)
            {
                return;
            }

            if (e.LastOperation == SocketAsyncOperation.Accept && e.SocketError == SocketError.Success && e.AcceptSocket != null)
            {
                var socket = e.AcceptSocket;
                if (socket!=null)
                {
                    if (this.m_socketClients.Count < this.m_maxCount)
                    {
                        //this.OnClientSocketInit(Tuple.Create(socket, (TcpNetworkMonitor)e.UserToken)).GetFalseAwaitResult();
                        Task.Factory.StartNew(this.OnClientSocketInit, Tuple.Create(socket, (TcpNetworkMonitor)e.UserToken));
                    }
                    else
                    {
                        socket.SafeDispose();
                        this.Logger?.Warning(this, "连接客户端数量已达到设定最大值");
                    }
                }
            }

            if (this.m_serverState == ServerState.Running)
            {
                e.AcceptSocket = null;

                try
                {
                    if (!((TcpNetworkMonitor)e.UserToken).Socket.AcceptAsync(e))
                    {
                        this.OnAccepted(e);
                    }
                }
                catch (Exception ex)
                {
                    if (this.m_serverState == ServerState.Running)
                    {
                        this.Logger?.Exception(ex);
                    }
                    e.SafeDispose();
                }
            }
        }

        private async Task OnClientSocketInit(object obj)
        {
            if (obj==null)
            {
                return;
            }

            var tuple = (Tuple<Socket, TcpNetworkMonitor>)obj;
            var socket = tuple.Item1;
            var monitor = tuple.Item2;

            try
            {
                if (monitor.Option.NoDelay.HasValue)
                {
                    socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, monitor.Option.NoDelay.Value);
                }
                socket.SendTimeout = monitor.Option.SendTimeout;

                var client = this.GetClientInstence(socket, monitor);
                client.InternalSetService(this);
                client.InternalSetContainer(this.Resolver);
                client.InternalSetListenOption(monitor.Option);
                client.InternalSetSocket(socket);
                client.InternalSetPluginManager(this.PluginManager);

                if (client.CanSetDataHandlingAdapter)
                {
                    client.SetDataHandlingAdapter(this.GetAdapter(monitor));
                }

                await client.InternalInitialized().ConfigureFalseAwait();

                var args = new ConnectingEventArgs(socket)
                {
                    Id = this.GetNextNewId()
                };
                await client.InternalConnecting(args).ConfigureFalseAwait();//Connecting
                if (args.IsPermitOperation)
                {
                    client.InternalSetId(args.Id);
                    if (!socket.Connected)
                    {
                        socket.SafeDispose();
                        return;
                    }
                    if (this.m_socketClients.TryAdd(client))
                    {
                        _ = client.InternalConnected(new ConnectedEventArgs());
                        if (!socket.Connected)
                        {
                            return;
                        }
                        if (monitor.Option.UseSsl)
                        {
                            try
                            {
                                await client.AuthenticateAsync(monitor.Option.ServiceSslOption).ConfigureFalseAwait();
                                client.BeginReceiveSsl();
                            }
                            catch (Exception ex)
                            {
                                this.OnAuthenticatingError(ex);
                                throw;
                            }
                        }
                        else
                        {
                            client.BeginReceive();
                        }
                    }
                    else
                    {
                        throw new Exception($"Id={client.Id}重复");
                    }
                }
                else
                {
                    socket.SafeDispose();
                }
            }
            catch (Exception ex)
            {
                socket.SafeDispose();
                this.Logger?.Log(LogLevel.Error, this, "接收新连接错误", ex);
            }
        }
    }

    /// <summary>
    /// Tcp服务器
    /// </summary>
    public class TcpService : TcpService<SocketClient>, ITcpService
    {
        /// <summary>
        /// 处理数据
        /// </summary>
        public ReceivedEventHandler<SocketClient> Received { get; set; }

        /// <inheritdoc/>
        protected override async Task OnReceived(SocketClient socketClient, ReceivedDataEventArgs e)
        {
            if (this.Received != null)
            {
                await this.Received.Invoke(socketClient, e).ConfigureFalseAwait();
                if (e.Handled)
                {
                    return;
                }
            }

            await base.OnReceived(socketClient, e);
        }
    }
}