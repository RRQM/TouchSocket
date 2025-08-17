//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;

namespace TouchSocket.Sockets;

/// <summary>
/// SendingEventArgs 类，继承自 PluginEventArgs，用于封装待发送数据的内存块。
/// </summary>
public class SendingEventArgs : MemoryEventArgs
{
    /// <summary>
    /// 初始化 SendingEventArgs 类的新实例。
    /// </summary>
    /// <param name="memory">待发送数据的只读内存块。</param>
    public SendingEventArgs(in ReadOnlyMemory<byte> memory) : base(memory)
    {

    }
}