// ------------------------------------------------------------------------------
// 此代码版权(除特别声明或在XREF结尾的命名空间的代码)归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议,若本仓库没有设置,则按MIT开源协议授权
// CSDN博客:https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频:https://space.bilibili.com/94253567
// Gitee源代码仓库:https://gitee.com/RRQM_Home
// Github源代码仓库:https://github.com/RRQM
// API首页:https://touchsocket.net/
// 交流QQ群:234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System.Buffers;

namespace TouchSocket.Core;

/// <summary>
/// 表示一个分段管道,提供基于内存池的高效读写缓冲区管理
/// </summary>
/// <remarks>
/// SegmentedPipe使用分段式内存管理,从<see cref="ArrayPool{T}"/>按需申请内存段,
/// 读取推进时自动回收已读段。仅支持单线程同步场景。
/// </remarks>
public class SegmentedPipe:DisposableObject
{
    private const int MinBufferSize = 4096;

    private readonly SegmentedPipeWriter m_writer;
    private readonly SegmentedPipeReader m_reader;
    private readonly ArrayPool<byte> m_pool;
    private readonly List<BufferSegment> m_segments;
    private readonly int m_initialCapacity;
    private long m_totalBytesWritten;
    private long m_totalBytesRead;
    private bool m_writerCompleted;
    private bool m_readerCompleted;

    /// <summary>
    /// 初始化<see cref="SegmentedPipe"/>的新实例
    /// </summary>
    /// <param name="initialCapacity">初始容量,最小为4096字节</param>
    /// <param name="pool">用于租用缓冲区的<see cref="ArrayPool{T}"/>实例</param>
    public SegmentedPipe(int initialCapacity = 4096, ArrayPool<byte> pool = null)
    {
        this.m_pool = pool ?? ArrayPool<byte>.Shared;
        this.m_initialCapacity = Math.Max(initialCapacity, MinBufferSize);
        this.m_segments = new List<BufferSegment>();
        this.m_totalBytesWritten = 0;
        this.m_totalBytesRead = 0;
        this.m_writer = new SegmentedPipeWriter(this);
        this.m_reader = new SegmentedPipeReader(this);
    }

    /// <summary>
    /// 获取写入器
    /// </summary>
    public SegmentedPipeWriter Writer => this.m_writer;

    /// <summary>
    /// 获取读取器
    /// </summary>
    public SegmentedPipeReader Reader => this.m_reader;

    /// <summary>
    /// 获取当前已写入但未消费的字节数
    /// </summary>
    public long Count => this.m_totalBytesWritten - this.m_totalBytesRead;

    internal void EnsureCapacity(int sizeHint)
    {
        if (sizeHint <= 0)
        {
            sizeHint = 1024;
        }

        if (this.m_segments.Count == 0)
        {
            this.AddNewSegment(sizeHint > this.m_initialCapacity ? sizeHint : this.m_initialCapacity);
            return;
        }

        var currentSegment = this.m_segments[^1];
        if (sizeHint <= currentSegment.Available)
        {
            return;
        }

        var nextSegmentSize = this.CalculateNextSegmentSize(sizeHint, currentSegment.ActualSize);
        this.AddNewSegment(nextSegmentSize);
    }

    private int CalculateNextSegmentSize(int requestedSize, int previousSize)
    {
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
        var segment = new BufferSegment(this.m_pool, size, this.m_totalBytesWritten);
        this.m_segments.Add(segment);
    }

    internal Memory<byte> GetWriteMemory(int sizeHint)
    {
        this.ThrowIfDisposed();
        if (this.m_writerCompleted)
        {
            ThrowHelper.ThrowInvalidOperationException("Writer has been completed");
        }
        this.EnsureCapacity(sizeHint);
        var currentSegment = this.m_segments[^1];
        return currentSegment.GetMemory();
    }

    internal void AdvanceWriter(int bytes)
    {
        this.ThrowIfDisposed();
        if (bytes < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bytes));
        }

        if (bytes == 0)
        {
            return;
        }

        if (this.m_segments.Count == 0)
        {
            ThrowHelper.ThrowInvalidOperationException("No segments available");
        }

        var currentSegment = this.m_segments[^1];
        if (currentSegment.Available < bytes)
        {
            ThrowHelper.ThrowInvalidOperationException("Cannot advance past buffer end");
        }

        currentSegment.Advance(bytes);
        this.m_totalBytesWritten += bytes;
    }

    internal ReadOnlySequence<byte> GetReadBuffer()
    {
        this.ThrowIfDisposed();
        if (this.m_segments.Count == 0 || this.m_totalBytesRead >= this.m_totalBytesWritten)
        {
            return ReadOnlySequence<byte>.Empty;
        }

        var firstSegmentIndex = this.FindFirstReadableSegmentIndex();
        if (firstSegmentIndex == -1)
        {
            return ReadOnlySequence<byte>.Empty;
        }

        var firstSegment = this.m_segments[firstSegmentIndex];
        var lastSegment = this.m_segments[^1];

        if (firstSegmentIndex == this.m_segments.Count - 1)
        {
            var firstOffset = (int)(this.m_totalBytesRead - firstSegment.StartPosition);
            var readableData = firstSegment.GetReadableMemory(firstOffset);
            return new ReadOnlySequence<byte>(readableData);
        }

        var firstNode = firstSegment.CreateOrGetNode();
        var lastNode = lastSegment.CreateOrGetNode();

        var currentNode = firstNode;
        for (var i = firstSegmentIndex; i < this.m_segments.Count - 1; i++)
        {
            var nextSegment = this.m_segments[i + 1];
            var nextNode = nextSegment.CreateOrGetNode();
            currentNode.Next = nextNode;
            currentNode = nextNode;
        }

        var startOffset = (int)(this.m_totalBytesRead - firstSegment.StartPosition);
        return new ReadOnlySequence<byte>(firstNode, startOffset, lastNode, lastNode.Memory.Length);
    }

    private int FindFirstReadableSegmentIndex()
    {
        for (var i = 0; i < this.m_segments.Count; i++)
        {
            var segment = this.m_segments[i];
            if (this.m_totalBytesRead < segment.StartPosition + segment.WrittenCount)
            {
                return i;
            }
        }
        return -1;
    }

    internal void AdvanceReader(int bytes)
    {
        this.ThrowIfDisposed();
        if (bytes < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bytes));
        }

        if (bytes == 0)
        {
            return;
        }

        if (this.m_totalBytesRead + bytes > this.m_totalBytesWritten)
        {
            ThrowHelper.ThrowInvalidOperationException("Cannot advance past written data");
        }

        this.m_totalBytesRead += bytes;

        while (this.m_segments.Count > 0)
        {
            var firstSegment = this.m_segments[0];
            var segmentEnd = firstSegment.StartPosition + firstSegment.WrittenCount;

            if (this.m_totalBytesRead >= segmentEnd)
            {
                firstSegment.Dispose();
                this.m_segments.RemoveAt(0);
            }
            else
            {
                break;
            }
        }
    }

    internal void CompleteWriter()
    {
        this.ThrowIfDisposed();
        this.m_writerCompleted = true;
    }

    internal void CompleteReader()
    {
        this.ThrowIfDisposed();
        this.m_readerCompleted = true;

        foreach (var segment in this.m_segments)
        {
            segment?.Dispose();
        }
        this.m_segments.Clear();
    }

    internal bool IsWriterCompleted => this.m_writerCompleted;
    internal bool IsReaderCompleted => this.m_readerCompleted;

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var segment in this.m_segments)
            {
                segment?.Dispose();
            }
            this.m_segments.Clear();
        }
        base.Dispose(disposing);
    }

    private sealed class BufferSegment : IDisposable
    {
        private readonly ArrayPool<byte> m_pool;
        private byte[] m_buffer;
        private int m_writtenCount;
        private BufferSegmentNode m_node;

        public long StartPosition { get; }

        public BufferSegment(ArrayPool<byte> pool, int requestedSize, long startPosition)
        {
            this.m_pool = pool;
            this.m_buffer = pool.Rent(requestedSize);
            this.StartPosition = startPosition;
        }

        public int Available => this.m_buffer?.Length - this.m_writtenCount ?? 0;
        public int WrittenCount => this.m_writtenCount;
        public int ActualSize => this.m_buffer?.Length ?? 0;

        public void Advance(int count) => this.m_writtenCount += count;

        public Memory<byte> GetMemory()
        {
            if (this.m_buffer == null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(BufferSegment));
            }

            return this.m_buffer.AsMemory(this.m_writtenCount);
        }

        public ReadOnlyMemory<byte> GetReadableMemory(int offset)
        {
            if (this.m_buffer == null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(BufferSegment));
            }

            return new ReadOnlyMemory<byte>(this.m_buffer, offset, this.m_writtenCount - offset);
        }

        public BufferSegmentNode CreateOrGetNode()
        {
            if (this.m_node == null)
            {
                this.m_node = new BufferSegmentNode
                {
                    Memory = new ReadOnlyMemory<byte>(this.m_buffer, 0, this.m_writtenCount),
                    RunningIndex = this.StartPosition
                };
            }
            else
            {
                this.m_node.Memory = new ReadOnlyMemory<byte>(this.m_buffer, 0, this.m_writtenCount);
            }
            return this.m_node;
        }

        public void Dispose()
        {
            if (this.m_buffer != null)
            {
                this.m_pool.Return(this.m_buffer);
                this.m_buffer = null;
            }
        }
    }

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
}

/// <summary>
/// 表示分段管道的写入器
/// </summary>
public class SegmentedPipeWriter:IBytesWriter
{
    private readonly SegmentedPipe m_pipe;
    private long m_writtenCount;

    /// <inheritdoc/>
    public short Version => 0;

    /// <inheritdoc/>
    public long WrittenCount => this.m_writtenCount;

    /// <inheritdoc/>
    public bool SupportsRewind => false;

    internal SegmentedPipeWriter(SegmentedPipe pipe)
    {
        this.m_pipe = pipe;
    }

    /// <summary>
    /// 获取用于写入的内存缓冲区
    /// </summary>
    /// <param name="sizeHint">期望的最小大小</param>
    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        if (sizeHint < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(sizeHint));
        }

        return this.m_pipe.GetWriteMemory(sizeHint > 0 ? sizeHint : 256);
    }

    /// <summary>
    /// 获取用于写入的Span缓冲区
    /// </summary>
    /// <param name="sizeHint">期望的最小大小</param>
    public Span<byte> GetSpan(int sizeHint = 0)
    {
        return this.GetMemory(sizeHint).Span;
    }

    /// <summary>
    /// 推进写入位置
    /// </summary>
    /// <param name="bytes">已写入的字节数</param>
    public void Advance(int bytes)
    {
        this.m_pipe.AdvanceWriter(bytes);
        this.m_writtenCount += bytes;
    }

    /// <summary>
    /// 完成写入操作
    /// </summary>
    public void Complete()
    {
        this.m_pipe.CompleteWriter();
    }

    /// <summary>
    /// 写入指定的字节数据
    /// </summary>
    /// <param name="source">要写入的数据</param>
    public void Write(ReadOnlySpan<byte> source)
    {
        var memory = this.GetMemory(source.Length);
        source.CopyTo(memory.Span);
        this.Advance(source.Length);
    }
}

/// <summary>
/// 表示分段管道的读取器
/// </summary>
public class SegmentedPipeReader
{
    private readonly SegmentedPipe m_pipe;

    internal SegmentedPipeReader(SegmentedPipe pipe)
    {
        this.m_pipe = pipe;
    }

    /// <summary>
    /// 读取可用数据
    /// </summary>
    public SegmentedPipeReadResult Read()
    {
        var buffer = this.m_pipe.GetReadBuffer();
        var isCompleted = this.m_pipe.IsWriterCompleted;
        return new SegmentedPipeReadResult(buffer, isCompleted);
    }

    /// <summary>
    /// 推进读取位置到指定位置
    /// </summary>
    /// <param name="consumed">已消费的数据位置</param>
    public void AdvanceTo(SequencePosition consumed)
    {
        var buffer = this.m_pipe.GetReadBuffer();
        if (buffer.IsEmpty)
        {
            return;
        }

        var consumedBytes = buffer.Slice(0, consumed).Length;
        this.m_pipe.AdvanceReader((int)consumedBytes);
    }

    /// <summary>
    /// 完成读取操作
    /// </summary>
    public void Complete()
    {
        this.m_pipe.CompleteReader();
    }
}

/// <summary>
/// 表示分段管道的读取结果
/// </summary>
public readonly struct SegmentedPipeReadResult
{
    /// <summary>
    /// 初始化<see cref="SegmentedPipeReadResult"/>的新实例
    /// </summary>
    public SegmentedPipeReadResult(ReadOnlySequence<byte> buffer, bool isCompleted)
    {
        this.Buffer = buffer;
        this.IsCompleted = isCompleted;
    }

    /// <summary>
    /// 获取读取的缓冲区数据
    /// </summary>
    public ReadOnlySequence<byte> Buffer { get; }

    /// <summary>
    /// 获取写入器是否已完成
    /// </summary>
    public bool IsCompleted { get; }
}