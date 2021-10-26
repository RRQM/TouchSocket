using System.Collections.Concurrent;
using System.Threading;

namespace RRQMCore.Collections.Concurrent
{
    /// <summary>
    /// 智能数据安全队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IntelligentDataQueue<T> : ConcurrentQueue<T> where T : IQueueData
    {
        private long actualSize;

        private long maxSize;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxSize"></param>
        public IntelligentDataQueue(long maxSize)
        {
            this.maxSize = maxSize;
        }

        /// <summary>
        /// 允许的最大长度
        /// </summary>
        public long MaxSize
        {
            get { return maxSize; }
        }

        /// <summary>
        /// 实际尺寸
        /// </summary>
        public long ActualSize
        {
            get { return this.actualSize; }
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="item"></param>
        public new void Enqueue(T item)
        {
            SpinWait.SpinUntil(this.Check);
            this.Calculate(item.Size);
            base.Enqueue(item);
        }

        private void Calculate(long value)
        {
            lock (this)
            {
                this.actualSize += value;
            }
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
                this.Calculate(-result.Size);
                return true;
            }
            return false;
        }

        private bool Check()
        {
            return this.actualSize < this.maxSize;
        }
    }
}