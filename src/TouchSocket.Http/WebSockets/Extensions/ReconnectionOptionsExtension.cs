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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
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
    /// <typeparam name="TClient">实现了 <see cref="IConnectableClient"/>, <see cref="IOnlineClient"/>, <see cref="IDependencyClient"/>, <see cref="IWebSocketClient"/> 的客户端类型。</typeparam>
    /// <param name="reconnectionOption">要配置的 <see cref="ReconnectionOption{TClient}"/> 实例。</param>
    /// <param name="activeTimeSpan">在此时间范围内若有活动则跳过心跳检测，默认 3 秒。</param>
    /// <param name="pingTimeout">执行 Ping 与 Close 操作时的超时时间，默认 5 秒。</param>
    /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="activeTimeSpan"/> 或 <paramref name="pingTimeout"/> 小于或等于零时抛出。</exception>
    public static void UseWebSocketCheckAction<TClient>(
        this ReconnectionOption<TClient> reconnectionOption,
        TimeSpan? activeTimeSpan = null,
        TimeSpan? pingTimeout = null)
  where TClient : IConnectableClient, IOnlineClient, IDependencyClient, IWebSocketClient
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
