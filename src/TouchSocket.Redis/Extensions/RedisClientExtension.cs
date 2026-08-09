namespace TouchSocket.Redis;

/// <summary>
/// Redis 客户端命令扩展。
/// </summary>
public static class RedisClientExtension
{
    /// <summary>
    /// 使用 UTF-8 字符串参数执行 Redis 命令。
    /// </summary>
    public static Task<RedisValue> ExecuteAsync(this IRedisClient client, CancellationToken cancellationToken, params string[] args)
    {
        return client.ExecuteAsync(RedisValue.Command(args), cancellationToken);
    }

    /// <summary>
    /// 使用 UTF-8 字符串参数执行 Redis 命令。
    /// </summary>
    public static Task<RedisValue> ExecuteAsync(this IRedisClient client, params string[] args)
    {
        return client.ExecuteAsync(RedisValue.Command(args), CancellationToken.None);
    }

    /// <summary>
    /// 发送 PING 命令。
    /// </summary>
    public static Task<RedisValue> PingAsync(this IRedisClient client, CancellationToken cancellationToken = default)
    {
        return client.ExecuteAsync(RedisValue.Command("PING"), cancellationToken);
    }

    /// <summary>
    /// 发送 ECHO 命令。
    /// </summary>
    public static async Task<string> EchoAsync(this IRedisClient client, string value, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(RedisValue.Command("ECHO", value), cancellationToken).ConfigureDefaultAwait();
        return response.AsString();
    }

    /// <summary>
    /// 发送 QUIT 命令。
    /// </summary>
    public static async Task<string> QuitAsync(this IRedisClient client, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(RedisValue.Command("QUIT"), cancellationToken).ConfigureDefaultAwait();
        return response.AsString();
    }

    /// <summary>
    /// 执行 Redis 命令并返回字符串结果。
    /// </summary>
    public static async Task<string> ExecuteForStringResultAsync(this IRedisClient client, string command, ICollection<string> args = null, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(CreateCommand(command, args), cancellationToken).ConfigureDefaultAwait();
        return response.AsString();
    }

    /// <summary>
    /// 执行 Redis 命令并返回整数结果。
    /// </summary>
    public static async Task<long> ExecuteForLongResultAsync(this IRedisClient client, string command, ICollection<string> args = null, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(CreateCommand(command, args), cancellationToken).ConfigureDefaultAwait();
        return response.Integer;
    }

    /// <summary>
    /// 执行 Redis 命令并返回字符串数组结果。
    /// </summary>
    public static async Task<string[]> ExecuteForStringArrayResultAsync(this IRedisClient client, string command, ICollection<string> args = null, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(CreateCommand(command, args), cancellationToken).ConfigureDefaultAwait();
        return ToStringArray(response);
    }

    /// <summary>
    /// 执行 Redis 命令并返回字节内存结果。返回内存由响应对象持有，不需要释放。
    /// </summary>
    public static async Task<ReadOnlyMemory<byte>> ExecuteForMemoryResultAsync(this IRedisClient client, string command, ICollection<string> args = null, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(CreateCommand(command, args), cancellationToken).ConfigureDefaultAwait();
        return response.Kind == RedisValueKind.BulkString ? response.AsBytes() : ReadOnlyMemory<byte>.Empty;
    }

    /// <summary>
    /// 执行 Redis 命令并返回字节内存数组结果。返回内存由响应对象持有，不需要释放。
    /// </summary>
    public static async Task<ReadOnlyMemory<byte>[]> ExecuteForMemoryResultArrayAsync(this IRedisClient client, string command, ICollection<string> args = null, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(CreateCommand(command, args), cancellationToken).ConfigureDefaultAwait();
        return ToMemoryArray(response);
    }

    /// <summary>
    /// 获取字符串键值。
    /// </summary>
    public static async Task<string> GetStringAsync(this IRedisClient client, string key, CancellationToken cancellationToken = default)
    {
        var value = await client.ExecuteAsync(RedisValue.Command("GET", key), cancellationToken).ConfigureDefaultAwait();
        return value.AsString();
    }

    /// <summary>
    /// 以字节数组获取键值。
    /// </summary>
    public static async Task<byte[]> GetBytesAsync(this IRedisClient client, string key, CancellationToken cancellationToken = default)
    {
        var value = await client.ExecuteAsync(RedisValue.Command("GET", key), cancellationToken).ConfigureDefaultAwait();
        return value.Kind == RedisValueKind.BulkString ? value.AsBytes().ToArray() : null;
    }

    /// <summary>
    /// 设置字符串键值。
    /// </summary>
    public static async Task SetAsync(this IRedisClient client, string key, string value, CancellationToken cancellationToken = default)
    {
        await client.ExecuteAsync(RedisValue.Command("SET", key, value), cancellationToken).ConfigureDefaultAwait();
    }

    /// <summary>
    /// 设置字符串键值。
    /// </summary>
    public static async Task<bool> StringSetAsync(this IRedisClient client, string key, string value, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(RedisValue.Command("SET", key, value), cancellationToken).ConfigureDefaultAwait();
        return response.AsString() == "OK";
    }

    /// <summary>
    /// 设置字节内存键值，发送时不复制 key 和 value。
    /// </summary>
    public static async Task<bool> StringSetAsync(this IRedisClient client, ReadOnlyMemory<byte> key, ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(RedisValue.Command("SET", key, value), cancellationToken).ConfigureDefaultAwait();
        return response.AsString() == "OK";
    }

    /// <summary>
    /// 获取字符串键值。
    /// </summary>
    public static Task<string> StringGetAsync(this IRedisClient client, string key, CancellationToken cancellationToken = default)
    {
        return client.GetStringAsync(key, cancellationToken);
    }

    /// <summary>
    /// 批量获取字符串键值。
    /// </summary>
    public static async Task<string[]> StringGetAsync(this IRedisClient client, string[] keys, CancellationToken cancellationToken = default)
    {
        var args = new string[keys.Length + 1];
        args[0] = "MGET";
        Array.Copy(keys, 0, args, 1, keys.Length);
        var response = await client.ExecuteAsync(RedisValue.Command(args), cancellationToken).ConfigureDefaultAwait();
        return ToStringArray(response);
    }

    /// <summary>
    /// 获取字节内存键值。返回内存由响应对象持有，不需要释放。
    /// </summary>
    public static async Task<ReadOnlyMemory<byte>> StringGetAsMemoryAsync(this IRedisClient client, string key, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(RedisValue.Command("GET", key), cancellationToken).ConfigureDefaultAwait();
        return response.Kind == RedisValueKind.BulkString ? response.AsBytes() : ReadOnlyMemory<byte>.Empty;
    }

    /// <summary>
    /// 获取字节内存键值。返回内存由响应对象持有，不需要释放。
    /// </summary>
    public static async Task<ReadOnlyMemory<byte>> StringGetAsMemoryAsync(this IRedisClient client, ReadOnlyMemory<byte> key, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(RedisValue.Command("GET", key), cancellationToken).ConfigureDefaultAwait();
        return response.Kind == RedisValueKind.BulkString ? response.AsBytes() : ReadOnlyMemory<byte>.Empty;
    }

    /// <summary>
    /// 批量获取字节内存键值。返回内存由响应对象持有，不需要释放。
    /// </summary>
    public static async Task<ReadOnlyMemory<byte>[]> StringGetAsMemoryAsync(this IRedisClient client, string[] keys, CancellationToken cancellationToken = default)
    {
        var args = new string[keys.Length + 1];
        args[0] = "MGET";
        Array.Copy(keys, 0, args, 1, keys.Length);
        var response = await client.ExecuteAsync(RedisValue.Command(args), cancellationToken).ConfigureDefaultAwait();
        return ToMemoryArray(response);
    }

    /// <summary>
    /// 增加键对应的整数值。
    /// </summary>
    public static Task<long> StringIncrementAsync(this IRedisClient client, string key, CancellationToken cancellationToken = default)
    {
        return client.ExecuteForLongResultAsync("INCR", new[] { key }, cancellationToken);
    }

    /// <summary>
    /// 增加键对应的整数值。
    /// </summary>
    public static Task<long> StringIncrementAsync(this IRedisClient client, string key, long value, CancellationToken cancellationToken = default)
    {
        return client.ExecuteForLongResultAsync("INCRBY", new[] { key, value.ToString(System.Globalization.CultureInfo.InvariantCulture) }, cancellationToken);
    }

    /// <summary>
    /// 增加键对应的整数值。
    /// </summary>
    public static Task<long> StringIncrement(this IRedisClient client, string key, CancellationToken cancellationToken = default)
    {
        return client.StringIncrementAsync(key, cancellationToken);
    }

    /// <summary>
    /// 增加键对应的整数值。
    /// </summary>
    public static Task<long> StringIncrement(this IRedisClient client, string key, long value, CancellationToken cancellationToken = default)
    {
        return client.StringIncrementAsync(key, value, cancellationToken);
    }

    /// <summary>
    /// 增加键对应的整数值。
    /// </summary>
    public static async Task<long> StringIncrement(this IRedisClient client, ReadOnlyMemory<byte> key, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(RedisValue.Command("INCR", key), cancellationToken).ConfigureDefaultAwait();
        return response.Integer;
    }

    /// <summary>
    /// 增加键对应的整数值。
    /// </summary>
    public static async Task<long> StringIncrement(this IRedisClient client, ReadOnlyMemory<byte> key, long value, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(CreateCommand("INCRBY", key, value.ToString(System.Globalization.CultureInfo.InvariantCulture)), cancellationToken).ConfigureDefaultAwait();
        return response.Integer;
    }

    /// <summary>
    /// 减少键对应的整数值。
    /// </summary>
    public static Task<long> StringDecrementAsync(this IRedisClient client, string key, CancellationToken cancellationToken = default)
    {
        return client.ExecuteForLongResultAsync("DECR", new[] { key }, cancellationToken);
    }

    /// <summary>
    /// 减少键对应的整数值。
    /// </summary>
    public static Task<long> StringDecrementAsync(this IRedisClient client, string key, long value, CancellationToken cancellationToken = default)
    {
        return client.ExecuteForLongResultAsync("DECRBY", new[] { key, value.ToString(System.Globalization.CultureInfo.InvariantCulture) }, cancellationToken);
    }

    /// <summary>
    /// 减少键对应的整数值。
    /// </summary>
    public static Task<long> StringDecrement(this IRedisClient client, string key, CancellationToken cancellationToken = default)
    {
        return client.StringDecrementAsync(key, cancellationToken);
    }

    /// <summary>
    /// 减少键对应的整数值。
    /// </summary>
    public static Task<long> StringDecrement(this IRedisClient client, string key, long value, CancellationToken cancellationToken = default)
    {
        return client.StringDecrementAsync(key, value, cancellationToken);
    }

    /// <summary>
    /// 减少键对应的整数值。
    /// </summary>
    public static async Task<long> StringDecrement(this IRedisClient client, ReadOnlyMemory<byte> key, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(RedisValue.Command("DECR", key), cancellationToken).ConfigureDefaultAwait();
        return response.Integer;
    }

    /// <summary>
    /// 减少键对应的整数值。
    /// </summary>
    public static async Task<long> StringDecrement(this IRedisClient client, ReadOnlyMemory<byte> key, long value, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(CreateCommand("DECRBY", key, value.ToString(System.Globalization.CultureInfo.InvariantCulture)), cancellationToken).ConfigureDefaultAwait();
        return response.Integer;
    }

    /// <summary>
    /// 批量设置字符串键值。
    /// </summary>
    public static async Task MSetAsync(this IRedisClient client, IDictionary<string, string> values, CancellationToken cancellationToken = default)
    {
        var args = new string[(values.Count * 2) + 1];
        args[0] = "MSET";
        var index = 1;
        foreach (var pair in values)
        {
            args[index++] = pair.Key;
            args[index++] = pair.Value;
        }

        await client.ExecuteAsync(RedisValue.Command(args), cancellationToken).ConfigureDefaultAwait();
    }

    /// <summary>
    /// 批量获取字符串键值。
    /// </summary>
    public static Task<string[]> MGetAsync(this IRedisClient client, string[] keys, CancellationToken cancellationToken = default)
    {
        return client.StringGetAsync(keys, cancellationToken);
    }

    /// <summary>
    /// 获取键是否存在。
    /// </summary>
    public static async Task<long> ExistsAsync(this IRedisClient client, CancellationToken cancellationToken, params string[] keys)
    {
        var args = new string[keys.Length + 1];
        args[0] = "EXISTS";
        Array.Copy(keys, 0, args, 1, keys.Length);
        var response = await client.ExecuteAsync(RedisValue.Command(args), cancellationToken).ConfigureDefaultAwait();
        return response.Integer;
    }

    /// <summary>
    /// 获取键是否存在。
    /// </summary>
    public static Task<long> ExistsAsync(this IRedisClient client, params string[] keys)
    {
        return client.ExistsAsync(CancellationToken.None, keys);
    }

    /// <summary>
    /// 删除键。
    /// </summary>
    public static Task<long> DelAsync(this IRedisClient client, params string[] keys)
    {
        return client.DelAsync(CancellationToken.None, keys);
    }

    /// <summary>
    /// 删除键。
    /// </summary>
    public static async Task<long> DelAsync(this IRedisClient client, CancellationToken cancellationToken, params string[] keys)
    {
        var args = new string[keys.Length + 1];
        args[0] = "DEL";
        Array.Copy(keys, 0, args, 1, keys.Length);
        var value = await client.ExecuteAsync(RedisValue.Command(args), cancellationToken).ConfigureDefaultAwait();
        return value.Integer;
    }

    /// <summary>
    /// 删除单个键。
    /// </summary>
    public static async Task<bool> KeyDeleteAsync(this IRedisClient client, string key, CancellationToken cancellationToken = default)
    {
        return await client.DelAsync(cancellationToken, key).ConfigureDefaultAwait() > 0;
    }

    /// <summary>
    /// 删除单个字节内存键。
    /// </summary>
    public static async Task<bool> KeyDeleteAsync(this IRedisClient client, ReadOnlyMemory<byte> key, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(RedisValue.Command("DEL", key), cancellationToken).ConfigureDefaultAwait();
        return response.Integer > 0;
    }

    /// <summary>
    /// 删除多个键。
    /// </summary>
    public static Task<long> KeyDeleteAsync(this IRedisClient client, string[] keys, CancellationToken cancellationToken = default)
    {
        return client.DelAsync(cancellationToken, keys);
    }

    /// <summary>
    /// 设置键的过期时间。
    /// </summary>
    public static async Task<bool> ExpireAsync(this IRedisClient client, string key, long seconds, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(RedisValue.Command("EXPIRE", key, seconds.ToString(System.Globalization.CultureInfo.InvariantCulture)), cancellationToken).ConfigureDefaultAwait();
        return response.Integer > 0;
    }

    /// <summary>
    /// 获取键的剩余生存时间。
    /// </summary>
    public static async Task<long> TtlAsync(this IRedisClient client, string key, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(RedisValue.Command("TTL", key), cancellationToken).ConfigureDefaultAwait();
        return response.Integer;
    }

    /// <summary>
    /// 获取匹配模式的键。
    /// </summary>
    public static async Task<string[]> KeysAsync(this IRedisClient client, string pattern, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(RedisValue.Command("KEYS", pattern), cancellationToken).ConfigureDefaultAwait();
        return ToStringArray(response);
    }

    /// <summary>
    /// 获取当前数据库键数量。
    /// </summary>
    public static async Task<long> DbSizeAsync(this IRedisClient client, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(RedisValue.Command("DBSIZE"), cancellationToken).ConfigureDefaultAwait();
        return response.Integer;
    }

    /// <summary>
    /// 清空当前数据库。
    /// </summary>
    public static async Task FlushDbAsync(this IRedisClient client, CancellationToken cancellationToken = default)
    {
        await client.ExecuteAsync(RedisValue.Command("FLUSHDB"), cancellationToken).ConfigureDefaultAwait();
    }

    /// <summary>
    /// 获取服务端信息。
    /// </summary>
    public static Task<string> InfoAsync(this IRedisClient client, CancellationToken cancellationToken = default)
    {
        return client.ExecuteForStringResultAsync("INFO", null, cancellationToken);
    }

    /// <summary>
    /// 请求服务端保存数据。
    /// </summary>
    public static async Task<bool> SaveAsync(this IRedisClient client, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(RedisValue.Command("SAVE"), cancellationToken).ConfigureDefaultAwait();
        return response.AsString() == "OK";
    }

    /// <summary>
    /// 设置当前节点的主节点。
    /// </summary>
    public static Task<string> ReplicaOfAsync(this IRedisClient client, string address, int port, CancellationToken cancellationToken = default)
    {
        return client.ExecuteForStringResultAsync("REPLICAOF", new[] { address, port.ToString(System.Globalization.CultureInfo.InvariantCulture) }, cancellationToken);
    }

    /// <summary>
    /// 请求服务端执行故障转移。
    /// </summary>
    public static async Task<bool> FailoverAsync(this IRedisClient client, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(RedisValue.Command("FAILOVER"), cancellationToken).ConfigureDefaultAwait();
        return response.AsString() == "OK";
    }

    /// <summary>
    /// 将元素添加到列表头部。
    /// </summary>
    public static Task<long> ListLeftPushAsync(this IRedisClient client, string key, params string[] elements)
    {
        return client.ListLeftPushAsync(key, CancellationToken.None, elements);
    }

    /// <summary>
    /// 将元素添加到列表头部。
    /// </summary>
    public static Task<long> ListLeftPushAsync(this IRedisClient client, string key, CancellationToken cancellationToken, params string[] elements)
    {
        return client.ExecuteForLongResultAsync("LPUSH", Combine(key, elements), cancellationToken);
    }

    /// <summary>
    /// 将元素添加到列表尾部。
    /// </summary>
    public static Task<long> ListRightPushAsync(this IRedisClient client, string key, params string[] elements)
    {
        return client.ListRightPushAsync(key, CancellationToken.None, elements);
    }

    /// <summary>
    /// 将元素添加到列表尾部。
    /// </summary>
    public static Task<long> ListRightPushAsync(this IRedisClient client, string key, CancellationToken cancellationToken, params string[] elements)
    {
        return client.ExecuteForLongResultAsync("RPUSH", Combine(key, elements), cancellationToken);
    }

    /// <summary>
    /// 获取列表长度。
    /// </summary>
    public static Task<long> ListLengthAsync(this IRedisClient client, string key, CancellationToken cancellationToken = default)
    {
        return client.ExecuteForLongResultAsync("LLEN", new[] { key }, cancellationToken);
    }

    /// <summary>
    /// 获取列表范围。
    /// </summary>
    public static async Task<string[]> ListRangeAsync(this IRedisClient client, string key, int start, int stop, CancellationToken cancellationToken = default)
    {
        var response = await client.ExecuteAsync(RedisValue.Command("LRANGE", key, start.ToString(System.Globalization.CultureInfo.InvariantCulture), stop.ToString(System.Globalization.CultureInfo.InvariantCulture)), cancellationToken).ConfigureDefaultAwait();
        return ToStringArray(response);
    }

    /// <summary>
    /// 添加或更新有序集合成员。
    /// </summary>
    public static Task<long> SortedSetAddAsync(this IRedisClient client, string key, string member, double score, CancellationToken cancellationToken = default)
    {
        return client.ExecuteForLongResultAsync("ZADD", new[] { key, score.ToString(System.Globalization.CultureInfo.InvariantCulture), member }, cancellationToken);
    }

    /// <summary>
    /// 批量添加或更新有序集合成员。
    /// </summary>
    public static Task<long> SortedSetAddAsync(this IRedisClient client, string key, IDictionary<string, double> values, CancellationToken cancellationToken = default)
    {
        var args = new string[(values.Count * 2) + 1];
        args[0] = key;
        var index = 1;
        foreach (var pair in values)
        {
            args[index++] = pair.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            args[index++] = pair.Key;
        }

        return client.ExecuteForLongResultAsync("ZADD", args, cancellationToken);
    }

    /// <summary>
    /// 获取有序集合长度。
    /// </summary>
    public static Task<long> SortedSetLengthAsync(this IRedisClient client, string key, CancellationToken cancellationToken = default)
    {
        return client.ExecuteForLongResultAsync("ZCARD", new[] { key }, cancellationToken);
    }

    /// <summary>
    /// 删除有序集合成员。
    /// </summary>
    public static Task<long> SortedSetRemoveAsync(this IRedisClient client, string key, string member, CancellationToken cancellationToken = default)
    {
        return client.ExecuteForLongResultAsync("ZREM", new[] { key, member }, cancellationToken);
    }

    private static RedisValue CreateCommand(string command, ICollection<string> args)
    {
        if (args is null || args.Count == 0)
        {
            return RedisValue.Command(command);
        }

        var values = new string[args.Count + 1];
        values[0] = command;
        args.CopyTo(values, 1);
        return RedisValue.Command(values);
    }

    private static RedisValue CreateCommand(string command, ReadOnlyMemory<byte> first, string second)
    {
        return RedisValue.Array(RedisValue.BulkString(command), RedisValue.BulkString(first), RedisValue.BulkString(second));
    }

    private static string[] Combine(string first, string[] values)
    {
        var args = new string[values.Length + 1];
        args[0] = first;
        Array.Copy(values, 0, args, 1, values.Length);
        return args;
    }

    private static string[] ToStringArray(RedisValue value)
    {
        if (value.Kind != RedisValueKind.Array)
        {
            return Array.Empty<string>();
        }

        var array = new string[value.Items.Count];
        for (var i = 0; i < array.Length; i++)
        {
            array[i] = value.Items[i].AsString();
        }

        return array;
    }

    private static ReadOnlyMemory<byte>[] ToMemoryArray(RedisValue value)
    {
        if (value.Kind != RedisValueKind.Array)
        {
            return Array.Empty<ReadOnlyMemory<byte>>();
        }

        var array = new ReadOnlyMemory<byte>[value.Items.Count];
        for (var i = 0; i < array.Length; i++)
        {
            array[i] = value.Items[i].Kind == RedisValueKind.BulkString ? value.Items[i].AsBytes() : ReadOnlyMemory<byte>.Empty;
        }

        return array;
    }
}
