using RRQMCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// ID调用
    /// </summary>
    public interface IIDInvoke
    {
        /// <summary>
        /// RPC调用
        /// </summary>
        /// <param name="id">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Invoke(string id, string method, InvokeOption invokeOption, params object[] parameters);

        /// <summary>
        /// RPC调用
        /// </summary>
        /// <param name="id">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        public T Invoke<T>(string id, string method, InvokeOption invokeOption, params object[] parameters);

    }
}
