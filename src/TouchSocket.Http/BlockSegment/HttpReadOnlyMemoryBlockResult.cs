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

namespace TouchSocket.Http;

public readonly struct HttpReadOnlyMemoryBlockResult : IDisposable
{
    public static readonly HttpReadOnlyMemoryBlockResult Completed = new HttpReadOnlyMemoryBlockResult(ReadOnlyMemory<byte>.Empty);
    private readonly Action m_disposeAction;

    public HttpReadOnlyMemoryBlockResult(Action disposeAction, ReadOnlyMemory<byte> memory, bool isCompleted)
    {
        this.m_disposeAction = disposeAction;
        this.Memory = memory;
        this.IsCompleted = isCompleted;
    }

    public HttpReadOnlyMemoryBlockResult(ReadOnlyMemory<byte> memory)
    {
        this.Memory = memory;
        this.IsCompleted = true;
        this.m_disposeAction = default;
    }

    public bool IsCompleted { get; }
    public ReadOnlyMemory<byte> Memory { get; }

    
    public void Dispose()
    {
        this.m_disposeAction?.Invoke();
    }
}