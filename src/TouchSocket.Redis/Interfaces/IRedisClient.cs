using TouchSocket.Sockets;

namespace TouchSocket.Redis;

/// <summary>
/// Redis 客户端契约。
/// </summary>
public interface IRedisClient : IRedisSession, ITcpSession, IConnectableClient
{
    /// <summary>
    /// 连接 Redis 服务端。
    /// </summary>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 执行 Redis 命令并等待响应。
    /// </summary>
    Task<RedisValue> ExecuteAsync(RedisValue command, CancellationToken cancellationToken = default);
}
