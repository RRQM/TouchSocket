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
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;

namespace TouchSocket.Core
{
    /// <summary>
    /// WebSocketPluginsManagerExtension
    /// </summary>
    public static class WebSocketPluginsManagerExtension
    {
        /// <summary>
        /// 使用WebSocket插件。
        /// </summary>
        /// <returns>插件类型实例</returns>
        public static WebSocketFeature UseWebSocket(this IPluginsManager pluginsManager)
        {
            return pluginsManager.Add<WebSocketFeature>();
        }

        /// <summary>
        /// 使用WebSocket心跳插件，客户端、服务器均有效。但是一般建议客户端使用即可。
        /// </summary>
        /// <returns>插件类型实例</returns>
        public static WebSocketHeartbeatPlugin<TClient> UseWebSocketHeartbeat<TClient>(this IPluginsManager pluginsManager) where TClient : IHttpClientBase
        {
            var heartbeatPlugin = new WebSocketHeartbeatPlugin<TClient>();

            pluginsManager.Add(heartbeatPlugin);
            return heartbeatPlugin;
        }
    }
}