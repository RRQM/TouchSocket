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
        public InvokeStatus  Status { get; set; }

        /// <summary>
        /// 状态消息
        /// </summary>
        public string StatusMessage { get;  set; }
       
        /// <summary>
        /// 可以传递其他类型的数据容器
        /// </summary>
        public object Flag { get; set; }
    }
}
