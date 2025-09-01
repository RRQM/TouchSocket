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

/// <summary>
/// 表示一个基于固定内存的字节写入器，提供高性能的字节缓冲区写入功能。
/// 实现了<see cref="IBytesWriter"/>接口。
/// </summary>
/// <remarks>
/// BytesWriter作为值类型实现，适用于高频使用且对性能要求较高的场景。
/// 基于固定大小的内存块进行操作，不支持自动扩容，当空间不足时会抛出异常。
/// </remarks>
public struct BytesWriter : IBytesWriter
{
    private readonly Memory<byte> m_memory;

    private int m_position = 0;

    /// <summary>
    /// 使用指定的内存块初始化<see cref="BytesWriter"/>的新实例。
    /// </summary>
    /// <param name="memory">要写入的内存块。</param>
    public BytesWriter(Memory<byte> memory)
    {
        this.m_memory = memory;
    }

    /// <summary>
    /// 获取完整的内存块。
    /// </summary>
    /// <value>表示完整内存块的<see cref="Memory{T}"/>。</value>
    public readonly Memory<byte> TotalMemory => this.m_memory;
    
    /// <summary>
    /// 获取已写入数据的只读跨度。
    /// </summary>
    /// <value>表示从索引0到当前位置的已写入数据的<see cref="ReadOnlySpan{T}"/>。</value>
    public readonly ReadOnlySpan<byte> Span => this.m_memory.Span.Slice(0, this.m_position);

    /// <inheritdoc/>
    public readonly long WrittenCount => this.m_position;

    /// <inheritdoc/>
    public readonly bool SupportsRewind => true;

    /// <inheritdoc/>
    public readonly short Version => 0;

    /// <summary>
    /// 获取或设置当前写入位置。
    /// </summary>
    /// <value>表示当前写入位置的索引值。</value>
    public int Position { readonly get => this.m_position; set => this.m_position = value; }

    /// <inheritdoc/>
    public void Advance(int count)
    {
        this.m_position += count;
        if (this.m_position > this.m_memory.Length)
        {
            throw new InvalidOperationException("Advance exceeds the available buffer.");
        }
    }

    /// <inheritdoc/>
    public readonly Memory<byte> GetMemory(int sizeHint = 0)
    {
        if (this.m_position + sizeHint > this.m_memory.Length)
        {
            throw new InvalidOperationException("Insufficient space to get memory.");
        }
        return this.m_memory.Slice(this.m_position, sizeHint);
    }

    /// <inheritdoc/>
    public readonly Span<byte> GetSpan(int sizeHint = 0)
    {
        return this.m_position + sizeHint > this.m_memory.Length
            ? throw new InvalidOperationException("Insufficient space to get span.")
            : this.m_memory.Span.Slice(this.m_position, sizeHint);
    }

    /// <inheritdoc/>
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