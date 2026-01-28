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
Task<System.String> TestGetContextAsync(System.String str,InvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> TestJsonRpcAsync(System.String str,InvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> ShowAsync(System.Int32 a,System.Int32 b,System.Int32 c,InvokeOption invokeOption = default);

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
public async Task<System.String> TestGetContextAsync(System.String str,InvokeOption invokeOption = default)
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
public async Task<System.String> TestJsonRpcAsync(System.String str,InvokeOption invokeOption = default)
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
public async Task<System.String> ShowAsync(System.Int32 a,System.Int32 b,System.Int32 c,InvokeOption invokeOption = default)
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
public static async Task<System.String> TestGetContextAsync<TClient>(this TClient client,System.String str,InvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] parameters = new object[]{str};
return (System.String) await client.InvokeAsync("TestGetContext",typeof(System.String),invokeOption, parameters);

}

///<summary>
///无注释信息
///</summary>
public static async Task<System.String> TestJsonRpcAsync<TClient>(this TClient client,System.String str,InvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] parameters = new object[]{str};
return (System.String) await client.InvokeAsync("TestJsonRpc",typeof(System.String),invokeOption, parameters);

}

///<summary>
///无注释信息
///</summary>
public static async Task<System.String> ShowAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,System.Int32 c,InvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] parameters = new object[]{a,b,c};
return (System.String) await client.InvokeAsync("Show",typeof(System.String),invokeOption, parameters);

}

}
}
