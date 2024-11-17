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

///<summary>
///测试客户端推送流数据
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 RpcPushChannel(System.Int32 channelID,IInvokeOption invokeOption = default);
///<summary>
///测试客户端推送流数据
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> RpcPushChannelAsync(System.Int32 channelID,IInvokeOption invokeOption = default);

///<summary>
///测试取消调用
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 TestCancellationToken(IInvokeOption invokeOption = default);
///<summary>
///测试取消调用
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> TestCancellationTokenAsync(IInvokeOption invokeOption = default);

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
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] @_parameters = new object[]{a,b};
System.Int32 returnData=(System.Int32)this.Client.Invoke("Add",typeof(System.Int32),invokeOption, @_parameters);
return returnData;
}
///<summary>
///将两个数相加
///</summary>
public async Task<System.Int32> AddAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{a,b};
return (System.Int32) await this.Client.InvokeAsync("Add",typeof(System.Int32),invokeOption, parameters);
}

///<summary>
///测试客户端请求，服务器响应大量流数据
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 RpcPullChannel(System.Int32 channelID,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] @_parameters = new object[]{channelID};
System.Int32 returnData=(System.Int32)this.Client.Invoke("consoleapp2.myrpcserver.rpcpullchannel",typeof(System.Int32),invokeOption, @_parameters);
return returnData;
}
///<summary>
///测试客户端请求，服务器响应大量流数据
///</summary>
public async Task<System.Int32> RpcPullChannelAsync(System.Int32 channelID,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{channelID};
return (System.Int32) await this.Client.InvokeAsync("consoleapp2.myrpcserver.rpcpullchannel",typeof(System.Int32),invokeOption, parameters);
}

///<summary>
///测试客户端推送流数据
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 RpcPushChannel(System.Int32 channelID,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] @_parameters = new object[]{channelID};
System.Int32 returnData=(System.Int32)this.Client.Invoke("consoleapp2.myrpcserver.rpcpushchannel",typeof(System.Int32),invokeOption, @_parameters);
return returnData;
}
///<summary>
///测试客户端推送流数据
///</summary>
public async Task<System.Int32> RpcPushChannelAsync(System.Int32 channelID,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{channelID};
return (System.Int32) await this.Client.InvokeAsync("consoleapp2.myrpcserver.rpcpushchannel",typeof(System.Int32),invokeOption, parameters);
}

///<summary>
///测试取消调用
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 TestCancellationToken(IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
System.Int32 returnData=(System.Int32)this.Client.Invoke("consoleapp2.myrpcserver.testcancellationtoken",typeof(System.Int32),invokeOption, null);
return returnData;
}
///<summary>
///测试取消调用
///</summary>
public async Task<System.Int32> TestCancellationTokenAsync(IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
return (System.Int32) await this.Client.InvokeAsync("consoleapp2.myrpcserver.testcancellationtoken",typeof(System.Int32),invokeOption, null);
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
object[] @_parameters = new object[]{a,b};
System.Int32 returnData=(System.Int32)client.Invoke("Add",typeof(System.Int32),invokeOption, @_parameters);
return returnData;
}
///<summary>
///将两个数相加
///</summary>
public static async Task<System.Int32> AddAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{a,b};
return (System.Int32) await client.InvokeAsync("Add",typeof(System.Int32),invokeOption, parameters);
}

///<summary>
///测试客户端请求，服务器响应大量流数据
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 RpcPullChannel<TClient>(this TClient client,System.Int32 channelID,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] @_parameters = new object[]{channelID};
System.Int32 returnData=(System.Int32)client.Invoke("consoleapp2.myrpcserver.rpcpullchannel",typeof(System.Int32),invokeOption, @_parameters);
return returnData;
}
///<summary>
///测试客户端请求，服务器响应大量流数据
///</summary>
public static async Task<System.Int32> RpcPullChannelAsync<TClient>(this TClient client,System.Int32 channelID,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{channelID};
return (System.Int32) await client.InvokeAsync("consoleapp2.myrpcserver.rpcpullchannel",typeof(System.Int32),invokeOption, parameters);
}

///<summary>
///测试客户端推送流数据
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 RpcPushChannel<TClient>(this TClient client,System.Int32 channelID,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] @_parameters = new object[]{channelID};
System.Int32 returnData=(System.Int32)client.Invoke("consoleapp2.myrpcserver.rpcpushchannel",typeof(System.Int32),invokeOption, @_parameters);
return returnData;
}
///<summary>
///测试客户端推送流数据
///</summary>
public static async Task<System.Int32> RpcPushChannelAsync<TClient>(this TClient client,System.Int32 channelID,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
object[] parameters = new object[]{channelID};
return (System.Int32) await client.InvokeAsync("consoleapp2.myrpcserver.rpcpushchannel",typeof(System.Int32),invokeOption, parameters);
}

///<summary>
///测试取消调用
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 TestCancellationToken<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
System.Int32 returnData=(System.Int32)client.Invoke("consoleapp2.myrpcserver.testcancellationtoken",typeof(System.Int32),invokeOption, null);
return returnData;
}
///<summary>
///测试取消调用
///</summary>
public static async Task<System.Int32> TestCancellationTokenAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Rpc.IRpcClient{
return (System.Int32) await client.InvokeAsync("consoleapp2.myrpcserver.testcancellationtoken",typeof(System.Int32),invokeOption, null);
}

}
}
