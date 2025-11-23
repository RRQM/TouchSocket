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
    /// <param name="pingTimeout">
    /// Ping操作的超时时间，防止长时间等待
    /// <para>默认值：5秒</para>
    /// <para>建议范围：3-10秒</para>
    /// </param>
    /// <exception cref="ArgumentNullException">当<paramref name="reconnectionOption"/>为<see langword="null"/>时抛出</exception>
    /// <exception cref="ArgumentOutOfRangeException">当<paramref name="activeTimeSpan"/>小于等于零时抛出</exception>
    /// <exception cref="ArgumentOutOfRangeException">当<paramref name="pingTimeout"/>小于等于零时抛出</exception>
    public static void UseDmtpCheckAction<TClient>(
        this ReconnectionOption<TClient> reconnectionOption,
        TimeSpan? activeTimeSpan = null,
        TimeSpan? pingTimeout = null)
  where TClient : IConnectableClient, IOnlineClient, IDependencyClient, IDmtpClient
    {
        ThrowHelper.ThrowIfNull(reconnectionOption, nameof(reconnectionOption));
        var span = activeTimeSpan ?? TimeSpan.FromSeconds(3);
        var timeout = pingTimeout ?? TimeSpan.FromSeconds(5);

        // 验证时间参数的有效性
        if (span <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(activeTimeSpan), "活动时间间隔必须大于零");
        }

        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(pingTimeout), "Ping超时时间必须大于零");
        }

        reconnectionOption.CheckAction = async (client) =>
        {
            // 第1步：快速在线状态检查
            // 如果客户端已经离线，无需进一步检查，直接返回Dead状态
            if (!client.Online)
            {
                return ConnectionCheckResult.Dead;
            }

            // 第2步：活动时间检查
            // 如果客户端在指定时间内有活动，说明连接正常，跳过本次心跳检查
            var lastActiveTime = client.GetLastActiveTime();
            var timeSinceLastActivity = DateTimeOffset.UtcNow - lastActiveTime;

            if (timeSinceLastActivity < span)
            {
                return ConnectionCheckResult.Skip;
            }

            // 第3步：主动心跳检查
            // 通过Ping操作验证连接的实际可用性
            try
            {
                using var pingCts = new CancellationTokenSource(timeout);
                var pingResult = await client.PingAsync(pingCts.Token).ConfigureAwait(false);

                if (pingResult.IsSuccess)
                {
                    return ConnectionCheckResult.Alive;
                }

                using var closeCts = new CancellationTokenSource(timeout);

                var closeResult = await client.CloseAsync("心跳插件ping失败主动断开连接", closeCts.Token).ConfigureAwait(false);

                return ConnectionCheckResult.Dead;
            }
            catch (OperationCanceledException)
            {
                // Ping超时，认为连接已死
                return ConnectionCheckResult.Dead;
            }
            catch
            {
                // 其他异常也认为连接不可用
                return ConnectionCheckResult.Dead;
            }
        };
    }
}
