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

namespace TouchSocket.Core;

/// <summary>
/// 读取租约的只读结构体。Dispose 表示消费完成。
/// </summary>
public readonly struct ReadLease<T> : IDisposable
{
    private readonly Action m_owner;

    internal ReadLease(Action disposeAction, T value, bool isCompleted)
    {
        this.m_owner = disposeAction;
        this.Value = value;
        this.IsCompleted = isCompleted;
    }

    public bool IsCompleted { get; }
    public T Value { get; }

    public void Dispose()
    {
        if (!this.IsCompleted && this.m_owner != null)
        {
            this.m_owner.Invoke();
        }
    }
}
