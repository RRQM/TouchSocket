using System;
using TouchSocket.Core;
using TouchSocket.Sockets;
using TouchSocket.Rpc;
using TouchSocket.Rpc.TouchRpc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
namespace JsonRpcProxy
{
public interface IServer:IRemoteServer
{
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
System.String TestJsonRpc1(System.String str,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> TestJsonRpc1Async(System.String str,IInvokeOption invokeOption = default);

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
TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject TestJObject(TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject obj,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject> TestJObjectAsync(TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject obj,IInvokeOption invokeOption = default);

}
public class Server :IServer
{
public Server(IRpcClient client)
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
public System.String TestJsonRpc(System.String str,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{str};
System.String returnData=Client.Invoke<System.String>("jsonrpcconsoleapp.server.testjsonrpc",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.String> TestJsonRpcAsync(System.String str,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{str};
return Client.InvokeAsync<System.String>("jsonrpcconsoleapp.server.testjsonrpc",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String TestJsonRpc1(System.String str,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{str};
System.String returnData=Client.Invoke<System.String>("TestJsonRpc1",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.String> TestJsonRpc1Async(System.String str,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{str};
return Client.InvokeAsync<System.String>("TestJsonRpc1",invokeOption, parameters);
}

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
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{str};
System.String returnData=Client.Invoke<System.String>("jsonrpcconsoleapp.server.testgetcontext",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<System.String> TestGetContextAsync(System.String str,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{str};
return Client.InvokeAsync<System.String>("jsonrpcconsoleapp.server.testgetcontext",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject TestJObject(TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject obj,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{obj};
TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject returnData=Client.Invoke<TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject>("jsonrpcconsoleapp.server.testjobject",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public Task<TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject> TestJObjectAsync(TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject obj,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
if (Client.TryCanInvoke?.Invoke(Client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{obj};
return Client.InvokeAsync<TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject>("jsonrpcconsoleapp.server.testjobject",invokeOption, parameters);
}

}
public static class ServerExtensions
{
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String TestJsonRpc<TClient>(this TClient client,System.String str,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.JsonRpc.IJsonRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{str};
System.String returnData=client.Invoke<System.String>("jsonrpcconsoleapp.server.testjsonrpc",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.String> TestJsonRpcAsync<TClient>(this TClient client,System.String str,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.JsonRpc.IJsonRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{str};
return client.InvokeAsync<System.String>("jsonrpcconsoleapp.server.testjsonrpc",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String TestJsonRpc1<TClient>(this TClient client,System.String str,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.JsonRpc.IJsonRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{str};
System.String returnData=client.Invoke<System.String>("TestJsonRpc1",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.String> TestJsonRpc1Async<TClient>(this TClient client,System.String str,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.JsonRpc.IJsonRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{str};
return client.InvokeAsync<System.String>("TestJsonRpc1",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String TestGetContext<TClient>(this TClient client,System.String str,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.JsonRpc.IJsonRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{str};
System.String returnData=client.Invoke<System.String>("jsonrpcconsoleapp.server.testgetcontext",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<System.String> TestGetContextAsync<TClient>(this TClient client,System.String str,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.JsonRpc.IJsonRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{str};
return client.InvokeAsync<System.String>("jsonrpcconsoleapp.server.testgetcontext",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject TestJObject<TClient>(this TClient client,TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject obj,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.JsonRpc.IJsonRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{obj};
TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject returnData=client.Invoke<TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject>("jsonrpcconsoleapp.server.testjobject",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static Task<TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject> TestJObjectAsync<TClient>(this TClient client,TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject obj,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.JsonRpc.IJsonRpcClient{
if (client.TryCanInvoke?.Invoke(client)==false)
{
throw new RpcException("Rpc无法执行。");
}
object[] parameters = new object[]{obj};
return client.InvokeAsync<TouchSocket.Core.XREF.Newtonsoft.Json.Linq.JObject>("jsonrpcconsoleapp.server.testjobject",invokeOption, parameters);
}

}
}
