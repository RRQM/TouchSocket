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
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp;

/// <summary>
/// 定义了一个部分类HttpDmtpClient，它继承自HttpClientBase，并实现IHttpDmtpClient接口。
/// 这个类的目的是通过HTTP协议提供DMTP客户端功能，
/// 允许应用程序以一种标准化的方式发送和接收直接邮件。
/// </summary>
public partial class HttpDmtpClient : HttpClientBase, IHttpDmtpClient
{
    #region 字段

    private readonly SemaphoreSlim m_semaphoreForConnect = new SemaphoreSlim(1, 1);
    private bool m_allowRoute;
    private SealedDmtpActor m_dmtpActor;
    private Func<string, Task<IDmtpActor>> m_findDmtpActor;

    #endregion 字段

    /// <inheritdoc/>
    public IDmtpActor DmtpActor => this.m_dmtpActor;

    /// <inheritdoc cref="IIdClient.Id"/>
    public string Id => this.m_dmtpActor.Id;

    /// <inheritdoc cref="IOnlineClient.Online"/>
    public override bool Online => this.m_dmtpActor != null && this.m_dmtpActor.Online;

    #region 连接

    /// <summary>
    /// 异步使用基于Http升级的协议，连接Dmtp服务器
    /// </summary>
    /// <param name="millisecondsTimeout">连接超时时间，单位为毫秒</param>
    /// <param name="token">用于取消异步操作的取消令牌</param>
    /// <returns>异步操作任务</returns>
    /// <exception cref="Exception">在连接过程中遇到错误时抛出异常</exception>
    public async Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
    {
        // 等待信号量，以确保同时只有一个连接操作
        await this.m_semaphoreForConnect.WaitTimeAsync(millisecondsTimeout, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        try
        {
            // 如果已经在线，则无需进一步操作
            if (this.Online)
            {
                return;
            }
            // 如果基础连接不在状态，则尝试建立TCP连接
            if (!base.Online)
            {
                await base.TcpConnectAsync(millisecondsTimeout, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }

            // 创建并配置HttpRequest，为升级到Dmtp协议做准备
            var request = new HttpRequest()
                .SetHost(this.RemoteIPHost.Host);
            request.Headers.Add(HttpHeaders.Connection, "upgrade");
            request.Headers.Add(HttpHeaders.Upgrade, DmtpUtility.Dmtp.ToLower());

            request.AsMethod(DmtpUtility.Dmtp);

            // 发送请求并处理响应
            using (var responseResult = await this.ProtectedRequestContentAsync(request).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
            {
                var response = responseResult.Response;
                // 如果状态码为101，则切换协议为Dmtp
                if (response.StatusCode == 101)
                {
                    this.SwitchProtocolToDmtp();
                }
                else
                {
                    // 其他状态码视为错误，抛出异常
                    throw new Exception(response.StatusMessage);
                }
            }

            // 与Dmtp服务器进行握手操作，完成连接
            await this.m_dmtpActor.HandshakeAsync(this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyToken, this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).Id, millisecondsTimeout, this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).Metadata, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            // 释放信号量，允许其他连接操作
            this.m_semaphoreForConnect.Release();
        }
    }

    #endregion 连接

    #region 断开

    /// <summary>
    /// 发送<see cref="IDmtpActor"/>关闭消息。
    /// </summary>
    /// <param name="msg">关闭消息的内容</param>
    /// <param name="token"></param>
    /// <returns>异步任务</returns>
    public override async Task<Result> CloseAsync(string msg, CancellationToken token = default)
    {
        // 检查是否已初始化IDmtpActor对象
        if (this.m_dmtpActor != null)
        {
            // 向IDmtpActor对象发送关闭消息
            await this.m_dmtpActor.SendCloseAsync(msg).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            // 关闭IDmtpActor对象
            await this.m_dmtpActor.CloseAsync(msg, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }

        // 调用基类的关闭方法
        return await base.CloseAsync(msg, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion 断开

    #region Override

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        this.m_dmtpActor?.SafeDispose();
        base.Dispose(disposing);
    }

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
    protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        if (this.Protocol == DmtpUtility.DmtpProtocol && e.RequestInfo is DmtpMessage message)
        {
            if (!await this.m_dmtpActor.InputReceivedData(message).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
            {
                if (this.PluginManager.Enable)
                {
                    await this.PluginManager.RaiseAsync(typeof(IDmtpReceivedPlugin), this.Resolver, this, new DmtpMessageEventArgs(message)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
            }
            return;
        }
        await base.OnTcpReceived(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion Override

    #region ResetId

    ///<inheritdoc/>
    public Task ResetIdAsync(string newId)
    {
        return this.m_dmtpActor.ResetIdAsync(newId);
    }

    #endregion ResetId

    private void SwitchProtocolToDmtp()
    {
        this.Protocol = DmtpUtility.DmtpProtocol;
        var adapter = new DmtpAdapter();
        base.SetWarpAdapter(adapter);
        this.SetAdapter(adapter);
        this.m_dmtpActor = new SealedDmtpActor(this.m_allowRoute)
        {
            //OutputSend = this.DmtpActorSend,
            OutputSendAsync = this.DmtpActorSendAsync,
            Routing = this.OnDmtpActorRouting,
            Handshaking = this.OnDmtpActorHandshaking,
            Handshaked = this.OnDmtpActorHandshaked,
            Closing = this.OnDmtpActorClose,
            CreatedChannel = this.OnDmtpActorCreateChannel,
            Logger = this.Logger,
            Client = this,
            FindDmtpActor = this.m_findDmtpActor
        };
    }

    #region 内部委托绑定

    private Task DmtpActorSendAsync(DmtpActor actor, ReadOnlyMemory<byte> memory)
    {
        return base.ProtectedDefaultSendAsync(memory);
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

    #endregion 内部委托绑定

    #region 事件触发

    /// <summary>
    /// 当创建通道时触发的事件处理程序
    /// 此方法用于在创建通道时执行插件或其他逻辑
    /// </summary>
    /// <param name="e">包含创建通道相关事件数据的参数对象</param>
    protected virtual async Task OnCreateChannel(CreateChannelEventArgs e)
    {
        // 如果事件已经被处理，则直接返回
        if (e.Handled)
        {
            return;
        }

        // 异步调用插件管理器，通知所有实现了IDmtpCreatedChannelPlugin接口的插件处理创建通道事件
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

    #endregion 事件触发
}