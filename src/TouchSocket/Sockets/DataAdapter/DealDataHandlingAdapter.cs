using System;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 具有处理动作的适配器。
    /// </summary>
    public abstract class DealDataHandlingAdapter: DataHandlingAdapter
    {
        /// <summary>
        /// 最后缓存的时间
        /// </summary>
        protected DateTime LastCacheTime { get; set; }

        /// <summary>
        /// 是否在收到数据时，即刷新缓存时间。默认true。
        /// <list type="number">
        /// <item>当设为true时，将弱化<see cref="CacheTimeout"/>的作用，只要一直有数据，则缓存不会过期。</item>
        /// <item>当设为false时，则在<see cref="CacheTimeout"/>的时效内。必须完成单个缓存的数据。</item>
        /// </list>
        /// </summary>
        public bool UpdateCacheTimeWhenRev { get; set; } = true;

        /// <summary>
        /// 缓存超时时间。默认1秒。
        /// </summary>
        public TimeSpan CacheTimeout { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// 是否启用缓存超时。默认true。
        /// </summary>
        public bool CacheTimeoutEnable { get; set; } = true;
    }
}
