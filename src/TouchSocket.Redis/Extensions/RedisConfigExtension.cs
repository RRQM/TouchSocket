namespace TouchSocket.Redis;

/// <summary>
/// Redis 配置扩展。
/// </summary>
public static class RedisConfigExtension
{
    /// <summary>
    /// Redis 客户端配置依赖属性。
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig), ActionMode = true)]
    public static readonly DependencyProperty<RedisClientOption> RedisClientOptionProperty = new DependencyProperty<RedisClientOption>("RedisClientOption", null);

    /// <summary>
    /// Redis 服务端配置依赖属性。
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig), ActionMode = true)]
    public static readonly DependencyProperty<RedisServerOption> RedisServerOptionProperty = new DependencyProperty<RedisServerOption>("RedisServerOption", null);
}
