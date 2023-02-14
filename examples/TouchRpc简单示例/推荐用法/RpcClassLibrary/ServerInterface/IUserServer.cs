using RpcClassLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;
using TouchSocket.Rpc;
using TouchSocket.Rpc.TouchRpc;

namespace RpcClassLibrary.ServerInterface
{
    /// <summary>
    /// 定义服务接口。
    /// </summary>
    [GeneratorRpcProxy(Prefix = "RpcClassLibrary",MethodFlags = MethodFlags.IncludeCallContext)]
    public interface IUserServer:IRpcServer
    {
        [GeneratorRpcMethod]
        [TouchRpc]
        LoginResponse Login(ICallContext callContext,LoginRequest request);
    }
}
