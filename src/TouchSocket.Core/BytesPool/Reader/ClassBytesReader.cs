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

namespace TouchSocket.Core;

/// <summary>
/// 表示一个基于类的字节读取器，提供对字节序列的读取功能。
/// 继承自<see cref="DisposableObject"/>并实现<see cref="IBytesReader"/>接口。
/// </summary>
/// <remarks>
/// ClassBytesReader是引用类型实现，适用于需要在多个方法间传递或长期持有的场景。
/// 内部使用缓存机制来优化多段序列的读取性能，对于单段序列提供零拷贝访问。
/// 使用完毕后应调用<see cref="Dispose"/>方法释放可能缓存的内存资源。
/// </remarks>
public class ClassBytesReader : DisposableObject, IBytesReader
{
    private readonly ReadOnlySequence<byte> m_sequence;
    private IMemoryOwner<byte> m_memoryOwner;

    private long m_position = 0;

    /// <summary>
    /// 使用指定的只读字节序列初始化<see cref="ClassBytesReader"/>的新实例。
    /// </summary>
    /// <param name="sequence">要读取的只读字节序列。</param>
    public ClassBytesReader(ReadOnlySequence<byte> sequence)
    {
        this.m_sequence = sequence;
    }

    /// <summary>
    /// 使用指定的只读内存初始化<see cref="ClassBytesReader"/>的新实例。
    /// </summary>
    /// <param name="memory">要读取的只读内存。</param>
    /// <remarks>
    /// 内部会将内存转换为<see cref="ReadOnlySequence{T}"/>进行处理。
    /// </remarks>
    public ClassBytesReader(ReadOnlyMemory<byte> memory)
    {
        this.m_sequence = new ReadOnlySequence<byte>(memory);
    }

    /// <inheritdoc/>
    public long BytesRead { get => this.m_position; set => this.m_position = value; }

    /// <inheritdoc/>
    public long BytesRemaining => this.m_sequence.Length - this.m_position;

    /// <inheritdoc/>
    public ReadOnlySequence<byte> TotalSequence => this.m_sequence;

    /// <inheritdoc/>
    public ReadOnlySequence<byte> Sequence => this.TotalSequence.Slice(this.BytesRead);

    /// <inheritdoc/>
    public void Advance(int count)
    {
        this.m_position += count;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.m_memoryOwner?.Dispose();
            this.m_memoryOwner = default;
        }
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    public ReadOnlyMemory<byte> GetMemory(int count)
    {
        var sequence = this.m_sequence.Slice(this.m_position, count);

        if (sequence.IsSingleSegment)
        {
            return sequence.First; // 零拷贝切片
        }

        var cacheMemory = this.GetCacheMemory(count);
        sequence.CopyTo(cacheMemory.Span);
        return cacheMemory;
    }

    /// <inheritdoc/>
    public ReadOnlySpan<byte> GetSpan(int count)
    {
        return this.GetMemory(count).Span;
    }

    /// <inheritdoc/>
    public int Read(Span<byte> span)
    {
        if (span.IsEmpty)
        {
            return 0;
        }
        var canReadLength = (int)Math.Min(span.Length, this.BytesRemaining);
        var tempSpan = this.GetSpan(canReadLength);
        tempSpan.CopyTo(span);
        this.Advance(canReadLength);
        return canReadLength;
    }

    private Memory<byte> GetCacheMemory(int size)
    {
        if (this.m_memoryOwner == null)
        {
            this.m_memoryOwner = MemoryPool<byte>.Shared.Rent(size);
        }
        else if (this.m_memoryOwner.Memory.Length < size)
        {
            this.m_memoryOwner.Dispose();
            this.m_memoryOwner = MemoryPool<byte>.Shared.Rent(size);
        }
        return this.m_memoryOwner.Memory.Slice(0, size); // 返回精确大小的切片
    }
}