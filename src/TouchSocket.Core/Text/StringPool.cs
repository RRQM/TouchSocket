// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

namespace TouchSocket.Core;

/// <summary>
/// 表示一个字符串池，用于高效管理和复用字符串。
/// </summary>
public sealed class StringPool
{
    private readonly Encoding m_encoding;
    private readonly int m_maxCapacity;

    private struct Entry
    {
        public byte[] Bytes;
        public string Value;
        public int Length;
        public byte First;
    }

    private readonly Dictionary<int, Entry[]> m_buckets;

    /// <summary>
    /// 初始化 <see cref="StringPool"/> 类的新实例。
    /// </summary>
    /// <param name="encoding">用于字符串编码的 <see cref="Encoding"/>。</param>
    /// <param name="maxCapacity">最大容量，默认为 -1 表示不限制。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="encoding"/> 为 <see langword="null"/> 时抛出。</exception>
    public StringPool(Encoding encoding, int maxCapacity = -1)
    {
        ThrowHelper.ThrowIfNull(encoding, nameof(encoding));
        this.m_encoding = encoding;
        this.m_maxCapacity = maxCapacity;
        this.m_buckets = new Dictionary<int, Entry[]>();
    }

    /// <summary>
    /// 获取与指定字节序列对应的字符串。
    /// </summary>
    /// <param name="bytes">字节序列。</param>
    /// <returns>与字节序列对应的字符串。</returns>
    public string Get(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 0)
        {
            return string.Empty;
        }

        var hash = ComputeHash(bytes);

        if (this.m_buckets.TryGetValue(hash, out var entries))
        {
            for (var i = 0; i < entries.Length; i++)
            {
                var e = entries[i];
                if (e.Length == bytes.Length && e.First == bytes[0] && bytes.SequenceEqual(e.Bytes))
                {
                    return e.Value;
                }
            }
        }

        var str = bytes.ToString(this.m_encoding);
        var keyBytes = bytes.ToArray();
        var newEntry = new Entry
        {
            Bytes = keyBytes,
            Value = str,
            Length = keyBytes.Length,
            First = keyBytes[0]
        };

        if (this.m_buckets.TryGetValue(hash, out var existingEntries))
        {
            for (var i = 0; i < existingEntries.Length; i++)
            {
                var cur = existingEntries[i];
                if (cur.Length == newEntry.Length && cur.First == newEntry.First && new ReadOnlySpan<byte>(newEntry.Bytes).SequenceEqual(cur.Bytes))
                {
                    return cur.Value;
                }
            }

            if (this.m_maxCapacity == -1 || this.m_buckets.Count < this.m_maxCapacity)
            {
                var arr = new Entry[existingEntries.Length + 1];
                Array.Copy(existingEntries, arr, existingEntries.Length);
                arr[existingEntries.Length] = newEntry;
                this.m_buckets[hash] = arr;
            }
        }
        else
        {
            if (this.m_maxCapacity == -1 || this.m_buckets.Count < this.m_maxCapacity)
            {
                this.m_buckets[hash] = new[] { newEntry };
            }
        }

        return str;
    }

    private static int ComputeHash(ReadOnlySpan<byte> data)
    {
        unchecked
        {
            const uint offset = 2166136261u;
            const uint prime = 16777619u;
            var hash = offset;
            for (var i = 0; i < data.Length; i++)
            {
                hash ^= data[i];
                hash *= prime;
            }
            return (int)hash;
        }
    }
}
