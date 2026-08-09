namespace TouchSocket.Redis;

/// <summary>
/// Redis 接收事件参数。
/// </summary>
public class RedisReceivedEventArgs : PluginEventArgs
{
    /// <summary>
    /// 初始化 <see cref="RedisReceivedEventArgs"/> 类的新实例。
    /// </summary>
    public RedisReceivedEventArgs(RedisValue value)
    {
        this.Value = value;
    }

    /// <summary>
    /// 获取接收到的 RESP 值。
    /// </summary>
    public RedisValue Value { get; }
}
