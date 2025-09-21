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

using System.Runtime.CompilerServices;
using System.Threading.Tasks.Sources;

namespace TouchSocket.Core;

/// <summary>
/// 精简版线程安全单槽异步交接（单生产者 + 单消费者，不支持并发写队列）：
/// 写：一次只能有一个未消费数据，写调用返回的任务在该数据被读取并 Dispose 后完成。
/// 读：无数据则挂起，得到 ReadLease 后需 Dispose 触发写端完成。Complete 后拒绝新写；若无数据则后续 Read 返回完成租约。
/// </summary>
public sealed class AsyncExchange<T> : IValueTaskSource<ReadLease<T>>, IValueTaskSource
{
    private readonly Lock m_lock = new();
    private bool m_completed;
    private bool m_hasItem;
    private T m_item;

    private CancellationTokenRegistration m_readerCancelReg;
    private ManualResetValueTaskSourceCore<ReadLease<T>> m_readerCore = new() { RunContinuationsAsynchronously = true };
    private bool m_readerWaiting;

    private CancellationTokenRegistration m_writerCancelReg;
    private ManualResetValueTaskSourceCore<EmptyStruct> m_writerCore = new() { RunContinuationsAsynchronously = true };
    private bool m_writerWaiting;

    private static readonly Action<object> s_cancelReader = static s => ((AsyncExchange<T>)s!).CancelReader();
    private static readonly Action<object> s_cancelWriter = static s => ((AsyncExchange<T>)s!).CancelWriter();

    /// <summary>
    /// 获取当前是否已完成（即已调用 <see cref="Complete"/>，且没有未消费数据和挂起的读写操作）。
    /// </summary>
    public bool IsCompleted
    {
        get
        {
            lock (this.m_lock)
            {
                return this.m_completed && !this.m_hasItem && !this.m_readerWaiting && !this.m_writerWaiting;
            }
        }
    }

    /// <summary>
    /// 标记当前交接已完成。调用后不再接受新的写入请求，
    /// 若当前没有未消费数据且有挂起的读取操作，则立即完成该读取操作。
    /// </summary>
    public void Complete()
    {
        lock (this.m_lock)
        {
            if (this.m_completed)
            {
                return;
            }
            this.m_completed = true;
            if (!this.m_hasItem && this.m_readerWaiting)
            {
                this.m_readerWaiting = false;
                this.m_readerCore.SetResult(this.CreateReadLease(default, true));
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ReadLease<T> CreateReadLease(T value, bool isCompleted)
    {
        return new ReadLease<T>(this.ReleaseAfterRead, value, isCompleted);
    }

    /// <summary>
    /// 异步读取数据。如果当前有可用数据则立即返回，否则挂起等待数据写入或交接完成。
    /// 返回的 <see cref="ReadLease{T}"/> 需在读取后调用 <see cref="ReadLease{T}.Dispose"/> 以释放资源并通知写端完成。
    /// 若已完成交接，则返回已完成的租约。
    /// </summary>
    /// <param name="token">用于取消等待操作的 <see cref="CancellationToken"/>。</param>
    /// <returns>表示异步读取操作的 <see cref="ValueTask{ReadLease}"/>。</returns>
    public ValueTask<ReadLease<T>> ReadAsync(CancellationToken token = default)
    {
        lock (this.m_lock)
        {
            token.ThrowIfCancellationRequested();
            if (this.m_hasItem)
            {
                return new ValueTask<ReadLease<T>>(this.CreateReadLease(this.m_item, false));
            }
            if (this.m_completed)
            {
                return new ValueTask<ReadLease<T>>(this.CreateReadLease(default!, true));
            }
            if (this.m_readerWaiting)
            {
                throw new InvalidOperationException("A reader is already pending.");
            }
            this.m_readerCore.Reset();
            this.m_readerWaiting = true;
            if (token.CanBeCanceled)
            {
                this.m_readerCancelReg.Dispose();
#if NET6_0_OR_GREATER
                this.m_readerCancelReg = token.UnsafeRegister(s_cancelReader, this);
#else
                this.m_readerCancelReg = token.Register(s_cancelReader, this);
#endif
            }
            return new ValueTask<ReadLease<T>>(this, this.m_readerCore.Version);
        }
    }

    /// <summary>
    /// 异步写入数据。如果当前有未消费数据或有挂起的写操作，则抛出异常；
    /// 否则将数据写入并挂起等待读取端消费，消费后写操作完成。
    /// 若已完成交接，则抛出异常。
    /// </summary>
    /// <param name="value">要写入的数据。</param>
    /// <param name="token">用于取消等待操作的 <see cref="CancellationToken"/>。</param>
    /// <returns>表示异步写入操作的 <see cref="ValueTask"/>。</returns>
    public ValueTask WriteAsync(T value, CancellationToken token = default)
    {
        lock (this.m_lock)
        {
            token.ThrowIfCancellationRequested();
            if (this.m_completed)
            {
                throw new InvalidOperationException("Completed; cannot write.");
            }
            if (this.m_hasItem || this.m_writerWaiting)
            {
                throw new InvalidOperationException("Previous write not yet completed.");
            }
            this.m_item = value;
            this.m_hasItem = true;
            if (this.m_readerWaiting)
            {
                this.m_readerWaiting = false;
                this.m_readerCore.SetResult(this.CreateReadLease(this.m_item, false));
            }
            this.m_writerCore.Reset();
            this.m_writerWaiting = true;
            if (token.CanBeCanceled)
            {
                this.m_writerCancelReg.Dispose();
#if NET6_0_OR_GREATER
                this.m_writerCancelReg = token.UnsafeRegister(s_cancelWriter, this);
#else
                this.m_writerCancelReg = token.Register(s_cancelWriter, this);
#endif
            }
            return new ValueTask(this, this.m_writerCore.Version);
        }
    }

    /// <summary>
    /// 重置当前交接状态。仅在已完成且无未消费数据和挂起操作时可调用，
    /// 否则会抛出异常。重置后可重新开始新的交接流程。
    /// </summary>
    public void Reset()
    {
        lock (this.m_lock)
        {
            if (!this.m_completed)
            {
                throw new InvalidOperationException("Not completed.");
            }
            if (this.m_hasItem || this.m_readerWaiting || this.m_writerWaiting)
            {
                throw new InvalidOperationException("Pending state exists.");
            }
            this.m_item = default!;
            this.m_completed = false;
        }
    }

    private void ReleaseAfterRead()
    {
        var completeWriter = false;
        lock (this.m_lock)
        {
            if (!this.m_hasItem)
            {
                return;
            }
            this.m_hasItem = false;
            this.m_item = default!;
            if (this.m_writerWaiting)
            {
                this.m_writerWaiting = false;
                completeWriter = true;
            }
            if (this.m_completed && this.m_readerWaiting)
            {
                this.m_readerWaiting = false;
                this.m_readerCore.SetResult(this.CreateReadLease(default!, true));
            }
        }
        if (completeWriter)
        {
            this.m_writerCore.SetResult(default);
        }
    }

    private void CancelReader()
    {
        lock (this.m_lock)
        {
            if (!this.m_readerWaiting)
            {
                return;
            }
            this.m_readerWaiting = false;
            this.m_readerCore.SetException(new OperationCanceledException());
        }
    }
    private void CancelWriter()
    {
        lock (this.m_lock)
        {
            if (!this.m_writerWaiting)
            {
                return;
            }
            this.m_writerWaiting = false;
            this.m_writerCore.SetException(new OperationCanceledException());
        }
    }

    #region IValueTaskSource
    ReadLease<T> IValueTaskSource<ReadLease<T>>.GetResult(short token)
    {
        var r = this.m_readerCore.GetResult(token);
        this.m_readerCancelReg.Dispose();
        return r;
    }
    void IValueTaskSource.GetResult(short token)
    {
        this.m_writerCore.GetResult(token);
        this.m_writerCancelReg.Dispose();
    }
    ValueTaskSourceStatus IValueTaskSource<ReadLease<T>>.GetStatus(short token) => this.m_readerCore.GetStatus(token);
    ValueTaskSourceStatus IValueTaskSource.GetStatus(short token) => this.m_writerCore.GetStatus(token);
    void IValueTaskSource<ReadLease<T>>.OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        => this.m_readerCore.OnCompleted(continuation, state, token, flags);
    void IValueTaskSource.OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        => this.m_writerCore.OnCompleted(continuation, state, token, flags);
    #endregion

}