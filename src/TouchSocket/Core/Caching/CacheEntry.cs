using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// 缓存实体
    /// </summary>
    public class CacheEntry<TKey, TValue> : ICacheEntry<TKey, TValue>
    {
        /// <summary>
        /// 缓存实体
        /// </summary>
        /// <param name="key"></param>
        public CacheEntry(TKey key) : this(key, default)
        {
        }

        /// <summary>
        /// 缓存实体
        /// </summary>
        public CacheEntry(TKey key, TValue value)
        {
            UpdateTime = DateTime.Now;
            Duration = TimeSpan.FromSeconds(60);
            Key = key;
            Value = value;
        }

        /// <summary>
        /// 有效区间。如果想长期有效，请使用<see cref="TimeSpan.Zero"/>
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// 键
        /// </summary>
        public TKey Key { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public TValue Value { get; set; }
    }
}