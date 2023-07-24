using System;
using TouchSocket.Core;
using TouchSocket.Sockets;
using TouchSocket.Rpc;
using TouchSocket.Dmtp.Rpc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
namespace WebApiProxy
{
    public interface IServer : TouchSocket.Rpc.IRemoteServer
    {
        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        System.Int32 Sum(System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default);
        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<System.Int32> SumAsync(System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default);

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        System.Int32 TestPost(MyClass myClass, IInvokeOption invokeOption = default);
        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<System.Int32> TestPostAsync(MyClass myClass, IInvokeOption invokeOption = default);

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        String DownloadFile(System.String id, IInvokeOption invokeOption = default);
        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<String> DownloadFileAsync(System.String id, IInvokeOption invokeOption = default);

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        String PostContent(IInvokeOption invokeOption = default);
        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<String> PostContentAsync(IInvokeOption invokeOption = default);

    }
    public class Server : IServer
    {
        public Server(IRpcClient client)
        {
            this.Client = client;
        }
        public IRpcClient Client { get; private set; }
        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public System.Int32 Sum(System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { a, b };
            System.Int32 returnData = (System.Int32)Client.Invoke(typeof(System.Int32), "GET:/server/sumab?a={0}&b={1}", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///无注释信息
        ///</summary>
        public async Task<System.Int32> SumAsync(System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { a, b };
            return (System.Int32)await Client.InvokeAsync(typeof(System.Int32), "GET:/server/sumab?a={0}&b={1}", invokeOption, parameters);
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public System.Int32 TestPost(MyClass myClass, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { myClass };
            System.Int32 returnData = (System.Int32)Client.Invoke(typeof(System.Int32), "POST:/server/testpost", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///无注释信息
        ///</summary>
        public async Task<System.Int32> TestPostAsync(MyClass myClass, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { myClass };
            return (System.Int32)await Client.InvokeAsync(typeof(System.Int32), "POST:/server/testpost", invokeOption, parameters);
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public String DownloadFile(System.String id, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { id };
            String returnData = (String)Client.Invoke(typeof(String), "GET:/server/downloadfile?id={1}", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///无注释信息
        ///</summary>
        public async Task<String> DownloadFileAsync(System.String id, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { id };
            return (String)await Client.InvokeAsync(typeof(String), "GET:/server/downloadfile?id={1}", invokeOption, parameters);
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public String PostContent(IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            String returnData = (String)Client.Invoke(typeof(String), "POST:/server/postcontent", invokeOption, null);
            return returnData;
        }
        ///<summary>
        ///无注释信息
        ///</summary>
        public async Task<String> PostContentAsync(IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            return (String)await Client.InvokeAsync(typeof(String), "POST:/server/postcontent", invokeOption, null);
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
        public static System.Int32 Sum<TClient>(this TClient client, System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.WebApi.IWebApiClient
        {
            object[] parameters = new object[] { a, b };
            System.Int32 returnData = (System.Int32)client.Invoke(typeof(System.Int32), "GET:/server/sumab?a={0}&b={1}", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///无注释信息
        ///</summary>
        public static async Task<System.Int32> SumAsync<TClient>(this TClient client, System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.WebApi.IWebApiClient
        {
            object[] parameters = new object[] { a, b };
            return (System.Int32)await client.InvokeAsync(typeof(System.Int32), "GET:/server/sumab?a={0}&b={1}", invokeOption, parameters);
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public static System.Int32 TestPost<TClient>(this TClient client, MyClass myClass, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.WebApi.IWebApiClient
        {
            object[] parameters = new object[] { myClass };
            System.Int32 returnData = (System.Int32)client.Invoke(typeof(System.Int32), "POST:/server/testpost", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///无注释信息
        ///</summary>
        public static async Task<System.Int32> TestPostAsync<TClient>(this TClient client, MyClass myClass, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.WebApi.IWebApiClient
        {
            object[] parameters = new object[] { myClass };
            return (System.Int32)await client.InvokeAsync(typeof(System.Int32), "POST:/server/testpost", invokeOption, parameters);
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public static String DownloadFile<TClient>(this TClient client, System.String id, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.WebApi.IWebApiClient
        {
            object[] parameters = new object[] { id };
            String returnData = (String)client.Invoke(typeof(String), "GET:/server/downloadfile?id={1}", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///无注释信息
        ///</summary>
        public static async Task<String> DownloadFileAsync<TClient>(this TClient client, System.String id, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.WebApi.IWebApiClient
        {
            object[] parameters = new object[] { id };
            return (String)await client.InvokeAsync(typeof(String), "GET:/server/downloadfile?id={1}", invokeOption, parameters);
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public static String PostContent<TClient>(this TClient client, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.WebApi.IWebApiClient
        {
            String returnData = (String)client.Invoke(typeof(String), "POST:/server/postcontent", invokeOption, null);
            return returnData;
        }
        ///<summary>
        ///无注释信息
        ///</summary>
        public static async Task<String> PostContentAsync<TClient>(this TClient client, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.WebApi.IWebApiClient
        {
            return (String)await client.InvokeAsync(typeof(String), "POST:/server/postcontent", invokeOption, null);
        }

    }

    public class MyClass
    {
        public System.Int32 A { get; set; }
        public System.Int32 B { get; set; }
    }

}

