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
namespace UnityRpcProxy_Json_HttpDmtp_2D
{
public interface IUnityRpcStore:TouchSocket.Rpc.IRemoteServer
{
///<summary>
///单位移动
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void JsonRpc_UnitMovement(System.Numerics.Vector3 vector3,IInvokeOption invokeOption = default);
///<summary>
///单位移动
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task JsonRpc_UnitMovementAsync(System.Numerics.Vector3 vector3,IInvokeOption invokeOption = default);

}
public class UnityRpcStore :IUnityRpcStore
{
public UnityRpcStore(IRpcClient client)
{
this.Client=client;
}
public IRpcClient Client{get;private set; }
///<summary>
///单位移动
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void JsonRpc_UnitMovement(System.Numerics.Vector3 vector3,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] @_parameters = new object[]{vector3};
this.Client.Invoke("JsonRpc_UnitMovement",null,invokeOption, @_parameters);

}
///<summary>
///单位移动
///</summary>
public Task JsonRpc_UnitMovementAsync(System.Numerics.Vector3 vector3,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{vector3};
return this.Client.InvokeAsync("JsonRpc_UnitMovement",null,invokeOption, parameters);

}

}
public static class UnityRpcStoreExtensions
{
///<summary>
///单位移动
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void JsonRpc_UnitMovement<TClient>(this TClient client,System.Numerics.Vector3 vector3,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] @_parameters = new object[]{vector3};
client.Invoke("JsonRpc_UnitMovement",null,invokeOption, @_parameters);

}
///<summary>
///单位移动
///</summary>
public static Task JsonRpc_UnitMovementAsync<TClient>(this TClient client,System.Numerics.Vector3 vector3,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] parameters = new object[]{vector3};
return client.InvokeAsync("JsonRpc_UnitMovement",null,invokeOption, parameters);

}

}
}
