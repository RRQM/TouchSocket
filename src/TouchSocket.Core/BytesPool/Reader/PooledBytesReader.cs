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
using System.Runtime.CompilerServices;

namespace TouchSocket.Core;

/// <summary>
/// 表示一个使用内存池的字节读取器。
/// <inheritdoc />
/// </summary>
public sealed class PooledBytesReader : IDisposable, IBytesReader
{
    private IMemoryOwner<byte> m_memoryOwner;
    private long m_position = 0;
    private ReadOnlySequence<byte> m_sequence;

    /// <inheritdoc />
    public long BytesRead { get => this.m_position; set => this.m_position = value; }

    /// <inheritdoc />
    public long BytesRemaining => this.m_sequence.Length - this.m_position;

    /// <inheritdoc />
    public ReadOnlySequence<byte> Sequence => this.TotalSequence.Slice(this.BytesRead);

    /// <inheritdoc />
    public ReadOnlySequence<byte> TotalSequence => this.m_sequence;

    /// <inheritdoc />
    public void Advance(int count)
    {
        this.m_position += count;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.Clear();
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public ReadOnlySpan<byte> GetSpan(int count)
    {
        return this.GetMemory(count).Span;
    }

    /// <inheritdoc />
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
    /// 重置读取器，使用新的字节序列。
    /// </summary>
    /// <param name="sequence">新的字节序列。</param>
    public void Reset(ReadOnlySequence<byte> sequence)
    {
        this.m_position = 0;
        this.m_sequence = sequence;
    }

    /// <summary>
    /// 重置读取器，使用新的只读内存。
    /// </summary>
    /// <param name="memory">新的只读内存。</param>
    public void Reset(ReadOnlyMemory<byte> memory)
    {
        this.m_position = 0;
        this.m_sequence = new ReadOnlySequence<byte>(memory);
    }

    /// <summary>
    /// 清理内存所有者。
    /// </summary>
    public void Clear()
    {
        this.m_memoryOwner?.Dispose();
        this.m_memoryOwner = default;
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