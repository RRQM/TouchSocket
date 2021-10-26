using System.Collections.Concurrent;
using System.Threading;

namespace RRQMCore.Collections.Concurrent
{
    /// <summary>
    /// 智能安全队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IntelligentConcurrentQueue<T> : ConcurrentQueue<T>
    {
        private int count;

        private int maxCount;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxCount"></param>
        public IntelligentConcurrentQueue(int maxCount)
        {
            this.maxCount = maxCount;
        }

        /// <summary>
        /// 允许的最大长度
        /// </summary>
        public int MaxCount
        {
            get { return maxCount; }
        }

        /// <summary>
        /// 长度
        /// </summary>
        public new int Count
        {
            get { return this.count; }
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="item"></param>
        public new void Enqueue(T item)
        {
            SpinWait.SpinUntil(this.Check);
            Interlocked.Increment(ref count);
            base.Enqueue(item);
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public new bool TryDequeue(out T result)
        {
            if (base.TryDequeue(out result))
            {
                Interlocked.Decrement(ref count);
                return true;
            }
            return false;
        }

        private bool Check()
        {
            return this.count < this.maxCount;
        }
    }
}