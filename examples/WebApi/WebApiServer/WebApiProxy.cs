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
using TouchSocket.Http;
using TouchSocket.WebApi;
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
Task<System.Int32> SumAsync(System.Int32 a,System.Int32 b,InvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> CalculateAsync(System.Int32 x,System.Int32 y,InvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> GetProductByIdAsync(InvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> GetCallContextAsync(System.Int32 a,System.Int32 b,InvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<MyClass> GetMyClassAsync(InvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> TestPostAsync(MyClass myClass,InvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> SumFromQueryAsync(System.Int32 a,System.Int32 b,InvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> SumFromFormAsync(System.Int32 a,System.Int32 b,InvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> SumFromHeaderAsync(System.Int32 a,System.Int32 b,InvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> SumCallContextAsync(System.Int32 a,System.Int32 b,InvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> DownloadFileAsync(System.String id,InvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> PostContentAsync(InvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> UploadMultiFileAsync(System.String id,InvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> UploadBigFileAsync(InvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task CustomResponseAsync(InvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> SetHeadersAsync(InvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> PlainTextAsync(InvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task StreamResponseAsync(InvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Object> GetRequestInfoAsync(InvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task ConditionalResponseAsync(System.String type,InvokeOption invokeOption = default);

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
public async Task<System.Int32> SumAsync(System.Int32 a,System.Int32 b,InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Forms = null;
return (System.Int32) await this.Client.InvokeAsync("/apiserver/sumab", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> CalculateAsync(System.Int32 x,System.Int32 y,InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("x",x.ToString()),new KeyValuePair<string, string>("y",y.ToString())};
_request.Forms = null;
return (System.Int32) await this.Client.InvokeAsync("/api/custom/calculate", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public async Task<System.String> GetProductByIdAsync(InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) await this.Client.InvokeAsync("/apiserver/getproductbyid", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> GetCallContextAsync(System.Int32 a,System.Int32 b,InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Forms = null;
return (System.Int32) await this.Client.InvokeAsync("/apiserver/getcallcontext", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public async Task<MyClass> GetMyClassAsync(InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = null;
_request.Forms = null;
return (MyClass) await this.Client.InvokeAsync("/apiserver/getmyclass", typeof(MyClass), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> TestPostAsync(MyClass myClass,InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = "Post";
_request.Headers = null;
_request.Querys = null;
_request.Forms = null;
_request.Body = myClass;
return (System.Int32) await this.Client.InvokeAsync("/apiserver/testpost", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> SumFromQueryAsync(System.Int32 a,System.Int32 b,InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("aa",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Forms = null;
return (System.Int32) await this.Client.InvokeAsync("/apiserver/sumfromquery", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> SumFromFormAsync(System.Int32 a,System.Int32 b,InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = null;
_request.Forms = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
return (System.Int32) await this.Client.InvokeAsync("/apiserver/sumfromform", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> SumFromHeaderAsync(System.Int32 a,System.Int32 b,InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Querys = null;
_request.Forms = null;
return (System.Int32) await this.Client.InvokeAsync("/apiserver/sumfromheader", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> SumCallContextAsync(System.Int32 a,System.Int32 b,InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Forms = null;
return (System.Int32) await this.Client.InvokeAsync("/apiserver/sumcallcontext", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public async Task<System.String> DownloadFileAsync(System.String id,InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("id",id?.ToString())};
_request.Forms = null;
return (System.String) await this.Client.InvokeAsync("/apiserver/downloadfile", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public async Task<System.String> PostContentAsync(InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = "Post";
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) await this.Client.InvokeAsync("/apiserver/postcontent", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public async Task<System.String> UploadMultiFileAsync(System.String id,InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = "Post";
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("id",id?.ToString())};
_request.Forms = null;
return (System.String) await this.Client.InvokeAsync("/apiserver/uploadmultifile", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public async Task<System.String> UploadBigFileAsync(InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = "Post";
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) await this.Client.InvokeAsync("/apiserver/uploadbigfile", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public Task CustomResponseAsync(InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = null;
_request.Forms = null;
return this.Client.InvokeAsync("/apiserver/customresponse", default, invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public async Task<System.String> SetHeadersAsync(InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) await this.Client.InvokeAsync("/apiserver/setheaders", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public async Task<System.String> PlainTextAsync(InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) await this.Client.InvokeAsync("/apiserver/plaintext", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public Task StreamResponseAsync(InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = null;
_request.Forms = null;
return this.Client.InvokeAsync("/apiserver/streamresponse", default, invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public async Task<System.Object> GetRequestInfoAsync(InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = null;
_request.Forms = null;
return (System.Object) await this.Client.InvokeAsync("/apiserver/getrequestinfo", typeof(System.Object), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public Task ConditionalResponseAsync(System.String type,InvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("type",type?.ToString())};
_request.Forms = null;
return this.Client.InvokeAsync("/apiserver/conditionalresponse", default, invokeOption, _request);

}

}
public static class ApiServerExtensions
{
///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> SumAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,InvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Forms = null;
return (System.Int32) await client.InvokeAsync("/apiserver/sumab", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> CalculateAsync<TClient>(this TClient client,System.Int32 x,System.Int32 y,InvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("x",x.ToString()),new KeyValuePair<string, string>("y",y.ToString())};
_request.Forms = null;
return (System.Int32) await client.InvokeAsync("/api/custom/calculate", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public static async Task<System.String> GetProductByIdAsync<TClient>(this TClient client,InvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) await client.InvokeAsync("/apiserver/getproductbyid", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> GetCallContextAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,InvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Forms = null;
return (System.Int32) await client.InvokeAsync("/apiserver/getcallcontext", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public static async Task<MyClass> GetMyClassAsync<TClient>(this TClient client,InvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = null;
_request.Forms = null;
return (MyClass) await client.InvokeAsync("/apiserver/getmyclass", typeof(MyClass), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> TestPostAsync<TClient>(this TClient client,MyClass myClass,InvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = "Post";
_request.Headers = null;
_request.Querys = null;
_request.Forms = null;
_request.Body = myClass;
return (System.Int32) await client.InvokeAsync("/apiserver/testpost", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> SumFromQueryAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,InvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("aa",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Forms = null;
return (System.Int32) await client.InvokeAsync("/apiserver/sumfromquery", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> SumFromFormAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,InvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = null;
_request.Forms = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
return (System.Int32) await client.InvokeAsync("/apiserver/sumfromform", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> SumFromHeaderAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,InvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Querys = null;
_request.Forms = null;
return (System.Int32) await client.InvokeAsync("/apiserver/sumfromheader", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> SumCallContextAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,InvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Forms = null;
return (System.Int32) await client.InvokeAsync("/apiserver/sumcallcontext", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public static async Task<System.String> DownloadFileAsync<TClient>(this TClient client,System.String id,InvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("id",id?.ToString())};
_request.Forms = null;
return (System.String) await client.InvokeAsync("/apiserver/downloadfile", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public static async Task<System.String> PostContentAsync<TClient>(this TClient client,InvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = "Post";
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) await client.InvokeAsync("/apiserver/postcontent", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public static async Task<System.String> UploadMultiFileAsync<TClient>(this TClient client,System.String id,InvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = "Post";
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("id",id?.ToString())};
_request.Forms = null;
return (System.String) await client.InvokeAsync("/apiserver/uploadmultifile", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public static async Task<System.String> UploadBigFileAsync<TClient>(this TClient client,InvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = "Post";
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) await client.InvokeAsync("/apiserver/uploadbigfile", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public static Task CustomResponseAsync<TClient>(this TClient client,InvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = null;
_request.Forms = null;
return client.InvokeAsync("/apiserver/customresponse", default, invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public static async Task<System.String> SetHeadersAsync<TClient>(this TClient client,InvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) await client.InvokeAsync("/apiserver/setheaders", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public static async Task<System.String> PlainTextAsync<TClient>(this TClient client,InvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) await client.InvokeAsync("/apiserver/plaintext", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public static Task StreamResponseAsync<TClient>(this TClient client,InvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = null;
_request.Forms = null;
return client.InvokeAsync("/apiserver/streamresponse", default, invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public static async Task<System.Object> GetRequestInfoAsync<TClient>(this TClient client,InvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = null;
_request.Forms = null;
return (System.Object) await client.InvokeAsync("/apiserver/getrequestinfo", typeof(System.Object), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
public static Task ConditionalResponseAsync<TClient>(this TClient client,System.String type,InvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = "Get";
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("type",type?.ToString())};
_request.Forms = null;
return client.InvokeAsync("/apiserver/conditionalresponse", default, invokeOption, _request);

}

}
public class MyClass
{
public System.Int32 A { get; set; }
public System.Int32 B { get; set; }
}

}
