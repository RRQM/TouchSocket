//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

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
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] @_parameters = new object[]{str};
System.String returnData=(System.String)this.Client.Invoke("TestGetContext",typeof(System.String),invokeOption, @_parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> TestGetContextAsync(System.String str,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{str};
return (System.String) await this.Client.InvokeAsync("TestGetContext",typeof(System.String),invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public Newtonsoft.Json.Linq.JObject TestJObject(Newtonsoft.Json.Linq.JObject obj,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] @_parameters = new object[]{obj};
Newtonsoft.Json.Linq.JObject returnData=(Newtonsoft.Json.Linq.JObject)this.Client.Invoke("TestJObject",typeof(Newtonsoft.Json.Linq.JObject),invokeOption, @_parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<Newtonsoft.Json.Linq.JObject> TestJObjectAsync(Newtonsoft.Json.Linq.JObject obj,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{obj};
return (Newtonsoft.Json.Linq.JObject) await this.Client.InvokeAsync("TestJObject",typeof(Newtonsoft.Json.Linq.JObject),invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String TestJsonRpc(System.String str,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] @_parameters = new object[]{str};
System.String returnData=(System.String)this.Client.Invoke("TestJsonRpc",typeof(System.String),invokeOption, @_parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> TestJsonRpcAsync(System.String str,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{str};
return (System.String) await this.Client.InvokeAsync("TestJsonRpc",typeof(System.String),invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String Show(System.Int32 a,System.Int32 b,System.Int32 c,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] @_parameters = new object[]{a,b,c};
System.String returnData=(System.String)this.Client.Invoke("Show",typeof(System.String),invokeOption, @_parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> ShowAsync(System.Int32 a,System.Int32 b,System.Int32 c,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{a,b,c};
return (System.String) await this.Client.InvokeAsync("Show",typeof(System.String),invokeOption, parameters);
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
object[] @_parameters = new object[]{str};
System.String returnData=(System.String)client.Invoke("TestGetContext",typeof(System.String),invokeOption, @_parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> TestGetContextAsync<TClient>(this TClient client,System.String str,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] parameters = new object[]{str};
return (System.String) await client.InvokeAsync("TestGetContext",typeof(System.String),invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static Newtonsoft.Json.Linq.JObject TestJObject<TClient>(this TClient client,Newtonsoft.Json.Linq.JObject obj,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] @_parameters = new object[]{obj};
Newtonsoft.Json.Linq.JObject returnData=(Newtonsoft.Json.Linq.JObject)client.Invoke("TestJObject",typeof(Newtonsoft.Json.Linq.JObject),invokeOption, @_parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<Newtonsoft.Json.Linq.JObject> TestJObjectAsync<TClient>(this TClient client,Newtonsoft.Json.Linq.JObject obj,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] parameters = new object[]{obj};
return (Newtonsoft.Json.Linq.JObject) await client.InvokeAsync("TestJObject",typeof(Newtonsoft.Json.Linq.JObject),invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String TestJsonRpc<TClient>(this TClient client,System.String str,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] @_parameters = new object[]{str};
System.String returnData=(System.String)client.Invoke("TestJsonRpc",typeof(System.String),invokeOption, @_parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> TestJsonRpcAsync<TClient>(this TClient client,System.String str,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] parameters = new object[]{str};
return (System.String) await client.InvokeAsync("TestJsonRpc",typeof(System.String),invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String Show<TClient>(this TClient client,System.Int32 a,System.Int32 b,System.Int32 c,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] @_parameters = new object[]{a,b,c};
System.String returnData=(System.String)client.Invoke("Show",typeof(System.String),invokeOption, @_parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> ShowAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,System.Int32 c,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] parameters = new object[]{a,b,c};
return (System.String) await client.InvokeAsync("Show",typeof(System.String),invokeOption, parameters);
}

}
}
