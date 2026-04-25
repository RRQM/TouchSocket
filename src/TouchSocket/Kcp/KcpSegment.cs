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

#nullable enable

using System.Buffers;
using System.Collections.Concurrent;

namespace TouchSocket.Sockets;

/// <summary>
/// KCP协议数据分片，使用对象池避免GC分配。
/// </summary>
/// <remarks>
/// 每个分片包含KCP协议头字段和从<see cref="ArrayPool{T}"/>租用的负载缓冲区。
/// 用完后需调用<see cref="Return"/>归还至对象池。
/// </remarks>
internal sealed class KcpSegment
{
    // 侵入式双向链表指针
    internal KcpSegment? Next;
    internal KcpSegment? Prev;

    // 协议头字段
    internal uint Conv;
    internal byte Cmd;
    internal byte Frg;
    internal ushort Wnd;
    internal uint Ts;
    internal uint Sn;
    internal uint Una;
    internal uint Len;

    // 重传状态
    internal uint ResendTs;
    internal uint Rto;
    internal uint FastAck;
    internal uint Xmit;

    // 负载缓冲区（从 ArrayPool<byte>.Shared 租用）
    internal byte[]? Data;
    internal int DataCapacity;

    private static readonly ConcurrentStack<KcpSegment> s_pool = new ConcurrentStack<KcpSegment>();

    internal KcpSegment() { }

    /// <summary>
    /// 从对象池租用一个分片，确保其数据缓冲区不小于 <paramref name="dataSize"/>。
    /// </summary>
    internal static KcpSegment Rent(int dataSize)
    {
        if (!s_pool.TryPop(out var seg))
            seg = new KcpSegment();

        if (dataSize > seg.DataCapacity)
        {
            if (seg.Data != null)
                ArrayPool<byte>.Shared.Return(seg.Data);
            seg.Data = ArrayPool<byte>.Shared.Rent(dataSize > 0 ? dataSize : 1);
            seg.DataCapacity = seg.Data.Length;
        }

        return seg;
    }

    /// <summary>
    /// 将分片归还至对象池，重置控制字段（保留负载缓冲区供下次复用）。
    /// </summary>
    internal static void Return(KcpSegment seg)
    {
        seg.Next = null;
        seg.Prev = null;
        seg.Len = 0;
        seg.Xmit = 0;
        seg.FastAck = 0;
        seg.Conv = 0;
        seg.Cmd = 0;
        seg.Frg = 0;
        seg.Wnd = 0;
        seg.Ts = 0;
        seg.Sn = 0;
        seg.Una = 0;
        seg.ResendTs = 0;
        seg.Rto = 0;
        s_pool.Push(seg);
    }

    /// <summary>
    /// 将分片归还至对象池，同时将数据缓冲区归还给<see cref="ArrayPool{T}"/>。
    /// 在销毁时调用，确保内存池缓冲区被释放。
    /// </summary>
    internal static void ReturnWithData(KcpSegment seg)
    {
        if (seg.Data != null)
        {
            ArrayPool<byte>.Shared.Return(seg.Data);
            seg.Data = null;
            seg.DataCapacity = 0;
        }
        Return(seg);
    }
}
