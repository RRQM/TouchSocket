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
using System.Buffers.Binary;
using System.IO.Pipelines;

namespace TouchSocket.Http;

/// <summary>
/// http辅助类 - HTTP/2 服务器端处理
/// </summary>
public abstract partial class HttpSessionClient
{
    // RFC 7540 §3.5 客户端连接前言（24 字节）
    private static readonly byte[] s_http2Preface = Encoding.ASCII.GetBytes(
        "PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n");

    #region HTTP/2 连接状态字段

    private Http2FrameWriter m_http2ServerWriter = null!;
    private HpackDecoder m_http2ServerDecoder = null!;
    private HpackEncoder m_http2ServerEncoder = null!;
    private readonly Http2PeerSettings m_http2LocalSettings = new Http2PeerSettings();
    private readonly Http2PeerSettings m_http2RemoteSettings = new Http2PeerSettings();
    private Dictionary<int, ServerStreamState> m_http2Streams = null!;
    private int m_http2LastStreamId;
    private bool m_http2GoAwaySent;
    private CancellationToken m_http2ClosedToken;
    private PipeReader m_http2ServerReader = null!;

    #endregion

    /// <summary>
    /// 检测是否为 HTTP/2 连接，若是则处理整个 HTTP/2 连接并返回 <see langword="true"/>
    /// </summary>
    private async Task<bool> TryHandleHttp2ConnectionAsync(PipeReader reader, CancellationToken closedToken)
    {
        var preface = s_http2Preface;
        var prefaceLen = preface.Length;

        // 非消耗性地读取足够字节进行判断
        while (true)
        {
            var result = await reader.ReadAsync(closedToken).ConfigureDefaultAwait();
            var buffer = result.Buffer;

            if (result.IsCanceled || (result.IsCompleted && buffer.Length == 0))
            {
                reader.AdvanceTo(buffer.Start);
                return false;
            }

            if (buffer.Length < prefaceLen)
            {
                if (result.IsCompleted)
                {
                    // 数据不足且不再有新数据，按 HTTP/1.1 处理
                    reader.AdvanceTo(buffer.Start);
                    return false;
                }
                // 等待更多数据
                reader.AdvanceTo(buffer.Start, buffer.End);
                continue;
            }

            // 检查前 preface 字节是否匹配
            var head = buffer.Slice(0, prefaceLen);
            Span<byte> headBytes = stackalloc byte[prefaceLen];
            var copyOffset = 0;
            foreach (var seg in head)
            {
                seg.Span.CopyTo(headBytes.Slice(copyOffset));
                copyOffset += seg.Length;
            }

            var isHttp2 = headBytes.SequenceEqual(preface.AsSpan());

            if (!isHttp2)
            {
                // 不消耗数据，回退给 HTTP/1.1 处理
                reader.AdvanceTo(buffer.Start);
                return false;
            }

            // 消耗前言字节，进入 HTTP/2 模式
            reader.AdvanceTo(buffer.GetPosition(prefaceLen));
            break;
        }

        // 进入 HTTP/2 连接处理
        await this.RunHttp2ServerConnectionAsync(reader, this.InternalTransport.Writer, closedToken).ConfigureDefaultAwait();
        return true;
    }

    private async Task RunHttp2ServerConnectionAsync(PipeReader reader, PipeWriter writer, CancellationToken closedToken)
    {
        this.m_http2ServerWriter = new Http2FrameWriter(writer);
        this.m_http2ServerDecoder = new HpackDecoder(this.m_http2LocalSettings.HeaderTableSize);
        this.m_http2ServerEncoder = new HpackEncoder(this.m_http2RemoteSettings.HeaderTableSize);
        this.m_http2Streams = new Dictionary<int, ServerStreamState>();
        this.m_http2LastStreamId = 0;
        this.m_http2GoAwaySent = false;
        this.m_http2ClosedToken = closedToken;
        this.m_http2ServerReader = reader;

        try
        {
            await this.m_http2ServerWriter.WriteSettingsAsync(this.m_http2LocalSettings, isAck: false, closedToken).ConfigureDefaultAwait();

            while (!closedToken.IsCancellationRequested)
            {
                using var frame = await this.Http2ServerReadNextFrameAsync(closedToken).ConfigureDefaultAwait();
                await this.Http2ProcessFrameAsync(frame.Header, frame.Payload).ConfigureDefaultAwait();
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Http2ConnectionException ex)
        {
            await this.Http2ServerCloseAsync(ex.ErrorCode).ConfigureDefaultAwait();
        }
        catch (Exception ex)
        {
            this.Logger?.Exception(this, ex);
            await this.Http2ServerCloseAsync(Http2ErrorCode.InternalError).ConfigureDefaultAwait();
        }
        finally
        {
            await this.Http2ServerCloseAsync().ConfigureDefaultAwait();
        }
    }

    private async Task HandleHttp2RequestAsync(int streamId, List<Http2Header> headers, ReadOnlyMemory<byte> body, CancellationToken closedToken)
    {
        try
        {
            var request = new Http2HttpRequest(this, streamId, headers, body);
            var response = new Http2HttpResponse(streamId, this, closedToken);
            var httpContext = new HttpContext(request, response);
            var eventArgs = new HttpContextEventArgs(httpContext);

            try
            {
                await this.PluginManager.RaiseIHttpPluginAsync(this.Resolver, this, eventArgs).ConfigureDefaultAwait();

                if (!response.Responsed)
                {
                    await this.Http2SendResponseAsync(streamId,
                        new[] { new Http2Header(":status", "200") },
                        ReadOnlyMemory<byte>.Empty,
                        closedToken).ConfigureDefaultAwait();
                }
            }
            catch (Exception ex)
            {
                this.Logger?.Exception(this, ex);
                if (!response.Responsed)
                {
                    try
                    {
                        await this.Http2SendResponseAsync(streamId,
                            new[] { new Http2Header(":status", "500") },
                            ReadOnlyMemory<byte>.Empty,
                            CancellationToken.None).ConfigureDefaultAwait();
                    }
                    catch
                    {
                    }
                }
            }
        }
        catch (Exception ex)
        {
            this.Logger?.Exception(this, ex);
        }
    }

    #region HTTP/2 发送方法（供 Http2HttpResponse 调用）

    /// <summary>
    /// 发送完整响应（HEADERS + DATA）。
    /// </summary>
    internal async Task Http2SendResponseAsync(int streamId, IList<Http2Header> headers, ReadOnlyMemory<byte> body, CancellationToken ct)
    {
        using var encodeBuf = new SegmentedBytesWriter(512);
        this.m_http2ServerEncoder.Encode(headers, encodeBuf);
        var endStream = body.IsEmpty;
        await this.m_http2ServerWriter.WriteHeadersAsync(streamId, encodeBuf.Sequence, endStream, ct).ConfigureDefaultAwait();
        if (!endStream)
        {
            await this.m_http2ServerWriter.WriteDataAsync(streamId, body, true, ct).ConfigureDefaultAwait();
        }
    }

    /// <summary>
    /// 仅发送 HEADERS 帧（流式响应第一帧）。
    /// </summary>
    internal async Task Http2SendResponseHeadersOnlyAsync(int streamId, IList<Http2Header> headers, bool endStream, CancellationToken ct)
    {
        using var encodeBuf = new SegmentedBytesWriter(512);
        this.m_http2ServerEncoder.Encode(headers, encodeBuf);
        await this.m_http2ServerWriter.WriteHeadersAsync(streamId, encodeBuf.Sequence, endStream, ct).ConfigureDefaultAwait();
    }

    /// <summary>
    /// 发送单个 DATA 帧（流式响应数据块）。
    /// </summary>
    internal ValueTask Http2SendStreamDataAsync(int streamId, ReadOnlyMemory<byte> data, bool endStream, CancellationToken ct)
        => this.m_http2ServerWriter.WriteDataAsync(streamId, data, endStream, ct);

    #endregion

    private async Task Http2ServerCloseAsync(Http2ErrorCode errorCode = Http2ErrorCode.NoError)
    {
        if (this.m_http2GoAwaySent)
        {
            return;
        }

        this.m_http2GoAwaySent = true;
        try
        {
            await this.m_http2ServerWriter.WriteGoAwayAsync(this.m_http2LastStreamId, errorCode, ReadOnlyMemory<byte>.Empty, CancellationToken.None).ConfigureDefaultAwait();
        }
        catch
        {
        }
    }

    #region HTTP/2 帧处理

    private async Task Http2ProcessFrameAsync(Http2FrameHeader header, ReadOnlyMemory<byte> payload)
    {
        if (header.PayloadLength > this.m_http2LocalSettings.MaxFrameSize)
        {
            throw new Http2ConnectionException(Http2ErrorCode.FrameSizeError,
                $"帧大小 {header.PayloadLength} 超过上限 {this.m_http2LocalSettings.MaxFrameSize}");
        }

        switch (header.Type)
        {
            case Http2FrameType.Data:
                await this.Http2ProcessDataFrameAsync(header, payload).ConfigureDefaultAwait();
                break;
            case Http2FrameType.Headers:
                this.Http2ProcessHeadersFrame(header, payload);
                break;
            case Http2FrameType.Continuation:
                this.Http2ProcessContinuationFrame(header, payload);
                break;
            case Http2FrameType.Settings:
                await this.Http2ProcessSettingsFrameAsync(header, payload).ConfigureDefaultAwait();
                break;
            case Http2FrameType.Ping:
                await this.Http2ProcessPingFrameAsync(header, payload).ConfigureDefaultAwait();
                break;
            case Http2FrameType.RstStream:
                this.Http2ProcessRstStreamFrame(header);
                break;
            case Http2FrameType.GoAway:
                throw new OperationCanceledException("收到 GOAWAY，连接关闭");
            case Http2FrameType.WindowUpdate:
            case Http2FrameType.Priority:
            default:
                break;
        }
    }

    private async Task Http2ProcessDataFrameAsync(Http2FrameHeader header, ReadOnlyMemory<byte> payload)
    {
        if (header.StreamId == 0)
        {
            throw new Http2ConnectionException(Http2ErrorCode.ProtocolError, "DATA 帧不能在流 0 上发送");
        }

        if (!this.m_http2Streams!.TryGetValue(header.StreamId, out var state))
        {
            await this.m_http2ServerWriter.WriteRstStreamAsync(header.StreamId, Http2ErrorCode.StreamClosed, this.m_http2ClosedToken).ConfigureDefaultAwait();
            return;
        }

        var data = payload;
        if (header.HasFlag(Http2Flags.Padded))
        {
            var padLen = payload.Span[0];
            data = payload.Slice(1, payload.Length - 1 - padLen);
        }

        if (data.Length > 0)
        {
            await this.m_http2ServerWriter.WriteWindowUpdateAsync(0, (uint)data.Length, this.m_http2ClosedToken).ConfigureDefaultAwait();
            await this.m_http2ServerWriter.WriteWindowUpdateAsync(header.StreamId, (uint)data.Length, this.m_http2ClosedToken).ConfigureDefaultAwait();
            var rented = ArrayPool<byte>.Shared.Rent(data.Length);
            try
            {
                data.Span.CopyTo(rented.AsSpan());
                state.BodyBuffer.Write(rented, 0, data.Length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }

        if (header.HasFlag(Http2Flags.EndStreamOrAck))
        {
            state.RemoteHalfClosed = true;
            this.Http2TryDispatch(header.StreamId, state);
        }
    }

    private void Http2ProcessHeadersFrame(Http2FrameHeader header, ReadOnlyMemory<byte> payload)
    {
        if (header.StreamId == 0)
        {
            throw new Http2ConnectionException(Http2ErrorCode.ProtocolError, "HEADERS 帧不能在流 0 上发送");
        }

        if (!this.m_http2Streams!.TryGetValue(header.StreamId, out var state))
        {
            state = new ServerStreamState();
            this.m_http2Streams[header.StreamId] = state;
            this.m_http2LastStreamId = header.StreamId;
        }

        var fragment = Http2ExtractServerHeaderFragment(header, payload);
        WriteToStream(state.HeaderBlockBuffer, fragment.Span);
        state.ExpectingContinuation = !header.HasFlag(Http2Flags.EndHeaders);

        if (header.HasFlag(Http2Flags.EndHeaders))
        {
            var bufLen = (int)state.HeaderBlockBuffer.Length;
            this.m_http2ServerDecoder.Decode(state.HeaderBlockBuffer.GetBuffer().AsSpan(0, bufLen), state.Headers);
            state.HeaderBlockBuffer.SetLength(0);
            state.HeadersComplete = true;

            if (header.HasFlag(Http2Flags.EndStreamOrAck))
            {
                state.RemoteHalfClosed = true;
                this.Http2TryDispatch(header.StreamId, state);
            }
        }
    }

    private void Http2ProcessContinuationFrame(Http2FrameHeader header, ReadOnlyMemory<byte> payload)
    {
        if (header.StreamId == 0)
        {
            throw new Http2ConnectionException(Http2ErrorCode.ProtocolError, "CONTINUATION 帧不能在流 0 上发送");
        }

        if (!this.m_http2Streams!.TryGetValue(header.StreamId, out var state) || !state.ExpectingContinuation)
        {
            throw new Http2ConnectionException(Http2ErrorCode.ProtocolError, "未预期的 CONTINUATION 帧");
        }

        WriteToStream(state.HeaderBlockBuffer, payload.Span);
        state.ExpectingContinuation = !header.HasFlag(Http2Flags.EndHeaders);

        if (header.HasFlag(Http2Flags.EndHeaders))
        {
            var bufLen = (int)state.HeaderBlockBuffer.Length;
            this.m_http2ServerDecoder.Decode(state.HeaderBlockBuffer.GetBuffer().AsSpan(0, bufLen), state.Headers);
            state.HeaderBlockBuffer.SetLength(0);
            state.HeadersComplete = true;

            if (state.RemoteHalfClosed)
            {
                this.Http2TryDispatch(header.StreamId, state);
            }
        }
    }

    private async Task Http2ProcessSettingsFrameAsync(Http2FrameHeader header, ReadOnlyMemory<byte> payload)
    {
        if (header.StreamId != 0)
        {
            throw new Http2ConnectionException(Http2ErrorCode.ProtocolError, "SETTINGS 帧必须在流 0 上发送");
        }

        if (header.HasFlag(Http2Flags.EndStreamOrAck))
        {
            return;
        }

        if (payload.Length % 6 != 0)
        {
            throw new Http2ConnectionException(Http2ErrorCode.FrameSizeError, "SETTINGS 帧负载大小不是 6 的倍数");
        }

        var span = payload.Span;
        for (var i = 0; i < span.Length; i += 6)
        {
            var paramId = (Http2SettingsParameter)BinaryPrimitives.ReadUInt16BigEndian(span.Slice(i));
            var value = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(i + 2));
            this.m_http2RemoteSettings.ApplySetting(paramId, value);
        }

        this.m_http2ServerEncoder!.UpdateMaxDynamicTableSize(this.m_http2RemoteSettings.HeaderTableSize);
        await this.m_http2ServerWriter.WriteSettingsAsync(this.m_http2LocalSettings, isAck: true, this.m_http2ClosedToken).ConfigureDefaultAwait();
    }

    private async Task Http2ProcessPingFrameAsync(Http2FrameHeader header, ReadOnlyMemory<byte> payload)
    {
        if (header.StreamId != 0)
        {
            throw new Http2ConnectionException(Http2ErrorCode.ProtocolError, "PING 帧必须在流 0 上发送");
        }

        if (payload.Length != 8)
        {
            throw new Http2ConnectionException(Http2ErrorCode.FrameSizeError, "PING 帧负载必须为 8 字节");
        }

        if (!header.HasFlag(Http2Flags.EndStreamOrAck))
        {
            await this.m_http2ServerWriter.WritePingAsync(payload, isAck: true, this.m_http2ClosedToken).ConfigureDefaultAwait();
        }
    }

    private void Http2ProcessRstStreamFrame(Http2FrameHeader header)
    {
        this.m_http2Streams!.Remove(header.StreamId);
    }

    private void Http2TryDispatch(int streamId, ServerStreamState state)
    {
        if (!state.HeadersComplete || !state.RemoteHalfClosed)
        {
            return;
        }

        var bodyLen = (int)state.BodyBuffer.Length;
        var body = bodyLen > 0
            ? (ReadOnlyMemory<byte>)state.BodyBuffer.GetBuffer().AsMemory(0, bodyLen)
            : ReadOnlyMemory<byte>.Empty;

        var headers = state.Headers;
        var closedToken = this.m_http2ClosedToken;
        _ = Task.Run(() => this.HandleHttp2RequestAsync(streamId, headers, body, closedToken));
        this.m_http2Streams.Remove(streamId);
    }

    private static ReadOnlyMemory<byte> Http2ExtractServerHeaderFragment(Http2FrameHeader header, ReadOnlyMemory<byte> payload)
    {
        var offset = 0;
        var padLength = 0;
        if (header.HasFlag(Http2Flags.Padded))
        {
            padLength = payload.Span[offset++];
        }

        if (header.HasFlag(Http2Flags.Priority))
        {
            offset += 5;
        }

        return payload.Slice(offset, payload.Length - offset - padLength);
    }

    private async Task<Http2Frame> Http2ServerReadNextFrameAsync(CancellationToken ct)
    {
        var reader = this.m_http2ServerReader;
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

    #endregion

    private sealed class ServerStreamState
    {
        public readonly MemoryStream HeaderBlockBuffer = new MemoryStream();
        public bool ExpectingContinuation;
        public readonly List<Http2Header> Headers = new List<Http2Header>(16);
        public readonly MemoryStream BodyBuffer = new MemoryStream();
        public bool HeadersComplete;
        public bool RemoteHalfClosed;
    }

    private static void WriteToStream(MemoryStream stream, ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
        {
            return;
        }
        var rented = ArrayPool<byte>.Shared.Rent(data.Length);
        try
        {
            data.CopyTo(rented.AsSpan());
            stream.Write(rented, 0, data.Length);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }
}