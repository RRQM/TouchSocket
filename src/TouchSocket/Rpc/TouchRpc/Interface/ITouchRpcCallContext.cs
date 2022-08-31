using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core.Serialization;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// ITouchRpcCallContext
    /// </summary>
    public interface ITouchRpcCallContext : ICallContext
    {
        /// <summary>
        /// 当<see cref="ICallContext.TokenSource"/>不为空时，调用<see cref="CancellationTokenSource.Cancel()"/>
        /// </summary>
        /// <returns></returns>
        public bool TryCancel();

        /// <summary>
        /// TouchRpcContext
        /// </summary>
        TouchRpcPackage TouchRpcPackage { get;}

        /// <summary>
        /// 序列化类型
        /// </summary>
        public SerializationType SerializationType { get; }
    }
}
