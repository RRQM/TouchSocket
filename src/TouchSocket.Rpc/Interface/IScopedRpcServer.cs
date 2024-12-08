using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 定义了一个接口，用于在特定范围内管理RPC（远程过程调用）服务器的调用上下文
    /// </summary>
    public interface IScopedRpcServer : IRpcServer
    {
        /// <summary>
        /// 调用上下文
        /// </summary>
        ICallContext CallContext { get; set; }
    }
}
