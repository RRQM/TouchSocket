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
    /// <summary>
    /// WebSocket用户终端
    /// </summary>
    public WebSocketClientBase()
    {
        this.m_webSocket = new InternalWebSocket(this);
    }

    /// <inheritdoc/>
    public bool AllowAsyncRead { get => this.m_webSocket.AllowAsyncRead; set => this.m_webSocket.AllowAsyncRead = value; }

    /// <inheritdoc/>
    public IHttpSession Client => this;

    /// <inheritdoc/>
    public WebSocketCloseStatus CloseStatus => this.WebSocket.CloseStatus;

    /// <inheritdoc/>
    public override bool Online => this.m_webSocket.Online;

    /// <inheritdoc/>
    public string Version => this.m_webSocket.Version;

    /// <inheritdoc/>
    public Task<Result> CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken = default)
    {
        return this.m_webSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<Result> PingAsync(CancellationToken cancellationToken = default)
    {
        return this.m_webSocket.PingAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<Result> PongAsync(CancellationToken cancellationToken = default)
    {
        return this.m_webSocket.PongAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public ValueTask<IWebSocketReceiveResult> ReadAsync(CancellationToken cancellationToken)
    {
        return this.m_webSocket.ReadAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task SendAsync(WSDataFrame dataFrame, bool endOfMessage = true, CancellationToken cancellationToken = default)
    {
        return this.m_webSocket.SendAsync(dataFrame, endOfMessage, cancellationToken);
    }

    /// <inheritdoc/>
    public Task SendAsync(string text, bool endOfMessage = true, CancellationToken cancellationToken = default)
    {
        return this.m_webSocket.SendAsync(text, endOfMessage, cancellationToken);
    }

    /// <inheritdoc/>
    public Task SendAsync(ReadOnlyMemory<byte> memory, bool endOfMessage = true, CancellationToken cancellationToken = default)
    {
        return this.m_webSocket.SendAsync(memory, endOfMessage, cancellationToken);
    }

    #region Connect

    /// <summary>
    /// 异步建立 WebSocket 连接。
    /// </summary>
    /// <param name="cancellationToken">用于取消操作的 <see cref="CancellationToken"/>。</param>
    /// <returns>表示异步操作的 <see cref="Task"/>。</returns>
    protected virtual async Task ProtectedWebSocketConnectAsync(CancellationToken cancellationToken)
    {
        if (!base.Online)
        {
            await this.HttpConnectAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }

        var option = this.Config.WebSocketOption;

        var request = WSTools.GetWSRequest(this, option.Version, out var base64Key);

        await this.OnWebSocketConnecting(new HttpContextEventArgs(new HttpContext(request, default)))
            .ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        //这里不要释放responseResult，主要是这是http最后一次请求，后续不会再用到http了。
        //如果释放了，会导致响应数据在PrivateOnConnected中失效。
        var responseResult = await this.ProtectedRequestAsync(request, cancellationToken)
            .ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        var response = responseResult.Response;
        if (response.StatusCode != 101)
        {
            throw new WebSocketConnectException($"协议升级失败，信息：{response.StatusMessage}，更多信息请捕获WebSocketConnectException异常，获得HttpContext得知。", new HttpContext(request, response));
        }
        var accept = response.Headers.Get("sec-websocket-accept");
        if (accept.IsEmpty || !accept.Equals(WSTools.CalculateBase64Key(base64Key).Trim(), StringComparison.OrdinalIgnoreCase))
        {
            await base.CloseAsync("WS服务器返回的应答3码不正确", cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            throw new WebSocketConnectException($"WS服务器返回的应答码不正确，更多信息请捕获WebSocketConnectException异常，获得HttpContext得知。", new HttpContext(request, response));
        }
        this.InitWebSocket();

        _ = EasyTask.SafeRun(this.PrivateOnConnected, new HttpContextEventArgs(new HttpContext(request, response)));
    }

    #endregion Connect

    #region 字段

    private readonly InternalWebSocket m_webSocket;
    private WebSocketDataHandlingAdapter m_dataHandlingAdapter;
    #endregion 字段

    #region 事件

    /// <summary>
    /// 当 WebSocket 连接关闭时触发的事件处理程序。
    /// </summary>
    /// <param name="e">包含关闭原因和状态代码的事件参数。</param>
    /// <returns>一个 <see cref="Task"/> 对象，表示异步操作的完成。</returns>
    protected virtual async Task OnWebSocketClosed(ClosedEventArgs e)
    {
        await this.PluginManager.RaiseIWebSocketClosedPluginAsync(this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 在WebSocket关闭时触发的事件处理程序。
    /// </summary>
    /// <param name="e">包含关闭原因和状态码的事件参数。</param>
    /// <returns>A <see cref="Task"/> 表示事件处理的异步操作。</returns>
    protected virtual async Task OnWebSocketClosing(ClosingEventArgs e)
    {
        await this.PluginManager.RaiseIWebSocketClosingPluginAsync(this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 表示完成握手后。
    /// </summary>
    /// <param name="e">包含HTTP上下文信息的参数对象。</param>
    /// <returns>一个表示任务已完成的Task对象。</returns>
    protected virtual async Task OnWebSocketConnected(HttpContextEventArgs e)
    {
        await this.PluginManager.RaiseIWebSocketConnectedPluginAsync(this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 表示在即将握手连接时。
    /// </summary>
    /// <param name="e">包含HTTP上下文信息的参数对象</param>
    /// <returns>一个表示异步操作完成的任务</returns>
    protected virtual async Task OnWebSocketConnecting(HttpContextEventArgs e)
    {
        await this.PluginManager.RaiseIWebSocketConnectingPluginAsync(this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private Task PrivateOnConnected(HttpContextEventArgs e)
    {
        return this.OnWebSocketConnected(e);
    }

    private async Task PrivateWebSocketClosed(ClosedEventArgs e)
    {
        this.m_webSocket.Online = false;
        if (this.m_webSocket.AllowAsyncRead)
        {
            this.m_webSocket.Complete(e.Message);
        }
        await this.OnWebSocketClosed(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private Task PrivateWebSocketClosing(ClosingEventArgs e)
    {
        return this.OnWebSocketClosing(e);
    }

    private async Task PrivateWebSocketReceived(WSDataFrame dataFrame)
    {
        if (dataFrame.IsClose)
        {
            var payloadMemory = dataFrame.PayloadData;
            var payloadSpan = payloadMemory.Span;
            if (payloadSpan.Length >= 2)
            {
                var closeStatus = (WebSocketCloseStatus)payloadSpan.ReadValue<ushort>(EndianType.Big);
                this.m_webSocket.CloseStatus = closeStatus;
            }

            var msg = payloadSpan.ToString(System.Text.Encoding.UTF8);

            await this.PrivateWebSocketClosing(new ClosingEventArgs(msg)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            await this.m_webSocket.CloseAsync(msg ?? "Auto closed successful").ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }
        if (this.m_webSocket.AllowAsyncRead)
        {
            await this.m_webSocket.InputReceiveAsync(dataFrame, CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }

        await this.OnWebSocketReceived(new WSDataFrameEventArgs(dataFrame)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion 事件

    /// <summary>
    /// 当收到WS数据时。
    /// </summary>
    /// <param name="e">包含接收数据的事件参数</param>
    /// <returns>一个Task对象，表示异步操作</returns>
    protected virtual async Task OnWebSocketReceived(WSDataFrameEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(IWebSocketReceivedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private void InitWebSocket()
    {
        var webSocketDataHandlingAdapter = new WebSocketDataHandlingAdapter();
        this.SetAdapter(webSocketDataHandlingAdapter);
        this.m_dataHandlingAdapter = webSocketDataHandlingAdapter;

        this.Protocol = Protocol.WebSocket;
        this.m_webSocket.Online = true;

        var transport = base.Transport;
        _ = EasyTask.SafeRun(this.WebSocketReceiveLoopAsync, transport);
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
        using var reader = new PooledBytesReader();
        await transport.ReadLocker.WaitAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        try
        {
            while (true)
            {
                if (this.DisposedValue || cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                var result = await transport.Reader.ReadAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                if (result.Buffer.Length == 0)
                {
                    break;
                }

                try
                {
                    reader.Reset(result.Buffer);

                    if (!await this.OnTcpReceiving(reader).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                    {
                        await this.m_dataHandlingAdapter.ReceivedInputAsync(reader).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }
                    var position = result.Buffer.GetPosition(reader.BytesRead);
                    transport.Reader.AdvanceTo(position, result.Buffer.End);

                    if (result.IsCompleted || result.IsCanceled)
                    {
                        return;
                    }
                    reader.Clear();
                }
                catch (Exception ex)
                {
                    this.Logger?.Exception(this, ex);
                    await transport.CloseAsync(ex.Message).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
            }
        }
        catch (Exception ex)
        {
            // 如果发生异常，记录日志并退出接收循环
            this.Logger?.Debug(this, ex);
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
        await this.PrivateWebSocketClosed(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        await base.OnTcpClosed(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        if (this.m_webSocket.Online)
        {
            var dataFrame = (WSDataFrame)e.RequestInfo;

            await this.PrivateWebSocketReceived(dataFrame).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else
        {
            if (e.RequestInfo is HttpResponse)
            {
                await base.OnTcpReceived(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }
        await base.OnTcpReceived(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion Override
}