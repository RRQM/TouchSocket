using System;
using System.Threading;

namespace TouchSocket.Core
{
    /// <summary>
    /// 计数器
    /// </summary>
    public class Counter
    {
        /// <summary>
        /// 周期内的累计计数值。
        /// </summary>
        protected long m_count;

        /// <summary>
        /// 最后一次递增时间
        /// </summary>
        protected DateTime m_lastIncrement;

        /// <summary>
        /// 周期内的累计计数值。
        /// </summary>
        public long Count { get => this.m_count; }

        /// <summary>
        /// 最后一次递增时间
        /// </summary>
        public DateTime LastIncrement { get => this.m_lastIncrement; }

        /// <summary>
        /// 计数周期
        /// </summary>
        public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// 累计增加计数
        /// </summary>
        /// <param name="value"></param>
        /// <returns>返回值表示当前递增的是否在一个周期内。</returns>
        public bool Increment(int value)
        {
            bool isPeriod;
            if (DateTime.Now - this.LastIncrement > this.Period)
            {
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
    }
}