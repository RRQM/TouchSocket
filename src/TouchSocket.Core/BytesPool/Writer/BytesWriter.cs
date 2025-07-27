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

namespace TouchSocket.Core;

public struct BytesWriter : IBytesWriter
{
    private readonly Memory<byte> m_memory;

    private int m_position = 0;

    public BytesWriter(Memory<byte> memory)
    {
        this.m_memory = memory;
    }

    public readonly Memory<byte> TotalMemory => this.m_memory;
    public readonly ReadOnlySpan<byte> Span => this.m_memory.Span.Slice(0, this.m_position);

    public readonly long WrittenCount => this.m_position;

    public readonly bool SupportsRewind => true;

    public readonly short Version => 0;

    public int Position { readonly get => this.m_position; set => this.m_position = value; }

    public void Advance(int count)
    {
        this.m_position += count;
        if (this.m_position > this.m_memory.Length)
        {
            throw new InvalidOperationException("Advance exceeds the available buffer.");
        }
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        if (this.m_position + sizeHint > this.m_memory.Length)
        {
            throw new InvalidOperationException("Insufficient space to get memory.");
        }
        return this.m_memory.Slice(this.m_position, sizeHint);
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        return this.m_position + sizeHint > this.m_memory.Length
            ? throw new InvalidOperationException("Insufficient space to get span.")
            : this.m_memory.Span.Slice(this.m_position, sizeHint);
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
}