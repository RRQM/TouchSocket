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
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace TouchSocket.Core
{
    /// <summary>
    /// 异步有界队列类，基于值任务源。
    /// </summary>
    /// <typeparam name="T">队列中元素的类型。</typeparam>
    public class AsyncBoundedQueue<T> : ValueTaskSource<T>
    {
        // 使用并发队列来存储元素，支持线程安全的操作。
        private readonly ConcurrentQueue<T> m_queue = new ConcurrentQueue<T>();
        // 使用SemaphoreSlim来限制同时写入队列的操作数量，从而实现有界队列的功能。
        private readonly SemaphoreSlim m_writeLock;

        /// <summary>
        /// 获取队列的最大容量。
        /// </summary>
        public int Capacity { get; }

        /// <summary>
        /// 获取队列中的元素数量。
        /// </summary>
        public int Count => this.m_queue.Count;

        /// <summary>
        /// 获取一个值，该值指示队列是否为空。
        /// </summary>
        public bool IsEmpty => this.m_queue.IsEmpty;

        /// <summary>
        /// 获取队列中剩余的可用空间数量。
        /// </summary>
        public int FreeCount => this.Capacity - this.Count;

        /// <summary>
        /// 构造函数，初始化有界队列。
        /// </summary>
        /// <param name="capacity">队列的最大容量，必须为正数。</param>
        public AsyncBoundedQueue(int capacity)
        {
            if (capacity < 1)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(capacity), capacity, 1);
            }

            this.m_writeLock = new SemaphoreSlim(capacity, capacity);
            this.Capacity = capacity;
        }

        /// <summary>
        /// 异步取出队列中的一个元素。
        /// <para>
        /// 注意：此方法为线程安全的方法，可以在多个线程中同时调用，但是await后的结果只能被一个线程获取。所以在多个线程中调用此方法时，可能会出现await为null的情况。
        /// </para>
        /// </summary>
        /// <param name="cancellationToken">取消操作的令牌。</param>
        /// <returns>一个ValueTask对象，可以异步等待。</returns>
        public ValueTask<T> DequeueAsync(CancellationToken cancellationToken = default)
        {
            return base.ValueWaitAsync(cancellationToken);
        }

        /// <summary>
        /// 尝试从队列中取出一个元素。
        /// </summary>
        /// <param name="result">当此方法返回时，如果操作成功，则包含从队列中移除的对象；否则为默认值。</param>
        /// <returns>如果成功从队列中移除并返回了一个对象，则为 true；否则为 false。</returns>
        public bool TryDequeue(out T result)
        {
            if (this.m_queue.TryDequeue(out result))
            {
                this.m_writeLock.Release();
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        protected override ValueTaskSourceStatus GetStatus(short token)
        {
            if (this.m_queue.IsEmpty)
            {
                return ValueTaskSourceStatus.Pending;
            }
            return ValueTaskSourceStatus.Succeeded;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                this.m_writeLock.Dispose();
            }
        }
        /// <summary>
        /// 异步向队列中添加一个元素。
        /// </summary>
        /// <param name="item">要添加到队列中的元素。</param>
        /// <param name="cancellationToken">取消操作的令牌。</param>
        public async Task EnqueueAsync(T item, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            //this.m_writeLock.CurrentCount
            await this.m_writeLock.WaitAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            this.m_queue.Enqueue(item);
            base.Complete(false);
        }

        /// <summary>
        /// 异步向队列中添加一个元素。
        /// </summary>
        /// <param name="item">要添加到队列中的元素。</param>
        /// <param name="timeout">等待添加操作完成的超时时间（以毫秒为单位）。</param>
        /// <param name="cancellationToken">取消操作的令牌。</param>
        public async Task EnqueueAsync(T item, int timeout, CancellationToken cancellationToken)
        {
            this.ThrowIfDisposed();
            await this.m_writeLock.WaitTimeAsync(timeout, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            this.m_queue.Enqueue(item);
            base.Complete(true);
        }

        /// <summary>
        /// 从队列中取出一个元素并返回。
        /// </summary>
        /// <returns>队列中的一个元素。</returns>
        protected override T GetResult()
        {
            if (this.TryDequeue(out var result))
            {
                return result;
            }
            //移除下面这行代码
            //主要是因为公布了TryDequeue方法，有可能异步完成了，但是取数据时已被其他线程通过TryDequeue取走。
            //所以这里不再抛出异常，而是返回默认值。
            //ThrowHelper.ThrowInvalidOperationException("队列意外为空。");
            return default;
        }

        /// <summary>
        /// 执行调度操作，直接执行给定的操作。
        /// </summary>
        /// <param name="action">要执行的操作。</param>
        /// <param name="state">操作的状态对象。</param>
        protected override void Scheduler(Action<object> action, object state)
        {
            Task.Factory.StartNew(action,state);
        }
    }
}