using System.Threading.Tasks;
using TouchSocket.Rpc;
using TouchSocket.Rpc.WebApi;

namespace RRQMProxy
{
    public interface IServer : IRemoteServer
    {
        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        System.Int32 Sum(System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default);

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        Task<System.Int32> SumAsync(System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default);

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        System.Int32 TestPost(MyClass myClass, IInvokeOption invokeOption = default);

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        Task<System.Int32> TestPostAsync(MyClass myClass, IInvokeOption invokeOption = default);
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
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public System.Int32 Sum(System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { a, b };
            System.Int32 returnData = Client.Invoke<System.Int32>("GET:/server/sum?a={0}&b={1}", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        public Task<System.Int32> SumAsync(System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { a, b };
            return Client.InvokeAsync<System.Int32>("GET:/server/sum?a={0}&b={1}", invokeOption, parameters);
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public System.Int32 TestPost(MyClass myClass, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { myClass };
            System.Int32 returnData = Client.Invoke<System.Int32>("POST:/server/testpost", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        public Task<System.Int32> TestPostAsync(MyClass myClass, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { myClass };
            return Client.InvokeAsync<System.Int32>("POST:/server/testpost", invokeOption, parameters);
        }
    }

    public static class ServerExtensions
    {
        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public static System.Int32 Sum<TClient>(this TClient client, System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default) where TClient :
        IWebApiClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { a, b };
            System.Int32 returnData = client.Invoke<System.Int32>("GET:/server/sum?a={0}&b={1}", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        public static Task<System.Int32> SumAsync<TClient>(this TClient client, System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default) where TClient :
        IWebApiClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { a, b };
            return client.InvokeAsync<System.Int32>("GET:/server/sum?a={0}&b={1}", invokeOption, parameters);
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public static System.Int32 TestPost<TClient>(this TClient client, MyClass myClass, IInvokeOption invokeOption = default) where TClient :
        IWebApiClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { myClass };
            System.Int32 returnData = client.Invoke<System.Int32>("POST:/server/testpost", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        public static Task<System.Int32> TestPostAsync<TClient>(this TClient client, MyClass myClass, IInvokeOption invokeOption = default) where TClient :
        IWebApiClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { myClass };
            return client.InvokeAsync<System.Int32>("POST:/server/testpost", invokeOption, parameters);
        }
    }

    public class MyClass
    {
        public System.Int32 A { get; set; }
        public System.Int32 B { get; set; }
    }
}