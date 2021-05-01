using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 调用状态
    /// </summary>
    public enum InvokeStatus
    {
        /// <summary>
        /// 未运行
        /// </summary>
        UnRun,

        /// <summary>
        /// 未找到服务
        /// </summary>
        UnFound,

        /// <summary>
        /// 成功调用
        /// </summary>
        Success,

        /// <summary>
        /// 终止执行
        /// </summary>
        Abort,

        /// <summary>
        /// 调用内部异常
        /// </summary>
        InvocationException,

        /// <summary>
        /// 其他异常
        /// </summary>
        Exception
    }
}
