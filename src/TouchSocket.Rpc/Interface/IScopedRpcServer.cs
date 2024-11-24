using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Rpc
{
    public interface IScopedRpcServer: IRpcServer
    {
        /// <summary>
        /// 调用上下文
        /// </summary>
        ICallContext CallContext { get; set; }
    }
}
