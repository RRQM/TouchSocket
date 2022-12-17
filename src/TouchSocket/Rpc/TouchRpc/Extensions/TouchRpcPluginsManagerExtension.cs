using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace TouchSocket.Core
{
    /// <summary>
    /// TouchRpcPluginsManagerExtension
    /// </summary>
    public static class TouchRpcPluginsManagerExtension
    {
        /// <summary>
        /// 使用Redis插件。仅：TouchRpc组成员会生效。
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <returns></returns>
        public static RedisPlugin UseRedis(this IPluginsManager pluginsManager)
        {
            return pluginsManager.Add<RedisPlugin>();
        }

        /// <summary>
        /// TouchRpc心跳。仅客户端适用。
        /// <para>
        /// 默认心跳每3秒进行一次。最大失败3次即判定为断开连接。
        /// </para>
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="pluginsManager"></param>
        /// <returns></returns>
        public static TouchRpcHeartbeatPlugin<TClient> UseTouchRpcHeartbeat<TClient>(this IPluginsManager pluginsManager) where TClient : class, ITcpClientBase, IDependencyTouchRpc
        {
            return pluginsManager.Add<TouchRpcHeartbeatPlugin<TClient>>();
        }
    }
}