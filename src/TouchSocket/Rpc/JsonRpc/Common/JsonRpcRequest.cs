using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Rpc.JsonRpc
{
    /// <summary>
    /// JsonRpcRequest
    /// </summary>
    public class JsonRpcRequest
    {
        /// <summary>
        /// jsonrpc
        /// </summary>
        public string jsonrpc="2.0";

        /// <summary>
        /// method
        /// </summary>
        public string method;

        /// <summary>
        /// @params
        /// </summary>
        public object @params;

        /// <summary>
        /// id
        /// </summary>
        public string id;
    }
}
