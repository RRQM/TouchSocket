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
/// HTTP/2 帧写入工具，负责向 <see cref="PipeWriter"/> 序列化各类型帧
/// </summary>
internal sealed class Http2FrameWriter
{
    private readonly PipeWriter m_writer;
    private readonly SemaphoreSlim m_writeLock = new SemaphoreSlim(1, 1);

    /// <summary>
    /// 初始化 <see cref="Http2FrameWriter"/>
    /// </summary>
    public Http2FrameWriter(PipeWriter writer)
    {
        this.m_writer = writer;
    }

    /// <summary>
    /// 写入 SETTINGS 帧
    /// </summary>
    public async ValueTask WriteSettingsAsync(Http2PeerSettings settings, bool isAck, CancellationToken cancellationToken)
    {
        await this.m_writeLock.WaitAsync(cancellationToken).ConfigureDefaultAwait();
        try
        {
            if (isAck)
            {
                this.WriteFrameHeader(0, Http2FrameType.Settings, Http2Flags.EndStreamOrAck, 0);
            }
            else
            {
                // 6 个参数，每个 6 字节 = 36 字节
                this.WriteFrameHeader(36, Http2FrameType.Settings, Http2Flags.None, 0);
                this.WriteSettingParam(Http2SettingsParameter.HeaderTableSize, settings.HeaderTableSize);
                this.WriteSettingParam(Http2SettingsParameter.EnablePush, settings.EnablePush ? 1u : 0u);
                this.WriteSettingParam(Http2SettingsParameter.MaxConcurrentStreams, settings.MaxConcurrentStreams);
                this.WriteSettingParam(Http2SettingsParameter.InitialWindowSize, settings.InitialWindowSize);
                this.WriteSettingParam(Http2SettingsParameter.MaxFrameSize, settings.MaxFrameSize);
                this.WriteSettingParam(Http2SettingsParameter.MaxHeaderListSize, settings.MaxHeaderListSize);
            }

            await this.m_writer.FlushAsync(cancellationToken).ConfigureDefaultAwait();
        }
        finally
        {
            this.m_writeLock.Release();
        }
    }

    /// <summary>
    /// 写入 PING 帧
    /// </summary>
    public async ValueTask WritePingAsync(ReadOnlyMemory<byte> opaqueData, bool isAck, CancellationToken cancellationToken)
    {
        await this.m_writeLock.WaitAsync(cancellationToken).ConfigureDefaultAwait();
        try
        {
            var flags = isAck ? Http2Flags.EndStreamOrAck : Http2Flags.None;
            this.WriteFrameHeader(8, Http2FrameType.Ping, flags, 0);
            var span = this.m_writer.GetSpan(8);
            opaqueData.Span.CopyTo(span);
            this.m_writer.Advance(8);
            await this.m_writer.FlushAsync(cancellationToken).ConfigureDefaultAwait();
        }
        finally
        {
            this.m_writeLock.Release();
        }
    }

    /// <summary>
    /// 写入 GOAWAY 帧
    /// </summary>
    public async ValueTask WriteGoAwayAsync(int lastStreamId, Http2ErrorCode errorCode, ReadOnlyMemory<byte> debugData, CancellationToken cancellationToken)
    {
        await this.m_writeLock.WaitAsync(cancellationToken).ConfigureDefaultAwait();
        try
        {
            var payloadLength = 8 + debugData.Length;
            this.WriteFrameHeader(payloadLength, Http2FrameType.GoAway, Http2Flags.None, 0);
            var span = this.m_writer.GetSpan(8);
            BinaryPrimitives.WriteUInt32BigEndian(span, (uint)(lastStreamId & 0x7FFFFFFF));
            BinaryPrimitives.WriteUInt32BigEndian(span.Slice(4), (uint)errorCode);
            this.m_writer.Advance(8);
            if (debugData.Length > 0)
            {
                var debugSpan = this.m_writer.GetSpan(debugData.Length);
                debugData.Span.CopyTo(debugSpan);
                this.m_writer.Advance(debugData.Length);
            }
            await this.m_writer.FlushAsync(cancellationToken).ConfigureDefaultAwait();
        }
        finally
        {
            this.m_writeLock.Release();
        }
    }

    /// <summary>
    /// 写入 RST_STREAM 帧
    /// </summary>
    public async ValueTask WriteRstStreamAsync(int streamId, Http2ErrorCode errorCode, CancellationToken cancellationToken)
    {
        await this.m_writeLock.WaitAsync(cancellationToken).ConfigureDefaultAwait();
        try
        {
            this.WriteFrameHeader(4, Http2FrameType.RstStream, Http2Flags.None, streamId);
            var span = this.m_writer.GetSpan(4);
            BinaryPrimitives.WriteUInt32BigEndian(span, (uint)errorCode);
            this.m_writer.Advance(4);
            await this.m_writer.FlushAsync(cancellationToken).ConfigureDefaultAwait();
        }
        finally
        {
            this.m_writeLock.Release();
        }
    }

    /// <summary>
    /// 写入 WINDOW_UPDATE 帧
    /// </summary>
    public async ValueTask WriteWindowUpdateAsync(int streamId, uint increment, CancellationToken cancellationToken)
    {
        await this.m_writeLock.WaitAsync(cancellationToken).ConfigureDefaultAwait();
        try
        {
            this.WriteFrameHeader(4, Http2FrameType.WindowUpdate, Http2Flags.None, streamId);
            var span = this.m_writer.GetSpan(4);
            BinaryPrimitives.WriteUInt32BigEndian(span, increment & 0x7FFFFFFF);
            this.m_writer.Advance(4);
            await this.m_writer.FlushAsync(cancellationToken).ConfigureDefaultAwait();
        }
        finally
        {
            this.m_writeLock.Release();
        }
    }

    /// <summary>
    /// 写入 HEADERS 帧（携带头部块）
    /// </summary>
    public async ValueTask WriteHeadersAsync(int streamId, ReadOnlySequence<byte> headerBlock, bool endStream, CancellationToken cancellationToken)
    {
        await this.m_writeLock.WaitAsync(cancellationToken).ConfigureDefaultAwait();
        try
        {
            var flags = Http2Flags.EndHeaders;
            if (endStream)
            {
                flags |= Http2Flags.EndStreamOrAck;
            }
            var payloadLength = (int)headerBlock.Length;
            this.WriteFrameHeader(payloadLength, Http2FrameType.Headers, flags, streamId);
            foreach (var segment in headerBlock)
            {
                var span = this.m_writer.GetSpan(segment.Length);
                segment.Span.CopyTo(span);
                this.m_writer.Advance(segment.Length);
            }
            await this.m_writer.FlushAsync(cancellationToken).ConfigureDefaultAwait();
        }
        finally
        {
            this.m_writeLock.Release();
        }
    }

    /// <summary>
    /// 写入 DATA 帧
    /// </summary>
    public async ValueTask WriteDataAsync(int streamId, ReadOnlyMemory<byte> data, bool endStream, CancellationToken cancellationToken)
    {
        await this.m_writeLock.WaitAsync(cancellationToken).ConfigureDefaultAwait();
        try
        {
            var flags = endStream ? Http2Flags.EndStreamOrAck : Http2Flags.None;
            this.WriteFrameHeader(data.Length, Http2FrameType.Data, flags, streamId);
            if (data.Length > 0)
            {
                var span = this.m_writer.GetSpan(data.Length);
                data.Span.CopyTo(span);
                this.m_writer.Advance(data.Length);
            }
            await this.m_writer.FlushAsync(cancellationToken).ConfigureDefaultAwait();
        }
        finally
        {
            this.m_writeLock.Release();
        }
    }

    /// <summary>
    /// 写入 PUSH_PROMISE 帧
    /// </summary>
    public async ValueTask WritePushPromiseAsync(int streamId, int promisedStreamId, ReadOnlyMemory<byte> headerBlock, CancellationToken cancellationToken)
    {
        await this.m_writeLock.WaitAsync(cancellationToken).ConfigureDefaultAwait();
        try
        {
            // payload = 4 (promised stream id) + header block
            var payloadLength = 4 + headerBlock.Length;
            this.WriteFrameHeader(payloadLength, Http2FrameType.PushPromise, Http2Flags.EndHeaders, streamId);
            var idSpan = this.m_writer.GetSpan(4);
            BinaryPrimitives.WriteUInt32BigEndian(idSpan, (uint)(promisedStreamId & 0x7FFFFFFF));
            this.m_writer.Advance(4);
            var hdrSpan = this.m_writer.GetSpan(headerBlock.Length);
            headerBlock.Span.CopyTo(hdrSpan);
            this.m_writer.Advance(headerBlock.Length);
            await this.m_writer.FlushAsync(cancellationToken).ConfigureDefaultAwait();
        }
        finally
        {
            this.m_writeLock.Release();
        }
    }

    private void WriteFrameHeader(int payloadLength, Http2FrameType type, Http2Flags flags, int streamId)
    {
        var span = this.m_writer.GetSpan(Http2FrameHeader.Size);
        var header = new Http2FrameHeader(payloadLength, type, flags, streamId);
        header.WriteTo(span);
        this.m_writer.Advance(Http2FrameHeader.Size);
    }

    private void WriteSettingParam(Http2SettingsParameter param, uint value)
    {
        var span = this.m_writer.GetSpan(6);
        BinaryPrimitives.WriteUInt16BigEndian(span, (ushort)param);
        BinaryPrimitives.WriteUInt32BigEndian(span.Slice(2), value);
        this.m_writer.Advance(6);
    }
}
