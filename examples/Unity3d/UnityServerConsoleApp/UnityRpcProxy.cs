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
public interface IMyRpcServer:TouchSocket.Rpc.IRemoteServer
{
///<summary>
///登录
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
MyLoginModelResult DmtpRpc_Login(MyLoginModel model,IInvokeOption invokeOption = default);
///<summary>
///登录
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<MyLoginModelResult> DmtpRpc_LoginAsync(MyLoginModel model,IInvokeOption invokeOption = default);

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 DmtpRpc_Performance(System.Int32 i,IInvokeOption invokeOption = default);
///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> DmtpRpc_PerformanceAsync(System.Int32 i,IInvokeOption invokeOption = default);

///<summary>
///登录
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
MyLoginModelResult JsonRpc_Login(MyLoginModel model,IInvokeOption invokeOption = default);
///<summary>
///登录
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<MyLoginModelResult> JsonRpc_LoginAsync(MyLoginModel model,IInvokeOption invokeOption = default);

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 JsonRpc_Performance(System.Int32 i,IInvokeOption invokeOption = default);
///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> JsonRpc_PerformanceAsync(System.Int32 i,IInvokeOption invokeOption = default);

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
public MyLoginModelResult DmtpRpc_Login(MyLoginModel model,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] @_parameters = new object[]{model};
MyLoginModelResult returnData=(MyLoginModelResult)this.Client.Invoke("DmtpRpc_Login",typeof(MyLoginModelResult),invokeOption, @_parameters);
return returnData;

}
///<summary>
///登录
///</summary>
public async Task<MyLoginModelResult> DmtpRpc_LoginAsync(MyLoginModel model,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{model};
return (MyLoginModelResult) await this.Client.InvokeAsync("DmtpRpc_Login",typeof(MyLoginModelResult),invokeOption, parameters);

}

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 DmtpRpc_Performance(System.Int32 i,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] @_parameters = new object[]{i};
System.Int32 returnData=(System.Int32)this.Client.Invoke("DmtpRpc_Performance",typeof(System.Int32),invokeOption, @_parameters);
return returnData;

}
///<summary>
///性能测试
///</summary>
public async Task<System.Int32> DmtpRpc_PerformanceAsync(System.Int32 i,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{i};
return (System.Int32) await this.Client.InvokeAsync("DmtpRpc_Performance",typeof(System.Int32),invokeOption, parameters);

}

///<summary>
///登录
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public MyLoginModelResult JsonRpc_Login(MyLoginModel model,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] @_parameters = new object[]{model};
MyLoginModelResult returnData=(MyLoginModelResult)this.Client.Invoke("JsonRpc_Login",typeof(MyLoginModelResult),invokeOption, @_parameters);
return returnData;

}
///<summary>
///登录
///</summary>
public async Task<MyLoginModelResult> JsonRpc_LoginAsync(MyLoginModel model,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{model};
return (MyLoginModelResult) await this.Client.InvokeAsync("JsonRpc_Login",typeof(MyLoginModelResult),invokeOption, parameters);

}

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 JsonRpc_Performance(System.Int32 i,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] @_parameters = new object[]{i};
System.Int32 returnData=(System.Int32)this.Client.Invoke("JsonRpc_Performance",typeof(System.Int32),invokeOption, @_parameters);
return returnData;

}
///<summary>
///性能测试
///</summary>
public async Task<System.Int32> JsonRpc_PerformanceAsync(System.Int32 i,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{i};
return (System.Int32) await this.Client.InvokeAsync("JsonRpc_Performance",typeof(System.Int32),invokeOption, parameters);

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
public static MyLoginModelResult DmtpRpc_Login<TClient>(this TClient client,MyLoginModel model,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Dmtp.Rpc.IDmtpRpcActor{
object[] @_parameters = new object[]{model};
MyLoginModelResult returnData=(MyLoginModelResult)client.Invoke("DmtpRpc_Login",typeof(MyLoginModelResult),invokeOption, @_parameters);
return returnData;

}
///<summary>
///登录
///</summary>
public static async Task<MyLoginModelResult> DmtpRpc_LoginAsync<TClient>(this TClient client,MyLoginModel model,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Dmtp.Rpc.IDmtpRpcActor{
object[] parameters = new object[]{model};
return (MyLoginModelResult) await client.InvokeAsync("DmtpRpc_Login",typeof(MyLoginModelResult),invokeOption, parameters);

}

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 DmtpRpc_Performance<TClient>(this TClient client,System.Int32 i,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Dmtp.Rpc.IDmtpRpcActor{
object[] @_parameters = new object[]{i};
System.Int32 returnData=(System.Int32)client.Invoke("DmtpRpc_Performance",typeof(System.Int32),invokeOption, @_parameters);
return returnData;

}
///<summary>
///性能测试
///</summary>
public static async Task<System.Int32> DmtpRpc_PerformanceAsync<TClient>(this TClient client,System.Int32 i,IInvokeOption invokeOption = default) where TClient:
TouchSocket.Dmtp.Rpc.IDmtpRpcActor{
object[] parameters = new object[]{i};
return (System.Int32) await client.InvokeAsync("DmtpRpc_Performance",typeof(System.Int32),invokeOption, parameters);

}

///<summary>
///登录
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static MyLoginModelResult JsonRpc_Login<TClient>(this TClient client,MyLoginModel model,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] @_parameters = new object[]{model};
MyLoginModelResult returnData=(MyLoginModelResult)client.Invoke("JsonRpc_Login",typeof(MyLoginModelResult),invokeOption, @_parameters);
return returnData;

}
///<summary>
///登录
///</summary>
public static async Task<MyLoginModelResult> JsonRpc_LoginAsync<TClient>(this TClient client,MyLoginModel model,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] parameters = new object[]{model};
return (MyLoginModelResult) await client.InvokeAsync("JsonRpc_Login",typeof(MyLoginModelResult),invokeOption, parameters);

}

///<summary>
///性能测试
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 JsonRpc_Performance<TClient>(this TClient client,System.Int32 i,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] @_parameters = new object[]{i};
System.Int32 returnData=(System.Int32)client.Invoke("JsonRpc_Performance",typeof(System.Int32),invokeOption, @_parameters);
return returnData;

}
///<summary>
///性能测试
///</summary>
public static async Task<System.Int32> JsonRpc_PerformanceAsync<TClient>(this TClient client,System.Int32 i,IInvokeOption invokeOption = default) where TClient:
TouchSocket.JsonRpc.IJsonRpcClient{
object[] parameters = new object[]{i};
return (System.Int32) await client.InvokeAsync("JsonRpc_Performance",typeof(System.Int32),invokeOption, parameters);

}

}
public class MyLoginModelResult
{
public TouchSocket.Core.ResultCode ResultCode { get; set; }
public System.String Message { get; set; }
}

public class MyLoginModel
{
public System.String Token { get; set; }
public System.String Account { get; set; }
public System.String Password { get; set; }
}

}
