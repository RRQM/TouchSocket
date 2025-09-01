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
using System.Buffers;

namespace TouchSocket.Core;

/// <summary>
/// 表示一个分段字节写入器，提供高效的多段缓冲区写入功能。
/// 继承自<see cref="SafetyDisposableObject"/>并实现<see cref="IBytesWriter"/>接口。
/// </summary>
/// <remarks>
/// SegmentedBytesWriter使用链表结构的缓冲段来管理内存，当单个段不足时会自动创建新段。
/// 每个段的最小大小为1024字节，支持动态扩展。所有缓冲区都使用<see cref="ArrayPool{T}.Shared"/>进行内存池管理。
/// </remarks>
public sealed class SegmentedBytesWriter : SafetyDisposableObject, IBytesWriter
{
    private const int MinBufferSize = 1024;
    private BufferSegment m_currentSegment;
    private BufferSegment m_firstSegment;
    private long m_totalBytesWritten;

    /// <summary>
    /// 使用指定的初始容量初始化<see cref="SegmentedBytesWriter"/>的新实例。
    /// </summary>
    /// <param name="initialCapacity">初始容量，最小为1024字节。</param>
    /// <remarks>
    /// 初始容量如果小于1024字节，将自动调整为1024字节。
    /// </remarks>
    public SegmentedBytesWriter(int initialCapacity)
    {
        this.m_firstSegment = new BufferSegment(Math.Max(initialCapacity, MinBufferSize));
        this.m_currentSegment = this.m_firstSegment;
    }

    /// <summary>
    /// 获取当前写入器的字节序列。
    /// </summary>
    /// <value>表示所有已写入数据的<see cref="ReadOnlySequence{T}"/>。</value>
    /// <remarks>
    /// 返回的序列包含所有段中的有效数据，如果没有写入任何数据则返回空序列。
    /// </remarks>
    public ReadOnlySequence<byte> Sequence => this.GetSequence();
    
    /// <inheritdoc/>
    public bool SupportsRewind => false;
    
    /// <inheritdoc/>
    public short Version => 0;
    
    /// <inheritdoc/>
    public long WrittenCount => this.m_totalBytesWritten;

    /// <inheritdoc/>
    public void Advance(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (this.m_currentSegment.Available < count)
        {
            throw new InvalidOperationException("Not enough space in current segment");
        }

        this.m_currentSegment.Advance(count);
        this.m_totalBytesWritten += count;
    }

    /// <summary>
    /// 清空所有段的数据，重置写入器到初始状态。
    /// </summary>
    /// <remarks>
    /// 此方法不会释放已分配的内存段，只是将其重置为可重复使用状态。
    /// </remarks>
    public void Clear()
    {
        var current = this.m_firstSegment;
        while (current != null)
        {
            current.Reset();
            current = current.Next;
        }
        this.m_currentSegment = this.m_firstSegment;
        this.m_totalBytesWritten = 0;
    }

    /// <inheritdoc/>
    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        sizeHint = Math.Max(sizeHint, 16);
        this.EnsureCapacity(sizeHint);
        return this.m_currentSegment.GetMemory();
    }

    /// <summary>
    /// 获取一个字节读取器，用于读取已写入的数据。
    /// </summary>
    /// <returns>基于当前序列的<see cref="BytesReader"/>实例。</returns>
    /// <remarks>
    /// 返回的读取器可以用于读取写入器中的所有数据，读取器的生命周期与写入器无关。
    /// </remarks>
    public BytesReader GetReader()
    {
        return new BytesReader(this.Sequence);
    }

    /// <inheritdoc/>
    public Span<byte> GetSpan(int sizeHint = 0)
    {
        return this.GetMemory(sizeHint).Span;
    }

    /// <inheritdoc/>
    public void Write(scoped ReadOnlySpan<byte> span)
    {
        var length = span.Length;
        if (length == 0)
        {
            return;
        }
        var tempSpan = this.GetSpan(length);
        span.CopyTo(tempSpan);
        this.Advance(length);
    }

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            var current = this.m_firstSegment;
            while (current != null)
            {
                current.Dispose();
                current = current.Next;
            }
        }
    }

    private void EnsureCapacity(int sizeHint)
    {
        sizeHint = sizeHint == 0 ? 1 : sizeHint;
        if (sizeHint <= this.m_currentSegment.Available)
        {
            return;
        }

        if (this.m_currentSegment.WrittenCount == 0)
        {
            var oldCurrentSegment = this.m_currentSegment;
            this.m_currentSegment = oldCurrentSegment.Previous;
            oldCurrentSegment.Dispose();
        }
        var newSegment = new BufferSegment(Math.Max(sizeHint, MinBufferSize));
        if (this.m_currentSegment == null)
        {
            this.m_firstSegment = newSegment;
            this.m_currentSegment = newSegment;
            return;
        }
        this.m_currentSegment.Next = newSegment;
        newSegment.Previous = this.m_currentSegment;
        this.m_currentSegment = newSegment;
    }

    private ReadOnlySequence<byte> GetSequence()
    {
        if (this.m_firstSegment == null || this.m_totalBytesWritten == 0)
            return ReadOnlySequence<byte>.Empty;

        // 创建第一个节点
        var firstNode = new BufferSegmentNode
        {
            Memory = this.m_firstSegment.WrittenMemory,
            RunningIndex = 0
        };

        var currentNode = firstNode;
        var currentSegment = this.m_firstSegment.Next;
        long runningIndex = this.m_firstSegment.WrittenMemory.Length;

        // 遍历所有有数据的段
        while (currentSegment != null && currentSegment.WrittenCount > 0)
        {
            var newNode = new BufferSegmentNode
            {
                Memory = currentSegment.WrittenMemory,
                RunningIndex = runningIndex
            };

            currentNode.Next = newNode;
            currentNode = newNode;

            runningIndex += currentSegment.WrittenMemory.Length;
            currentSegment = currentSegment.Next;
        }

        return new ReadOnlySequence<byte>(firstNode, 0, currentNode, currentNode.Memory.Length);
    }

    #region Class

    private sealed class BufferSegment : IDisposable
    {
        private readonly int m_bufferSize;
        private byte[] m_buffer;
        private int m_writtenCount;

        public BufferSegment(int size)
        {
            this.m_buffer = ArrayPool<byte>.Shared.Rent(size);
            this.m_bufferSize = size;
        }

        public int Available => this.m_bufferSize - this.m_writtenCount;
        public BufferSegment Next { get; set; }
        public BufferSegment Previous { get; set; }
        public int WrittenCount => this.m_writtenCount;
        public ReadOnlyMemory<byte> WrittenMemory => new ReadOnlyMemory<byte>(this.m_buffer, 0, this.m_writtenCount);

        public void Advance(int count) => this.m_writtenCount += count;

        public void Dispose()
        {
            if (this.m_buffer != null)
            {
                ArrayPool<byte>.Shared.Return(this.m_buffer);
                this.m_buffer = null;
            }
        }

        public Memory<byte> GetMemory() => this.m_buffer.AsMemory(this.m_writtenCount);

        public Span<byte> GetSpan() => this.m_buffer.AsSpan(this.m_writtenCount);

        public void Reset()
        {
            this.m_writtenCount = 0;
        }
    }

    // 自定义的ReadOnlySequenceSegment实现
    private sealed class BufferSegmentNode : ReadOnlySequenceSegment<byte>
    {
        public new ReadOnlyMemory<byte> Memory
        {
            get => base.Memory;
            set => base.Memory = value;
        }

        public new BufferSegmentNode Next
        {
            get => (BufferSegmentNode)base.Next;
            set => base.Next = value;
        }

        public new long RunningIndex
        {
            get => base.RunningIndex;
            set => base.RunningIndex = value;
        }
    }

    #endregion Class
}