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

using System.Collections.Concurrent;

namespace TouchSocket.Sockets;

internal sealed class TcpCorePool : DisposableObject
{
    private const int MaxQueueSize = 5000;

    private readonly ConcurrentQueue<TcpCore> m_queue = new();
    private int m_count;

    public TcpCore Rent()
    {
        if (this.m_queue.TryDequeue(out var sender))
        {
            Interlocked.Decrement(ref this.m_count);
            return sender;
        }
        return new TcpCore();
    }

    public void Return(TcpCore tcpCore)
    {
        if (this.DisposedValue || Interlocked.Increment(ref this.m_count) > MaxQueueSize)
        {
            Interlocked.Decrement(ref this.m_count);
            tcpCore.Dispose();
            return;
        }

        tcpCore.Reset();
        this.m_queue.Enqueue(tcpCore);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            while (this.m_queue.TryDequeue(out var tcpCore))
            {
                tcpCore.Dispose();
            }
        }
        base.Dispose(disposing);
    }
}