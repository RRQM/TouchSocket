//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// CacheExtensions
    /// </summary>
    public static class CacheManagementExtensions
    {
        #region ICache

        /// <summary>
        /// 向缓存中添加键值对条目。
        /// </summary>
        /// <param name="cacheManagement">实现缓存管理的接口。</param>
        /// <param name="key">缓存条目的键。</param>
        /// <param name="value">缓存条目的值。</param>
        /// <param name="duration">缓存条目存在的持续时间（以毫秒为单位），默认为60分钟。</param>
        public static void AddCache<TKey, TValue>(this ICache<TKey, TValue> cacheManagement, TKey key, TValue value, int duration = 60 * 1000)
        {
            // 创建一个新的缓存条目实例，并设置其值和持续时间。
            cacheManagement.AddCache(new CacheEntry<TKey, TValue>(key)
            {
                Value = value,
                Duration = TimeSpan.FromMilliseconds(duration)
            });
        }

        /// <summary>
        /// 将指定的键值对存储到缓存中，并设置缓存项的过期时间。
        /// </summary>
        /// <param name="cacheManagement">实现缓存管理的接口实例。</param>
        /// <param name="key">缓存项的键。</param>
        /// <param name="value">缓存项的值。</param>
        /// <param name="duration">缓存项的持续时间（以毫秒为单位），默认为60秒。</param>
        public static void SetCache<TKey, TValue>(this ICache<TKey, TValue> cacheManagement, TKey key, TValue value, int duration = 60 * 1000)
        {
            // 创建并配置一个缓存条目实例。
            cacheManagement.SetCache(new CacheEntry<TKey, TValue>(key)
            {
                Value = value,
                Duration = TimeSpan.FromMilliseconds(duration)
            });
        }
        #endregion


        /// <summary>
        /// 尝试从缓存中获取值。
        /// </summary>
        /// <typeparam name="TKey">缓存键的类型。</typeparam>
        /// <typeparam name="TValue">缓存值的类型。</typeparam>
        /// <param name="cacheClient">缓存客户端实例。</param>
        /// <param name="key">要获取的缓存键。</param>
        /// <param name="value">输出参数，包含获取的缓存值。</param>
        /// <param name="update">是否更新缓存项的时间戳。</param>
        /// <returns>如果成功获取值则返回true，否则返回false。</returns>
        public static bool TryGetValue<TKey, TValue>(this ICache<TKey, TValue> cacheClient, TKey key, out TValue value, bool update = false)
        {
            // 从缓存中获取缓存项。
            var cacheEntry = cacheClient.GetCache(key);

            // 如果缓存项为默认值，表示缓存中不存在该键。
            if (cacheEntry == default)
            {
                value = default;
                return false;
            }

            // 如果缓存项的持续时间是零，表示该缓存项永不过期。
            if (cacheEntry.Duration == TimeSpan.Zero)
            {
                if (update)
                {
                    // 如果设置了更新时间戳，则更新缓存项的时间戳为当前时间。
                    cacheEntry.UpdateTime = DateTime.UtcNow;
                }
                value = cacheEntry.Value;
                return true;
            }
            else
            {
                // 如果当前时间与上次更新时间的差值大于等于缓存项的持续时间，表示缓存项已过期。
                if (DateTime.UtcNow - cacheEntry.UpdateTime > cacheEntry.Duration)
                {
                    // 从缓存中移除过期的缓存项。
                    cacheClient.RemoveCache(key);
                    value = default;
                    return false;
                }
                else
                {
                    if (update)
                    {
                        // 如果设置了更新时间戳，则更新缓存项的时间戳为当前时间。
                        cacheEntry.UpdateTime = DateTime.UtcNow;
                    }
                    value = cacheEntry.Value;
                    return true;
                }
            }
        }
    }
}