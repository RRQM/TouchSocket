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
using System.Collections.Generic;
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
    protected async ValueTask<HttpResponseResult> ProtectedRequestAsync(HttpRequest request, CancellationToken token = default)
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
        var writer = new PipeBytesWriter(this.Transport.Output);
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

        await transport.Output.WriteAsync(memory, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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