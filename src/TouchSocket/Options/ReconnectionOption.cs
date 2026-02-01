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

        this.ConnectAction = async (client, cancellationToken) => await client.ConnectAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
    /// 重连失败回调
    /// <para>当单次重连尝试失败时触发此回调，每次重连失败都会调用</para>
    /// <para>触发时机：在ConnectAsync抛出异常时立即触发</para>
    /// <para>第1参数：重连失败的客户端实例</para>
    /// <para>第2参数：当前重连尝试次数（从1开始计数）</para>
    /// <para>第3参数：导致重连失败的异常信息</para>
    /// </summary>
    public Action<TClient, int, Exception> OnFailed { get; set; }

    /// <summary>
    /// 重连放弃回调
    /// <para>当达到最大重连次数限制，决定放弃继续重连时触发此回调</para>
    /// <para>触发时机：当MaxRetryCount大于0且重连尝试次数达到该限制时</para>
    /// <para>注意：如果MaxRetryCount设置为-1（无限重连），此回调永远不会被触发</para>
    /// <para>第1参数：放弃重连的客户端实例</para>
    /// <para>第2参数：总共尝试的重连次数</para>
    /// </summary>
    public Action<TClient, int> OnGiveUp { get; set; }

    /// <summary>
    /// 重连成功回调
    /// <para>当重连操作成功完成时触发此回调</para>
    /// <para>触发时机：</para>
    /// <para>1. 检测到客户端已经在线时</para>
    /// <para>2. 执行ConnectAsync成功后</para>
    /// <para>第1参数：重连成功的客户端实例</para>
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

}