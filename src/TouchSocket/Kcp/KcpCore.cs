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
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace TouchSocket.Sockets;

/// <summary>
/// KCP数据输出回调委托。
/// </summary>
/// <param name="data">待发送的已编码数据，仅在回调执行期间有效，不可持久化引用。</param>
public delegate void KcpOutputHandler(ReadOnlySpan<byte> data);

/// <summary>
/// KCP可靠传输算法核心实现（C# 0GC移植自 skywind3000/kcp）。
/// </summary>
/// <remarks>
/// KCP是一个快速可靠协议，以约10%~20%额外带宽为代价，换取平均延迟降低30%~40%、
/// 最大延迟降低三倍的传输效果。
/// <para>
/// 本实现使用侵入式双向循环链表作为内部队列，使用<see cref="ArrayPool{T}"/>
/// 管理缓冲区，不产生GC压力。
/// </para>
/// <para>
/// 非线程安全，调用方需自行保证单线程调用。
/// </para>
/// </remarks>
public sealed class KcpCore : IDisposable
{
    #region 协议常量

    private const uint RtoNdl = 30;
    private const uint RtoMin = 100;
    private const uint RtoDef = 200;
    private const uint RtoMax = 60000;
    private const byte CmdPush = 81;
    private const byte CmdAck = 82;
    private const byte CmdWask = 83;
    private const byte CmdWins = 84;
    private const uint AskSend = 1;
    private const uint AskTell = 2;
    private const uint WndSndDef = 32;
    private const uint WndRcvDef = 128;
    private const uint MtuDef = 1400;
    private const int Overhead = 24;
    private const uint DeadLinkDef = 20;
    private const uint ThreshInit = 2;
    private const uint ThreshMin = 2;
    private const uint ProbeInit = 7000;
    private const uint ProbeLimit = 120000;
    private const uint FastAckLimit = 5;
    private const uint IntervalDef = 100;

    #endregion

    #region 状态字段

    private readonly uint m_conv;
    private uint m_mtu;
    private uint m_mss;
    private uint m_state;

    private uint m_sndUna;
    private uint m_sndNxt;
    private uint m_rcvNxt;

    private int m_rxRttval;
    private int m_rxSrtt;
    private int m_rxRto;
    private int m_rxMinrto;

    private uint m_sndWnd;
    private uint m_rcvWnd;
    private uint m_rmtWnd;
    private uint m_cwnd;
    private uint m_probe;
    private uint m_incr;

    private uint m_current;
    private uint m_interval;
    private uint m_tsFlush;
    private uint m_xmit;

    private uint m_nrcvBuf;
    private uint m_nsndBuf;
    private uint m_nrcvQue;
    private uint m_nsndQue;

    private uint m_nodelay;
    private uint m_updated;

    private uint m_tsProbe;
    private uint m_probeWait;
    private uint m_ssthresh;

    private int m_fastresend;
    private int m_fastlimit;
    private int m_nocwnd;
    private int m_stream;
    private uint m_deadLink;

    // ACK列表：按 [sn, ts, sn, ts, ...] 交替存储
    private uint[] m_ackList;
    private int m_ackCount;

    // 四个队列的哨兵节点（循环双向链表）
    private readonly KcpSegment m_sndQueueHead;
    private readonly KcpSegment m_rcvQueueHead;
    private readonly KcpSegment m_sndBufHead;
    private readonly KcpSegment m_rcvBufHead;

    // 输出缓冲区（预分配，跨Flush调用复用）
    private byte[] m_outputBuf;

    private bool m_disposed;

    #endregion

    /// <summary>
    /// KCP数据输出回调，将编码后的数据发往网络层。
    /// </summary>
    public KcpOutputHandler? Output;

    /// <summary>
    /// 初始化KCP核心，指定会话标识符。
    /// </summary>
    /// <param name="conv">会话标识符，通信双方必须一致。</param>
    public KcpCore(uint conv)
    {
        this.m_conv = conv;

        // 初始化四个队列哨兵节点（自引用形成空循环链表）
        this.m_sndQueueHead = CreateSentinel();
        this.m_rcvQueueHead = CreateSentinel();
        this.m_sndBufHead = CreateSentinel();
        this.m_rcvBufHead = CreateSentinel();

        this.m_mtu = MtuDef;
        this.m_mss = this.m_mtu - Overhead;
        this.m_sndWnd = WndSndDef;
        this.m_rcvWnd = WndRcvDef;
        this.m_rmtWnd = WndRcvDef;
        this.m_cwnd = 1;               // 初始拥塞窗口=1，允许首包立即发送
        this.m_rxRto = (int)RtoDef;
        this.m_rxMinrto = (int)RtoMin;
        this.m_interval = IntervalDef;
        this.m_tsFlush = IntervalDef;
        this.m_ssthresh = ThreshInit;
        this.m_fastlimit = (int)FastAckLimit;
        this.m_deadLink = DeadLinkDef;

        this.m_ackList = ArrayPool<uint>.Shared.Rent(16);   // 初始8对(sn,ts)容量
        this.m_outputBuf = ArrayPool<byte>.Shared.Rent(((int)this.m_mtu + Overhead) * 3);
    }

    #region 公共属性

    /// <summary>
    /// 获取会话标识符。
    /// </summary>
    public uint Conv => this.m_conv;

    /// <summary>
    /// 获取连接状态，<see langword="0"/>为正常，<see cref="uint.MaxValue"/>表示死链路。
    /// </summary>
    public uint State => this.m_state;

    /// <summary>
    /// 获取等待发送（发送队列+发送缓冲区）的数据包数量。
    /// </summary>
    public int WaitSnd => (int)(this.m_nsndBuf + this.m_nsndQue);

    #endregion

    #region 配置方法

    /// <summary>
    /// 设置最大传输单元。
    /// </summary>
    /// <param name="mtu">MTU大小，不得小于50或协议头大小。</param>
    /// <returns>设置成功返回<see langword="true"/>，否则返回<see langword="false"/>。</returns>
    public bool SetMtu(int mtu)
    {
        if (mtu < 50 || mtu < Overhead)
            return false;

        var newBuf = ArrayPool<byte>.Shared.Rent((mtu + Overhead) * 3);
        ArrayPool<byte>.Shared.Return(this.m_outputBuf);
        this.m_outputBuf = newBuf;
        this.m_mtu = (uint)mtu;
        this.m_mss = this.m_mtu - Overhead;
        return true;
    }

    /// <summary>
    /// 配置KCP工作模式。
    /// </summary>
    /// <param name="nodelay">是否启用nodelay模式（降低延迟）。</param>
    /// <param name="interval">内部更新时钟间隔（毫秒），范围[10, 5000]，默认100。</param>
    /// <param name="resend">快速重传触发阈值，0表示禁用，推荐设为2。</param>
    /// <param name="nc">是否关闭拥塞控制（禁用后吞吐量更大但可能造成拥塞）。</param>
    public void SetNoDelay(bool nodelay, int interval = (int)IntervalDef, int resend = 0, bool nc = false)
    {
        this.m_nodelay = nodelay ? 1u : 0u;
        this.m_rxMinrto = nodelay ? (int)RtoNdl : (int)RtoMin;

        if (interval >= 0)
        {
            if (interval > 5000) interval = 5000;
            else if (interval < 10) interval = 10;
            this.m_interval = (uint)interval;
        }

        if (resend >= 0)
            this.m_fastresend = resend;

        this.m_nocwnd = nc ? 1 : 0;
    }

    /// <summary>
    /// 设置发送窗口和接收窗口大小。
    /// </summary>
    /// <param name="sndWnd">发送窗口大小，默认32。</param>
    /// <param name="rcvWnd">接收窗口大小，不得小于<see cref="WndRcvDef"/>，默认128。</param>
    public void SetWindowSize(int sndWnd = (int)WndSndDef, int rcvWnd = (int)WndRcvDef)
    {
        if (sndWnd > 0)
            this.m_sndWnd = (uint)sndWnd;
        if (rcvWnd > 0)
            this.m_rcvWnd = (uint)Math.Max(rcvWnd, (int)WndRcvDef);
    }

    /// <summary>
    /// 设置流模式（启用后会合并连续小包）。
    /// </summary>
    public void SetStream(bool stream) => this.m_stream = stream ? 1 : 0;

    /// <summary>
    /// 设置死链路检测阈值（重传次数超过此值时判定为死链路）。
    /// </summary>
    public void SetDeadLink(uint deadLink) => this.m_deadLink = deadLink;

    #endregion

    #region 主要操作

    /// <summary>
    /// 发送数据。将数据分片后放入发送队列，由<see cref="Update"/>驱动实际发送。
    /// </summary>
    /// <param name="data">要发送的数据。</param>
    /// <returns>成功返回发送的字节数，失败返回负值（-1:参数错误，-2:超出窗口限制）。</returns>
    public int Send(ReadOnlySpan<byte> data)
    {
        var len = data.Length;
        if (len < 0)
            return -1;

        var sent = 0;

        // 流模式：将新数据追加到发送队列末尾的不满分片
        if (this.m_stream != 0 && !this.IsSndQueueEmpty())
        {
            var old = this.m_sndQueueHead.Prev!;
            if (old.Len < this.m_mss)
            {
                var capacity = (int)(this.m_mss - old.Len);
                var extend = len < capacity ? len : capacity;
                var merged = KcpSegment.Rent((int)old.Len + extend);
                // 拼接旧数据+新数据
                old.Data.AsSpan(0, (int)old.Len).CopyTo(merged.Data!);
                data.Slice(0, extend).CopyTo(merged.Data!.AsSpan((int)old.Len));
                merged.Len = old.Len + (uint)extend;
                merged.Frg = 0;
                // 替换旧节点（nsndQue 净变化为0）
                QueueInsertBefore(old, merged);
                QueueRemove(old);
                KcpSegment.Return(old);
                data = data.Slice(extend);
                len -= extend;
                sent = extend;
            }
        }

        if (len <= 0)
            return sent;

        var count = len <= (int)this.m_mss ? 1 : (len + (int)this.m_mss - 1) / (int)this.m_mss;

        if (count >= (int)WndRcvDef)
            return this.m_stream != 0 && sent > 0 ? sent : -2;

        if (count == 0)
            count = 1;

        for (var i = 0; i < count; i++)
        {
            var size = len > (int)this.m_mss ? (int)this.m_mss : len;
            var seg = KcpSegment.Rent(size);
            if (size > 0)
                data.Slice(0, size).CopyTo(seg.Data!);
            seg.Len = (uint)size;
            seg.Frg = this.m_stream == 0 ? (byte)(count - i - 1) : (byte)0;
            QueueAddTail(this.m_sndQueueHead, seg);
            this.m_nsndQue++;
            data = data.Slice(size);
            len -= size;
            sent += size;
        }

        return sent;
    }

    /// <summary>
    /// 输入从网络层接收到的原始数据包，解析后更新接收队列。
    /// </summary>
    /// <param name="data">接收到的原始数据。</param>
    /// <returns>成功返回0，失败返回负值（-1:数据太短，-2:负载超出声明长度，-3:未知命令）。</returns>
    public int Input(ReadOnlySpan<byte> data)
    {
        var prevUna = this.m_sndUna;
        uint maxAck = 0, latestTs = 0;
        var gotAck = false;

        if (data.Length < Overhead)
            return -1;

        while (true)
        {
            if (data.Length < Overhead)
                break;

            var conv = BinaryPrimitives.ReadUInt32LittleEndian(data); data = data.Slice(4);
            if (conv != this.m_conv)
                return -1;

            var cmd = data[0]; data = data.Slice(1);
            var frg = data[0]; data = data.Slice(1);
            var wnd = BinaryPrimitives.ReadUInt16LittleEndian(data); data = data.Slice(2);
            var ts = BinaryPrimitives.ReadUInt32LittleEndian(data); data = data.Slice(4);
            var sn = BinaryPrimitives.ReadUInt32LittleEndian(data); data = data.Slice(4);
            var una = BinaryPrimitives.ReadUInt32LittleEndian(data); data = data.Slice(4);
            var len = BinaryPrimitives.ReadUInt32LittleEndian(data); data = data.Slice(4);

            if ((long)data.Length < (long)len)
                return -2;

            if (cmd != CmdPush && cmd != CmdAck && cmd != CmdWask && cmd != CmdWins)
                return -3;

            this.m_rmtWnd = wnd;
            this.ParseUna(una);
            this.ShrinkBuf();

            if (cmd == CmdAck)
            {
                if (TimeDiff(this.m_current, ts) >= 0)
                    this.UpdateAck(TimeDiff(this.m_current, ts));
                this.ParseAck(sn);
                this.ShrinkBuf();
                if (!gotAck)
                {
                    gotAck = true;
                    maxAck = sn;
                    latestTs = ts;
                }
                else if (TimeDiff(sn, maxAck) > 0)
                {
                    // IKCP_FASTACK_CONSERVE：取最新ts对应的sn
                    if (TimeDiff(ts, latestTs) > 0)
                    {
                        maxAck = sn;
                        latestTs = ts;
                    }
                }
            }
            else if (cmd == CmdPush)
            {
                if (TimeDiff(sn, this.m_rcvNxt + this.m_rcvWnd) < 0)
                {
                    this.AckPush(sn, ts);
                    if (TimeDiff(sn, this.m_rcvNxt) >= 0)
                    {
                        var seg = KcpSegment.Rent((int)len);
                        seg.Conv = conv;
                        seg.Cmd = cmd;
                        seg.Frg = frg;
                        seg.Wnd = wnd;
                        seg.Ts = ts;
                        seg.Sn = sn;
                        seg.Una = una;
                        seg.Len = len;
                        if (len > 0)
                            data.Slice(0, (int)len).CopyTo(seg.Data!);
                        this.ParseData(seg);
                    }
                }
            }
            else if (cmd == CmdWask)
            {
                this.m_probe |= AskTell;
            }
            // CmdWins: 仅更新 rmtWnd（已在上方处理），无需额外操作

            data = data.Slice((int)len);
        }

        if (gotAck)
            this.ParseFastAck(maxAck, latestTs);

        // ACK触发后更新拥塞窗口
        if (TimeDiff(this.m_sndUna, prevUna) > 0 && this.m_cwnd < this.m_rmtWnd)
        {
            var mss = this.m_mss;
            if (this.m_cwnd < this.m_ssthresh)
            {
                this.m_cwnd++;
                this.m_incr += mss;
            }
            else
            {
                if (this.m_incr < mss) this.m_incr = mss;
                this.m_incr += (mss * mss) / this.m_incr + (mss / 16);
                if ((this.m_cwnd + 1) * mss <= this.m_incr)
                    this.m_cwnd = (this.m_incr + mss - 1) / (mss > 0 ? mss : 1);
            }
            if (this.m_cwnd > this.m_rmtWnd)
            {
                this.m_cwnd = this.m_rmtWnd;
                this.m_incr = this.m_rmtWnd * mss;
            }
        }

        return 0;
    }

    /// <summary>
    /// 从接收队列读取一条完整消息到 <paramref name="buffer"/>。
    /// </summary>
    /// <param name="buffer">目标缓冲区。</param>
    /// <returns>实际读取字节数；-1无数据，-2无完整消息，-3缓冲区太小。</returns>
    public int Recv(Span<byte> buffer)
    {
        if (this.IsRcvQueueEmpty())
            return -1;

        var peekSize = this.PeekSize();
        if (peekSize < 0)
            return -2;
        if (peekSize > buffer.Length)
            return -3;

        var recover = this.m_nrcvQue >= this.m_rcvWnd;

        var len = 0;
        var p = this.m_rcvQueueHead.Next!;
        while (p != this.m_rcvQueueHead)
        {
            var next = p.Next!;
            p.Data.AsSpan(0, (int)p.Len).CopyTo(buffer.Slice(len));
            len += (int)p.Len;
            int frag = p.Frg;
            QueueRemove(p);
            KcpSegment.Return(p);
            this.m_nrcvQue--;
            if (frag == 0)
                break;
            p = next;
        }

        this.MoveRcvBufToQueue();

        // 接收窗口有空闲时通知对端
        if (this.m_nrcvQue < this.m_rcvWnd && recover)
            this.m_probe |= AskTell;

        return len;
    }

    /// <summary>
    /// 查看下一条可读消息的字节数，不消耗数据。
    /// </summary>
    /// <returns>消息字节数；-1表示无完整消息。</returns>
    public int PeekSize()
    {
        if (this.IsRcvQueueEmpty())
            return -1;

        var head = this.m_rcvQueueHead.Next!;
        if (head.Frg == 0)
            return (int)head.Len;

        if (this.m_nrcvQue < head.Frg + 1)
            return -1;

        var length = 0;
        var p = this.m_rcvQueueHead.Next!;
        while (p != this.m_rcvQueueHead)
        {
            length += (int)p.Len;
            if (p.Frg == 0)
                break;
            p = p.Next!;
        }
        return length;
    }

    /// <summary>
    /// 驱动KCP状态机，需以固定间隔周期性调用（每10~100ms一次）。
    /// </summary>
    /// <param name="current">当前时间戳（毫秒）。</param>
    public void Update(uint current)
    {
        this.m_current = current;

        if (this.m_updated == 0)
        {
            this.m_updated = 1;
            this.m_tsFlush = this.m_current;
        }

        var slap = TimeDiff(this.m_current, this.m_tsFlush);
        if (slap >= 10000 || slap < -10000)
        {
            this.m_tsFlush = this.m_current;
            slap = 0;
        }

        if (slap >= 0)
        {
            this.m_tsFlush += this.m_interval;
            if (TimeDiff(this.m_current, this.m_tsFlush) >= 0)
                this.m_tsFlush = this.m_current + this.m_interval;
            this.Flush();
        }
    }

    /// <summary>
    /// 计算下次需调用<see cref="Update"/>的最晚时间戳，用于优化定时器调度。
    /// </summary>
    /// <param name="current">当前时间戳（毫秒）。</param>
    /// <returns>建议的下次Update时间戳（毫秒）。</returns>
    public uint Check(uint current)
    {
        if (this.m_updated == 0)
            return current;

        var tsFlush = this.m_tsFlush;
        if (TimeDiff(current, tsFlush) >= 10000 || TimeDiff(current, tsFlush) < -10000)
            tsFlush = current;

        if (TimeDiff(current, tsFlush) >= 0)
            return current;

        var tmFlush = TimeDiff(tsFlush, current);
        var tmPacket = 0x7fffffff;

        var p = this.m_sndBufHead.Next!;
        while (p != this.m_sndBufHead)
        {
            var diff = TimeDiff(p.ResendTs, current);
            if (diff <= 0)
                return current;
            if (diff < tmPacket)
                tmPacket = diff;
            p = p.Next!;
        }

        var minimal = tmPacket < tmFlush ? tmPacket : tmFlush;
        if ((uint)minimal >= this.m_interval)
            minimal = (int)this.m_interval;

        return current + (uint)minimal;
    }

    #endregion

    #region 内部实现 - Flush

    private void Flush()
    {
        if (this.m_updated == 0)
            return;

        var current = this.m_current;
        var ptr = 0;  // m_outputBuf 当前写入偏移

        var wndUnused = (ushort)this.WndUnused();
        var rcvNxt = this.m_rcvNxt;

        // 1. 发送待确认的ACK
        var ackCount = this.m_ackCount;
        for (var i = 0; i < ackCount; i++)
        {
            if (ptr + Overhead > (int)this.m_mtu)
            {
                this.Output?.Invoke(this.m_outputBuf.AsSpan(0, ptr));
                ptr = 0;
            }
            ptr = EncodeHeader(this.m_outputBuf, ptr, this.m_conv, CmdAck, 0, wndUnused,
                this.m_ackList[i * 2 + 1],  // ts
                this.m_ackList[i * 2],       // sn
                rcvNxt, 0);
        }
        this.m_ackCount = 0;

        // 2. 处理窗口探测
        if (this.m_rmtWnd == 0)
        {
            if (this.m_probeWait == 0)
            {
                this.m_probeWait = ProbeInit;
                this.m_tsProbe = current + this.m_probeWait;
            }
            else if (TimeDiff(current, this.m_tsProbe) >= 0)
            {
                if (this.m_probeWait < ProbeInit) this.m_probeWait = ProbeInit;
                this.m_probeWait += this.m_probeWait / 2;
                if (this.m_probeWait > ProbeLimit) this.m_probeWait = ProbeLimit;
                this.m_tsProbe = current + this.m_probeWait;
                this.m_probe |= AskSend;
            }
        }
        else
        {
            this.m_tsProbe = 0;
            this.m_probeWait = 0;
        }

        if ((this.m_probe & AskSend) != 0)
        {
            if (ptr + Overhead > (int)this.m_mtu) { this.Output?.Invoke(this.m_outputBuf.AsSpan(0, ptr)); ptr = 0; }
            ptr = EncodeHeader(this.m_outputBuf, ptr, this.m_conv, CmdWask, 0, wndUnused, 0, 0, rcvNxt, 0);
        }
        if ((this.m_probe & AskTell) != 0)
        {
            if (ptr + Overhead > (int)this.m_mtu) { this.Output?.Invoke(this.m_outputBuf.AsSpan(0, ptr)); ptr = 0; }
            ptr = EncodeHeader(this.m_outputBuf, ptr, this.m_conv, CmdWins, 0, wndUnused, 0, 0, rcvNxt, 0);
        }
        this.m_probe = 0;

        // 3. 计算有效发送窗口，将分片从发送队列移入发送缓冲区
        var cwnd = Math.Min(this.m_sndWnd, this.m_rmtWnd);
        if (this.m_nocwnd == 0) cwnd = Math.Min(this.m_cwnd, cwnd);

        while (TimeDiff(this.m_sndNxt, this.m_sndUna + cwnd) < 0)
        {
            if (this.IsSndQueueEmpty()) break;
            var newseg = this.m_sndQueueHead.Next!;
            QueueRemove(newseg);
            this.m_nsndQue--;
            QueueAddTail(this.m_sndBufHead, newseg);
            this.m_nsndBuf++;

            newseg.Conv = this.m_conv;
            newseg.Cmd = CmdPush;
            newseg.Wnd = wndUnused;
            newseg.Ts = current;
            newseg.Sn = this.m_sndNxt++;
            newseg.Una = this.m_rcvNxt;
            newseg.ResendTs = current;
            newseg.Rto = (uint)this.m_rxRto;
            newseg.FastAck = 0;
            newseg.Xmit = 0;
        }

        // 4. 发送/重传发送缓冲区中的分片
        var resent = this.m_fastresend > 0 ? (uint)this.m_fastresend : 0xffffffff;
        var rtoMin = this.m_nodelay == 0 ? (uint)(this.m_rxRto >> 3) : 0;
        var change = 0;
        var lost = 0;

        var seg = this.m_sndBufHead.Next!;
        while (seg != this.m_sndBufHead)
        {
            var needsend = false;

            if (seg.Xmit == 0)
            {
                // 首次发送
                needsend = true;
                seg.Xmit++;
                seg.Rto = (uint)this.m_rxRto;
                seg.ResendTs = current + seg.Rto + rtoMin;
            }
            else if (TimeDiff(current, seg.ResendTs) >= 0)
            {
                // 超时重传
                needsend = true;
                seg.Xmit++;
                this.m_xmit++;
                if (this.m_nodelay == 0)
                    seg.Rto += Math.Max(seg.Rto, (uint)this.m_rxRto);
                else
                {
                    var step = this.m_nodelay < 2 ? (int)seg.Rto : this.m_rxRto;
                    seg.Rto += (uint)(step / 2);
                }
                seg.ResendTs = current + seg.Rto;
                lost = 1;
            }
            else if (seg.FastAck >= resent)
            {
                // 快速重传
                if ((int)seg.Xmit <= this.m_fastlimit || this.m_fastlimit <= 0)
                {
                    needsend = true;
                    seg.Xmit++;
                    seg.FastAck = 0;
                    seg.ResendTs = current + seg.Rto;
                    change++;
                }
            }

            if (needsend)
            {
                seg.Ts = current;
                seg.Wnd = wndUnused;
                seg.Una = this.m_rcvNxt;

                var need = Overhead + (int)seg.Len;
                if (ptr + need > (int)this.m_mtu)
                {
                    this.Output?.Invoke(this.m_outputBuf.AsSpan(0, ptr));
                    ptr = 0;
                }

                ptr = EncodeHeader(this.m_outputBuf, ptr, seg.Conv, seg.Cmd, seg.Frg,
                    seg.Wnd, seg.Ts, seg.Sn, seg.Una, seg.Len);

                if (seg.Len > 0)
                    seg.Data.AsSpan(0, (int)seg.Len).CopyTo(this.m_outputBuf.AsSpan(ptr));
                ptr += (int)seg.Len;

                if (seg.Xmit >= this.m_deadLink)
                    this.m_state = uint.MaxValue;
            }

            seg = seg.Next!;
        }

        // 发送缓冲区剩余数据
        if (ptr > 0)
            this.Output?.Invoke(this.m_outputBuf.AsSpan(0, ptr));

        // 5. 更新拥塞控制状态
        if (change > 0)
        {
            var inflight = this.m_sndNxt - this.m_sndUna;
            this.m_ssthresh = inflight / 2;
            if (this.m_ssthresh < ThreshMin) this.m_ssthresh = ThreshMin;
            this.m_cwnd = this.m_ssthresh + resent;
            this.m_incr = this.m_cwnd * this.m_mss;
        }

        if (lost != 0)
        {
            this.m_ssthresh = cwnd / 2;
            if (this.m_ssthresh < ThreshMin) this.m_ssthresh = ThreshMin;
            this.m_cwnd = 1;
            this.m_incr = this.m_mss;
        }

        if (this.m_cwnd < 1)
        {
            this.m_cwnd = 1;
            this.m_incr = this.m_mss;
        }
    }

    #endregion

    #region 内部实现 - 协议逻辑

    private void UpdateAck(int rtt)
    {
        if (this.m_rxSrtt == 0)
        {
            this.m_rxSrtt = rtt;
            this.m_rxRttval = rtt / 2;
        }
        else
        {
            var delta = rtt - this.m_rxSrtt;
            if (delta < 0) delta = -delta;
            this.m_rxRttval = (3 * this.m_rxRttval + delta) / 4;
            this.m_rxSrtt = (7 * this.m_rxSrtt + rtt) / 8;
            if (this.m_rxSrtt < 1) this.m_rxSrtt = 1;
        }
        var rto = this.m_rxSrtt + Math.Max((int)this.m_interval, 4 * this.m_rxRttval);
        this.m_rxRto = Clamp(this.m_rxMinrto, rto, (int)RtoMax);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ShrinkBuf()
    {
        this.m_sndUna = this.m_sndBufHead.Next != this.m_sndBufHead
            ? this.m_sndBufHead.Next!.Sn
            : this.m_sndNxt;
    }

    private void ParseAck(uint sn)
    {
        if (TimeDiff(sn, this.m_sndUna) < 0 || TimeDiff(sn, this.m_sndNxt) >= 0)
            return;

        var p = this.m_sndBufHead.Next!;
        while (p != this.m_sndBufHead)
        {
            var next = p.Next!;
            if (sn == p.Sn)
            {
                QueueRemove(p);
                KcpSegment.Return(p);
                this.m_nsndBuf--;
                break;
            }
            if (TimeDiff(sn, p.Sn) < 0)
                break;
            p = next;
        }
    }

    private void ParseUna(uint una)
    {
        var p = this.m_sndBufHead.Next!;
        while (p != this.m_sndBufHead)
        {
            var next = p.Next!;
            if (TimeDiff(una, p.Sn) > 0)
            {
                QueueRemove(p);
                KcpSegment.Return(p);
                this.m_nsndBuf--;
            }
            else break;
            p = next;
        }
    }

    private void ParseFastAck(uint sn, uint ts)
    {
        if (TimeDiff(sn, this.m_sndUna) < 0 || TimeDiff(sn, this.m_sndNxt) >= 0)
            return;

        var p = this.m_sndBufHead.Next!;
        while (p != this.m_sndBufHead)
        {
            if (TimeDiff(sn, p.Sn) < 0)
                break;
            if (sn != p.Sn)
            {
                // IKCP_FASTACK_CONSERVE：只有 ts >= seg.ts 时才计数
                if (TimeDiff(ts, p.Ts) >= 0)
                    p.FastAck++;
            }
            p = p.Next!;
        }
    }

    private void AckPush(uint sn, uint ts)
    {
        if (((this.m_ackCount + 1) * 2) > this.m_ackList.Length)
        {
            var newBlock = Math.Max(16, this.m_ackList.Length * 2);
            var newList = ArrayPool<uint>.Shared.Rent(newBlock);
            if (this.m_ackCount > 0)
                Array.Copy(this.m_ackList, newList, this.m_ackCount * 2);
            ArrayPool<uint>.Shared.Return(this.m_ackList);
            this.m_ackList = newList;
        }
        this.m_ackList[this.m_ackCount * 2] = sn;
        this.m_ackList[this.m_ackCount * 2 + 1] = ts;
        this.m_ackCount++;
    }

    private void ParseData(KcpSegment newSeg)
    {
        var sn = newSeg.Sn;

        // 丢弃超出接收窗口或已接收过的分片
        if (TimeDiff(sn, this.m_rcvNxt + this.m_rcvWnd) >= 0 || TimeDiff(sn, this.m_rcvNxt) < 0)
        {
            KcpSegment.Return(newSeg);
            return;
        }

        // 从尾部向前找插入位置（rcv_buf 按 sn 升序排列）
        var repeat = false;
        var p = this.m_rcvBufHead.Prev!;
        while (p != this.m_rcvBufHead)
        {
            if (p.Sn == sn) { repeat = true; break; }
            if (TimeDiff(sn, p.Sn) > 0) break;
            p = p.Prev!;
        }

        if (!repeat)
        {
            // 在 p 之后插入（p 是最后一个 sn < newSeg.sn 的节点，或哨兵）
            QueueInsertAfter(p, newSeg);
            this.m_nrcvBuf++;
        }
        else
        {
            KcpSegment.Return(newSeg);
        }

        this.MoveRcvBufToQueue();
    }

    private void MoveRcvBufToQueue()
    {
        while (this.m_rcvBufHead.Next != this.m_rcvBufHead)
        {
            var seg = this.m_rcvBufHead.Next!;
            if (seg.Sn == this.m_rcvNxt && this.m_nrcvQue < this.m_rcvWnd)
            {
                QueueRemove(seg);
                this.m_nrcvBuf--;
                QueueAddTail(this.m_rcvQueueHead, seg);
                this.m_nrcvQue++;
                this.m_rcvNxt++;
            }
            else break;
        }
    }

    #endregion

    #region 内部工具方法

    /// <summary>
    /// 时间差计算（支持uint回绕）。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int TimeDiff(uint later, uint earlier) => (int)(later - earlier);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint WndUnused() => this.m_nrcvQue < this.m_rcvWnd ? this.m_rcvWnd - this.m_nrcvQue : 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsSndQueueEmpty() => this.m_sndQueueHead.Next == this.m_sndQueueHead;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsRcvQueueEmpty() => this.m_rcvQueueHead.Next == this.m_rcvQueueHead;

    /// <summary>
    /// 创建循环链表哨兵节点（自引用空链表）。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static KcpSegment CreateSentinel()
    {
        var s = new KcpSegment();
        s.Next = s;
        s.Prev = s;
        return s;
    }

    /// <summary>
    /// 将 <paramref name="seg"/> 添加到以 <paramref name="sentinel"/> 为头节点的链表尾部。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void QueueAddTail(KcpSegment sentinel, KcpSegment seg)
    {
        seg.Prev = sentinel.Prev;
        seg.Next = sentinel;
        sentinel.Prev!.Next = seg;
        sentinel.Prev = seg;
    }

    /// <summary>
    /// 将 <paramref name="newSeg"/> 插入到 <paramref name="anchor"/> 节点的前方（即 anchor.Prev 位置）。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void QueueInsertBefore(KcpSegment anchor, KcpSegment newSeg)
    {
        newSeg.Next = anchor;
        newSeg.Prev = anchor.Prev;
        anchor.Prev!.Next = newSeg;
        anchor.Prev = newSeg;
    }

    /// <summary>
    /// 将 <paramref name="newSeg"/> 插入到 <paramref name="anchor"/> 节点的后方（即 anchor.Next 位置）。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void QueueInsertAfter(KcpSegment anchor, KcpSegment newSeg)
    {
        newSeg.Prev = anchor;
        newSeg.Next = anchor.Next;
        anchor.Next!.Prev = newSeg;
        anchor.Next = newSeg;
    }

    /// <summary>
    /// 从链表中移除 <paramref name="seg"/> 并清空其前后指针。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void QueueRemove(KcpSegment seg)
    {
        seg.Prev!.Next = seg.Next;
        seg.Next!.Prev = seg.Prev;
        seg.Next = null;
        seg.Prev = null;
    }

    /// <summary>
    /// 编码24字节KCP段头部到缓冲区，返回更新后的写入偏移。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int EncodeHeader(byte[] buf, int offset,
        uint conv, byte cmd, byte frg, ushort wnd,
        uint ts, uint sn, uint una, uint len)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(offset), conv); offset += 4;
        buf[offset++] = cmd;
        buf[offset++] = frg;
        BinaryPrimitives.WriteUInt16LittleEndian(buf.AsSpan(offset), wnd); offset += 2;
        BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(offset), ts); offset += 4;
        BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(offset), sn); offset += 4;
        BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(offset), una); offset += 4;
        BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(offset), len); offset += 4;
        return offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Clamp(int min, int val, int max) =>
        val < min ? min : (val > max ? max : val);

    #endregion

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose()
    {
        if (this.m_disposed) return;
        this.m_disposed = true;

        FreeQueue(this.m_sndQueueHead, ref this.m_nsndQue);
        FreeQueue(this.m_rcvQueueHead, ref this.m_nrcvQue);
        FreeQueue(this.m_sndBufHead, ref this.m_nsndBuf);
        FreeQueue(this.m_rcvBufHead, ref this.m_nrcvBuf);

        if (this.m_ackList.Length > 0)
        {
            ArrayPool<uint>.Shared.Return(this.m_ackList);
            this.m_ackList = Array.Empty<uint>();
        }

        ArrayPool<byte>.Shared.Return(this.m_outputBuf);
    }

    private static void FreeQueue(KcpSegment sentinel, ref uint count)
    {
        var p = sentinel.Next!;
        while (p != sentinel)
        {
            var next = p.Next!;
            KcpSegment.ReturnWithData(p);
            count--;
            p = next;
        }
        sentinel.Next = sentinel;
        sentinel.Prev = sentinel;
    }

    #endregion
}
