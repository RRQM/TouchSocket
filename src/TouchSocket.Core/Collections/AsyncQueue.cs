using Microsoft;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Resources;

namespace TouchSocket.Core;

/// <summary>
/// 一个线程安全的、支持异步出队的队列。
/// </summary>
/// <typeparam name="T">队列中存储的值的类型。</typeparam>
[DebuggerDisplay("Count = {Count}, Completed = {m_completeSignaled}")]
public class AsyncQueue<T>
{
    /// <summary>
    /// 由 <see cref="Completion"/> 返回的任务的来源。延迟构造。
    /// </summary>
    /// <remarks>
    /// 使用 volatile 以允许在 <see cref="Completion"/> 中的检查-锁定-检查模式可靠，
    /// 如果在锁内，一个线程初始化值并分配字段，而弱内存模型允许在初始化之前进行分配。
    /// 锁外的另一个线程可能会观察到非空字段并在实际初始化之前开始访问 Task 属性。
    /// volatile 防止围绕此字段的分配（或读取）的命令的 CPU 重排序。
    /// </remarks>
    private volatile TaskCompletionSource<object> m_completedSource;

    /// <summary>
    /// 内部元素队列。延迟构造。
    /// </summary>
    private Queue<T> m_queueElements;

    /// <summary>
    /// <see cref="DequeueAsync(CancellationToken)"/> 等待者的内部队列。延迟构造。
    /// </summary>
    private Queue<TaskCompletionSource<T>> m_dequeuingWaiters;

    /// <summary>
    /// 指示是否已调用 <see cref="Complete"/> 的值。
    /// </summary>
    private bool m_completeSignaled;

    /// <summary>
    /// 指示是否已调用 <see cref="OnCompleted"/> 的标志。
    /// </summary>
    private bool m_onCompletedInvoked;

    /// <summary>
    /// 初始化 <see cref="AsyncQueue{T}"/> 类的新实例。
    /// </summary>
    public AsyncQueue()
    {
    }

    /// <summary>
    /// 获取一个值，指示队列当前是否为空。
    /// </summary>
    public bool IsEmpty => this.Count == 0;

    /// <summary>
    /// 获取队列中当前元素的数量。
    /// </summary>
    public int Count
    {
        get
        {
            lock (this.SyncRoot)
            {
                return this.m_queueElements?.Count ?? 0;
            }
        }
    }

    /// <summary>
    /// 获取一个值，指示队列是否为空且已调用 <see cref="Complete" />。
    /// </summary>
    /// <remarks>
    /// 这在功能上与 <see cref="Completion"/>.IsCompleted 冗余，但此属性
    /// 不会导致 <see cref="Completion"/> 可能的任务的延迟实例化，
    /// 如果没有其他原因需要任务存在。
    /// </remarks>
    public bool IsCompleted
    {
        get
        {
            lock (this.SyncRoot)
            {
                return this.m_completeSignaled && this.IsEmpty;
            }
        }
    }

    /// <summary>
    /// 获取一个任务，当调用 <see cref="Complete"/> 且队列为空时，该任务会转换为完成状态。
    /// </summary>
    public Task Completion
    {
        get
        {
            if (this.m_completedSource is null)
            {
                lock (this.SyncRoot)
                {
                    if (this.m_completedSource is null)
                    {
                        if (this.IsCompleted)
                        {
                            return EasyTask.CompletedTask;
                        }
                        else
                        {
                            this.m_completedSource = new TaskCompletionSource<object>();
                        }
                    }
                }
            }

            return this.m_completedSource.Task;
        }
    }

    /// <summary>
    /// 获取此队列使用的同步对象。
    /// </summary>
    protected Lock SyncRoot => new Lock();

    /// <summary>
    /// 获取队列的初始容量。
    /// </summary>
    protected virtual int InitialCapacity => 4;

    /// <summary>
    /// 表示不再有元素会被入队。
    /// </summary>
    /// <remarks>
    /// 此方法会立即返回。
    /// 在调用此方法之前入队的元素仍然可以被出队。
    /// 只有在调用此方法并且队列为空后，<see cref="IsCompleted" /> 才会返回 true。
    /// </remarks>
    public void Complete()
    {
        lock (this.SyncRoot)
        {
            this.m_completeSignaled = true;
        }

        this.CompleteIfNecessary();
    }

    /// <summary>
    /// 将一个元素添加到队列的尾部。
    /// </summary>
    /// <param name="value">要添加的值。</param>
    /// <exception cref="InvalidOperationException">如果已调用 <see cref="Complete" />，则抛出此异常。使用 <see cref="TryEnqueue" /> 可避免此异常。</exception>
    public void Enqueue(T value)
    {
        if (!this.TryEnqueue(value))
        {
            ThrowHelper.ThrowInvalidOperationException(TouchSocketCoreResource.InvalidAfterCompleted);
        }
    }

    /// <summary>
    /// 如果队列尚未完成，则将一个元素添加到队列的尾部。
    /// </summary>
    /// <param name="value">要添加的值。</param>
    /// <returns>如果值已添加到队列，则返回 <see langword="true" />；如果队列已完成，则返回 <see langword="false" />。</returns>
    public bool TryEnqueue(T value)
    {
        var alreadyDispatched = false;
        lock (this.SyncRoot)
        {
            if (this.m_completeSignaled)
            {
                return false;
            }

            // 是否有出队者在等待此项？
            while (this.m_dequeuingWaiters?.Count > 0)
            {
                var waitingDequeuer = this.m_dequeuingWaiters.Dequeue();
                if (waitingDequeuer.TrySetResult(value))
                {
                    alreadyDispatched = true;
                    break;
                }
            }

            this.FreeCanceledDequeuers();

            if (!alreadyDispatched)
            {
                this.m_queueElements ??= new Queue<T>(this.InitialCapacity);

                this.m_queueElements.Enqueue(value);
            }
        }

        this.OnEnqueued(value, alreadyDispatched);

        return true;
    }

    /// <summary>
    /// 获取队列头部的值而不将其从队列中移除（如果队列非空）。
    /// </summary>
    /// <param name="value">接收队列头部的值；如果队列为空，则为元素类型的默认值。</param>
    /// <returns>如果队列非空，则返回 <see langword="true" />；否则返回 <see langword="false" />。</returns>
    public bool TryPeek(out T value)
    {
        lock (this.SyncRoot)
        {
            if (this.m_queueElements is object && this.m_queueElements.Count > 0)
            {
                value = this.m_queueElements.Peek();
                return true;
            }
            else
            {
                value = default(T)!;
                return false;
            }
        }
    }

    /// <summary>
    /// 获取队列头部的值而不将其从队列中移除。
    /// </summary>
    /// <exception cref="InvalidOperationException">如果队列为空，则抛出此异常。</exception>
    public T Peek()
    {
        if (!this.TryPeek(out T value))
        {
            ThrowHelper.ThrowInvalidOperationException(TouchSocketCoreResource.QueueEmpty);
        }

        return value;
    }

    /// <summary>
    /// 获取一个任务，其结果是队列头部的元素。
    /// </summary>
    /// <param name="cancellationToken">
    /// 一个令牌，其取消表示对该项失去兴趣。
    /// 取消此令牌并不保证任务会在分配队列头部的结果元素之前被取消。
    /// 调用者有责任在取消后确保任务被取消，或者它有一个结果，调用者需要负责处理。
    /// </param>
    /// <returns>一个任务，其结果是队列头部的元素。</returns>
    /// <exception cref="OperationCanceledException">
    /// 当此实例的队列为空且已调用 <see cref="Complete()"/> 时抛出。
    /// 当 <paramref name="cancellationToken"/> 在可以出队工作项之前被取消时也会抛出。
    /// </exception>
    public Task<T> DequeueAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return EasyTask.FromCanceled<T>(cancellationToken);
        }

        T result;
        lock (this.SyncRoot)
        {
            if (this.IsCompleted)
            {
                return EasyTask.FromCanceled<T>(new CancellationToken(true));
            }

            if (this.m_queueElements?.Count > 0)
            {
                result = this.m_queueElements.Dequeue();
            }
            else
            {
                if (this.m_dequeuingWaiters is null)
                {
                    this.m_dequeuingWaiters = new Queue<TaskCompletionSource<T>>(capacity: 2);
                }
                else
                {
                    this.FreeCanceledDequeuers();
                }

                // 替换以下代码行：
                // var waiterTcs = new TaskCompletionSourceWithoutInlining<T>(allowInliningContinuations: false);

                // 修复后的代码如下：
                var waiterTcs = new TaskCompletionSource<T>();
                waiterTcs.AttachCancellation(cancellationToken);
                this.m_dequeuingWaiters.Enqueue(waiterTcs);
                return waiterTcs.Task;
            }
        }

        this.CompleteIfNecessary();
        return Task.FromResult(result);
    }

    /// <summary>
    /// 如果队列头部有可用的元素，则立即将其出队，否则返回而不出队。
    /// </summary>
    /// <param name="value">接收队列头部的元素；如果队列为空，则为 <c>default(T)</c>。</param>
    /// <returns>如果有元素被出队，则返回 <see langword="true" />；如果队列为空，则返回 <see langword="false" />。</returns>
    public bool TryDequeue(out T value)
    {
        var result = this.TryDequeueInternal(null, out value);
        this.CompleteIfNecessary();
        return result;
    }

    /// <summary>
    /// 将此队列的副本作为数组返回。
    /// </summary>
    public T[] ToArray()
    {
        lock (this.SyncRoot)
        {
            return this.m_queueElements?.ToArray() ?? [];
        }
    }

    /// <summary>
    /// 如果队列头部有满足指定检查的可用元素，则立即将其出队；
    /// 否则返回而不出队。
    /// </summary>
    /// <param name="valueCheck">必须成功以出队的头部元素的测试。</param>
    /// <param name="value">接收队列头部的元素；如果队列为空，则为 <c>default(T)</c>。</param>
    /// <returns>如果有元素被出队，则返回 <see langword="true" />；如果队列为空，则返回 <see langword="false" />。</returns>
    protected bool TryDequeue(Predicate<T> valueCheck, out T value)
    {
        bool result = this.TryDequeueInternal(valueCheck, out value);
        this.CompleteIfNecessary();
        return result;
    }

    /// <summary>
    /// 当一个值被入队时调用。
    /// </summary>
    /// <param name="value">入队的值。</param>
    /// <param name="alreadyDispatched">
    /// 如果该项将跳过队列，因为已有出队者在等待项，则为 <see langword="true" />；
    /// 如果该项实际被添加到队列，则为 <see langword="false" />。
    /// </param>
    protected virtual void OnEnqueued(T value, bool alreadyDispatched)
    {
    }

    /// <summary>
    /// 当一个值被出队时调用。
    /// </summary>
    /// <param name="value">出队的值。</param>
    protected virtual void OnDequeued(T value)
    {
    }

    /// <summary>
    /// 当队列完成时调用。
    /// </summary>
    protected virtual void OnCompleted()
    {
    }

    /// <summary>
    /// 如果队列头部有可用的元素，则立即将其出队；
    /// 否则返回而不出队。
    /// </summary>
    /// <param name="valueCheck">必须成功以出队的头部元素的测试。</param>
    /// <param name="value">接收队列头部的元素；如果队列为空，则为 <c>default(T)</c>。</param>
    /// <returns><c><see langword="true"/></c> 如果有元素被出队；如果队列为空，则返回 <see langword="false" />。</returns>
    private bool TryDequeueInternal(Predicate<T> valueCheck, out T value)
    {
        bool dequeued;
        lock (this.SyncRoot)
        {
            if (this.m_queueElements is object && this.m_queueElements.Count > 0 && (valueCheck is null || valueCheck(this.m_queueElements.Peek())))
            {
                value = this.m_queueElements.Dequeue();
                dequeued = true;
            }
            else
            {
                value = default(T)!;
                dequeued = false;
            }
        }

        if (dequeued)
        {
            this.OnDequeued(value);
        }

        return dequeued;
    }

    /// <summary>
    /// 如果已发出信号且队列为空，则将此队列转换为完成状态。
    /// </summary>
    private void CompleteIfNecessary()
    {
        //ThrowHelper.AssertFalse(Monitor.IsEntered(this.SyncRoot), nameof(Monitor.IsEntered)); // 重要，因为我们将转换任务为完成。

        bool transitionTaskSource, invokeOnCompleted = false;
        lock (this.SyncRoot)
        {
            transitionTaskSource = this.m_completeSignaled && (this.m_queueElements is null || this.m_queueElements.Count == 0);
            if (transitionTaskSource)
            {
                invokeOnCompleted = !this.m_onCompletedInvoked;
                this.m_onCompletedInvoked = true;
                while (this.m_dequeuingWaiters?.Count > 0)
                {
                    this.m_dequeuingWaiters.Dequeue().TrySetCanceled();
                }
            }
        }

        if (transitionTaskSource)
        {
            this.m_completedSource?.TrySetResult(null);
            if (invokeOnCompleted)
            {
                this.OnCompleted();
            }
        }
    }

    /// <summary>
    /// 从等待队列的头部清除尽可能多的已取消的出队者。
    /// </summary>
    private void FreeCanceledDequeuers()
    {
        lock (this.SyncRoot)
        {
            while (this.m_dequeuingWaiters?.Count > 0 && this.m_dequeuingWaiters.Peek().Task.IsCompleted)
            {
                this.m_dequeuingWaiters.Dequeue();
            }
        }
    }
}
