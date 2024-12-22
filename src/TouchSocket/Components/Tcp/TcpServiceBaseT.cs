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
using System.Net.Sockets;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 提供基于TCP的服务基类，用于管理和操作TCP客户端会话。
    /// </summary>
    /// <typeparam name="TClient">TCP客户端会话的类型，必须继承自TcpSessionClientBase，并实现IIdClient和IClient接口。</typeparam>
    public abstract class TcpServiceBase<TClient> : ConnectableService<TClient>, ITcpServiceBase<TClient> where TClient : TcpSessionClientBase, IIdClient, IClient
    {
        #region 变量

        private readonly InternalClientCollection<TClient> m_clients = new InternalClientCollection<TClient>();
        private readonly ConcurrentList<TcpNetworkMonitor> m_monitors = new ConcurrentList<TcpNetworkMonitor>();
        private readonly TcpCorePool m_tcpCorePool = new TcpCorePool();
        private ServerState m_serverState;

        #endregion 变量

        #region 属性

        /// <inheritdoc/>
        public override IClientCollection<TClient> Clients => this.m_clients;

        /// <inheritdoc/>
        public override int Count => this.m_clients.Count;

        /// <inheritdoc/>
        public IEnumerable<TcpNetworkMonitor> Monitors => this.m_monitors;

        /// <inheritdoc/>
        public override ServerState ServerState => this.m_serverState;

        #endregion 属性

        /// <inheritdoc/>
        public void AddListen(TcpListenOption option)
        {
            ThrowHelper.ThrowArgumentNullExceptionIf(option, nameof(option));
            this.ThrowIfDisposed();

            ThrowHelper.ThrowArgumentNullExceptionIf(option.IpHost, nameof(option.IpHost));

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
        public override async Task ClearAsync()
        {
            foreach (var id in this.GetIds())
            {
                if (this.TryGetClient(id, out var client))
                {
                    await client.CloseAsync().ConfigureAwait(false);
                    client.SafeDispose();
                }
            }
        }

        /// <inheritdoc/>
        public override bool ClientExists(string id)
        {
            return this.m_clients.ClientExist(id);
        }

        /// <inheritdoc/>
        public override IEnumerable<string> GetIds()
        {
            return this.m_clients.GetIds();
        }

        /// <inheritdoc/>
        public bool RemoveListen(TcpNetworkMonitor monitor)
        {
            ThrowHelper.ThrowArgumentNullExceptionIf(monitor, nameof(monitor));

            if (this.m_monitors.Remove(monitor))
            {
                monitor.SocketAsyncEvent.SafeDispose();
                monitor.Socket.SafeDispose();
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public override async Task ResetIdAsync(string sourceId, string targetId)
        {
            ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(sourceId, nameof(sourceId));
            ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(targetId, nameof(targetId));

            if (sourceId == targetId)
            {
                return;
            }

            if (this.m_clients.TryGetClient(sourceId, out var client))
            {
                await client.ResetIdAsync(targetId).ConfigureAwait(false);
            }
            else
            {
                ThrowHelper.ThrowClientNotFindException(sourceId);
            }
        }

        /// <inheritdoc/>
        public override async Task StartAsync()
        {
            this.ThrowIfDisposed();
            this.ThrowIfConfigIsNull();
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
                    case ServerState.Stopped:
                        {
                            this.AddListenList(optionList);
                            break;
                        }
                    case ServerState.Running:
                        {
                            return;
                        }
                    default:
                        {
                            ThrowHelper.ThrowInvalidEnumArgumentException(this.m_serverState);
                            return;
                        }
                }
                this.m_serverState = ServerState.Running;

                await this.PluginManager.RaiseAsync(typeof(IServerStartedPlugin), this.Resolver, this, new ServiceStateEventArgs(this.m_serverState, default)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.m_serverState = ServerState.Exception;

                await this.PluginManager.RaiseAsync(typeof(IServerStartedPlugin), this.Resolver, this, new ServiceStateEventArgs(this.m_serverState, ex) { Message = ex.Message }).ConfigureAwait(false);
                throw;
            }
        }

        /// <inheritdoc/>
        public override async Task StopAsync()
        {
            this.ThrowIfDisposed();

            var serverState = this.m_serverState;

            //调整这行语句顺序，修复Stop时可能会抛出异常的bug
            //https://gitee.com/RRQM_Home/TouchSocket/issues/IAWD4N
            this.m_serverState = ServerState.Stopped;//当无异常执行释放时重置状态到Stopped。意味可恢复启动
            //无条件释放
            await this.ReleaseAll().ConfigureAwait(false);

            if (serverState == ServerState.Running)
            {
                //当且仅当服务器的状态是Running时才触发ServerStoped
                await this.PluginManager.RaiseAsync(typeof(IServerStopedPlugin), this.Resolver, this, new ServiceStateEventArgs(this.m_serverState, default)).ConfigureAwait(false);
            }
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
                this.Stop();
                this.m_tcpCorePool.SafeDispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 在身份验证过程中发生错误时触发。
        /// 此方法用于记录身份验证过程中的异常错误。
        /// </summary>
        /// <param name="ex">发生的异常对象。</param>
        protected virtual void OnAuthenticatingError(Exception ex)
        {
            // 尝试记录异常信息，如果Logger为空则不会记录
            this.Logger?.Exception(ex);
        }

        /// <summary>
        /// 预览绑定 TCP 网络监视器。
        /// 此方法允许派生类在绑定监视器之前执行自定义的预览绑定逻辑。
        /// </summary>
        /// <param name="monitor">要绑定的 TCP 网络监视器实例。</param>
        protected virtual void PreviewBind(TcpNetworkMonitor monitor)
        {
        }

        #region TcpCore

        private TcpCore RentTcpCore()
        {
            return this.m_tcpCorePool.Rent();
        }

        private void ReturnTcpCore(TcpCore tcpCore)
        {
            if (tcpCore.DisposedValue)
            {
                return;
            }

            if (this.DisposedValue)
            {
                tcpCore.SafeDispose();
                return;
            }
            this.m_tcpCorePool.Return(tcpCore);
        }

        #endregion TcpCore

        private void Args_Completed(object sender, SocketAsyncEventArgs e)
        {
            this.OnAccepted(e);
        }

        private void AddListenList(List<TcpListenOption> optionList)
        {
            foreach (var item in optionList)
            {
                this.AddListen(item);
            }
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
                if (this.Count < this.MaxCount)
                {
                    //this.OnClientSocketInit(Tuple.Create(socket, (TcpNetworkMonitor)e.UserToken)).GetFalseAwaitResult();
                    _ = Task.Factory.StartNew(this.OnClientInit, Tuple.Create(socket, (TcpNetworkMonitor)e.UserToken));
                }
                else
                {
                    socket.SafeDispose();
                    this.Logger?.Warning(this, TouchSocketResource.ConnectedMaximum.Format(this.MaxCount));
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
                    return;
                }
            }
        }

        private async Task OnClientInit(object obj)
        {
            if (obj == null)
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
                    socket.NoDelay = monitor.Option.NoDelay.Value;
                }

                socket.SendTimeout = monitor.Option.SendTimeout;

                var tcpCore = this.RentTcpCore();

                tcpCore.Reset(socket);

                var client = this.NewClient();

                client.InternalSetService(this);
                client.InternalSetResolver(this.Resolver);
                client.InternalSetListenOption(monitor.Option);
                client.InternalSetTcpCore(tcpCore);
                client.InternalSetPluginManager(this.PluginManager);
                client.InternalSetReturnTcpCore(this.ReturnTcpCore);
                client.InternalSetAction(this.TryAdd, this.TryRemove, this.TryGet);

                this.ClientInitialized(client);

                await client.InternalInitialized().ConfigureAwait(false);

                var args = new ConnectingEventArgs()
                {
                    Id = this.GetNextNewId()
                };
                await client.InternalConnecting(args).ConfigureAwait(false);//Connecting
                if (args.IsPermitOperation)
                {
                    client.InternalSetId(args.Id);
                    if (!socket.Connected)
                    {
                        socket.SafeDispose();
                        return;
                    }

                    if (monitor.Option.UseSsl)
                    {
                        try
                        {
                            await tcpCore.AuthenticateAsync(monitor.Option.ServiceSslOption).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            this.OnAuthenticatingError(ex);
                            throw;
                        }
                    }

                    if (this.m_clients.TryAdd(client))
                    {
                        await client.InternalConnected(new ConnectedEventArgs()).ConfigureAwait(false);
                    }
                    else
                    {
                        ThrowHelper.ThrowException(TouchSocketResource.IdAlreadyExists.Format(args.Id));
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
                this.Logger?.Exception(ex);
            }
        }

        private async Task ReleaseAll()
        {
            foreach (var item in this.m_monitors)
            {
                item.Socket.SafeDispose();
                item.SocketAsyncEvent.SafeDispose();
            }

            this.m_monitors.Clear();

            await this.ClearAsync().ConfigureAwait(false);
        }

        private bool TryAdd(TcpSessionClientBase client)
        {
            return this.m_clients.TryAdd((TClient)client);
        }

        private bool TryGet(string id, out TcpSessionClientBase client)
        {
            if (this.m_clients.TryGetClient(id, out var newClient))
            {
                client = newClient;
                return true;
            }
            client = default;
            return false;
        }

        private bool TryRemove(string id, out TcpSessionClientBase client)
        {
            if (this.m_clients.TryRemoveClient(id, out var newClient))
            {
                client = newClient;
                return true;
            }
            client = default;
            return false;
        }
    }
}