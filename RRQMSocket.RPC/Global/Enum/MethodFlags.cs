using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 函数标识
    /// </summary>
    [Flags]
    public enum MethodFlags
    {
        /// <summary>
        /// 空
        /// </summary>
        None=0,

        /// <summary>
        /// 包含调用者
        /// </summary>
        IncludeCallContext = 1
    }
}
