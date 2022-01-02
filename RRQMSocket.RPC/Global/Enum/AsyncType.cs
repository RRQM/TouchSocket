using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 异步类型
    /// </summary>
    [Flags]
    public enum AsyncType
    {
        /// <summary>
        /// 无异步
        /// </summary>
        None=1,

        /// <summary>
        /// 异步
        /// </summary>
        Async=2,

        /// <summary>
        /// 返回值Task
        /// </summary>
        Task=4
    }
}
