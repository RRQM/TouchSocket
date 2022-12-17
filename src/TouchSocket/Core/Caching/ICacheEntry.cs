using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// 缓存实体接口
    /// </summary>
    public interface ICacheEntry
    {
        /// <summary>
        /// 有效区间。如果想长期有效，请使用<see cref="TimeSpan.Zero"/>
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }

    /// <summary>
    /// 缓存实体接口
    /// </summary>
    public interface ICacheEntry<out TKey, TValue> : ICacheEntry
    {
        /// <summary>
        /// 键
        /// </summary>
        public TKey Key { get; }

        /// <summary>
        /// 值
        /// </summary>
        public TValue Value { get; set; }
    }
}