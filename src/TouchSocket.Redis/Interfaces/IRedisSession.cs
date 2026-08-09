namespace TouchSocket.Redis;

/// <summary>
/// Redis 会话契约。
/// </summary>
public interface IRedisSession
{
    /// <summary>
    /// 发送 Redis RESP 值。
    /// </summary>
    Task SendRedisValueAsync(RedisValue value, CancellationToken cancellationToken = default);
}
