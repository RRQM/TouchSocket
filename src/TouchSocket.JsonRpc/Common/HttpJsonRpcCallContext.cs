using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Http;
using TouchSocket.WebApi;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// HttpJsonRpcCallContext
    /// </summary>
    class HttpJsonRpcCallContext : JsonRpcCallContextBase, IHttpCallContext
    {
        /// <summary>
        /// HttpJsonRpcCallContext
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="jsonString"></param>
        /// <param name="context"></param>
        public HttpJsonRpcCallContext(object caller, string jsonString, HttpContext context) : base(caller, jsonString)
        {
            this.HttpContext = context;
        }

        /// <inheritdoc/>
        public HttpContext HttpContext { get; }
    }
}
