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

namespace TouchSocket.Core
{
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
            if (!await semaphoreSlim.WaitAsync(millisecondsTimeout, token).ConfigureAwait(false))
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
            return task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 同步配置ConfigureAwait为false时的执行。
        /// </summary>
        /// <param name="task"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetFalseAwaitResult(this Task task)
        {
            task.ConfigureAwait(false).GetAwaiter().GetResult();
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
            return task.ConfigureAwait(false);
        }

        /// <summary>
        /// 配置ConfigureAwait为false。
        /// </summary>
        /// <param name="task"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable ConfigureFalseAwait(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        /// <summary>
        /// 异步等待指定最大时间
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="task"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static async Task<TResult> WaitAsync<TResult>(this Task<TResult> task, TimeSpan millisecondsTimeout)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var delayTask = Task.Delay(millisecondsTimeout, timeoutCancellationTokenSource.Token);
                _ = delayTask.ConfigureAwait(false);
                if (await Task.WhenAny(task, delayTask).ConfigureAwait(false) == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    return await task.ConfigureAwait(false);
                }
                ThrowHelper.ThrowTimeoutException();
                return default;
            }
        }

        /// <summary>
        /// 异步等待指定最大时间
        /// </summary>
        /// <param name="task"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static async Task WaitAsync(this Task task, TimeSpan millisecondsTimeout)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var delayTask = Task.Delay(millisecondsTimeout, timeoutCancellationTokenSource.Token);
                _ = delayTask.ConfigureAwait(false);
                if (await Task.WhenAny(task, delayTask).ConfigureAwait(false) == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    await task.ConfigureAwait(false);
                    return;
                }
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

        #endregion Task
    }
}