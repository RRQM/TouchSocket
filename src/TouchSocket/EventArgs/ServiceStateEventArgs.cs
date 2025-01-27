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
/// 服务器状态事件参数
/// </summary>
public class ServiceStateEventArgs : MsgPermitEventArgs
{
    /// <summary>
    /// 初始化服务器状态事件参数类
    /// </summary>
    /// <param name="serverState">服务器的状态信息</param>
    /// <param name="exception">与状态变化相关的异常（如果有的话）</param>
    public ServiceStateEventArgs(ServerState serverState, Exception exception)
    {
        this.ServerState = serverState;
        this.Exception = exception;
    }

    /// <summary>
    /// 获取服务器状态
    /// </summary>
    public ServerState ServerState { get; }

    /// <summary>
    /// 获取与状态变化相关的异常信息
    /// </summary>
    public Exception Exception { get; }
}