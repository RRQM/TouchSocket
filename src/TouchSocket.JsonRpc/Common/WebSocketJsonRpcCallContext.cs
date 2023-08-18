using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.JsonRpc
{
    internal class WebSocketJsonRpcCallContext : JsonRpcCallContextBase
    {
        public WebSocketJsonRpcCallContext(object caller, string jsonString) : base(caller, jsonString)
        {
        }
    }
}
