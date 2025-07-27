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

public sealed class SegmentedBytesWriter : SafetyDisposableObject, IBytesWriter
{
    private const int MinBufferSize = 1024;
    private BufferSegment m_currentSegment;
    private BufferSegment m_firstSegment;
    private long m_totalBytesWritten;

    public SegmentedBytesWriter(int initialCapacity)
    {
        this.m_firstSegment = new BufferSegment(Math.Max(initialCapacity, MinBufferSize));
        this.m_currentSegment = this.m_firstSegment;
    }

    public ReadOnlySequence<byte> Sequence => this.GetSequence();
    public bool SupportsRewind => false;
    public short Version => 0;
    public long WrittenCount => this.m_totalBytesWritten;

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

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        this.EnsureCapacity(sizeHint);
        return this.m_currentSegment.GetMemory();
    }

    public BytesReader GetReader()
    {
        return new BytesReader(this.Sequence);
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        this.EnsureCapacity(sizeHint);
        return this.m_currentSegment.GetSpan();
    }

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