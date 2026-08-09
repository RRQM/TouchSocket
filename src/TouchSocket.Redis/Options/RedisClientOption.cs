namespace TouchSocket.Redis;

/// <summary>
/// Redis 客户端配置。
/// </summary>
public class RedisClientOption
{
    /// <summary>
    /// 获取或设置认证用户名。为空时使用 Redis 旧式密码认证。
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 获取或设置认证密码。为 null 时不自动发送 AUTH 命令。
    /// </summary>
    public string Password { get; set; }
}
