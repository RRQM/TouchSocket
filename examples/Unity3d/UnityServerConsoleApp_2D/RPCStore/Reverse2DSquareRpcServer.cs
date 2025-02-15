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
public interface IReverse2DSquareRpcServer:TouchSocket.Rpc.IRemoteServer
{
///<summary>
///更新位置
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void UpdatePosition(System.Int32 id,System.Numerics.Vector3 vector3,System.Int64 time,IInvokeOption invokeOption = default);
///<summary>
///更新位置
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task UpdatePositionAsync(System.Int32 id,System.Numerics.Vector3 vector3,System.Int64 time,IInvokeOption invokeOption = default);

///<summary>
///创建新的NPC
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void NewNPC(System.Int32 id,System.Numerics.Vector3 vector3,IInvokeOption invokeOption = default);
///<summary>
///创建新的NPC
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task NewNPCAsync(System.Int32 id,System.Numerics.Vector3 vector3,IInvokeOption invokeOption = default);

///<summary>
///玩家离线
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void Offline(System.Int32 id,IInvokeOption invokeOption = default);
///<summary>
///玩家离线
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task OfflineAsync(System.Int32 id,IInvokeOption invokeOption = default);

///<summary>
///玩家登陆
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
void PlayerLogin(System.Int32 id,IInvokeOption invokeOption = default);
///<summary>
///玩家登陆
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task PlayerLoginAsync(System.Int32 id,IInvokeOption invokeOption = default);

}
public class Reverse2DSquareRpcServer :IReverse2DSquareRpcServer
{
public Reverse2DSquareRpcServer(IRpcClient client)
{
this.Client=client;
}
public IRpcClient Client{get;private set; }
///<summary>
///更新位置
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void UpdatePosition(System.Int32 id,System.Numerics.Vector3 vector3,System.Int64 time,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] @_parameters = new object[]{id,vector3,time};
this.Client.Invoke("UpdatePosition",null,invokeOption, @_parameters);

}
///<summary>
///更新位置
///</summary>
public Task UpdatePositionAsync(System.Int32 id,System.Numerics.Vector3 vector3,System.Int64 time,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id,vector3,time};
return this.Client.InvokeAsync("UpdatePosition",null,invokeOption, parameters);

}

///<summary>
///创建新的NPC
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void NewNPC(System.Int32 id,System.Numerics.Vector3 vector3,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] @_parameters = new object[]{id,vector3};
this.Client.Invoke("NewNPC",null,invokeOption, @_parameters);

}
///<summary>
///创建新的NPC
///</summary>
public Task NewNPCAsync(System.Int32 id,System.Numerics.Vector3 vector3,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id,vector3};
return this.Client.InvokeAsync("NewNPC",null,invokeOption, parameters);

}

///<summary>
///玩家离线
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void Offline(System.Int32 id,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] @_parameters = new object[]{id};
this.Client.Invoke("Offline",null,invokeOption, @_parameters);

}
///<summary>
///玩家离线
///</summary>
public Task OfflineAsync(System.Int32 id,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id};
return this.Client.InvokeAsync("Offline",null,invokeOption, parameters);

}

///<summary>
///玩家登陆
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public void PlayerLogin(System.Int32 id,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] @_parameters = new object[]{id};
this.Client.Invoke("PlayerLogin",null,invokeOption, @_parameters);

}
///<summary>
///玩家登陆
///</summary>
public Task PlayerLoginAsync(System.Int32 id,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id};
return this.Client.InvokeAsync("PlayerLogin",null,invokeOption, parameters);

}

}
public static class Reverse2DSquareRpcServerExtensions
{
///<summary>
///更新位置
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void UpdatePosition<TClient>(this TClient client,System.Int32 id,System.Numerics.Vector3 vector3,System.Int64 time,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] @_parameters = new object[]{id,vector3,time};
client.Invoke("UpdatePosition",null,invokeOption, @_parameters);

}
///<summary>
///更新位置
///</summary>
public static Task UpdatePositionAsync<TClient>(this TClient client,System.Int32 id,System.Numerics.Vector3 vector3,System.Int64 time,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] parameters = new object[]{id,vector3,time};
return client.InvokeAsync("UpdatePosition",null,invokeOption, parameters);

}

///<summary>
///创建新的NPC
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void NewNPC<TClient>(this TClient client,System.Int32 id,System.Numerics.Vector3 vector3,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] @_parameters = new object[]{id,vector3};
client.Invoke("NewNPC",null,invokeOption, @_parameters);

}
///<summary>
///创建新的NPC
///</summary>
public static Task NewNPCAsync<TClient>(this TClient client,System.Int32 id,System.Numerics.Vector3 vector3,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] parameters = new object[]{id,vector3};
return client.InvokeAsync("NewNPC",null,invokeOption, parameters);

}

///<summary>
///玩家离线
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void Offline<TClient>(this TClient client,System.Int32 id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] @_parameters = new object[]{id};
client.Invoke("Offline",null,invokeOption, @_parameters);

}
///<summary>
///玩家离线
///</summary>
public static Task OfflineAsync<TClient>(this TClient client,System.Int32 id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] parameters = new object[]{id};
return client.InvokeAsync("Offline",null,invokeOption, parameters);

}

///<summary>
///玩家登陆
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static void PlayerLogin<TClient>(this TClient client,System.Int32 id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] @_parameters = new object[]{id};
client.Invoke("PlayerLogin",null,invokeOption, @_parameters);

}
///<summary>
///玩家登陆
///</summary>
public static Task PlayerLoginAsync<TClient>(this TClient client,System.Int32 id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] parameters = new object[]{id};
return client.InvokeAsync("PlayerLogin",null,invokeOption, parameters);

}

}
}
