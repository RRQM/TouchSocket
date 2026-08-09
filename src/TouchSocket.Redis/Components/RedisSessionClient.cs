using TouchSocket.Sockets;

namespace TouchSocket.Redis;

/// <summary>
/// Redis 服务端 TCP 会话。
/// </summary>
public class RedisSessionClient : TcpSessionClientBase, IRedisSession
{
    private readonly RedisDatabase m_database;
    private readonly RedisCommandProcessor m_commandProcessor;

    /// <summary>
    /// 初始化 <see cref="RedisSessionClient"/> 类的新实例。
    /// </summary>
    public RedisSessionClient(RedisDatabase database, RedisCommandProcessor commandProcessor)
    {
        this.m_database = database;
        this.m_commandProcessor = commandProcessor;
    }

    /// <summary>
    /// 发送 Redis 值。
    /// </summary>
    public Task SendRedisValueAsync(RedisValue value, CancellationToken cancellationToken = default)
    {
        return this.ProtectedSendAsync<RedisValue>(value, cancellationToken);
    }

    /// <summary>
    /// 获取或设置当前会话是否已经通过 AUTH 认证。
    /// </summary>
    public bool IsAuthenticated { get; set; }

    /// <inheritdoc/>
    protected override async Task OnTcpConnecting(ConnectingEventArgs e)
    {
        await base.OnTcpConnecting(e).ConfigureDefaultAwait();
        this.SetAdapter(new RedisAdapter());
    }

    /// <inheritdoc/>
    protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        if (e.RequestInfo is RedisValue request)
        {
            await this.OnRedisReceived(new RedisReceivedEventArgs(request)).ConfigureDefaultAwait();
            var option = this.Config?.GetValue(RedisConfigExtension.RedisServerOptionProperty);
            var response = this.m_commandProcessor.Process(this, this.m_database, request, option);
            await this.SendRedisValueAsync(response).ConfigureDefaultAwait();
            if (request.ArgumentEquals(0, "QUIT"))
            {
                await this.CloseAsync("Redis QUIT").ConfigureDefaultAwait();
            }
        }

        await base.OnTcpReceived(e).ConfigureDefaultAwait();
    }

    /// <summary>
    /// 触发 Redis 接收插件。
    /// </summary>
    protected virtual async Task OnRedisReceived(RedisReceivedEventArgs e)
    {
        await this.PluginManager.RaiseIRedisReceivedPluginAsync(this.Resolver, this, e).ConfigureDefaultAwait();
    }
}
