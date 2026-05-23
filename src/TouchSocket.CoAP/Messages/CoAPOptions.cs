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

using System.Collections.Generic;
using TouchSocket.Core;

namespace TouchSocket.CoAP;

/// <summary>
/// CoAP 选项集合，支持按选项编号有序管理，并提供 delta 编码的序列化与反序列化。
/// </summary>
public sealed class CoAPOptions
{
    private readonly List<CoAPOption> m_options = new List<CoAPOption>();

    /// <summary>
    /// 获取选项数量。
    /// </summary>
    public int Count => this.m_options.Count;

    /// <summary>
    /// 添加一个选项。允许同一编号存在多个选项（如 Uri-Path）。
    /// </summary>
    /// <param name="option">要添加的选项。</param>
    public void Add(CoAPOption option)
    {
        this.m_options.Add(option);
        this.m_options.Sort((a, b) => (int)a.Number - (int)b.Number);
    }

    /// <summary>
    /// 清除所有选项。
    /// </summary>
    public void Clear()
    {
        this.m_options.Clear();
    }

    /// <summary>
    /// 获取指定编号的第一个选项，未找到时返回 <see langword="null"/>。
    /// </summary>
    /// <param name="number">选项编号。</param>
    public CoAPOption GetOption(CoAPOptionNumber number)
    {
        foreach (var option in this.m_options)
        {
            if (option.Number == number)
            {
                return option;
            }
        }

        return null;
    }

    /// <summary>
    /// 获取指定编号的所有选项。
    /// </summary>
    /// <param name="number">选项编号。</param>
    public IEnumerable<CoAPOption> GetOptions(CoAPOptionNumber number)
    {
        foreach (var option in this.m_options)
        {
            if (option.Number == number)
            {
                yield return option;
            }
        }
    }

    /// <summary>
    /// 获取所有选项的只读列表。
    /// </summary>
    public IReadOnlyList<CoAPOption> GetAll() => this.m_options;

    /// <summary>
    /// 将选项集合序列化并写入 <see cref="IBytesWriter"/>，使用 delta 编码（RFC 7252 Section 3.1）。
    /// </summary>
    /// <typeparam name="TWriter">实现了 <see cref="IBytesWriter"/> 接口的写入器类型。</typeparam>
    /// <param name="writer">目标写入器。</param>
    public void Encode<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        var prevNumber = 0;
        foreach (var option in this.m_options)
        {
            var delta = (int)option.Number - prevNumber;
            var length = option.Value.Length;
            WriteOptionHeader(ref writer, delta, length);
            writer.Write(option.Value.Span);
            prevNumber = (int)option.Number;
        }
    }

    /// <summary>
    /// 将选项集合序列化为字节并写入缓冲区，使用 delta 编码（RFC 7252 Section 3.1）。
    /// </summary>
    /// <param name="buffer">目标缓冲区。</param>
    /// <param name="offset">写入起始偏移量。</param>
    /// <returns>写入的字节数。</returns>
    public int Encode(byte[] buffer, int offset)
    {
        var written = 0;
        var prevNumber = 0;

        foreach (var option in this.m_options)
        {
            var delta = (int)option.Number - prevNumber;
            var length = option.Value.Length;

            written += WriteOptionHeader(buffer, offset + written, delta, length);
            option.Value.Span.CopyTo(buffer.AsSpan(offset + written, length));
            written += length;

            prevNumber = (int)option.Number;
        }

        return written;
    }

    /// <summary>
    /// 计算选项集合序列化后的字节数（不含有效载荷标记）。
    /// </summary>
    public int GetEncodedLength()
    {
        var total = 0;
        var prevNumber = 0;

        foreach (var option in this.m_options)
        {
            var delta = (int)option.Number - prevNumber;
            var length = option.Value.Length;
            total += GetOptionHeaderLength(delta, length) + length;
            prevNumber = (int)option.Number;
        }

        return total;
    }

    /// <summary>
    /// 从字节缓冲区反序列化选项集合（RFC 7252 Section 3.1 delta 解码）。
    /// </summary>
    /// <param name="data">包含选项数据的字节序列。</param>
    /// <param name="offset">解析起始偏移量。</param>
    /// <param name="length">可读取的字节总数。</param>
    /// <returns>解析后的 <see cref="CoAPOptions"/>。</returns>
    public static CoAPOptions Decode(byte[] data, int offset, int length)
    {
        var options = new CoAPOptions();
        var pos = offset;
        var end = offset + length;
        var prevNumber = 0;

        while (pos < end)
        {
            var b = data[pos];

            if (b == 0xFF)
            {
                break;
            }

            var deltaNibble = (b >> 4) & 0x0F;
            var lengthNibble = b & 0x0F;
            pos++;

            var delta = ReadExtendedValue(data, ref pos, deltaNibble);
            var optLength = ReadExtendedValue(data, ref pos, lengthNibble);

            var optNumber = prevNumber + delta;
            var value = new byte[optLength];
            Array.Copy(data, pos, value, 0, optLength);
            pos += optLength;

            options.m_options.Add(new CoAPOption((CoAPOptionNumber)optNumber, value));
            prevNumber = optNumber;
        }

        return options;
    }

    private static void WriteOptionHeader<TWriter>(ref TWriter writer, int delta, int length) where TWriter : IBytesWriter
    {
        Span<byte> buf = stackalloc byte[5];
        var pos = 1;
        pos += WriteExtendedValueToSpan(buf, pos, delta, out var deltaNibble);
        pos += WriteExtendedValueToSpan(buf, pos, length, out var lengthNibble);
        buf[0] = (byte)((deltaNibble << 4) | (lengthNibble & 0x0F));
        writer.Write(buf.Slice(0, pos));
    }

    private static int WriteExtendedValueToSpan(Span<byte> buffer, int offset, int value, out int nibble)
    {
        if (value < 13)
        {
            nibble = value;
            return 0;
        }

        if (value < 269)
        {
            nibble = 13;
            buffer[offset] = (byte)(value - 13);
            return 1;
        }

        nibble = 14;
        var ext = value - 269;
        buffer[offset] = (byte)(ext >> 8);
        buffer[offset + 1] = (byte)ext;
        return 2;
    }

    private static int GetExtendedBytes(int value)
    {
        if (value < 13)
        {
            return 0;
        }

        if (value < 269)
        {
            return 1;
        }

        return 2;
    }

    private static int GetOptionHeaderLength(int delta, int length)
    {
        return 1 + GetExtendedBytes(delta) + GetExtendedBytes(length);
    }

    private static int ReadExtendedValue(byte[] data, ref int pos, int nibble)
    {
        if (nibble < 13)
        {
            return nibble;
        }

        if (nibble == 13)
        {
            return data[pos++] + 13;
        }

        if (nibble == 14)
        {
            var ext = (data[pos] << 8) | data[pos + 1];
            pos += 2;
            return ext + 269;
        }

        return 0;
    }

    private static int WriteExtendedValue(byte[] buffer, int offset, int value, out int nibble)
    {
        if (value < 13)
        {
            nibble = value;
            return 0;
        }

        if (value < 269)
        {
            nibble = 13;
            buffer[offset] = (byte)(value - 13);
            return 1;
        }

        nibble = 14;
        var ext = value - 269;
        buffer[offset] = (byte)(ext >> 8);
        buffer[offset + 1] = (byte)ext;
        return 2;
    }

    private static int WriteOptionHeader(byte[] buffer, int offset, int delta, int length)
    {
        var pos = offset + 1;
        var deltaNibble = 0;
        var lengthNibble = 0;

        pos += WriteExtendedValue(buffer, pos, delta, out deltaNibble);
        pos += WriteExtendedValue(buffer, pos, length, out lengthNibble);

        buffer[offset] = (byte)((deltaNibble << 4) | (lengthNibble & 0x0F));
        return pos - offset;
    }
}
