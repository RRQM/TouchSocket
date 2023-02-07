using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using TouchSocket.Rpc;

namespace JsonRpcProxy
{
    public interface IJsonRpcServer : IRemoteServer
    {
        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        System.String TestGetContext(System.String str, IInvokeOption invokeOption = default);

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<System.String> TestGetContextAsync(System.String str, IInvokeOption invokeOption = default);

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Newtonsoft.Json.Linq.JObject TestJObject(Newtonsoft.Json.Linq.JObject obj, IInvokeOption invokeOption = default);

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<Newtonsoft.Json.Linq.JObject> TestJObjectAsync(Newtonsoft.Json.Linq.JObject obj, IInvokeOption invokeOption = default);

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        System.String TestJsonRpc(System.String str, IInvokeOption invokeOption = default);

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<System.String> TestJsonRpcAsync(System.String str, IInvokeOption invokeOption = default);

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        System.String TestJsonRpc1(System.String str, IInvokeOption invokeOption = default);

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<System.String> TestJsonRpc1Async(System.String str, IInvokeOption invokeOption = default);
    }

    public class JsonRpcServer : IJsonRpcServer
    {
        public JsonRpcServer(IRpcClient client)
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
        public System.String TestGetContext(System.String str, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            if (Client.TryCanInvoke?.Invoke(Client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { str };
            System.String returnData = Client.Invoke<System.String>("jsonrpcconsoleapp.jsonrpcserver.testgetcontext", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        public Task<System.String> TestGetContextAsync(System.String str, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            if (Client.TryCanInvoke?.Invoke(Client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { str };
            return Client.InvokeAsync<System.String>("jsonrpcconsoleapp.jsonrpcserver.testgetcontext", invokeOption, parameters);
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public Newtonsoft.Json.Linq.JObject TestJObject(Newtonsoft.Json.Linq.JObject obj, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            if (Client.TryCanInvoke?.Invoke(Client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { obj };
            Newtonsoft.Json.Linq.JObject returnData = Client.Invoke<Newtonsoft.Json.Linq.JObject>("jsonrpcconsoleapp.jsonrpcserver.testjobject", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        public Task<Newtonsoft.Json.Linq.JObject> TestJObjectAsync(Newtonsoft.Json.Linq.JObject obj, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            if (Client.TryCanInvoke?.Invoke(Client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { obj };
            return Client.InvokeAsync<Newtonsoft.Json.Linq.JObject>("jsonrpcconsoleapp.jsonrpcserver.testjobject", invokeOption, parameters);
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public System.String TestJsonRpc(System.String str, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            if (Client.TryCanInvoke?.Invoke(Client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { str };
            System.String returnData = Client.Invoke<System.String>("jsonrpcconsoleapp.jsonrpcserver.testjsonrpc", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        public Task<System.String> TestJsonRpcAsync(System.String str, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            if (Client.TryCanInvoke?.Invoke(Client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { str };
            return Client.InvokeAsync<System.String>("jsonrpcconsoleapp.jsonrpcserver.testjsonrpc", invokeOption, parameters);
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public System.String TestJsonRpc1(System.String str, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            if (Client.TryCanInvoke?.Invoke(Client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { str };
            System.String returnData = Client.Invoke<System.String>("TestJsonRpc1", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        public Task<System.String> TestJsonRpc1Async(System.String str, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            if (Client.TryCanInvoke?.Invoke(Client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { str };
            return Client.InvokeAsync<System.String>("TestJsonRpc1", invokeOption, parameters);
        }
    }

    public static class JsonRpcServerExtensions
    {
        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public static System.String TestGetContext<TClient>(this TClient client, System.String str, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.JsonRpc.IJsonRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { str };
            System.String returnData = client.Invoke<System.String>("jsonrpcconsoleapp.jsonrpcserver.testgetcontext", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        public static Task<System.String> TestGetContextAsync<TClient>(this TClient client, System.String str, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.JsonRpc.IJsonRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { str };
            return client.InvokeAsync<System.String>("jsonrpcconsoleapp.jsonrpcserver.testgetcontext", invokeOption, parameters);
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public static Newtonsoft.Json.Linq.JObject TestJObject<TClient>(this TClient client, Newtonsoft.Json.Linq.JObject obj, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.JsonRpc.IJsonRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { obj };
            JObject returnData = client.Invoke<JObject>("jsonrpcconsoleapp.jsonrpcserver.testjobject", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        public static Task<JObject> TestJObjectAsync<TClient>(this TClient client, Newtonsoft.Json.Linq.JObject obj, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.JsonRpc.IJsonRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { obj };
            return client.InvokeAsync<JObject>("jsonrpcconsoleapp.jsonrpcserver.testjobject", invokeOption, parameters);
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public static System.String TestJsonRpc<TClient>(this TClient client, System.String str, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.JsonRpc.IJsonRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { str };
            System.String returnData = client.Invoke<System.String>("jsonrpcconsoleapp.jsonrpcserver.testjsonrpc", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        public static Task<System.String> TestJsonRpcAsync<TClient>(this TClient client, System.String str, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.JsonRpc.IJsonRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { str };
            return client.InvokeAsync<System.String>("jsonrpcconsoleapp.jsonrpcserver.testjsonrpc", invokeOption, parameters);
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public static System.String TestJsonRpc1<TClient>(this TClient client, System.String str, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.JsonRpc.IJsonRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { str };
            System.String returnData = client.Invoke<System.String>("TestJsonRpc1", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        public static Task<System.String> TestJsonRpc1Async<TClient>(this TClient client, System.String str, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.JsonRpc.IJsonRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { str };
            return client.InvokeAsync<System.String>("TestJsonRpc1", invokeOption, parameters);
        }
    }
}