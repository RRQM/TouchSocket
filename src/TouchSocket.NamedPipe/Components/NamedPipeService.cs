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

using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 命名管道服务器
    /// </summary>
    public class NamedPipeService : NamedPipeService<NamedPipeSocketClient>, INamedPipeService
    {
        /// <summary>
        /// 处理数据
        /// </summary>
        public ReceivedEventHandler<NamedPipeSocketClient> Received { get; set; }

        /// <inheritdoc/>
        protected override Task OnReceived(NamedPipeSocketClient socketClient, ReceivedDataEventArgs e)
        {
            if (this.Received != null)
            {
                return this.Received.Invoke(socketClient, e);
            }
            return EasyTask.CompletedTask;
        }
    }

    /// <summary>
    /// 泛型命名管道服务器。
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public class NamedPipeService<TClient> : NamedPipeServiceBase, INamedPipeService<TClient> where TClient : NamedPipeSocketClient, new()
    {
        /// <summary>
        /// 泛型命名管道服务器
        /// </summary>
        public NamedPipeService()
        {
            this.m_getDefaultNewId = () =>
            {
                return Interlocked.Increment(ref this.m_nextId).ToString();
            };
        }

        #region 字段

        private readonly List<NamedPipeMonitor> m_monitors = new List<NamedPipeMonitor>();
        private Func<string> m_getDefaultNewId;
        private int m_maxCount;
        private long m_nextId;
        private ServerState m_serverState;
        private NamedPipeSocketClientCollection m_socketClients = new NamedPipeSocketClientCollection();

        #endregion 字段

        #region 属性

        /// <inheritdoc/>
        public override int MaxCount { get => this.m_maxCount; }

        /// <inheritdoc/>
        public override IEnumerable<NamedPipeMonitor> Monitors => this.m_monitors;

        /// <inheritdoc/>
        public override string ServerName => this.Config?.GetValue(TouchSocketConfigExtension.ServerNameProperty);

        /// <inheritdoc/>
        public override ServerState ServerState => this.m_serverState;

        /// <inheritdoc/>
        public override INamedPipeSocketClientCollection SocketClients { get => this.m_socketClients; }

        #endregion 属性

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="option"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public override void AddListen(NamedPipeListenOption option)
        {
            if (option is null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            this.ThrowIfDisposed();

            var e = new SocketAsyncEventArgs();

            var networkMonitor = new NamedPipeMonitor(option);

            Task.Factory.StartNew(this.ThreadBegin, networkMonitor, TaskCreationOptions.LongRunning);
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
        public override bool RemoveListen(NamedPipeMonitor monitor)
        {
            if (monitor is null)
            {
                throw new ArgumentNullException(nameof(monitor));
            }

            if (this.m_monitors.Remove(monitor))
            {
                //monitor.SocketAsyncEvent.SafeDispose();
                //monitor.Socket.SafeDispose();
                throw new Exception();
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

            var optionList = new List<NamedPipeListenOption>();
            if (this.Config.GetValue(NamedPipeConfigExtension.NamedPipeListenOptionProperty) is Action<List<NamedPipeListenOption>> action)
            {
                action.Invoke(optionList);
            }

            var pipeName = this.Config.GetValue(NamedPipeConfigExtension.PipeNameProperty);
            if (pipeName != null)
            {
                var option = new NamedPipeListenOption
                {
                    Name = pipeName,
                    Adapter = this.Config.GetValue(NamedPipeConfigExtension.NamedPipeDataHandlingAdapterProperty),
                    SendTimeout = this.Config.GetValue(TouchSocketConfigExtension.SendTimeoutProperty)
                };

                optionList.Add(option);
            }

            if (optionList.Count == 0)
            {
                return;
            }
            try
            {
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

            var optionList = new List<NamedPipeListenOption>();
            if (this.Config.GetValue(NamedPipeConfigExtension.NamedPipeListenOptionProperty) is Action<List<NamedPipeListenOption>> action)
            {
                action.Invoke(optionList);
            }

            var pipeName = this.Config.GetValue(NamedPipeConfigExtension.PipeNameProperty);
            if (pipeName != null)
            {
                var option = new NamedPipeListenOption
                {
                    Name = pipeName,
                    Adapter = this.Config.GetValue(NamedPipeConfigExtension.NamedPipeDataHandlingAdapterProperty),
                    SendTimeout = this.Config.GetValue(TouchSocketConfigExtension.SendTimeoutProperty)
                };

                optionList.Add(option);
            }

            if (optionList.Count == 0)
            {
                return;
            }
            try
            {
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
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override async Task StopAsync()
        {
            await EasyTask.CompletedTask;
            throw new NotImplementedException();
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
        /// 获取客户端实例
        /// </summary>
        /// <param name="namedPipe"></param>
        /// <param name="monitor"></param>
        /// <returns></returns>
        protected virtual TClient GetClientInstence(NamedPipeServerStream namedPipe, NamedPipeMonitor monitor)
        {
            return new TClient();
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

        private void BeginListen(List<NamedPipeListenOption> optionList)
        {
            foreach (var item in optionList)
            {
                this.AddListen(item);
            }
        }

        private async Task OnClientSocketInit(NamedPipeServerStream namedPipe, NamedPipeMonitor monitor)
        {
            var client = this.GetClientInstence(namedPipe, monitor);
            client.InternalSetConfig(this.Config);
            client.InternalSetContainer(this.Resolver);
            client.InternalSetService(this);
            client.InternalSetNamedPipe(namedPipe);
            client.InternalSetPluginManager(this.PluginManager);

            if (client.CanSetDataHandlingAdapter)
            {
                client.SetDataHandlingAdapter(monitor.Option.Adapter.Invoke());
            }
            await client.InternalInitialized();

            var args = new ConnectingEventArgs(null)
            {
                Id = this.m_getDefaultNewId.Invoke()
            };
            await client.InternalConnecting(args);//Connecting
            if (args.IsPermitOperation)
            {
                client.InternalSetId(args.Id);
                if (this.m_socketClients.TryAdd(client))
                {
                    _ = client.InternalConnected(new ConnectedEventArgs());

                    _ = client.BeginReceive();
                }
                else
                {
                    throw new Exception($"Id={client.Id}重复");
                }
            }
            else
            {
                namedPipe.SafeDispose();
            }
        }

        private async Task ThreadBegin(object obj)
        {
            var monitor = (NamedPipeMonitor)obj;
            var option = monitor.Option;
            while (true)
            {
                try
                {
                    if (this.ServerState != ServerState.Running)
                    {
                        return;
                    }
                    var namedPipe = new NamedPipeServerStream(option.Name, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, 0);

                    namedPipe.WaitForConnection();

                    await this.OnClientSocketInit(namedPipe, monitor);
                }
                catch (Exception ex)
                {
                    this.Logger.Exception(ex);
                }
            }
        }

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
        protected override sealed Task OnClientConnected(INamedPipeSocketClient socketClient, ConnectedEventArgs e)
        {
            return this.OnConnected((TClient)socketClient, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override sealed Task OnClientConnecting(INamedPipeSocketClient socketClient, ConnectingEventArgs e)
        {
            return this.OnConnecting((TClient)socketClient, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override sealed Task OnClientDisconnected(INamedPipeSocketClient socketClient, DisconnectEventArgs e)
        {
            return this.OnDisconnected((TClient)socketClient, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override sealed Task OnClientDisconnecting(INamedPipeSocketClient socketClient, DisconnectEventArgs e)
        {
            return this.OnDisconnecting((TClient)socketClient, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override sealed Task OnClientReceivedData(INamedPipeSocketClient socketClient, ReceivedDataEventArgs e)
        {
            return this.OnReceived((TClient)socketClient, e);
        }

        /// <summary>
        /// 客户端连接完成，覆盖父类方法将不会触发事件。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual Task OnConnected(TClient socketClient, ConnectedEventArgs e)
        {
            if (this.Connected != null)
            {
                return this.Connected.Invoke(socketClient, e);
            }
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 客户端请求连接，覆盖父类方法将不会触发事件。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual Task OnConnecting(TClient socketClient, ConnectingEventArgs e)
        {
            if (this.Connecting != null)
            {
                return this.Connecting.Invoke(socketClient, e);
            }
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 客户端断开连接，覆盖父类方法将不会触发事件。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual Task OnDisconnected(TClient socketClient, DisconnectEventArgs e)
        {
            if (this.Disconnected != null)
            {
                return this.Disconnected.Invoke(socketClient, e);
            }
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual Task OnDisconnecting(TClient socketClient, DisconnectEventArgs e)
        {
            if (this.Disconnecting != null)
            {
                return this.Disconnecting.Invoke(socketClient, e);
            }
            return EasyTask.CompletedTask;
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
    }
}