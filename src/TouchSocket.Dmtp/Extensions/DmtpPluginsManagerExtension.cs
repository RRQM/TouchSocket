//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// DmtpPluginsManagerExtension
    /// </summary>
    public static class DmtpPluginsManagerExtension
    {
        /// <summary>
        /// DmtpRpc心跳。客户端、服务器均，但是一般建议仅客户端使用即可。
        /// <para>
        /// 默认心跳每3秒进行一次。最大失败3次即判定为断开连接。
        /// </para>
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="pluginsManager"></param>
        /// <returns></returns>
        public static DmtpHeartbeatPlugin<TClient> UseDmtpHeartbeat<TClient>(this IPluginsManager pluginsManager) where TClient : IDmtpActorObject
        {
            var heartbeat = new DmtpHeartbeatPlugin<TClient>();
            pluginsManager.Add(heartbeat);
            return heartbeat;
        }

        /// <summary>
        /// DmtpRpc心跳。客户端、服务器均，但是一般建议仅客户端使用即可。
        /// <para>
        /// 默认心跳每3秒进行一次。最大失败3次即判定为断开连接。
        /// </para>
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <returns></returns>
        public static DmtpHeartbeatPlugin<IDmtpActorObject> UseDmtpHeartbeat(this IPluginsManager pluginsManager)
        {
            var heartbeat = new DmtpHeartbeatPlugin<IDmtpActorObject>();
            pluginsManager.Add(heartbeat);
            return heartbeat;
        }
    }
}