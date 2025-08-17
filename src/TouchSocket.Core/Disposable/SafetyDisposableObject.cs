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
using System.Threading;

namespace TouchSocket.Core;

/// <summary>
/// 线程安全的释放模型。无论是<see cref="IDisposable"/>还是<see cref="GC"/>执行，都只会触发1次<see cref="SafetyDispose(bool)"/>方法。
/// </summary>
public abstract class SafetyDisposableObject : DisposableObject
{
    private int m_disposed = 0;

    /// <inheritdoc/>
    protected sealed override void Dispose(bool disposing)
    {
        if (this.DisposedValue)
        {
            return;
        }

        if (Interlocked.Exchange(ref this.m_disposed, 1) == 0)
        {
            this.SafetyDispose(disposing);
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// 线程安全模式的释放，无论是<see cref="IDisposable"/>还是<see cref="GC"/>执行，都只会触发一次
    /// </summary>
    protected abstract void SafetyDispose(bool disposing);
}