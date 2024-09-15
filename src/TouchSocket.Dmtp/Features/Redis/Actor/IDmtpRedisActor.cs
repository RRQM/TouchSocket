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
using TouchSocket.Core;

namespace TouchSocket.Dmtp.Redis
{
    /// <summary>
    /// 具有远程键值存贮的操作端。
    /// </summary>
    public interface IDmtpRedisActor : ICacheAsync<string, byte[]>, IActor
    {
        /// <summary>
        /// 序列化转换器。
        /// </summary>
        BytesSerializerConverter Converter { get; set; }

        /// <summary>
        /// 实际储存缓存。
        /// </summary>
        ICache<string, byte[]> ICache { get; set; }

        /// <summary>
        /// 超时设定。默认30000ms
        /// </summary>
        int Timeout { get; set; }

        /// <summary>
        /// 添加一个缓存项到缓存中，如果键已经存在，则不进行任何操作。
        /// 该方法用于异步地添加缓存项。
        /// </summary>
        /// <typeparam name="TValue">缓存值的类型。</typeparam>
        /// <param name="key">缓存项的键。</param>
        /// <param name="value">缓存项的值。</param>
        /// <param name="duration">缓存项的过期时间，单位为毫秒。默认为60000毫秒（1分钟）。</param>
        /// <returns>一个Task对象，表示异步操作的结果。结果为true表示添加成功，false表示失败（例如，键已经存在）。</returns>
        /// <exception cref="ArgumentNullException">如果键或值为null，则抛出该异常。</exception>
        /// <exception cref="TimeoutException">如果异步操作超时，则抛出该异常。</exception>
        /// <exception cref="Exception">如果发生其他异常，则抛出该异常。</exception>
        public Task<bool> AddAsync<TValue>(string key, TValue value, int duration = 60000);

        /// <summary>
        /// 异步获取缓存的键值对。
        /// </summary>
        /// <typeparam name="TValue">缓存值的类型</typeparam>
        /// <param name="key">缓存的键</param>
        /// <returns>缓存的值</returns>
        /// <exception cref="ArgumentNullException">如果 <paramref name="key"/> 为空或为 null，则抛出此异常。</exception>
        /// <exception cref="TimeoutException">如果获取操作超时，则抛出此异常。</exception>
        /// <exception cref="Exception">如果发生其他异常，则抛出此异常。</exception>
        public Task<TValue> GetAsync<TValue>(string key);

        /// <summary>
        /// 设置缓存值
        /// <inheritdoc cref="ICache{TKey, TValue}.SetCache(ICacheEntry{TKey, TValue})"/>
        /// </summary>
        /// <typeparam name="TValue">缓存值的类型</typeparam>
        /// <param name="key">缓存的键</param>
        /// <param name="value">缓存的值</param>
        /// <param name="duration">缓存的持续时间</param>
        /// <returns>操作是否成功</returns>
        /// <exception cref="ArgumentNullException">当参数为空时抛出</exception>
        /// <exception cref="TimeoutException">当操作超时时抛出</exception>
        /// <exception cref="Exception">当发生其他异常时抛出</exception>
        public Task<bool> SetAsync<TValue>(string key, TValue value, int duration = 60000);
    }
}