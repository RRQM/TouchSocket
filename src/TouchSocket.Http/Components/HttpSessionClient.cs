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

using System.IO.Pipelines;
using TouchSocket.Core;
using TouchSocket.Http.WebSockets;
using TouchSocket.Sockets;

namespace TouchSocket.Http;

/// <summary>
/// http辅助类
/// </summary>
public abstract partial class HttpSessionClient : TcpSessionClientBase, IHttpSessionClient
{
    private readonly HttpContext m_httpContext;
    private readonly HttpContextEventArgs m_httpContextEventArgs;
    private readonly ServerHttpRequest m_requestRoot;
    private readonly ServerHttpResponse m_serverHttpResponse;
    private readonly WSDataFrameEventArgs m_wsDataFrameEventArgs;
    private bool m_isCustomProtocol;

    /// <summary>
    /// 构造函数
    /// </summary>
    protected HttpSessionClient()
    {
        this.Protocol = Protocol.Http;
        this.m_requestRoot = new ServerHttpRequest(this);
        this.m_serverHttpResponse = new ServerHttpResponse(this.m_requestRoot, this);
        this.m_httpContext = new HttpContext(this.m_requestRoot, this.m_serverHttpResponse);
        this.m_httpContextEventArgs = new HttpContextEventArgs(this.m_httpContext);
        this.m_wsDataFrameEventArgs = new WSDataFrameEventArgs();
    }

    internal ITransport InternalTransport => this.Transport;

    /// <summary>
    /// 当收到到Http请求时。覆盖父类方法将不会触发插件。
    /// </summary>
    protected virtual async Task OnReceivedHttpRequest(HttpContext httpContext)
    {
        this.m_httpContextEventArgs.Reset(this.m_httpContext);
        await this.PluginManager.RaiseIHttpPluginAsync(this.Resolver, this, this.m_httpContextEventArgs).ConfigureDefaultAwait();
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosed(ClosedEventArgs e)
    {
        if (this.m_webSocket != null)
        {
            await this.PrivateWebSocketClosed(e).ConfigureDefaultAwait();
        }
        await base.OnTcpClosed(e).ConfigureDefaultAwait();
    }

    /// <inheritdoc/>
    /// <remarks>此处直接从 <see cref="ITransport"/> 的 <see cref="PipeReader"/> 读取数据，不再使用适配器机制。</remarks>
    protected override sealed async Task ReceiveLoopAsync(ITransport transport)
    {
        if (!await transport.ReadLocker.WaitAsync(0).ConfigureDefaultAwait())
        {
            return;
        }
        try
        {
            await this.HttpPipelineLoopAsync(transport.Reader, transport.ClosedToken).ConfigureDefaultAwait();
        }
        finally
        {
            transport.ReadLocker.Release();
        }

        if (this.m_isCustomProtocol && !transport.ClosedToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(-1, transport.ClosedToken).ConfigureDefaultAwait();
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    private async Task HttpPipelineLoopAsync(PipeReader reader, CancellationToken closedToken)
    {
        while (!closedToken.IsCancellationRequested && !this.DisposedValue)
        {
            // 阶段一：解析 HTTP 请求头部
            this.m_requestRoot.Reset();
            while (true)
            {
                var readResult = await reader.ReadAsync(CancellationToken.None).ConfigureDefaultAwait();
                var buffer = readResult.Buffer;

                if (readResult.IsCanceled || (readResult.IsCompleted && buffer.Length == 0))
                {
                    return;
                }

                var bytesReader = new BytesReader(buffer);
                bool parsed;
                long headerBytesRead;
                try
                {
                    parsed = this.m_requestRoot.ParsingHeader(ref bytesReader);
                    headerBytesRead = bytesReader.BytesRead;
                }
                finally
                {
                    bytesReader.Dispose();
                }

                if (parsed)
                {
                    var contentLength = this.m_requestRoot.ContentLength;
                    var isChunked = contentLength == 0 && this.m_requestRoot.IsChunk;

                    if (!isChunked && contentLength > 0)
                    {
                        // 尝试内联读取：如果 body 已全部在当前缓冲区，直接推进头部让 PipeReader 流式提供 body
                        var bodySlice = buffer.Slice(buffer.GetPosition(headerBytesRead));
                        if (bodySlice.Length >= contentLength)
                        {
                            reader.AdvanceTo(buffer.GetPosition(headerBytesRead));
                            this.m_requestRoot.InternalSetForPipeReading(reader, contentLength, false);
                            break;
                        }
                    }

                    // 仅推进头部，body 由 PipeReader 流式读取
                    reader.AdvanceTo(buffer.GetPosition(headerBytesRead));

                    this.m_requestRoot.InternalSetForPipeReading(reader, contentLength, isChunked);
                    break;
                }
                else
                {
                    if (readResult.IsCompleted)
                    {
                        return;
                    }
                    reader.AdvanceTo(buffer.Start, buffer.End);
                }
            }

            // 阶段二：调度 handler
            try
            {
                await this.OnReceivedHttpRequest(this.m_httpContext).ConfigureDefaultAwait();
            }
            catch (Exception ex)
            {
                this.Logger?.Exception(this, ex);
            }
            finally
            {
                this.m_serverHttpResponse.Reset();
            }

            // 阶段三：丢弃未读取的 body
            try
            {
                await this.m_requestRoot.InternalDrainBodyAsync(closedToken).ConfigureDefaultAwait();
            }
            catch
            {
                return;
            }

            if (this.m_isCustomProtocol)
            {
                return;
            }
        }
    }
}