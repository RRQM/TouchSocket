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

using System.IO.Pipelines;
using System.Threading;

namespace TouchSocket.Sockets;

/// <summary>
/// 表示传输层接口，继承自 <see cref="IDuplexPipe"/> 和 <see cref="IClosableClient"/>，用于数据的双向传输和关闭操作。
/// </summary>
public interface ITransport : IDuplexPipe, IClosableClient
{
    /// <summary>
    /// 获取用于写入操作的信号量。
    /// </summary>
    SemaphoreSlim SemaphoreSlimForWriter { get; }

    /// <summary>
    /// 获取连接关闭时的事件参数。
    /// </summary>
    ClosedEventArgs ClosedEventArgs { get; }
}
