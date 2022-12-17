using System;
using TouchSocket.Core;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// RedisClientExtensions
    /// </summary>
    public static class RedisClientExtensions
    {

        /// <summary>
        /// 获取或设置RedisClient的注入键。
        /// </summary>
        public static readonly DependencyProperty<RedisClient> RedisClientProperty =
            DependencyProperty<RedisClient>.Register("RedisClient", typeof(RedisClientExtensions), null);

        /// <summary>
        /// 获取RedisClient
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static RedisClient GetRedisClient<TClient>(this TClient client) where TClient : IDependencyTouchRpc, IDependencyObject
        {
            return client.GetValue(RedisClientProperty);
        }

    }
}
