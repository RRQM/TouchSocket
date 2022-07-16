using System;
using System.Threading.Tasks;
using TouchSocket.Rpc;

namespace RRQMProxy
{
    public interface IServer : IRemoteServer
    {
        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">Rpc异常</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        System.Int32 Sum(System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default);

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">Rpc异常</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        Task<System.Int32> SumAsync(System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default);

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">Rpc异常</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        System.Int32 TestClass(MyClass myClass, IInvokeOption invokeOption = default);

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">Rpc异常</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        Task<System.Int32> TestClassAsync(MyClass myClass, IInvokeOption invokeOption = default);
    }

    public class Server : IServer
    {
        public Server(IRpcClient client)
        {
            this.Client = client;
        }

        public IRpcClient Client { get; private set; }

        ///<summary>
        ///<inheritdoc/>
        ///</summary>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">Rpc异常</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        public System.Int32 Sum(System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { a, b };
            System.Int32 returnData = Client.Invoke<System.Int32>("Sum", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///<inheritdoc/>
        ///</summary>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">Rpc异常</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        public Task<System.Int32> SumAsync(System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { a, b };
            return Client.InvokeAsync<System.Int32>("Sum", invokeOption, parameters);
        }

        ///<summary>
        ///<inheritdoc/>
        ///</summary>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">Rpc异常</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        public System.Int32 TestClass(MyClass myClass, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { myClass };
            System.Int32 returnData = Client.Invoke<System.Int32>("TestClass", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///<inheritdoc/>
        ///</summary>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">Rpc异常</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        public Task<System.Int32> TestClassAsync(MyClass myClass, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { myClass };
            return Client.InvokeAsync<System.Int32>("TestClass", invokeOption, parameters);
        }
    }

    public class MyClass
    {
        public System.Int32 A { get; set; }
        public System.Int32 B { get; set; }
    }
}