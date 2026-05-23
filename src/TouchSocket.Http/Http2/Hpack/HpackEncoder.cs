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

namespace TouchSocket.Http;

/// <summary>
/// HPACK 头部块编码器，见 RFC 7541
/// </summary>
internal sealed class HpackEncoder
{
    // 动态表（最新在前）
    private readonly List<Http2Header> m_dynamicTable = new List<Http2Header>(32);
    private uint m_dynamicTableSize;
    private uint m_maxDynamicTableSize;

    /// <summary>
    /// 初始化 <see cref="HpackEncoder"/>
    /// </summary>
    /// <param name="maxDynamicTableSize">动态表大小上限</param>
    public HpackEncoder(uint maxDynamicTableSize = 4096)
    {
        this.m_maxDynamicTableSize = maxDynamicTableSize;
    }

    /// <summary>
    /// 更新动态表大小上限（由远端 SETTINGS 触发），并写入大小更新表示
    /// </summary>
    public void UpdateMaxDynamicTableSize(uint newMax)
    {
        this.m_maxDynamicTableSize = newMax;
        this.EvictToLimit(newMax);
    }

    /// <summary>
    /// 将头部列表编码写入 <paramref name="writer"/>
    /// </summary>
    public void Encode(IList<Http2Header> headers, SegmentedBytesWriter writer)
    {
        for (var i = 0; i < headers.Count; i++)
        {
            var h = headers[i];
            this.EncodeHeader(writer, h.Name, h.Value);
        }
    }

    private void EncodeHeader(SegmentedBytesWriter writer, string name, string value)
    {
        // 1. 尝试静态表完整匹配
        var staticIndex = HpackStaticTable.FindIndex(name, value);
        if (staticIndex > 0)
        {
            WriteIndexed(writer, staticIndex);
            return;
        }

        // 2. 尝试动态表完整匹配
        var dynFullIndex = this.FindInDynamicTable(name, value);
        if (dynFullIndex > 0)
        {
            WriteIndexed(writer, dynFullIndex);
            return;
        }

        // 3. 尝试仅名称匹配（静态表）
        var nameOnlyIndex = HpackStaticTable.FindNameIndex(name);

        // 4. 尝试仅名称匹配（动态表）
        if (nameOnlyIndex == 0)
        {
            nameOnlyIndex = this.FindNameInDynamicTable(name);
        }

        // 使用增量索引编码（6.2.1），并添加到动态表
        WriteLiteralWithIndexing(writer, nameOnlyIndex, name, value);
        this.InsertIntoDynamicTable(name, value);
    }

    private static void WriteIndexed(SegmentedBytesWriter writer, int index)
    {
        // 1xxxxxxx
        WriteInteger(writer, (uint)index, 7, 0x80);
    }

    private static void WriteLiteralWithIndexing(SegmentedBytesWriter writer, int nameIndex, string name, string value)
    {
        // 01xxxxxx（名称索引或 0）
        WriteInteger(writer, (uint)nameIndex, 6, 0x40);
        if (nameIndex == 0)
        {
            WriteString(writer, name);
        }
        WriteString(writer, value);
    }

    private static void WriteInteger(SegmentedBytesWriter writer, uint value, int prefixBits, byte prefixMask)
    {
        var maxPrefix = (uint)((1 << prefixBits) - 1);
        Span<byte> buf = stackalloc byte[8];
        var pos = 0;

        if (value < maxPrefix)
        {
            buf[pos++] = (byte)(prefixMask | (byte)value);
        }
        else
        {
            buf[pos++] = (byte)(prefixMask | maxPrefix);
            value -= maxPrefix;
            while (value >= 128)
            {
                buf[pos++] = (byte)((value & 0x7F) | 0x80);
                value >>= 7;
            }
            buf[pos++] = (byte)value;
        }
        writer.Write(buf.Slice(0, pos));
    }

    private static void WriteString(SegmentedBytesWriter writer, string value)
    {
        var maxByteCount = Encoding.UTF8.GetMaxByteCount(value.Length);
        var rented = ArrayPool<byte>.Shared.Rent(maxByteCount);
        try
        {
            var byteCount = Encoding.UTF8.GetBytes(value, 0, value.Length, rented, 0);
            var bytes = rented.AsSpan(0, byteCount);
            var huffmanLen = HpackHuffman.GetEncodedLength(bytes);

            if (huffmanLen < byteCount)
            {
                WriteInteger(writer, (uint)huffmanLen, 7, 0x80);
                var huffBuf = ArrayPool<byte>.Shared.Rent(huffmanLen);
                try
                {
                    var written = HpackHuffman.Encode(bytes, huffBuf.AsSpan());
                    writer.Write(huffBuf.AsSpan(0, written));
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(huffBuf);
                }
            }
            else
            {
                WriteInteger(writer, (uint)byteCount, 7, 0x00);
                writer.Write(bytes);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private int FindInDynamicTable(string name, string value)
    {
        for (var i = 0; i < this.m_dynamicTable.Count; i++)
        {
            var e = this.m_dynamicTable[i];
            if (string.Equals(e.Name, name, StringComparison.Ordinal)
                && string.Equals(e.Value, value, StringComparison.Ordinal))
            {
                return HpackStaticTable.Count + 1 + i;
            }
        }
        return 0;
    }

    private int FindNameInDynamicTable(string name)
    {
        for (var i = 0; i < this.m_dynamicTable.Count; i++)
        {
            if (string.Equals(this.m_dynamicTable[i].Name, name, StringComparison.Ordinal))
            {
                return HpackStaticTable.Count + 1 + i;
            }
        }
        return 0;
    }

    private void InsertIntoDynamicTable(string name, string value)
    {
        var entrySize = (uint)(Encoding.UTF8.GetByteCount(name) + Encoding.UTF8.GetByteCount(value) + 32);
        this.EvictToLimit(this.m_maxDynamicTableSize > entrySize ? this.m_maxDynamicTableSize - entrySize : 0);

        if (entrySize <= this.m_maxDynamicTableSize)
        {
            this.m_dynamicTable.Insert(0, new Http2Header(name, value));
            this.m_dynamicTableSize += entrySize;
        }
    }

    private void EvictToLimit(uint limit)
    {
        while (this.m_dynamicTableSize > limit && this.m_dynamicTable.Count > 0)
        {
            var last = this.m_dynamicTable[this.m_dynamicTable.Count - 1];
            this.m_dynamicTable.RemoveAt(this.m_dynamicTable.Count - 1);
            this.m_dynamicTableSize -= (uint)(Encoding.UTF8.GetByteCount(last.Name)
                + Encoding.UTF8.GetByteCount(last.Value) + 32);
        }
    }
}
