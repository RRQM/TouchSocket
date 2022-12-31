//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace TouchSocket.Core
{
    /// <summary>
    /// TouchRpcPluginsManagerExtension
    /// </summary>
    public static partial class TouchRpcPluginsManagerExtension
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