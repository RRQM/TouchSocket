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

using Microsoft.AspNetCore.Http;
using System.Buffers;
using System.Net.WebSockets;
using TouchSocket.Http.WebSockets;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp.AspNetCore;

/// <summary>
/// WebSocket Dmtp会话客户端。
/// </summary>
public class WebSocketDmtpSessionClient : ResolverConfigObject, IWebSocketDmtpSessionClient
{
    /// <summary>
    /// 初始化WebSocket Dmtp会话客户端。
    /// </summary>
    public WebSocketDmtpSessionClient()
    {
        this.m_receiveCounter = new ValueCounter(TimeSpan.FromSeconds(1), this.OnReceivePeriod);
        this.m_sentCounter = new ValueCounter(TimeSpan.FromSeconds(1), this.OnSendPeriod);
    }

    #region 字段
    private WebSocket m_client;
    private ClosedEventArgs m_closedEventArgs;
    private TouchSocketConfig m_config;
    private SealedDmtpActor m_dmtpActor;
    private DmtpAdapter m_dmtpAdapter;
    private HttpContext m_httpContext;
    private string m_id;
    private IPluginManager m_pluginManager;
    private ValueCounter m_receiveCounter;
    private IResolver m_resolver;
    private ValueCounter m_sentCounter;
    private WebSocketDmtpService m_service;

    #endregion 字段

    /// <inheritdoc/>
    public CancellationToken ClosedToken => this.m_httpContext?.RequestAborted ?? new CancellationToken(true);

    /// <inheritdoc/>
    public override TouchSocketConfig Config => this.m_config;

    /// <inheritdoc/>
    public IDmtpActor DmtpActor => this.m_dmtpActor;

    /// <inheritdoc/>
    public HttpContext HttpContext => this.m_httpContext;

    /// <inheritdoc/>
    public string Id => this.m_id;

    /// <inheritdoc/>
    public bool IsClient => false;

    /// <inheritdoc/>
    public DateTimeOffset LastReceivedTime => this.m_receiveCounter.LastIncrement;

    /// <inheritdoc/>
    public DateTimeOffset LastSentTime => this.m_sentCounter.LastIncrement;

    /// <inheritdoc/>
    public bool Online => this.DmtpActor.Online;

    /// <inheritdoc/>
    public override IPluginManager PluginManager => this.m_pluginManager;

    /// <inheritdoc/>
    public Protocol Protocol { get; protected set; } = DmtpUtility.DmtpProtocol;

    /// <inheritdoc/>
    public override IResolver Resolver => this.m_resolver;

    /// <inheritdoc/>
    public IWebSocketDmtpService Service => this.m_service;

    /// <inheritdoc/>
    public TimeSpan VerifyTimeout => this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyTimeout;

    /// <inheritdoc/>
    public string VerifyToken => this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyToken;

    /// <inheritdoc/>
    public async Task<Result> CloseAsync(string msg, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!this.Online)
            {
                return Result.Success;
            }
            await this.OnDmtpClosing(new ClosingEventArgs(msg)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            var dmtpActor = this.m_dmtpActor;
            if (dmtpActor != null)
            {
                // 向IDmtpActor对象发送关闭消息
                await dmtpActor.SendCloseAsync(msg).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                // 关闭IDmtpActor对象
                await dmtpActor.CloseAsync(msg, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }

            await this.m_client.SafeCloseClientAsync(msg, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <inheritdoc/>
    public async Task ResetIdAsync(string newId, CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(newId, nameof(newId));
        if (this.m_id == newId)
        {
            return;
        }

        await this.DmtpActor.ResetIdAsync(newId, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        await this.ProtectedResetIdAsync(newId).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    internal void InternalSetConfig(TouchSocketConfig config)
    {
        this.m_config = config;
        this.m_dmtpAdapter = new DmtpAdapter()
        {
            ReceivedAsyncCallBack = this.PrivateHandleReceivedData
        };

        this.m_dmtpAdapter.Config(config);
    }

    internal void InternalSetContainer(IResolver resolver)
    {
        this.m_resolver = resolver;
        this.Logger ??= resolver.Resolve<ILog>();
    }

    internal void InternalSetId(string id)
    {
        this.m_id = id;
    }

    internal void InternalSetPluginManager(IPluginManager pluginManager)
    {
        this.m_pluginManager = pluginManager;
    }

    internal void InternalSetService(WebSocketDmtpService service)
    {
        this.m_service = service;
    }

    internal void InitDmtpActor(bool allowRoute, Func<string, Task<IDmtpActor>> onServiceFindDmtpActor)
    {
        var actor = new SealedDmtpActor(allowRoute, true)
        {
            Id = this.m_id,
            FindDmtpActor = onServiceFindDmtpActor,
            IdChanged = this.ThisOnResetId,
            OutputSendAsync = this.OnDmtpActorSendAsync,
            Client = this,
            Closing = this.OnDmtpActorClose,
            Routing = this.OnDmtpActorRouting,
            Connected = this.OnDmtpActorConnected,
            Connecting = this.OnDmtpActorConnecting,
            CreatedChannel = this.OnDmtpActorCreateChannel,
            Logger = this.Logger
        };

        this.m_dmtpActor = actor;
    }

    internal async Task Start(WebSocket client, HttpContext context)
    {
        var cancellationToken = context.RequestAborted;
        this.m_client = client;
        this.m_httpContext = context;
        await this.ReceiveLoopAsync(client, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        this.m_service.TryRemove(this.m_id, out _);
    }

    /// <summary>
    /// 直接重置Id。
    /// </summary>
    /// <param name="targetId">目标Id</param>
    /// <exception cref="Exception">Id重复时抛出异常</exception>
    /// <exception cref="ClientNotFindException">找不到客户端时抛出异常</exception>
    protected async Task ProtectedResetIdAsync(string targetId)
    {
        var sourceId = this.m_id;
        if (this.m_service.TryRemove(this.m_id, out var sessionClient))
        {
            sessionClient.m_id = targetId;
            if (this.m_service.TryAdd(sessionClient))
            {
                if (this.PluginManager.Enable)
                {
                    var e = new IdChangedEventArgs(sourceId, targetId);
                    await this.PluginManager.RaiseAsync(typeof(IIdChangedPlugin), this.Resolver, sessionClient, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
                return;
            }
            else
            {
                sessionClient.m_id = sourceId;
                if (this.m_service.TryAdd(sessionClient))
                {
                    throw new Exception("Id重复");
                }
                else
                {
                    await sessionClient.CloseAsync("修改新Id时操作失败，且回退旧Id时也失败。").ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
            }
        }
        else
        {
            throw new ClientNotFindException(TouchSocketAspNetCoreResource.ClientNotFind.GetDescription(sourceId));
        }
    }

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            _ = EasyTask.SafeRun(async () => await this.CloseAsync(TouchSocketResource.DisposeClose).ConfigureAwait(EasyTask.ContinueOnCapturedContext));
        }
    }

    private void OnReceivePeriod(long value)
    {
        //this.m_receiveBufferSize = TouchSocketCoreUtility.HitBufferLength(value);
    }

    private void OnSendPeriod(long value)
    {
        //this.m_sendBufferSize = TouchSocketCoreUtility.HitBufferLength(value);
    }

    private async Task OnWebSocketReceived(System.Net.WebSockets.WebSocketReceiveResult result, ReadOnlySequence<byte> sequence)
    {
        try
        {
            using (var buffer = new ContiguousMemoryBuffer(sequence))
            {
                var message = DmtpMessage.CreateFrom(buffer.Memory);
                if (!await this.m_dmtpActor.InputReceivedData(message).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                {
                    await this.PluginManager.RaiseAsync(typeof(IDmtpReceivedPlugin), this.Resolver, this, new DmtpMessageEventArgs(message)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
            }
        }
        catch (Exception ex)
        {
            this.Logger?.Debug(this, ex);
        }
    }

    private async Task PrivateHandleReceivedData(ReadOnlyMemory<byte> memory, IRequestInfo requestInfo)
    {
        var message = (DmtpMessage)requestInfo;
        if (!await this.m_dmtpActor.InputReceivedData(message).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
        {
            await this.PluginManager.RaiseAsync(typeof(IDmtpReceivedPlugin), this.Resolver, this, new DmtpMessageEventArgs(message)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    private async Task ReceiveLoopAsync(WebSocket client, CancellationToken cancellationToken)
    {
        try
        {
            SegmentedBytesWriter writer = default;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (client == null || client.State != WebSocketState.Open)
                    {
                        break;
                    }
                    writer ??= new SegmentedBytesWriter(1024 * 64);
                    var segment = writer.GetMemory(1024 * 64).GetArray();
                    var result = await client.ReceiveAsync(segment, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        try
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                            }
                        }
                        catch
                        {
                        }
                        break;
                    }

                    if (result.Count == 0)
                    {
                        break;
                    }
                    this.m_receiveCounter.Increment(result.Count);
                    writer.Advance(result.Count);
                    if (result.EndOfMessage)
                    {
                        //处理数据
                        await this.OnWebSocketReceived(result, writer.Sequence).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        writer.Dispose();
                        writer = null;
                    }
                }
                catch (Exception ex)
                {
                    this.Logger?.Debug(this, ex);
                    break;
                }
            }
            this.m_closedEventArgs ??= new ClosedEventArgs(false, TouchSocketResource.RemoteDisconnects);
        }
        catch (Exception ex)
        {
            this.m_closedEventArgs = new ClosedEventArgs(false, ex.Message);
            this.Logger?.Debug(this, ex);
        }
    }

    #region 内部委托绑定

    private async Task OnDmtpActorClose(DmtpActor actor, string msg)
    {
        await this.CloseAsync(msg, CancellationToken.None);
    }

    private Task OnDmtpActorCreateChannel(DmtpActor actor, CreateChannelEventArgs e)
    {
        return this.OnCreateChannel(e);
    }

    private Task OnDmtpActorConnected(DmtpActor actor, DmtpVerifyEventArgs e)
    {
        return this.OnConnected(e);
    }

    private async Task OnDmtpActorConnecting(DmtpActor actor, DmtpVerifyEventArgs e)
    {
        if (e.Token == this.VerifyToken)
        {
            e.IsPermitOperation = true;
        }
        else
        {
            e.Message = "Token不受理";
        }
        await this.OnConnecting(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private Task OnDmtpActorRouting(DmtpActor actor, PackageRouterEventArgs e)
    {
        return this.OnRouting(e);
    }

    private async Task OnDmtpActorSendAsync(DmtpActor actor, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken)
    {
        await this.m_client.SendAsync(memory.GetArray(), WebSocketMessageType.Binary, true, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        this.m_sentCounter.Increment(memory.Length);
    }

    #endregion 内部委托绑定

    #region 事件

    /// <summary>
    /// 当创建通道
    /// </summary>
    /// <param name="e">通道事件参数</param>
    protected virtual async Task OnCreateChannel(CreateChannelEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        await this.PluginManager.RaiseAsync(typeof(IDmtpCreatedChannelPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 在完成握手连接时
    /// </summary>
    /// <param name="e">验证事件参数</param>
    protected virtual async Task OnConnected(DmtpVerifyEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }
        await this.PluginManager.RaiseAsync(typeof(IDmtpConnectedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 在验证Token时
    /// </summary>
    /// <param name="e">验证事件参数</param>
    protected virtual async Task OnConnecting(DmtpVerifyEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }
        await this.PluginManager.RaiseAsync(typeof(IDmtpConnectingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 在需要转发路由包时。
    /// </summary>
    /// <param name="e">路由事件参数</param>
    protected virtual async Task OnRouting(PackageRouterEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }
        await this.PluginManager.RaiseAsync(typeof(IDmtpRoutingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 当Dmtp即将被关闭时触发。
    /// <para>
    /// 该触发条件有2种：
    /// <list type="number">
    /// <item>终端主动调用<see cref="IClosableClient.CloseAsync(string, System.Threading.CancellationToken)"/>。</item>
    /// <item>终端收到<see cref="DmtpActor.P0_Close"/>的请求。</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="e">关闭事件参数</param>
    /// <returns>异步操作任务</returns>
    protected virtual async Task OnDmtpClosing(ClosingEventArgs e)
    {
        // 如果关闭事件已经被处理，则直接返回，不再执行后续操作。
        if (e.Handled)
        {
            return;
        }
        // 通知插件管理器，触发IDmtpClosingPlugin接口的事件处理程序，并传递相关参数。
        await this.PluginManager.RaiseAsync(typeof(IDmtpClosingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 已断开连接。
    /// </summary>
    /// <param name="e">断开事件参数</param>
    protected virtual async Task OnDmtpClosed(ClosedEventArgs e)
    {
        // 如果事件已经被处理，则直接返回
        if (e.Handled)
        {
            return;
        }
        // 异步触发插件管理器中的 IDmtpClosedPlugin 接口的事件，并传递相关参数
        await this.PluginManager.RaiseAsync(typeof(IDmtpClosedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }
    #endregion 事件

    private async Task ThisOnResetId(DmtpActor actor, IdChangedEventArgs e)
    {
        await this.ProtectedResetIdAsync(e.NewId).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }
}