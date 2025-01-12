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
using System.IO.Pipes;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 命名管道服务基类
    /// </summary>
    /// <typeparam name="TClient">客户端类型，必须继承自NamedPipeSessionClientBase</typeparam>
    public abstract class NamedPipeServiceBase<TClient> : ConnectableService<TClient>, INamedPipeServiceBase<TClient> where TClient : NamedPipeSessionClientBase
    {
        #region 字段

        private readonly InternalClientCollection<TClient> m_clients = new InternalClientCollection<TClient>();
        private readonly List<NamedPipeMonitor> m_monitors = new List<NamedPipeMonitor>();
        private ServerState m_serverState;

        #endregion 字段

        #region 属性

        /// <inheritdoc/>
        public override int Count => this.m_clients.Count;

        /// <inheritdoc/>
        public IEnumerable<NamedPipeMonitor> Monitors => this.m_monitors;

        /// <inheritdoc/>
        public override ServerState ServerState => this.m_serverState;

        /// <inheritdoc/>
        public override IClientCollection<TClient> Clients => this.m_clients;

        #endregion 属性


        /// <inheritdoc/>
        public void AddListen(NamedPipeListenOption option)
        {
            if (option is null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            this.ThrowIfDisposed();

            var networkMonitor = new NamedPipeMonitor(option);

            _ = Task.Factory.StartNew(this.ThreadBegin, networkMonitor, TaskCreationOptions.LongRunning);
            this.m_monitors.Add(networkMonitor);
        }

        /// <inheritdoc/>
        public override async Task ClearAsync()
        {
            foreach (var id in this.GetIds())
            {
                if (this.TryGetClient(id, out var client))
                {
                    await client.CloseAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    client.SafeDispose();
                }
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<string> GetIds()
        {
            return this.m_clients.GetIds();
        }

        /// <inheritdoc/>
        public bool RemoveListen(NamedPipeMonitor monitor)
        {
            if (monitor is null)
            {
                throw new ArgumentNullException(nameof(monitor));
            }

            if (this.m_monitors.Remove(monitor))
            {
                throw new Exception();
            }
            return false;
        }

        /// <inheritdoc/>
        public override async Task ResetIdAsync(string sourceId, string targetId)
        {
            if (string.IsNullOrEmpty(sourceId))
            {
                throw new ArgumentException($"“{nameof(sourceId)}”不能为 null 或空。", nameof(sourceId));
            }

            if (string.IsNullOrEmpty(targetId))
            {
                throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
            }

            if (sourceId == targetId)
            {
                return;
            }
            if (this.m_clients.TryGetClient(sourceId, out var sessionClient))
            {
                await sessionClient.ResetIdAsync(targetId).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketResource.ClientNotFind.Format(sourceId));
            }
        }

        /// <inheritdoc/>
        public override bool ClientExists(string id)
        {
            return this.m_clients.ClientExist(id);
        }

        /// <inheritdoc/>
        public override async Task StartAsync()
        {
            this.ThrowIfConfigIsNull();

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
                    default:
                        {
                            ThrowHelper.ThrowInvalidEnumArgumentException(this.m_serverState);
                            return;
                        }
                }
                this.m_serverState = ServerState.Running;

                await this.PluginManager.RaiseAsync(typeof(IServerStartedPlugin), this.Resolver, this, new ServiceStateEventArgs(this.m_serverState, default)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            catch (Exception ex)
            {
                this.m_serverState = ServerState.Exception;

                await this.PluginManager.RaiseAsync(typeof(IServerStartedPlugin), this.Resolver, this, new ServiceStateEventArgs(this.m_serverState, ex) { Message = ex.Message }).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                throw;
            }
        }

        /// <inheritdoc/>
        public override async Task StopAsync()
        {
            await EasyTask.CompletedTask.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            throw new NotImplementedException();
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
            var client = this.NewClient();

            client.InternalSetConfig(this.Config);
            client.InternalSetContainer(this.Resolver);
            client.InternalSetListenOption(monitor.Option);
            client.InternalSetService(this);
            client.InternalSetNamedPipe(namedPipe);
            client.InternalSetPluginManager(this.PluginManager);
            client.InternalSetAction(this.TryAdd, this.TryRemove, this.TryGet);

            this.ClientInitialized(client);

            await client.InternalInitialized().ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            var args = new ConnectingEventArgs()
            {
                Id = this.GetNextNewId()
            };
            await client.InternalNamedPipeConnecting(args).ConfigureAwait(EasyTask.ContinueOnCapturedContext);//Connecting
            if (args.IsPermitOperation)
            {
                client.InternalSetId(args.Id);
                if (this.m_clients.TryAdd(client))
                {
                    await client.InternalNamedPipeConnected(new ConnectedEventArgs()).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
                else
                {
                    ThrowHelper.ThrowException(TouchSocketResource.IdAlreadyExists.Format(args.Id));
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

                    await this.OnClientSocketInit(namedPipe, monitor).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
                catch (Exception ex)
                {
                    this.Logger?.Exception(ex);
                }
            }
        }

        private bool TryAdd(NamedPipeSessionClientBase client)
        {
            return this.m_clients.TryAdd((TClient)client);
        }

        private bool TryGet(string id, out NamedPipeSessionClientBase client)
        {
            if (this.m_clients.TryGetClient(id, out var newClient))
            {
                client = newClient;
                return true;
            }
            client = default;
            return false;
        }

        private bool TryRemove(string id, out NamedPipeSessionClientBase client)
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