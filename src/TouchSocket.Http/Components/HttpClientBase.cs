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
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Http;

/// <summary>
/// Http客户端基类
/// </summary>
public abstract class HttpClientBase : TcpClientBase, IHttpSession
{
    #region 字段

    private readonly SemaphoreSlim m_semaphoreForRequest = new SemaphoreSlim(1, 1);
    private HttpClientDataHandlingAdapter m_dataHandlingAdapter;
    private TaskCompletionSource<HttpResponse> m_responseTaskSource;

    #endregion 字段

    internal Task InternalSendAsync(ReadOnlyMemory<byte> memory, CancellationToken token)
    {
        return this.ProtectedSendAsync(memory, token);
    }

    /// <summary>
    /// 异步连接HTTP服务器，支持代理连接
    /// </summary>
    /// <param name="token">用于取消操作的CancellationToken</param>
    /// <returns>返回一个任务，表示异步连接操作</returns>
    protected async Task HttpConnectAsync(CancellationToken token)
    {
        this.ThrowIfDisposed();

        this.ThrowIfConfigIsNull();

        var proxy = this.Config.Proxy;
        if (proxy != null)
        {
            await this.ConnectThroughProxyAsync(proxy, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else
        {
            await this.TcpConnectAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            this.m_semaphoreForRequest.Dispose();
            this.m_responseTaskSource.TrySetException(new ObjectDisposedException(nameof(HttpClientBase)));
        }
        base.SafetyDispose(disposing);
    }

    /// <summary>
    /// 设置用于处理单流数据的转换适配器
    /// </summary>
    /// <param name="adapter">要设置的SingleStreamDataHandlingAdapter实例</param>
    protected void SetWarpAdapter(SingleStreamDataHandlingAdapter adapter)
    {
        // 将提供的适配器设置为当前数据处理适配器的WarpAdapter
        this.m_dataHandlingAdapter.WarpAdapter = adapter;
    }

    private async Task ConnectThroughProxyAsync(IWebProxy proxy, CancellationToken token)
    {
        var targetHost = this.RemoteIPHost;
        var proxyUri = proxy.GetProxy(targetHost);
        //proxyUri = new Uri("http://127.0.0.1:8089");
        if (proxyUri == null || proxyUri.Equals(new Uri($"http://{targetHost.Host}:{targetHost.Port}")))
        {
            // 代理返回原始URI或null，直接连接

            await this.TcpConnectAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }

        // 先连接到代理服务器
        var originalRemoteIPHost = this.RemoteIPHost;
        try
        {
            // 临时设置代理服务器作为连接目标
            var proxyHost = $"{proxyUri.Host}:{proxyUri.Port}";
            var proxyIPHost = new IPHost(proxyHost);
            var config = this.Config;
            config.SetRemoteIPHost(proxyIPHost);

            // 连接到代理服务器
            await this.TcpConnectAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            // 发送 CONNECT 请求建立隧道
            await this.EstablishProxyTunnelAsync(proxy, targetHost, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            var sslOption = new ClientSslOption()
            {
                SslProtocols = System.Security.Authentication.SslProtocols.None,
                TargetHost = targetHost.Host
            };
            await base.AuthenticateAsync(sslOption).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            // 恢复原始的远程主机配置
            this.Config.SetRemoteIPHost(originalRemoteIPHost);
        }
    }

    private async Task EstablishProxyTunnelAsync(IWebProxy proxy, IPHost targetHost, CancellationToken token)
    {
        // 构建 CONNECT 请求
        var connectRequest = new HttpRequest();
        connectRequest.Method = new HttpMethod("CONNECT");
        connectRequest.URL = $"{targetHost.Host}:{targetHost.Port}";
        //connectRequest.ContentLength = 0;
        connectRequest.Headers.Add(HttpHeaders.Host, $"{targetHost.Host}:{targetHost.Port}");
        connectRequest.Headers.Add(HttpHeaders.UserAgent, "GitHub-File-Downloader/1.0");

        // 处理代理身份验证
        var credentials = proxy.Credentials;
        if (credentials != null)
        {
            var networkCredential = credentials.GetCredential(new Uri($"http://{targetHost.Host}:{targetHost.Port}"), "Basic");
            if (networkCredential != null && !string.IsNullOrEmpty(networkCredential.UserName))
            {
                var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{networkCredential.UserName}:{networkCredential.Password}"));
                connectRequest.Headers.Add("Proxy-Authorization", $"Basic {auth}");
            }
        }

        // 发送 CONNECT 请求
        // 接收代理响应
        using (var responseResult = await this.ProtectedRequestAsync(connectRequest, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
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
    /// 异步发送Http请求，并仅等待响应头
    /// </summary>
    /// <param name="request">要发送的HttpRequest对象</param>
    /// <param name="token">用于取消操作的CancellationToken</param>
    /// <returns>返回HttpResponseResult对象，包含响应结果和释放锁的方法</returns>
    /// <exception cref="TimeoutException">当操作超时时抛出</exception>
    /// <exception cref="OperationCanceledException">当操作被取消时抛出</exception>
    /// <exception cref="Exception">当发生其他异常时抛出</exception>
    protected async ValueTask<HttpResponseResult> ProtectedRequestAsync(HttpRequest request, CancellationToken token)
    {
        await this.m_semaphoreForRequest.WaitAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        try
        {
            this.m_responseTaskSource = new TaskCompletionSource<HttpResponse>();
            request.Headers.TryAdd(HttpHeaders.Host, this.RemoteIPHost.Authority);
            await this.BuildAndSendAsync(request, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            // 等待响应状态，超时设定
            var response = await this.m_responseTaskSource.Task.WithCancellation(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            return new HttpResponseResult(response, this.ReleaseLock);
        }
        catch
        {
            this.m_semaphoreForRequest.Release();
            throw;
        }
    }

    private async Task BuildAndSendAsync(HttpRequest request, CancellationToken token)
    {
        var content = request.Content;
        var writer = new PipeBytesWriter(this.Transport.Writer);
        if (content == null)
        {
            request.BuildHeader(ref writer);
            await writer.FlushAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }

        content.InternalTryComputeLength(out var contentLength);
        content.InternalBuildingHeader(request.Headers);
        request.BuildHeader(ref writer);

        var result = content.InternalBuildingContent(ref writer);

        await writer.FlushAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        if (!result)
        {
            await content.InternalWriteContent(this.UnsafeSendAsync, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    private void ReleaseLock()
    {
        this.m_semaphoreForRequest.Release();
        this.m_dataHandlingAdapter.SetCompleteLock();
    }

    private async Task UnsafeSendAsync(ReadOnlyMemory<byte> memory, CancellationToken token)
    {
        this.ThrowIfDisposed();
        this.ThrowIfClientNotConnected();
        var transport = this.Transport;

        await transport.Writer.WriteAsync(memory, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion Request

    #region override

    /// <inheritdoc/>
    protected override async Task OnTcpClosed(ClosedEventArgs e)
    {
        this.m_responseTaskSource.TrySetException(new ClientNotConnectedException());
        await base.OnTcpClosed(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpConnecting(ConnectingEventArgs e)
    {
        this.Protocol = Protocol.Http;
        this.m_dataHandlingAdapter = new HttpClientDataHandlingAdapter();
        this.SetAdapter(this.m_dataHandlingAdapter);
        await base.OnTcpConnecting(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected override Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        if (e.RequestInfo is HttpResponse response)
        {
            var taskSource = this.m_responseTaskSource;
            taskSource?.TrySetResult(response);
        }

        return EasyTask.CompletedTask;
    }

    #endregion override
}
