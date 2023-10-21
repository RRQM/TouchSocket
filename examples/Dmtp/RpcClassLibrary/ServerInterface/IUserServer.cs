using RpcClassLibrary.Models;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;

namespace RpcClassLibrary.ServerInterface
{
    /// <summary>
    /// 定义服务接口。
    /// </summary>
    [GeneratorRpcProxy]
    public interface IUserServer : IRpcServer
    {
        [GeneratorRpcMethod]
        [DmtpRpc]
        LoginResponse Login(ICallContext callContext, LoginRequest request);
    }
}