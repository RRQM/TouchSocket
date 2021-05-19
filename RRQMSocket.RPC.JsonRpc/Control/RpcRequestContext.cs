using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.JsonRpc
{
    /// <summary>
    /// JsonRpc调用器
    /// </summary>
    public class RpcRequestContext
    {
#pragma warning disable CS1591 
        public string jsonrpc;
        public string method;
        public object[] @params;
        public string id;
#pragma warning restore CS1591 
    }
}
