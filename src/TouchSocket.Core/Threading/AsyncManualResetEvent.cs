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

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TouchSocket.Core;

/// <summary>
/// A flavor of <see cref="ManualResetEvent"/> that can be asynchronously awaited on.
/// </summary>
/// <remarks>
/// 此代码摘抄自微软VS相关库。
/// 这是一个可异步等待的手动重置事件实现，提供了与<see cref="ManualResetEvent"/>类似的功能，
/// 但支持异步操作模式。允许多个任务异步等待事件的设置，并在事件被设置时唤醒所有等待的任务。
/// </remarks>
[DebuggerDisplay("Signaled: {IsSet}")]
public class AsyncManualResetEvent
{
    /// <summary>
    /// Whether the task completion source should allow executing continuations synchronously.
    /// </summary>
    private readonly bool m_allowInliningAwaiters;

    /// <summary>
    /// The object to lock when accessing fields.
    /// </summary>
    private readonly Lock m_syncObject = new();

    /// <summary>
    /// The source of the task to return from <see cref="WaitOneAsync()"/>.
    /// </summary>
    /// <devremarks>
    /// This should not need the volatile modifier because it is
    /// always accessed within a lock.
    /// </devremarks>
    private TaskCompletionSourceWithoutInlining<EmptyStruct> m_taskCompletionSource;

    /// <summary>
    /// A flag indicating whether the event is signaled.
    /// When this is set to true, it's possible that
    /// <see cref="m_taskCompletionSource"/>.Task.IsCompleted is still false
    /// if the completion has been scheduled asynchronously.
    /// Thus, this field should be the definitive answer as to whether
    /// the event is signaled because it is synchronously updated.
    /// </summary>
    /// <devremarks>
    /// This should not need the volatile modifier because it is
    /// always accessed within a lock.
    /// </devremarks>
    private bool m_isSet;

    /// <summary>
    /// 初始化<see cref="AsyncManualResetEvent"/>类的新实例。
    /// </summary>
    /// <param name="initialState">指示事件是否应初始处于已设置状态的值。</param>
    /// <param name="allowInliningAwaiters">
    /// 指示是否允许<see cref="WaitOneAsync()"/>调用者的延续在调用<see cref="SetAsync()"/>的线程上执行的值，
    /// 在调用返回之前执行。如果此值为<see langword="true"/>，则<see cref="SetAsync()"/>调用者不应持有私有锁以避免死锁。
    /// 当为<see langword="false"/>时，从<see cref="WaitOneAsync()"/>返回的任务可能在<see cref="SetAsync()"/>返回给其调用者时尚未完全转换到其完成状态。
    /// </param>
    public AsyncManualResetEvent(bool initialState = false, bool allowInliningAwaiters = false)
    {
        this.m_allowInliningAwaiters = allowInliningAwaiters;

        this.m_taskCompletionSource = this.CreateTaskSource();
        this.m_isSet = initialState;
        if (initialState)
        {
            this.m_taskCompletionSource.SetResult(EmptyStruct.Instance);
        }
    }

    /// <summary>
    /// 获取一个值，指示事件当前是否处于已设置状态。
    /// </summary>
    /// <value>如果事件已设置，则为<see langword="true"/>；否则为<see langword="false"/>。</value>
    public bool IsSet
    {
        get
        {
            lock (this.m_syncObject)
            {
                return this.m_isSet;
            }
        }
    }

    /// <summary>
    /// 返回一个在此事件被设置时完成的任务。
    /// </summary>
    /// <returns>表示异步等待操作的<see cref="Task"/>。</returns>
    /// <remarks>
    /// 此方法返回一个任务，当事件被设置时该任务将完成。
    /// 如果事件已经被设置，则返回一个已完成的任务。
    /// </remarks>
    public Task WaitOneAsync()
    {
        lock (this.m_syncObject)
        {
            return this.m_taskCompletionSource.Task;
        }
    }

    /// <summary>
    /// 返回一个在此事件被设置或超时时完成的任务。
    /// </summary>
    /// <param name="millisecondsTimeout">等待的超时时间。</param>
    /// <returns>
    /// 表示异步等待操作的<see cref="ValueTask{TResult}"/>。
    /// 如果事件在超时前被设置，则返回<see langword="true"/>；如果超时，则返回<see langword="false"/>。
    /// </returns>
    /// <remarks>
    /// 此方法在指定的超时时间内等待事件被设置。
    /// 如果在超时前事件被设置，则返回<see langword="true"/>；否则返回<see langword="false"/>。
    /// </remarks>
    public async ValueTask<bool> WaitOneAsync(TimeSpan millisecondsTimeout)
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
    /// 返回一个在此事件被设置时完成的任务。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>一个在事件被设置时完成的任务，或者在<paramref name="cancellationToken"/>被取消时取消。</returns>
    /// <remarks>
    /// 此方法返回一个任务，当事件被设置时该任务将完成。
    /// 如果提供的取消令牌被取消，则返回的任务也将被取消。
    /// </remarks>
    public Task WaitOneAsync(CancellationToken cancellationToken) => this.WaitOneAsync().WithCancellation(cancellationToken);

    private Task SetAsync()
    {
        TaskCompletionSourceWithoutInlining<EmptyStruct> tcs = null;
        var transitionRequired = false;
        lock (this.m_syncObject)
        {
            transitionRequired = !this.m_isSet;
            tcs = this.m_taskCompletionSource;
            this.m_isSet = true;
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
    /// 设置此事件以解除对<see cref="WaitOneAsync()"/>调用者的阻塞。
    /// </summary>
    /// <remarks>
    /// 此方法将事件设置为已设置状态，使所有等待此事件的任务完成。
    /// 一旦调用此方法，所有当前和将来的<see cref="WaitOneAsync()"/>调用都将立即返回已完成的任务，
    /// 直到调用<see cref="Reset()"/>方法。
    /// </remarks>
    public void Set()
    {
        this.SetAsync();
    }

    /// <summary>
    /// 将此事件重置为将阻塞<see cref="WaitOneAsync()"/>调用者的状态。
    /// </summary>
    /// <remarks>
    /// 此方法将事件重置为未设置状态，使后续的<see cref="WaitOneAsync()"/>调用将等待直到事件再次被设置。
    /// 如果事件已经处于未设置状态，则此方法不执行任何操作。
    /// </remarks>
    public void Reset()
    {
        lock (this.m_syncObject)
        {
            if (this.m_isSet)
            {
                this.m_taskCompletionSource = this.CreateTaskSource();
                this.m_isSet = false;
            }
        }
    }

    private Task PulseAllAsync()
    {
        TaskCompletionSourceWithoutInlining<EmptyStruct> tcs = null;
        lock (this.m_syncObject)
        {
            // Atomically replace the completion source with a new, uncompleted source
            // while capturing the previous one so we can complete it.
            // This ensures that we don't leave a gap in time where WaitAsync() will
            // continue to return completed Tasks due to a Pulse method which should
            // execute instantaneously.
            tcs = this.m_taskCompletionSource;
            this.m_taskCompletionSource = this.CreateTaskSource();
            this.m_isSet = false;
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
    /// 设置并立即重置此事件，允许所有当前等待者解除阻塞。
    /// </summary>
    /// <remarks>
    /// 此方法执行一个脉冲操作：瞬间设置事件以释放所有当前等待的任务，然后立即将事件重置为未设置状态。
    /// 这确保只有在调用此方法时正在等待的任务会被释放，而在脉冲操作完成后开始等待的新任务将继续阻塞。
    /// </remarks>
    public void PulseAll()
    {
        this.PulseAllAsync();
    }

    /// <summary>
    /// 获取一个在此事件被设置时完成的等待器。
    /// </summary>
    /// <returns>一个<see cref="TaskAwaiter"/>，可用于异步等待事件。</returns>
    /// <remarks>
    /// 此方法使<see cref="AsyncManualResetEvent"/>能够直接在await表达式中使用。
    /// 它是编辑器隐藏的方法，通常不应直接调用。
    /// </remarks>
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
        return new TaskCompletionSourceWithoutInlining<EmptyStruct>(this.m_allowInliningAwaiters);
    }
}
