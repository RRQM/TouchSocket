using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.JsonRpc
{
    /// <summary>
    /// JsonRpc协议类型
    /// </summary>
    public enum JsonRpcProtocolType : byte
    {
        /// <summary>
        /// 普通TCP协议
        /// </summary>
        Tcp,

        /// <summary>
        /// Http协议
        /// </summary>
        Http
    }
}
