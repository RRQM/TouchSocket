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

using System.Diagnostics;

namespace TouchSocket.Core;

/// <summary>
/// 表示一个异步自动重置事件，提供基于<see cref="Task"/>的异步等待机制。
/// </summary>
/// <remarks>
/// AsyncAutoResetEvent是<see cref="AutoResetEvent"/>的异步版本实现，
/// 允许多个任务异步等待信号，当信号被设置时，只有一个等待者会被唤醒，然后信号自动重置。
/// 此实现基于微软VS相关库的代码。
/// <para>
/// 与传统的<see cref="AutoResetEvent"/>不同，此类不会阻塞线程，而是返回可等待的<see cref="Task"/>，
/// 更适合在异步编程模式中使用，能够避免线程阻塞并提高系统的并发性能。
/// 此代码摘抄自微软VS相关库。
/// </para>
/// </remarks>
[DebuggerDisplay("Signaled: {signaled}")]
public class AsyncAutoResetEvent
{
    /// <summary>
    /// A queue of folks awaiting signals.
    /// </summary>
    private readonly Queue<WaiterCompletionSource> signalAwaiters = new Queue<WaiterCompletionSource>();

    /// <summary>
    /// Whether to complete the task synchronously in the <see cref="Set"/> method,
    /// as opposed to asynchronously.
    /// </summary>
    private readonly bool allowInliningAwaiters;

    /// <summary>
    /// A reusable delegate that points to the <see cref="OnCancellationRequest(object)"/> method.
    /// </summary>
    private readonly Action<object> onCancellationRequestHandler;

    /// <summary>
    /// A value indicating whether this event is already in a signaled state.
    /// </summary>
    /// <devremarks>
    /// This should not need the volatile modifier because it is
    /// always accessed within a lock.
    /// </devremarks>
    private bool signaled;

    /// <summary>
    /// 初始化<see cref="AsyncAutoResetEvent"/>类的新实例，默认不允许内联等待者。
    /// </summary>
    /// <remarks>
    /// 使用默认设置创建异步自动重置事件，等待者的完成操作将异步执行，
    /// 这样能更好地模拟<see cref="AutoResetEvent"/>的行为。
    /// </remarks>
    public AsyncAutoResetEvent()
        : this(allowInliningAwaiters: false)
    {
    }

    /// <summary>
    /// 初始化<see cref="AsyncAutoResetEvent"/>类的新实例。
    /// </summary>
    /// <param name="allowInliningAwaiters">
    /// 指示是否在<see cref="Set"/>方法中同步完成任务，而不是异步完成。
    /// <see langword="false"/>能更好地模拟<see cref="AutoResetEvent"/>类的行为，
    /// 但<see langword="true"/>可能会带来略好的性能。
    /// </param>
    public AsyncAutoResetEvent(bool allowInliningAwaiters)
    {
        this.allowInliningAwaiters = allowInliningAwaiters;
        this.onCancellationRequestHandler = this.OnCancellationRequest;
    }

    /// <summary>
    /// 返回一个可等待对象，用于异步获取下一个信号。
    /// </summary>
    /// <returns>表示异步等待操作的<see cref="Task"/>。</returns>
    /// <remarks>
    /// 如果当前事件已处于信号状态，则立即返回已完成的任务；
    /// 否则返回一个将在信号设置时完成的任务。
    /// </remarks>
    public Task WaitOneAsync()
    {
        return this.WaitOneAsync(CancellationToken.None);
    }

    /// <summary>
    /// 返回一个可等待对象，用于异步获取下一个信号，并支持超时。
    /// </summary>
    /// <param name="millisecondsTimeout">等待超时时间。</param>
    /// <returns>
    /// 表示异步等待操作的<see cref="Task{TResult}"/>，
    /// 如果在超时时间内获得信号则返回<see langword="true"/>，否则返回<see langword="false"/>。
    /// </returns>
    /// <remarks>
    /// 此方法在指定的超时时间内等待信号，如果超时则取消等待操作。
    /// 超时机制通过<see cref="CancellationTokenSource"/>实现。
    /// </remarks>
    public async Task<bool> WaitOneAsync(TimeSpan millisecondsTimeout)
    {
        try
        {
            using (var timeoutSource = new CancellationTokenSource(millisecondsTimeout))
            {
                await this.WaitOneAsync(timeoutSource.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                return true;
            }
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    /// <summary>
    /// 返回一个可等待对象，用于异步获取下一个信号，并支持取消操作。
    /// </summary>
    /// <param name="cancellationToken">用于取消等待操作的取消令牌，取消时会将调用方从等待队列中移除。</param>
    /// <returns>表示异步等待操作的<see cref="Task"/>。</returns>
    /// <exception cref="OperationCanceledException">当<paramref name="cancellationToken"/>被取消时抛出。</exception>
    /// <remarks>
    /// 如果当前事件已处于信号状态，则立即返回已完成的任务；
    /// 否则将调用方加入等待队列，并返回一个将在信号设置时完成的任务。
    /// 如果取消令牌被触发，等待者将从队列中移除并抛出<see cref="OperationCanceledException"/>。
    /// </remarks>
    public Task WaitOneAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        lock (this.signalAwaiters)
        {
            if (this.signaled)
            {
                this.signaled = false;
                return Task.CompletedTask;
            }
            else
            {
                var waiter = new WaiterCompletionSource(this, this.allowInliningAwaiters, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    waiter.TrySetCanceled(cancellationToken);
                }
                else
                {
                    this.signalAwaiters.Enqueue(waiter);
                }

                return waiter.Task;
            }
        }
    }

    /// <summary>
    /// 解除阻塞一个等待者，或者如果没有等待者则设置信号，使下一个等待者可以立即继续执行。
    /// </summary>
    /// <remarks>
    /// 如果有等待者在队列中，则唤醒队列中的第一个等待者；
    /// 如果没有等待者且事件未处于信号状态，则设置信号状态，使下一个调用<see cref="WaitOneAsync()"/>的等待者可以立即继续。
    /// 每次调用此方法只会唤醒一个等待者，符合自动重置事件的语义。
    /// </remarks>
    public void Set()
    {
        WaiterCompletionSource toRelease = null;
        lock (this.signalAwaiters)
        {
            if (this.signalAwaiters.Count > 0)
            {
                toRelease = this.signalAwaiters.Dequeue();
            }
            else if (!this.signaled)
            {
                this.signaled = true;
            }
        }

        if (toRelease is not null)
        {
            toRelease.Registration.Dispose();
            toRelease.TrySetResult(default);
        }
    }

    /// <summary>
    /// 解除阻塞所有当前等待的等待者。
    /// </summary>
    /// <remarks>
    /// 此方法会唤醒队列中的所有等待者，与标准的自动重置事件语义不同。
    /// 通常用于需要同时唤醒所有等待者的特殊场景，如应用程序关闭时的清理操作。
    /// </remarks>
    public void SetAll()
    {
        lock (this.signalAwaiters)
        {
            for (var i = 0; i < this.signalAwaiters.Count; i++)
            {
                this.Set();
            }
        }
    }

    /// <summary>
    /// Responds to cancellation requests by removing the request from the waiter queue.
    /// </summary>
    /// <param name="state">The <see cref="WaiterCompletionSource"/> passed in to the <see cref="CancellationToken.Register(Action{object}, object)"/> method.</param>
    private void OnCancellationRequest(object state)
    {
        var tcs = (WaiterCompletionSource)state;
        bool removed;
        lock (this.signalAwaiters)
        {
            removed = this.signalAwaiters.RemoveMidQueue(tcs);
        }

        // We only cancel the task if we removed it from the queue.
        // If it wasn't in the queue, either it has already been signaled
        // or it hasn't even been added to the queue yet. If the latter,
        // the Task will be canceled later so long as the signal hasn't been awarded
        // to this Task yet.
        if (removed)
        {
            tcs.TrySetCanceled(tcs.CancellationToken);
        }
    }

    /// <summary>
    /// Tracks someone waiting for a signal from the event.
    /// </summary>
    private class WaiterCompletionSource : TaskCompletionSourceWithoutInlining<EmptyStruct>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WaiterCompletionSource"/> class.
        /// </summary>
        /// <param name="owner">The event that is initializing this value.</param>
        /// <param name="allowInliningContinuations"><see langword="true" /> to allow continuations to be inlined upon the completer's callstack.</param>
        /// <param name="cancellationToken">The cancellation cancellationToken associated with the waiter.</param>
        internal WaiterCompletionSource(AsyncAutoResetEvent owner, bool allowInliningContinuations, CancellationToken cancellationToken)
            : base(allowInliningContinuations)
        {
            this.CancellationToken = cancellationToken;
            this.Registration = cancellationToken.Register(NullableHelpers.AsNullableArgAction(owner.onCancellationRequestHandler), this);
        }

        /// <summary>
        /// Gets the <see cref="CancellationToken"/> provided by the waiter.
        /// </summary>
        internal CancellationToken CancellationToken { get; private set; }

        /// <summary>
        /// Gets the registration to dispose of when the waiter receives their event.
        /// </summary>
        internal CancellationTokenRegistration Registration { get; private set; }
    }
}
