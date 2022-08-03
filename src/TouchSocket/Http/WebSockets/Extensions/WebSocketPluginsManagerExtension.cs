using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Http.WebSockets;

namespace TouchSocket.Core.Plugins
{
    /// <summary>
    /// WebSocketPluginsManagerExtension
    /// </summary>
    public static class WebSocketPluginsManagerExtension
    {
        /// <summary>
        /// 使用WebSocket插件
        /// </summary>
        /// <returns>插件类型实例</returns>
        public static WebSocketServerPlugin UseWebSocket(this IPluginsManager pluginsManager)
        {
            return pluginsManager.Add<WebSocketServerPlugin>();
        }
    }
}
