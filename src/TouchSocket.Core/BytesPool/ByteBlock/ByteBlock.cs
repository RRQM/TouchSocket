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
/// 表示一个字节块，提供高效的字节缓冲区操作，支持自动扩容和内存池管理。
/// 实现了<see cref="IByteBlock"/>接口，线程安全。
/// </summary>
/// <remarks>
/// ByteBlock作为引用类型实现，适用于需要在多个方法间传递或长期持有的场景。
/// 支持内存池管理、线程安全的释放操作、自动扩容、读写操作等功能。
/// </remarks>
[DebuggerDisplay("Length={Length},Position={Position},Capacity={Capacity}")]
public sealed class ByteBlock : IByteBlock
{
    #region Common

    private readonly Func<int, Memory<byte>> m_onRent;
    private readonly Action<Memory<byte>> m_onReturn;
    private int m_dis;
    private bool m_holding;
    private int m_length;
    private Memory<byte> m_memory;
    private short m_version;

    /// <summary>
    /// 使用指定内存块初始化<see cref="ByteBlock"/>的新实例。
    /// </summary>
    /// <param name="memory">要使用的内存块。</param>
    public ByteBlock(Memory<byte> memory)
    {
        this.m_memory = memory;
        this.m_length = memory.Length;
    }

    /// <summary>
    /// 使用指定容量和内存管理委托初始化<see cref="ByteBlock"/>的新实例。
    /// </summary>
    /// <param name="capacity">初始容量，最小为1024字节。</param>
    /// <param name="onRent">内存租赁委托。</param>
    /// <param name="onReturn">内存归还委托。</param>
    public ByteBlock(int capacity, Func<int, Memory<byte>> onRent, Action<Memory<byte>> onReturn)
    {
        capacity = Math.Max(capacity, 1024);
        this.m_memory = onRent(capacity);
        this.m_onRent = onRent;
        this.m_onReturn = onReturn;
        this.m_length = 0;
    }

    /// <summary>
    /// 使用指定容量初始化<see cref="ByteBlock"/>的新实例，使用默认的<see cref="ArrayPool{T}"/>进行内存管理。
    /// </summary>
    /// <param name="capacity">初始容量，最小为1024字节。</param>
    public ByteBlock(int capacity)
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
    public long BytesRemaining => this.Length - this.Position;

    /// <inheritdoc/>
    public int CanReadLength => this.Length - this.Position;

    /// <inheritdoc/>
    public int Capacity => this.m_memory.Length;

    /// <inheritdoc/>
    public int FreeLength => this.Capacity - this.Position;

    /// <inheritdoc/>
    public int Length => this.m_length;

    /// <inheritdoc/>
    public ReadOnlyMemory<byte> Memory => this.m_memory.Slice(0, this.m_length);

    /// <inheritdoc/>
    public int Position { get; set; }

    /// <inheritdoc/>
    public ReadOnlySpan<byte> Span => this.Memory.Span;

    /// <inheritdoc/>
    public bool SupportsRewind => true;

    /// <inheritdoc/>
    public Memory<byte> TotalMemory => this.m_memory;

    /// <inheritdoc/>
    public bool Using => this.m_dis == 0;

    /// <inheritdoc/>
    public short Version => this.m_version;

    /// <inheritdoc/>
    long IBytesWriter.WrittenCount => this.Position;

    /// <inheritdoc/>
    public ReadOnlySequence<byte> TotalSequence => new ReadOnlySequence<byte>(this.Memory);

    /// <inheritdoc/>
    public ReadOnlySequence<byte> Sequence => this.TotalSequence.Slice(this.Position);

    /// <inheritdoc/>
    public long BytesRead { get => this.Position; set => this.Position = (int)value; }

    /// <inheritdoc/>
    public void Clear()
    {
        this.m_memory.Span.Clear();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (this.m_holding)
        {
            return;
        }

        if (Interlocked.Increment(ref this.m_dis) == 1)
        {
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
    /// 设置字节块的持有状态，防止在特定情况下被意外释放。
    /// </summary>
    /// <param name="holding">如果为 <see langword="true"/>，则防止释放；如果为 <see langword="false"/>，则允许释放并立即调用 <see cref="Dispose()"/>。</param>
    /// <remarks>
    /// 当设置为 <see langword="false"/> 时，会立即调用 <see cref="Dispose()"/> 方法释放资源。
    /// </remarks>
    public void SetHolding(bool holding)
    {
        this.m_holding = holding;
        if (!holding)
        {
            this.Dispose();
        }
    }

    /// <summary>
    /// 返回当前字节块的UTF-8字符串表示形式。
    /// </summary>
    /// <returns>UTF-8编码的字符串。</returns>
    public override string ToString()
    {
        return this.Span.ToString(Encoding.UTF8);
    }
    #endregion Common

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

    #endregion Reader

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
        if (this.m_onRent == null || this.m_onReturn == null)
        {
            ThrowHelper.ThrowNotSupportedException("不支持扩容");
            return;
        }

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

    private Span<byte> GetCurrentSpan()
    {
        return this.m_memory.Span.Slice(this.Position);
    }

    #endregion Writer
}