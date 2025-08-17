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
/// ReceiverResult
/// </summary>
internal class InternalReceiverResult : IReceiverResult
{
    private readonly Action m_disAction;

    /// <summary>
    /// ReceiverResult
    /// </summary>
    /// <param name="disAction"></param>
    public InternalReceiverResult(Action disAction)
    {
        this.m_disAction = disAction;
    }

    /// <summary>
    /// 字节块
    /// </summary>
    public ReadOnlyMemory<byte> Memory { get; set; }

    /// <summary>
    /// 数据对象
    /// </summary>
    public IRequestInfo RequestInfo { get; set; }

    public bool IsCompleted { get; set; }

    public string Message { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.m_disAction.Invoke();
    }
}