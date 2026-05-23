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

using System.Buffers.Binary;

namespace TouchSocket.Http;

/// <summary>
/// HTTP/2 帧头，共 9 字节，见 RFC 7540 §4.1
/// </summary>
internal readonly struct Http2FrameHeader
{
    /// <summary>帧头固定长度（9 字节）</summary>
    public const int Size = 9;

    private readonly int m_payloadLength;
    private readonly Http2FrameType m_type;
    private readonly Http2Flags m_flags;
    private readonly int m_streamId;

    /// <summary>
    /// 初始化 <see cref="Http2FrameHeader"/>
    /// </summary>
    public Http2FrameHeader(int payloadLength, Http2FrameType type, Http2Flags flags, int streamId)
    {
        this.m_payloadLength = payloadLength;
        this.m_type = type;
        this.m_flags = flags;
        this.m_streamId = streamId & 0x7FFFFFFF;
    }

    /// <summary>负载字节数（24 位）</summary>
    public int PayloadLength => this.m_payloadLength;

    /// <summary>帧类型</summary>
    public Http2FrameType Type => this.m_type;

    /// <summary>帧标志</summary>
    public Http2Flags Flags => this.m_flags;

    /// <summary>流标识符（31 位，忽略保留位）</summary>
    public int StreamId => this.m_streamId;

    /// <summary>判断是否具有指定标志</summary>
    public bool HasFlag(Http2Flags flag) => (this.m_flags & flag) != 0;

    /// <summary>
    /// 从字节缓冲区解析帧头
    /// </summary>
    public static bool TryRead(ReadOnlySpan<byte> buffer, out Http2FrameHeader header)
    {
        if (buffer.Length < Size)
        {
            header = default;
            return false;
        }

        var length = (buffer[0] << 16) | (buffer[1] << 8) | buffer[2];
        var type = (Http2FrameType)buffer[3];
        var flags = (Http2Flags)buffer[4];
        var streamId = (int)(BinaryPrimitives.ReadUInt32BigEndian(buffer.Slice(5)) & 0x7FFFFFFF);

        header = new Http2FrameHeader(length, type, flags, streamId);
        return true;
    }

    /// <summary>
    /// 将帧头序列化到字节缓冲区
    /// </summary>
    public void WriteTo(Span<byte> buffer)
    {
        buffer[0] = (byte)(this.m_payloadLength >> 16);
        buffer[1] = (byte)(this.m_payloadLength >> 8);
        buffer[2] = (byte)this.m_payloadLength;
        buffer[3] = (byte)this.m_type;
        buffer[4] = (byte)this.m_flags;
        BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(5), (uint)(this.m_streamId & 0x7FFFFFFF));
    }
}
