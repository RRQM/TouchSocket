using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    public class AsyncBoundedQueue<T> : ValueTaskSource<T>
    {
        private readonly ConcurrentQueue<T> m_queue = new ConcurrentQueue<T>();
        private readonly SemaphoreSlim m_writeLock;

        public AsyncBoundedQueue(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");
            m_writeLock = new SemaphoreSlim(capacity, capacity);
        }

        public ValueTask<T> DequeueAsync(CancellationToken cancellationToken = default)
        {
            return base.ValueWaitAsync(cancellationToken);
        }

        public async Task EnqueueAsync(T item, CancellationToken cancellationToken = default)
        {
            await m_writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);

            m_queue.Enqueue(item);
            base.Complete(false);
        }

        public override T GetResult()
        {
            m_writeLock.Release();
            if (this.m_queue.TryDequeue(out T result))
            {
                return result;
            }

            ThrowHelper.ThrowInvalidOperationException("队列意外为空。");
            return default(T);
        }

        protected override void Scheduler(Action<object> action, object state)
        {
            action(state);
        }
    }
}