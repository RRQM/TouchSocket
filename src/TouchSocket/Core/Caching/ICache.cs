using System;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 缓存键值
    /// </summary>
    public interface ICache<TKey, TValue>
    {
        /// <summary>
        /// 添加缓存。当缓存存在时，不会添加成功。
        /// </summary>
        /// <param name="entity">缓存实体</param>
        /// <exception cref="ArgumentNullException"></exception>
        bool AddCache(ICacheEntry<TKey, TValue> entity);

        /// <summary>
        /// 添加缓存。当缓存存在时，不会添加成功。
        /// </summary>
        /// <param name="entity">缓存实体</param>
        /// <exception cref="ArgumentNullException"></exception>
        Task<bool> AddCacheAsync(ICacheEntry<TKey, TValue> entity);

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        void ClearCache();

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        /// <returns></returns>
        Task ClearCacheAsync();

        /// <summary>
        /// 判断缓存是否存在，且在生命周期内。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        bool ContainsCache(TKey key);

        /// <summary>
        /// 判断缓存是否存在，且在生命周期内。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<bool> ContainsCacheAsync(TKey key);

        /// <summary>
        /// 设置缓存，不管缓存存不存在，都会添加。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        bool SetCache(ICacheEntry<TKey, TValue> entity);

        /// <summary>
        /// 设置缓存，不管缓存存不存在，都会添加。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<bool> SetCacheAsync(ICacheEntry<TKey, TValue> entity);

        /// <summary>
        /// 获取指定键的缓存。
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        ICacheEntry<TKey, TValue> GetCache(TKey key);

        /// <summary>
        /// 获取指定键的缓存。
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<ICacheEntry<TKey, TValue>> GetCacheAsync(TKey key);

        /// <summary>
        /// 移除指定键的缓存。
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        bool RemoveCache(TKey key);

        /// <summary>
        /// 移除指定键的缓存。
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<bool> RemoveCacheAsync(TKey key);
    }
}