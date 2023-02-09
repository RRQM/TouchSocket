using System;
using TouchSocket.Core;
using TouchSocket.Sockets;
using TouchSocket.Rpc;
using TouchSocket.Rpc.TouchRpc;
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
System.Boolean Login(System.String account,System.String password,IInvokeOption invokeOption = default);
///<summary>
///登录
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Boolean> LoginAsync(System.String account,System.String password,IInvokeOption invokeOption = default);

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 Performance(System.Int32 a,IInvokeOption invokeOption = default);
///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> PerformanceAsync(System.Int32 a,IInvokeOption invokeOption = default);

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
public System.Boolean Login(System.String account,System.String password,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{account,password};
System.Boolean returnData=Client.Invoke<System.Boolean>("touchrpcserverapp.myrpcserver.login",invokeOption, parameters);
return returnData;
}
///<summary>
///登录
///</summary>
public Task<System.Boolean> LoginAsync(System.String account,System.String password,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{account,password};
return Client.InvokeAsync<System.Boolean>("touchrpcserverapp.myrpcserver.login",invokeOption, parameters);
}

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 Performance(System.Int32 a,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a};
System.Int32 returnData=Client.Invoke<System.Int32>("touchrpcserverapp.myrpcserver.performance",invokeOption, parameters);
return returnData;
}
///<summary>
///性能测试
///</summary>
public Task<System.Int32> PerformanceAsync(System.Int32 a,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a};
return Client.InvokeAsync<System.Int32>("touchrpcserverapp.myrpcserver.performance",invokeOption, parameters);
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
public static System.Boolean Login<TClient>(this TClient client,System.String account,System.String password,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{account,password};
System.Boolean returnData=client.Invoke<System.Boolean>("touchrpcserverapp.myrpcserver.login",invokeOption, parameters);
return returnData;
}
///<summary>
///登录
///</summary>
public static Task<System.Boolean> LoginAsync<TClient>(this TClient client,System.String account,System.String password,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{account,password};
return client.InvokeAsync<System.Boolean>("touchrpcserverapp.myrpcserver.login",invokeOption, parameters);
}

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 Performance<TClient>(this TClient client,System.Int32 a,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a};
System.Int32 returnData=client.Invoke<System.Int32>("touchrpcserverapp.myrpcserver.performance",invokeOption, parameters);
return returnData;
}
///<summary>
///性能测试
///</summary>
public static Task<System.Int32> PerformanceAsync<TClient>(this TClient client,System.Int32 a,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{a};
return client.InvokeAsync<System.Int32>("touchrpcserverapp.myrpcserver.performance",invokeOption, parameters);
}

}
}
