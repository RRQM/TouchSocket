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
/// HTTP/2 服务器端请求，持有预解码头部和 body，实现统一的 <see cref="HttpRequest"/> 接口。
/// </summary>
internal sealed class Http2HttpRequest : HttpRequest
{
    private readonly ReadOnlyMemory<byte> m_body;
    private int m_readPosition;

    internal Http2HttpRequest(HttpSessionClient httpSessionClient, int streamId, List<Http2Header> headers, ReadOnlyMemory<byte> body) : base(httpSessionClient)
    {
        this.m_body = body;
        this.ParseHttp2RequestHeaders(headers);
        this.ContentStatus = ContentCompletionStatus.ContentCompleted;
    }

    private void ParseHttp2RequestHeaders(List<Http2Header> headers)
    {
        foreach (var h in headers)
        {
            switch (h.Name)
            {
                case ":method":
                    this.Method = new HttpMethod(h.Value);
                    break;
                case ":path":
                    this.URL = h.Value;
                    break;
                case ":scheme":
                    break;
                case ":authority":
                    this.Headers[HttpHeaders.Host] = h.Value;
                    break;
                default:
                    if (h.Name.Length > 0 && h.Name[0] != ':')
                    {
                        this.Headers[h.Name] = h.Value;
                    }
                    break;
            }
        }

        this.ProtocolVersion = "2";
        this.Protocols = Protocol.Http;
    }

    /// <inheritdoc/>
    public override ValueTask<ReadOnlyMemory<byte>> GetContentAsync(CancellationToken cancellationToken = default)
        => new ValueTask<ReadOnlyMemory<byte>>(this.m_body);

    /// <inheritdoc/>
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var remaining = this.m_body.Slice(this.m_readPosition);
        if (remaining.IsEmpty)
        {
            return new ValueTask<int>(0);
        }

        var len = Math.Min(remaining.Length, buffer.Length);
        remaining.Slice(0, len).CopyTo(buffer);
        this.m_readPosition += len;
        return new ValueTask<int>(len);
    }

    /// <inheritdoc/>
    protected override void ReadRequestLine(ReadOnlySpan<byte> requestLineSpan)
    {
        // HTTP/2 无请求行，头部已在构造函数中解析
    }
}
