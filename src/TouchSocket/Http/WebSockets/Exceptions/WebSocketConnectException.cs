/* 项目“TouchSocketPro (net5)”的未合并的更改
在此之前:
using TouchSocket.Core;
using System;
在此之后:
using System;
*/

/* 项目“TouchSocketPro (netcoreapp3.1)”的未合并的更改
在此之前:
using TouchSocket.Core;
using System;
在此之后:
using System;
*/

/* 项目“TouchSocketPro (netstandard2.0)”的未合并的更改
在此之前:
using TouchSocket.Core;
using System;
在此之后:
using System;
*/

using System;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// WebSocket连接异常。
    /// </summary>
    [Serializable]
    public class WebSocketConnectException : Exception
    {
        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="mes"></param>
        /// <param name="context"></param>
        public WebSocketConnectException(string mes, HttpContext context) : base(mes)
        {
            this.Context = context;
        }

        /// <summary>
        /// HttpContext
        /// </summary>
        public HttpContext Context { get; }
    }
}