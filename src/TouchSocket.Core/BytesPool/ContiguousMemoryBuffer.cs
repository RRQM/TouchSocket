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
using System.Runtime.InteropServices;

namespace TouchSocket.Core;

/// <summary>
/// 表示一个连续内存缓冲区，用于将<see cref="ReadOnlySequence{T}"/>转换为连续的内存块。
/// 实现了<see cref="IDisposable"/>接口以支持资源释放。
/// </summary>
/// <remarks>
/// 此结构体用于优化多段序列内存的访问性能。如果输入序列已经是单段内存，则直接使用原始内存；
/// 如果是多段内存，则从内存池中租用缓冲区并将所有段复制到连续内存中。
/// 使用后应调用<see cref="Dispose"/>方法释放可能租用的内存资源。
/// </remarks>
public readonly struct ContiguousMemoryBuffer : IDisposable
{
    private readonly ReadOnlyMemory<byte> m_memory;
    private readonly IMemoryOwner<byte> m_memoryOwner;

    /// <summary>
    /// 使用指定的只读字节序列初始化<see cref="ContiguousMemoryBuffer"/>的新实例。
    /// </summary>
    /// <param name="sequence">要转换为连续内存的只读字节序列。</param>
    /// <remarks>
    /// 如果序列是单段内存，则直接使用原始内存；如果是多段内存，则从<see cref="MemoryPool{T}.Shared"/>
    /// 租用缓冲区并复制所有段的数据。
    /// </remarks>
    public ContiguousMemoryBuffer(ReadOnlySequence<byte> sequence)
    {
        if (sequence.IsEmpty)
        {
            this.m_memory = default;
            this.m_memoryOwner = null;
            return;
        }

        if (sequence.IsSingleSegment)
        {
            this.m_memory = SequenceMarshal.TryGetReadOnlyMemory(sequence, out var memory) 
                ? memory 
                : sequence.First;
            this.m_memoryOwner = null;
            return;
        }

        var totalLength = (int)sequence.Length;
        this.m_memoryOwner = MemoryPool<byte>.Shared.Rent(totalLength);
        
        var destination = this.m_memoryOwner.Memory.Span;
        sequence.CopyTo(destination);

        this.m_memory = this.m_memoryOwner.Memory.Slice(0, totalLength);
    }

    /// <summary>
    /// 获取一个值，该值指示是否从内存池租用了新缓冲区。
    /// </summary>
    /// <value>如果从内存池租用了缓冲区，则为 <see langword="true"/>；否则为 <see langword="false"/>。</value>
    /// <remarks>
    /// 当输入序列是多段内存时，此属性返回 <see langword="true"/>；
    /// 当输入序列是单段内存时，此属性返回 <see langword="false"/>。
    /// </remarks>
    public bool IsRentedFromPool => this.m_memoryOwner != null;

    /// <summary>
    /// 获取连续的内存块。
    /// </summary>
    /// <value>表示连续内存数据的<see cref="ReadOnlyMemory{T}"/>。</value>
    /// <remarks>
    /// 返回的内存块保证是连续的，可以安全地用于需要连续内存的操作。
    /// 使用完毕后需要调用<see cref="Dispose"/>方法释放可能租用的资源。
    /// </remarks>
    public ReadOnlyMemory<byte> Memory => this.m_memory;

    /// <summary>
    /// 获取一个值，该值指示当前缓冲区是否为空。
    /// </summary>
    /// <value>如果缓冲区为空，则为 <see langword="true"/>；否则为 <see langword="false"/>。</value>
    public bool IsEmpty => this.m_memory.IsEmpty;

    /// <summary>
    /// 获取缓冲区的长度（字节数）。
    /// </summary>
    /// <value>缓冲区中的字节数。</value>
    public int Length => this.m_memory.Length;

    /// <inheritdoc/>
    public void Dispose()
    {
        this.m_memoryOwner?.Dispose();
    }
}