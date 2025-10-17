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

namespace TouchSocket.Sockets;

/// <summary>
/// 重连插件配置选项
/// </summary>
/// <typeparam name="TClient">客户端类型</typeparam>
public class ReconnectionOptions<TClient>
    where TClient : IConnectableClient, IOnlineClient, IDependencyClient
{
    /// <summary>
    /// 轮询时间间隔
    /// </summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// 检查客户端活性的委托
    /// </summary>
    public Func<TClient, int, Task<ConnectionCheckResult>> CheckAction { get; set; }

    /// <summary>
    /// 尝试连接的委托
    /// </summary>
    public Func<TClient, CancellationToken, Task<bool>> ConnectAction { get; set; }

    /// <summary>
    /// 尝试重连次数,设为-1时则永远尝试连接
    /// </summary>
    public int TryCount { get; set; } = -1;

    /// <summary>
    /// 失败时停留时间(毫秒)
    /// </summary>
    public int SleepTime { get; set; } = 1000;

    /// <summary>
    /// 是否输出日志
    /// </summary>
    public bool PrintLog { get; set; }

    /// <summary>
    /// 连接成功时的回调
    /// </summary>
    public Action<TClient> SuccessCallback { get; set; }

    /// <summary>
    /// 连接失败时的回调(参数依次为:客户端,本轮尝试重连次数,异常信息)。如果回调返回<see langword="false"/>,则终止尝试下次连接
    /// </summary>
    public Func<TClient, int, Exception, bool> FailCallback { get; set; }
}
