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
/// 表示一个基于字节序列的高性能字节读取器，提供对<see cref="ReadOnlySequence{T}"/>的读取操作。
/// 实现了<see cref="IBytesReader"/>和<see cref="IDisposable"/>接口。
/// </summary>
/// <remarks>
/// BytesReader作为值类型实现，适用于高频使用且对性能要求较高的场景。
/// 支持单段和多段字节序列的读取，当序列为多段时会自动进行内存合并。
/// 内部使用内存池来优化多段序列的处理性能。
/// </remarks>
public struct BytesReader : IDisposable, IBytesReader
{
    private readonly ReadOnlySequence<byte> m_sequence;
    private IMemoryOwner<byte> m_memoryOwner;

    private long m_position = 0;

    /// <summary>
    /// 使用指定的字节序列初始化<see cref="BytesReader"/>的新实例。
    /// </summary>
    /// <param name="sequence">要读取的字节序列。</param>
    public BytesReader(ReadOnlySequence<byte> sequence)
    {
        this.m_sequence = sequence;
    }

    /// <summary>
    /// 使用指定的内存块初始化<see cref="BytesReader"/>的新实例。
    /// </summary>
    /// <param name="memory">要读取的内存块。</param>
    /// <remarks>
    /// 内存块会被转换为单段字节序列进行处理。
    /// </remarks>
    public BytesReader(ReadOnlyMemory<byte> memory)
    {
        this.m_sequence = new ReadOnlySequence<byte>(memory);
    }

    /// <inheritdoc/>
    public long BytesRead { readonly get => this.m_position; set => this.m_position = value; }

    /// <inheritdoc/>
    public readonly long BytesRemaining => this.m_sequence.Length - this.m_position;

    /// <summary>
    /// 获取总的字节序列。
    /// </summary>
    /// <value>返回完整的字节序列，从索引0开始到序列末尾。</value>
    public readonly ReadOnlySequence<byte> TotalSequence => this.m_sequence;

    /// <inheritdoc/>
    public readonly ReadOnlySequence<byte> Sequence => this.TotalSequence.Slice(this.BytesRead);

    /// <inheritdoc/>
    public void Advance(int count)
    {
        this.m_position += count;
    }

    /// <summary>
    /// 释放<see cref="BytesReader"/>使用的资源。
    /// </summary>
    /// <remarks>
    /// 主要用于释放内部缓存的内存池资源。
    /// </remarks>
    public void Dispose() => this.m_memoryOwner?.Dispose();

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

    /// <summary>
    /// 获取或创建指定大小的缓存内存。
    /// </summary>
    /// <param name="size">所需的内存大小。</param>
    /// <returns>返回一个精确大小的内存切片。</returns>
    /// <remarks>
    /// 使用共享内存池来提高性能，当现有缓存不足时会自动扩容。
    /// 返回的内存大小始终与请求的大小一致。
    /// </remarks>
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