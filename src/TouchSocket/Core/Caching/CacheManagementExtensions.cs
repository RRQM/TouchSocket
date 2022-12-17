using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// CacheExtensions
    /// </summary>
    public static class CacheManagementExtensions
    {
        /// <summary>
        /// <inheritdoc cref="ICache{TKey, TValue}.AddCache(ICacheEntry{TKey, TValue})"/>
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="cacheManagement"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="duration"></param>
        public static void AddCache<TKey, TValue>(this ICache<TKey, TValue> cacheManagement, TKey key, TValue value, int duration = 60 * 1000)
        {
            cacheManagement.AddCache(new CacheEntry<TKey, TValue>(key)
            {
                Value = value,
                Duration = TimeSpan.FromMilliseconds(duration)
            });
        }

        /// <summary>
        ///  <inheritdoc cref="ICache{TKey, TValue}.SetCache(ICacheEntry{TKey, TValue})"/>
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="cacheManagement"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="duration"></param>
        public static void SetCache<TKey, TValue>(this ICache<TKey, TValue> cacheManagement, TKey key, TValue value, int duration = 60 * 1000)
        {
            cacheManagement.SetCache(new CacheEntry<TKey, TValue>(key)
            {
                Value = value,
                Duration = TimeSpan.FromMilliseconds(duration)
            });
        }
    }
}