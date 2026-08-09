using TouchSocket.Sockets;

namespace TouchSocket.Redis;

/// <summary>
/// Redis TCP 客户端。
/// </summary>
public class RedisClient : TcpClientBase, IRedisClient
{
    private readonly Queue<PendingRequest> m_waiting = new Queue<PendingRequest>();
    private readonly object m_syncRoot = new object();

    /// <summary>
    /// 连接 Redis 服务端。
    /// </summary>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        await this.TcpConnectAsync(cancellationToken).ConfigureDefaultAwait();
        try
        {
            await this.AuthenticateIfNeededAsync(cancellationToken).ConfigureDefaultAwait();
        }
        catch
        {
            try
            {
                await this.CloseAsync("Redis AUTH failed").ConfigureDefaultAwait();
            }
            catch
            {
                // 关闭失败不能覆盖 AUTH 的原始异常。
            }

            throw;
        }
    }

    /// <summary>
    /// 发送 Redis 值，并且不等待响应。
    /// </summary>
    public Task SendRedisValueAsync(RedisValue value, CancellationToken cancellationToken = default)
    {
        return this.ProtectedSendAsync<RedisValue>(value, cancellationToken);
    }

    /// <summary>
    /// 执行 Redis 命令。
    /// </summary>
    public async Task<RedisValue> ExecuteAsync(RedisValue command, CancellationToken cancellationToken = default)
    {
        var pending = new PendingRequest(cancellationToken);
        lock (this.m_syncRoot)
        {
            this.m_waiting.Enqueue(pending);
        }

        try
        {
            await this.SendRedisValueAsync(command, cancellationToken).ConfigureDefaultAwait();
            var response = await pending.Task.ConfigureDefaultAwait();
            response.ThrowIfError();
            return response;
        }
        catch
        {
            pending.TryCancel();
            throw;
        }
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosed(ClosedEventArgs e)
    {
        this.CancelAllPending();
        await base.OnTcpClosed(e).ConfigureDefaultAwait();
    }

    /// <inheritdoc/>
    protected override async Task OnTcpConnecting(ConnectingEventArgs e)
    {
        await base.OnTcpConnecting(e).ConfigureDefaultAwait();
        this.SetAdapter(new RedisAdapter());
    }

    /// <inheritdoc/>
    protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        if (e.RequestInfo is RedisValue value)
        {
            if (!this.TrySetNext(value))
            {
                await this.OnRedisReceived(new RedisReceivedEventArgs(value)).ConfigureDefaultAwait();
            }
        }

        await base.OnTcpReceived(e).ConfigureDefaultAwait();
    }

    /// <summary>
    /// 为未匹配请求的消息触发 Redis 接收插件。
    /// </summary>
    protected virtual async Task OnRedisReceived(RedisReceivedEventArgs e)
    {
        await this.PluginManager.RaiseIRedisReceivedPluginAsync(this.Resolver, this, e).ConfigureDefaultAwait();
    }

    private bool TrySetNext(RedisValue value)
    {
        while (true)
        {
            PendingRequest pending = null;
            lock (this.m_syncRoot)
            {
                if (this.m_waiting.Count > 0)
                {
                    pending = this.m_waiting.Dequeue();
                }
            }

            if (pending is null)
            {
                return false;
            }

            if (pending.TrySetResult(value))
            {
                return true;
            }
        }
    }

    private void CancelAllPending()
    {
        PendingRequest[] requests;
        lock (this.m_syncRoot)
        {
            requests = this.m_waiting.ToArray();
            this.m_waiting.Clear();
        }

        for (var i = 0; i < requests.Length; i++)
        {
            requests[i].TryCancel();
        }
    }

    private async Task AuthenticateIfNeededAsync(CancellationToken cancellationToken)
    {
        var option = this.Config?.GetValue(RedisConfigExtension.RedisClientOptionProperty);
        if (option?.Password is null)
        {
            return;
        }

        var command = string.IsNullOrEmpty(option.UserName)
            ? RedisValue.Command("AUTH", option.Password)
            : RedisValue.Command("AUTH", option.UserName, option.Password);
        await this.ExecuteAsync(command, cancellationToken).ConfigureDefaultAwait();
    }

    private sealed class PendingRequest
    {
        private readonly TaskCompletionSource<RedisValue> m_source = new TaskCompletionSource<RedisValue>(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly CancellationTokenRegistration m_registration;

        public PendingRequest(CancellationToken cancellationToken)
        {
            if (cancellationToken.CanBeCanceled)
            {
                this.m_registration = cancellationToken.Register(state => ((PendingRequest)state).TryCancel(), this);
            }
        }

        public Task<RedisValue> Task => this.m_source.Task;

        public bool TrySetResult(RedisValue value)
        {
            this.m_registration.Dispose();
            return this.m_source.TrySetResult(value);
        }

        public bool TryCancel()
        {
            this.m_registration.Dispose();
            return this.m_source.TrySetCanceled();
        }
    }
}
