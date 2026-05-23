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

namespace TouchSocket.Http;

/// <summary>
/// HPACK Huffman 编解码，见 RFC 7541 Appendix B
/// </summary>
internal static class HpackHuffman
{
    // 编码用：MSB 对齐的码字（左移到 uint 高位），共 256 个字节符号
    private static readonly uint[] s_encodeCodes = new uint[256];
    // 编码用：码长（位数）
    private static readonly byte[] s_encodeLengths = new byte[256];
    // 解码用：二叉树，每节点两个子槽位 [node*2+bit]；负值 ~sym 表示叶子
    private static readonly int[] s_decodeTree;

    static HpackHuffman()
    {
        // RFC 7541 Appendix B 完整码表，格式 (右对齐码值, 位长)，共 257 个符号（0-255 + EOS=256）
        var rawCodes = new uint[]
        {
            0x1ff8, 0x7fffd8, 0xfffffe2, 0xfffffe3, 0xfffffe4, 0xfffffe5,
            0xfffffe6, 0xfffffe7, 0xfffffe8, 0xffffea, 0x3ffffffc, 0xfffffe9,
            0xfffffea, 0x3ffffffd, 0xfffffeb, 0xfffffec, 0xfffffed, 0xfffffee,
            0xfffffef, 0xffffff0, 0xffffff1, 0xffffff2, 0x3ffffffe, 0xffffff3,
            0xffffff4, 0xffffff5, 0xffffff6, 0xffffff7, 0xffffff8, 0xffffff9,
            0xffffffa, 0xffffffb,
            0x14, 0x3f8, 0x3f9, 0xffa, 0x1ff9, 0x15, 0xf8, 0x7fa,
            0x3fa, 0x3fb, 0xf9, 0x7fb, 0xfa, 0x16, 0x17, 0x18,
            0x0, 0x1, 0x2, 0x19, 0x1a, 0x1b, 0x1c, 0x1d,
            0x1e, 0x1f, 0x5c, 0xfb, 0x7ffc, 0x20, 0xffb, 0x3fc,
            0x1ffa, 0x21, 0x5d, 0x5e, 0x5f, 0x60, 0x61, 0x62,
            0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6a,
            0x6b, 0x6c, 0x6d, 0x6e, 0x6f, 0x70, 0x71, 0x72,
            0xfc, 0x73, 0xfd, 0x1ffb, 0x7fff0, 0x1ffc, 0x3ffc, 0x22,
            0x7ffd, 0x3, 0x23, 0x4, 0x24, 0x5, 0x25, 0x26,
            0x27, 0x6, 0x74, 0x75, 0x28, 0x29, 0x2a, 0x7,
            0x2b, 0x76, 0x2c, 0x8, 0x9, 0x2d, 0x77, 0x78,
            0x79, 0x7a, 0x7b, 0x7ffe, 0x7fc, 0x3ffd, 0x1ffd, 0xffffffc,
            0xfffe6, 0x3fffd2, 0xfffe7, 0xfffe8, 0x3fffd3, 0x3fffd4,
            0x3fffd5, 0x7fffd9, 0x3fffd6, 0x7fffda, 0x7fffdb, 0x7fffdc,
            0x7fffdd, 0x7fffde, 0xffffeb, 0x7fffdf, 0xffffec, 0xffffed,
            0x3fffd7, 0x7fffe0, 0xffffee, 0x7fffe1, 0x7fffe2, 0x7fffe3,
            0x7fffe4, 0x1fffdc, 0x3fffd8, 0x7fffe5, 0x3fffd9, 0x7fffe6,
            0x7fffe7, 0xffffef, 0x3fffda, 0x1fffdd, 0xfffe9, 0x3fffdb,
            0x3fffdc, 0x7fffe8, 0x7fffe9, 0x1fffde, 0x7fffea, 0x3fffdd,
            0x3fffde, 0xfffff0, 0x1fffdf, 0x3fffdf, 0x7fffeb, 0x7fffec,
            0x1fffe0, 0x1fffe1, 0x3fffe0, 0x1fffe2, 0x7fffed, 0x3fffe1,
            0x7fffee, 0x7fffef, 0xfffea, 0x3fffe2, 0x3fffe3, 0x3fffe4,
            0x7ffff0, 0x3fffe5, 0x3fffe6, 0x7ffff1, 0x3ffffe0, 0x3ffffe1,
            0xfffeb, 0x7fff1, 0x3fffe7, 0x7ffff2, 0x3fffe8, 0x1ffffec,
            0x3ffffe2, 0x3ffffe3, 0x3ffffe4, 0x7ffffde, 0x7ffffdf,
            0x3ffffe5, 0xfffff1, 0x1ffffed, 0x7fff2, 0x1fffe3,
            0x3ffffe6, 0x7ffffe0, 0x7ffffe1, 0x3ffffe7, 0x7ffffe2,
            0xfffff2, 0x1fffe4, 0x1fffe5, 0x3ffffe8, 0x3ffffe9,
            0xffffffd, 0x7ffffe3, 0x7ffffe4, 0x7ffffe5, 0xfffec,
            0xfffff3, 0xfffed, 0x1fffe6, 0x3fffe9, 0x1fffe7,
            0x1fffe8, 0x7ffff3, 0x3fffea, 0x3fffeb, 0x1ffffee,
            0x1ffffef, 0xfffff4, 0xfffff5, 0x3ffffea, 0x7ffff4,
            0x3ffffeb, 0x7ffffe6, 0x3ffffec, 0x3ffffed, 0x7ffffe7,
            0x7ffffe8, 0x7ffffe9, 0x7ffffea, 0x7ffffeb, 0xffffffe,
            0x7ffffec, 0x7ffffed, 0x7ffffee, 0x7ffffef, 0x7fffff0,
            0x3ffffee,   // 255
            0x3fffffff,   // 256 EOS
        };

        var rawLengths = new byte[]
        {
            13, 23, 28, 28, 28, 28, 28, 28, 28, 24, 30, 28, 28, 30, 28, 28,
            28, 28, 28, 28, 28, 28, 30, 28, 28, 28, 28, 28, 28, 28, 28, 28,
             6, 10, 10, 12, 13,  6,  8, 11, 10, 10,  8, 11,  8,  6,  6,  6,
             5,  5,  5,  6,  6,  6,  6,  6,  6,  6,  7,  8, 15,  6, 12, 10,
            13,  6,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,
             7,  7,  7,  7,  7,  7,  7,  7,  8,  7,  8, 13, 19, 13, 14,  6,
            15,  5,  6,  5,  6,  5,  6,  6,  6,  5,  7,  7,  6,  6,  6,  5,
             6,  7,  6,  5,  5,  6,  7,  7,  7,  7,  7, 15, 11, 14, 13, 28,
            20, 22, 20, 20, 22, 22, 22, 23, 22, 23, 23, 23, 23, 23, 24, 23,
            24, 24, 22, 23, 24, 23, 23, 23, 23, 21, 22, 23, 22, 23, 23, 24,
            22, 21, 20, 22, 22, 23, 23, 21, 23, 22, 22, 24, 21, 22, 23, 23,
            21, 21, 22, 21, 23, 22, 23, 23, 20, 22, 22, 22, 23, 22, 22, 23,
            26, 26, 20, 19, 22, 23, 22, 25, 26, 26, 26, 27, 27, 26, 24, 25,
            19, 21, 26, 27, 27, 26, 27, 24, 21, 21, 26, 26, 28, 27, 27, 27,
            20, 24, 20, 21, 22, 21, 21, 23, 22, 22, 25, 25, 24, 24, 26, 23,
            26, 27, 26, 26, 27, 27, 27, 27, 27, 28, 27, 27, 27, 27, 27,
            26,   // 255
            30,   // 256 EOS
        };

        // 初始化编码表（MSB 对齐）
        for (var i = 0; i < 256; i++)
        {
            s_encodeLengths[i] = rawLengths[i];
            s_encodeCodes[i] = rawCodes[i] << (32 - rawLengths[i]);
        }

        // 构建解码树
        s_decodeTree = BuildDecodeTree(rawCodes, rawLengths);
    }

    /// <summary>
    /// Huffman 编码：将字节序列写入目标缓冲区，返回写入字节数
    /// </summary>
    public static int Encode(ReadOnlySpan<byte> input, Span<byte> output)
    {
        ulong accumulator = 0;
        var bitsInAccumulator = 0;
        var outputIndex = 0;

        foreach (var b in input)
        {
            var code = s_encodeCodes[b];
            var length = s_encodeLengths[b];

            accumulator |= (ulong)code << (32 - bitsInAccumulator);
            bitsInAccumulator += length;

            while (bitsInAccumulator >= 8)
            {
                output[outputIndex++] = (byte)(accumulator >> 56);
                accumulator <<= 8;
                bitsInAccumulator -= 8;
            }
        }

        if (bitsInAccumulator > 0)
        {
            accumulator |= (0xFFFFFFFFFFFFFF00uL >> bitsInAccumulator);
            output[outputIndex++] = (byte)(accumulator >> 56);
        }

        return outputIndex;
    }

    /// <summary>
    /// 计算 Huffman 编码后字节数（不实际编码）
    /// </summary>
    public static int GetEncodedLength(ReadOnlySpan<byte> input)
    {
        var totalBits = 0;
        foreach (var b in input)
        {
            totalBits += s_encodeLengths[b];
        }
        return (totalBits + 7) / 8;
    }

    /// <summary>
    /// Huffman 解码：将 Huffman 编码字节还原为原始字节，返回解码字节数
    /// </summary>
    /// <exception cref="Http2ConnectionException">解码失败时抛出</exception>
    public static int Decode(ReadOnlySpan<byte> input, Span<byte> output)
    {
        var node = 0;
        var outputIndex = 0;

        foreach (var b in input)
        {
            for (var bit = 7; bit >= 0; bit--)
            {
                var direction = (b >> bit) & 1;
                node = s_decodeTree[node * 2 + direction];

                if (node < 0)
                {
                    var symbol = ~node;
                    if (symbol == 256)
                    {
                        return outputIndex;
                    }
                    if (outputIndex >= output.Length)
                    {
                        throw new Http2ConnectionException(Http2ErrorCode.CompressionError, "Huffman 解码缓冲区溢出");
                    }
                    output[outputIndex++] = (byte)symbol;
                    node = 0;
                }
            }
        }

        return outputIndex;
    }

    private static int[] BuildDecodeTree(uint[] codes, byte[] lengths)
    {
        var tree = new List<int>(1024);
        // 根节点：两个子槽位（[0]=0分支, [1]=1分支），初始 -2 表示未设置的内部节点
        tree.Add(-2);
        tree.Add(-2);

        for (var sym = 0; sym < 257; sym++)
        {
            var code = codes[sym];
            var length = lengths[sym];
            if (length == 0) continue;

            var node = 0;
            for (var i = length - 1; i >= 0; i--)
            {
                var bit = (int)((code >> i) & 1);
                var childIndex = node * 2 + bit;

                // 扩展 tree 容量
                while (childIndex >= tree.Count)
                {
                    tree.Add(-2);
                }

                if (i == 0)
                {
                    // 叶子节点
                    tree[childIndex] = ~sym;
                }
                else
                {
                    if (tree[childIndex] == -2)
                    {
                        // 新内部节点
                        var newNodeIndex = tree.Count / 2;
                        tree.Add(-2);
                        tree.Add(-2);
                        tree[childIndex] = newNodeIndex;
                        node = newNodeIndex;
                    }
                    else if (tree[childIndex] >= 0)
                    {
                        node = tree[childIndex];
                    }
                    else
                    {
                        // 内部节点冲突（理论上不应发生）
                        throw new InvalidOperationException("Huffman 树构建失败：节点冲突");
                    }
                }
            }
        }

        return tree.ToArray();
    }
}
