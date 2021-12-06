using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMCore
{
    /// <summary>
    /// 结果类型
    /// </summary>
    public enum ResultCode
    {
        /// <summary>
        /// 未执行的
        /// </summary>
        Default,

        /// <summary>
        /// 错误
        /// </summary>
        Error,

        /// <summary>
        /// 成功
        /// </summary>
        Success,

        /// <summary>
        /// 操作超时
        /// </summary>
        Overtime,

        /// <summary>
        /// 操作取消
        /// </summary>
        Canceled
    }
}
