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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 一个简单的内存缓存
    /// </summary>
    public class MemoryCache<TKey, TValue> : IEnumerable<ICacheEntry<TKey, TValue>>, ICache<TKey, TValue>, ICacheAsync<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, ICacheEntry<TKey, TValue>> m_pairs = new ConcurrentDictionary<TKey, ICacheEntry<TKey, TValue>>();
        private readonly Timer m_timer;

        /// <summary>
        ///  一个简单的内存缓存
        /// </summary>
        public MemoryCache()
        {
            this.m_timer = new Timer((o) =>
            {
                var list = new List<TKey>();
                foreach (var item in this.m_pairs)
                {
                    if (DateTime.UtcNow - item.Value.UpdateTime > item.Value.Duration)
                    {
                        list.Add(item.Key);
                    }
                }
                foreach (var item in list)
                {
                    this.OnRemove(item, out _);
                }
            }, null, 0, 60 * 1000);
        }

        /// <summary>
        /// 当每个元素超时被移除时触发。
        /// </summary>
        public Action<ICacheEntry<TKey, TValue>> Remove { get; set; }

        /// <inheritdoc/>
        public bool AddCache(ICacheEntry<TKey, TValue> entity)
        {
            return this.m_pairs.TryAdd(entity.Key, entity);
        }

        /// <inheritdoc/>
        public Task<bool> AddCacheAsync(ICacheEntry<TKey, TValue> entity)
        {
            return Task.FromResult(this.AddCache(entity));
        }

        /// <inheritdoc/>
        public void ClearCache()
        {
            this.m_pairs.Clear();
        }

        /// <inheritdoc/>
        public Task ClearCacheAsync()
        {
            this.ClearCache();
            return EasyTask.CompletedTask;
        }

        /// <inheritdoc/>
        public bool ContainsCache(TKey key)
        {
            if (this.m_pairs.TryGetValue(key, out var cache))
            {
                if (cache.Duration == TimeSpan.Zero)
                {
                    return true;
                }
                else
                {
                    if (DateTime.UtcNow - cache.UpdateTime > cache.Duration)
                    {
                        this.OnRemove(key, out _);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <inheritdoc/>
        public Task<bool> ContainsCacheAsync(TKey key)
        {
            return Task.FromResult(this.ContainsCache(key));
        }

        /// <inheritdoc/>
        public ICacheEntry<TKey, TValue> GetCache(TKey key)
        {
            if (this.m_pairs.TryGetValue(key, out var cache))
            {
                if (cache.Duration == TimeSpan.Zero)
                {
                    return cache;
                }
                else
                {
                    if (DateTime.UtcNow - cache.UpdateTime > cache.Duration)
                    {
                        this.OnRemove(key, out _);
                        return default;
                    }
                    else
                    {
                        return cache;
                    }
                }
            }
            return default;
        }

        /// <inheritdoc/>
        public Task<ICacheEntry<TKey, TValue>> GetCacheAsync(TKey key)
        {
            return Task.FromResult(this.GetCache(key));
        }

        /// <inheritdoc/>
        public IEnumerator<ICacheEntry<TKey, TValue>> GetEnumerator()
        {
            return this.m_pairs.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc/>
        public bool RemoveCache(TKey key, out ICacheEntry<TKey, TValue> entity)
        {
            return this.OnRemove(key, out entity);
        }

        /// <inheritdoc/>
        public bool RemoveCache(TKey key)
        {
            return this.OnRemove(key, out _);
        }

        /// <inheritdoc/>
        public Task<bool> RemoveCacheAsync(TKey key)
        {
            return Task.FromResult(this.RemoveCache(key));
        }

        /// <inheritdoc/>
        public bool SetCache(ICacheEntry<TKey, TValue> entity)
        {
            this.m_pairs.AddOrUpdate(entity.Key, entity, (k, v) =>
            {
                return entity;
            });
            return true;
        }

        /// <inheritdoc/>
        public Task<bool> SetCacheAsync(ICacheEntry<TKey, TValue> entity)
        {
            return Task.FromResult(this.SetCache(entity));
        }


        private bool OnRemove(TKey key, out ICacheEntry<TKey, TValue> cache)
        {
            if (this.m_pairs.TryRemove(key, out cache))
            {
                try
                {
                    this.Remove?.Invoke(cache);
                    return true;
                }
                catch
                {
                }
            }

            return false;
        }
    }
}