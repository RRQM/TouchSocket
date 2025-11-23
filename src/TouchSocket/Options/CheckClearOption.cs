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
/// 检查清理连接插件配置选项
/// </summary>
/// <typeparam name="TClient">客户端类型</typeparam>
public class CheckClearOption<TClient>
    where TClient : class, IDependencyClient, IClosableClient
{
    /// <summary>
    /// 清理统计类型。默认为:<see cref="CheckClearType.All"/>。当设置为<see cref="CheckClearType.OnlySend"/>时,
    /// 则只检验发送方向是否有数据流动。没有的话则会断开连接。
    /// </summary>
    public CheckClearType CheckClearType { get; set; } = CheckClearType.All;

    /// <summary>
    /// 清理无数据交互的Client,默认60秒。
    /// </summary>
    public TimeSpan Tick { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// 当因为超出时间限定而关闭。
    /// </summary>
    public Func<TClient, CheckClearType, Task> OnClose { get; set; }
}
