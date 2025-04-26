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
using TouchSocket.Core;

namespace TouchSocket.Sockets;

/// <summary>
/// 终端接口
/// </summary>
public interface IClient : ILoggerObject,IDisposableObject
{
    /// <summary>
    /// 终端协议
    /// </summary>
    Protocol Protocol { get; }

    /// <summary>
    /// 最后一次接收到数据的时间
    /// </summary>
    DateTimeOffset LastReceivedTime { get; }

    /// <summary>
    /// 最后一次发送数据的时间
    /// </summary>
    DateTimeOffset LastSentTime { get; }

    /// <summary>
    /// 表示是否为客户端终端。当为<see langword="true"/>时，表示是客户端。否则为服务器通讯终端。
    /// </summary>
    bool IsClient { get; }
}