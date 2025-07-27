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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace TouchSocket.Core;

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

    public ByteBlock(Memory<byte> memory)
    {
        this.m_memory = memory;
    }

    public ByteBlock(int capacity, Func<int, Memory<byte>> onRent, Action<Memory<byte>> onReturn)
    {
        capacity = Math.Max(capacity, 1024);
        this.m_memory = onRent(capacity);
        this.m_onRent = onRent;
        this.m_onReturn = onReturn;
        this.m_length = 0;
    }

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

    public long BytesRemaining => this.Length - this.Position;

    public int CanReadLength => this.Length - this.Position;

    public int Capacity => this.m_memory.Length;
    public int FreeLength => this.Capacity - this.Position;
    public int Length => this.m_length;

    public ReadOnlyMemory<byte> Memory => this.m_memory.Slice(0, this.m_length);

    public int Position { get; set; }

    public ReadOnlySpan<byte> Span => this.Memory.Span;
    public bool SupportsRewind => true;
    public Memory<byte> TotalMemory => this.m_memory;

    public bool Using => this.m_dis == 0;
    public short Version => this.m_version;
    long IBytesWriter.WrittenCount => this.Position;

    public ReadOnlySequence<byte> TotalSequence => new ReadOnlySequence<byte>(this.Memory);
    public ReadOnlySequence<byte> Sequence => this.TotalSequence.Slice(this.Position);

    public long BytesRead { get => this.Position; set => this.Position = (int)value; }

    public void Clear()
    {
        this.m_memory.Span.Clear();
    }

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

    public void Reset()
    {
        this.Position = 0;
        this.m_length = 0;
    }

    public void SeekToEnd()
    {
        this.Position = this.m_length;
    }

    public void SeekToStart()
    {
        this.Position = 0;
    }

    public void SetHolding(bool holding)
    {
        this.m_holding = holding;
        if (!holding)
        {
            this.Dispose();
        }
    }

    public override string ToString()
    {
        return this.Span.ToString(Encoding.UTF8);
    }

    #endregion Common

    #region Reader

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

    public void Advance(int count)
    {
        this.Position += count;
        this.m_length = this.Position > this.m_length ? this.Position : this.m_length;
    }

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

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        this.ExtendSize(sizeHint);
        return this.m_memory.Slice(this.Position, this.FreeLength);
    }

    ReadOnlyMemory<byte> IBytesReader.GetMemory(int count)
    {
        return this.GetMemory(count).Slice(0, count);
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        return this.GetMemory(sizeHint).Span;
    }

    ReadOnlySpan<byte> IBytesReader.GetSpan(int count)
    {
        return this.GetSpan(count).Slice(0, count);
    }

    public void SetLength(int value)
    {
        if (value > this.m_memory.Length)
        {
            ThrowHelper.ThrowException("设置的长度超过了内存的长度。");
        }
        this.m_length = value;
    }

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