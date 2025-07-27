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
using System.Collections.Generic;

namespace TouchSocket.Core;

public struct BytesReader : IDisposable, IBytesReader
{
    private readonly ReadOnlySequence<byte> m_sequence;
    private IMemoryOwner<byte> m_memoryOwner;

    private long m_position = 0;

    public BytesReader(ReadOnlySequence<byte> sequence)
    {
        this.m_sequence = sequence;
    }

    public BytesReader(ReadOnlyMemory<byte> memory)
    {
        this.m_sequence = new ReadOnlySequence<byte>(memory);
    }

    public long BytesRead { readonly get => this.m_position; set => this.m_position = value; }

    public readonly long BytesRemaining => this.m_sequence.Length - this.m_position;

    public readonly ReadOnlySequence<byte> TotalSequence => this.m_sequence;

    public readonly ReadOnlySequence<byte> Sequence => this.TotalSequence.Slice(this.BytesRead);
    public void Advance(int count)
    {
        this.m_position += count;
    }

    public void Dispose() => this.m_memoryOwner?.Dispose();

    public ReadOnlyMemory<byte> GetMemory(int count)
    {
        var memories = this.m_sequence.Slice(this.m_position);
        var memory = memories.First;
        if (count <= memory.Length)
        {
            return memory.Slice(0, count); // 零拷贝切片
        }

        var cacheMemory = this.GetCacheMemory(count);
        memories.CopyTo(cacheMemory.Span);
        return cacheMemory;
    }

    public ReadOnlySpan<byte> GetSpan(int count)
    {
        return this.GetMemory(count).Span;
    }

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