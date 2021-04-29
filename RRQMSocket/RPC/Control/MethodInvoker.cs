using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 函数调用信使
    /// </summary>
    public class MethodInvoker
    {
        /// <summary>
        /// 函数标识
        /// </summary>
        public int MethodToken { get; set; }

        /// <summary>
        /// 返回值
        /// </summary>
        public object ReturnParameter { get; set; }

        /// <summary>
        /// 参数值集合
        /// </summary>
        public object[] Parameters { get; set; }

        /// <summary>
        /// 获取调用状态
        /// </summary>
        public InvokeStatus  Status { get;internal set; }

        /// <summary>
        /// 状态消息
        /// </summary>
        public string StatusMessage { get; internal  set; }
    }
}
