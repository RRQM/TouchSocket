using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Rpc
{
    public readonly struct RpcRequest
    {
        public RpcRequest(string invokeKey, Type returnType, IInvokeOption invokeOption, object[] parameters, Type[] parameterTypes)
        {
            this.InvokeKey = invokeKey;
            this.InvokeOption = invokeOption;
            this.Parameters = parameters;
            this.ParameterTypes = parameterTypes;
            this.ReturnType = returnType;
        }

        public string InvokeKey { get; }
        public Type ReturnType { get; }
        public IInvokeOption InvokeOption { get; }
        public object[] Parameters { get; }
        public Type[] ParameterTypes { get; }
        public bool HasReturn => this.ReturnType != null;
    }
}
