using System.Threading.Tasks;
using TouchSocket.Rpc;

namespace RpcProxy
{
    public interface IXmlServer : TouchSocket.Rpc.IRemoteServer
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
        System.Int32 TestClass(MyClass myClass, IInvokeOption invokeOption = default);
        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<System.Int32> TestClassAsync(MyClass myClass, IInvokeOption invokeOption = default);

    }
    public class XmlServer : IXmlServer
    {
        public XmlServer(IRpcClient client)
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
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            var parameters = new object[] { a, b };
            var returnData = (System.Int32)this.Client.Invoke(typeof(System.Int32), "Sum", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///无注释信息
        ///</summary>
        public async Task<System.Int32> SumAsync(System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            var parameters = new object[] { a, b };
            return (System.Int32)await this.Client.InvokeAsync(typeof(System.Int32), "Sum", invokeOption, parameters);
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public System.Int32 TestClass(MyClass myClass, IInvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            var parameters = new object[] { myClass };
            var returnData = (System.Int32)this.Client.Invoke(typeof(System.Int32), "TestClass", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///无注释信息
        ///</summary>
        public async Task<System.Int32> TestClassAsync(MyClass myClass, IInvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            var parameters = new object[] { myClass };
            return (System.Int32)await this.Client.InvokeAsync(typeof(System.Int32), "TestClass", invokeOption, parameters);
        }

    }
    public static class XmlServerExtensions
    {
        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public static System.Int32 Sum<TClient>(this TClient client, System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.XmlRpc.IXmlRpcClient
        {
            var parameters = new object[] { a, b };
            var returnData = (System.Int32)client.Invoke(typeof(System.Int32), "Sum", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///无注释信息
        ///</summary>
        public static async Task<System.Int32> SumAsync<TClient>(this TClient client, System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.XmlRpc.IXmlRpcClient
        {
            var parameters = new object[] { a, b };
            return (System.Int32)await client.InvokeAsync(typeof(System.Int32), "Sum", invokeOption, parameters);
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public static System.Int32 TestClass<TClient>(this TClient client, MyClass myClass, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.XmlRpc.IXmlRpcClient
        {
            var parameters = new object[] { myClass };
            var returnData = (System.Int32)client.Invoke(typeof(System.Int32), "TestClass", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///无注释信息
        ///</summary>
        public static async Task<System.Int32> TestClassAsync<TClient>(this TClient client, MyClass myClass, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.XmlRpc.IXmlRpcClient
        {
            var parameters = new object[] { myClass };
            return (System.Int32)await client.InvokeAsync(typeof(System.Int32), "TestClass", invokeOption, parameters);
        }

    }

    public class MyClass
    {
        public System.Int32 A { get; set; }
        public System.Int32 B { get; set; }
    }

}
