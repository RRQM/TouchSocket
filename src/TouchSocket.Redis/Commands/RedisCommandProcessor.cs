namespace TouchSocket.Redis;

/// <summary>
/// 为 <see cref="RedisService"/> 处理 Redis 命令。
/// </summary>
public class RedisCommandProcessor
{
    /// <summary>
    /// 处理 RESP 命令。
    /// </summary>
    public virtual RedisValue Process(RedisSessionClient session, RedisDatabase database, RedisValue request, RedisServerOption option = null)
    {
        if (request.Kind != RedisValueKind.Array || request.Items.Count == 0)
        {
            return RedisValue.Error("ERR Protocol error: expected array command");
        }

        try
        {
            if (request.ArgumentEquals(0, "AUTH"))
            {
                return this.Auth(session, request, option);
            }

            if (option?.Password is not null && !session.IsAuthenticated)
            {
                return RedisValue.Error("NOAUTH Authentication required.");
            }

            if (request.ArgumentEquals(0, "PING"))
            {
                return this.Ping(request);
            }
            if (request.ArgumentEquals(0, "ECHO"))
            {
                return this.Echo(request);
            }
            if (request.ArgumentEquals(0, "GET"))
            {
                return this.Get(database, request);
            }
            if (request.ArgumentEquals(0, "SET"))
            {
                return this.Set(database, request);
            }
            if (request.ArgumentEquals(0, "DEL"))
            {
                return this.Delete(database, request);
            }
            if (request.ArgumentEquals(0, "EXISTS"))
            {
                return this.Exists(database, request);
            }
            if (request.ArgumentEquals(0, "INCR"))
            {
                return this.Increment(database, request, 1);
            }
            if (request.ArgumentEquals(0, "DECR"))
            {
                return this.Increment(database, request, -1);
            }
            if (request.ArgumentEquals(0, "INCRBY"))
            {
                return this.IncrementBy(database, request, 1);
            }
            if (request.ArgumentEquals(0, "DECRBY"))
            {
                return this.IncrementBy(database, request, -1);
            }
            if (request.ArgumentEquals(0, "MGET"))
            {
                return this.MGet(database, request);
            }
            if (request.ArgumentEquals(0, "MSET"))
            {
                return this.MSet(database, request);
            }
            if (request.ArgumentEquals(0, "EXPIRE"))
            {
                return this.Expire(database, request);
            }
            if (request.ArgumentEquals(0, "TTL"))
            {
                return this.Ttl(database, request);
            }
            if (request.ArgumentEquals(0, "KEYS"))
            {
                return this.Keys(database, request);
            }
            if (request.ArgumentEquals(0, "LPUSH"))
            {
                return this.ListPush(database, request, true);
            }
            if (request.ArgumentEquals(0, "RPUSH"))
            {
                return this.ListPush(database, request, false);
            }
            if (request.ArgumentEquals(0, "LLEN"))
            {
                return this.ListLength(database, request);
            }
            if (request.ArgumentEquals(0, "LRANGE"))
            {
                return this.ListRange(database, request);
            }
            if (request.ArgumentEquals(0, "ZADD"))
            {
                return this.SortedSetAdd(database, request);
            }
            if (request.ArgumentEquals(0, "ZCARD"))
            {
                return this.SortedSetLength(database, request);
            }
            if (request.ArgumentEquals(0, "ZREM"))
            {
                return this.SortedSetRemove(database, request);
            }
            if (request.ArgumentEquals(0, "DBSIZE"))
            {
                return RedisValue.IntegerValue(database.Count);
            }
            if (request.ArgumentEquals(0, "FLUSHDB"))
            {
                database.Clear();
                return RedisValue.Ok;
            }
            if (request.ArgumentEquals(0, "SELECT"))
            {
                return option?.AcceptSelectCommand != false ? RedisValue.Ok : RedisValue.Error("ERR SELECT is disabled");
            }
            if (request.ArgumentEquals(0, "INFO"))
            {
                return this.Info(request);
            }
            if (request.ArgumentEquals(0, "SAVE"))
            {
                return RedisValue.Ok;
            }
            if (request.ArgumentEquals(0, "REPLICAOF"))
            {
                return RedisValue.Ok;
            }
            if (request.ArgumentEquals(0, "FAILOVER"))
            {
                return RedisValue.Ok;
            }
            if (request.ArgumentEquals(0, "QUIT"))
            {
                return RedisValue.Ok;
            }

            return RedisValue.Error("ERR unknown command");
        }
        catch (Exception ex)
        {
            return RedisValue.Error(ex.Message);
        }
    }

    /// <summary>
    /// 处理 AUTH 命令。
    /// </summary>
    protected virtual RedisValue Auth(RedisSessionClient session, RedisValue request, RedisServerOption option)
    {
        if (option?.Password is null)
        {
            return RedisValue.Error("ERR AUTH called without any password configured for the default user.");
        }

        if (request.Items.Count == 2)
        {
            if (string.IsNullOrEmpty(option.UserName) && request.TryGetStringArgument(1, out var password) && password == option.Password)
            {
                session.IsAuthenticated = true;
                return RedisValue.Ok;
            }
        }
        else if (request.Items.Count == 3)
        {
            if (request.TryGetStringArgument(1, out var userName)
                && request.TryGetStringArgument(2, out var password)
                && string.Equals(userName, option.UserName, StringComparison.Ordinal)
                && password == option.Password)
            {
                session.IsAuthenticated = true;
                return RedisValue.Ok;
            }
        }
        else
        {
            return RedisValue.Error("ERR wrong number of arguments for 'auth' command");
        }

        return RedisValue.Error("WRONGPASS invalid username-password pair or user is disabled.");
    }

    /// <summary>
    /// 处理 PING 命令。
    /// </summary>
    protected virtual RedisValue Ping(RedisValue request)
    {
        if (request.Items.Count == 1)
        {
            return RedisValue.Pong;
        }

        return request.TryGetBytesArgument(1, out var bytes) ? RedisValue.BulkString(bytes) : RedisValue.Error("ERR wrong number of arguments for 'ping' command");
    }

    /// <summary>
    /// 处理 ECHO 命令。
    /// </summary>
    protected virtual RedisValue Echo(RedisValue request)
    {
        return request.Items.Count == 2 && request.TryGetBytesArgument(1, out var bytes)
            ? RedisValue.BulkString(bytes)
            : RedisValue.Error("ERR wrong number of arguments for 'echo' command");
    }

    /// <summary>
    /// 处理 GET 命令。
    /// </summary>
    protected virtual RedisValue Get(RedisDatabase database, RedisValue request)
    {
        if (request.Items.Count != 2 || !request.TryGetStringArgument(1, out var key))
        {
            return RedisValue.Error("ERR wrong number of arguments for 'get' command");
        }

        return database.TryGet(key, out var value) ? RedisValue.BulkString(value) : RedisValue.NullBulkString;
    }

    /// <summary>
    /// 处理 SET 命令。
    /// </summary>
    protected virtual RedisValue Set(RedisDatabase database, RedisValue request)
    {
        if (request.Items.Count < 3 || !request.TryGetStringArgument(1, out var key) || !request.TryGetBytesArgument(2, out var value))
        {
            return RedisValue.Error("ERR wrong number of arguments for 'set' command");
        }

        TimeSpan? expiry = null;
        if (request.Items.Count > 3)
        {
            if (request.Items.Count != 5)
            {
                return RedisValue.Error("ERR syntax error");
            }

            if (request.ArgumentEquals(3, "EX") && request.TryGetInt64Argument(4, out var seconds))
            {
                expiry = TimeSpan.FromSeconds(seconds);
            }
            else if (request.ArgumentEquals(3, "PX") && request.TryGetInt64Argument(4, out var milliseconds))
            {
                expiry = TimeSpan.FromMilliseconds(milliseconds);
            }
            else
            {
                return RedisValue.Error("ERR syntax error");
            }
        }

        database.Set(key, value, expiry);
        return RedisValue.Ok;
    }

    /// <summary>
    /// 处理 DEL 命令。
    /// </summary>
    protected virtual RedisValue Delete(RedisDatabase database, RedisValue request)
    {
        if (request.Items.Count < 2)
        {
            return RedisValue.Error("ERR wrong number of arguments for 'del' command");
        }

        var keys = new List<string>(request.Items.Count - 1);
        for (var i = 1; i < request.Items.Count; i++)
        {
            if (request.TryGetStringArgument(i, out var key))
            {
                keys.Add(key);
            }
        }

        return RedisValue.IntegerValue(database.Delete(keys));
    }

    /// <summary>
    /// 处理 EXISTS 命令。
    /// </summary>
    protected virtual RedisValue Exists(RedisDatabase database, RedisValue request)
    {
        if (request.Items.Count < 2)
        {
            return RedisValue.Error("ERR wrong number of arguments for 'exists' command");
        }

        long count = 0;
        for (var i = 1; i < request.Items.Count; i++)
        {
            if (request.TryGetStringArgument(i, out var key) && database.Exists(key))
            {
                count++;
            }
        }

        return RedisValue.IntegerValue(count);
    }

    /// <summary>
    /// 处理 INCR 和 DECR 命令。
    /// </summary>
    protected virtual RedisValue Increment(RedisDatabase database, RedisValue request, long delta)
    {
        if (request.Items.Count != 2 || !request.TryGetStringArgument(1, out var key))
        {
            return RedisValue.Error("ERR wrong number of arguments for increment command");
        }

        return RedisValue.IntegerValue(database.Increment(key, delta));
    }

    /// <summary>
    /// 处理 INCRBY 和 DECRBY 命令。
    /// </summary>
    protected virtual RedisValue IncrementBy(RedisDatabase database, RedisValue request, long sign)
    {
        if (request.Items.Count != 3 || !request.TryGetStringArgument(1, out var key) || !request.TryGetInt64Argument(2, out var delta))
        {
            return RedisValue.Error("ERR wrong number of arguments for increment command");
        }

        return RedisValue.IntegerValue(database.Increment(key, delta * sign));
    }

    /// <summary>
    /// 处理 MGET 命令。
    /// </summary>
    protected virtual RedisValue MGet(RedisDatabase database, RedisValue request)
    {
        if (request.Items.Count < 2)
        {
            return RedisValue.Error("ERR wrong number of arguments for 'mget' command");
        }

        var values = new RedisValue[request.Items.Count - 1];
        for (var i = 1; i < request.Items.Count; i++)
        {
            if (request.TryGetStringArgument(i, out var key) && database.TryGet(key, out var value))
            {
                values[i - 1] = RedisValue.BulkString(value);
            }
            else
            {
                values[i - 1] = RedisValue.NullBulkString;
            }
        }

        return RedisValue.Array(values);
    }

    /// <summary>
    /// 处理 MSET 命令。
    /// </summary>
    protected virtual RedisValue MSet(RedisDatabase database, RedisValue request)
    {
        if (request.Items.Count < 3 || (request.Items.Count - 1) % 2 != 0)
        {
            return RedisValue.Error("ERR wrong number of arguments for 'mset' command");
        }

        for (var i = 1; i < request.Items.Count; i += 2)
        {
            if (request.TryGetStringArgument(i, out var key) && request.TryGetBytesArgument(i + 1, out var value))
            {
                database.Set(key, value);
            }
        }

        return RedisValue.Ok;
    }

    /// <summary>
    /// 处理 EXPIRE 命令。
    /// </summary>
    protected virtual RedisValue Expire(RedisDatabase database, RedisValue request)
    {
        if (request.Items.Count != 3 || !request.TryGetStringArgument(1, out var key) || !request.TryGetInt64Argument(2, out var seconds))
        {
            return RedisValue.Error("ERR wrong number of arguments for 'expire' command");
        }

        return RedisValue.IntegerValue(database.Expire(key, seconds) ? 1 : 0);
    }

    /// <summary>
    /// 处理 TTL 命令。
    /// </summary>
    protected virtual RedisValue Ttl(RedisDatabase database, RedisValue request)
    {
        if (request.Items.Count != 2 || !request.TryGetStringArgument(1, out var key))
        {
            return RedisValue.Error("ERR wrong number of arguments for 'ttl' command");
        }

        return RedisValue.IntegerValue(database.Ttl(key));
    }

    /// <summary>
    /// 处理 KEYS 命令。
    /// </summary>
    protected virtual RedisValue Keys(RedisDatabase database, RedisValue request)
    {
        if (request.Items.Count != 2 || !request.TryGetStringArgument(1, out var pattern))
        {
            return RedisValue.Error("ERR wrong number of arguments for 'keys' command");
        }

        var keys = database.Keys(pattern);
        var values = new RedisValue[keys.Length];
        for (var i = 0; i < keys.Length; i++)
        {
            values[i] = RedisValue.BulkString(keys[i]);
        }

        return RedisValue.Array(values);
    }

    /// <summary>
    /// 处理 LPUSH 和 RPUSH 命令。
    /// </summary>
    protected virtual RedisValue ListPush(RedisDatabase database, RedisValue request, bool left)
    {
        if (request.Items.Count < 3 || !request.TryGetStringArgument(1, out var key))
        {
            return RedisValue.Error("ERR wrong number of arguments for list push command");
        }

        var values = new List<byte[]>(request.Items.Count - 2);
        for (var i = 2; i < request.Items.Count; i++)
        {
            if (!request.TryGetBytesArgument(i, out var value))
            {
                return RedisValue.Error("ERR syntax error");
            }

            values.Add(value);
        }

        return RedisValue.IntegerValue(left ? database.ListLeftPush(key, values) : database.ListRightPush(key, values));
    }

    /// <summary>
    /// 处理 LLEN 命令。
    /// </summary>
    protected virtual RedisValue ListLength(RedisDatabase database, RedisValue request)
    {
        if (request.Items.Count != 2 || !request.TryGetStringArgument(1, out var key))
        {
            return RedisValue.Error("ERR wrong number of arguments for 'llen' command");
        }

        return RedisValue.IntegerValue(database.ListLength(key));
    }

    /// <summary>
    /// 处理 LRANGE 命令。
    /// </summary>
    protected virtual RedisValue ListRange(RedisDatabase database, RedisValue request)
    {
        if (request.Items.Count != 4
            || !request.TryGetStringArgument(1, out var key)
            || !request.TryGetInt64Argument(2, out var start)
            || !request.TryGetInt64Argument(3, out var stop))
        {
            return RedisValue.Error("ERR wrong number of arguments for 'lrange' command");
        }

        var bytes = database.ListRange(key, (int)start, (int)stop);
        var values = new RedisValue[bytes.Length];
        for (var i = 0; i < bytes.Length; i++)
        {
            values[i] = RedisValue.BulkString(bytes[i]);
        }

        return RedisValue.Array(values);
    }

    /// <summary>
    /// 处理 ZADD 命令。
    /// </summary>
    protected virtual RedisValue SortedSetAdd(RedisDatabase database, RedisValue request)
    {
        if (request.Items.Count < 4 || (request.Items.Count - 2) % 2 != 0 || !request.TryGetStringArgument(1, out var key))
        {
            return RedisValue.Error("ERR wrong number of arguments for 'zadd' command");
        }

        long count = 0;
        for (var i = 2; i < request.Items.Count; i += 2)
        {
            if (!request.TryGetStringArgument(i, out var scoreText)
                || !double.TryParse(scoreText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var score)
                || !request.TryGetStringArgument(i + 1, out var member))
            {
                return RedisValue.Error("ERR syntax error");
            }

            count += database.SortedSetAdd(key, member, score);
        }

        return RedisValue.IntegerValue(count);
    }

    /// <summary>
    /// 处理 ZCARD 命令。
    /// </summary>
    protected virtual RedisValue SortedSetLength(RedisDatabase database, RedisValue request)
    {
        if (request.Items.Count != 2 || !request.TryGetStringArgument(1, out var key))
        {
            return RedisValue.Error("ERR wrong number of arguments for 'zcard' command");
        }

        return RedisValue.IntegerValue(database.SortedSetLength(key));
    }

    /// <summary>
    /// 处理 ZREM 命令。
    /// </summary>
    protected virtual RedisValue SortedSetRemove(RedisDatabase database, RedisValue request)
    {
        if (request.Items.Count < 3 || !request.TryGetStringArgument(1, out var key))
        {
            return RedisValue.Error("ERR wrong number of arguments for 'zrem' command");
        }

        var members = new List<string>(request.Items.Count - 2);
        for (var i = 2; i < request.Items.Count; i++)
        {
            if (request.TryGetStringArgument(i, out var member))
            {
                members.Add(member);
            }
        }

        return RedisValue.IntegerValue(database.SortedSetRemove(key, members));
    }

    /// <summary>
    /// 处理 INFO 命令。
    /// </summary>
    protected virtual RedisValue Info(RedisValue request)
    {
        if (request.Items.Count > 2)
        {
            return RedisValue.Error("ERR wrong number of arguments for 'info' command");
        }

        return RedisValue.BulkString("# Server\r\nredis_version:touchsocket\r\n");
    }
}
