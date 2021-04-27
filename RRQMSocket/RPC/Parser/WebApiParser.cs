using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    public class WebApiParser : IRPCParser
    {
        public Func<string, IRPCParser, RPCProxyInfo> GetProxyInfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Func<IRPCParser, List<MethodItem>> InitMethodServer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public SerializeConverter SerializeConverter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event Action<IRPCParser, RPCContext> InvokeMethod;

        public void EndInvokeMethod(RPCContext context)
        {
            throw new NotImplementedException();
        }
    }
}
