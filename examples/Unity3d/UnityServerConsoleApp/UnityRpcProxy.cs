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
namespace UnityRpcProxy
{
public interface IMyRpcServer:TouchSocket.Rpc.IRemoteServer
{
///<summary>
///登录
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
MyLoginModelResult Login(MyLoginModel model,IInvokeOption invokeOption = default);
///<summary>
///登录
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<MyLoginModelResult> LoginAsync(MyLoginModel model,IInvokeOption invokeOption = default);

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 Performance(System.Int32 i,IInvokeOption invokeOption = default);
///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> PerformanceAsync(System.Int32 i,IInvokeOption invokeOption = default);

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
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public MyLoginModelResult Login(MyLoginModel model,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{model};
MyLoginModelResult returnData=(MyLoginModelResult)Client.Invoke(typeof(MyLoginModelResult),"Login",invokeOption, parameters);
return returnData;
}
///<summary>
///登录
///</summary>
public async Task<MyLoginModelResult> LoginAsync(MyLoginModel model,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{model};
return (MyLoginModelResult) await Client.InvokeAsync(typeof(MyLoginModelResult),"Login",invokeOption, parameters);
}

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 Performance(System.Int32 i,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{i};
System.Int32 returnData=(System.Int32)Client.Invoke(typeof(System.Int32),"Performance",invokeOption, parameters);
return returnData;
}
///<summary>
///性能测试
///</summary>
public async Task<System.Int32> PerformanceAsync(System.Int32 i,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{i};
return (System.Int32) await Client.InvokeAsync(typeof(System.Int32),"Performance",invokeOption, parameters);
}

}
public static class MyRpcServerExtensions
{
///<summary>
///登录
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static MyLoginModelResult Login<TClient>(this TClient client,MyLoginModel model,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{model};
MyLoginModelResult returnData=(MyLoginModelResult)client.Invoke(typeof(MyLoginModelResult),"Login",invokeOption, parameters);
return returnData;
}
///<summary>
///登录
///</summary>
public static async Task<MyLoginModelResult> LoginAsync<TClient>(this TClient client,MyLoginModel model,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{model};
return (MyLoginModelResult) await client.InvokeAsync(typeof(MyLoginModelResult),"Login",invokeOption, parameters);
}

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 Performance<TClient>(this TClient client,System.Int32 i,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{i};
System.Int32 returnData=(System.Int32)client.Invoke(typeof(System.Int32),"Performance",invokeOption, parameters);
return returnData;
}
///<summary>
///性能测试
///</summary>
public static async Task<System.Int32> PerformanceAsync<TClient>(this TClient client,System.Int32 i,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{i};
return (System.Int32) await client.InvokeAsync(typeof(System.Int32),"Performance",invokeOption, parameters);
}

}
public class MyLoginModelResult
{
public System.Byte Status { get; set; }
public System.String Message { get; set; }
}

public class MyLoginModel
{
public System.String Token { get; set; }
public System.String Account { get; set; }
public System.String Password { get; set; }
}

}
