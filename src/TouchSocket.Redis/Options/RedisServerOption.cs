namespace TouchSocket.Redis;

/// <summary>
/// Redis 服务端配置。
/// </summary>
public class RedisServerOption
{
    /// <summary>
    /// 获取或设置认证用户名。为空时使用 Redis 旧式密码认证。
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 获取或设置认证密码。为 null 时不启用 AUTH 校验。
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// 获取或设置是否接受 SELECT 命令。
    /// </summary>
    public bool AcceptSelectCommand { get; set; } = true;
}
