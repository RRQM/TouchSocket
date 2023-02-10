using RpcClassLibrary.Models;
using RpcClassLibrary.ServerInterface;
using System;
using TouchSocket.Core;

namespace RpcImplementationClassLibrary
{
    public class UserServer : IUserServer
    {
        public LoginResponse Login(LoginRequest request)
        {
            //返回假逻辑
            return new LoginResponse() {  Result=Result.Success};
        }
    }
}
