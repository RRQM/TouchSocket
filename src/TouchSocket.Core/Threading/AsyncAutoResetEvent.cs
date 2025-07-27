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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core;

/// <summary>
/// An asynchronous implementation of an AutoResetEvent.
/// </summary>
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
    /// Initializes a new instance of the <see cref="AsyncAutoResetEvent"/> class
    /// that does not inline awaiters.
    /// </summary>
    public AsyncAutoResetEvent()
        : this(allowInliningAwaiters: false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncAutoResetEvent"/> class.
    /// </summary>
    /// <param name="allowInliningAwaiters">
    /// A value indicating whether to complete the task synchronously in the <see cref="Set"/> method,
    /// as opposed to asynchronously. <see langword="false" /> better simulates the behavior of the
    /// <see cref="AutoResetEvent"/> class, but <see langword="true" /> can result in slightly better performance.
    /// </param>
    public AsyncAutoResetEvent(bool allowInliningAwaiters)
    {
        this.allowInliningAwaiters = allowInliningAwaiters;
        this.onCancellationRequestHandler = this.OnCancellationRequest;
    }

    /// <summary>
    /// Returns an awaitable that may be used to asynchronously acquire the next signal.
    /// </summary>
    /// <returns>An awaitable.</returns>
    public Task WaitOneAsync()
    {
        return this.WaitOneAsync(CancellationToken.None);
    }

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
    /// Returns an awaitable that may be used to asynchronously acquire the next signal.
    /// </summary>
    /// <param name="cancellationToken">A token whose cancellation removes the caller from the queue of those waiting for the event.</param>
    /// <returns>An awaitable.</returns>
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
    /// Unblocks one waiter or sets the signal if no waiters are present so the next waiter may proceed immediately.
    /// </summary>
    public void Set()
    {
        WaiterCompletionSource? toRelease = null;
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

        if (toRelease is object)
        {
            toRelease.Registration.Dispose();
            toRelease.TrySetResult(default(EmptyStruct));
        }
    }

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
        /// <param name="cancellationToken">The cancellation token associated with the waiter.</param>
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
