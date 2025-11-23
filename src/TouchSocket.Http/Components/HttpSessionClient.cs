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

using TouchSocket.Http.WebSockets;
using TouchSocket.Sockets;

namespace TouchSocket.Http;

/// <summary>
/// http辅助类
/// </summary>
public abstract partial class HttpSessionClient : TcpSessionClientBase, IHttpSessionClient
{
    private HttpContext m_httpContext;

    private ServerHttpResponse m_serverHttpResponse;
    private readonly HttpServerDataHandlingAdapter m_httpAdapter;
    private WebSocketDataHandlingAdapter m_webSocketAdapter;
    /// <summary>
    /// 构造函数
    /// </summary>
    protected HttpSessionClient()
    {
        this.Protocol = Protocol.Http;
        this.m_httpAdapter = new HttpServerDataHandlingAdapter(this.OnReceivingHttpRequest);
        this.m_webSocketAdapter = new WebSocketDataHandlingAdapter();
    }


    internal ITransport InternalTransport => this.Transport;

    /// <summary>
    /// 当收到到Http请求时。覆盖父类方法将不会触发插件。
    /// </summary>
    protected virtual async Task OnReceivedHttpRequest(HttpContext httpContext)
    {
        var e = new HttpContextEventArgs(httpContext);

        await this.PluginManager.RaiseIHttpPluginAsync(this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosed(ClosedEventArgs e)
    {
        if (this.m_webSocket != null)
        {
            await this.PrivateWebSocketClosed(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        await base.OnTcpClosed(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected override Task OnTcpConnecting(ConnectingEventArgs e)
    {
        this.SetAdapter(this.m_httpAdapter);
        return base.OnTcpConnecting(e);
    }

    private async Task OnReceivingHttpRequest(ServerHttpRequest request)
    {
        if (this.m_httpContext == null)
        {
            this.m_serverHttpResponse = new ServerHttpResponse(request, this);
            this.m_httpContext = new HttpContext(request, this.m_serverHttpResponse);
        }

        await this.OnReceivedHttpRequest(this.m_httpContext).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        this.m_serverHttpResponse.Reset();
    }

    /// <inheritdoc/>
    protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        if (this.m_webSocket != null && e.RequestInfo is WSDataFrame dataFrame)
        {
            e.Handled = true;
            await this.PrivateWebSocketReceived(dataFrame).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }
    }

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        if (this.DisposedValue)
        {
            return;
        }

        if (disposing && this.m_webSocket != null)
        {
            this.m_webSocket.Dispose();
        }

        base.SafetyDispose(disposing);
    }
}