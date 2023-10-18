using System;
using System.Threading;

namespace TouchSocket.Core
{
    /// <summary>
    /// 值类型计数器。
    /// </summary>
    public struct ValueCounter
    {
        /// <summary>
        /// 周期内的累计计数值。
        /// </summary>
        private long m_count;

        /// <summary>
        /// 最后一次递增时间
        /// </summary>
        private DateTime m_lastIncrement;

        /// <summary>
        /// 周期内的累计计数值。
        /// </summary>
        public long Count { get => this.m_count; }

        /// <summary>
        /// 最后一次递增时间
        /// </summary>
        public DateTime LastIncrement { get => this.m_lastIncrement; }

        /// <summary>
        /// 当达到一个周期时触发。
        /// </summary>
        public Action<long> OnPeriod { get; set; }

        /// <summary>
        /// 计数周期。
        /// </summary>
        public TimeSpan Period { get; set; }

        /// <summary>
        /// 累计增加计数
        /// </summary>
        /// <param name="value"></param>
        /// <returns>返回值表示当前递增的是否在一个周期内。</returns>
        public bool Increment(long value)
        {
            bool isPeriod;
            if (DateTime.Now - this.LastIncrement > this.Period)
            {
                this.OnPeriod?.Invoke(this.m_count);
                Interlocked.Exchange(ref this.m_count, 0);
                isPeriod = false;
                this.m_lastIncrement = DateTime.Now;
            }
            else
            {
                isPeriod = true;
            }
            Interlocked.Add(ref this.m_count, value);
            return isPeriod;
        }

        /// <summary>
        /// 累计增加一个计数
        /// </summary>
        /// <returns></returns>
        public bool Increment()
        {
            return this.Increment(1);
        }

        /// <summary>
        /// 重置<see cref="Count"/>和<see cref="LastIncrement"/>
        /// </summary>
        public void Reset()
        {
            this.m_count = 0;
            this.m_lastIncrement = default;
        }
    }
}