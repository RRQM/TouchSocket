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

namespace TouchSocket.Core;

/// <summary>
/// 环形缓冲区，固定容量的读写操作。
/// </summary>
/// <typeparam name="T">存储的元素类型</typeparam>
public sealed class CircularBuffer<T>
{
    private readonly T[] m_buffer;
    private int m_dataCount;
    private int m_readIndex;
    private int m_writeIndex;

    /// <summary>
    /// 创建指定容量的环形缓冲区。
    /// </summary>
    /// <param name="capacity">缓冲区容量，必须大于0</param>
    public CircularBuffer(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentException("容量必须大于0", nameof(capacity));
        }

        this.Capacity = capacity;
        this.m_buffer = new T[capacity];
        this.m_readIndex = 0;
        this.m_writeIndex = 0;
        this.m_dataCount = 0;
    }

    /// <summary>
    /// 缓冲区总容量（最大可存放的元素数量）。
    /// </summary>
    public int Capacity { get; }

    /// <summary>
    /// 当前缓冲区中可读取的数据数量。
    /// </summary>
    public int DataCount => this.m_dataCount;

    /// <summary>
    /// 是否为空（无可读数据）。
    /// </summary>
    public bool IsEmpty => this.m_dataCount == 0;

    /// <summary>
    /// 是否已满（无法再写入数据）。
    /// </summary>
    public bool IsFull => this.m_dataCount == this.Capacity;

    /// <summary>
    /// 可用的剩余空间（可写入的元素数量）。
    /// </summary>
    public int SpaceFree => this.Capacity - this.m_dataCount;

    /// <summary>
    /// 按相对于当前读位置的偏移获取元素（不移动读指针）。
    /// </summary>
    /// <param name="index">相对读偏移索引，从 0 开始</param>
    /// <returns>指定位置的元素</returns>
    /// <exception cref="IndexOutOfRangeException">当索引超出缓冲区数据范围时抛出</exception>
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= this.m_dataCount)
            {
                throw new IndexOutOfRangeException("索引超出缓冲区数据范围");
            }

            var actualIndex = (this.m_readIndex + index) % this.Capacity;
            return this.m_buffer[actualIndex];
        }
    }

    /// <summary>
    /// 写入后推进写指针。
    /// </summary>
    /// <param name="count">写入数量</param>
    public void AdvanceWrite(int count)
    {
        if (count < 0 || count > this.SpaceFree)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "写入数量超出可用空间范围");
        }
        this.m_writeIndex = (this.m_writeIndex + count) % this.Capacity;
        this.m_dataCount += count;
    }

    /// <summary>
    /// 清空缓冲区，重置读写指针和数据计数。
    /// </summary>
    public void Clear()
    {
        this.m_readIndex = 0;
        this.m_writeIndex = 0;
        this.m_dataCount = 0;
    }

    /// <summary>
    /// 获取可写入的内存块。
    /// </summary>
    /// <returns>可写入的内存块</returns>
    public Memory<T> GetWriteMemory()
    {
        if (this.IsFull)
        {
            return Memory<T>.Empty;
        }
        if (this.m_writeIndex >= this.m_readIndex)
        {
            var spaceAtEnd = this.Capacity - this.m_writeIndex;
            if (spaceAtEnd >= this.SpaceFree)
            {
                return new Memory<T>(this.m_buffer, this.m_writeIndex, this.SpaceFree);
            }
            else
            {
                return new Memory<T>(this.m_buffer, this.m_writeIndex, spaceAtEnd);
            }
        }
        else
        {
            var spaceAvailable = this.m_readIndex - this.m_writeIndex;
            return new Memory<T>(this.m_buffer, this.m_writeIndex, spaceAvailable);
        }
    }

    /// <summary>
    /// 将缓冲区中的数据复制到目标 <see cref="Span{T}"/> 中，但不移动读指针。
    /// </summary>
    /// <param name="span">目标缓冲区</param>
    /// <returns>实际复制的元素数量</returns>
    public int Peek(Span<T> span)
    {
        if (span.IsEmpty || this.m_dataCount == 0)
        {
            return 0;
        }

        var toRead = Math.Min(span.Length, this.m_dataCount);
        if (toRead == 0)
        {
            return 0;
        }

        var bufferSpan = this.m_buffer.AsSpan();
        var dest = span.Slice(0, toRead);
        var tempReadIndex = this.m_readIndex;

        if (tempReadIndex + toRead <= this.Capacity)
        {
            bufferSpan.Slice(tempReadIndex, toRead).CopyTo(dest);
        }
        else
        {
            var firstSegment = this.Capacity - tempReadIndex;
            var secondSegment = toRead - firstSegment;

            bufferSpan.Slice(tempReadIndex, firstSegment).CopyTo(dest);
            bufferSpan.Slice(0, secondSegment).CopyTo(dest.Slice(firstSegment));
        }

        return toRead;
    }

    /// <summary>
    /// 从缓冲区读取数据到目标 <see cref="Span{T}"/> 并移动读指针。
    /// </summary>
    /// <param name="span">目标缓冲区</param>
    /// <returns>实际读取的元素数量</returns>
    public int Read(Span<T> span)
    {
        if (span.IsEmpty || this.m_dataCount == 0)
        {
            return 0;
        }

        var toRead = Math.Min(span.Length, this.m_dataCount);
        if (toRead == 0)
        {
            return 0;
        }

        var bufferSpan = this.m_buffer.AsSpan();
        var dest = span.Slice(0, toRead);

        if (this.m_readIndex + toRead <= this.Capacity)
        {
            bufferSpan.Slice(this.m_readIndex, toRead).CopyTo(dest);
            this.m_readIndex += toRead;
        }
        else
        {
            var firstSegment = this.Capacity - this.m_readIndex;
            var secondSegment = toRead - firstSegment;

            bufferSpan.Slice(this.m_readIndex, firstSegment).CopyTo(dest);
            bufferSpan.Slice(0, secondSegment).CopyTo(dest.Slice(firstSegment));

            this.m_readIndex = secondSegment;
        }

        this.m_dataCount -= toRead;

        if (this.m_dataCount == 0)
        {
            this.m_readIndex = this.m_writeIndex = 0;
        }

        return toRead;
    }

    /// <summary>
    /// 将数据从源 <see cref="ReadOnlySpan{T}"/> 写入缓冲区。
    /// </summary>
    /// <param name="span">源数据</param>
    /// <returns>实际写入的元素数量</returns>
    public int Write(ReadOnlySpan<T> span)
    {
        if (span.IsEmpty)
        {
            return 0;
        }

        var toWrite = Math.Min(span.Length, this.SpaceFree);
        if (toWrite == 0)
        {
            return 0;
        }

        var bufferSpan = this.m_buffer.AsSpan();
        var src = span.Slice(0, toWrite);

        if (this.m_writeIndex + toWrite <= this.Capacity)
        {
            src.CopyTo(bufferSpan.Slice(this.m_writeIndex, toWrite));
            this.m_writeIndex += toWrite;
        }
        else
        {
            var firstSegment = this.Capacity - this.m_writeIndex;
            var secondSegment = toWrite - firstSegment;

            src.Slice(0, firstSegment).CopyTo(bufferSpan.Slice(this.m_writeIndex, firstSegment));
            src.Slice(firstSegment, secondSegment).CopyTo(bufferSpan.Slice(0, secondSegment));

            this.m_writeIndex = secondSegment;
        }

        this.m_dataCount += toWrite;
        return toWrite;
    }
}