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
namespace RpcProxy
{
public interface IMyRpcServer:TouchSocket.Rpc.IRemoteServer
{
///<summary>
///登录
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Boolean> LoginAsync(System.String account,System.String password,InvokeOption invokeOption = default);

///<summary>
///注册
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Boolean> RegisterAsync(RegisterModel register,InvokeOption invokeOption = default);

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> PerformanceAsync(System.Int32 a,InvokeOption invokeOption = default);

}
public class MyRpcServer :IMyRpcServer
{
public MyRpcServer(IRpcClient client)
{
this.Client=client;
}
public IRpcClient Client{get;private set; }
///<summary>
///登录
///</summary>
public async Task<System.Boolean> LoginAsync(System.String account,System.String password,InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{account,password};
return (System.Boolean) await this.Client.InvokeAsync("Login",typeof(System.Boolean),invokeOption, parameters);

}

///<summary>
///注册
///</summary>
public async Task<System.Boolean> RegisterAsync(RegisterModel register,InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{register};
return (System.Boolean) await this.Client.InvokeAsync("Register",typeof(System.Boolean),invokeOption, parameters);

}

///<summary>
///性能测试
///</summary>
public async Task<System.Int32> PerformanceAsync(System.Int32 a,InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{a};
return (System.Int32) await this.Client.InvokeAsync("Performance",typeof(System.Int32),invokeOption, parameters);

}

}
public static class MyRpcServerExtensions
{
///<summary>
///登录
///</summary>
public static async Task<System.Boolean> LoginAsync<TClient>(this TClient client,System.String account,System.String password,InvokeOption invokeOption = default) where TClient:
TouchSocket.Dmtp.Rpc.IDmtpRpcActor{
object[] parameters = new object[]{account,password};
return (System.Boolean) await client.InvokeAsync("Login",typeof(System.Boolean),invokeOption, parameters);

}

///<summary>
///注册
///</summary>
public static async Task<System.Boolean> RegisterAsync<TClient>(this TClient client,RegisterModel register,InvokeOption invokeOption = default) where TClient:
TouchSocket.Dmtp.Rpc.IDmtpRpcActor{
object[] parameters = new object[]{register};
return (System.Boolean) await client.InvokeAsync("Register",typeof(System.Boolean),invokeOption, parameters);

}

///<summary>
///性能测试
///</summary>
public static async Task<System.Int32> PerformanceAsync<TClient>(this TClient client,System.Int32 a,InvokeOption invokeOption = default) where TClient:
TouchSocket.Dmtp.Rpc.IDmtpRpcActor{
object[] parameters = new object[]{a};
return (System.Int32) await client.InvokeAsync("Performance",typeof(System.Int32),invokeOption, parameters);

}

}
public class RegisterModel
{
public System.String Account { get; set; }
public System.String Password { get; set; }
public System.Int32 Id { get; set; }
}

}
