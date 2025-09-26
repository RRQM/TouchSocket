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

namespace TouchSocket.Core;

/// <summary>
/// 缓存键值
/// </summary>
public partial interface ICacheAsync<TKey, TValue>
{
    /// <summary>
    /// 添加缓存。当缓存存在时，不会添加成功。
    /// </summary>
    /// <param name="entity">要添加的缓存项，类型为ICacheEntry泛型接口。</param>
    /// <param name="cancellationToken"></param>
    /// <returns>返回一个<see cref="bool"/>类型的异步操作结果，表示添加缓存项的操作是否成功。</returns>
    Task<bool> AddCacheAsync(ICacheEntry<TKey, TValue> entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 清空所有缓存
    /// </summary>
    /// <returns>一个异步任务，表示清空缓存操作</returns>
    Task ClearCacheAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步判断指定键的缓存是否存在且在生命周期内。
    /// </summary>
    /// <param name="key">缓存的键。</param>
    /// <param name="cancellationToken"></param>
    /// <returns>返回一个Task对象，其结果指示缓存是否存在且在生命周期内。</returns>
    /// <exception cref="ArgumentNullException">当键值为空时抛出。</exception>
    Task<bool> ContainsCacheAsync(TKey key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步获取指定键的缓存条目。
    /// </summary>
    /// <param name="key">用于检索缓存条目的键。</param>
    /// <param name="cancellationToken"></param>
    /// <returns>返回一个任务，该任务结果包含缓存条目。</returns>
    /// <exception cref="ArgumentNullException">当键为<see langword="null"/>时抛出此异常。</exception>
    Task<ICacheEntry<TKey, TValue>> GetCacheAsync(TKey key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步移除缓存项。
    /// </summary>
    /// <param name="key">缓存项的键。</param>
    /// <param name="cancellationToken"></param>
    /// <returns>移除操作是否成功的布尔值。</returns>
    Task<bool> RemoveCacheAsync(TKey key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置缓存，不管缓存存不存在，都会添加。
    /// </summary>
    /// <param name="entity">要添加到缓存中的项，类型为ICacheEntry泛型接口。</param>
    /// <param name="cancellationToken"></param>
    /// <returns>返回一个<see cref="bool"/>类型的异步操作结果，表示缓存设置操作是否成功。</returns>
    /// <exception cref="ArgumentNullException">当尝试将null作为缓存项添加时，抛出此异常。</exception>
    Task<bool> SetCacheAsync(ICacheEntry<TKey, TValue> entity, CancellationToken cancellationToken = default);
}