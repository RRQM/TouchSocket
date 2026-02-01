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

using TouchSocket.Sockets;

namespace TouchSocket.Dmtp;

/// <summary>
/// 抽象类TcpDmtpSessionClient定义了基于TCP的Dmtp会话客户端的基本行为。
/// 它扩展了TcpSessionClientBase类，并实现了ITcpDmtpSessionClient接口。
/// </summary>
public abstract class TcpDmtpSessionClient : TcpSessionClientBase, ITcpDmtpSessionClient
{
    private readonly DmtpAdapter m_dmtpAdapter = new();
    private SealedDmtpActor m_dmtpActor;
    /// <summary>
    /// 构造函数：初始化TcpDmtpSessionClient实例
    /// </summary>
    /// <remarks>
    /// 设置协议属性为DmtpProtocol，这是应用程序通信使用的协议
    /// </remarks>
    protected TcpDmtpSessionClient()
    {
        this.Protocol = DmtpUtility.DmtpProtocol;
    }

    /// <inheritdoc/>
    public IDmtpActor DmtpActor => this.m_dmtpActor;

    /// <inheritdoc cref="IOnlineClient.Online"/>
    public override bool Online => this.m_dmtpActor != null && this.m_dmtpActor.Online;

    /// <summary>
    /// 验证超时时间,默认为3000ms
    /// </summary>
    public TimeSpan VerifyTimeout => this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyTimeout;

    /// <summary>
    /// 连接令箭
    /// </summary>
    public string VerifyToken => this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyToken;

    #region 内部委托绑定

    private Task OnDmtpActorClose(DmtpActor actor, string msg)
    {
        //base.Abort(false, msg);
        return EasyTask.CompletedTask;
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

    #endregion 内部委托绑定

    #region 事件

    /// <summary>
    /// 当创建通道时触发的事件处理方法
    /// 此方法主要用于在创建通道时，触发相关插件的调用
    /// </summary>
    /// <param name="e">包含通道创建信息的事件参数</param>
    protected virtual async Task OnCreateChannel(CreateChannelEventArgs e)
    {
        // 如果事件已经被处理，则直接返回
        if (e.Handled)
        {
            return;
        }

        // 异步调用所有IDmtpCreatedChannelPlugin类型的插件，并传递当前实例和事件参数
        await this.PluginManager.RaiseAsync(typeof(IDmtpCreatedChannelPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 当Dmtp关闭以后。
    /// </summary>
    /// <param name="e">事件参数</param>
    /// <returns></returns>
    protected virtual async Task OnDmtpClosed(ClosedEventArgs e)
    {
        // 如果事件已经被处理，则直接返回
        if (e.Handled)
        {
            return;
        }
        // 通知插件管理器，Dmtp已经关闭
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
    /// <param name="e">关闭事件参数，包含关闭的原因等信息。</param>
    /// <returns>返回一个异步任务，表示事件处理的完成。</returns>
    protected virtual async Task OnDmtpClosing(ClosingEventArgs e)
    {
        // 如果事件已经被处理，则直接返回
        if (e.Handled)
        {
            return;
        }
        // 通知插件管理器，Dmtp即将被关闭
        await this.PluginManager.RaiseAsync(typeof(IDmtpClosingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 在完成握手连接时
    /// </summary>
    /// <param name="e">包含握手信息的事件参数</param>
    protected virtual async Task OnConnected(DmtpVerifyEventArgs e)
    {
        // 如果握手已经被处理，则不再继续执行
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
        // 如果事件已经被处理，则直接返回
        if (e.Handled)
        {
            return;
        }
        // 异步调用插件管理器，执行握手验证插件
        await this.PluginManager.RaiseAsync(typeof(IDmtpConnectingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 在需要转发路由包时。
    /// </summary>
    /// <param name="e">路由事件参数</param>
    protected virtual async Task OnRouting(PackageRouterEventArgs e)
    {
        // 如果事件已经被处理，则直接返回
        if (e.Handled)
        {
            return;
        }
        // 异步调用插件管理器，执行路由转发插件
        await this.PluginManager.RaiseAsync(typeof(IDmtpRoutingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion 事件

    #region 断开

    /// <summary>
    /// 发送<see cref="IDmtpActor"/>关闭消息。
    /// </summary>
    /// <param name="msg">关闭消息的内容</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>异步任务</returns>
    public override async Task<Result> CloseAsync(string msg, CancellationToken cancellationToken = default)
    {
        
        if (this.m_dmtpActor != null)
        {
            //issue：https://gitee.com/RRQM_Home/TouchSocket/issues/IDND2L
            await this.m_dmtpActor.SendCloseAsync(msg,cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
           
            await this.m_dmtpActor.CloseAsync(msg, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }

        // 调用基类的CloseAsync方法发送关闭消息
        return await base.CloseAsync(msg, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        this.m_dmtpActor?.SafeDispose();
        base.SafetyDispose(disposing);
    }

    #endregion 断开

    #region ResetId

    ///<inheritdoc/>
    public override Task ResetIdAsync(string newId, CancellationToken cancellationToken = default)
    {
        return this.m_dmtpActor.ResetIdAsync(newId, cancellationToken);
    }

    #endregion ResetId

    private void InitDmtpActor(string id)
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
            Id = id,
            FindDmtpActor = FindDmtpActor,
            IdChanged = this.ThisOnResetId,
            OutputSendAsync = this.ThisDmtpActorOutputSendAsync,
            Client = this,
            Closing = this.OnDmtpActorClose,
            Routing = this.OnDmtpActorRouting,
            Connected = this.OnDmtpActorConnected,
            Connecting = this.OnDmtpActorConnecting,
            CreatedChannel = this.OnDmtpActorCreateChannel,
            Logger = this.Logger
        };

        this.m_dmtpActor = actor;
        this.m_dmtpAdapter.Config(this.Config);
    }

    private Task ThisDmtpActorOutputSendAsync(DmtpActor actor, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken)
    {
        if (memory.Length > this.m_dmtpAdapter.MaxPackageSize)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(memory.Length), memory.Length, this.m_dmtpAdapter.MaxPackageSize);
        }

        return base.ProtectedSendAsync(memory, cancellationToken);
    }

    private async Task ThisOnResetId(DmtpActor rpcActor, IdChangedEventArgs e)
    {
        await this.ProtectedResetIdAsync(e.NewId).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #region Override

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
    protected override async Task OnTcpConnected(ConnectedEventArgs e)
    {
        _ = EasyTask.SafeRun(async () =>
        {
            await Task.Delay(this.VerifyTimeout).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            if (!this.Online)
            {
                await base.CloseAsync("握手验证超时").ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        });

        await base.OnTcpConnected(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpConnecting(ConnectingEventArgs e)
    {
        await base.OnTcpConnecting(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        this.InitDmtpActor(e.Id);
    }

    ///// <inheritdoc/>
    //protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    //{
    //    var message = (DmtpMessage)e.RequestInfo;
    //    if (!await this.m_dmtpActor.InputReceivedData(message).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
    //    {
    //        await this.PluginManager.RaiseAsync(typeof(IDmtpReceivedPlugin), this.Resolver, this, new DmtpMessageEventArgs(message)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    //    }
    //}

    /// <inheritdoc/>
    protected override async ValueTask<bool> OnTcpReceiving(IBytesReader reader)
    {
        while (reader.BytesRemaining > 0)
        {
            if (this.m_dmtpAdapter.TryParseRequest(ref reader, out var message))
            {
                if (!await this.m_dmtpActor.InputReceivedData(message).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                {
                    await this.PluginManager.RaiseAsync(typeof(IDmtpReceivedPlugin), this.Resolver, this, new DmtpMessageEventArgs(message)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
            }
            else
            {
                break;
            }
        }
        return true;
    }

    #endregion Override
}