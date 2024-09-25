using RpcLibrary.Shared.RpcServers;
using System.ComponentModel;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;

namespace WebServerApplication.RpcServers
{
    public partial class MyRpcServer : RpcServer, IMyRpcServer
    {

        public bool Login(string account, string password)
        {
            if (account == "123" && password == "abc")
            {
                return true;
            }

            return false;
        }
    }
}
