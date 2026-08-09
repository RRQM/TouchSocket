// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using TouchSocket.Resources;

namespace TouchSocket.Sockets;

/// <summary>
/// 重连插件配置选项
/// </summary>
/// <typeparam name="TClient">客户端类型</typeparam>
public class ReconnectionOption<TClient>
    where TClient : IConnectableClient, IOnlineClient, IDependencyClient
{
    /// <summary>
    /// 重连插件配置选项
    /// </summary>
    public ReconnectionOption()
    {
        this.CheckAction = (client) =>
        {
            var result = client.Online ? ConnectionCheckResult.Alive : ConnectionCheckResult.Dead;
            return Task.FromResult(result);
        };

        this.ConnectAction = async (client, cancellationToken) => await client.ConnectAsync(cancellationToken).ConfigureDefaultAwait();
        this.OnLog = this.DefaultOnLogAsync;
    }

    /// <summary>
    /// 退避倍数（用于指数和线性退避）
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;

    /// <summary>
    /// 基础重连间隔
    /// </summary>
    public TimeSpan BaseInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// 检查客户端活性的委托。
    /// </summary>
    /// <remarks>
    /// 注意，当返回值为<see cref="ConnectionCheckResult.Dead"/>时，请确保已经清理现有异常的在线状态（例如：tcp的断网假死），不然重连可能无法触发。
    /// </remarks>
    public Func<TClient, Task<ConnectionCheckResult>> CheckAction { get; set; }

    /// <summary>
    /// 尝试连接的委托
    /// </summary>
    public Func<TClient, CancellationToken, Task> ConnectAction { get; set; }

    /// <summary>
    /// 在执行连接时，连接时间
    /// </summary>
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 是否记录重连日志
    /// </summary>
    public bool LogReconnection { get; set; } = true;

    /// <summary>
    /// 最大重连间隔
    /// </summary>
    public TimeSpan MaxInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// 最大重连次数，-1表示无限重连
    /// </summary>
    public int MaxRetryCount { get; set; } = -1;

    /// <summary>
    /// 重连日志回调。
    /// <para>默认实现会转调旧的 <see cref="OnFailed"/>、<see cref="OnGiveUp"/>、<see cref="OnSuccessed"/>。</para>
    /// <para>当重新赋值此回调时，旧三个回调不会再被自动调用。</para>
    /// <para>该回调支持异步返回。</para>
    /// </summary>
    public Func<TClient, ReconnectionLogEventArgs, Task> OnLog { get; set; }

    /// <summary>
    /// 重连失败回调。
    /// <para>保留兼容性：当单次重连尝试失败时触发。</para>
    /// </summary>
    public Action<TClient, int, Exception> OnFailed { get; set; }

    /// <summary>
    /// 重连放弃回调。
    /// <para>保留兼容性：当达到最大重连次数后触发。</para>
    /// </summary>
    public Action<TClient, int> OnGiveUp { get; set; }

    /// <summary>
    /// 重连成功回调。
    /// <para>保留兼容性：当重连成功时触发。</para>
    /// </summary>
    public Action<TClient> OnSuccessed { get; set; }

    /// <summary>
    /// 轮询时间间隔
    /// </summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// 重连策略
    /// </summary>
    public ReconnectionStrategy Strategy { get; set; } = ReconnectionStrategy.Simple;

    /// <summary>
    /// 使用自定义连接策略
    /// </summary>
    /// <param name="connectAction">自定义连接动作</param>
    public void UseCustom(Func<TClient, CancellationToken, Task> connectAction)
    {
        this.Strategy = ReconnectionStrategy.Custom;
        this.ConnectAction = connectAction;
    }

    /// <summary>
    /// 使用指数退避重连策略 - 每次失败后延迟时间指数增长
    /// </summary>
    /// <param name="baseInterval">基础间隔，默认1秒</param>
    /// <param name="maxInterval">最大间隔，默认5分钟</param>
    /// <param name="multiplier">退避倍数，默认2.0</param>
    /// <param name="maxRetryCount">最大重连次数，-1表示无限重连</param>
    public void UseExponentialBackoff(
        TimeSpan? baseInterval = null,
        TimeSpan? maxInterval = null,
        double multiplier = 2.0,
        int maxRetryCount = -1)
    {
        this.Strategy = ReconnectionStrategy.ExponentialBackoff;
        this.BaseInterval = baseInterval ?? TimeSpan.FromSeconds(1);
        this.MaxInterval = maxInterval ?? TimeSpan.FromMinutes(5);
        this.BackoffMultiplier = multiplier;
        this.MaxRetryCount = maxRetryCount;
    }

    /// <summary>
    /// 使用线性退避重连策略 - 每次失败后延迟时间线性增长
    /// </summary>
    /// <param name="baseInterval">基础间隔，默认1秒</param>
    /// <param name="maxInterval">最大间隔，默认5分钟</param>
    /// <param name="increment">每次增加的时间，默认1秒</param>
    /// <param name="maxRetryCount">最大重连次数，-1表示无限重连</param>
    public void UseLinearBackoff(
        TimeSpan? baseInterval = null,
        TimeSpan? maxInterval = null,
        TimeSpan? increment = null,
        int maxRetryCount = -1)
    {
        this.Strategy = ReconnectionStrategy.LinearBackoff;
        this.BaseInterval = baseInterval ?? TimeSpan.FromSeconds(1);
        this.MaxInterval = maxInterval ?? TimeSpan.FromMinutes(5);
        this.BackoffMultiplier = (increment ?? TimeSpan.FromSeconds(1)).TotalMilliseconds;
        this.MaxRetryCount = maxRetryCount;
    }

    /// <summary>
    /// 使用简单重连策略 - 固定间隔重连
    /// </summary>
    /// <param name="interval">重连间隔，默认1秒</param>
    /// <param name="maxRetryCount">最大重连次数，-1表示无限重连</param>
    public void UseSimple(TimeSpan? interval = null, int maxRetryCount = -1)
    {
        this.Strategy = ReconnectionStrategy.Simple;
        this.BaseInterval = interval ?? TimeSpan.FromSeconds(1);
        this.MaxRetryCount = maxRetryCount;
    }

    private Task DefaultOnLogAsync(TClient client, ReconnectionLogEventArgs e)
    {
        switch (e.Type)
        {
            case ReconnectionLogType.Failed:
                this.OnFailed?.Invoke(client, e.Attempts, e.Exception ?? new InvalidOperationException(e.Message));
                break;
            case ReconnectionLogType.GiveUp:
                this.OnGiveUp?.Invoke(client, e.Attempts);
                break;
            case ReconnectionLogType.Success:
                this.OnSuccessed?.Invoke(client);
                break;
        }

        return Task.CompletedTask;
    }

}
