using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace TouchSocket.Rpc.JsonRpc
{
    /// <summary>
    /// IJsonRpcCallContext
    /// </summary>
    public interface IJsonRpcCallContext:ICallContext
    {
        /// <summary>
        /// Json字符串
        /// </summary>
        public string JsonString { get; }

        /// <summary>
        /// JsonRpc数据包
        /// </summary>
        public JsonRpcPackage JsonRpcPackage { get; }

        /// <summary>
        /// 当使用Http请求时，表明Http上下文。
        /// </summary>
        HttpContext HttpContext { get; }

        /// <summary>
        /// 表明当前的调用协议。
        /// </summary>
        JRPT JRPT { get; }
    }
}
