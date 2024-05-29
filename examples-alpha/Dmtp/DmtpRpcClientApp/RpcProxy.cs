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
System.Boolean Login(System.String account,System.String password,IInvokeOption invokeOption = default);
///<summary>
///登录
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Boolean> LoginAsync(System.String account,System.String password,IInvokeOption invokeOption = default);

///<summary>
///注册
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Boolean Register(RegisterModel register,IInvokeOption invokeOption = default);
///<summary>
///注册
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Boolean> RegisterAsync(RegisterModel register,IInvokeOption invokeOption = default);

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

///<summary>
///测试out
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Boolean OutBytes(out System.Byte[] bytes,IInvokeOption invokeOption = default);
///<summary>
///测试out
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Boolean> OutBytesAsync(out System.Byte[] bytes,IInvokeOption invokeOption = default);

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
var @_request = new RpcRequest(
"Login",typeof(System.Boolean),invokeOption,
new object[]{account,password},
default);
var @_response = Client.Invoke(@_request);
return (System.Boolean)@_response.ReturnValue;
}
///<summary>
///登录
///</summary>
public async Task<System.Boolean> LoginAsync(System.String account,System.String password,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var @_request = new RpcRequest(
"Login",typeof(System.Boolean),invokeOption,
new object[]{account,password},
default);
var @_response =await Client.InvokeAsync(@_request).ConfigureFalseAwait();
return (System.Boolean)@_response.ReturnValue;
}

///<summary>
///注册
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Boolean Register(RegisterModel register,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var @_request = new RpcRequest(
"Register",typeof(System.Boolean),invokeOption,
new object[]{register},
default);
var @_response = Client.Invoke(@_request);
return (System.Boolean)@_response.ReturnValue;
}
///<summary>
///注册
///</summary>
public async Task<System.Boolean> RegisterAsync(RegisterModel register,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var @_request = new RpcRequest(
"Register",typeof(System.Boolean),invokeOption,
new object[]{register},
default);
var @_response =await Client.InvokeAsync(@_request).ConfigureFalseAwait();
return (System.Boolean)@_response.ReturnValue;
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
var @_request = new RpcRequest(
"Performance",typeof(System.Int32),invokeOption,
new object[]{a},
default);
var @_response = Client.Invoke(@_request);
return (System.Int32)@_response.ReturnValue;
}
///<summary>
///性能测试
///</summary>
public async Task<System.Int32> PerformanceAsync(System.Int32 a,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var @_request = new RpcRequest(
"Performance",typeof(System.Int32),invokeOption,
new object[]{a},
default);
var @_response =await Client.InvokeAsync(@_request).ConfigureFalseAwait();
return (System.Int32)@_response.ReturnValue;
}

///<summary>
///测试out
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Boolean OutBytes(out System.Byte[] bytes,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var @_request = new RpcRequest(
"OutBytes",typeof(System.Boolean),invokeOption,
new object[]{default(System.Byte[])},
new Type[]{typeof(System.Byte[])}
);
var @_response = Client.Invoke(@_request);
var parameters = _response.Parameters;
if(parameters!=null)
{
bytes=(System.Byte[])parameters[0];
}
else
{
bytes=default(System.Byte[]);
}
return (System.Boolean)@_response.ReturnValue;
}
///<summary>
///测试out
///</summary>
public Task<System.Boolean> OutBytesAsync(out System.Byte[] bytes,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var @_request = new RpcRequest(
"OutBytes",typeof(System.Boolean),invokeOption,
new object[]{default(System.Byte[])},
new Type[]{typeof(System.Byte[])}
);
var @_response = Client.Invoke(@_request);
var parameters = _response.Parameters;
if(parameters!=null)
{
bytes=(System.Byte[])parameters[0];
}
else
{
bytes=default(System.Byte[]);
}
return Task.FromResult<System.Boolean>((System.Boolean)@_response.ReturnValue);
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
var @_request = new RpcRequest(
"Login",typeof(System.Boolean),invokeOption,
new object[]{account,password},
default);
var @_response = client.Invoke(@_request);
return (System.Boolean)@_response.ReturnValue;
}
///<summary>
///登录
///</summary>
public static async Task<System.Boolean> LoginAsync<TClient>(this TClient client,System.String account,System.String password,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
var @_request = new RpcRequest(
"Login",typeof(System.Boolean),invokeOption,
new object[]{account,password},
default);
var @_response =await client.InvokeAsync(@_request).ConfigureFalseAwait();
return (System.Boolean)@_response.ReturnValue;
}

///<summary>
///注册
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Boolean Register<TClient>(this TClient client,RegisterModel register,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
var @_request = new RpcRequest(
"Register",typeof(System.Boolean),invokeOption,
new object[]{register},
default);
var @_response = client.Invoke(@_request);
return (System.Boolean)@_response.ReturnValue;
}
///<summary>
///注册
///</summary>
public static async Task<System.Boolean> RegisterAsync<TClient>(this TClient client,RegisterModel register,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
var @_request = new RpcRequest(
"Register",typeof(System.Boolean),invokeOption,
new object[]{register},
default);
var @_response =await client.InvokeAsync(@_request).ConfigureFalseAwait();
return (System.Boolean)@_response.ReturnValue;
}

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 Performance<TClient>(this TClient client,System.Int32 a,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
var @_request = new RpcRequest(
"Performance",typeof(System.Int32),invokeOption,
new object[]{a},
default);
var @_response = client.Invoke(@_request);
return (System.Int32)@_response.ReturnValue;
}
///<summary>
///性能测试
///</summary>
public static async Task<System.Int32> PerformanceAsync<TClient>(this TClient client,System.Int32 a,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
var @_request = new RpcRequest(
"Performance",typeof(System.Int32),invokeOption,
new object[]{a},
default);
var @_response =await client.InvokeAsync(@_request).ConfigureFalseAwait();
return (System.Int32)@_response.ReturnValue;
}

///<summary>
///测试out
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Boolean OutBytes<TClient>(this TClient client,out System.Byte[] bytes,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
var @_request = new RpcRequest(
"OutBytes",typeof(System.Boolean),invokeOption,
new object[]{default(System.Byte[])},
new Type[]{typeof(System.Byte[])}
);
var @_response = client.Invoke(@_request);
var parameters = _response.Parameters;
if(parameters!=null)
{
bytes=(System.Byte[])parameters[0];
}
else
{
bytes=default(System.Byte[]);
}
return (System.Boolean)@_response.ReturnValue;
}
///<summary>
///测试out
///</summary>
public static Task<System.Boolean> OutBytesAsync<TClient>(this TClient client,out System.Byte[] bytes,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
var @_request = new RpcRequest(
"OutBytes",typeof(System.Boolean),invokeOption,
new object[]{default(System.Byte[])},
new Type[]{typeof(System.Byte[])}
);
var @_response = client.Invoke(@_request);
var parameters = _response.Parameters;
if(parameters!=null)
{
bytes=(System.Byte[])parameters[0];
}
else
{
bytes=default(System.Byte[]);
}
return Task.FromResult<System.Boolean>((System.Boolean)@_response.ReturnValue);
}

}
public class RegisterModel
{
public System.String Account { get; set; }
public System.String Password { get; set; }
public System.Int32 Id { get; set; }
}

}
