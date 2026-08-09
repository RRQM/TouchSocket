using TouchSocket.Sockets;

namespace TouchSocket.Redis;

/// <summary>
/// Redis 重连配置扩展。
/// </summary>
public static class RedisReconnectionOptionsExtension
{
    /// <summary>
    /// 配置 Redis 连接检查操作。
    /// </summary>
    /// <typeparam name="TClient">Redis 客户端类型。</typeparam>
    /// <param name="reconnectionOption">重连选项。</param>
    /// <param name="activeTimeSpan">活动时间间隔，默认为 3 秒。</param>
    /// <param name="pingInterval">PING 间隔，默认为 5 秒。</param>
    public static void UseRedisCheckAction<TClient>(
        this ReconnectionOption<TClient> reconnectionOption,
        TimeSpan? activeTimeSpan = null,
        TimeSpan? pingInterval = null)
        where TClient : IRedisClient
    {
        ThrowHelper.ThrowIfNull(reconnectionOption, nameof(reconnectionOption));
        var span = activeTimeSpan ?? TimeSpan.FromSeconds(3);
        var interval = pingInterval ?? TimeSpan.FromSeconds(5);
        var pingOperationTimeout = TimeSpan.FromSeconds(5);

        if (span <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(activeTimeSpan), "活动时间间隔必须大于零");
        }

        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(pingInterval), "PING 间隔必须大于零");
        }

        var lastPingTime = DateTimeOffset.MinValue;

        reconnectionOption.CheckAction = async (client) =>
        {
            if (!client.Online)
            {
                return ConnectionCheckResult.Dead;
            }

            var timeSinceLastPing = DateTimeOffset.UtcNow - lastPingTime;
            var timeSinceLastActivity = DateTimeOffset.UtcNow - client.GetLastActiveTime();

            if (timeSinceLastPing >= interval)
            {
                if (timeSinceLastActivity < span)
                {
                    return ConnectionCheckResult.Skip;
                }

                try
                {
                    using var pingCts = new CancellationTokenSource(pingOperationTimeout);
                    var response = await client.PingAsync(pingCts.Token).ConfigureDefaultAwait();
                    if (!response.IsError && response.AsString() == "PONG")
                    {
                        lastPingTime = DateTimeOffset.UtcNow;
                        return ConnectionCheckResult.Alive;
                    }

                    using var closeCts = new CancellationTokenSource(pingOperationTimeout);
                    await client.CloseAsync("Redis PING 失败，主动断开连接", closeCts.Token).ConfigureDefaultAwait();
                    return ConnectionCheckResult.Dead;
                }
                catch
                {
                    return ConnectionCheckResult.Dead;
                }
            }

            return timeSinceLastActivity < span ? ConnectionCheckResult.Skip : ConnectionCheckResult.Alive;
        };
    }
}
