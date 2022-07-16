//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Threading.Tasks;
using TouchSocket.Rpc;

namespace RRQM
{
    public interface ITestServerProvider : IRemoteServer
    {
        ///<summary>
        ///登录
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        System.Boolean Login(System.String account, System.String password, IInvokeOption invokeOption = default);

        ///<summary>
        ///登录
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        Task<System.Boolean> LoginAsync(System.String account, System.String password, IInvokeOption invokeOption = default);
    }

    public class TestServerProvider : ITestServerProvider
    {
        public TestServerProvider(IRpcClient client)
        {
            this.Client = client;
        }

        public IRpcClient Client { get; private set; }

        ///<summary>
        ///登录
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public System.Boolean Login(System.String account, System.String password, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { account, password };
            System.Boolean returnData = Client.Invoke<System.Boolean>("testserverprovider/login", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///登录
        ///</summary>
        public Task<System.Boolean> LoginAsync(System.String account, System.String password, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { account, password };
            return Client.InvokeAsync<System.Boolean>("testserverprovider/login", invokeOption, parameters);
        }
    }

    public static class TestServerProviderExtensions
    {
        ///<summary>
        ///登录
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public static System.Boolean Login<TClient>(this TClient client, System.String account, System.String password, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { account, password };
            System.Boolean returnData = client.Invoke<System.Boolean>("testserverprovider/login", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///登录
        ///</summary>
        public static Task<System.Boolean> LoginAsync<TClient>(this TClient client, System.String account, System.String password, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { account, password };
            return client.InvokeAsync<System.Boolean>("testserverprovider/login", invokeOption, parameters);
        }
    }
}