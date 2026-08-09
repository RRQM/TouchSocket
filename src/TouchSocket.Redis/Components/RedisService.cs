using TouchSocket.Sockets;

namespace TouchSocket.Redis;

/// <summary>
/// 基于内存的 Redis 兼容 TCP 服务端。
/// </summary>
public class RedisService : TcpServiceBase<RedisSessionClient>
{
    /// <summary>
    /// 初始化 <see cref="RedisService"/> 类的新实例。
    /// </summary>
    public RedisService()
    {
        this.Database = new RedisDatabase();
        this.CommandProcessor = new RedisCommandProcessor();
    }

    /// <summary>
    /// 获取内存数据库。
    /// </summary>
    public RedisDatabase Database { get; }

    /// <summary>
    /// 获取或设置命令处理器。
    /// </summary>
    public RedisCommandProcessor CommandProcessor { get; set; }

    /// <inheritdoc/>
    protected override RedisSessionClient NewClient()
    {
        return new RedisSessionClient(this.Database, this.CommandProcessor);
    }
}
