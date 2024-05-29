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
public interface IXmlServer:TouchSocket.Rpc.IRemoteServer
{
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 Sum(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> SumAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 TestClass(MyClass myClass,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> TestClassAsync(MyClass myClass,IInvokeOption invokeOption = default);

}
public class XmlServer :IXmlServer
{
public XmlServer(IRpcClient client)
{
this.Client=client;
}
public IRpcClient Client{get;private set; }
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 Sum(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var @_request = new RpcRequest(
"Sum",typeof(System.Int32),invokeOption,
new object[]{a,b},
default);
var @_response = Client.Invoke(@_request);
return (System.Int32)@_response.ReturnValue;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> SumAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var @_request = new RpcRequest(
"Sum",typeof(System.Int32),invokeOption,
new object[]{a,b},
default);
var @_response =await Client.InvokeAsync(@_request).ConfigureFalseAwait();
return (System.Int32)@_response.ReturnValue;
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 TestClass(MyClass myClass,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var @_request = new RpcRequest(
"TestClass",typeof(System.Int32),invokeOption,
new object[]{myClass},
default);
var @_response = Client.Invoke(@_request);
return (System.Int32)@_response.ReturnValue;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> TestClassAsync(MyClass myClass,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var @_request = new RpcRequest(
"TestClass",typeof(System.Int32),invokeOption,
new object[]{myClass},
default);
var @_response =await Client.InvokeAsync(@_request).ConfigureFalseAwait();
return (System.Int32)@_response.ReturnValue;
}

}
public static class XmlServerExtensions
{
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 Sum<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.XmlRpc.IXmlRpcClient{
var @_request = new RpcRequest(
"Sum",typeof(System.Int32),invokeOption,
new object[]{a,b},
default);
var @_response = client.Invoke(@_request);
return (System.Int32)@_response.ReturnValue;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> SumAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.XmlRpc.IXmlRpcClient{
var @_request = new RpcRequest(
"Sum",typeof(System.Int32),invokeOption,
new object[]{a,b},
default);
var @_response =await client.InvokeAsync(@_request).ConfigureFalseAwait();
return (System.Int32)@_response.ReturnValue;
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 TestClass<TClient>(this TClient client,MyClass myClass,IInvokeOption invokeOption = default) where TClient:
TouchSocket.XmlRpc.IXmlRpcClient{
var @_request = new RpcRequest(
"TestClass",typeof(System.Int32),invokeOption,
new object[]{myClass},
default);
var @_response = client.Invoke(@_request);
return (System.Int32)@_response.ReturnValue;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> TestClassAsync<TClient>(this TClient client,MyClass myClass,IInvokeOption invokeOption = default) where TClient:
TouchSocket.XmlRpc.IXmlRpcClient{
var @_request = new RpcRequest(
"TestClass",typeof(System.Int32),invokeOption,
new object[]{myClass},
default);
var @_response =await client.InvokeAsync(@_request).ConfigureFalseAwait();
return (System.Int32)@_response.ReturnValue;
}

}
public class MyClass
{
public System.Int32 A { get; set; }
public System.Int32 B { get; set; }
}

}
