//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

#if !NET6_0_OR_GREATER
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Dmtp;

internal sealed partial class InternalChannel : SafetyDisposableObject, IDmtpChannel
{
    private readonly Queue<ChannelPackage> m_dataQueue;
    private readonly SemaphoreSlim m_dataAvailable;
    private bool m_closed;

    public InternalChannel(DmtpActor client, string targetId, Metadata metadata)
    {
        this.m_actor = client;
        this.m_lastOperationTime = DateTimeOffset.UtcNow;
        this.TargetId = targetId;
        this.Status = ChannelStatus.Default;
        this.m_dataQueue = new Queue<ChannelPackage>();
        this.m_dataAvailable = new SemaphoreSlim(0);
        this.Metadata = metadata;
    }

    internal void ReceivedData(ChannelPackage channelPackage)
    {
        lock (this.m_dataQueue)
        {
            if (this.m_closed)
            {
                channelPackage.SafeDispose();
                return;
            }
            this.m_lastOperationTime = DateTimeOffset.UtcNow;
            this.m_dataQueue.Enqueue(channelPackage);
            this.m_dataAvailable.Release();
        }
    }

    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            lock (this.m_dataQueue)
            {
                this.m_closed = true;
                while (this.m_dataQueue.Count > 0)
                {
                    this.m_dataQueue.Dequeue().SafeDispose();
                }
            }
            try { this.m_dataAvailable.Release(); } catch { }
            this.m_dataAvailable.SafeDispose();
        }
    }

    private async Task<ChannelPackage> WaitAsync(CancellationToken cancellationToken)
    {
        await this.m_dataAvailable.WaitAsync(cancellationToken).ConfigureDefaultAwait();
        lock (this.m_dataQueue)
        {
            if (this.m_dataQueue.Count > 0)
            {
                return this.m_dataQueue.Dequeue();
            }
        }
        throw new ObjectDisposedException(nameof(InternalChannel), "通道已被释放");
    }
}
#endif
