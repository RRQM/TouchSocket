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
using TouchSocket.Core;

namespace TouchSocket.Sockets;
/// <summary>
/// 表示包含内存数据的事件参数。
/// </summary>
public class MemoryEventArgs : PluginEventArgs
{
    /// <summary>
    /// 使用指定的内存数据初始化 <see cref="MemoryEventArgs"/> 类的新实例。
    /// </summary>
    /// <param name="memory">事件中包含的只读内存数据。</param>
    public MemoryEventArgs(ReadOnlyMemory<byte> memory)
    {
        this.Memory = memory;
    }

    /// <summary>
    /// 获取事件中包含的只读内存数据。
    /// </summary>
    /// <remarks>
    /// 在使用时，不要脱离 <see cref="Memory"/> 的生命周期。
    /// </remarks>
    public ReadOnlyMemory<byte> Memory { get; }
}
