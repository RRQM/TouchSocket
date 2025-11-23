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

namespace TouchSocket.Http.WebSockets;

/// <summary>
/// WebSocket数据帧
/// </summary>
public sealed class WSDataFrame : IRequestInfo, IRequestInfoBuilder, IBigUnfixedHeaderRequestInfo
{
    private readonly ReadOnlyMemory<byte> m_payloadData;
    private int m_headerLength;
    private ByteBlock m_payloadDataBlock;
    private int m_payloadLength;

    public WSDataFrame(ReadOnlyMemory<byte> payloadData)
    {
        this.m_payloadData = payloadData;
    }

    internal WSDataFrame()
    {
    }

    /// <summary>
    /// 是否为最后数据帧。
    /// </summary>
    public bool FIN { get; set; }

    /// <summary>
    /// 是否是二进制数据类型
    /// </summary>
    public bool IsBinary => this.Opcode == WSDataType.Binary;

    /// <summary>
    /// 是否是关闭请求
    /// </summary>
    public bool IsClose => this.Opcode == WSDataType.Close;

    /// <summary>
    /// 是否是Ping
    /// </summary>
    public bool IsPing => this.Opcode == WSDataType.Ping;

    /// <summary>
    /// 是否是Pong
    /// </summary>
    public bool IsPong => this.Opcode == WSDataType.Pong;

    /// <summary>
    /// 是否是文本类型
    /// </summary>
    public bool IsText => this.Opcode == WSDataType.Text;

    /// <summary>
    /// 计算掩码
    /// </summary>
    public bool Mask { get; set; }

    /// <summary>
    /// 掩码值
    /// </summary>
    public ReadOnlyMemory<byte> MaskingKey { get; set; }

    /// <inheritdoc/>
    public int MaxLength => this.PayloadData.Length + 14;

    /// <summary>
    /// 数据类型
    /// </summary>
    public WSDataType Opcode { get; set; }

    /// <summary>
    /// 有效数据
    /// </summary>
    public ReadOnlyMemory<byte> PayloadData => this.m_payloadDataBlock?.Memory ?? this.m_payloadData;

    /// <summary>
    /// 标识RSV-1。
    /// </summary>
    public bool RSV1 { get; set; }

    /// <summary>
    /// 标识RSV-2。
    /// </summary>
    public bool RSV2 { get; set; }

    /// <summary>
    /// 标识RSV-3。
    /// </summary>
    public bool RSV3 { get; set; }

    /// <inheritdoc/>
    public void Build<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        var memory = this.PayloadData;

        var rawMemory = writer.GetMemory(memory.Length + 14);

        var rawWriter = new BytesWriter(rawMemory);

        int payloadLength;

        Span<byte> extLen = stackalloc byte[8];

        var length = memory.Length;

        if (length < 126)
        {
            payloadLength = length;
            extLen = [];
        }
        else if (length < 65536)
        {
            payloadLength = 126;

            TouchSocketBitConverter.BigEndian.WriteBytes(extLen, (ushort)length);
            extLen = extLen.Slice(0, 2);
        }
        else
        {
            payloadLength = 127;
            TouchSocketBitConverter.BigEndian.WriteBytes(extLen, (ulong)length);
        }

        var header = this.FIN ? 1 : 0;
        header = (header << 1) + (this.RSV1 ? 1 : 0);
        header = (header << 1) + (this.RSV2 ? 1 : 0);
        header = (header << 1) + (this.RSV3 ? 1 : 0);
        header = (header << 4) + (byte)this.Opcode;

        header = this.Mask ? (header << 1) + 1 : (header << 1) + 0;

        header = (header << 7) + payloadLength;

        WriterExtension.WriteValue<BytesWriter, ushort>(ref rawWriter, (ushort)header, EndianType.Big);

        if (payloadLength > 125)
        {
            rawWriter.Write(extLen);
        }

        Span<byte> maskingKey = stackalloc byte[4];

        if (this.Mask)
        {
            if (this.MaskingKey.IsEmpty)
            {
                TouchSocketBitConverter.BigEndian.WriteBytes(maskingKey, (int)DateTime.UtcNow.Ticks);
            }
            else
            {
                this.MaskingKey.Span.Slice(0, 4).CopyTo(maskingKey);
            }

            rawWriter.Write(maskingKey);
        }

        if (payloadLength > 0)
        {
            if (this.Mask)
            {
                WSTools.DoMask(rawMemory.Span.Slice((int)rawWriter.WrittenCount), memory.Span, maskingKey);
                rawWriter.Advance(length);
            }
            else
            {
                rawWriter.Write(memory.Span);
            }
        }

        writer.Advance((int)rawWriter.WrittenCount);
    }

    public void SetMask(ReadOnlyMemory<byte> mask)
    {
        if (mask.Length != 4)
        {
            throw new OverlengthException("Mask只能为四位。");
        }
        this.MaskingKey = mask;
        this.Mask = true;
    }

    /// <summary>
    /// 设置Mask。
    /// </summary>
    /// <param name="mask"></param>
    /// <returns></returns>
    public void SetMaskString(string mask)
    {
        var masks = Encoding.ASCII.GetBytes(mask);
        if (masks.Length != 4)
        {
            throw new OverlengthException("Mask只能为ASCII，且只能为四位。");
        }
        this.MaskingKey = masks;
        this.Mask = true;
    }

    /// <inheritdoc/>
    internal void Dispose()
    {
        this.m_payloadDataBlock.SafeDispose();
        this.m_payloadDataBlock = default;
    }

    #region UnfixedHeader

    long IBigUnfixedHeaderRequestInfo.BodyLength => this.m_payloadLength;
    int IBigUnfixedHeaderRequestInfo.HeaderLength => this.m_headerLength;

    void IBigUnfixedHeaderRequestInfo.OnAppendBody(ReadOnlySpan<byte> buffer)
    {
        this.m_payloadDataBlock.Write(buffer);
    }

    bool IBigUnfixedHeaderRequestInfo.OnFinished()
    {
        if (this.m_payloadLength > 0)
        {
            if (this.m_payloadLength != this.PayloadData.Length)
            {
                return false;
            }

            if (this.Mask)
            {
                WSTools.DoMask(this.m_payloadDataBlock.TotalMemory.Span, this.m_payloadDataBlock.Memory.Span, this.MaskingKey.Span);
            }
        }

        return true;
    }

    bool IBigUnfixedHeaderRequestInfo.OnParsingHeader<TReader>(ref TReader reader)
    {
        var offset = 0;
        var dataBuffer = reader.GetSpan((int)reader.BytesRemaining);
        var length = dataBuffer.Length;
        if (length < 2)
        {
            return false;
        }

        this.RSV1 = dataBuffer[offset].GetBit(6);
        this.RSV2 = dataBuffer[offset].GetBit(5);
        this.RSV3 = dataBuffer[offset].GetBit(4);
        this.FIN = (dataBuffer[offset] >> 7) == 1;
        this.Opcode = (WSDataType)(dataBuffer[offset] & 0xf);
        this.Mask = (dataBuffer[++offset] >> 7) == 1;

        var payloadLength = dataBuffer[offset] & 0x7f;
        if (payloadLength < 126)
        {
            offset++;
        }
        else if (payloadLength == 126)
        {
            if (length < 4)
            {
                return false;
            }
            payloadLength = TouchSocketBitConverter.BigEndian.To<ushort>(dataBuffer.Slice(++offset));
            offset += 2;
        }
        else if (payloadLength == 127)
        {
            if (length < 12)
            {
                return false;
            }
            payloadLength = (int)TouchSocketBitConverter.BigEndian.To<ulong>(dataBuffer.Slice(++offset));
            offset += 8;
        }

        this.m_payloadLength = payloadLength;

        if (this.Mask)
        {
            if (length < (offset) + 4)
            {
                return false;
            }
            var maskingKey = new byte[4];
            maskingKey[0] = dataBuffer[offset++];
            maskingKey[1] = dataBuffer[offset++];
            maskingKey[2] = dataBuffer[offset++];
            maskingKey[3] = dataBuffer[offset++];
            this.MaskingKey = maskingKey;
        }

        this.m_headerLength = offset;
        reader.Advance(this.m_headerLength);

        if (payloadLength > 0)
        {
            this.m_payloadDataBlock = new ByteBlock(payloadLength);
        }

        return true;
    }

    #endregion UnfixedHeader
}