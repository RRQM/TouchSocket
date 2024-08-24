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
namespace WebApiProxy
{
public interface IApiServer:TouchSocket.Rpc.IRemoteServer
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
MyClass GetMyClass(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<MyClass> GetMyClassAsync(IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 TestPost(MyClass myClass,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> TestPostAsync(MyClass myClass,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.String DownloadFile(System.String id,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> DownloadFileAsync(System.String id,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.String PostContent(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> PostContentAsync(IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.String UploadMultiFile(System.String id,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> UploadMultiFileAsync(System.String id,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.String UploadBigFile(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> UploadBigFileAsync(IInvokeOption invokeOption = default);

}
public class ApiServer :IApiServer
{
public ApiServer(IRpcClient client)
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
object[] parameters = new object[]{a,b};
System.Int32 returnData=(System.Int32)Client.Invoke(typeof(System.Int32),"GET:/apiserver/sumab?a={0}&b={1}",invokeOption, parameters);
return returnData;
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
object[] parameters = new object[]{a,b};
return (System.Int32) await Client.InvokeAsync(typeof(System.Int32),"GET:/apiserver/sumab?a={0}&b={1}",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public MyClass GetMyClass(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
MyClass returnData=(MyClass)Client.Invoke(typeof(MyClass),"GET:/apiserver/getmyclass",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<MyClass> GetMyClassAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
return (MyClass) await Client.InvokeAsync(typeof(MyClass),"GET:/apiserver/getmyclass",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 TestPost(MyClass myClass,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{myClass};
System.Int32 returnData=(System.Int32)Client.Invoke(typeof(System.Int32),"POST:/apiserver/testpost?",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> TestPostAsync(MyClass myClass,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{myClass};
return (System.Int32) await Client.InvokeAsync(typeof(System.Int32),"POST:/apiserver/testpost?",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String DownloadFile(System.String id,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id};
System.String returnData=(System.String)Client.Invoke(typeof(System.String),"GET:/apiserver/downloadfile?id={0}",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> DownloadFileAsync(System.String id,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id};
return (System.String) await Client.InvokeAsync(typeof(System.String),"GET:/apiserver/downloadfile?id={0}",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String PostContent(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
System.String returnData=(System.String)Client.Invoke(typeof(System.String),"POST:/apiserver/postcontent",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> PostContentAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
return (System.String) await Client.InvokeAsync(typeof(System.String),"POST:/apiserver/postcontent",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String UploadMultiFile(System.String id,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id};
System.String returnData=(System.String)Client.Invoke(typeof(System.String),"POST:/apiserver/uploadmultifile?id={0}",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> UploadMultiFileAsync(System.String id,IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
object[] parameters = new object[]{id};
return (System.String) await Client.InvokeAsync(typeof(System.String),"POST:/apiserver/uploadmultifile?id={0}",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String UploadBigFile(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
System.String returnData=(System.String)Client.Invoke(typeof(System.String),"POST:/apiserver/uploadbigfile",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> UploadBigFileAsync(IInvokeOption invokeOption = default)
{
if(Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
return (System.String) await Client.InvokeAsync(typeof(System.String),"POST:/apiserver/uploadbigfile",invokeOption, null);
}

}
public static class ApiServerExtensions
{
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 Sum<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{a,b};
System.Int32 returnData=(System.Int32)client.Invoke(typeof(System.Int32),"GET:/apiserver/sumab?a={0}&b={1}",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> SumAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{a,b};
return (System.Int32) await client.InvokeAsync(typeof(System.Int32),"GET:/apiserver/sumab?a={0}&b={1}",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static MyClass GetMyClass<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
MyClass returnData=(MyClass)client.Invoke(typeof(MyClass),"GET:/apiserver/getmyclass",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<MyClass> GetMyClassAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
return (MyClass) await client.InvokeAsync(typeof(MyClass),"GET:/apiserver/getmyclass",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 TestPost<TClient>(this TClient client,MyClass myClass,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{myClass};
System.Int32 returnData=(System.Int32)client.Invoke(typeof(System.Int32),"POST:/apiserver/testpost?",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> TestPostAsync<TClient>(this TClient client,MyClass myClass,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{myClass};
return (System.Int32) await client.InvokeAsync(typeof(System.Int32),"POST:/apiserver/testpost?",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String DownloadFile<TClient>(this TClient client,System.String id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{id};
System.String returnData=(System.String)client.Invoke(typeof(System.String),"GET:/apiserver/downloadfile?id={0}",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> DownloadFileAsync<TClient>(this TClient client,System.String id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{id};
return (System.String) await client.InvokeAsync(typeof(System.String),"GET:/apiserver/downloadfile?id={0}",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String PostContent<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
System.String returnData=(System.String)client.Invoke(typeof(System.String),"POST:/apiserver/postcontent",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> PostContentAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
return (System.String) await client.InvokeAsync(typeof(System.String),"POST:/apiserver/postcontent",invokeOption, null);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String UploadMultiFile<TClient>(this TClient client,System.String id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{id};
System.String returnData=(System.String)client.Invoke(typeof(System.String),"POST:/apiserver/uploadmultifile?id={0}",invokeOption, parameters);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> UploadMultiFileAsync<TClient>(this TClient client,System.String id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
object[] parameters = new object[]{id};
return (System.String) await client.InvokeAsync(typeof(System.String),"POST:/apiserver/uploadmultifile?id={0}",invokeOption, parameters);
}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String UploadBigFile<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
System.String returnData=(System.String)client.Invoke(typeof(System.String),"POST:/apiserver/uploadbigfile",invokeOption, null);
return returnData;
}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> UploadBigFileAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
return (System.String) await client.InvokeAsync(typeof(System.String),"POST:/apiserver/uploadbigfile",invokeOption, null);
}

}
public class MyClass
{
public System.Int32 A { get; set; }
public System.Int32 B { get; set; }
}

}
