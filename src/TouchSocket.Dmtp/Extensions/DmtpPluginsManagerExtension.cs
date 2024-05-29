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

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// DmtpPluginManagerExtension
    /// </summary>
    public static class DmtpPluginManagerExtension
    {
        /// <summary>
        /// DmtpRpc心跳。客户端、服务器均，但是一般建议仅客户端使用即可。
        /// <para>
        /// 默认心跳每3秒进行一次。最大失败3次即判定为断开连接。
        /// </para>
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns></returns>
        public static DmtpHeartbeatPlugin UseDmtpHeartbeat(this IPluginManager pluginManager)
        {
            var heartbeat = new DmtpHeartbeatPlugin();
            pluginManager.Add(heartbeat);
            return heartbeat;
        }

        #region WebSocketReconnection

        /// <summary>
        /// 使用<see cref="IDmtpClient"/>断线重连。
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="pluginManager"></param>
        /// <returns></returns>
        public static ReconnectionPlugin<TClient> UseDmtpReconnection<TClient>(this IPluginManager pluginManager) where TClient : IDmtpClient
        {
            var reconnectionPlugin = new DmtpReconnectionPlugin<TClient>();
            pluginManager.Add(reconnectionPlugin);
            return reconnectionPlugin;
        }

        /// <summary>
        /// 使用<see cref="IDmtpClient"/>断线重连。
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns></returns>
        public static ReconnectionPlugin<IDmtpClient> UseWebSocketReconnection(this IPluginManager pluginManager)
        {
            var reconnectionPlugin = new DmtpReconnectionPlugin<IDmtpClient>();
            pluginManager.Add(reconnectionPlugin);
            return reconnectionPlugin;
        }

        #endregion WebSocketReconnection
    }
}