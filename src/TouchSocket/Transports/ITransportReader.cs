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
/// 传输读取器接口，提供管道读取器和读取锁定器
/// </summary>
public interface ITransportReader
{
    /// <summary>
    /// 获取管道读取器，用于从传输层读取数据
    /// </summary>
    PipeReader Reader { get; }

    /// <summary>
    /// 获取读取锁定器，用于同步读取操作
    /// </summary>
    SemaphoreSlim ReadLocker { get; }
}
