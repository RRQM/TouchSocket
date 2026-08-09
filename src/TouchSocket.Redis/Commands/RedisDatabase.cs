using System.Collections.Concurrent;

namespace TouchSocket.Redis;

/// <summary>
/// 基于内存的 Redis 数据库。
/// </summary>
public sealed class RedisDatabase
{
    private readonly ConcurrentDictionary<string, RedisEntry> m_items = new ConcurrentDictionary<string, RedisEntry>(StringComparer.Ordinal);

    /// <summary>
    /// 获取未过期的键数量。
    /// </summary>
    public int Count
    {
        get
        {
            this.SweepExpired();
            return this.m_items.Count;
        }
    }

    /// <summary>
    /// 移除全部键。
    /// </summary>
    public void Clear()
    {
        this.m_items.Clear();
    }

    /// <summary>
    /// 删除键。
    /// </summary>
    public long Delete(IList<string> keys)
    {
        long count = 0;
        for (var i = 0; i < keys.Count; i++)
        {
            if (this.m_items.TryRemove(keys[i], out _))
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 获取指定键是否存在。
    /// </summary>
    public bool Exists(string key)
    {
        if (!this.m_items.TryGetValue(key, out var entry))
        {
            return false;
        }

        if (entry.IsExpired())
        {
            this.m_items.TryRemove(key, out _);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 设置键值。
    /// </summary>
    public void Set(string key, byte[] value, TimeSpan? expiry = null)
    {
        var ticks = expiry.HasValue ? DateTime.UtcNow.Add(expiry.Value).Ticks : 0;
        this.m_items[key] = new RedisEntry(RedisEntryKind.String, value ?? Array.Empty<byte>(), ticks);
    }

    /// <summary>
    /// 尝试获取值。
    /// </summary>
    public bool TryGet(string key, out byte[] value)
    {
        value = default;
        if (!this.TryGetEntry(key, RedisEntryKind.String, out var entry))
        {
            return false;
        }

        value = (byte[])entry.Value;
        return true;
    }

    /// <summary>
    /// 增加键对应的整数值。
    /// </summary>
    public long Increment(string key, long delta)
    {
        while (true)
        {
            if (!this.TryGet(key, out var bytes))
            {
                var created = delta;
                this.Set(key, Encoding.ASCII.GetBytes(created.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                return created;
            }

            if (!RedisValue.TryParseInt64(bytes, out var current))
            {
                ThrowHelper.ThrowException("ERR value is not an integer or out of range");
            }

            var next = current + delta;
            var nextBytes = Encoding.ASCII.GetBytes(next.ToString(System.Globalization.CultureInfo.InvariantCulture));
            this.m_items[key] = new RedisEntry(RedisEntryKind.String, nextBytes, this.m_items[key].ExpireAtTicks);
            return next;
        }
    }

    /// <summary>
    /// 设置键的过期时间，单位为秒。
    /// </summary>
    public bool Expire(string key, long seconds)
    {
        if (!this.m_items.TryGetValue(key, out var entry))
        {
            return false;
        }

        if (entry.IsExpired())
        {
            this.m_items.TryRemove(key, out _);
            return false;
        }

        this.m_items[key] = new RedisEntry(entry.Kind, entry.Value, DateTime.UtcNow.AddSeconds(seconds).Ticks);
        return true;
    }

    /// <summary>
    /// 向列表头部添加元素。
    /// </summary>
    public long ListLeftPush(string key, IList<byte[]> values)
    {
        var list = this.GetOrCreateList(key);
        lock (list)
        {
            for (var i = 0; i < values.Count; i++)
            {
                list.Insert(0, values[i] ?? Array.Empty<byte>());
            }

            return list.Count;
        }
    }

    /// <summary>
    /// 向列表尾部添加元素。
    /// </summary>
    public long ListRightPush(string key, IList<byte[]> values)
    {
        var list = this.GetOrCreateList(key);
        lock (list)
        {
            for (var i = 0; i < values.Count; i++)
            {
                list.Add(values[i] ?? Array.Empty<byte>());
            }

            return list.Count;
        }
    }

    /// <summary>
    /// 获取列表长度。
    /// </summary>
    public long ListLength(string key)
    {
        if (!this.TryGetEntry(key, RedisEntryKind.List, out var entry))
        {
            return 0;
        }

        var list = (List<byte[]>)entry.Value;
        lock (list)
        {
            return list.Count;
        }
    }

    /// <summary>
    /// 获取列表范围。
    /// </summary>
    public byte[][] ListRange(string key, int start, int stop)
    {
        if (!this.TryGetEntry(key, RedisEntryKind.List, out var entry))
        {
            return Array.Empty<byte[]>();
        }

        var list = (List<byte[]>)entry.Value;
        lock (list)
        {
            if (list.Count == 0)
            {
                return Array.Empty<byte[]>();
            }

            var normalizedStart = NormalizeIndex(start, list.Count);
            var normalizedStop = NormalizeIndex(stop, list.Count);
            if (normalizedStart < 0)
            {
                normalizedStart = 0;
            }
            if (normalizedStop >= list.Count)
            {
                normalizedStop = list.Count - 1;
            }
            if (normalizedStart > normalizedStop || normalizedStart >= list.Count)
            {
                return Array.Empty<byte[]>();
            }

            var length = normalizedStop - normalizedStart + 1;
            var values = new byte[length][];
            for (var i = 0; i < length; i++)
            {
                values[i] = list[normalizedStart + i];
            }

            return values;
        }
    }

    /// <summary>
    /// 添加或更新有序集合成员。
    /// </summary>
    public long SortedSetAdd(string key, string member, double score)
    {
        var sortedSet = this.GetOrCreateSortedSet(key);
        lock (sortedSet)
        {
            var added = sortedSet.ContainsKey(member) ? 0 : 1;
            sortedSet[member] = score;
            return added;
        }
    }

    /// <summary>
    /// 获取有序集合成员数量。
    /// </summary>
    public long SortedSetLength(string key)
    {
        if (!this.TryGetEntry(key, RedisEntryKind.SortedSet, out var entry))
        {
            return 0;
        }

        var sortedSet = (Dictionary<string, double>)entry.Value;
        lock (sortedSet)
        {
            return sortedSet.Count;
        }
    }

    /// <summary>
    /// 删除有序集合成员。
    /// </summary>
    public long SortedSetRemove(string key, IList<string> members)
    {
        if (!this.TryGetEntry(key, RedisEntryKind.SortedSet, out var entry))
        {
            return 0;
        }

        var sortedSet = (Dictionary<string, double>)entry.Value;
        lock (sortedSet)
        {
            long count = 0;
            for (var i = 0; i < members.Count; i++)
            {
                if (sortedSet.Remove(members[i]))
                {
                    count++;
                }
            }

            return count;
        }
    }

    /// <summary>
    /// 获取键的剩余生存时间，单位为秒。
    /// </summary>
    public long Ttl(string key)
    {
        if (!this.m_items.TryGetValue(key, out var entry))
        {
            return -2;
        }

        if (entry.IsExpired())
        {
            this.m_items.TryRemove(key, out _);
            return -2;
        }

        if (entry.ExpireAtTicks <= 0)
        {
            return -1;
        }

        var ticks = entry.ExpireAtTicks - DateTime.UtcNow.Ticks;
        return ticks <= 0 ? -2 : (long)Math.Ceiling(TimeSpan.FromTicks(ticks).TotalSeconds);
    }

    /// <summary>
    /// 返回匹配简单 glob 表达式的键。
    /// </summary>
    public string[] Keys(string pattern)
    {
        this.SweepExpired();
        if (string.IsNullOrEmpty(pattern) || pattern == "*")
        {
            return this.m_items.Keys.ToArray();
        }

        return this.m_items.Keys.Where(key => RedisGlob.IsMatch(key, pattern)).ToArray();
    }

    private void SweepExpired()
    {
        foreach (var pair in this.m_items)
        {
            if (pair.Value.IsExpired())
            {
                this.m_items.TryRemove(pair.Key, out _);
            }
        }
    }

    private List<byte[]> GetOrCreateList(string key)
    {
        while (true)
        {
            if (!this.m_items.TryGetValue(key, out var entry) || entry.IsExpired())
            {
                if (entry != null && entry.IsExpired())
                {
                    this.m_items.TryRemove(key, out _);
                }

                var created = new RedisEntry(RedisEntryKind.List, new List<byte[]>(), 0);
                if (this.m_items.TryAdd(key, created))
                {
                    return (List<byte[]>)created.Value;
                }

                continue;
            }

            if (entry.Kind != RedisEntryKind.List)
            {
                ThrowWrongType();
            }

            return (List<byte[]>)entry.Value;
        }
    }

    private Dictionary<string, double> GetOrCreateSortedSet(string key)
    {
        while (true)
        {
            if (!this.m_items.TryGetValue(key, out var entry) || entry.IsExpired())
            {
                if (entry != null && entry.IsExpired())
                {
                    this.m_items.TryRemove(key, out _);
                }

                var created = new RedisEntry(RedisEntryKind.SortedSet, new Dictionary<string, double>(StringComparer.Ordinal), 0);
                if (this.m_items.TryAdd(key, created))
                {
                    return (Dictionary<string, double>)created.Value;
                }

                continue;
            }

            if (entry.Kind != RedisEntryKind.SortedSet)
            {
                ThrowWrongType();
            }

            return (Dictionary<string, double>)entry.Value;
        }
    }

    private bool TryGetEntry(string key, RedisEntryKind kind, out RedisEntry entry)
    {
        entry = default;
        if (!this.m_items.TryGetValue(key, out entry))
        {
            return false;
        }

        if (entry.IsExpired())
        {
            this.m_items.TryRemove(key, out _);
            return false;
        }

        if (entry.Kind != kind)
        {
            ThrowWrongType();
        }

        return true;
    }

    private static int NormalizeIndex(int index, int count)
    {
        return index < 0 ? count + index : index;
    }

    private static void ThrowWrongType()
    {
        ThrowHelper.ThrowException("WRONGTYPE Operation against a key holding the wrong kind of value");
    }

    private enum RedisEntryKind
    {
        String,
        List,
        SortedSet
    }

    private sealed class RedisEntry
    {
        public RedisEntry(RedisEntryKind kind, object value, long expireAtTicks)
        {
            this.Kind = kind;
            this.Value = value;
            this.ExpireAtTicks = expireAtTicks;
        }

        public RedisEntryKind Kind { get; }

        public object Value { get; }

        public long ExpireAtTicks { get; }

        public bool IsExpired()
        {
            return this.ExpireAtTicks > 0 && DateTime.UtcNow.Ticks >= this.ExpireAtTicks;
        }
    }
}
