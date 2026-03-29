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

using System.Net.WebSockets;
using TouchSocket.Sockets;

namespace TouchSocket.Http.WebSockets;

/// <summary>
/// WebSocket用户终端。
/// </summary>
public abstract class WebSocketClientBase : HttpClientBase, IWebSocket
{
    /// <inheritdoc/>
    public IHttpSession Client => this;

    /// <inheritdoc/>
    public WebSocketCloseStatus CloseStatus => this.WebSocket.CloseStatus;

    /// <inheritdoc/>
    public override bool Online => this.m_webSocket != null && this.m_webSocket.Online;

    /// <inheritdoc/>
    public string Version => this.m_webSocket.Version;

    /// <inheritdoc/>
    public Task<Result> CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken = default)
    {
        return this.m_webSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);
    }

    /// <inheritdoc/>
    public ValueTask<WebSocketReceiveResult> ReadAsync(CancellationToken cancellationToken)
    {
        return this.m_webSocket.ReadAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task SendAsync(WSDataFrame dataFrame, bool endOfMessage = true, CancellationToken cancellationToken = default)
    {
        return this.m_webSocket.SendAsync(dataFrame, endOfMessage, cancellationToken);
    }

    #region Connect

    /// <summary>
    /// 异步建立 WebSocket 连接。
    /// </summary>
    /// <param name="autoReceive">是否自动接收消息。</param>
    /// <param name="cancellationToken">用于取消操作的 <see cref="CancellationToken"/>。</param>
    /// <returns>表示异步操作的 <see cref="Task"/>。</returns>
    protected virtual async Task ProtectedWebSocketConnectAsync(bool autoReceive, CancellationToken cancellationToken)
    {
        if (!base.Online)
        {
            await base.HttpConnectAsync(cancellationToken).ConfigureDefaultAwait();
        }

        this.m_webSocket = new InternalWebSocket(this, this.Transport, autoReceive);
        var option = this.Config.WebSocketOption;

        var request = WSTools.GetWSRequest(this, option.Version, out var base64Key);

        await this.OnWebSocketConnecting(new HttpContextEventArgs(new HttpContext(request, default)))
            .ConfigureDefaultAwait();

        //这里不要释放responseResult，主要是这是http最后一次请求，后续不会再用到http了。
        //如果释放了，会导致响应数据在PrivateOnConnected中失效。
        var responseResult = await this.ProtectedRequestAsync(request, cancellationToken)
            .ConfigureDefaultAwait();
        var response = responseResult.Response;
        if (response.StatusCode != 101)
        {
            throw new WebSocketConnectException($"协议升级失败，信息：{response.StatusMessage}，更多信息请捕获WebSocketConnectException异常，获得HttpContext得知。", new HttpContext(request, response));
        }
        var accept = response.Headers.Get("sec-websocket-accept");
        if (accept.IsEmpty || !accept.Equals(WSTools.CalculateBase64Key(base64Key).Trim(), StringComparison.OrdinalIgnoreCase))
        {
            await base.CloseAsync("WS服务器返回的应答3码不正确", cancellationToken).ConfigureDefaultAwait();
            throw new WebSocketConnectException($"WS服务器返回的应答码不正确，更多信息请捕获WebSocketConnectException异常，获得HttpContext得知。", new HttpContext(request, response));
        }
        this.Protocol = Protocol.WebSocket;
        this.m_webSocket.Online = true;
        if (autoReceive)
        {
            var transport = base.Transport;
            _ = EasyTask.SafeRun(this.WebSocketReceiveLoopAsync, transport);
        }

        await this.PrivateOnConnected(new HttpContextEventArgs(new HttpContext(request, response))).ConfigureDefaultAwait();
    }

#pragma warning disable CS0809
    /// <inheritdoc/>
    [Obsolete("请使用ProtectedWebSocketConnectAsync方法进行连接。", true)]
    protected sealed override Task HttpConnectAsync(CancellationToken cancellationToken)
    {
        throw new NotSupportedException("请使用ProtectedWebSocketConnectAsync方法进行连接。");
    }
#pragma warning restore CS0809

    #endregion Connect

    #region 字段

    private InternalWebSocket m_webSocket;

    #endregion 字段

    #region 事件

    /// <summary>
    /// 当 WebSocket 连接关闭时触发的事件处理程序。
    /// </summary>
    /// <param name="e">包含关闭原因和状态代码的事件参数。</param>
    /// <returns>一个 <see cref="Task"/> 对象，表示异步操作的完成。</returns>
    protected virtual async Task OnWebSocketClosed(ClosedEventArgs e)
    {
        await this.PluginManager.RaiseIWebSocketClosedPluginAsync(this.Resolver, this, e).ConfigureDefaultAwait();
    }

    /// <summary>
    /// 在WebSocket关闭时触发的事件处理程序。
    /// </summary>
    /// <param name="e">包含关闭原因和状态码的事件参数。</param>
    /// <returns>A <see cref="Task"/> 表示事件处理的异步操作。</returns>
    protected virtual async Task OnWebSocketClosing(ClosingEventArgs e)
    {
        await this.PluginManager.RaiseIWebSocketClosingPluginAsync(this.Resolver, this, e).ConfigureDefaultAwait();
    }

    /// <summary>
    /// 表示完成握手后。
    /// </summary>
    /// <param name="e">包含HTTP上下文信息的参数对象。</param>
    /// <returns>一个表示任务已完成的Task对象。</returns>
    protected virtual async Task OnWebSocketConnected(HttpContextEventArgs e)
    {
        await this.PluginManager.RaiseIWebSocketConnectedPluginAsync(this.Resolver, this, e).ConfigureDefaultAwait();
    }

    /// <summary>
    /// 表示在即将握手连接时。
    /// </summary>
    /// <param name="e">包含HTTP上下文信息的参数对象</param>
    /// <returns>一个表示异步操作完成的任务</returns>
    protected virtual async Task OnWebSocketConnecting(HttpContextEventArgs e)
    {
        await this.PluginManager.RaiseIWebSocketConnectingPluginAsync(this.Resolver, this, e).ConfigureDefaultAwait();
    }

    private Task PrivateOnConnected(HttpContextEventArgs e)
    {
        return this.OnWebSocketConnected(e);
    }

    private async Task PrivateWebSocketClosed(ClosedEventArgs e)
    {
        this.m_webSocket.Online = false;
        await this.OnWebSocketClosed(e).ConfigureDefaultAwait();
    }

    #endregion 事件

    /// <summary>
    /// 当收到WS数据时。
    /// </summary>
    /// <param name="e">包含接收数据的事件参数</param>
    /// <returns>一个Task对象，表示异步操作</returns>
    protected virtual async Task OnWebSocketReceived(WSDataFrameEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(IWebSocketReceivedPlugin), this.Resolver, this, e).ConfigureDefaultAwait();
    }

    #region Properties

    /// <summary>
    /// 实际通讯的WebSocket。
    /// </summary>
    protected IWebSocket WebSocket => this.m_webSocket;

    #endregion Properties

    private async Task WebSocketReceiveLoopAsync(ITransport transport)
    {
        var cancellationToken = transport.ClosedToken;
        await transport.ReadLocker.WaitAsync(cancellationToken).ConfigureDefaultAwait();
        try
        {
            var eventArgs = new WSDataFrameEventArgs();
            while (!this.DisposedValue && !cancellationToken.IsCancellationRequested)
            {
                using var result = await this.m_webSocket.InternalReadAsync(cancellationToken).ConfigureDefaultAwait();
                if (result.IsCompleted)
                {
                    break;
                }
                eventArgs.Reset(result.DataFrame);
                try
                {
                    await this.OnWebSocketReceived(eventArgs).ConfigureDefaultAwait();
                }
                catch (Exception ex)
                {
                    this.Logger?.Exception(this, ex);
                }
            }
        }
        catch
        {
        }
        finally
        {
            transport.ReadLocker.Release();
        }
    }

    #region Override

    /// <inheritdoc/>
    protected override async Task OnTcpClosed(ClosedEventArgs e)
    {
        await this.PrivateWebSocketClosed(e).ConfigureDefaultAwait();
        await base.OnTcpClosed(e).ConfigureDefaultAwait();
    }

    #endregion Override
}
