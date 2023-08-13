using
/* 项目“DmtpRpcClientConsoleApp”的未合并的更改
在此之前:
using TouchSocket.Core;
using TouchSocket.Sockets;
using TouchSocket.Rpc;
using TouchSocket.Dmtp.Rpc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
在此之后:
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
*/
TouchSocket.Rpc;
namespace RpcProxy
{
    public interface IMyRpcServer : TouchSocket.Rpc.IRemoteServer
    {
        ///<summary>
        ///将两个数相加
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        System.Int32 Add(System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default);
        ///<summary>
        ///将两个数相加
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<System.Int32> AddAsync(System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default);

        ///<summary>
        ///测试客户端请求，服务器响应大量流数据
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        System.Int32 RpcPullChannel(System.Int32 channelID, IInvokeOption invokeOption = default);
        ///<summary>
        ///测试客户端请求，服务器响应大量流数据
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<System.Int32> RpcPullChannelAsync(System.Int32 channelID, IInvokeOption invokeOption = default);

        ///<summary>
        ///测试客户端推送流数据
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        System.Int32 RpcPushChannel(System.Int32 channelID, IInvokeOption invokeOption = default);
        ///<summary>
        ///测试客户端推送流数据
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<System.Int32> RpcPushChannelAsync(System.Int32 channelID, IInvokeOption invokeOption = default);

    }
    public class MyRpcServer : IMyRpcServer
    {
        public MyRpcServer(IRpcClient client)
        {
            this.Client = client;
        }
        public IRpcClient Client { get; private set; }
        ///<summary>
        ///将两个数相加
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public System.Int32 Add(System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            var parameters = new object[] { a, b };
            var returnData = (System.Int32)this.Client.Invoke(typeof(System.Int32), "Add", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///将两个数相加
        ///</summary>
        public async Task<System.Int32> AddAsync(System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            var parameters = new object[] { a, b };
            return (System.Int32)await this.Client.InvokeAsync(typeof(System.Int32), "Add", invokeOption, parameters);
        }

        ///<summary>
        ///测试客户端请求，服务器响应大量流数据
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public System.Int32 RpcPullChannel(System.Int32 channelID, IInvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            var parameters = new object[] { channelID };
            var returnData = (System.Int32)this.Client.Invoke(typeof(System.Int32), "consoleapp2.program+myrpcserver.rpcpullchannel", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///测试客户端请求，服务器响应大量流数据
        ///</summary>
        public async Task<System.Int32> RpcPullChannelAsync(System.Int32 channelID, IInvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            var parameters = new object[] { channelID };
            return (System.Int32)await this.Client.InvokeAsync(typeof(System.Int32), "consoleapp2.program+myrpcserver.rpcpullchannel", invokeOption, parameters);
        }

        ///<summary>
        ///测试客户端推送流数据
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public System.Int32 RpcPushChannel(System.Int32 channelID, IInvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            var parameters = new object[] { channelID };
            var returnData = (System.Int32)this.Client.Invoke(typeof(System.Int32), "consoleapp2.program+myrpcserver.rpcpushchannel", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///测试客户端推送流数据
        ///</summary>
        public async Task<System.Int32> RpcPushChannelAsync(System.Int32 channelID, IInvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            var parameters = new object[] { channelID };
            return (System.Int32)await this.Client.InvokeAsync(typeof(System.Int32), "consoleapp2.program+myrpcserver.rpcpushchannel", invokeOption, parameters);
        }

    }
    public static class MyRpcServerExtensions
    {
        ///<summary>
        ///将两个数相加
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public static System.Int32 Add<TClient>(this TClient client, System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.IRpcClient
        {
            var parameters = new object[] { a, b };
            var returnData = (System.Int32)client.Invoke(typeof(System.Int32), "Add", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///将两个数相加
        ///</summary>
        public static async Task<System.Int32> AddAsync<TClient>(this TClient client, System.Int32 a, System.Int32 b, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.IRpcClient
        {
            var parameters = new object[] { a, b };
            return (System.Int32)await client.InvokeAsync(typeof(System.Int32), "Add", invokeOption, parameters);
        }

        ///<summary>
        ///测试客户端请求，服务器响应大量流数据
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public static System.Int32 RpcPullChannel<TClient>(this TClient client, System.Int32 channelID, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.IRpcClient
        {
            var parameters = new object[] { channelID };
            var returnData = (System.Int32)client.Invoke(typeof(System.Int32), "consoleapp2.program+myrpcserver.rpcpullchannel", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///测试客户端请求，服务器响应大量流数据
        ///</summary>
        public static async Task<System.Int32> RpcPullChannelAsync<TClient>(this TClient client, System.Int32 channelID, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.IRpcClient
        {
            var parameters = new object[] { channelID };
            return (System.Int32)await client.InvokeAsync(typeof(System.Int32), "consoleapp2.program+myrpcserver.rpcpullchannel", invokeOption, parameters);
        }

        ///<summary>
        ///测试客户端推送流数据
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public static System.Int32 RpcPushChannel<TClient>(this TClient client, System.Int32 channelID, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.IRpcClient
        {
            var parameters = new object[] { channelID };
            var returnData = (System.Int32)client.Invoke(typeof(System.Int32), "consoleapp2.program+myrpcserver.rpcpushchannel", invokeOption, parameters);
            return returnData;
        }
        ///<summary>
        ///测试客户端推送流数据
        ///</summary>
        public static async Task<System.Int32> RpcPushChannelAsync<TClient>(this TClient client, System.Int32 channelID, IInvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.IRpcClient
        {
            var parameters = new object[] { channelID };
            return (System.Int32)await client.InvokeAsync(typeof(System.Int32), "consoleapp2.program+myrpcserver.rpcpushchannel", invokeOption, parameters);
        }

    }
}
