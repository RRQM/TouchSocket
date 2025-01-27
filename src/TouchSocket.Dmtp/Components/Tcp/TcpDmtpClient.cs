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
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp;

/// <summary>
/// TcpDmtpClient 类是部分类，实现了 ITcpDmtpClient 接口，用于提供基于 TCP 协议的客户端功能。
/// 继承自 TcpClientBase，复用基础的 TCP 客户端功能。
/// </summary>
public partial class TcpDmtpClient : TcpClientBase, ITcpDmtpClient
{
    /// <summary>
    /// 初始化TcpDmtpClient类的新实例
    /// </summary>
    public TcpDmtpClient()
    {
        // 设置协议属性为DmtpProtocol，表示使用DMTP协议
        this.Protocol = DmtpUtility.DmtpProtocol;
    }

    /// <inheritdoc/>
    public IDmtpActor DmtpActor => this.m_dmtpActor;

    /// <inheritdoc cref="IIdClient.Id"/>
    public string Id => this.m_dmtpActor.Id;

    #region 字段

    private readonly SemaphoreSlim m_semaphoreForConnect = new SemaphoreSlim(1, 1);
    private bool m_allowRoute;
    private SealedDmtpActor m_dmtpActor;
    private DmtpAdapter m_dmtpAdapter;
    private Func<string, Task<IDmtpActor>> m_findDmtpActor;

    #endregion 字段

    /// <inheritdoc cref="IOnlineClient.Online"/>
    public override bool Online => this.m_dmtpActor != null && this.m_dmtpActor.Online;

    #region 断开

    /// <summary>
    /// 发送<see cref="IDmtpActor"/>关闭消息。
    /// </summary>
    /// <param name="msg">关闭消息的内容</param>
    /// <returns>异步任务</returns>
    public override async Task CloseAsync(string msg)
    {
        // 检查是否已初始化IDmtpActor对象
        if (this.m_dmtpActor != null)
        {
            // 向IDmtpActor对象发送关闭消息
            await this.m_dmtpActor.SendCloseAsync(msg).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            // 关闭IDmtpActor对象
            await this.m_dmtpActor.CloseAsync(msg).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }

        // 调用基类的CloseAsync方法完成后续关闭操作
        await base.CloseAsync(msg).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion 断开

    #region 连接

    /// <inheritdoc/>
    public virtual async Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
    {
        await this.m_semaphoreForConnect.WaitTimeAsync(millisecondsTimeout, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        try
        {
            if (this.Online)
            {
                return;
            }
            if (!base.Online)
            {
                await base.TcpConnectAsync(millisecondsTimeout, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }

            await this.m_dmtpActor.HandshakeAsync(this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyToken, this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).Id, millisecondsTimeout, this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).Metadata, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            this.m_semaphoreForConnect.Release();
        }
    }

    #endregion 连接

    #region ResetId

    ///<inheritdoc/>
    public Task ResetIdAsync(string newId)
    {
        return this.m_dmtpActor.ResetIdAsync(newId);
    }

    #endregion ResetId

    /// <inheritdoc/>
    protected override void LoadConfig(TouchSocketConfig config)
    {
        base.LoadConfig(config);
        var dmtpRouteService = this.Resolver.Resolve<IDmtpRouteService>();
        if (dmtpRouteService != null)
        {
            this.m_allowRoute = true;
            this.m_findDmtpActor = dmtpRouteService.FindDmtpActor;
        }
        this.m_dmtpActor = new SealedDmtpActor(this.m_allowRoute)
        {
            OutputSendAsync = this.DmtpActorSendAsync,
            Routing = this.OnDmtpActorRouting,
            Handshaking = this.OnDmtpActorHandshaking,
            Handshaked = this.OnDmtpActorHandshaked,
            Closing = this.OnDmtpActorClose,
            Logger = this.Logger,
            Client = this,
            FindDmtpActor = this.m_findDmtpActor,
            CreatedChannel = this.OnDmtpActorCreateChannel
        };
    }

    #region 内部委托绑定

    private Task DmtpActorSendAsync(DmtpActor actor, ReadOnlyMemory<byte> memory)
    {
        if (memory.Length > this.m_dmtpAdapter.MaxPackageSize)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(memory.Length), memory.Length, this.m_dmtpAdapter.MaxPackageSize);
        }
        return base.ProtectedDefaultSendAsync(memory);
    }

    private async Task OnDmtpActorClose(DmtpActor actor, string msg)
    {
        await this.OnDmtpClosing(new ClosingEventArgs(msg)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        this.Abort(false, msg);
    }

    private Task OnDmtpActorCreateChannel(DmtpActor actor, CreateChannelEventArgs e)
    {
        return this.OnDmtpCreatedChannel(e);
    }

    private Task OnDmtpActorHandshaked(DmtpActor actor, DmtpVerifyEventArgs e)
    {
        return this.OnDmtpHandshaked(e);
    }

    private Task OnDmtpActorHandshaking(DmtpActor actor, DmtpVerifyEventArgs e)
    {
        return this.OnDmtpHandshaking(e);
    }

    private Task OnDmtpActorRouting(DmtpActor actor, PackageRouterEventArgs e)
    {
        return this.OnDmtpRouting(e);
    }

    #endregion 内部委托绑定

    #region 事件触发

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
    /// <item>终端主动调用<see cref="CloseAsync(string)"/>。</item>
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
    /// 当创建通道时触发的事件处理程序
    /// </summary>
    /// <param name="e">包含通道创建信息的事件参数</param>
    protected virtual async Task OnDmtpCreatedChannel(CreateChannelEventArgs e)
    {
        // 如果事件已经被处理，则直接返回
        if (e.Handled)
        {
            return;
        }

        // 异步调用所有实现IDmtpCreatedChannelPlugin接口的插件，通知通道创建
        await this.PluginManager.RaiseAsync(typeof(IDmtpCreatedChannelPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 在完成握手连接时
    /// </summary>
    /// <param name="e">包含握手信息的事件参数</param>
    protected virtual async Task OnDmtpHandshaked(DmtpVerifyEventArgs e)
    {
        // 如果握手已经被处理，则不再执行后续操作
        if (e.Handled)
        {
            return;
        }
        // 触发插件管理器中的握手完成插件事件
        await this.PluginManager.RaiseAsync(typeof(IDmtpHandshakedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 即将握手连接时
    /// </summary>
    /// <param name="e">参数</param>
    protected virtual async Task OnDmtpHandshaking(DmtpVerifyEventArgs e)
    {
        //如果参数已经被处理，则直接返回，不再执行后续操作
        if (e.Handled)
        {
            return;
        }
        //调用插件管理器，触发即将握手连接的事件
        await this.PluginManager.RaiseAsync(typeof(IDmtpHandshakingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 当需要转发路由包时
    /// </summary>
    /// <param name="e">包含路由包相关信息的事件参数</param>
    protected virtual async Task OnDmtpRouting(PackageRouterEventArgs e)
    {
        // 如果事件已经被处理，则直接返回
        if (e.Handled)
        {
            return;
        }
        // 异步调用插件管理器，通知所有实现了IDmtpRoutingPlugin接口的插件进行路由包的处理
        await this.PluginManager.RaiseAsync(typeof(IDmtpRoutingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion 事件触发

    #region Override

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        this.m_dmtpActor?.SafeDispose();
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosed(ClosedEventArgs e)
    {
        await this.m_dmtpActor.CloseAsync(e.Message).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
    protected sealed override async Task OnTcpConnecting(ConnectingEventArgs e)
    {
        this.m_dmtpAdapter = new DmtpAdapter();
        this.SetAdapter(this.m_dmtpAdapter);
        await base.OnTcpConnecting(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected sealed override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        var message = (DmtpMessage)e.RequestInfo;
        if (!await this.m_dmtpActor.InputReceivedData(message).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
        {
            await this.PluginManager.RaiseAsync(typeof(IDmtpReceivedPlugin), this.Resolver, this, new DmtpMessageEventArgs(message)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    /// <inheritdoc/>
    protected override async ValueTask<bool> OnTcpReceiving(ByteBlock byteBlock)
    {
        while (byteBlock.CanRead)
        {
            if (this.m_dmtpAdapter.TryParseRequest(ref byteBlock, out var message))
            {
                using (message)
                {
                    if (!await this.m_dmtpActor.InputReceivedData(message).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                    {
                        await this.PluginManager.RaiseAsync(typeof(IDmtpReceivedPlugin), this.Resolver, this, new DmtpMessageEventArgs(message)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }
                }
            }
        }
        return true;
    }

    #endregion Override
}