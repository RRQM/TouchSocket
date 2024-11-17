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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 异步AsyncResetEvent
    /// 能够创建一个手动Reset或者自动Reset.
    /// </summary>
    public class AsyncResetEvent : DisposableObject
    {
        private readonly bool m_autoReset;

        private readonly object m_locker = new object();

        private readonly Queue<TaskCompletionSource<bool>> m_waitQueue = new Queue<TaskCompletionSource<bool>>();

        private volatile bool m_eventSet;

        /// <summary>
        /// 创建一个异步AsyncResetEvent
        /// </summary>
        /// <param name="initialState">是否包含初始信号</param>
        /// <param name="autoReset">是否自动重置</param>
        public AsyncResetEvent(bool initialState, bool autoReset)
        {
            this.m_eventSet = initialState;
            this.m_autoReset = autoReset;
        }

        /// <summary>
        /// 异步等待设置此事件
        /// </summary>
        public Task WaitOneAsync()
        {
            return this.WaitOneAsync(CancellationToken.None);
        }

        /// <summary>
        ///异步等待指定时间
        /// </summary>
        /// <param name="millisecondsTimeout">超时时间</param>
        public async Task<bool> WaitOneAsync(TimeSpan millisecondsTimeout)
        {
            try
            {
                using (var timeoutSource = new CancellationTokenSource(millisecondsTimeout))
                {
                    await this.WaitOneAsync(timeoutSource.Token).ConfigureAwait(false);
                    return true;
                }
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        /// <summary>
        /// 异步等待可取消
        /// </summary>
        /// <param name="cancellationToken">可取消令箭</param>
        public Task WaitOneAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return EasyTask.FromCanceled(cancellationToken);
            }

            lock (this.m_locker)
            {
                if (this.m_eventSet)
                {
                    if (this.m_autoReset)
                    {
                        this.m_eventSet = false;
                    }

                    return EasyTask.CompletedTask;
                }
                else
                {
#if NET45
                    var completionSource = new TaskCompletionSource<bool>();
#else
                    var completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
#endif

                    var registration = cancellationToken.Register(() =>
                    {
                        lock (this.m_locker)
                        {
#if NET45
                            completionSource.TrySetCanceled();
#else
                            completionSource.TrySetCanceled(cancellationToken);
#endif
                        }
                    }, useSynchronizationContext: false);

                    this.m_waitQueue.Enqueue(completionSource);

                    completionSource.Task.ContinueWith(
                            (_) => registration.Dispose(),
                            CancellationToken.None,
                            TaskContinuationOptions.ExecuteSynchronously,
                            TaskScheduler.Default
                            );

                    return completionSource.Task;
                }
            }
        }

        /// <summary>
        /// 重置
        /// </summary>
        public bool Reset()
        {
            lock (this.m_locker)
            {
                this.m_eventSet = false;
                return true;
            }
        }

        /// <summary>
        /// 设置信号
        /// </summary>
        public bool Set()
        {
            lock (this.m_locker)
            {
                while (this.m_waitQueue.Count > 0)
                {
                    var toRelease = this.m_waitQueue.Dequeue();

                    if (toRelease.Task.IsCompleted)
                    {
                        continue;
                    }

                    var b = toRelease.TrySetResult(true);

                    if (this.m_autoReset)
                    {
                        return b;
                    }
                }

                this.m_eventSet = true;
                return false;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.DisposedValue)
            {
                return;
            }

            if (disposing)
            {
                while (true)
                {
                    lock (this.m_locker)
                    {
                        if (this.m_waitQueue.Count == 0)
                        {
                            break;
                        }
                    }

                    this.Set();
                }
            }
            base.Dispose(disposing);
        }
    }
}