using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// RPC类型
    /// </summary>
    [Flags]
    public enum RpcType
    {
        /// <summary>
        /// RRQMRPC
        /// </summary>
        RRQMRPC = 1,

        /// <summary>
        /// JsonRpc
        /// </summary>
        JsonRpc = 2,

        /// <summary>
        /// XmlRpc
        /// </summary>
        XmlRpc = 4,

        /// <summary>
        /// 反向RPC
        /// </summary>
        RRQMCallbackRPC = 8
    }
}
