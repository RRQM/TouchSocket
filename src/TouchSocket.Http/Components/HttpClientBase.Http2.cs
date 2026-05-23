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
/// Http客户端基类 - HTTP/2 客户端处理
/// </summary>
public abstract partial class HttpClientBase
{
    private static readonly byte[] s_http2ClientPreface = Encoding.ASCII.GetBytes("PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n");
    private bool m_http2Active;
    private Http2FrameWriter m_http2FrameWriter = null!;
    private HpackDecoder m_http2Decoder = null!;
    private HpackEncoder m_http2Encoder = null!;
    private int m_http2NextStreamId;

    private async Task Http2ActivateAsync(ITransport transport, CancellationToken ct)
    {
        this.m_http2FrameWriter = new Http2FrameWriter(transport.Writer);
        this.m_http2Decoder = new HpackDecoder();
        this.m_http2Encoder = new HpackEncoder();
        this.m_http2NextStreamId = 1;
        this.m_http2Active = true;
        await this.Http2InitialHandshakeAsync(transport.Reader, ct).ConfigureDefaultAwait();
    }

    private void Http2Deactivate()
    {
        this.m_http2Active = false;
        this.m_http2FrameWriter = null!;
        this.m_http2Decoder = null!;
        this.m_http2Encoder = null!;
    }

    private async ValueTask<HttpResponseResult> Http2ProtectedRequestAsync(HttpRequest request, CancellationToken ct)
    {
        var body = ReadOnlyMemory<byte>.Empty;
        if (request.Content != null)
        {
            request.Content.InternalBuildingHeader(request.Headers);
            body = await Http2CollectBodyAsync(request.Content, ct).ConfigureDefaultAwait();
        }

        var streamId = this.Http2AllocateStreamId();
        var headers = Http2BuildRequestHeaders(request, body);
        using var encodeBuf = new SegmentedBytesWriter(512);
        this.m_http2Encoder.Encode(headers, encodeBuf);
        await this.m_http2FrameWriter.WriteHeadersAsync(streamId, encodeBuf.Sequence, body.IsEmpty, ct).ConfigureDefaultAwait();
        if (!body.IsEmpty)
        {
            await this.m_http2FrameWriter.WriteDataAsync(streamId, body, true, ct).ConfigureDefaultAwait();
        }

        var responseData = await this.Http2ReadResponseAsync(streamId, ct).ConfigureDefaultAwait();
        return new HttpResponseResult(new Http2ClientResponse(responseData.Headers, responseData.Body));
    }

    private static List<Http2Header> Http2BuildRequestHeaders(HttpRequest request, ReadOnlyMemory<byte> body)
    {
        var headers = new List<Http2Header>
        {
            new Http2Header(":method", request.Method.ToString()),
            new Http2Header(":path", request.URL ?? "/"),
            new Http2Header(":scheme", "http"),
        };

        var host = request.Headers.Get(HttpHeaders.Host);
        if (!host.IsEmpty)
        {
            headers.Add(new Http2Header(":authority", (string)host));
        }

        if (!body.IsEmpty)
        {
            headers.Add(new Http2Header("content-length", body.Length.ToString()));
        }

        foreach (var h in request.Headers)
        {
            var key = h.Key;
            if (string.Equals(key, HttpHeaders.Host, StringComparison.OrdinalIgnoreCase)) continue;
            if (key.Length > 0 && key[0] == ':') continue;
            headers.Add(new Http2Header(key.ToLowerInvariant(), (string)h.Value));
        }

        return headers;
    }

    private static async Task<ReadOnlyMemory<byte>> Http2CollectBodyAsync(HttpContent content, CancellationToken ct)
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

    private int Http2AllocateStreamId() => Interlocked.Add(ref this.m_http2NextStreamId, 2) - 2;

    private async Task Http2InitialHandshakeAsync(PipeReader reader, CancellationToken ct)
    {
        var buf = new PipeBytesWriter(this.Transport.Writer);
        buf.Write(s_http2ClientPreface);
        await buf.FlushAsync(ct).ConfigureDefaultAwait();

        await this.m_http2FrameWriter.WriteSettingsAsync(new Http2PeerSettings(), isAck: false, ct).ConfigureDefaultAwait();

        while (true)
        {
            using var frame = await Http2ReadNextFrameAsync(reader, ct).ConfigureDefaultAwait();
            if (frame.Header.Type == Http2FrameType.Settings && !frame.Header.HasFlag(Http2Flags.EndStreamOrAck))
            {
                await this.m_http2FrameWriter.WriteSettingsAsync(new Http2PeerSettings(), isAck: true, ct).ConfigureDefaultAwait();
                break;
            }
        }
    }

    private async Task<Http2ResponseData> Http2ReadResponseAsync(int streamId, CancellationToken ct)
    {
        var reader = this.Transport.Reader;
        var responseHeaders = new List<Http2Header>(16);
        var bodyStream = new MemoryStream();
        var headersReceived = false;

        while (true)
        {
            using var frame = await Http2ReadNextFrameAsync(reader, ct).ConfigureDefaultAwait();

            switch (frame.Header.Type)
            {
                case Http2FrameType.Headers when frame.Header.StreamId == streamId:
                    var fragment = Http2ExtractHeaderFragment(frame.Header, frame.Payload);
                    this.m_http2Decoder.Decode(fragment, responseHeaders);
                    headersReceived = true;
                    if (frame.Header.HasFlag(Http2Flags.EndStreamOrAck))
                        goto done;
                    break;

                case Http2FrameType.Data when frame.Header.StreamId == streamId && headersReceived:
                    if (frame.Payload.Length > 0)
                    {
                        await this.m_http2FrameWriter.WriteWindowUpdateAsync(0, (uint)frame.Payload.Length, ct).ConfigureDefaultAwait();
                        await this.m_http2FrameWriter.WriteWindowUpdateAsync(streamId, (uint)frame.Payload.Length, ct).ConfigureDefaultAwait();
                        var rented = ArrayPool<byte>.Shared.Rent(frame.Payload.Length);
                        try
                        {
                            frame.Payload.Span.CopyTo(rented.AsSpan());
                            bodyStream.Write(rented, 0, frame.Payload.Length);
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(rented);
                        }
                    }
                    if (frame.Header.HasFlag(Http2Flags.EndStreamOrAck))
                        goto done;
                    break;

                case Http2FrameType.Settings when !frame.Header.HasFlag(Http2Flags.EndStreamOrAck):
                    await this.m_http2FrameWriter.WriteSettingsAsync(new Http2PeerSettings(), isAck: true, ct).ConfigureDefaultAwait();
                    break;

                case Http2FrameType.WindowUpdate:
                    break;

                case Http2FrameType.Ping when !frame.Header.HasFlag(Http2Flags.EndStreamOrAck):
                    await this.m_http2FrameWriter.WritePingAsync(frame.Payload, isAck: true, ct).ConfigureDefaultAwait();
                    break;

                case Http2FrameType.RstStream when frame.Header.StreamId == streamId:
                    ThrowHelper.ThrowException($"HTTP/2 流 {streamId} 被服务器重置");
                    break;

                case Http2FrameType.GoAway:
                    ThrowHelper.ThrowException("HTTP/2 服务器发送了 GOAWAY，连接即将关闭");
                    break;
            }
        }

        done:
        var bodyLen = (int)bodyStream.Length;
        var body = bodyLen > 0
            ? (ReadOnlyMemory<byte>)bodyStream.GetBuffer().AsMemory(0, bodyLen)
            : ReadOnlyMemory<byte>.Empty;
        return new Http2ResponseData(responseHeaders, body);
    }

    private static ReadOnlySpan<byte> Http2ExtractHeaderFragment(Http2FrameHeader header, ReadOnlyMemory<byte> payload)
    {
        var span = payload.Span;
        var offset = 0;
        var padLength = 0;
        if (header.HasFlag(Http2Flags.Padded))
            padLength = span[offset++];
        if (header.HasFlag(Http2Flags.Priority))
            offset += 5;
        return span.Slice(offset, span.Length - offset - padLength);
    }

    private static async Task<Http2Frame> Http2ReadNextFrameAsync(PipeReader reader, CancellationToken ct)
    {
        while (true)
        {
            var result = await reader.ReadAsync(ct).ConfigureDefaultAwait();
            var buffer = result.Buffer;

            if (result.IsCanceled || (result.IsCompleted && buffer.Length == 0))
            {
                throw new OperationCanceledException("HTTP/2 连接已关闭");
            }

            if (buffer.Length >= Http2FrameHeader.Size)
            {
                Span<byte> headerBytes = stackalloc byte[Http2FrameHeader.Size];
                buffer.Slice(0, Http2FrameHeader.Size).CopyTo(headerBytes);

                if (Http2FrameHeader.TryRead(headerBytes, out var frameHeader))
                {
                    var totalSize = Http2FrameHeader.Size + frameHeader.PayloadLength;
                    if (buffer.Length >= totalSize)
                    {
                        if (frameHeader.PayloadLength == 0)
                        {
                            reader.AdvanceTo(buffer.GetPosition(totalSize));
                            return new Http2Frame(frameHeader);
                        }
                        var owner = MemoryPool<byte>.Shared.Rent(frameHeader.PayloadLength);
                        buffer.Slice(Http2FrameHeader.Size, frameHeader.PayloadLength).CopyTo(owner.Memory.Span);
                        reader.AdvanceTo(buffer.GetPosition(totalSize));
                        return new Http2Frame(frameHeader, owner, frameHeader.PayloadLength);
                    }
                }
            }

            reader.AdvanceTo(buffer.Start, buffer.End);
        }
    }
}