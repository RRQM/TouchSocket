using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Rpc
{
    public readonly struct RpcResponse
    {
        public RpcResponse(object returnValue, object[] parameters)
        {
            this.ReturnValue = returnValue;
            this.Parameters = parameters;
        }

        public RpcResponse(object returnValue)
        {
            this.ReturnValue = returnValue;
            this.Parameters = default;
        }

        public object ReturnValue { get;}
        public object[] Parameters { get; }
    }
}
