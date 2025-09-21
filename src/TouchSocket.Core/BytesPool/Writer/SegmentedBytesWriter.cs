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
#if CollectionsMarshal
using System.Runtime.InteropServices;
#endif

namespace TouchSocket.Core;

/// <summary>
/// 表示一个分段字节写入器，提供高效的多段缓冲区写入功能。
/// </summary>
/// <remarks>
/// SegmentedBytesWriter使用列表结构的缓冲段来管理内存，当单个段不足时会自动创建新段。
/// 每个段的最小大小为4096字节，支持动态扩展。所有缓冲区都使用<see cref="ArrayPool{T}.Shared"/>进行内存池管理。
/// </remarks>
public sealed class SegmentedBytesWriter : DisposableObject, IBytesWriter
{
    private const int MinBufferSize = 4096; // 增加最小缓冲区大小

    private readonly List<BufferSegment> m_segments; // 使用可扩展列表以支持动态增长
    private readonly ArrayPool<byte> m_pool;
    private readonly int m_initialCapacity;
    private long m_totalBytesWritten;

    // 维护增量构建的序列节点，避免每次重建链表
    private BufferSegmentNode m_firstNode;
    private BufferSegmentNode m_lastNode;


    /// <summary>
    /// 初始化 <see cref="SegmentedBytesWriter"/> 类的新实例，使用默认的最小缓冲区大小（4096字节）。
    /// </summary>
    public SegmentedBytesWriter() : this(MinBufferSize)
    {
    }
    /// <summary>
    /// 使用指定的初始容量初始化<see cref="SegmentedBytesWriter"/>的新实例。
    /// </summary>
    /// <param name="initialCapacity">初始容量，最小为4096字节。</param>
    /// <remarks>
    /// 初始容量如果小于4096字节，将自动调整为4096字节。
    /// </remarks>
    public SegmentedBytesWriter(int initialCapacity) : this(initialCapacity, ArrayPool<byte>.Shared)
    {
    }

    /// <summary>
    /// 使用指定的初始容量和数组池初始化<see cref="SegmentedBytesWriter"/>的新实例。
    /// </summary>
    /// <param name="initialCapacity">初始容量，最小为4096字节。</param>
    /// <param name="pool">用于租用缓冲区的<see cref="ArrayPool{T}"/>实例。</param>
    /// <remarks>
    /// 初始容量如果小于4096字节，将自动调整为4096字节。
    /// </remarks>
    public SegmentedBytesWriter(int initialCapacity, ArrayPool<byte> pool)
    {
        this.m_pool = pool ?? ArrayPool<byte>.Shared;
        // 为基准测试优化：确保初始容量足够大以避免频繁分配
        this.m_initialCapacity = Math.Max(initialCapacity, MinBufferSize);
        this.m_segments = new List<BufferSegment>();

        this.AddNewSegment(this.m_initialCapacity);
    }

    /// <summary>
    /// 获取当前写入器的字节序列。
    /// </summary>
    public ReadOnlySequence<byte> Sequence => this.GetSequence();

    /// <summary>
    /// 获取已写入的字节数。
    /// </summary>
    public long WrittenCount => this.m_totalBytesWritten;

    /// <inheritdoc/>
    public short Version => 0;

    /// <inheritdoc/>
    public bool SupportsRewind => false;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        if (count < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(count), count, 0);
        }

        if (count == 0)
        {
            return;
        }

#if CollectionsMarshal
        Span<BufferSegment> segSpan = CollectionsMarshal.AsSpan(this.m_segments);
        if (segSpan.Length == 0)
        {
            ThrowHelper.ThrowInvalidOperationException("No segments available");
        }

        var currentSegment = segSpan[^1];
#else
        if (this.m_segments.Count == 0)
        {
            ThrowHelper.ThrowInvalidOperationException("No segments available");
        }

        var currentSegment = this.m_segments[^1];
#endif
        if (currentSegment.Available < count)
        {
            ThrowHelper.ThrowInvalidOperationException("Not enough space in current segment");
        }

        currentSegment.Advance(count);
        // 更新节点内存范围
        currentSegment.Node.Memory = currentSegment.WrittenMemory;

        this.m_totalBytesWritten += count;
    }

    /// <summary>
    /// 清空所有段的数据，重置写入器到初始状态。
    /// </summary>
    public void Clear()
    {
#if CollectionsMarshal
        Span<BufferSegment> segSpan = CollectionsMarshal.AsSpan(this.m_segments);
        if (segSpan.Length == 0)
        {
            return;
        }

        // 保留第一个段，释放其他段
        for (var i = 1; i < segSpan.Length; i++)
        {
            segSpan[i]?.Dispose();
        }
#else
        if (this.m_segments.Count == 0)
        {
            return;
        }

        // 保留第一个段，释放其他段
        for (var i = 1; i < this.m_segments.Count; i++)
        {
            this.m_segments[i]?.Dispose();
        }
#endif
        if (this.m_segments.Count > 1)
        {
            this.m_segments.RemoveRange(1, this.m_segments.Count - 1);
        }

        // 重置第一个段
#if CollectionsMarshal
        segSpan = CollectionsMarshal.AsSpan(this.m_segments); // 重新获取以确保一致性
        var first = segSpan[0];
#else
        var first = this.m_segments[0];
#endif
        first?.Reset();
        if (first is not null)
        {
            first.Node.Memory = first.WrittenMemory; // 应为长度为0
        }

        // 重置序列头尾
        this.m_firstNode = first?.Node;
        this.m_lastNode = this.m_firstNode;

        this.m_totalBytesWritten = 0;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        if (sizeHint <= 0)
        {
            sizeHint = 1024; // 增加默认值
        }

        this.EnsureCapacity(sizeHint);
        return this.GetCurrentSegment().GetMemory();
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> GetSpan(int sizeHint = 0)
    {
        return this.GetMemory(sizeHint).Span;
    }

    /// <summary>
    /// 将指定的字节范围写入到缓冲区中。
    /// </summary>
    /// <param name="span">要写入的字节数据。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(scoped ReadOnlySpan<byte> span)
    {
        var length = span.Length;
        if (length == 0)
        {
            return;
        }

        // 优化：预检查当前段是否有足够空间，避免不必要的分支
#if CollectionsMarshal
        Span<BufferSegment> segSpan = CollectionsMarshal.AsSpan(this.m_segments);
        if (segSpan.Length > 0)
        {
            var currentSegment = segSpan[^1];
            if (currentSegment.Available >= length)
            {
                var targetSpan = currentSegment.GetSpan();
                span.CopyTo(targetSpan);
                currentSegment.Advance(length);
                this.m_totalBytesWritten += length;

                // 更新节点内存范围
                currentSegment.Node.Memory = currentSegment.WrittenMemory;
                return;
            }
        }
#else
        if (this.m_segments.Count > 0)
        {
            var currentSegment = this.m_segments[^1];
            if (currentSegment.Available >= length)
            {
                var targetSpan = currentSegment.GetSpan();
                span.CopyTo(targetSpan);
                currentSegment.Advance(length);
                this.m_totalBytesWritten += length;

                // 更新节点内存范围
                currentSegment.Node.Memory = currentSegment.WrittenMemory;
                return;
            }
        }
#endif

        // 需要跨段写入时才调用复杂逻辑
        this.WriteAcrossSegments(span);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void WriteAcrossSegments(ReadOnlySpan<byte> span)
    {
        var remaining = span.Length;
        var sourceOffset = 0;

        while (remaining > 0)
        {
            // 确保有足够的容量（此调用可能修改 List，故不要跨调用持有 span）
            this.EnsureCapacity(remaining);

#if CollectionsMarshal
            Span<BufferSegment> segSpan = CollectionsMarshal.AsSpan(this.m_segments);
            var currentSegment = segSpan[^1];
#else
            var currentSegment = this.m_segments[^1];
#endif
            var available = currentSegment.Available;

            if (available == 0)
            {
                // 这种情况不应该发生，因为 EnsureCapacity 应该保证有空间
                ThrowHelper.ThrowInvalidOperationException("No available space after ensuring capacity");
            }

            var copyCount = remaining < available ? remaining : available;
            var sourceSlice = span.Slice(sourceOffset, copyCount);
            var targetSpan = currentSegment.GetSpan();

            sourceSlice.CopyTo(targetSpan);
            currentSegment.Advance(copyCount);
            currentSegment.Node.Memory = currentSegment.WrittenMemory;

            this.m_totalBytesWritten += copyCount;
            remaining -= copyCount;
            sourceOffset += copyCount;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BufferSegment GetCurrentSegment()
    {
#if CollectionsMarshal
        Span<BufferSegment> segSpan = CollectionsMarshal.AsSpan(this.m_segments);
        if (segSpan.Length == 0)
        {
            ThrowHelper.ThrowInvalidOperationException("No segments available");
        }
        return segSpan[^1];
#else
        if (this.m_segments.Count == 0)
        {
            ThrowHelper.ThrowInvalidOperationException("No segments available");
        }
        return this.m_segments[^1];
#endif
    }

    private void EnsureCapacity(int sizeHint)
    {
        if (sizeHint <= 0)
        {
            sizeHint = 1;
        }

#if CollectionsMarshal
        Span<BufferSegment> segSpan = CollectionsMarshal.AsSpan(this.m_segments);
        if (segSpan.Length == 0)
        {
            this.AddNewSegment(sizeHint > this.m_initialCapacity ? sizeHint : this.m_initialCapacity);
            return;
        }

        var currentSegment = segSpan[^1];
#else
        if (this.m_segments.Count == 0)
        {
            this.AddNewSegment(sizeHint > this.m_initialCapacity ? sizeHint : this.m_initialCapacity);
            return;
        }

        var currentSegment = this.m_segments[^1];
#endif
        if (sizeHint <= currentSegment.Available)
        {
            return;
        }

        // 计算下一个段的大小：按需与指数增长结合，减少分段次数
        var nextSegmentSize = CalculateNextSegmentSize(sizeHint, currentSegment.ActualSize);
        this.AddNewSegment(nextSegmentSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CalculateNextSegmentSize(int requestedSize, int previousSize)
    {
        // 使用指数增长（上次大小*2），并满足本次需求，受最大值限制
        var grow = previousSize > 0 ? previousSize << 1 : MinBufferSize;
        var baseSize = requestedSize > grow ? requestedSize : grow;
        if (baseSize < MinBufferSize)
        {
            baseSize = MinBufferSize;
        }

        return baseSize;
    }

    private void AddNewSegment(int size)
    {
        var segment = new BufferSegment(this.m_pool, size);
        this.m_segments.Add(segment);

        // 建立并链接节点；其起始索引为当前总写入量（之前段已定型）
        var node = new BufferSegmentNode
        {
            Memory = segment.WrittenMemory, // 初始为长度0
            RunningIndex = this.m_totalBytesWritten
        };

        segment.Node = node;

        if (this.m_firstNode is null)
        {
            this.m_firstNode = node;
            this.m_lastNode = node;
        }
        else
        {
            this.m_lastNode!.Next = node;
            this.m_lastNode = node;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ReadOnlySequence<byte> GetSequence()
    {
        if (this.m_firstNode is null || this.m_lastNode is null)
        {
            return ReadOnlySequence<byte>.Empty;
        }
        return new ReadOnlySequence<byte>(this.m_firstNode, 0, this.m_lastNode, this.m_lastNode.Memory.Length);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
#if CollectionsMarshal
            Span<BufferSegment> segSpan = CollectionsMarshal.AsSpan(this.m_segments);
            for (var i = 0; i < segSpan.Length; i++)
            {
                segSpan[i]?.Dispose();
            }
#else
            for (var i = 0; i < this.m_segments.Count; i++)
            {
                this.m_segments[i]?.Dispose();
            }
#endif
            this.m_segments.Clear();
            this.m_firstNode = null;
            this.m_lastNode = null;
        }
        base.Dispose(disposing);
    }

    #region Class

    private sealed class BufferSegment : IDisposable
    {
        private readonly ArrayPool<byte> m_pool;
        private byte[] m_buffer;
        private int m_writtenCount;

        public BufferSegmentNode Node { get; set; } = null!;

        public BufferSegment(ArrayPool<byte> pool, int requestedSize)
        {
            this.m_pool = pool;
            this.m_buffer = pool.Rent(requestedSize);
        }

        public int Available => this.m_buffer.Length - this.m_writtenCount;
        public int WrittenCount => this.m_writtenCount;
        public int ActualSize => this.m_buffer?.Length ?? 0;
        public ReadOnlyMemory<byte> WrittenMemory => new ReadOnlyMemory<byte>(this.m_buffer, 0, this.m_writtenCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count) => this.m_writtenCount += count;

        public void Dispose()
        {
            if (this.m_buffer != null)
            {
                this.m_pool.Return(this.m_buffer);
                this.m_buffer = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<byte> GetMemory() => this.m_buffer.AsMemory(this.m_writtenCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> GetSpan() => this.m_buffer.AsSpan(this.m_writtenCount);

        public void Reset()
        {
            this.m_writtenCount = 0;
        }
    }

    // 简化的ReadOnlySequenceSegment实现
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