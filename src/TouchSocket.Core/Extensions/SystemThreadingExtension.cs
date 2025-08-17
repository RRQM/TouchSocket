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

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core;

/// <summary>
/// SystemThreadingExtension
/// </summary>
public static class SystemThreadingExtension
{
    #region ReaderWriterLockSlim

    /// <summary>
    /// 创建一个可释放的读取锁
    /// </summary>
    /// <param name="lockSlim"></param>
    /// <returns></returns>
    public static ReadLock CreateReadLock(this ReaderWriterLockSlim lockSlim)
    {
        return new ReadLock(lockSlim);
    }

    /// <summary>
    /// 创建一个可释放的写入锁
    /// </summary>
    /// <param name="lockSlim"></param>
    /// <returns></returns>
    public static WriteLock CreateWriteLock(this ReaderWriterLockSlim lockSlim)
    {
        return new WriteLock(lockSlim);
    }

    #endregion ReaderWriterLockSlim

    #region SemaphoreSlim

    /// <summary>
    /// 使用指定的超时和取消令牌等待信号量。
    /// </summary>
    /// <param name="semaphoreSlim">要等待的信号量。</param>
    /// <param name="millisecondsTimeout">等待的超时时间（以毫秒为单位）。</param>
    /// <param name="token">用于取消操作的取消令牌。</param>
    /// <remarks>
    /// 如果信号量未在指定的超时时间内释放，则抛出超时异常。
    /// </remarks>
    public static void WaitTime(this SemaphoreSlim semaphoreSlim, int millisecondsTimeout, CancellationToken token)
    {
        if (!semaphoreSlim.Wait(millisecondsTimeout, token))
        {
            ThrowHelper.ThrowTimeoutException();
        }
    }

    /// <summary>
    /// 异步等待信号量，具有指定的超时和取消令牌。
    /// </summary>
    /// <param name="semaphoreSlim">要等待的信号量。</param>
    /// <param name="millisecondsTimeout">等待的超时时间（以毫秒为单位）。</param>
    /// <param name="token">用于取消操作的取消令牌。</param>
    /// <returns>一个Task对象，表示异步等待操作。</returns>
    /// <remarks>
    /// 如果信号量未在指定的超时时间内释放，则抛出超时异常。
    /// </remarks>
    public static async Task WaitTimeAsync(this SemaphoreSlim semaphoreSlim, int millisecondsTimeout, CancellationToken token)
    {
        if (!await semaphoreSlim.WaitAsync(millisecondsTimeout, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
        {
            ThrowHelper.ThrowTimeoutException();
        }
    }

    /// <summary>  
    /// 异步等待信号量并返回结果，支持取消令牌。  
    /// </summary>  
    /// <param name="semaphoreSlim">要等待的信号量。</param>  
    /// <param name="token">用于取消操作的取消令牌。</param>  
    /// <returns>一个 <see cref="Result"/> 对象，表示操作的结果。</returns>  
    public static async ValueTask<Result> WaitResultAsync(this SemaphoreSlim semaphoreSlim, CancellationToken token)
    {
        try
        {
            await semaphoreSlim.WaitAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return Result.Success;
        }
        catch (OperationCanceledException)
        {
            return Result.Canceled;
        }
        catch (ObjectDisposedException)
        {
            return Result.Disposed;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    #endregion SemaphoreSlim

    #region Task

    /// <summary>
    /// 如果给定的 <see cref="CancellationToken"/> 被取消，则取消 <see cref="TaskCompletionSource{TResult}.Task"/>。
    /// </summary>
    /// <typeparam name="T">成功完成的 <see cref="Task{TResult}"/> 返回的值的类型。</typeparam>
    /// <param name="taskCompletionSource">要取消的 <see cref="TaskCompletionSource{TResult}"/>。</param>
    /// <param name="cancellationToken">用于取消的 <see cref="CancellationToken"/>。</param>
    public static void AttachCancellation<T>(this TaskCompletionSource<T> taskCompletionSource, CancellationToken cancellationToken)
    {
        if (taskCompletionSource == null)
        {
            throw new ArgumentNullException(nameof(taskCompletionSource));
        }

        if (cancellationToken.CanBeCanceled && !taskCompletionSource.Task.IsCompleted)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                taskCompletionSource.TrySetCanceled();
            }
            else
            {
                var tuple = new CancelableTaskCompletionSource<T>(taskCompletionSource, cancellationToken);
                tuple.CancellationTokenRegistration = cancellationToken.Register(
                    s =>
                    {
                        var t = (CancelableTaskCompletionSource<T>)s!;
                        if (t.TaskCompletionSource.TrySetCanceled())
                        {
                        }
                    },
                    tuple,
                    useSynchronizationContext: false);

                taskCompletionSource.Task.ContinueWith(
                    (_, s) =>
                    {
                        var t = (CancelableTaskCompletionSource<T>)s!;
                        if (t.ContinuationScheduled || !t.OnOwnerThread)
                        {
                            t.CancellationTokenRegistration.Dispose();
                        }
                        else if (!t.CancellationToken.IsCancellationRequested)
                        {
                            ThreadPool.QueueUserWorkItem(
                                s2 =>
                                {
                                    try
                                    {
                                        var t2 = (CancelableTaskCompletionSource<T>)s2!;
                                        t2.CancellationTokenRegistration.Dispose();
                                    }
                                    catch
                                    {
                                    }
                                },
                                s);
                        }
                    },
                    tuple,
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
                tuple.ContinuationScheduled = true;
            }
        }
    }

    /// <summary>
    /// 配置ConfigureAwait为<see langword="false"/>。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredTaskAwaitable<T> ConfigureFalseAwait<T>(this Task<T> task)
    {
        return task.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 配置ConfigureAwait为<see langword="false"/>。
    /// </summary>
    /// <param name="task"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredTaskAwaitable ConfigureFalseAwait(this Task task)
    {
        return task.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 启动一个任务并立即返回，任务执行过程中发生的异常不会导致程序崩溃。
    /// </summary>
    /// <param name="task">要启动的异步任务。</param>
    public static void FireAndForget(this Task task)
    {
        // 检查传入的任务是否为空，如果是空则直接返回，不执行后续逻辑。
        if (task is null)
        {
            return;
        }

        // 如果任务已经完成，则检查是否有异常，如果有异常则通过GC.KeepAlive防止异常对象被过早回收。
        if (task.IsCompleted)
        {
            GC.KeepAlive(task.Exception);
            return;
        }

        // 如果任务尚未完成，则注册一个在任务出现异常时执行的继续操作。
        // 这里使用GC.KeepAlive防止异常对象被过早回收，同时通过TaskContinuationOptions.OnlyOnFaulted确保只有在任务出现异常时才执行继续操作。
        task.ContinueWith(t => GC.KeepAlive(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
    }

    /// <summary>
    /// 同步获取配置ConfigureAwait为<see langword="false"/>时的结果。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetFalseAwaitResult<T>(this Task<T> task)
    {
        return task.ConfigureAwait(EasyTask.ContinueOnCapturedContext).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 同步配置ConfigureAwait为<see langword="false"/>时的执行。
    /// </summary>
    /// <param name="task"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetFalseAwaitResult(this Task task)
    {
        task.ConfigureAwait(EasyTask.ContinueOnCapturedContext).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 表示一个可取消的 TaskCompletionSource。
    /// </summary>
    /// <typeparam name="T">成功完成的 <see cref="Task{TResult}"/> 返回的值的类型。</typeparam>
    private class CancelableTaskCompletionSource<T>
    {
        public CancelableTaskCompletionSource(TaskCompletionSource<T> taskCompletionSource, CancellationToken cancellationToken)
        {
            this.TaskCompletionSource = taskCompletionSource;
            this.CancellationToken = cancellationToken;
        }

        public CancellationToken CancellationToken { get; }
        public CancellationTokenRegistration CancellationTokenRegistration { get; set; }
        public bool ContinuationScheduled { get; set; }
        public bool OnOwnerThread => Thread.CurrentThread.IsThreadPoolThread;
        public TaskCompletionSource<T> TaskCompletionSource { get; }
    }

#if !NET6_0_OR_GREATER

    /// <summary>
    /// 等待指定的任务完成，支持超时和取消令牌。
    /// </summary>
    /// <typeparam name="TResult">任务返回的结果类型。</typeparam>
    /// <param name="task">要等待的任务。</param>
    /// <param name="timeout">等待的超时时间。</param>
    /// <param name="cancellationToken">用于取消操作的取消令牌。</param>
    /// <returns>任务的结果。</returns>
    /// <exception cref="TimeoutException">如果任务未在指定的超时时间内完成，则抛出超时异常。</exception>
    /// <exception cref="OperationCanceledException">如果操作被取消，则抛出取消异常。</exception>
    public static async Task<TResult> WaitAsync<TResult>(this Task<TResult> task, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        using var timeoutCts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, timeoutCts.Token);

        var delayTask = Task.Delay(timeout, linkedCts.Token);

        var completedTask = await Task.WhenAny(task, delayTask).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        if (completedTask == delayTask)
        {
            timeoutCts.Cancel(); // 触发超时取消
            ThrowHelper.ThrowTimeoutException();
            return default; // 不会到达这里，但为了满足返回类型
        }

        // 确保所有任务异常被传播
        return await task.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 等待指定的任务完成，支持超时和取消令牌。
    /// </summary>
    /// <param name="task">要等待的任务。</param>
    /// <param name="timeout">等待的超时时间。</param>
    /// <param name="cancellationToken">用于取消操作的取消令牌。</param>
    /// <returns>任务的结果。</returns>
    /// <exception cref="TimeoutException">如果任务未在指定的超时时间内完成，则抛出超时异常。</exception>
    /// <exception cref="OperationCanceledException">如果操作被取消，则抛出取消异常。</exception>
    public static async Task WaitAsync(this Task task, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        using var timeoutCts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, timeoutCts.Token);

        var delayTask = Task.Delay(timeout, linkedCts.Token);

        var completedTask = await Task.WhenAny(task, delayTask).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        if (completedTask == delayTask)
        {
            timeoutCts.Cancel();
            ThrowHelper.ThrowTimeoutException();
            return; // 不会到达这里，但为了满足返回类型
        }

        // 确保所有任务异常被传播
        await task.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }
#endif
    #endregion Task

    #region CancellationTokenSource
    /// <summary>
    /// 获取 <see cref="CancellationToken"/>，如果 <paramref name="tokenSource"/> 为 null，则返回一个已取消的 <see cref="CancellationToken"/>。
    /// </summary>
    /// <param name="tokenSource">用于生成 <see cref="CancellationToken"/> 的 <see cref="CancellationTokenSource"/>。</param>
    /// <returns>返回一个 <see cref="CancellationToken"/>。</returns>
    public static CancellationToken GetTokenOrCanceled(this CancellationTokenSource tokenSource)
    {
        if (tokenSource is null)
        {
            // 如果 tokenSource 为 null，则返回一个已取消的 CancellationToken
            return new CancellationToken(canceled: true);
        }
        // 如果 tokenSource 不为 null，则返回其 Token 属性
        return tokenSource.Token;
    }


    /// <summary>
    /// 安全地取消 <see cref="CancellationTokenSource"/>，并返回操作结果。
    /// </summary>
    /// <param name="tokenSource">要取消的 <see cref="CancellationTokenSource"/>。</param>
    /// <returns>一个 <see cref="Result"/> 对象，表示操作的结果。</returns>
    public static Result SafeCancel(this CancellationTokenSource tokenSource)
    {
        if (tokenSource is null)
        {
            return Result.Success;
        }
        try
        {
            tokenSource.Cancel();
            return Result.Success;
        }
        catch (ObjectDisposedException)
        {
            return Result.Disposed;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }
    #endregion

    #region MyRegion
    /// <summary>
    /// Optimistically performs some value transformation based on some field and tries to apply it back to the field,
    /// retrying as many times as necessary until no other thread is manipulating the same field.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    /// <param name="hotLocation">The field that may be manipulated by multiple threads.</param>
    /// <param name="applyChange">A function that receives the unchanged value and returns the changed value.</param>
    /// <returns>
    /// <see langword="true" /> if the location's value is changed by applying the result of the <paramref name="applyChange"/> function;
    /// <see langword="false" /> if the location's value remained the same because the last invocation of <paramref name="applyChange"/> returned the existing value.
    /// </returns>
    public static bool ApplyChangeOptimistically<T>(ref T hotLocation, Func<T, T> applyChange)
        where T : class
    {

        bool successful;
        do
        {
            var oldValue = Volatile.Read(ref hotLocation);
            var newValue = applyChange(oldValue);
            if (object.ReferenceEquals(oldValue, newValue))
            {
                // No change was actually required.
                return false;
            }

            var actualOldValue = Interlocked.CompareExchange<T>(ref hotLocation, newValue, oldValue);
            successful = object.ReferenceEquals(oldValue, actualOldValue);
        }
        while (!successful);

        return true;
    }

    /// <summary>
    /// Optimistically performs some value transformation based on some field and tries to apply it back to the field,
    /// retrying as many times as necessary until no other thread is manipulating the same field.
    /// </summary>
    /// <remarks>
    /// Use this overload when <paramref name="applyChange"/> requires a single item, as is common when updating immutable
    /// collection types. By passing the item as a method operand, the caller may be able to avoid allocating a closure
    /// object for every call.
    /// </remarks>
    /// <typeparam name="T">The type of data to apply the change to.</typeparam>
    /// <typeparam name="TArg">The type of argument passed to the <paramref name="applyChange" />.</typeparam>
    /// <param name="hotLocation">The field that may be manipulated by multiple threads.</param>
    /// <param name="applyChangeArgument">An argument to pass to <paramref name="applyChange"/>.</param>
    /// <param name="applyChange">A function that receives both the unchanged value and <paramref name="applyChangeArgument"/>, then returns the changed value.</param>
    /// <returns>
    /// <see langword="true" /> if the location's value is changed by applying the result of the <paramref name="applyChange"/> function;
    /// <see langword="false" /> if the location's value remained the same because the last invocation of <paramref name="applyChange"/> returned the existing value.
    /// </returns>
    public static bool ApplyChangeOptimistically<T, TArg>(ref T hotLocation, TArg applyChangeArgument, Func<T, TArg, T> applyChange)
        where T : class
    {
        bool successful;
        do
        {
            var oldValue = Volatile.Read(ref hotLocation);
            var newValue = applyChange(oldValue, applyChangeArgument);
            if (object.ReferenceEquals(oldValue, newValue))
            {
                // No change was actually required.
                return false;
            }

            var actualOldValue = Interlocked.CompareExchange<T>(ref hotLocation, newValue, oldValue);
            successful = object.ReferenceEquals(oldValue, actualOldValue);
        }
        while (!successful);

        return true;
    }

    /// <summary>
    /// Wraps a task with one that will complete as cancelled based on a cancellation token,
    /// allowing someone to await a task but be able to break out early by cancelling the token.
    /// </summary>
    /// <typeparam name="T">The type of value returned by the task.</typeparam>
    /// <param name="task">The task to wrap.</param>
    /// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
    /// <returns>The wrapping task.</returns>
    public static Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
    {

        if (!cancellationToken.CanBeCanceled || task.IsCompleted)
        {
            return task;
        }

        return cancellationToken.IsCancellationRequested
            ? Task.FromCanceled<T>(cancellationToken)
            : WithCancellationSlow(task, cancellationToken);
    }

    /// <summary>
    /// Wraps a task with one that will complete as cancelled based on a cancellation token,
    /// allowing someone to await a task but be able to break out early by cancelling the token.
    /// </summary>
    /// <param name="task">The task to wrap.</param>
    /// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
    /// <returns>The wrapping task.</returns>
    public static Task WithCancellation(this Task task, CancellationToken cancellationToken)
    {

        if (!cancellationToken.CanBeCanceled || task.IsCompleted)
        {
            return task;
        }

        return cancellationToken.IsCancellationRequested
            ? Task.FromCanceled(cancellationToken)
            : WithCancellationSlow(task, continueOnCapturedContext: false, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Wraps a task with one that will complete as cancelled based on a cancellation token,
    /// allowing someone to await a task but be able to break out early by cancelling the token.
    /// </summary>
    /// <param name="task">The task to wrap.</param>
    /// <param name="continueOnCapturedContext">A value indicating whether *internal* continuations required to respond to cancellation should run on the current <see cref="SynchronizationContext"/>.</param>
    /// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
    /// <returns>The wrapping task.</returns>
    internal static Task WithCancellation(this Task task, bool continueOnCapturedContext, CancellationToken cancellationToken)
    {
        if (!cancellationToken.CanBeCanceled || task.IsCompleted)
        {
            return task;
        }

        return cancellationToken.IsCancellationRequested
            ? Task.FromCanceled(cancellationToken)
            : WithCancellationSlow(task, continueOnCapturedContext, cancellationToken);
    }


    /// <summary>
    /// Wraps a task with one that will complete as cancelled based on a cancellation token,
    /// allowing someone to await a task but be able to break out early by cancelling the token.
    /// </summary>
    /// <typeparam name="T">The type of value returned by the task.</typeparam>
    /// <param name="task">The task to wrap.</param>
    /// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
    /// <returns>The wrapping task.</returns>
    private static async Task<T> WithCancellationSlow<T>(Task<T> task, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>();
        using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s!).TrySetResult(true), tcs))
        {
            if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        // Rethrow any fault/cancellation exception, even if we awaited above.
        // But if we skipped the above if branch, this will actually yield
        // on an incompleted task.
        return await task.ConfigureAwait(false);
    }

    /// <summary>
    /// Wraps a task with one that will complete as cancelled based on a cancellation token,
    /// allowing someone to await a task but be able to break out early by cancelling the token.
    /// </summary>
    /// <param name="task">The task to wrap.</param>
    /// <param name="continueOnCapturedContext">A value indicating whether *internal* continuations required to respond to cancellation should run on the current <see cref="SynchronizationContext"/>.</param>
    /// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
    /// <returns>The wrapping task.</returns>
    private static async Task WithCancellationSlow(this Task task, bool continueOnCapturedContext, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>();
        using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s!).TrySetResult(true), tcs))
        {
            if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(continueOnCapturedContext))
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        // Rethrow any fault/cancellation exception, even if we awaited above.
        // But if we skipped the above if branch, this will actually yield
        // on an incompleted task.
        await task.ConfigureAwait(continueOnCapturedContext);
    }


    #endregion
}