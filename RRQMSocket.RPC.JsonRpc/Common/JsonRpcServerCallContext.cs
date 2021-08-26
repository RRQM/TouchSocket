using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.JsonRpc
{
    /// <summary>
    /// JsonRpc调用上下文
    /// </summary>
    public class JsonRpcServerCallContext : IServerCallContext
    {
        internal ICaller caller;
        internal JsonRpcContext context;
        internal MethodInstance methodInstance;
        internal string jsonString;
        internal JsonRpcProtocolType protocolType;
        internal MethodInvoker methodInvoker;

        /// <summary>
        /// Json字符串
        /// </summary>
        public string JsonString
        {
            get { return jsonString; }
        }


        /// <summary>
        /// 协议类型
        /// </summary>
        public JsonRpcProtocolType ProtocolType
        {
            get { return protocolType; }
            set { protocolType = value; }
        }



#pragma warning disable CS1591 
        public ICaller Caller => this.caller;

        public IRpcContext Context => this.context;

        public MethodInstance MethodInstance => this.methodInstance;

        public MethodInvoker MethodInvoker => this.methodInvoker;
    }
}
