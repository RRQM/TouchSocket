using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// Task类型
    /// </summary>
    public enum TaskReturnType
    {
        /// <summary>
        /// 没有Task
        /// </summary>
        None,

        /// <summary>
        /// 仅返回Task
        /// </summary>
        Task,

        /// <summary>
        /// 返回Task的值
        /// </summary>
        TaskObject
    }

}
