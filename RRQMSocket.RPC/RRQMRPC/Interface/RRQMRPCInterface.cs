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
using RRQMCore;
using RRQMCore.Log;
using System;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// Rpc辅助类接口
    /// </summary>
    public interface ITcpRpcClientBase : IProtocolClientBase, IRpcClient, IIDInvoke
    {

    }

    /// <summary>
    /// RRQMRPC接口
    /// </summary>
    public interface IRRQMRPCParser
    {
        /// <summary>
        /// 代理令箭，当客户端获取代理文件时需验证令箭
        /// </summary>
        string ProxyToken { get; }

        /// <summary>
        /// 函数仓库
        /// </summary>
        MethodStore MethodStore { get; }

        /// <summary>
        /// 序列化选择器
        /// </summary>
        SerializationSelector SerializationSelector { get; }

        /// <summary>
        /// 获取注册函数
        /// </summary>
        /// <param name="proxyToken"></param>
        /// <param name="caller">调用作用者/></param>
        /// <returns></returns>
        MethodItem[] GetRegisteredMethodItems(string proxyToken, object caller);
    }

    interface ITcpRpcClient : ITcpRpcClientBase, IRpcParser
    {

    }

    /// <summary>
    /// 客户端Rpc接口
    /// </summary>
    public interface IRRQMRPCClient : IRpcClient
    {
        /// <summary>
        /// 获取ID
        /// </summary>
        string ID { get; }

        /// <summary>
        /// 日志记录器
        /// </summary>
        ILog Logger { get; }

        /// <summary>
        /// 序列化生成器
        /// </summary>
        SerializationSelector SerializationSelector { get; }

        /// <summary>
        ///  发现服务
        /// </summary>
        /// <param name="proxyToken">代理令箭</param>
        /// <param name="token"></param>
        /// <returns>发现的服务</returns>
        MethodItem[] DiscoveryService(string proxyToken, System.Threading.CancellationToken token = default);
    }

    /// <summary>
    /// ID调用
    /// </summary>
    public interface IIDInvoke
    {
        /// <summary>
        /// 反向调用客户端Rpc
        /// </summary>
        /// <param name="id">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        Task InvokeAsync(string id, string method, IInvokeOption invokeOption, params object[] parameters);

        /// <summary>
        /// 反向调用客户端Rpc
        /// </summary>
        /// <param name="id">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        /// <returns>返回值</returns>
        Task<T> InvokeAsync<T>(string id, string method, IInvokeOption invokeOption, params object[] parameters);

        /// <summary>
        /// 反向调用客户端Rpc
        /// </summary>
        /// <param name="id">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        void Invoke(string id, string method, IInvokeOption invokeOption, params object[] parameters);

        /// <summary>
        /// 反向调用客户端Rpc
        /// </summary>
        /// <param name="id">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        /// <returns>返回值</returns>
        T Invoke<T>(string id, string method, IInvokeOption invokeOption, params object[] parameters);
    }
}
