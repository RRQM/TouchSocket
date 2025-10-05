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

using System.IO.Pipes;
using TouchSocket.Resources;

namespace TouchSocket.NamedPipe;

/// <summary>
/// 命名管道服务基类
/// </summary>
/// <typeparam name="TClient">客户端类型，必须继承自NamedPipeSessionClientBase</typeparam>
public abstract class NamedPipeServiceBase<TClient> : ConnectableService<TClient>, INamedPipeServiceBase<TClient> where TClient : NamedPipeSessionClientBase
{
    #region 字段

    private readonly InternalClientCollection<TClient> m_clients = new InternalClientCollection<TClient>();
    private readonly List<NamedPipeMonitor> m_monitors = new List<NamedPipeMonitor>();
    private readonly CancellationTokenSource m_cancellationTokenSource;
    private ServerState m_serverState;
    #endregion 字段

    #region 属性

    /// <inheritdoc/>
    public override IClientCollection<TClient> Clients => this.m_clients;

    /// <inheritdoc/>
    public override int Count => this.m_clients.Count;

    /// <inheritdoc/>
    public IEnumerable<NamedPipeMonitor> Monitors => this.m_monitors;

    /// <inheritdoc/>
    public override ServerState ServerState => this.m_serverState;
    #endregion 属性


    /// <inheritdoc/>
    public void AddListen(NamedPipeListenOption option)
    {
        this.ThrowIfDisposed();
        ThrowHelper.ThrowArgumentNullExceptionIf(option, nameof(option));
        ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(option.PipeName, nameof(option.PipeName));
        

        var networkMonitor = new NamedPipeMonitor(option);

        _ = EasyTask.SafeRun(this.ThreadBegin, networkMonitor);
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
    public bool RemoveListen(NamedPipeMonitor monitor)
    {
        this.ThrowIfDisposed();
        ThrowHelper.ThrowArgumentNullExceptionIf(monitor, nameof(monitor));

        if (this.m_monitors.Remove(monitor))
        {
            monitor.Dispose();
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public override async Task ResetIdAsync(string sourceId, string targetId, CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();
        ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(sourceId, nameof(sourceId));
        ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(targetId, nameof(targetId));

        if (sourceId == targetId)
        {
            return;
        }
        if (this.m_clients.TryGetClient(sourceId, out var sessionClient))
        {
            await sessionClient.ResetIdAsync(targetId, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else
        {
            throw new ClientNotFindException(TouchSocketResource.ClientNotFind.Format(sourceId));
        }
    }
    /// <inheritdoc/>
    public override async Task StartAsync()
    {
        this.ThrowIfConfigIsNull();

        var optionList = this.Config.NamedPipeListenOption ?? new List<NamedPipeListenOption>();

        var pipeName = this.Config.GetValue(NamedPipeConfigExtension.PipeNameProperty);
        if (pipeName != null)
        {
            var option = new NamedPipeListenOption
            {
                PipeName = pipeName,
                Adapter = this.Config.GetValue(NamedPipeConfigExtension.NamedPipeDataHandlingAdapterProperty),
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
                case ServerState.Stopped:
                    {
                        this.m_serverState = ServerState.Running;
                        this.BeginListen(optionList);
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
    public override async Task<Result> StopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await this.ClearAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.m_serverState = ServerState.Stopped;
            await this.PluginManager.RaiseAsync(typeof(IServerStoppedPlugin), this.Resolver, this, new ServiceStateEventArgs(this.m_serverState, default)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return Result.Success;
        }
        catch (Exception ex)
        {
            this.m_serverState = ServerState.Exception;
            await this.PluginManager.RaiseAsync(typeof(IServerStoppedPlugin), this.Resolver, this, new ServiceStateEventArgs(this.m_serverState, ex) { Message = ex.Message }).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return Result.FromException(ex);
        }
        finally
        {
            this.m_monitors.Clear();
        }
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
        try
        {
            var client = this.NewClient();

            this.ClientInitialized(client);

            await client.InternalInitialized(this.Config,
                monitor.Option,
                this.Resolver,
                this.PluginManager,
                this,
                this.TryAdd,
                this.TryRemove,
                this.TryGet).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            var args = new ConnectingEventArgs()
            {
                Id = this.GetNextNewId(client)
            };
            await client.InternalNamedPipeConnecting(args).ConfigureAwait(EasyTask.ContinueOnCapturedContext);//Connecting
            if (!args.IsPermitOperation)
            {
                return;
            }

            client.InternalSetId(args.Id);
            if (!this.m_clients.TryAdd(client))
            {
                this.Logger?.Error(this, TouchSocketResource.IdAlreadyExists.Format(args.Id));
                return;
            }
            await client.InternalNamedPipeConnected(new NamedPipeTransport(namedPipe, this.Config.GetValue(TouchSocketConfigExtension.TransportOptionProperty))).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        catch (Exception ex)
        {
            this.Logger?.Debug(this, ex);
        }
        finally
        {
            namedPipe.SafeDispose();
        }
    }

    private async Task ThreadBegin(NamedPipeMonitor monitor)
    {
        var cancellationToken = monitor.MonitorToken;
        var option = monitor.Option;
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (this.ServerState != ServerState.Running)
                {
                    return;
                }
                var namedPipe = new NamedPipeServerStream(option.PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, 0);

                await namedPipe.WaitForConnectionAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                _ = EasyTask.SafeRun(this.OnClientSocketInit, namedPipe, monitor);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                this.Logger?.Debug(this, ex);
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