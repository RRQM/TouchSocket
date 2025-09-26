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

    /// <summary>
    /// 构造函数
    /// </summary>
    protected HttpSessionClient()
    {
        this.Protocol = Protocol.Http;
    }

    #region Send

    internal Task InternalSendAsync(in ReadOnlyMemory<byte> memory, CancellationToken cancellationToken)
    {
        return this.ProtectedSendAsync(memory, cancellationToken);
    }

    #endregion Send

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
        this.SetAdapter(new HttpServerDataHandlingAdapter());
        return base.OnTcpConnecting(e);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        if (e.RequestInfo is HttpRequest request)
        {
            this.m_httpContext ??= new HttpContext(request, new ServerHttpResponse(request, this));
            await this.OnReceivedHttpRequest(this.m_httpContext).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.m_httpContext.Response.Reset();
        }
        else if (this.m_webSocket != null && e.RequestInfo is WSDataFrame dataFrame)
        {
            e.Handled = true;
            await this.PrivateWebSocketReceived(dataFrame).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }
    }
}