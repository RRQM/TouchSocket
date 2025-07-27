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
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core;

/// <summary>
/// A flavor of <see cref="ManualResetEvent"/> that can be asynchronously awaited on.
/// </summary>
[DebuggerDisplay("Signaled: {IsSet}")]
public class AsyncManualResetEvent
{
    /// <summary>
    /// Whether the task completion source should allow executing continuations synchronously.
    /// </summary>
    private readonly bool allowInliningAwaiters;

    /// <summary>
    /// The object to lock when accessing fields.
    /// </summary>
    private readonly Lock syncObject = new();

    /// <summary>
    /// The source of the task to return from <see cref="WaitOneAsync()"/>.
    /// </summary>
    /// <devremarks>
    /// This should not need the volatile modifier because it is
    /// always accessed within a lock.
    /// </devremarks>
    private TaskCompletionSourceWithoutInlining<EmptyStruct> taskCompletionSource;

    /// <summary>
    /// A flag indicating whether the event is signaled.
    /// When this is set to true, it's possible that
    /// <see cref="taskCompletionSource"/>.Task.IsCompleted is still false
    /// if the completion has been scheduled asynchronously.
    /// Thus, this field should be the definitive answer as to whether
    /// the event is signaled because it is synchronously updated.
    /// </summary>
    /// <devremarks>
    /// This should not need the volatile modifier because it is
    /// always accessed within a lock.
    /// </devremarks>
    private bool isSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncManualResetEvent"/> class.
    /// </summary>
    /// <param name="initialState">A value indicating whether the event should be initially signaled.</param>
    /// <param name="allowInliningAwaiters">
    /// A value indicating whether to allow <see cref="WaitOneAsync()"/> callers' continuations to execute
    /// on the thread that calls <see cref="SetAsync()"/> before the call returns.
    /// <see cref="SetAsync()"/> callers should not hold private locks if this value is <see langword="true" /> to avoid deadlocks.
    /// When <see langword="false" />, the task returned from <see cref="WaitOneAsync()"/> may not have fully transitioned to
    /// its completed state by the time <see cref="SetAsync()"/> returns to its caller.
    /// </param>
    public AsyncManualResetEvent(bool initialState = false, bool allowInliningAwaiters = false)
    {
        this.allowInliningAwaiters = allowInliningAwaiters;

        this.taskCompletionSource = this.CreateTaskSource();
        this.isSet = initialState;
        if (initialState)
        {
            this.taskCompletionSource.SetResult(EmptyStruct.Instance);
        }
    }

    /// <summary>
    /// Gets a value indicating whether the event is currently in a signaled state.
    /// </summary>
    public bool IsSet
    {
        get
        {
            lock (this.syncObject)
            {
                return this.isSet;
            }
        }
    }

    /// <summary>
    /// Returns a task that will be completed when this event is set.
    /// </summary>
    public Task WaitOneAsync()
    {
        lock (this.syncObject)
        {
            return this.taskCompletionSource.Task;
        }
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
    /// Returns a task that will be completed when this event is set.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the event is set, or cancels with the <paramref name="cancellationToken"/>.</returns>
    public Task WaitOneAsync(CancellationToken cancellationToken) => this.WaitOneAsync().WithCancellation(cancellationToken);

    /// <summary>
    /// Sets this event to unblock callers of <see cref="WaitOneAsync()"/>.
    /// </summary>
    /// <returns>A task that completes when the signal has been set.</returns>
    /// <remarks>
    /// <para>
    /// On .NET versions prior to 4.6:
    /// This method may return before the signal set has propagated (so <see cref="IsSet"/> may return <see langword="false" /> for a bit more if called immediately).
    /// The returned task completes when the signal has definitely been set.
    /// </para>
    /// <para>
    /// On .NET 4.6 and later:
    /// This method is not asynchronous. The returned Task is always completed.
    /// </para>
    /// </remarks>
    [Obsolete("Use Set() instead."), EditorBrowsable(EditorBrowsableState.Never)]
    public Task SetAsync()
    {
        TaskCompletionSourceWithoutInlining<EmptyStruct>? tcs = null;
        var transitionRequired = false;
        lock (this.syncObject)
        {
            transitionRequired = !this.isSet;
            tcs = this.taskCompletionSource;
            this.isSet = true;
        }

        // Snap the Task that is exposed to the outside so we return that one.
        // Once we complete the TaskCompletionSourceWithoutInlinining's task,
        // the Task property will return the inner Task.
        // SetAsync should return the same Task that WaitAsync callers would have observed previously.
        Task result = tcs.Task;

        if (transitionRequired)
        {
            tcs.TrySetResult(default(EmptyStruct));
        }

        return result;
    }

    /// <summary>
    /// Sets this event to unblock callers of <see cref="WaitOneAsync()"/>.
    /// </summary>
    public void Set()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        this.SetAsync();
#pragma warning restore CS0618 // Type or member is obsolete
    }

    /// <summary>
    /// Resets this event to a state that will block callers of <see cref="WaitOneAsync()"/>.
    /// </summary>
    public void Reset()
    {
        lock (this.syncObject)
        {
            if (this.isSet)
            {
                this.taskCompletionSource = this.CreateTaskSource();
                this.isSet = false;
            }
        }
    }

    /// <summary>
    /// Sets and immediately resets this event, allowing all current waiters to unblock.
    /// </summary>
    /// <returns>A task that completes when the signal has been set.</returns>
    /// <remarks>
    /// <para>
    /// On .NET versions prior to 4.6:
    /// This method may return before the signal set has propagated (so <see cref="IsSet"/> may return <see langword="false" /> for a bit more if called immediately).
    /// The returned task completes when the signal has definitely been set.
    /// </para>
    /// <para>
    /// On .NET 4.6 and later:
    /// This method is not asynchronous. The returned Task is always completed.
    /// </para>
    /// </remarks>
    [Obsolete("Use PulseAll() instead."), EditorBrowsable(EditorBrowsableState.Never)]
    public Task PulseAllAsync()
    {
        TaskCompletionSourceWithoutInlining<EmptyStruct>? tcs = null;
        lock (this.syncObject)
        {
            // Atomically replace the completion source with a new, uncompleted source
            // while capturing the previous one so we can complete it.
            // This ensures that we don't leave a gap in time where WaitAsync() will
            // continue to return completed Tasks due to a Pulse method which should
            // execute instantaneously.
            tcs = this.taskCompletionSource;
            this.taskCompletionSource = this.CreateTaskSource();
            this.isSet = false;
        }

        // Snap the Task that is exposed to the outside so we return that one.
        // Once we complete the TaskCompletionSourceWithoutInlinining's task,
        // the Task property will return the inner Task.
        // PulseAllAsync should return the same Task that WaitAsync callers would have observed previously.
        Task result = tcs.Task;
        tcs.TrySetResult(default(EmptyStruct));
        return result;
    }

    /// <summary>
    /// Sets and immediately resets this event, allowing all current waiters to unblock.
    /// </summary>
    public void PulseAll()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        this.PulseAllAsync();
#pragma warning restore CS0618 // Type or member is obsolete
    }

    /// <summary>
    /// Gets an awaiter that completes when this event is signaled.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public TaskAwaiter GetAwaiter()
    {
        return this.WaitOneAsync().GetAwaiter();
    }

    /// <summary>
    /// Creates a new TaskCompletionSource to represent an unset event.
    /// </summary>
    private TaskCompletionSourceWithoutInlining<EmptyStruct> CreateTaskSource()
    {
        return new TaskCompletionSourceWithoutInlining<EmptyStruct>(this.allowInliningAwaiters);
    }
}
