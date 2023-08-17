using System.Threading.Tasks;
using TouchSocket.Rpc;
namespace RpcProxy
{
    public interface IMyRpcServer : TouchSocket.Rpc.IRemoteServer
    {
        ///<summary>
        ///登录
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        System.Boolean Login(System.String account, System.String password, IInvokeOption invokeOption = default);
        ///<summary>
        ///登录
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<System.Boolean> LoginAsync(System.String account, System.String password, IInvokeOption invokeOption = default);

        ///<summary>
        ///注册
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        System.Boolean Register(RegisterModel register, IInvokeOption invokeOption = default);
        ///<summary>
        ///注册
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<System.Boolean> RegisterAsync(RegisterModel register, IInvokeOption invokeOption = default);

        ///<summary>
        ///性能测试
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        System.Int32 Performance(System.Int32 a, IInvokeOption invokeOption = default);
        ///<summary>
        ///性能测试
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<System.Int32> PerformanceAsync(System.Int32 a, IInvokeOption invokeOption = default);

    }
    public class MyRpcServer : IMyRpcServer
    {
        public MyRpcServer(IRpcClient client)
        {
            this.Client = client;
        }
        public IRpcClient Client { get; private set; }
        ///<summary>
        ///登录
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public System.Boolean Login(System.String account, System.String password, IInvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            var parameters = new object[] { account, password };
            var returnData = (System.Boolean)this.Client.Invoke(typeof(System.Boolean), "Login", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///登录
        ///</summary>
        public async Task<System.Boolean> LoginAsync(System.String account, System.String password, IInvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            var parameters = new object[] { account, password };
            return (System.Boolean)await this.Client.InvokeAsync(typeof(System.Boolean), "Login", invokeOption, parameters);
        }

        ///<summary>
        ///注册
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public System.Boolean Register(RegisterModel register, IInvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            var parameters = new object[] { register };
            var returnData = (System.Boolean)this.Client.Invoke(typeof(System.Boolean), "Register", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///注册
        ///</summary>
        public async Task<System.Boolean> RegisterAsync(RegisterModel register, IInvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            var parameters = new object[] { register };
            return (System.Boolean)await this.Client.InvokeAsync(typeof(System.Boolean), "Register", invokeOption, parameters);
        }

        ///<summary>
        ///性能测试
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public System.Int32 Performance(System.Int32 a, IInvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            var parameters = new object[] { a };
            var returnData = (System.Int32)this.Client.Invoke(typeof(System.Int32), "Performance", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///性能测试
        ///</summary>
        public async Task<System.Int32> PerformanceAsync(System.Int32 a, IInvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            var parameters = new object[] { a };
            return (System.Int32)await this.Client.InvokeAsync(typeof(System.Int32), "Performance", invokeOption, parameters);
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
        public static System.Boolean Login<TClient>(this TClient client, System.String account, System.String password, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.IRpcClient
        {
            var parameters = new object[] { account, password };
            var returnData = (System.Boolean)client.Invoke(typeof(System.Boolean), "Login", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///登录
        ///</summary>
        public static async Task<System.Boolean> LoginAsync<TClient>(this TClient client, System.String account, System.String password, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.IRpcClient
        {
            var parameters = new object[] { account, password };
            return (System.Boolean)await client.InvokeAsync(typeof(System.Boolean), "Login", invokeOption, parameters);
        }

        ///<summary>
        ///注册
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public static System.Boolean Register<TClient>(this TClient client, RegisterModel register, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.IRpcClient
        {
            var parameters = new object[] { register };
            var returnData = (System.Boolean)client.Invoke(typeof(System.Boolean), "Register", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///注册
        ///</summary>
        public static async Task<System.Boolean> RegisterAsync<TClient>(this TClient client, RegisterModel register, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.IRpcClient
        {
            var parameters = new object[] { register };
            return (System.Boolean)await client.InvokeAsync(typeof(System.Boolean), "Register", invokeOption, parameters);
        }

        ///<summary>
        ///性能测试
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public static System.Int32 Performance<TClient>(this TClient client, System.Int32 a, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.IRpcClient
        {
            var parameters = new object[] { a };
            var returnData = (System.Int32)client.Invoke(typeof(System.Int32), "Performance", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///性能测试
        ///</summary>
        public static async Task<System.Int32> PerformanceAsync<TClient>(this TClient client, System.Int32 a, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.IRpcClient
        {
            var parameters = new object[] { a };
            return (System.Int32)await client.InvokeAsync(typeof(System.Int32), "Performance", invokeOption, parameters);
        }

    }

    public class RegisterModel
    {
        public System.String Account { get; set; }
        public System.String Password { get; set; }
        public System.Int32 Id { get; set; }
    }

}
