using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// TcpJsonRpcCallContext
    /// </summary>
    internal class TcpJsonRpcCallContext : JsonRpcCallContextBase, ITcpJsonRpcCallContext
    {
        /// <summary>
        /// TcpJsonRpcCallContext
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="jsonString"></param>
        public TcpJsonRpcCallContext(object caller, string jsonString) : base(caller, jsonString)
        {
        }
    }
}
