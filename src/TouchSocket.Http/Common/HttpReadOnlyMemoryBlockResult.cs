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

using System.Buffers;

namespace TouchSocket.Http;

/// <summary>
/// 表示只读内存块的结果，支持释放资源。
/// </summary>
public readonly struct HttpReadOnlyMemoryBlockResult : IDisposable
{
    /// <summary>
    /// 获取已完成的只读内存块结果。
    /// </summary>
    public static readonly HttpReadOnlyMemoryBlockResult Completed = new HttpReadOnlyMemoryBlockResult(ReadOnlySequence<byte>.Empty, true);

    private readonly Action m_dispose;
    private readonly IMemoryOwner<byte> m_memoryOwner;

    /// <summary>
    /// 初始化 <see cref="HttpReadOnlyMemoryBlockResult"/> 结构的新实例。
    /// </summary>
    /// <param name="memories">只读字节序列。</param>
    /// <param name="isCompleted">指示是否已完成。</param>
    public HttpReadOnlyMemoryBlockResult(ReadOnlySequence<byte> memories, bool isCompleted)
    {
        this.IsCompleted = isCompleted;
        var length = (int)memories.Length;
        this.m_memoryOwner = MemoryPool<byte>.Shared.Rent(length);

        memories.CopyTo(this.m_memoryOwner.Memory.Span);
        this.Memory = this.m_memoryOwner.Memory.Slice(0, length);
    }

    /// <summary>
    /// 初始化 <see cref="HttpReadOnlyMemoryBlockResult"/> 结构的新实例。
    /// </summary>
    /// <param name="dispose">释放资源的委托。</param>
    /// <param name="memory">只读内存块。</param>
    /// <param name="isCompleted">指示是否已完成。</param>
    public HttpReadOnlyMemoryBlockResult(Action dispose, ReadOnlyMemory<byte> memory, bool isCompleted)
    {
        this.m_dispose = dispose;
        this.Memory = memory;
        this.IsCompleted = isCompleted;
    }

    /// <summary>
    /// 获取一个值，指示操作是否已完成。
    /// </summary>
    public bool IsCompleted { get; }

    /// <summary>
    /// 获取只读内存块。
    /// </summary>
    public ReadOnlyMemory<byte> Memory { get; }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.m_memoryOwner?.Dispose();
        this.m_dispose?.Invoke();
    }
}