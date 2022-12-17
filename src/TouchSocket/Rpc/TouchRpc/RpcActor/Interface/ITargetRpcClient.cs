using System;
using System.Threading.Tasks;
using TouchSocket.Sockets;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// ITargetRpcClient
    /// </summary>
    public interface ITargetRpcClient
    {
        /// <summary>
        /// 调用对应ID的客户端Rpc
        /// </summary>
        /// <param name="targetId">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RpcInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        Task InvokeAsync(string targetId, string method, IInvokeOption invokeOption, params object[] parameters);

        /// <summary>
        /// 调用对应ID的客户端Rpc
        /// </summary>
        /// <param name="targetId">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RpcInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回值</returns>
        Task<T> InvokeAsync<T>(string targetId, string method, IInvokeOption invokeOption, params object[] parameters);

        /// <summary>
        /// 调用对应ID的客户端Rpc
        /// </summary>
        /// <param name="targetId">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RpcInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        void Invoke(string targetId, string method, IInvokeOption invokeOption, params object[] parameters);

        /// <summary>
        /// 调用对应ID的客户端Rpc
        /// </summary>
        /// <param name="targetId">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RpcInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回值</returns>
        T Invoke<T>(string targetId, string method, IInvokeOption invokeOption, params object[] parameters);

        /// <summary>
        /// 调用对应ID的客户端Rpc
        /// </summary>
        /// <param name="targetId">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RpcInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        void Invoke(string targetId, string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types);

        /// <summary>
        /// 调用对应ID的客户端Rpc
        /// </summary>
        /// <param name="targetId">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RpcInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回值</returns>
        T Invoke<T>(string targetId, string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types);
    }
}
