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
namespace JsonRpcProxy
{
public interface IJsonRpcServer:TouchSocket.Rpc.IRemoteServer
{
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.String TestGetContext(System.String str,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> TestGetContextAsync(System.String str,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Newtonsoft.Json.Linq.JObject TestJObject(Newtonsoft.Json.Linq.JObject obj,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<Newtonsoft.Json.Linq.JObject> TestJObjectAsync(Newtonsoft.Json.Linq.JObject obj,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.String TestJsonRpc(System.String str,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> TestJsonRpcAsync(System.String str,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.String Show(System.Int32 a,System.Int32 b,System.Int32 c,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> ShowAsync(System.Int32 a,System.Int32 b,System.Int32 c,IInvokeOption invokeOption = default);

}
public class JsonRpcServer :IJsonRpcServer
{
public JsonRpcServer(IRpcClient client)
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
public System.String TestGetContext(System.String str,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var @_request = new RpcRequest(
"TestGetContext",typeof(System.String),invokeOption,
new object[]{str},
default);
var @_response = Client.Invoke(@_request);
return (System.String)@_response.ReturnValue;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> TestGetContextAsync(System.String str,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var @_request = new RpcRequest(
"TestGetContext",typeof(System.String),invokeOption,
new object[]{str},
default);
var @_response =await Client.InvokeAsync(@_request).ConfigureFalseAwait();
return (System.String)@_response.ReturnValue;
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public Newtonsoft.Json.Linq.JObject TestJObject(Newtonsoft.Json.Linq.JObject obj,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var @_request = new RpcRequest(
"TestJObject",typeof(Newtonsoft.Json.Linq.JObject),invokeOption,
new object[]{obj},
default);
var @_response = Client.Invoke(@_request);
return (Newtonsoft.Json.Linq.JObject)@_response.ReturnValue;
}
///<summary>
///无注释信息
///</summary>
public async Task<Newtonsoft.Json.Linq.JObject> TestJObjectAsync(Newtonsoft.Json.Linq.JObject obj,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var @_request = new RpcRequest(
"TestJObject",typeof(Newtonsoft.Json.Linq.JObject),invokeOption,
new object[]{obj},
default);
var @_response =await Client.InvokeAsync(@_request).ConfigureFalseAwait();
return (Newtonsoft.Json.Linq.JObject)@_response.ReturnValue;
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String TestJsonRpc(System.String str,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var @_request = new RpcRequest(
"TestJsonRpc",typeof(System.String),invokeOption,
new object[]{str},
default);
var @_response = Client.Invoke(@_request);
return (System.String)@_response.ReturnValue;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> TestJsonRpcAsync(System.String str,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var @_request = new RpcRequest(
"TestJsonRpc",typeof(System.String),invokeOption,
new object[]{str},
default);
var @_response =await Client.InvokeAsync(@_request).ConfigureFalseAwait();
return (System.String)@_response.ReturnValue;
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String Show(System.Int32 a,System.Int32 b,System.Int32 c,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var @_request = new RpcRequest(
"Show",typeof(System.String),invokeOption,
new object[]{a,b,c},
default);
var @_response = Client.Invoke(@_request);
return (System.String)@_response.ReturnValue;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> ShowAsync(System.Int32 a,System.Int32 b,System.Int32 c,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var @_request = new RpcRequest(
"Show",typeof(System.String),invokeOption,
new object[]{a,b,c},
default);
var @_response =await Client.InvokeAsync(@_request).ConfigureFalseAwait();
return (System.String)@_response.ReturnValue;
}

}
public static class JsonRpcServerExtensions
{
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String TestGetContext<TClient>(this TClient client,System.String str,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
var @_request = new RpcRequest(
"TestGetContext",typeof(System.String),invokeOption,
new object[]{str},
default);
var @_response = client.Invoke(@_request);
return (System.String)@_response.ReturnValue;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> TestGetContextAsync<TClient>(this TClient client,System.String str,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
var @_request = new RpcRequest(
"TestGetContext",typeof(System.String),invokeOption,
new object[]{str},
default);
var @_response =await client.InvokeAsync(@_request).ConfigureFalseAwait();
return (System.String)@_response.ReturnValue;
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static Newtonsoft.Json.Linq.JObject TestJObject<TClient>(this TClient client,Newtonsoft.Json.Linq.JObject obj,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
var @_request = new RpcRequest(
"TestJObject",typeof(Newtonsoft.Json.Linq.JObject),invokeOption,
new object[]{obj},
default);
var @_response = client.Invoke(@_request);
return (Newtonsoft.Json.Linq.JObject)@_response.ReturnValue;
}
///<summary>
///无注释信息
///</summary>
public static async Task<Newtonsoft.Json.Linq.JObject> TestJObjectAsync<TClient>(this TClient client,Newtonsoft.Json.Linq.JObject obj,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
var @_request = new RpcRequest(
"TestJObject",typeof(Newtonsoft.Json.Linq.JObject),invokeOption,
new object[]{obj},
default);
var @_response =await client.InvokeAsync(@_request).ConfigureFalseAwait();
return (Newtonsoft.Json.Linq.JObject)@_response.ReturnValue;
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String TestJsonRpc<TClient>(this TClient client,System.String str,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
var @_request = new RpcRequest(
"TestJsonRpc",typeof(System.String),invokeOption,
new object[]{str},
default);
var @_response = client.Invoke(@_request);
return (System.String)@_response.ReturnValue;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> TestJsonRpcAsync<TClient>(this TClient client,System.String str,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
var @_request = new RpcRequest(
"TestJsonRpc",typeof(System.String),invokeOption,
new object[]{str},
default);
var @_response =await client.InvokeAsync(@_request).ConfigureFalseAwait();
return (System.String)@_response.ReturnValue;
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String Show<TClient>(this TClient client,System.Int32 a,System.Int32 b,System.Int32 c,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
var @_request = new RpcRequest(
"Show",typeof(System.String),invokeOption,
new object[]{a,b,c},
default);
var @_response = client.Invoke(@_request);
return (System.String)@_response.ReturnValue;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> ShowAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,System.Int32 c,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
var @_request = new RpcRequest(
"Show",typeof(System.String),invokeOption,
new object[]{a,b,c},
default);
var @_response =await client.InvokeAsync(@_request).ConfigureFalseAwait();
return (System.String)@_response.ReturnValue;
}

}
}
