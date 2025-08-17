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

namespace TouchSocket.Core;

[DebuggerDisplay("Length={Length},Position={Position},Capacity={Capacity}")]
public struct ValueByteBlock : IByteBlock, IBytesReader, IBytesWriter
{
    #region Common
    private readonly Func<int, Memory<byte>> m_onRent;
    private readonly Action<Memory<byte>> m_onReturn;
    private bool m_dis;
    private int m_length;
    private Memory<byte> m_memory;
    private short m_version;
    public ValueByteBlock(Memory<byte> memory)
    {
        this.m_memory = memory;
    }

    public ValueByteBlock(int capacity, Func<int, Memory<byte>> onRent, Action<Memory<byte>> onReturn)
    {
        capacity = Math.Max(capacity, 1024);
        this.m_memory = onRent(capacity);
        this.m_onRent = onRent;
        this.m_onReturn = onReturn;
        this.m_length = 0;
    }

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

    public long BytesRead { readonly get => this.Position; set => this.Position = (int)value; }
    public readonly long BytesRemaining => this.Length - this.Position;
    public readonly int CanReadLength => this.Length - this.Position;
    public readonly int Capacity => this.m_memory.Length;
    public readonly int FreeLength => this.Capacity - this.Position;
    public readonly bool IsEmpty => this.m_memory.IsEmpty;
    public readonly int Length => this.m_length;
    public readonly ReadOnlyMemory<byte> Memory => this.m_memory.Slice(0, this.m_length);
    public int Position { get; set; }
    public readonly ReadOnlySpan<byte> Span => this.Memory.Span;
    public readonly bool SupportsRewind => true;
    public readonly Memory<byte> TotalMemory => this.m_memory;
    public readonly bool Using => !this.m_dis;
    public readonly short Version => this.m_version;

    public readonly ReadOnlySequence<byte> TotalSequence => new ReadOnlySequence<byte>(this.Memory);
    public readonly ReadOnlySequence<byte> Sequence => this.TotalSequence.Slice(this.Position);
    public readonly long WrittenCount => this.Position;
    public readonly void Clear()
    {
        this.m_memory.Span.Clear();
    }

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
    public override readonly string ToString()
    {
        return this.Span.ToString(Encoding.UTF8);
    }
    #endregion

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


    #endregion

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

    private readonly Span<byte> GetCurrentSpan()
    {
        return this.m_memory.Span.Slice(this.Position);
    }
    #endregion
}

