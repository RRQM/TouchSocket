using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// HeartbeatPlugin
    /// </summary>
    public abstract class HeartbeatPlugin<TClient>:PluginBase
    {
        /// <summary>
        /// 获取或设置清理无数据交互的Client，默认60秒。
        /// </summary>
        public TimeSpan Tick { get; set; } = TimeSpan.FromSeconds(60);

        protected void BeginHeartbeat()
        { 
       
        }
    }
}
