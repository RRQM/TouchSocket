/* 项目“TouchSocketPro (net5)”的未合并的更改
在此之前:
using TouchSocket.Core.Dependency;
using System;
在此之后:
using System;
*/

/* 项目“TouchSocketPro (netcoreapp3.1)”的未合并的更改
在此之前:
using TouchSocket.Core.Dependency;
using System;
在此之后:
using System;
*/

/* 项目“TouchSocketPro (netstandard2.0)”的未合并的更改
在此之前:
using TouchSocket.Core.Dependency;
using System;
在此之后:
using System;
*/

using System.Threading;
using TouchSocket.Core.Dependency;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// WebSocketExtensions
    /// </summary>
    public static class WebSocketExtensions
    {
        /// <summary>
        /// 心跳Timer
        /// </summary>
        public static readonly DependencyProperty HeartbeatTimerProperty =
       DependencyProperty.Register("HeartbeatTimer", typeof(Timer), typeof(WebSocketExtensions), null);
    }
}