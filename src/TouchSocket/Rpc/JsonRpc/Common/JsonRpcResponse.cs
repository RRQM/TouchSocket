using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Rpc.JsonRpc
{
    /// <summary>
    /// JsonRpcSuccessResponse
    /// </summary>
    public class JsonRpcSuccessResponse
    {
        /// <summary>
        /// jsonrpc
        /// </summary>
        public string jsonrpc = "2.0";

        /// <summary>
        /// result
        /// </summary>
        public object result;

        /// <summary>
        /// id
        /// </summary>
        public string id;
    }

    /// <summary>
    /// JsonRpcErrorResponse
    /// </summary>
    public class JsonRpcErrorResponse
    {
        /// <summary>
        /// jsonrpc
        /// </summary>
        public string jsonrpc = "2.0";

        /// <summary>
        /// error
        /// </summary>
        public error error;

        /// <summary>
        /// id
        /// </summary>
        public string id;
    }
}
