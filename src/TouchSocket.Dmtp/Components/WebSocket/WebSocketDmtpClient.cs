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
using System.Net.WebSockets;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http.WebSockets;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp;

/// <summary>
/// WebSocketDmtpClient 类，继承自 SetupConfigObject 并实现了 IWebSocketDmtpClient 接口。
/// 该类负责 WebSocket 客户端的配置和管理，提供与 Dmtp 协议相关的功能。
/// </summary>
public class WebSocketDmtpClient : SetupClientWebSocket, IWebSocketDmtpClient
{
    /// <summary>
    /// 初始化WebSocketDmtpClient类的新实例。
    /// </summary>
    public WebSocketDmtpClient()
    {
        this.Protocol = DmtpUtility.DmtpProtocol;
    }

    #region 字段

    private readonly SemaphoreSlim m_connectionSemaphore = new SemaphoreSlim(1, 1);
    private bool m_allowRoute;
    private SealedDmtpActor m_dmtpActor;
    private DmtpAdapter m_dmtpAdapter;
    private Func<string, Task<IDmtpActor>> m_findDmtpActor;
    private CancellationTokenSource m_tokenSourceForReceive;

    #endregion 字段

    #region 连接

    /// <inheritdoc/>
    public override async Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
    {
        await this.m_connectionSemaphore.WaitTimeAsync(millisecondsTimeout, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        try
        {
            if (this.Online)
            {
                return;
            }

            if (!base.Online)
            {
                await base.ConnectAsync(millisecondsTimeout, token);
            }

            this.m_dmtpActor = new SealedDmtpActor(this.m_allowRoute)
            {
                OutputSendAsync = this.OnDmtpActorSendAsync,
                Routing = this.OnDmtpActorRouting,
                Handshaking = this.OnDmtpActorHandshaking,
                Handshaked = this.OnDmtpActorHandshaked,
                Closing = this.OnDmtpActorClose,
                Logger = this.Logger,
                Client = this,
                FindDmtpActor = this.m_findDmtpActor,
                CreatedChannel = this.OnDmtpActorCreateChannel
            };

            this.m_dmtpAdapter = new DmtpAdapter();
            this.m_dmtpAdapter.Config(this.Config);
            this.m_tokenSourceForReceive = new CancellationTokenSource();

            var option = this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty);

            await this.m_dmtpActor.HandshakeAsync(option.VerifyToken, option.Id, millisecondsTimeout, option.Metadata, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            this.m_connectionSemaphore.Release();
        }
    }

    #endregion 连接

    /// <inheritdoc/>
    public IDmtpActor DmtpActor => this.m_dmtpActor;

    /// <inheritdoc/>
    public string Id => this.m_dmtpActor?.Id;

    /// <inheritdoc/>
    public bool IsClient => true;

    /// <inheritdoc/>
    public override bool Online => base.Online && this.m_dmtpActor != null && this.m_dmtpActor.Online;

    
    /// <inheritdoc/>
    public override async Task<Result> CloseAsync(string msg, CancellationToken token = default)
    {
        try
        {
            await this.OnDmtpClosing(new ClosingEventArgs(msg)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            var dmtpActor = this.m_dmtpActor;
            if (dmtpActor != null)
            {
                // 向IDmtpActor对象发送关闭消息
                await dmtpActor.SendCloseAsync(msg).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                // 关闭IDmtpActor对象
                await dmtpActor.CloseAsync(msg, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }

            await base.CloseAsync(msg, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <inheritdoc/>
    public Task ResetIdAsync(string newId)
    {
        return this.m_dmtpActor.ResetIdAsync(newId);
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
            this.Abort(true, $"调用{nameof(Dispose)}");
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    protected override void LoadConfig(TouchSocketConfig config)
    {
        var dmtpRouteService = this.Resolver.Resolve<IDmtpRouteService>();
        if (dmtpRouteService != null)
        {
            this.m_allowRoute = true;
            this.m_findDmtpActor = dmtpRouteService.FindDmtpActor;
        }
    }

    /// <inheritdoc/>
    protected override async Task OnReceived(WebSocketReceiveResult result, ByteBlock byteBlock)
    {
        try
        {
            //处理数据
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
        }
        catch (Exception ex)
        {
            this.Logger?.Debug(this, ex);
        }
    }

    #region 内部委托绑定

    /// <inheritdoc/>
    protected override Task OnWebSocketClosed(ClosedEventArgs e)
    {
        return this.PrivateOnDmtpClosed(e);
    }

    private async Task OnDmtpActorClose(DmtpActor actor, string msg)
    {
        await this.OnDmtpClosing(new ClosingEventArgs(msg)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        this.Abort(false, msg);
    }

    private Task OnDmtpActorCreateChannel(DmtpActor actor, CreateChannelEventArgs e)
    {
        return this.OnCreateChannel(e);
    }

    private Task OnDmtpActorHandshaked(DmtpActor actor, DmtpVerifyEventArgs e)
    {
        return this.OnHandshaked(e);
    }

    private Task OnDmtpActorHandshaking(DmtpActor actor, DmtpVerifyEventArgs e)
    {
        return this.OnHandshaking(e);
    }

    private Task OnDmtpActorRouting(DmtpActor actor, PackageRouterEventArgs e)
    {
        return this.OnRouting(e);
    }

    private async Task OnDmtpActorSendAsync(DmtpActor actor, ReadOnlyMemory<byte> memory)
    {
        await base.ProtectedSendAsync(memory, WebSocketMessageType.Binary, true, CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion 内部委托绑定

    #region 事件触发

    /// <summary>
    /// 当创建通道时触发的事件处理程序
    /// </summary>
    /// <param name="e">包含通道创建信息的事件参数</param>
    protected virtual async Task OnCreateChannel(CreateChannelEventArgs e)
    {
        // 如果事件已经被处理，则直接返回
        if (e.Handled)
        {
            return;
        }

        // 异步调用插件管理器，通知所有实现IDmtpCreatedChannelPlugin接口的插件处理通道创建事件
        await this.PluginManager.RaiseAsync(typeof(IDmtpCreatedChannelPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 已断开连接。
    /// </summary>
    /// <param name="e">包含断开连接信息的事件参数</param>
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
    protected virtual async Task OnHandshaked(DmtpVerifyEventArgs e)
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
    protected virtual async Task OnHandshaking(DmtpVerifyEventArgs e)
    {
        // 如果握手已经被处理，则直接返回
        if (e.Handled)
        {
            return;
        }
        // 触发握手过程的插件事件
        await this.PluginManager.RaiseAsync(typeof(IDmtpHandshakingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 当需要转发路由包时
    /// </summary>
    /// <param name="e">包含路由包相关信息的事件参数</param>
    protected virtual async Task OnRouting(PackageRouterEventArgs e)
    {
        // 如果事件已经被处理，则直接返回
        if (e.Handled)
        {
            return;
        }
        // 异步调用插件管理器，通知所有实现了IDmtpRoutingPlugin接口的插件处理路由包
        await this.PluginManager.RaiseAsync(typeof(IDmtpRoutingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task PrivateOnDmtpClosed(ClosedEventArgs e)
    {
        await this.OnDmtpClosed(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion 事件触发
}