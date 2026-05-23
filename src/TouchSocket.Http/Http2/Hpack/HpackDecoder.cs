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
/// HPACK 头部块解码器，见 RFC 7541
/// </summary>
internal sealed class HpackDecoder
{
    // 动态表：按插入顺序排列（最新在前），索引 = HpackStaticTable.Count + 1 + 位置
    private readonly List<Http2Header> m_dynamicTable = new List<Http2Header>(32);
    private uint m_dynamicTableSize;
    private uint m_dynamicTableSizeLimit;

    /// <summary>
    /// 初始化 <see cref="HpackDecoder"/>
    /// </summary>
    /// <param name="maxDynamicTableSize">动态表大小上限（来自 SETTINGS_HEADER_TABLE_SIZE）</param>
    public HpackDecoder(uint maxDynamicTableSize = 4096)
    {
        this.m_dynamicTableSizeLimit = maxDynamicTableSize;
    }

    /// <summary>
    /// 更新动态表大小上限（由 SETTINGS_HEADER_TABLE_SIZE 触发）
    /// </summary>
    public void UpdateTableSizeLimit(uint newLimit)
    {
        this.m_dynamicTableSizeLimit = newLimit;
        this.EvictToLimit(newLimit);
    }

    /// <summary>
    /// 解码完整头部块，将结果追加到 <paramref name="output"/>
    /// </summary>
    /// <exception cref="Http2ConnectionException">解码失败时抛出</exception>
    public void Decode(ReadOnlySpan<byte> headerBlock, List<Http2Header> output)
    {
        var pos = 0;

        while (pos < headerBlock.Length)
        {
            var b = headerBlock[pos];

            if ((b & 0x80) != 0)
            {
                // 6.1 索引头部字段：1xxxxxxx
                this.DecodeIndexed(headerBlock, ref pos, output);
            }
            else if ((b & 0xC0) == 0x40)
            {
                // 6.2.1 增量索引：01xxxxxx
                this.DecodeLiteralWithIndexing(headerBlock, ref pos, output);
            }
            else if ((b & 0xE0) == 0x20)
            {
                // 6.3 动态表大小更新：001xxxxx
                this.DecodeDynamicTableSizeUpdate(headerBlock, ref pos);
            }
            else
            {
                // 6.2.2 不增量索引：0000xxxx；6.2.3 永不索引：0001xxxx
                this.DecodeLiteralWithoutIndexing(headerBlock, ref pos, output);
            }
        }
    }

    private void DecodeIndexed(ReadOnlySpan<byte> block, ref int pos, List<Http2Header> headers)
    {
        var index = DecodeInteger(block, ref pos, 7);
        if (index == 0)
        {
            throw new Http2ConnectionException(Http2ErrorCode.CompressionError, "HPACK 索引不能为 0");
        }

        headers.Add(this.LookupIndex((int)index));
    }

    private void DecodeLiteralWithIndexing(ReadOnlySpan<byte> block, ref int pos, List<Http2Header> headers)
    {
        var nameIndex = DecodeInteger(block, ref pos, 6);
        string name;
        if (nameIndex == 0)
        {
            name = DecodeString(block, ref pos);
        }
        else
        {
            name = this.LookupIndex((int)nameIndex).Name;
        }

        var value = DecodeString(block, ref pos);
        headers.Add(new Http2Header(name, value));

        this.InsertIntoDynamicTable(name, value);
    }

    private void DecodeLiteralWithoutIndexing(ReadOnlySpan<byte> block, ref int pos, List<Http2Header> headers)
    {
        var nameIndex = DecodeInteger(block, ref pos, 4);
        string name;
        if (nameIndex == 0)
        {
            name = DecodeString(block, ref pos);
        }
        else
        {
            name = this.LookupIndex((int)nameIndex).Name;
        }

        var value = DecodeString(block, ref pos);
        headers.Add(new Http2Header(name, value));
    }

    private void DecodeDynamicTableSizeUpdate(ReadOnlySpan<byte> block, ref int pos)
    {
        var newSize = DecodeInteger(block, ref pos, 5);
        if (newSize > this.m_dynamicTableSizeLimit)
        {
            throw new Http2ConnectionException(Http2ErrorCode.CompressionError,
                $"HPACK 动态表大小更新值 {newSize} 超过上限 {this.m_dynamicTableSizeLimit}");
        }
        this.EvictToLimit((uint)newSize);
    }

    private static string DecodeString(ReadOnlySpan<byte> block, ref int pos)
    {
        var isHuffman = (block[pos] & 0x80) != 0;
        var length = (int)DecodeInteger(block, ref pos, 7);
        var strBytes = block.Slice(pos, length);
        pos += length;

        if (isHuffman)
        {
            var buf = ArrayPool<byte>.Shared.Rent(length * 2);
            try
            {
                var decoded = HpackHuffman.Decode(strBytes, buf.AsSpan());
                return Encoding.UTF8.GetString(buf, 0, decoded);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buf);
            }
        }
        else
        {
            var rented = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                strBytes.CopyTo(rented.AsSpan());
                return Encoding.UTF8.GetString(rented, 0, length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }
    }

    private static ulong DecodeInteger(ReadOnlySpan<byte> block, ref int pos, int prefixBits)
    {
        var mask = (uint)((1 << prefixBits) - 1);
        var value = (ulong)(block[pos] & mask);
        pos++;

        if (value < mask)
        {
            return value;
        }

        var shift = 0;
        while (pos < block.Length)
        {
            var b = block[pos++];
            value += (ulong)(b & 0x7F) << shift;
            shift += 7;
            if ((b & 0x80) == 0)
            {
                break;
            }
        }

        return value;
    }

    private Http2Header LookupIndex(int index)
    {
        if (index <= HpackStaticTable.Count)
        {
            return HpackStaticTable.Get(index);
        }

        var dynIndex = index - HpackStaticTable.Count - 1;
        if (dynIndex >= this.m_dynamicTable.Count)
        {
            throw new Http2ConnectionException(Http2ErrorCode.CompressionError,
                $"HPACK 索引 {index} 超出动态表范围");
        }
        return this.m_dynamicTable[dynIndex];
    }

    private void InsertIntoDynamicTable(string name, string value)
    {
        var entrySize = (uint)(Encoding.UTF8.GetByteCount(name) + Encoding.UTF8.GetByteCount(value) + 32);

        this.EvictToLimit(this.m_dynamicTableSizeLimit > entrySize ? this.m_dynamicTableSizeLimit - entrySize : 0);

        if (entrySize <= this.m_dynamicTableSizeLimit)
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
