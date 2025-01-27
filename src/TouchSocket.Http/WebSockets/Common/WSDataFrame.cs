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
using System.Text;
using TouchSocket.Core;

namespace TouchSocket.Http.WebSockets;

/// <summary>
/// WebSocket数据帧
/// </summary>
public class WSDataFrame : DisposableObject, IRequestInfo, IRequestInfoBuilder, IBigUnfixedHeaderRequestInfo
{
    private int m_headerLength;
    private int m_payloadLength;

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
    public byte[] MaskingKey { get; set; }

    /// <inheritdoc/>
    public int MaxLength => this.PayloadLength + 100;

    /// <summary>
    /// 数据类型
    /// </summary>
    public WSDataType Opcode { get; set; }

    /// <summary>
    /// 有效数据
    /// </summary>
    public ByteBlock PayloadData { get; set; }

    /// <summary>
    /// 有效载荷数据长度
    /// </summary>
    public int PayloadLength
    {
        get => this.m_payloadLength != 0 ? this.m_payloadLength : this.PayloadData != null ? this.PayloadData.Length : 0;

        set => this.m_payloadLength = value;
    }

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
    public void Build<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        var memory = this.PayloadData == null ? ReadOnlyMemory<byte>.Empty : this.PayloadData.Memory;
        int payloadLength;

        Span<byte> extLen = stackalloc byte[8];

        var length = memory.Length;

        if (length < 126)
        {
            payloadLength = length;
            extLen = Span<byte>.Empty;
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
        header = (header << 4) + (ushort)this.Opcode;

        header = this.Mask ? (header << 1) + 1 : (header << 1) + 0;

        header = (header << 7) + payloadLength;

        byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes((ushort)header));

        if (payloadLength > 125)
        {
            byteBlock.Write(extLen);
        }

        if (this.Mask)
        {
            byteBlock.Write(new ReadOnlySpan<byte>(this.MaskingKey, 0, 4));
        }

        if (payloadLength > 0)
        {
            if (this.Mask)
            {
                if (byteBlock.Capacity < byteBlock.Position + length)
                {
                    byteBlock.SetCapacity(byteBlock.Position + length, true);
                }
                WSTools.DoMask(byteBlock.TotalMemory.Span.Slice(byteBlock.Position), memory.Span, this.MaskingKey);
                byteBlock.SetLength(byteBlock.Position + length);
            }
            else
            {
                byteBlock.Write(memory.Span);
            }
        }
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
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.PayloadData.SafeDispose();
        }
        base.Dispose(disposing);
    }

    #region UnfixedHeader

    long IBigUnfixedHeaderRequestInfo.BodyLength => this.m_payloadLength;
    int IBigUnfixedHeaderRequestInfo.HeaderLength => this.m_headerLength;

    void IBigUnfixedHeaderRequestInfo.OnAppendBody(ReadOnlySpan<byte> buffer)
    {
        this.PayloadData.Write(buffer);
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
                WSTools.DoMask(this.PayloadData.TotalMemory.Span, this.PayloadData.Memory.Span, this.MaskingKey);
            }
        }

        return true;
    }

    bool IBigUnfixedHeaderRequestInfo.OnParsingHeader<TByteBlock>(ref TByteBlock byteBlock)
    {
        var offset = byteBlock.Position;
        var index = offset;
        var dataBuffer = byteBlock.Span.Slice(offset);
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
                //offset = index;
                //cache
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
            if (length < (offset - index) + 4)
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

        this.m_headerLength = offset - index;
        byteBlock.Position += this.m_headerLength;

        if (payloadLength > 0)
        {
            this.PayloadData = new ByteBlock(payloadLength);
        }

        return true;

        //var block = new ByteBlock(payloadLength);
        //dataFrame.PayloadData = block;

        //var surlen = length - (offset - index);
        //if (payloadLength <= surlen)
        //{
        //    block.Write(new System.ReadOnlySpan<byte>(dataBuffer, offset, payloadLength));
        //    offset += payloadLength;
        //}
        //else
        //{
        //    block.Write(new System.ReadOnlySpan<byte>(dataBuffer, offset, surlen));
        //    offset += surlen;
        //}
    }

    #endregion UnfixedHeader
}