using System;
using TouchSocket.Core;
using TouchSocket.Sockets;
using TouchSocket.Rpc;
using TouchSocket.Dmtp.Rpc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
namespace RpcProxy
{
public interface IMyRpcServer:TouchSocket.Rpc.IRemoteServer
{
///<summary>
///将两个数相加
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 Add(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default);
///<summary>
///将两个数相加
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> AddAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default);

///<summary>
///测试客户端请求，服务器响应大量流数据
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 RpcPullChannel(System.Int32 channelID,IInvokeOption invokeOption = default);
///<summary>
///测试客户端请求，服务器响应大量流数据
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> RpcPullChannelAsync(System.Int32 channelID,IInvokeOption invokeOption = default);

}
public class MyRpcServer :IMyRpcServer
{
public MyRpcServer(IRpcClient client)
{
this.Client=client;
}
public IRpcClient Client{get;private set; }
///<summary>
///将两个数相加
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 Add(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{a,b};
System.Int32 returnData=(System.Int32)Client.Invoke(typeof(System.Int32),"Add",invokeOption, parameters);
return returnData;
}
///<summary>
///将两个数相加
///</summary>
public async Task<System.Int32> AddAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{a,b};
return (System.Int32) await Client.InvokeAsync(typeof(System.Int32),"Add",invokeOption, parameters);
}

///<summary>
///测试客户端请求，服务器响应大量流数据
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 RpcPullChannel(System.Int32 channelID,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{channelID};
System.Int32 returnData=(System.Int32)Client.Invoke(typeof(System.Int32),"consoleapp2.program+myrpcserver.rpcpullchannel",invokeOption, parameters);
return returnData;
}
///<summary>
///测试客户端请求，服务器响应大量流数据
///</summary>
public async Task<System.Int32> RpcPullChannelAsync(System.Int32 channelID,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{channelID};
return (System.Int32) await Client.InvokeAsync(typeof(System.Int32),"consoleapp2.program+myrpcserver.rpcpullchannel",invokeOption, parameters);
}

}
public static class MyRpcServerExtensions
{
///<summary>
///将两个数相加
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 Add<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{a,b};
System.Int32 returnData=(System.Int32)client.Invoke(typeof(System.Int32),"Add",invokeOption, parameters);
return returnData;
}
///<summary>
///将两个数相加
///</summary>
public static async Task<System.Int32> AddAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{a,b};
return (System.Int32) await client.InvokeAsync(typeof(System.Int32),"Add",invokeOption, parameters);
}

///<summary>
///测试客户端请求，服务器响应大量流数据
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 RpcPullChannel<TClient>(this TClient client,System.Int32 channelID,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{channelID};
System.Int32 returnData=(System.Int32)client.Invoke(typeof(System.Int32),"consoleapp2.program+myrpcserver.rpcpullchannel",invokeOption, parameters);
return returnData;
}
///<summary>
///测试客户端请求，服务器响应大量流数据
///</summary>
public static async Task<System.Int32> RpcPullChannelAsync<TClient>(this TClient client,System.Int32 channelID,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{channelID};
return (System.Int32) await client.InvokeAsync(typeof(System.Int32),"consoleapp2.program+myrpcserver.rpcpullchannel",invokeOption, parameters);
}

}
}
