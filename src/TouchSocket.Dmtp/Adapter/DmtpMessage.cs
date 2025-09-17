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
/// DMTP协议的消息对象。
/// </summary>
/// <remarks>
/// <para>此类表示DMTP协议的消息格式，实现了<see cref="IBytesBuilder"/>和<see cref="IRequestInfo"/>接口。</para>
/// <para>消息格式如下：</para>
/// <para>|*2*|*2*|**4**|***************n***********|</para>
/// <para>|dm|ProtocolFlags|Length|Data|</para>
/// <para>|head|<see cref="ushort"/>|<see cref="int"/>|bytes|</para>
/// <para>其中head为固定的"dm"标识符，ProtocolFlags为协议标志，Length为数据长度，Data为实际数据。</para>
/// </remarks>
public sealed class DmtpMessage : IBytesBuilder, IRequestInfo
{
    /// <summary>
    /// DMTP协议的消息头部标识符。
    /// </summary>
    /// <value>固定为"dm"的字节数组。</value>
    public static readonly byte[] Head = "dm"u8.ToArray();

    private readonly ReadOnlyMemory<byte> m_memory;

    /// <summary>
    /// 初始化<see cref="DmtpMessage"/>类的新实例。
    /// </summary>
    /// <param name="protocolFlags">协议标志。</param>
    /// <param name="body">消息主体数据。</param>
    /// <remarks>
    /// <para>创建包含指定协议标志和主体数据的DMTP消息。</para>
    /// <para>消息格式：</para>
    /// <para>|*2*|*2*|**4**|***************n***********|</para>
    /// <para>|Head|ProtocolFlags|Length|Data|</para>
    /// <para>|dm|<see cref="ushort"/>|<see cref="int"/>|bytes|</para>
    /// </remarks>
    public DmtpMessage(ushort protocolFlags, ReadOnlyMemory<byte> body)
    {
        this.ProtocolFlags = protocolFlags;
        this.m_memory = body;
    }

    /// <summary>
    /// 初始化<see cref="DmtpMessage"/>类的新实例。
    /// </summary>
    /// <param name="protocolFlags">协议标志。</param>
    /// <remarks>
    /// <para>创建只包含协议标志的DMTP消息，主体数据为空。</para>
    /// <para>消息格式：</para>
    /// <para>|*2*|**4**|***************n***********|</para>
    /// <para>|ProtocolFlags|Length|Data|</para>
    /// <para>|<see cref="ushort"/>|<see cref="int"/>|bytes|</para>
    /// </remarks>
    public DmtpMessage(ushort protocolFlags)
    {
        this.ProtocolFlags = protocolFlags;
    }

    /// <summary>
    /// 获取构建数据时指示内存池的申请长度。
    /// </summary>
    /// <value>消息的最大长度，包括头部、协议标志、长度字段和数据部分。</value>
    /// <remarks>
    /// 此属性用于指示内存池应该申请的长度，建议设置得尽可能大一些以避免内存池扩容。
    /// 计算方式为：数据长度 + 8字节（2字节头部 + 2字节协议标志 + 4字节长度字段）。
    /// </remarks>
    public int MaxLength => this.Memory.Length + 8;

    /// <summary>
    /// 获取消息的主体数据内存块。
    /// </summary>
    /// <value>包含消息主体数据的只读内存块。</value>
    public ReadOnlyMemory<byte> Memory => this.m_memory;

    /// <summary>
    /// 获取协议标志。
    /// </summary>
    /// <value>标识DMTP协议类型或操作类型的16位无符号整数。</value>
    public ushort ProtocolFlags { get; private set; }

    /// <summary>
    /// 从指定的内存块中解析并创建<see cref="DmtpMessage"/>实例。
    /// </summary>
    /// <param name="memory">包含完整DMTP消息数据的内存块。</param>
    /// <returns>解析后的<see cref="DmtpMessage"/>实例。</returns>
    /// <exception cref="Exception">当内存块不包含有效的DMTP协议数据时抛出。</exception>
    /// <remarks>
    /// <para>重要注意事项：</para>
    /// <list type="number">
    /// <item>此解析方法只能解析一个完整的消息，使用前请确认已接收到完整的<see cref="DmtpMessage"/>数据包。</item>
    /// <item>解析所得的<see cref="DmtpMessage"/>消息会脱离生命周期管理，需要手动释放相关资源。</item>
    /// </list>
    /// <para>解析过程会验证消息头部是否为"dm"标识，如果不匹配则抛出异常。</para>
    /// </remarks>
    public static DmtpMessage CreateFrom(ReadOnlyMemory<byte> memory)
    {
        var offset = 0;
        var span = memory.Span;
        if (span[offset++] != Head[0] || span[offset++] != Head[1])
        {
            throw new Exception("这可能不是Dmtp协议数据");
        }
        var protocolFlags = TouchSocketBitConverter.BigEndian.To<ushort>(span.Slice(offset));
        offset += 2;
        var bodyLength = TouchSocketBitConverter.BigEndian.To<int>(span.Slice(offset));
        offset += 4;
        return new DmtpMessage(protocolFlags, memory.Slice(offset, bodyLength));
    }

    /// <summary>
    /// 将DMTP消息构建到指定的字节写入器中。
    /// </summary>
    /// <typeparam name="TWriter">实现<see cref="IBytesWriter"/>接口的字节写入器类型。</typeparam>
    /// <param name="writer">要写入数据的字节写入器。</param>
    /// <remarks>
    /// 此方法按照DMTP协议格式将消息数据写入到字节写入器中，写入顺序为：
    /// <list type="number">
    /// <item>消息头部标识符（"dm"）</item>
    /// <item>协议标志（大端序的<see cref="ushort"/>）</item>
    /// <item>数据长度（大端序的<see cref="int"/>）</item>
    /// <item>实际数据内容（如果存在）</item>
    /// </list>
    /// </remarks>
    public void Build<TWriter>(ref TWriter writer)
        where TWriter : IBytesWriter

    {
        writer.Write(Head);
        WriterExtension.WriteValue<TWriter, ushort>(ref writer, this.ProtocolFlags, EndianType.Big);
        if (this.Memory.IsEmpty)
        {
            WriterExtension.WriteValue<TWriter, int>(ref writer, 0, EndianType.Big);
        }
        else
        {
            WriterExtension.WriteValue<TWriter, int>(ref writer, this.Memory.Length, EndianType.Big);
            writer.Write(this.Memory.Span);
        }
    }

    /// <summary>
    /// 将消息主体数据转换为UTF-8编码的字符串。
    /// </summary>
    /// <returns>消息主体数据对应的UTF-8字符串。</returns>
    /// <remarks>
    /// 此方法将<see cref="Memory"/>中的有效字节数据按照UTF-8编码转换为字符串。
    /// 如果主体数据为空，则返回空字符串。
    /// </remarks>
    public string GetBodyString()
    {
        return this.Memory.Span.ToString(Encoding.UTF8);
    }
}