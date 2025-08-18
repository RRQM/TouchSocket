using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;

namespace RpcLibrary.Shared.RpcServers
{
    [GeneratorRpcProxy]
    public interface IMyRpcServer:IRpcServer
    {
        [Description("登录")]
        [DmtpRpc]
        bool Login(string account, string password);
    }
}
