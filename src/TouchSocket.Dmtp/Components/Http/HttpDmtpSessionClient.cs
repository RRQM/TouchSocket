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

using TouchSocket.Http;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp;

/// <summary>
/// 表示基于HTTP协议的Dmtp会话客户端，继承自<see cref="HttpSessionClient"/>，实现<see cref="IHttpDmtpSessionClient"/>接口。
/// </summary>
public abstract class HttpDmtpSessionClient : HttpSessionClient, IHttpDmtpSessionClient
{
    private SealedDmtpActor m_dmtpActor;

    /// <inheritdoc/>
    public IDmtpActor DmtpActor => this.m_dmtpActor;

    /// <inheritdoc/>
    public override bool Online => this.m_dmtpActor == null ? base.Online : this.m_dmtpActor.Online;

    /// <summary>
    /// 验证超时时间
    /// </summary>
    public TimeSpan VerifyTimeout => this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyTimeout;

    /// <summary>
    /// 连接令箭
    /// </summary>
    public string VerifyToken => this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyToken;

    #region 断开

    /// <inheritdoc/>
    public override async Task<Result> CloseAsync(string msg, CancellationToken token = default)
    {
        if (this.m_dmtpActor != null)
        {
            await this.m_dmtpActor.CloseAsync(msg, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }

        return await base.CloseAsync(msg, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        if (this.DisposedValue)
        {
            return;
        }
        base.SafetyDispose(disposing);
        if (disposing && this.m_dmtpActor != null)
        {
            this.m_dmtpActor.Dispose();
        }
    }

    #endregion 断开

    #region ResetId

    ///<inheritdoc/>
    public override async Task ResetIdAsync(string newId, CancellationToken token)
    {
        if (this.m_dmtpActor == null)
        {
            await base.ResetIdAsync(newId, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }
        await this.m_dmtpActor.ResetIdAsync(newId, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion ResetId

    private async Task OnDmtpIdChanged(DmtpActor actor, IdChangedEventArgs e)
    {
        await this.ProtectedResetIdAsync(e.NewId).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private void InitDmtpActor()
    {
        var allowRoute = false;
        Func<string, Task<IDmtpActor>> findDmtpActor = default;
        var dmtpRouteService = this.Resolver.Resolve<IDmtpRouteService>();
        if (dmtpRouteService != null)
        {
            allowRoute = true;
            findDmtpActor = dmtpRouteService.FindDmtpActor;
        }

        async Task<IDmtpActor> FindDmtpActor(string id)
        {
            if (allowRoute)
            {
                if (findDmtpActor != null)
                {
                    return await findDmtpActor.Invoke(id).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
                if (this.ProtectedTryGetClient(id, out var client) && client is IDmtpActorObject dmtpActorObject)
                {
                    return dmtpActorObject.DmtpActor;
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        var actor = new SealedDmtpActor(allowRoute, true)
        {
            Id = this.Id,
            FindDmtpActor = FindDmtpActor,
            IdChanged = this.OnDmtpIdChanged,
            OutputSendAsync = this.ThisDmtpActorOutputSendAsync,
            Client = this,
            Closing = this.OnDmtpActorClose,
            Routing = this.OnDmtpActorRouting,
            Connected = this.OnDmtpActorConnected,
            Connecting = this.OnDmtpActorConnecting,
            CreatedChannel = this.OnDmtpActorCreatedChannel,
            Logger = this.Logger
        };

        this.m_dmtpActor = actor;

        this.Protocol = DmtpUtility.DmtpProtocol;
        this.SetAdapter(new DmtpAdapter());
    }

    private Task ThisDmtpActorOutputSendAsync(DmtpActor actor, ReadOnlyMemory<byte> memory, CancellationToken token)
    {
        return base.ProtectedSendAsync(memory, token);
    }

    #region Override

    /// <inheritdoc/>
    protected override async Task OnReceivedHttpRequest(HttpContext httpContext)
    {
        var request = httpContext.Request;
        var response = httpContext.Response;

        if (request.IsMethod(DmtpUtility.Dmtp)
            && request.IsUpgrade()
            && request.Headers.Contains(HttpHeaders.Upgrade, DmtpUtility.Dmtp))
        {
            this.InitDmtpActor();

            await response.SetStatus(101, "Switching Protocols")
                .AnswerAsync()
                .ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }
        await base.OnReceivedHttpRequest(httpContext).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosed(ClosedEventArgs e)
    {
        await this.OnDmtpClosed(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        await base.OnTcpClosed(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosing(ClosingEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(IDmtpClosingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        await base.OnTcpClosing(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        if (this.Protocol == DmtpUtility.DmtpProtocol && e.RequestInfo is DmtpMessage message)
        {
            if (!await this.m_dmtpActor.InputReceivedData(message).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
            {
                await this.PluginManager.RaiseAsync(typeof(IDmtpReceivedPlugin), this.Resolver, this, new DmtpMessageEventArgs(message)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }
        await base.OnTcpReceived(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion Override

    #region 内部委托绑定

    private Task OnDmtpActorClose(DmtpActor actor, string msg)
    {
        //base.Abort(false, msg);
        return EasyTask.CompletedTask;
    }

    private Task OnDmtpActorCreatedChannel(DmtpActor actor, CreateChannelEventArgs e)
    {
        return this.OnCreatedChannel(e);
    }

    private Task OnDmtpActorConnected(DmtpActor actor, DmtpVerifyEventArgs e)
    {
        return this.OnConnected(e);
    }

    private Task OnDmtpActorConnecting(DmtpActor actor, DmtpVerifyEventArgs e)
    {
        if (e.Token == this.VerifyToken)
        {
            e.IsPermitOperation = true;
        }
        else
        {
            e.Message = "Token不受理";
        }

        return this.OnConnecting(e);
    }

    private Task OnDmtpActorRouting(DmtpActor actor, PackageRouterEventArgs e)
    {
        return this.OnRouting(e);
    }

    #endregion 内部委托绑定

    #region 事件

    /// <summary>
    /// 当创建通道时触发的事件处理程序
    /// </summary>
    /// <param name="e">包含通道创建信息的事件参数</param>
    protected virtual async Task OnCreatedChannel(CreateChannelEventArgs e)
    {
        // 如果事件已经被处理，则直接返回
        if (e.Handled)
        {
            return;
        }

        // 异步调用插件管理器，通知所有实现IDmtpCreatedChannelPlugin接口的插件关于通道创建的事件
        await this.PluginManager.RaiseAsync(typeof(IDmtpCreatedChannelPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 当Dmtp关闭以后。
    /// </summary>
    /// <param name="e">事件参数</param>
    /// <returns></returns>
    protected virtual async Task OnDmtpClosed(ClosedEventArgs e)
    {
        //如果事件已经被处理，则直接返回
        if (e.Handled)
        {
            return;
        }
        //通知插件管理器，Dmtp已经关闭
        await this.PluginManager.RaiseAsync(typeof(IDmtpClosedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
    /// <param name="e">提供了关闭事件的相关信息。</param>
    /// <returns>返回一个Task对象，表示异步操作的完成。</returns>
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
    /// 在完成握手连接时
    /// </summary>
    /// <param name="e">包含握手信息的事件参数</param>
    protected virtual async Task OnConnected(DmtpVerifyEventArgs e)
    {
        // 如果握手已经被处理，则不再执行后续操作
        if (e.Handled)
        {
            return;
        }
        // 触发插件管理器中的握手完成插件事件
        await this.PluginManager.RaiseAsync(typeof(IDmtpConnectedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 在验证Token时
    /// </summary>
    /// <param name="e">参数</param>
    protected virtual async Task OnConnecting(DmtpVerifyEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }
        await this.PluginManager.RaiseAsync(typeof(IDmtpConnectingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 当需要转发路由包时。
    /// </summary>
    /// <param name="e">包含路由包相关信息的事件参数。</param>
    protected virtual async Task OnRouting(PackageRouterEventArgs e)
    {
        // 如果事件已经被处理，则直接返回。
        if (e.Handled)
        {
            return;
        }
        // 异步调用插件管理器，通知所有实现了IDmtpRoutingPlugin接口的插件处理路由包。
        await this.PluginManager.RaiseAsync(typeof(IDmtpRoutingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion 事件
}