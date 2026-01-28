/*
此代码由Rpc工具直接生成，非必要请不要修改此处代码
*/
#pragma warning disable
using System;
using TouchSocket.Core;
using TouchSocket.Sockets;
using TouchSocket.Rpc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
namespace UnityRpcProxy_Json_HttpDmtp
{
public interface IUnityRpcStore:TouchSocket.Rpc.IRemoteServer
{
///<summary>
///登录
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<LoginModelResult> JsonRpc_LoginAsync(LoginModel model,InvokeOption invokeOption = default);

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> JsonRpc_PerformanceAsync(System.Int32 i,InvokeOption invokeOption = default);

}
public class UnityRpcStore :IUnityRpcStore
{
public UnityRpcStore(IRpcClient client)
{
this.Client=client;
}
public IRpcClient Client{get;private set; }

///<summary>
///登录
///</summary>
public async Task<LoginModelResult> JsonRpc_LoginAsync(LoginModel model,InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{model};
return (LoginModelResult) await this.Client.InvokeAsync("JsonRpc_Login",typeof(LoginModelResult),invokeOption, parameters);

}

///<summary>
///性能测试
///</summary>
public async Task<System.Int32> JsonRpc_PerformanceAsync(System.Int32 i,InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{i};
return (System.Int32) await this.Client.InvokeAsync("JsonRpc_Performance",typeof(System.Int32),invokeOption, parameters);

}

}
public static class UnityRpcStoreExtensions
{

///<summary>
///登录
///</summary>
public static async Task<LoginModelResult> JsonRpc_LoginAsync<TClient>(this TClient client,LoginModel model,InvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] parameters = new object[]{model};
return (LoginModelResult) await client.InvokeAsync("JsonRpc_Login",typeof(LoginModelResult),invokeOption, parameters);

}

///<summary>
///性能测试
///</summary>
public static async Task<System.Int32> JsonRpc_PerformanceAsync<TClient>(this TClient client,System.Int32 i,InvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] parameters = new object[]{i};
return (System.Int32) await client.InvokeAsync("JsonRpc_Performance",typeof(System.Int32),invokeOption, parameters);

}

}
public class LoginModelResult
{
public TouchSocket.Core.ResultCode ResultCode { get; set; }
public System.String Message { get; set; }
}

public class LoginModel
{
public System.String Token { get; set; }
public System.String Account { get; set; }
public System.String Password { get; set; }
}

}
