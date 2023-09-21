﻿using TouchSocket.Http;
using TouchSocket.WebApi;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// HttpJsonRpcCallContext
    /// </summary>
    internal class HttpJsonRpcCallContext : JsonRpcCallContextBase, IHttpCallContext
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