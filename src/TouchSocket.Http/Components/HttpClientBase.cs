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

using System.Net;
using TouchSocket.Sockets;

namespace TouchSocket.Http;

/// <summary>
/// Http客户端基类
/// </summary>
public abstract class HttpClientBase : TcpClientBase, IHttpSession
{
    private readonly SemaphoreSlim m_connectSemaphore = new SemaphoreSlim(1, 1);

    private readonly ClientHttpResponse m_httpClientResponse;

    /// <summary>
    /// 初始化 <see cref="HttpClientBase"/> 类的新实例。
    /// </summary>
    public HttpClientBase()
    {
        this.m_httpClientResponse = new ClientHttpResponse(this);
    }

    /// <summary>
    /// 获取内部传输层对象，用于HTTP响应内容读取
    /// </summary>
    internal ITransport InternalTransport => this.Transport;

    /// <summary>
    /// 异步连接HTTP服务器，支持代理连接
    /// </summary>
    /// <param name="cancellationToken">用于取消操作的CancellationToken</param>
    /// <returns>返回一个任务，表示异步连接操作</returns>
    protected virtual async Task HttpConnectAsync(CancellationToken cancellationToken)
    {
        // 重置响应对象状态。
        // 修复在重连时，响应对象仍然保持上次状态的问题。
        // issue:https://github.com/RRQM/TouchSocket/issues/110
        this.m_httpClientResponse.Reset();
        await this.m_connectSemaphore.WaitAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        try
        {
            await this.PrivateHttpConnectAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            this.m_connectSemaphore.Release();
        }
    }

    private async Task PrivateHttpConnectAsync(CancellationToken cancellationToken)
    {
        this.ThrowIfDisposed();

        this.ThrowIfConfigIsNull();

        var proxy = this.Config.Proxy;
        if (proxy != null)
        {
            await this.ConnectThroughProxyAsync(proxy, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else
        {
            await base.TcpConnectAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    /// <summary>
    /// 此方法会一直抛出异常，请使用<see cref="HttpConnectAsync(CancellationToken)"/>进行连接
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [Obsolete("请使用HttpConnectAsync进行连接", true)]
    protected sealed override Task TcpConnectAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException("请使用HttpConnectAsync进行连接");
        //return base.TcpConnectAsync(cancellationToken);
    }

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            this.m_connectSemaphore?.Dispose();
        }
        base.SafetyDispose(disposing);
    }

    private async Task ConnectThroughProxyAsync(IWebProxy proxy, CancellationToken cancellationToken)
    {
        var targetHost = this.RemoteIPHost;
        var proxyUri = proxy.GetProxy(targetHost);

        if (proxyUri == null || proxyUri.Equals(targetHost))
        {
            // 代理返回原始URI或null，直接连接
            await base.TcpConnectAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }

        // 先连接到代理服务器
        var originalRemoteIPHost = this.RemoteIPHost;
        try
        {
            // 临时设置代理服务器作为连接目标
            var proxyIPHost = new IPHost(proxyUri.ToString());
            var config = this.Config;

            config.SetRemoteIPHost(proxyIPHost);

            // 连接到代理服务器
            await base.TcpConnectAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            // 发送 CONNECT 请求建立隧道
            await this.EstablishProxyTunnelAsync(proxy, targetHost, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            await this.TryAuthenticateAsync(originalRemoteIPHost).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            // 恢复原始的远程主机配置
            this.Config.SetRemoteIPHost(originalRemoteIPHost);
        }
    }

    private async Task TryAuthenticateAsync(IPHost iPHost)
    {
        if (!this.Config.TryGetValue(TouchSocketConfigExtension.ClientSslOptionProperty, out var sslOption))
        {
            if (!iPHost.IsSsl)
            {
                return;
            }

            sslOption = new ClientSslOption()
            {
                TargetHost = iPHost.Host
            };
        }
        await this.AuthenticateAsync(sslOption).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task EstablishProxyTunnelAsync(IWebProxy proxy, IPHost targetHost, CancellationToken cancellationToken)
    {
        // 构建 CONNECT 请求
        var connectRequest = new HttpRequest();
        connectRequest.Method = HttpMethod.Connect;
        connectRequest.URL = $"{targetHost.Host}:{targetHost.Port}";
        connectRequest.Headers.Add(HttpHeaders.Host, $"{targetHost.Host}:{targetHost.Port}");

        // 处理代理身份验证
        var credentials = proxy.Credentials;
        if (credentials != null)
        {
            var networkCredential = credentials.GetCredential(targetHost, "Basic");
            if (networkCredential != null && !string.IsNullOrEmpty(networkCredential.UserName))
            {
                var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{networkCredential.UserName}:{networkCredential.Password}"));
                connectRequest.Headers.Add("Proxy-Authorization", $"Basic {auth}");
            }
        }

        // 发送 CONNECT 请求
        // 接收代理响应
        using (var responseResult = await this.ProtectedRequestAsync(connectRequest, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
        {
            var response = responseResult.Response;
            // 检查响应状态
            if (response.StatusCode == 200)
            {
                // 隧道建立成功，现在可以发送HTTPS流量
                return;
            }
            else if (response.StatusCode == 407)
            {
                // 代理身份验证失败
                throw new ProxyAuthenticationException("代理服务器要求身份验证");
            }
            else
            {
                // 其他错误
                throw new ProxyConnectionException($"代理连接失败: {response.StatusCode} {response.StatusMessage}");
            }
        }
    }

    #region Request

    /// <summary>
    /// 最大重定向次数
    /// </summary>
    public const int MaximumRedirectCount = 50;

    /// <summary>
    /// 异步发送Http请求,并仅等待响应头
    /// </summary>
    /// <param name="request">要发送的HttpRequest对象</param>
    /// <param name="cancellationToken">用于取消操作的CancellationToken</param>
    /// <returns>返回HttpResponseResult对象,包含响应结果和释放锁的方法</returns>
    /// <exception cref="TimeoutException">当操作超时时抛出</exception>
    /// <exception cref="OperationCanceledException">当操作被取消时抛出</exception>
    /// <exception cref="TooManyRedirectsException">当重定向次数超过最大限制时抛出</exception>
    /// <exception cref="Exception">当发生其他异常时抛出</exception>
    protected async ValueTask<HttpResponseResult> ProtectedRequestAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        return await this.ProtectedRequestAsync(request, true, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 异步发送Http请求,并仅等待响应头
    /// </summary>
    /// <param name="request">要发送的HttpRequest对象</param>
    /// <param name="allowRedirect">是否允许自动重定向</param>
    /// <param name="cancellationToken">用于取消操作的CancellationToken</param>
    /// <returns>返回HttpResponseResult对象,包含响应结果和释放锁的方法</returns>
    /// <exception cref="TimeoutException">当操作超时时抛出</exception>
    /// <exception cref="OperationCanceledException">当操作被取消时抛出</exception>
    /// <exception cref="TooManyRedirectsException">当重定向次数超过最大限制时抛出</exception>
    /// <exception cref="Exception">当发生其他异常时抛出</exception>
    protected async ValueTask<HttpResponseResult> ProtectedRequestAsync(HttpRequest request, bool allowRedirect, CancellationToken cancellationToken)
    {
        await this.m_connectSemaphore.WaitAsync(cancellationToken);

        try
        {
            var redirectCount = 0;
            var currentRequest = request;
            var originalHost = this.RemoteIPHost;

            while (true)
            {
                currentRequest.Headers.TryAdd(HttpHeaders.Host, this.RemoteIPHost.Authority);

                await this.BuildAndSendAsync(currentRequest, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                var response = await this.ReadHttpResponseAsync(cancellationToken)
                    .ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                if (!allowRedirect || !this.IsRedirectStatusCode(response.StatusCode))
                {
                    return new HttpResponseResult(response);
                }

                var location = response.Headers.Get(HttpHeaders.Location);
                if (location.IsEmpty)
                {
                    return new HttpResponseResult(response);
                }

                redirectCount++;
                if (redirectCount > MaximumRedirectCount)
                {
                    throw new TooManyRedirectsException($"重定向次数超过最大限制 {MaximumRedirectCount}");
                }

                var redirectUri = this.BuildRedirectUri(location.First);

                await this.HandleRedirectConnectionAsync(redirectUri, originalHost, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                currentRequest = this.CreateRedirectRequest(currentRequest, response.StatusCode, redirectUri);

                response.Reset();
            }
        }
        finally
        {
            this.m_connectSemaphore.Release();
        }
    }

    private bool IsRedirectStatusCode(int statusCode)
    {
        return statusCode == 301 || statusCode == 302 || statusCode == 303 ||
               statusCode == 307 || statusCode == 308;
    }

    private Uri BuildRedirectUri(string location)
    {
        if (Uri.TryCreate(location, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri;
        }

        var baseUri = new Uri($"{this.RemoteIPHost.Scheme}://{this.RemoteIPHost.Authority}");
        return new Uri(baseUri, location);
    }

    private HttpRequest CreateRedirectRequest(HttpRequest originalRequest, int statusCode, Uri redirectUri)
    {
        var newRequest = new HttpRequest();
        newRequest.URL = redirectUri.PathAndQuery;
        newRequest.ProtocolVersion = originalRequest.ProtocolVersion;

        foreach (var header in originalRequest.Headers)
        {
            if (!string.Equals(header.Key, HttpHeaders.Host, StringComparison.OrdinalIgnoreCase))
            {
                newRequest.Headers.Add(header.Key, header.Value);
            }
        }

        if (statusCode == 303)
        {
            newRequest.Method = HttpMethod.Get;
            newRequest.Content = null;
        }
        else if (statusCode == 301 || statusCode == 302)
        {
            if (originalRequest.Method == HttpMethod.Post)
            {
                newRequest.Method = HttpMethod.Get;
                newRequest.Content = null;
            }
            else
            {
                newRequest.Method = originalRequest.Method;
                newRequest.Content = originalRequest.Content;
            }
        }
        else
        {
            newRequest.Method = originalRequest.Method;
            newRequest.Content = originalRequest.Content;
        }

        return newRequest;
    }

    private async Task HandleRedirectConnectionAsync(Uri redirectUri, IPHost originalHost, CancellationToken cancellationToken)
    {
        var newHost = redirectUri.Host;
        var newPort = redirectUri.Port;
        var newScheme = redirectUri.Scheme;

        if (!string.Equals(newHost, originalHost.Host, StringComparison.OrdinalIgnoreCase) ||
            newPort != originalHost.Port ||
            !string.Equals(newScheme, originalHost.Scheme, StringComparison.OrdinalIgnoreCase))
        {
            var newIPHost = new IPHost($"{newScheme}://{newHost}:{newPort}");

            await this.CloseAsync("重定向到不同主机",cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            this.Config.SetRemoteIPHost(newIPHost);

            await this.PrivateHttpConnectAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    private async Task BuildAndSendAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var content = request.Content;
        var writer = new PipeBytesWriter(this.Transport.Writer);
        if (content == null)
        {
            request.BuildHeader(ref writer);
            await writer.FlushAsync(cancellationToken)
                .ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }

        content.InternalBuildingHeader(request.Headers);
        request.BuildHeader(ref writer);

        var result = content.InternalBuildingContent(ref writer);

        await writer.FlushAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        if (!result)
        {
            await content.InternalWriteContent(this.Transport.Writer, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    /// <summary>
    /// 直接从Transport读取HTTP响应
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>HTTP响应对象</returns>
    private async Task<ClientHttpResponse> ReadHttpResponseAsync(CancellationToken cancellationToken)
    {
        await this.m_httpClientResponse.ReadHeader(cancellationToken);
        return this.m_httpClientResponse;
    }

    #endregion Request

    #region override

    /// <inheritdoc/>
    protected override async Task OnTcpClosed(ClosedEventArgs e)
    {
        await base.OnTcpClosed(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpConnecting(ConnectingEventArgs e)
    {
        this.Protocol = Protocol.Http;

        await base.OnTcpConnecting(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    /// <remarks>Http客户端中，不再允许子类在<see cref="ReceiveLoopAsync"/>中自动接收数据</remarks>
    protected sealed override async Task ReceiveLoopAsync(ITransport transport)
    {
        if (transport.ClosedToken.IsCancellationRequested)
        {
            return;
        }
        try
        {
            //重写接收循环，阻止基类自动接收。
            //http的接收由适配器自行处理。
            await Task.Delay(-1, transport.ClosedToken);
        }
        catch
        {
            //忽略异常
        }
    }

    #endregion override
}