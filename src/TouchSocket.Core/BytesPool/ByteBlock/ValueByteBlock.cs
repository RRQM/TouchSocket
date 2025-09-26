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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TouchSocket.Core;

/// <summary>
/// 表示一个值类型的字节块，提供高性能的字节缓冲区操作，避免堆分配开销。
/// 实现了<see cref="IByteBlock"/>接口。
/// </summary>
/// <remarks>
/// ValueByteBlock作为值类型实现，适用于高频使用且对性能要求较高的场景。
/// 支持内存池管理、自动扩容、读写操作等功能。
/// 注意：由于是值类型，在多线程环境下使用时需要特别小心。
/// </remarks>
[DebuggerDisplay("Length={Length},Position={Position},Capacity={Capacity}")]
public struct ValueByteBlock : IByteBlock
{
    #region Common
    private readonly Func<int, Memory<byte>> m_onRent;
    private readonly Action<Memory<byte>> m_onReturn;
    private bool m_dis;
    private int m_length;
    private Memory<byte> m_memory;
    private short m_version;

    /// <summary>
    /// 使用指定内存块初始化<see cref="ValueByteBlock"/>的新实例。
    /// </summary>
    /// <param name="memory">要使用的内存块。</param>
    public ValueByteBlock(Memory<byte> memory)
    {
        this.m_memory = memory;
    }

    /// <summary>
    /// 使用指定容量和内存管理委托初始化<see cref="ValueByteBlock"/>的新实例。
    /// </summary>
    /// <param name="capacity">初始容量，最小为1024字节。</param>
    /// <param name="onRent">内存租赁委托。</param>
    /// <param name="onReturn">内存归还委托。</param>
    public ValueByteBlock(int capacity, Func<int, Memory<byte>> onRent, Action<Memory<byte>> onReturn)
    {
        capacity = Math.Max(capacity, 1024);
        this.m_memory = onRent(capacity);
        this.m_onRent = onRent;
        this.m_onReturn = onReturn;
        this.m_length = 0;
    }

    /// <summary>
    /// 使用指定容量初始化<see cref="ValueByteBlock"/>的新实例，使用默认的<see cref="ArrayPool{T}"/>进行内存管理。
    /// </summary>
    /// <param name="capacity">初始容量，最小为1024字节。</param>
    public ValueByteBlock(int capacity)
    {
        capacity = Math.Max(capacity, 1024);
        this.m_onRent = (c) =>
        {
            return ArrayPool<byte>.Shared.Rent(c);
        };
        this.m_onReturn = (m) =>
        {
            if (MemoryMarshal.TryGetArray((ReadOnlyMemory<byte>)m, out var result))
            {
                ArrayPool<byte>.Shared.Return(result.Array);
            }
        };

        this.m_memory = this.m_onRent(capacity);
    }

    /// <inheritdoc/>
    public long BytesRead { readonly get => this.Position; set => this.Position = (int)value; }

    /// <inheritdoc/>
    public readonly long BytesRemaining => this.Length - this.Position;

    /// <inheritdoc/>
    public readonly int CanReadLength => this.Length - this.Position;

    /// <inheritdoc/>
    public readonly int Capacity => this.m_memory.Length;

    /// <inheritdoc/>
    public readonly int FreeLength => this.Capacity - this.Position;

    /// <summary>
    /// 获取一个值，该值指示内存块是否为空。
    /// </summary>
    /// <value>如果内存块为空，则为 <see langword="true"/>；否则为 <see langword="false"/>。</value>
    public readonly bool IsEmpty => this.m_memory.IsEmpty;

    /// <inheritdoc/>
    public readonly int Length => this.m_length;

    /// <inheritdoc/>
    public readonly ReadOnlyMemory<byte> Memory => this.m_memory.Slice(0, this.m_length);

    /// <inheritdoc/>
    public int Position { get; set; }

    /// <inheritdoc/>
    public readonly ReadOnlySpan<byte> Span => this.Memory.Span;

    /// <inheritdoc/>
    public readonly bool SupportsRewind => true;

    /// <inheritdoc/>
    public readonly Memory<byte> TotalMemory => this.m_memory;

    /// <inheritdoc/>
    public readonly bool Using => !this.m_dis;

    /// <inheritdoc/>
    public readonly short Version => this.m_version;

    /// <inheritdoc/>
    public readonly ReadOnlySequence<byte> TotalSequence => new ReadOnlySequence<byte>(this.Memory);

    /// <inheritdoc/>
    public readonly ReadOnlySequence<byte> Sequence => this.TotalSequence.Slice(this.Position);

    /// <inheritdoc/>
    public readonly long WrittenCount => this.Position;

    /// <inheritdoc/>
    public readonly void Clear()
    {
        this.m_memory.Span.Clear();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (this.m_dis)
        {
            return;
        }
        this.m_dis = true;
        var memory = this.m_memory;
        if (memory.IsEmpty)
        {
            return;
        }

        this.m_memory = default;
        this.m_length = 0;
        this.Position = 0;

        this.m_onReturn?.Invoke(memory);
    }

    /// <inheritdoc/>
    public void Reset()
    {
        this.Position = 0;
        this.m_length = 0;
    }

    /// <summary>
    /// 将位置设置到数据末尾。
    /// </summary>
    public void SeekToEnd()
    {
        this.Position = this.m_length;
    }

    /// <summary>
    /// 将位置设置到数据开头。
    /// </summary>
    public void SeekToStart()
    {
        this.Position = 0;
    }

    /// <summary>
    /// 返回当前字节块的UTF-8字符串表示形式。
    /// </summary>
    /// <returns>UTF-8编码的字符串。</returns>
    public override readonly string ToString()
    {
        return this.Span.ToString(Encoding.UTF8);
    }
    #endregion

    #region Reader

    /// <inheritdoc/>
    public int Read(Span<byte> span)
    {
        if (span.IsEmpty)
        {
            return 0;
        }

        var length = Math.Min(this.CanReadLength, span.Length);

        this.Span.Slice(this.Position, length).CopyTo(span);
        this.Position += length;
        return length;
    }

    #endregion

    #region Writer

    /// <inheritdoc/>
    public void Advance(int count)
    {
        this.Position += count;
        this.m_length = this.Position > this.m_length ? this.Position : this.m_length;
    }

    /// <summary>
    /// 扩展内存块大小以满足指定的空间需求。
    /// </summary>
    /// <param name="size">需要的额外空间大小。</param>
    /// <exception cref="NotSupportedException">当未提供内存管理委托时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ExtendSize(int size)
    {
        if (this.FreeLength < size)
        {
            var need = this.Capacity + size - (this.Capacity - this.Position);
            long lend = this.Capacity;
            while (need > lend)
            {
                lend *= 2;
            }

            if (lend > int.MaxValue)
            {
                lend = Math.Min(need + 1024 * 1024 * 100, int.MaxValue);
            }

            if (this.m_onRent == null || this.m_onReturn == null)
            {
                ThrowHelper.ThrowNotSupportedException("不支持扩容");
                return;
            }


            var bytes = this.m_onRent((int)lend);

            this.m_memory.CopyTo(bytes);

            this.m_onReturn(this.m_memory);
            this.m_memory = bytes;

            this.m_version++;
        }
    }

    /// <inheritdoc/>
    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        this.ExtendSize(sizeHint);
        return this.m_memory.Slice(this.Position, this.FreeLength);
    }

    /// <inheritdoc/>
    ReadOnlyMemory<byte> IBytesReader.GetMemory(int count)
    {
        return this.GetMemory(count).Slice(0, count);
    }

    /// <inheritdoc/>
    public Span<byte> GetSpan(int sizeHint = 0)
    {
        return this.GetMemory(sizeHint).Span;
    }

    /// <inheritdoc/>
    ReadOnlySpan<byte> IBytesReader.GetSpan(int count)
    {
        return this.GetSpan(count).Slice(0, count);
    }

    /// <inheritdoc/>
    public void SetLength(int value)
    {
        if (value > this.m_memory.Length)
        {
            ThrowHelper.ThrowException("设置的长度超过了内存的长度。");
        }
        this.m_length = value;
    }

    /// <inheritdoc/>
    public void Write(scoped ReadOnlySpan<byte> span)
    {
        if (span.IsEmpty)
        {
            return;
        }

        this.ExtendSize(span.Length);

        var currentSpan = this.GetCurrentSpan();
        span.CopyTo(currentSpan);
        this.Position += span.Length;
        this.m_length = Math.Max(this.Position, this.m_length);
    }

    private readonly Span<byte> GetCurrentSpan()
    {
        return this.m_memory.Span.Slice(this.Position);
    }
    #endregion
}

