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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core;
public struct ContiguousMemoryBuffer : IDisposable
{
    private readonly bool m_isSingleSegment;
    private ReadOnlyMemory<byte> m_memory;
    private IMemoryOwner<byte> m_memoryOwner;

    public ContiguousMemoryBuffer(ReadOnlySequence<byte> sequence)
    {
        // 尝试直接获取单段内存
        if (SequenceMarshal.TryGetReadOnlyMemory(sequence, out m_memory))
        {
            m_isSingleSegment = true;
            m_memoryOwner = null;
            return;
        }

        // 多段内存处理
        this.m_isSingleSegment = false;
        var totalLength = (int)sequence.Length;
        this.m_memoryOwner = MemoryPool<byte>.Shared.Rent(totalLength);

        // 分段复制数据
        var destSpan = m_memoryOwner.Memory.Span;
        foreach (var segment in sequence)
        {
            segment.Span.CopyTo(destSpan);
            destSpan = destSpan.Slice(segment.Length);
        }

        this.m_memory = this.m_memoryOwner.Memory.Slice(0, totalLength);
    }

    /// <summary>
    /// 是否从内存池租用了新缓冲区
    /// </summary>
    public readonly bool IsRentedFromPool => this.m_memoryOwner != null;

    /// <summary>
    /// 获取连续的内存块（使用后需Dispose）
    /// </summary>
    public readonly ReadOnlyMemory<byte> Memory => this.m_memory;

    /// <summary>
    /// 释放租用的内存（仅当实际复制时有效）
    /// </summary>
    public void Dispose()
    {
        if (!this.m_isSingleSegment)
        {
            this.m_memoryOwner?.Dispose();
            this.m_memoryOwner = null;
            this.m_memory = default;
        }
    }
}