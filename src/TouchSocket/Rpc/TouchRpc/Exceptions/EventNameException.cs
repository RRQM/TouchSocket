
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

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// 事件名称异常
    /// </summary>
    [Serializable]
    public class EventNameException : Exception
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public EventNameException()
        { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        public EventNameException(string message) : base(message) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public EventNameException(string message, System.Exception inner) : base(message, inner) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected EventNameException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
