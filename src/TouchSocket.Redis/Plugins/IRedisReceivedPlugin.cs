using TouchSocket.Core;

namespace TouchSocket.Redis;

/// <summary>
/// Redis 接收插件。
/// </summary>
[DynamicMethod]
public interface IRedisReceivedPlugin : IPlugin
{
    /// <summary>
    /// 在接收到 Redis RESP 值时触发。
    /// </summary>
    Task OnRedisReceived(IRedisSession session, RedisReceivedEventArgs e);
}
