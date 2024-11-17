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
System.Int32 SumCallContext(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> SumCallContextAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default);

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

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.String GetString(IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.String> GetStringAsync(IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 SumFromForm(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> SumFromFormAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 SumFromQuery(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> SumFromQueryAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default);

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
System.Int32 SumFromHeader(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default);
///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
Task<System.Int32> SumFromHeaderAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default);

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
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Forms = null;
return (System.Int32) this.Client.Invoke("/apiserver/sumab", typeof(System.Int32), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> SumAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Forms = null;
return (System.Int32) await this.Client.InvokeAsync("/apiserver/sumab", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 SumCallContext(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Forms = null;
return (System.Int32) this.Client.Invoke("/apiserver/sumcallcontext", typeof(System.Int32), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> SumCallContextAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Forms = null;
return (System.Int32) await this.Client.InvokeAsync("/apiserver/sumcallcontext", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public MyClass GetMyClass(IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = null;
_request.Querys = null;
_request.Forms = null;
return (MyClass) this.Client.Invoke("/apiserver/getmyclass", typeof(MyClass), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public async Task<MyClass> GetMyClassAsync(IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = null;
_request.Querys = null;
_request.Forms = null;
return (MyClass) await this.Client.InvokeAsync("/apiserver/getmyclass", typeof(MyClass), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 TestPost(MyClass myClass,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Post;
_request.Headers = null;
_request.Querys = null;
_request.Forms = null;
_request.Body = myClass;
return (System.Int32) this.Client.Invoke("/apiserver/testpost", typeof(System.Int32), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> TestPostAsync(MyClass myClass,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Post;
_request.Headers = null;
_request.Querys = null;
_request.Forms = null;
_request.Body = myClass;
return (System.Int32) await this.Client.InvokeAsync("/apiserver/testpost", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String DownloadFile(System.String id,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("id",id?.ToString())};
_request.Forms = null;
return (System.String) this.Client.Invoke("/apiserver/downloadfile", typeof(System.String), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> DownloadFileAsync(System.String id,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("id",id?.ToString())};
_request.Forms = null;
return (System.String) await this.Client.InvokeAsync("/apiserver/downloadfile", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String PostContent(IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Post;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) this.Client.Invoke("/apiserver/postcontent", typeof(System.String), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> PostContentAsync(IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Post;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) await this.Client.InvokeAsync("/apiserver/postcontent", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String UploadMultiFile(System.String id,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Post;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("id",id?.ToString())};
_request.Forms = null;
return (System.String) this.Client.Invoke("/apiserver/uploadmultifile", typeof(System.String), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> UploadMultiFileAsync(System.String id,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Post;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("id",id?.ToString())};
_request.Forms = null;
return (System.String) await this.Client.InvokeAsync("/apiserver/uploadmultifile", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String UploadBigFile(IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Post;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) this.Client.Invoke("/apiserver/uploadbigfile", typeof(System.String), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> UploadBigFileAsync(IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Post;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) await this.Client.InvokeAsync("/apiserver/uploadbigfile", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.String GetString(IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) this.Client.Invoke("/apiserver/getstring", typeof(System.String), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public async Task<System.String> GetStringAsync(IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) await this.Client.InvokeAsync("/apiserver/getstring", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 SumFromForm(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = null;
_request.Querys = null;
_request.Forms = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
return (System.Int32) this.Client.Invoke("/apiserver/sumfromform", typeof(System.Int32), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> SumFromFormAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = null;
_request.Querys = null;
_request.Forms = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
return (System.Int32) await this.Client.InvokeAsync("/apiserver/sumfromform", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 SumFromQuery(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("aa",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Forms = null;
return (System.Int32) this.Client.Invoke("/apiserver/sumfromquery", typeof(System.Int32), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> SumFromQueryAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("aa",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Forms = null;
return (System.Int32) await this.Client.InvokeAsync("/apiserver/sumfromquery", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public System.Int32 SumFromHeader(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Querys = null;
_request.Forms = null;
return (System.Int32) this.Client.Invoke("/apiserver/sumfromheader", typeof(System.Int32), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public async Task<System.Int32> SumFromHeaderAsync(System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default)
{
if(this.Client==null)
{
throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
}
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Querys = null;
_request.Forms = null;
return (System.Int32) await this.Client.InvokeAsync("/apiserver/sumfromheader", typeof(System.Int32), invokeOption, _request);

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
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Forms = null;
return (System.Int32) client.Invoke("/apiserver/sumab", typeof(System.Int32), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> SumAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Forms = null;
return (System.Int32) await client.InvokeAsync("/apiserver/sumab", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 SumCallContext<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Forms = null;
return (System.Int32) client.Invoke("/apiserver/sumcallcontext", typeof(System.Int32), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> SumCallContextAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Forms = null;
return (System.Int32) await client.InvokeAsync("/apiserver/sumcallcontext", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static MyClass GetMyClass<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = null;
_request.Querys = null;
_request.Forms = null;
return (MyClass) client.Invoke("/apiserver/getmyclass", typeof(MyClass), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public static async Task<MyClass> GetMyClassAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = null;
_request.Querys = null;
_request.Forms = null;
return (MyClass) await client.InvokeAsync("/apiserver/getmyclass", typeof(MyClass), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 TestPost<TClient>(this TClient client,MyClass myClass,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Post;
_request.Headers = null;
_request.Querys = null;
_request.Forms = null;
_request.Body = myClass;
return (System.Int32) client.Invoke("/apiserver/testpost", typeof(System.Int32), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> TestPostAsync<TClient>(this TClient client,MyClass myClass,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Post;
_request.Headers = null;
_request.Querys = null;
_request.Forms = null;
_request.Body = myClass;
return (System.Int32) await client.InvokeAsync("/apiserver/testpost", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String DownloadFile<TClient>(this TClient client,System.String id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("id",id?.ToString())};
_request.Forms = null;
return (System.String) client.Invoke("/apiserver/downloadfile", typeof(System.String), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> DownloadFileAsync<TClient>(this TClient client,System.String id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("id",id?.ToString())};
_request.Forms = null;
return (System.String) await client.InvokeAsync("/apiserver/downloadfile", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String PostContent<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Post;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) client.Invoke("/apiserver/postcontent", typeof(System.String), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> PostContentAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Post;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) await client.InvokeAsync("/apiserver/postcontent", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String UploadMultiFile<TClient>(this TClient client,System.String id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Post;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("id",id?.ToString())};
_request.Forms = null;
return (System.String) client.Invoke("/apiserver/uploadmultifile", typeof(System.String), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> UploadMultiFileAsync<TClient>(this TClient client,System.String id,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Post;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("id",id?.ToString())};
_request.Forms = null;
return (System.String) await client.InvokeAsync("/apiserver/uploadmultifile", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String UploadBigFile<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Post;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) client.Invoke("/apiserver/uploadbigfile", typeof(System.String), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> UploadBigFileAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Post;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) await client.InvokeAsync("/apiserver/uploadbigfile", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.String GetString<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) client.Invoke("/apiserver/getstring", typeof(System.String), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public static async Task<System.String> GetStringAsync<TClient>(this TClient client,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("Accept","text/plain")};
_request.Querys = null;
_request.Forms = null;
return (System.String) await client.InvokeAsync("/apiserver/getstring", typeof(System.String), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 SumFromForm<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = null;
_request.Querys = null;
_request.Forms = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
return (System.Int32) client.Invoke("/apiserver/sumfromform", typeof(System.Int32), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> SumFromFormAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = null;
_request.Querys = null;
_request.Forms = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
return (System.Int32) await client.InvokeAsync("/apiserver/sumfromform", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 SumFromQuery<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("aa",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Forms = null;
return (System.Int32) client.Invoke("/apiserver/sumfromquery", typeof(System.Int32), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> SumFromQueryAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = null;
_request.Querys = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("aa",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Forms = null;
return (System.Int32) await client.InvokeAsync("/apiserver/sumfromquery", typeof(System.Int32), invokeOption, _request);

}

///<summary>
///无注释信息
///</summary>
/// <exception cref="System.TimeoutException">调用超时</exception>
/// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
/// <exception cref="System.Exception">其他异常</exception>
public static System.Int32 SumFromHeader<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Querys = null;
_request.Forms = null;
return (System.Int32) client.Invoke("/apiserver/sumfromheader", typeof(System.Int32), invokeOption, _request);

}
///<summary>
///无注释信息
///</summary>
public static async Task<System.Int32> SumFromHeaderAsync<TClient>(this TClient client,System.Int32 a,System.Int32 b,IInvokeOption invokeOption = default) where TClient:
TouchSocket.WebApi.IWebApiClientBase{
var _request=new WebApiRequest();
_request.Method = HttpMethodType.Get;
_request.Headers = new KeyValuePair<string, string>[] {new KeyValuePair<string, string>("a",a.ToString()),new KeyValuePair<string, string>("b",b.ToString())};
_request.Querys = null;
_request.Forms = null;
return (System.Int32) await client.InvokeAsync("/apiserver/sumfromheader", typeof(System.Int32), invokeOption, _request);

}

}
public class MyClass
{
public System.Int32 A { get; set; }
public System.Int32 B { get; set; }
}

}
