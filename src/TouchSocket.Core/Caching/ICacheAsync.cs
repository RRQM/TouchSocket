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
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 缓存键值
    /// </summary>
    public partial interface ICacheAsync<TKey, TValue>
    {
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
        Task<bool> ContainsCacheAsync(TKey key);

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
        Task<ICacheEntry<TKey, TValue>> GetCacheAsync(TKey key);

        /// <summary>
        /// 移除指定键的缓存。
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<bool> RemoveCacheAsync(TKey key);

        /// <summary>
        /// 添加缓存。当缓存存在时，不会添加成功。
        /// </summary>
        /// <param name="entity">缓存实体</param>
        /// <exception cref="ArgumentNullException"></exception>
        Task<bool> AddCacheAsync(ICacheEntry<TKey, TValue> entity);
    }
}