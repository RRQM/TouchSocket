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

using System.Buffers;
using System.IO.Pipelines;
using TouchSocket.Sockets;

namespace TouchSocket.Http;

/// <summary>
/// HTTP/2 服务器端响应，通过 <see cref="HttpSessionClient"/> 发送 HTTP/2 帧，实现与 <see cref="HttpResponse"/> 统一的接口。
/// </summary>
internal sealed class Http2HttpResponse : HttpResponse
{
    private readonly int m_streamId;
    private readonly HttpSessionClient m_session;
    private readonly CancellationToken m_closedToken;
    private bool m_headersSent;

    internal Http2HttpResponse(int streamId, HttpSessionClient session, CancellationToken closedToken)
    {
        this.m_streamId = streamId;
        this.m_session = session;
        this.m_closedToken = closedToken;
        this.StatusCode = 200;
        this.StatusMessage = "OK";
        this.ProtocolVersion = "2";
        this.Protocols = Protocol.Http;
    }

    /// <inheritdoc/>
    public override bool IsServer => true;

    /// <inheritdoc/>
    public override async Task AnswerAsync(CancellationToken cancellationToken = default)
    {
        if (this.Responsed)
        {
            return;
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(this.m_closedToken, cancellationToken);
        var ct = cts.Token;

        var content = this.Content;
        var body = ReadOnlyMemory<byte>.Empty;

        if (content != null)
        {
            content.InternalBuildingHeader(this.Headers);
            body = await CollectBodyAsync(content, ct).ConfigureDefaultAwait();
        }

        var headers = this.BuildHttp2ResponseHeaders();
        await this.m_session.Http2SendResponseAsync(this.m_streamId, headers, body, ct).ConfigureDefaultAwait();
        this.Responsed = true;
    }

    /// <inheritdoc/>
    public override async Task WriteAsync(ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
    {
        if (this.Responsed)
        {
            return;
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(this.m_closedToken, cancellationToken);
        var ct = cts.Token;

        if (!this.m_headersSent)
        {
            var headers = this.BuildHttp2ResponseHeaders();
            await this.m_session.Http2SendResponseHeadersOnlyAsync(this.m_streamId, headers, false, ct).ConfigureDefaultAwait();
            this.m_headersSent = true;
        }

        if (!memory.IsEmpty)
        {
            await this.m_session.Http2SendStreamDataAsync(this.m_streamId, memory, false, ct).ConfigureDefaultAwait();
        }
    }

    /// <inheritdoc/>
    public override async Task CompleteChunkAsync(CancellationToken cancellationToken = default)
    {
        if (this.Responsed)
        {
            return;
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(this.m_closedToken, cancellationToken);
        var ct = cts.Token;

        if (!this.m_headersSent)
        {
            var headers = this.BuildHttp2ResponseHeaders();
            await this.m_session.Http2SendResponseHeadersOnlyAsync(this.m_streamId, headers, true, ct).ConfigureDefaultAwait();
        }
        else
        {
            await this.m_session.Http2SendStreamDataAsync(this.m_streamId, ReadOnlyMemory<byte>.Empty, true, ct).ConfigureDefaultAwait();
        }

        this.Responsed = true;
    }

    /// <inheritdoc/>
    public override ValueTask<ReadOnlyMemory<byte>> GetContentAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("HTTP/2 服务器端响应不支持读取内容。");

    /// <inheritdoc/>
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("HTTP/2 服务器端响应不支持读取操作。");

    /// <inheritdoc/>
    protected override void ReadRequestLine(ReadOnlySpan<byte> responseLineSpan)
    {
        // HTTP/2 无响应行
    }

    private List<Http2Header> BuildHttp2ResponseHeaders()
    {
        var headers = new List<Http2Header>
        {
            new Http2Header(":status", this.StatusCode.ToString())
        };

        foreach (var h in this.Headers)
        {
            var key = h.Key;
            if (key.Length > 0 && key[0] != ':')
            {
                headers.Add(new Http2Header(key.ToLowerInvariant(), h.Value));
            }
        }

        return headers;
    }

    private static async Task<ReadOnlyMemory<byte>> CollectBodyAsync(HttpContent content, CancellationToken ct)
    {
        var buf = new SegmentedBytesWriter();
        try
        {
            if (content.InternalBuildingContent(ref buf))
            {
                var bytes = new byte[(int)buf.WrittenCount];
                var offset = 0;
                foreach (var seg in buf.Sequence)
                {
                    seg.Span.CopyTo(bytes.AsSpan(offset));
                    offset += seg.Length;
                }
                return bytes;
            }
        }
        finally
        {
            buf.Dispose();
        }

        var pipe = new Pipe();
        await content.InternalWriteContent(pipe.Writer, ct).ConfigureDefaultAwait();
        pipe.Writer.Complete();

        var ms = new MemoryStream();
        while (true)
        {
            var result = await pipe.Reader.ReadAsync(ct).ConfigureDefaultAwait();
            var buffer = result.Buffer;
            foreach (var seg in buffer)
            {
                var rented = ArrayPool<byte>.Shared.Rent(seg.Length);
                try
                {
                    seg.Span.CopyTo(rented.AsSpan());
                    ms.Write(rented, 0, seg.Length);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(rented);
                }
            }
            pipe.Reader.AdvanceTo(buffer.End);
            if (result.IsCompleted)
            {
                break;
            }
        }

        return ms.ToArray();
    }
}
