using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// ScopedRpcServer
    /// </summary>
    public abstract class ScopedRpcServer<TCallContext> : RpcServer, IScopedRpcServer where TCallContext : ICallContext
    {
        ICallContext IScopedRpcServer.CallContext { get; set; }

        /// <summary>
        /// 调用上下文。
        /// </summary>
        protected TCallContext CallContext => (((IScopedRpcServer)this).CallContext is TCallContext Transient) ? Transient : default;
    }

    /// <summary>
    /// ScopedRpcServer
    /// </summary>
    public abstract class ScopedRpcServer : RpcServer, IScopedRpcServer
    {
        ICallContext IScopedRpcServer.CallContext { get; set; }

        /// <summary>
        /// 调用上下文。
        /// </summary>
        protected ICallContext CallContext => ((IScopedRpcServer)this).CallContext;
    }
}
