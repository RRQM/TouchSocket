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
        /// 构造函数，初始化有界队列。
        /// </summary>
        /// <param name="capacity">队列的最大容量，必须为正数。</param>
        public AsyncBoundedQueue(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");
            }

            this.m_writeLock = new SemaphoreSlim(capacity, capacity);
        }

        /// <summary>
        /// 异步取出队列中的一个元素。
        /// </summary>
        /// <param name="cancellationToken">取消操作的令牌。</param>
        /// <returns>一个ValueTask对象，可以异步等待。</returns>
        public ValueTask<T> DequeueAsync(CancellationToken cancellationToken = default)
        {
            return base.ValueWaitAsync(cancellationToken);
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

        /// <summary>
        /// 异步向队列中添加一个元素。
        /// </summary>
        /// <param name="item">要添加到队列中的元素。</param>
        /// <param name="cancellationToken">取消操作的令牌。</param>
        public async Task EnqueueAsync(T item, CancellationToken cancellationToken = default)
        {
            await this.m_writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);

            this.m_queue.Enqueue(item);
            base.Complete(false);
        }

        /// <summary>
        /// 从队列中取出一个元素并返回。
        /// </summary>
        /// <returns>队列中的一个元素。</returns>
        protected override T GetResult()
        {
            if (this.m_queue.TryDequeue(out var result))
            {
                this.m_writeLock.Release();
                return result;
            }

            ThrowHelper.ThrowInvalidOperationException("队列意外为空。");
            return default(T);
        }

        /// <summary>
        /// 执行调度操作，直接执行给定的操作。
        /// </summary>
        /// <param name="action">要执行的操作。</param>
        /// <param name="state">操作的状态对象。</param>
        protected override void Scheduler(Action<object> action, object state)
        {
            action(state);
        }
    }
}