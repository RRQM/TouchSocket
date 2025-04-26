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

using Newtonsoft.Json;
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

    #endregion SemaphoreSlim

    #region Task

    /// <summary>
    /// 同步获取配置ConfigureAwait为false时的结果。
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
    /// 同步配置ConfigureAwait为false时的执行。
    /// </summary>
    /// <param name="task"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetFalseAwaitResult(this Task task)
    {
        task.ConfigureAwait(EasyTask.ContinueOnCapturedContext).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 配置ConfigureAwait为false。
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
    /// 配置ConfigureAwait为false。
    /// </summary>
    /// <param name="task"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredTaskAwaitable ConfigureFalseAwait(this Task task)
    {
        return task.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 异步等待指定最大时间
    /// </summary>
    /// <typeparam name="TResult">任务完成后的结果类型</typeparam>
    /// <param name="task">需要等待完成的任务</param>
    /// <param name="millisecondsTimeout">最大等待时间（毫秒）</param>
    /// <returns>如果在指定时间内完成，则返回任务结果；否则返回默认值</returns>
    /// <exception cref="TimeoutException">如果任务超时未完成，则抛出超时异常</exception>
    public static async Task<TResult> WaitAsync<TResult>(this Task<TResult> task, TimeSpan millisecondsTimeout)
    {
        // 使用using语句确保CancellationTokenSource在使用后正确释放资源
        using (var timeoutCancellationTokenSource = new CancellationTokenSource())
        {
            // 创建一个延迟任务，用于等待指定的最大时间
            var delayTask = Task.Delay(millisecondsTimeout, timeoutCancellationTokenSource.Token);
            // 配置延迟任务不使用上下文继续，避免阻塞调用线程
            _ = delayTask.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            // 使用Task.WhenAny等待task和delayTask中第一个完成的任务
            if (await Task.WhenAny(task, delayTask).ConfigureAwait(EasyTask.ContinueOnCapturedContext) == task)
            {
                // 如果task先完成，则取消延迟任务
                timeoutCancellationTokenSource.Cancel();
                // 返回task的结果
                return await task.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            // 如果延迟任务先完成，说明task超时，抛出超时异常
            ThrowHelper.ThrowTimeoutException();
            // 返回默认值
            return default;
        }
    }

    /// <summary>
    /// 异步等待指定最大时间
    /// </summary>
    /// <param name="task">要等待完成的任务</param>
    /// <param name="millisecondsTimeout">最大等待时间，以毫秒为单位</param>
    /// <exception cref="TimeoutException">当等待超时时抛出</exception>
    /// <remarks>
    /// 此方法允许调用者指定一个最大等待时间，如果指定时间内任务未能完成，将抛出超时异常。
    /// </remarks>
    public static async Task WaitAsync(this Task task, TimeSpan millisecondsTimeout)
    {
        // 使用using语句自动释放取消令牌源资源
        using (var timeoutCancellationTokenSource = new CancellationTokenSource())
        {
            // 创建一个延迟任务，用于计时
            var delayTask = Task.Delay(millisecondsTimeout, timeoutCancellationTokenSource.Token);
            // 配置延迟任务不使用上下文等待
            _ = delayTask.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            // 等待任务或延迟任务完成
            if (await Task.WhenAny(task, delayTask).ConfigureAwait(EasyTask.ContinueOnCapturedContext) == task)
            {
                // 如果任务先完成，取消延迟任务
                timeoutCancellationTokenSource.Cancel();
                // 继续等待任务完成
                await task.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                return;
            }
            // 如果延迟任务先完成，抛出超时异常
            ThrowHelper.ThrowTimeoutException();
            return;
        }
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
    /// 表示一个可取消的 TaskCompletionSource。
    /// </summary>
    /// <typeparam name="T">成功完成的 <see cref="Task{TResult}"/> 返回的值的类型。</typeparam>
    private class CancelableTaskCompletionSource<T>
    {
        public TaskCompletionSource<T> TaskCompletionSource { get; }
        public CancellationToken CancellationToken { get; }
        public CancellationTokenRegistration CancellationTokenRegistration { get; set; }
        public bool ContinuationScheduled { get; set; }
        public bool OnOwnerThread => Thread.CurrentThread.IsThreadPoolThread;

        public CancelableTaskCompletionSource(TaskCompletionSource<T> taskCompletionSource, CancellationToken cancellationToken)
        {
            TaskCompletionSource = taskCompletionSource;
            CancellationToken = cancellationToken;
        }
    }
    #endregion
}