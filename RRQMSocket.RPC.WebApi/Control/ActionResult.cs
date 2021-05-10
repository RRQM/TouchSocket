using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.WebApi
{
    /// <summary>
    /// 结果状态
    /// </summary>
    public class ActionResult
    {
        /// <summary>
        /// 状态类型
        /// </summary>
        public InvokeStatus Status { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }
    }
}
