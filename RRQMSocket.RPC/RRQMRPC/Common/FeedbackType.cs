using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// 反馈类型
    /// </summary>
    public enum FeedbackType : byte
    {
        /// <summary>
        /// 仅发送
        /// </summary>
        OnlySend,

        /// <summary>
        /// 等待，直到发送抵达
        /// </summary>
        WaitSend,

        /// <summary>
        /// 等待，直到调用完成
        /// </summary>
        WaitInvoke
    }
}
