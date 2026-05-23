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

using TouchSocket.Sockets;

namespace TouchSocket.Http;

/// <summary>
/// HTTP/2 客户端响应，持有从 Transport 直接读取的已解码头部与 body 数据。
/// </summary>
internal sealed class Http2ClientResponse : HttpResponse
{
    private readonly ReadOnlyMemory<byte> m_body;
    private int m_readPosition;

    internal Http2ClientResponse(List<Http2Header> responseHeaders, ReadOnlyMemory<byte> body)
    {
        this.m_body = body;
        this.ParseHttp2ResponseHeaders(responseHeaders);
        this.ContentStatus = ContentCompletionStatus.ContentCompleted;
    }

    /// <inheritdoc/>
    public override bool IsServer => false;

    /// <inheritdoc/>
    public override ValueTask<ReadOnlyMemory<byte>> GetContentAsync(CancellationToken cancellationToken = default)
        => new ValueTask<ReadOnlyMemory<byte>>(this.m_body);

    /// <inheritdoc/>
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (buffer.IsEmpty || this.m_readPosition >= this.m_body.Length)
        {
            return new ValueTask<int>(0);
        }

        var remaining = this.m_body.Slice(this.m_readPosition);
        var len = Math.Min(remaining.Length, buffer.Length);
        remaining.Slice(0, len).CopyTo(buffer);
        this.m_readPosition += len;
        return new ValueTask<int>(len);
    }

    /// <inheritdoc/>
    public override Task AnswerAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("HTTP/2 客户端响应不支持发送数据。");

    /// <inheritdoc/>
    public override Task WriteAsync(ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("HTTP/2 客户端响应不支持写入操作。");

    /// <inheritdoc/>
    public override Task CompleteChunkAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("HTTP/2 客户端响应不支持完成分块操作。");

    /// <inheritdoc/>
    protected override void ReadRequestLine(ReadOnlySpan<byte> responseLineSpan)
    {
        // HTTP/2 无响应行
    }

    private void ParseHttp2ResponseHeaders(List<Http2Header> headers)
    {
        foreach (var h in headers)
        {
            if (h.Name == ":status")
            {
                if (int.TryParse(h.Value, out var code))
                {
                    this.StatusCode = code;
                    this.StatusMessage = GetDefaultStatusMessage(code);
                }
            }
            else if (h.Name.Length > 0 && h.Name[0] != ':')
            {
                this.Headers[h.Name] = h.Value;
            }
        }

        this.ProtocolVersion = "2";
        this.Protocols = Protocol.Http;
    }

    private static string GetDefaultStatusMessage(int code) => code switch
    {
        200 => "OK",
        201 => "Created",
        204 => "No Content",
        301 => "Moved Permanently",
        302 => "Found",
        304 => "Not Modified",
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        500 => "Internal Server Error",
        502 => "Bad Gateway",
        503 => "Service Unavailable",
        _ => string.Empty
    };
}
