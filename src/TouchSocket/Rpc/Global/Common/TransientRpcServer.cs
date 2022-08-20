using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// TransientRpcServer
    /// </summary>
    public abstract class TransientRpcServer : RpcServer, ITransientRpcServer
    {
        ICallContext ITransientRpcServer.CallContext { get; set; }

        /// <summary>
        /// 调用上下文。
        /// </summary>
        protected ICallContext CallContext => ((ITransientRpcServer)this).CallContext;
    }
}
