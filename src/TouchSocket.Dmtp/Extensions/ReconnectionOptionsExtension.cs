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

namespace TouchSocket.Dmtp;

/// <summary>
/// 重连选项扩展类，提供DMTP协议特定的检活策略配置
/// </summary>
public static class ReconnectionOptionsExtension
{
    /// <summary>
    /// 配置DMTP检活策略
    /// <para>此方法为DMTP客户端配置智能的连接检查策略，支持三级检查：</para>
    /// <para>1. 在线状态检查 - 快速判断连接是否断开</para>
    /// <para>2. 活动时间检查 - 避免频繁检查，优化性能</para>
    /// <para>3. Ping心跳检查 - 主动验证连接可用性</para>
    /// </summary>
    /// <typeparam name="TClient">客户端类型，必须实现相关接口以支持连接、在线状态和DMTP功能</typeparam>
    /// <param name="reconnectionOption">重连选项实例，不能为<see langword="null"/></param>
    /// <param name="activeTimeSpan">
    /// 活动时间检查间隔，用于避免频繁的心跳检查
    /// <para>如果客户端在此时间内有活动，则跳过本次检查</para>
    /// <para>默认值：3秒</para>
    /// <para>建议范围：1-10秒</para>
    /// </param>
    /// <param name="pingInterval">
    /// Ping间隔时间，只有当上次Ping距离本次超过该间隔时才会执行Ping
    /// <para>默认值：5秒</para>
    /// <para>建议范围：3-10秒</para>
    /// </param>
    /// <exception cref="ArgumentNullException">当<paramref name="reconnectionOption"/>为<see langword="null"/>时抛出</exception>
    /// <exception cref="ArgumentOutOfRangeException">当<paramref name="activeTimeSpan"/>小于等于零时抛出</exception>
    /// <exception cref="ArgumentOutOfRangeException">当<paramref name="pingInterval"/>小于等于零时抛出</exception>
    public static void UseDmtpCheckAction<TClient>(
        this ReconnectionOption<TClient> reconnectionOption,
        TimeSpan? activeTimeSpan = null,
        TimeSpan? pingInterval = null)
  where TClient : IConnectableClient, IOnlineClient, IDependencyClient, IDmtpClient
    {
        ThrowHelper.ThrowIfNull(reconnectionOption, nameof(reconnectionOption));
        var span = activeTimeSpan ?? TimeSpan.FromSeconds(3);
        var interval = pingInterval ?? TimeSpan.FromSeconds(5);
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
}
