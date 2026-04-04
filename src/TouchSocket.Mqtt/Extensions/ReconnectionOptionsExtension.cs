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

namespace TouchSocket.Mqtt;

/// <summary>
/// 提供扩展方法以支持MQTT检查操作。
/// </summary>
public static class ReconnectionOptionsExtension
{

    /// <summary>
    /// 配置MQTT检查操作。
    /// </summary>
    /// <typeparam name="TClient">客户端类型，必须实现<see cref="IConnectableClient"/>、<see cref="IOnlineClient"/>、<see cref="IDependencyClient"/>和<see cref="IMqttClient"/>。</typeparam>
    /// <param name="reconnectionOption">重连选项。</param>
    /// <param name="activeTimeSpan">活动时间间隔，默认为3秒。</param>
    /// <param name="pingInterval">Ping间隔时间，默认为5秒。如果上次Ping距离本次超过该间隔，则进行Ping。</param>
    /// <exception cref="ArgumentOutOfRangeException">当时间参数小于或等于零时抛出。</exception>
    /// <exception cref="ArgumentNullException">当<paramref name="reconnectionOption"/>为<see langword="null"/>时抛出。</exception>
    public static void UseMqttCheckAction<TClient>(
        this ReconnectionOption<TClient> reconnectionOption,
        TimeSpan? activeTimeSpan = null,
        TimeSpan? pingInterval = null)
        where TClient : IConnectableClient, IOnlineClient, IDependencyClient, IMqttClient
    {
        ThrowHelper.ThrowIfNull(reconnectionOption, nameof(reconnectionOption));
        var span = activeTimeSpan ?? TimeSpan.FromSeconds(3);
        var interval = pingInterval ?? TimeSpan.FromSeconds(5);
        // 单次Ping操作的超时时间，固定使用5秒。
        var pingOperationTimeout = TimeSpan.FromSeconds(5);

        // 验证时间参数的有效性
        if (span <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(activeTimeSpan), "活动时间间隔必须大于零");
        }

        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(pingInterval), "Ping间隔必须大于零");
        }

        var lastPingTime = DateTimeOffset.MinValue;

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
}
