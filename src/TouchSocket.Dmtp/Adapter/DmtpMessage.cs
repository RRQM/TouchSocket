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

namespace TouchSocket.Dmtp;

/// <summary>
/// Dmtp协议的消息。
/// <para>|*2*|*2*|**4**|***************n***********|</para>
/// <para>|dm|ProtocolFlags|Length|Data|</para>
/// <para>|head|ushort|int32|bytes|</para>
/// </summary>
public class DmtpMessage : DisposableObject, IFixedHeaderByteBlockRequestInfo, IRequestInfoBuilder
{
    private int m_bodyLength;

    /// <summary>
    /// Dmtp协议的消息。
    /// <para>|*2*|*2*|**4**|***************n***********|</para>
    /// <para>|Head|ProtocolFlags|Length|Data|</para>
    /// <para>|dm|ushort|int32|bytes|</para>
    /// </summary>
    public DmtpMessage()
    {
    }

    /// <summary>
    /// Head
    /// </summary>
    public static readonly byte[] Head = "dm"u8.ToArray();

    /// <summary>
    /// Dmtp协议的消息。
    /// <para>|*2*|**4**|***************n***********|</para>
    /// <para>|ProtocolFlags|Length|Data|</para>
    /// <para>|ushort|int32|bytes|</para>
    /// <param name="protocolFlags"></param>
    /// </summary>
    public DmtpMessage(ushort protocolFlags)
    {
        this.ProtocolFlags = protocolFlags;
    }

    /// <summary>
    /// 实际使用的Body数据。
    /// </summary>
    public ByteBlock BodyByteBlock { get; set; }

    int IFixedHeaderByteBlockRequestInfo.BodyLength => this.m_bodyLength;

    /// <summary>
    /// 协议标识
    /// </summary>
    public ushort ProtocolFlags { get; set; }

    /// <inheritdoc/>
    public int MaxLength => this.BodyByteBlock == null ? 6 : this.BodyByteBlock.Length + 6;

    /// <summary>
    /// 构建数据到<see cref="ByteBlock"/>
    /// </summary>
    /// <param name="byteBlock"></param>
    public void Build<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        byteBlock.Write(Head);
        byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes(this.ProtocolFlags));
        if (this.BodyByteBlock == null)
        {
            byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes(0));
        }
        else
        {
            byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes(this.BodyByteBlock.Length));
            byteBlock.Write(this.BodyByteBlock.Span);
        }
    }

    /// <summary>
    /// 从当前内存中解析出一个<see cref="DmtpMessage"/>
    /// <para>注意：
    /// <list type="number">
    /// <item>本解析只能解析一个完整消息。所以使用该方法时，请确认是否已经接收完成一个完整的<see cref="DmtpMessage"/>包。</item>
    /// <item>本解析所得的<see cref="DmtpMessage"/>消息会脱离生命周期管理，所以需要手动释放。</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <returns></returns>
    public static DmtpMessage CreateFrom(ReadOnlySpan<byte> span)
    {
        var offset = 0;
        if (span[offset++] != Head[0] || span[offset++] != Head[1])
        {
            throw new Exception("这可能不是Dmtp协议数据");
        }
        var protocolFlags = TouchSocketBitConverter.BigEndian.To<ushort>(span.Slice(offset));
        offset += 2;
        var bodyLength = TouchSocketBitConverter.BigEndian.To<int>(span.Slice(offset));
        offset += 4;
        var byteBlock = new ByteBlock(bodyLength);
        byteBlock.Write(span.Slice(offset, bodyLength));
        byteBlock.SeekToStart();
        return new DmtpMessage()
        {
            m_bodyLength = bodyLength,
            BodyByteBlock = byteBlock,
            ProtocolFlags = protocolFlags
        };
    }

    /// <summary>
    /// 将<see cref="BodyByteBlock"/>的有效数据转换为utf-8的字符串。
    /// </summary>
    /// <returns></returns>
    public string GetBodyString()
    {
        if (this.BodyByteBlock == null || this.BodyByteBlock.Length == 0)
        {
            return default;
        }
        else
        {
            return this.BodyByteBlock.Span.ToString(Encoding.UTF8);
        }
    }

    bool IFixedHeaderByteBlockRequestInfo.OnParsingBody(ByteBlock byteBlock)
    {
        if (byteBlock.Length == this.m_bodyLength)
        {
            this.BodyByteBlock = byteBlock;
            return true;
        }

        return false;
    }

    bool IFixedHeaderByteBlockRequestInfo.OnParsingHeader(ReadOnlySpan<byte> header)
    {
        if (header.Length == 8)
        {
            var offset = 0;
            if (header[offset++] != Head[0] || header[offset++] != Head[1])
            {
                throw new Exception("这可能不是Dmtp协议数据");
            }
            this.ProtocolFlags = TouchSocketBitConverter.BigEndian.To<ushort>(header.Slice(offset));
            offset += 2;
            this.m_bodyLength = TouchSocketBitConverter.BigEndian.To<int>(header.Slice(offset));
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        if (this.DisposedValue)
        {
            return;
        }
        if (disposing)
        {
            this.BodyByteBlock.SafeDispose();
        }

        base.Dispose(disposing);
    }
}