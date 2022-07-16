
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

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 超长异常
    /// </summary>
    [Serializable]
    public class OverlengthException : Exception
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public OverlengthException() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        public OverlengthException(string message) : base(message) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public OverlengthException(string message, System.Exception inner) : base(message, inner) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected OverlengthException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
