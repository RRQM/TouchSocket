//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Threading.Tasks;
using TouchSocket.Rpc;

namespace RRQMProxy
{
    public interface IRpcServer : IRemoteServer
    {
        ///<summary>
        ///测试同步调用
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        System.String TestOne(System.Int32 id, IInvokeOption invokeOption = default);

        ///<summary>
        ///测试同步调用
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        Task<System.String> TestOneAsync(System.Int32 id, IInvokeOption invokeOption = default);

        ///<summary>
        ///测试TestTwo
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        System.String TestTwo(System.Int32 id, IInvokeOption invokeOption = default);

        ///<summary>
        ///测试TestTwo
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        Task<System.String> TestTwoAsync(System.Int32 id, IInvokeOption invokeOption = default);

        ///<summary>
        ///测试Out
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        void TestOut(out System.Int32 id, IInvokeOption invokeOption = default);

        ///<summary>
        ///测试Ref
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        void TestRef(ref System.Int32 id, IInvokeOption invokeOption = default);

        ///<summary>
        ///测试异步
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        System.String AsyncTestOne(System.Int32 id, IInvokeOption invokeOption = default);

        ///<summary>
        ///测试异步
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        Task<System.String> AsyncTestOneAsync(System.Int32 id, IInvokeOption invokeOption = default);

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        void InvokeAll(IInvokeOption invokeOption = default);

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        Task InvokeAllAsync(IInvokeOption invokeOption = default);
    }

    public class RpcServer : IRpcServer
    {
        public RpcServer(IRpcClient client)
        {
            this.Client = client;
        }

        public IRpcClient Client { get; private set; }

        ///<summary>
        ///测试同步调用
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public System.String TestOne(System.Int32 id, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { id };
            System.String returnData = Client.Invoke<System.String>("rpcserver/testone", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///测试同步调用
        ///</summary>
        public Task<System.String> TestOneAsync(System.Int32 id, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { id };
            return Client.InvokeAsync<System.String>("rpcserver/testone", invokeOption, parameters);
        }

        ///<summary>
        ///测试TestTwo
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public System.String TestTwo(System.Int32 id, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { id };
            System.String returnData = Client.Invoke<System.String>("rpcserver/testtwo", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///测试TestTwo
        ///</summary>
        public Task<System.String> TestTwoAsync(System.Int32 id, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { id };
            return Client.InvokeAsync<System.String>("rpcserver/testtwo", invokeOption, parameters);
        }

        ///<summary>
        ///测试Out
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public void TestOut(out System.Int32 id, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { default(System.Int32) };
            Type[] types = new Type[] { typeof(System.Int32) };
            Client.Invoke("rpcserver/testout", invokeOption, ref parameters, types);
            if (parameters != null)
            {
                id = (System.Int32)parameters[0];
            }
            else
            {
                id = default(System.Int32);
            }
        }

        ///<summary>
        ///测试Ref
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public void TestRef(ref System.Int32 id, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { id };
            Type[] types = new Type[] { typeof(System.Int32) };
            Client.Invoke("rpcserver/testref", invokeOption, ref parameters, types);
            if (parameters != null)
            {
                id = (System.Int32)parameters[0];
            }
        }

        ///<summary>
        ///测试异步
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public System.String AsyncTestOne(System.Int32 id, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { id };
            System.String returnData = Client.Invoke<System.String>("rpcserver/asynctestone", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///测试异步
        ///</summary>
        public Task<System.String> AsyncTestOneAsync(System.Int32 id, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { id };
            return Client.InvokeAsync<System.String>("rpcserver/asynctestone", invokeOption, parameters);
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public void InvokeAll(IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            Client.Invoke("rpcserver/invokeall", invokeOption, null);
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        public Task InvokeAllAsync(IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            return Client.InvokeAsync("rpcserver/invokeall", invokeOption, null);
        }
    }

    public static class RpcServerExtensions
    {
        ///<summary>
        ///测试同步调用
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public static System.String TestOne<TClient>(this TClient client, System.Int32 id, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { id };
            System.String returnData = client.Invoke<System.String>("rpcserver/testone", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///测试同步调用
        ///</summary>
        public static Task<System.String> TestOneAsync<TClient>(this TClient client, System.Int32 id, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { id };
            return client.InvokeAsync<System.String>("rpcserver/testone", invokeOption, parameters);
        }

        ///<summary>
        ///测试TestTwo
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public static System.String TestTwo<TClient>(this TClient client, System.Int32 id, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { id };
            System.String returnData = client.Invoke<System.String>("rpcserver/testtwo", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///测试TestTwo
        ///</summary>
        public static Task<System.String> TestTwoAsync<TClient>(this TClient client, System.Int32 id, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { id };
            return client.InvokeAsync<System.String>("rpcserver/testtwo", invokeOption, parameters);
        }

        ///<summary>
        ///测试Out
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public static void TestOut<TClient>(this TClient client, out System.Int32 id, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { default(System.Int32) };
            Type[] types = new Type[] { typeof(System.Int32) };
            client.Invoke("rpcserver/testout", invokeOption, ref parameters, types);
            if (parameters != null)
            {
                id = (System.Int32)parameters[0];
            }
            else
            {
                id = default(System.Int32);
            }
        }

        ///<summary>
        ///测试Ref
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public static void TestRef<TClient>(this TClient client, ref System.Int32 id, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { id };
            Type[] types = new Type[] { typeof(System.Int32) };
            client.Invoke("rpcserver/testref", invokeOption, ref parameters, types);
            if (parameters != null)
            {
                id = (System.Int32)parameters[0];
            }
        }

        ///<summary>
        ///测试异步
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public static System.String AsyncTestOne<TClient>(this TClient client, System.Int32 id, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { id };
            System.String returnData = client.Invoke<System.String>("rpcserver/asynctestone", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///测试异步
        ///</summary>
        public static Task<System.String> AsyncTestOneAsync<TClient>(this TClient client, System.Int32 id, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { id };
            return client.InvokeAsync<System.String>("rpcserver/asynctestone", invokeOption, parameters);
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public static void InvokeAll<TClient>(this TClient client, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            client.Invoke("rpcserver/invokeall", invokeOption, null);
        }

        ///<summary>
        ///无注释信息
        ///</summary>
        public static Task InvokeAllAsync<TClient>(this TClient client, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            return client.InvokeAsync("rpcserver/invokeall", invokeOption, null);
        }
    }

    public interface IPerformanceRpcServer : IRemoteServer
    {
        ///<summary>
        ///测试性能
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        System.String Performance(IInvokeOption invokeOption = default);

        ///<summary>
        ///测试性能
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        Task<System.String> PerformanceAsync(IInvokeOption invokeOption = default);

        ///<summary>
        ///测试并发性能
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        System.Int32 ConPerformance(System.Int32 num, IInvokeOption invokeOption = default);

        ///<summary>
        ///测试并发性能
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        Task<System.Int32> ConPerformanceAsync(System.Int32 num, IInvokeOption invokeOption = default);

        ///<summary>
        ///测试并发性能2
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        System.Int32 ConPerformance2(System.Int32 num, IInvokeOption invokeOption = default);

        ///<summary>
        ///测试并发性能2
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        Task<System.Int32> ConPerformance2Async(System.Int32 num, IInvokeOption invokeOption = default);
    }

    public class PerformanceRpcServer : IPerformanceRpcServer
    {
        public PerformanceRpcServer(IRpcClient client)
        {
            this.Client = client;
        }

        public IRpcClient Client { get; private set; }

        ///<summary>
        ///测试性能
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public System.String Performance(IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            System.String returnData = Client.Invoke<System.String>("performance", invokeOption, null);
            return returnData;
        }

        ///<summary>
        ///测试性能
        ///</summary>
        public Task<System.String> PerformanceAsync(IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            return Client.InvokeAsync<System.String>("performancerpcserver/performance", invokeOption, null);
        }

        ///<summary>
        ///测试并发性能
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public System.Int32 ConPerformance(System.Int32 num, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { num };
            System.Int32 returnData = Client.Invoke<System.Int32>("performancerpcserver/conperformance", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///测试并发性能
        ///</summary>
        public Task<System.Int32> ConPerformanceAsync(System.Int32 num, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { num };
            return Client.InvokeAsync<System.Int32>("performancerpcserver/conperformance", invokeOption, parameters);
        }

        ///<summary>
        ///测试并发性能2
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public System.Int32 ConPerformance2(System.Int32 num, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { num };
            System.Int32 returnData = Client.Invoke<System.Int32>("performancerpcserver/conperformance2", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///测试并发性能2
        ///</summary>
        public Task<System.Int32> ConPerformance2Async(System.Int32 num, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { num };
            return Client.InvokeAsync<System.Int32>("performancerpcserver/conperformance2", invokeOption, parameters);
        }
    }

    public static class PerformanceRpcServerExtensions
    {
        ///<summary>
        ///测试性能
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public static System.String Performance<TClient>(this TClient client, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            System.String returnData = client.Invoke<System.String>("performancerpcserver/performance", invokeOption, null);
            return returnData;
        }

        ///<summary>
        ///测试性能
        ///</summary>
        public static Task<System.String> PerformanceAsync<TClient>(this TClient client, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            return client.InvokeAsync<System.String>("performancerpcserver/performance", invokeOption, null);
        }

        ///<summary>
        ///测试并发性能
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public static System.Int32 ConPerformance<TClient>(this TClient client, System.Int32 num, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { num };
            System.Int32 returnData = client.Invoke<System.Int32>("performancerpcserver/conperformance", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///测试并发性能
        ///</summary>
        public static Task<System.Int32> ConPerformanceAsync<TClient>(this TClient client, System.Int32 num, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { num };
            return client.InvokeAsync<System.Int32>("performancerpcserver/conperformance", invokeOption, parameters);
        }

        ///<summary>
        ///测试并发性能2
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public static System.Int32 ConPerformance2<TClient>(this TClient client, System.Int32 num, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { num };
            System.Int32 returnData = client.Invoke<System.Int32>("performancerpcserver/conperformance2", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///测试并发性能2
        ///</summary>
        public static Task<System.Int32> ConPerformance2Async<TClient>(this TClient client, System.Int32 num, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { num };
            return client.InvokeAsync<System.Int32>("performancerpcserver/conperformance2", invokeOption, parameters);
        }
    }

    public interface IElapsedTimeRpcServer : IRemoteServer
    {
        ///<summary>
        ///测试可取消的调用
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        System.Boolean DelayInvoke(System.Int32 tick, IInvokeOption invokeOption = default);

        ///<summary>
        ///测试可取消的调用
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        Task<System.Boolean> DelayInvokeAsync(System.Int32 tick, IInvokeOption invokeOption = default);
    }

    public class ElapsedTimeRpcServer : IElapsedTimeRpcServer
    {
        public ElapsedTimeRpcServer(IRpcClient client)
        {
            this.Client = client;
        }

        public IRpcClient Client { get; private set; }

        ///<summary>
        ///测试可取消的调用
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public System.Boolean DelayInvoke(System.Int32 tick, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { tick };
            System.Boolean returnData = Client.Invoke<System.Boolean>("elapsedtimerpcserver/delayinvoke", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///测试可取消的调用
        ///</summary>
        public Task<System.Boolean> DelayInvokeAsync(System.Int32 tick, IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { tick };
            return Client.InvokeAsync<System.Boolean>("elapsedtimerpcserver/delayinvoke", invokeOption, parameters);
        }
    }

    public static class ElapsedTimeRpcServerExtensions
    {
        ///<summary>
        ///测试可取消的调用
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public static System.Boolean DelayInvoke<TClient>(this TClient client, System.Int32 tick, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { tick };
            System.Boolean returnData = client.Invoke<System.Boolean>("elapsedtimerpcserver/delayinvoke", invokeOption, parameters);
            return returnData;
        }

        ///<summary>
        ///测试可取消的调用
        ///</summary>
        public static Task<System.Boolean> DelayInvokeAsync<TClient>(this TClient client, System.Int32 tick, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            object[] parameters = new object[] { tick };
            return client.InvokeAsync<System.Boolean>("elapsedtimerpcserver/delayinvoke", invokeOption, parameters);
        }
    }

    public interface IInstanceRpcServer : IRemoteServer
    {
        ///<summary>
        ///测试调用实例
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        System.Int32 Increment(IInvokeOption invokeOption = default);

        ///<summary>
        ///测试调用实例
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        Task<System.Int32> IncrementAsync(IInvokeOption invokeOption = default);
    }

    public class InstanceRpcServer : IInstanceRpcServer
    {
        public InstanceRpcServer(IRpcClient client)
        {
            this.Client = client;
        }

        public IRpcClient Client { get; private set; }

        ///<summary>
        ///测试调用实例
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public System.Int32 Increment(IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            System.Int32 returnData = Client.Invoke<System.Int32>("instancerpcserver/increment", invokeOption, null);
            return returnData;
        }

        ///<summary>
        ///测试调用实例
        ///</summary>
        public Task<System.Int32> IncrementAsync(IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            return Client.InvokeAsync<System.Int32>("instancerpcserver/increment", invokeOption, null);
        }
    }

    public static class InstanceRpcServerExtensions
    {
        ///<summary>
        ///测试调用实例
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public static System.Int32 Increment<TClient>(this TClient client, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            System.Int32 returnData = client.Invoke<System.Int32>("instancerpcserver/increment", invokeOption, null);
            return returnData;
        }

        ///<summary>
        ///测试调用实例
        ///</summary>
        public static Task<System.Int32> IncrementAsync<TClient>(this TClient client, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            return client.InvokeAsync<System.Int32>("instancerpcserver/increment", invokeOption, null);
        }
    }

    public interface IGetCallerRpcServer : IRemoteServer
    {
        ///<summary>
        ///测试调用上下文
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        System.String GetCallerID(IInvokeOption invokeOption = default);

        ///<summary>
        ///测试调用上下文
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        Task<System.String> GetCallerIDAsync(IInvokeOption invokeOption = default);
    }

    public class GetCallerRpcServer : IGetCallerRpcServer
    {
        public GetCallerRpcServer(IRpcClient client)
        {
            this.Client = client;
        }

        public IRpcClient Client { get; private set; }

        ///<summary>
        ///测试调用上下文
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public System.String GetCallerID(IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            System.String returnData = Client.Invoke<System.String>("getcallerrpcserver/getcallerid", invokeOption, null);
            return returnData;
        }

        ///<summary>
        ///测试调用上下文
        ///</summary>
        public Task<System.String> GetCallerIDAsync(IInvokeOption invokeOption = default)
        {
            if (Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            return Client.InvokeAsync<System.String>("getcallerrpcserver/getcallerid", invokeOption, null);
        }
    }

    public static class GetCallerRpcServerExtensions
    {
        ///<summary>
        ///测试调用上下文
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSocket.RPC.RRQMRpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="RRQMCore.RRQMException">其他异常</exception>
        public static System.String GetCallerID<TClient>(this TClient client, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            System.String returnData = client.Invoke<System.String>("getcallerrpcserver/getcallerid", invokeOption, null);
            return returnData;
        }

        ///<summary>
        ///测试调用上下文
        ///</summary>
        public static Task<System.String> GetCallerIDAsync<TClient>(this TClient client, IInvokeOption invokeOption = default) where TClient :
        IRpcClient
        {
            if (client.TryCanInvoke?.Invoke(client) == false)
            {
                throw new RpcException("Rpc无法执行。");
            }
            return client.InvokeAsync<System.String>("getcallerrpcserver/getcallerid", invokeOption, null);
        }
    }
}