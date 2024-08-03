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

        public static void WaitTime(this SemaphoreSlim semaphoreSlim, int millisecondsTimeout, CancellationToken token)
        {
            if (!semaphoreSlim.Wait(millisecondsTimeout, token))
            {
                ThrowHelper.ThrowTimeoutException();
            }
        }

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

        public static void FireAndForget(this Task task)
        {
            if (task is null)
            {
                return;
            }

            if (task.IsCompleted)
            {
                GC.KeepAlive(task.Exception);
                return;
            }
            task.ContinueWith(t => GC.KeepAlive(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
        }

        #endregion Task
    }
}