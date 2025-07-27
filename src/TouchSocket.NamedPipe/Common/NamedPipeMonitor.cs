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

using System.Threading;
using TouchSocket.Core;

namespace TouchSocket.NamedPipe;

/// <summary>
/// 命名管道监听器
/// </summary>
public class NamedPipeMonitor:DisposableObject
{
    /// <summary>
    /// 命名管道监听器
    /// </summary>
    /// <param name="option"></param>
    public NamedPipeMonitor(NamedPipeListenOption option)
    {
        this.Option = option;
    }

    /// <summary>
    /// 命名管道监听配置
    /// </summary>
    public NamedPipeListenOption Option { get; }

    private CancellationTokenSource m_cancellationTokenSource=new CancellationTokenSource();

    public CancellationToken MonitorToken => m_cancellationTokenSource.Token;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.m_cancellationTokenSource.SafeCancel();
        }
        base.Dispose(disposing);
    }
}