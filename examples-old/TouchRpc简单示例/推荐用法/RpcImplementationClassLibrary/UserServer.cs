using RpcClassLibrary.Models;
using RpcClassLibrary.ServerInterface;
using System;
using TouchSocket.Core;
using TouchSocket.Rpc;

namespace RpcImplementationClassLibrary
{
    public class UserServer : IUserServer
    {
        public LoginResponse Login(ICallContext callContext, LoginRequest request)
        {
            //返回假逻辑
            return new LoginResponse() { Result = Result.Success };
        }
    }
}
