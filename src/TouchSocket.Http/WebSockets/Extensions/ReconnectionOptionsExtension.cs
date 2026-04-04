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

using TouchSocket.Sockets;

namespace TouchSocket.Http.WebSockets;

/// <summary>
/// 提供用于为 <see cref="ReconnectionOption{TClient}"/> 配置 WebSocket 心跳检查的扩展方法。
/// </summary>
public static class ReconnectionOptionsExtension
{
    /// <summary>
    /// 为 <see cref="ReconnectionOption{TClient}"/> 设置一个基于活动时间与 Ping 的检查动作。
    /// </summary>
    /// <typeparam name="TClient">实现了 <see cref="IConnectableClient"/>、<see cref="IOnlineClient"/>、<see cref="IDependencyClient"/>、<see cref="IWebSocket"/> 的客户端类型。</typeparam>
    /// <param name="reconnectionOption">要配置的 <see cref="ReconnectionOption{TClient}"/> 实例。</param>
    /// <param name="activeTimeSpan">在此时间范围内若有活动则跳过心跳检测，默认 3 秒。</param>
    /// <param name="pingInterval">Ping 间隔时间，默认 5 秒。如果上次 Ping 距离本次超过该间隔，则进行 Ping。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="reconnectionOption"/> 为 <see langword="null"/> 时抛出。</exception>
    /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="activeTimeSpan"/> 或 <paramref name="pingInterval"/> 小于或等于零时抛出。</exception>
    public static void UseWebSocketCheckAction<TClient>(
        this ReconnectionOption<TClient> reconnectionOption,
        TimeSpan? activeTimeSpan = null,
        TimeSpan? pingInterval = null)
        where TClient : IConnectableClient, IOnlineClient, IDependencyClient, IWebSocket
    {
        //PR:https://github.com/RRQM/TouchSocket/pull/112
        ThrowHelper.ThrowIfNull(reconnectionOption, nameof(reconnectionOption));
        var span = activeTimeSpan ?? TimeSpan.FromSeconds(3);
        var interval = pingInterval ?? TimeSpan.FromSeconds(5);
        var pingOperationTimeout = TimeSpan.FromSeconds(5);

        if (span <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(activeTimeSpan), "活动时间间隔必须大于零");
        }

        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(pingInterval), "Ping间隔必须大于零");
        }

        DateTimeOffset lastPingTime = DateTimeOffset.MinValue;

        reconnectionOption.CheckAction = async (client) =>
        {
            if (!client.Online)
            {
                return ConnectionCheckResult.Dead;
            }

            var timeSinceLastPing = DateTimeOffset.UtcNow - lastPingTime;
            var timeSinceLastActivity = DateTimeOffset.UtcNow - client.GetLastActiveTime();

            if (timeSinceLastPing >= interval)
            {
                try
                {
                    using var pingCts = new CancellationTokenSource(pingOperationTimeout);
                    var pingResult = await client.PingAsync(pingCts.Token).ConfigureDefaultAwait();

                    if (pingResult.IsSuccess)
                    {
                        lastPingTime = DateTimeOffset.UtcNow;
                        return ConnectionCheckResult.Alive;
                    }

                    using var closeCts = new CancellationTokenSource(pingOperationTimeout);
                    await client.CloseAsync("心跳插件ping失败主动断开连接", closeCts.Token).ConfigureDefaultAwait();
                    return ConnectionCheckResult.Dead;
                }
                catch
                {
                    return ConnectionCheckResult.Dead;
                }
            }

            return timeSinceLastActivity < span ? ConnectionCheckResult.Skip : ConnectionCheckResult.Alive;
        };
    }

    /// <summary>
    /// 为 <see cref="ReconnectionOption{TClient}"/> 设置一个仅基于活动时间的检查动作。
    /// </summary>
    /// <typeparam name="TClient">实现了 <see cref="SetupClientWebSocket"/> 的客户端类型。</typeparam>
    /// <param name="reconnectionOption">要配置的 <see cref="ReconnectionOption{TClient}"/> 实例。</param>
    /// <param name="activeTimeSpan">在此时间范围内若有活动则跳过心跳检测，默认 3 秒。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="reconnectionOption"/> 为 <see langword="null"/> 时抛出。</exception>
    /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="activeTimeSpan"/> 小于或等于零时抛出。</exception>
    public static void UseWebSocketCheckAction<TClient>(
        this ReconnectionOption<TClient> reconnectionOption,
        TimeSpan? activeTimeSpan = null)
        where TClient : SetupClientWebSocket, IConnectableClient, IOnlineClient, IDependencyClient
    {
        ThrowHelper.ThrowIfNull(reconnectionOption, nameof(reconnectionOption));
        var span = activeTimeSpan ?? TimeSpan.FromSeconds(3);

        if (span <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(activeTimeSpan), "活动时间间隔必须大于零");
        }

        reconnectionOption.CheckAction = async (client) =>
        {
            if (!client.Online)
            {
                return ConnectionCheckResult.Dead;
            }

            var timeSinceLastActivity = DateTimeOffset.UtcNow - client.GetLastActiveTime();
            return timeSinceLastActivity < span ? ConnectionCheckResult.Skip : ConnectionCheckResult.Alive;
        };
    }
}
