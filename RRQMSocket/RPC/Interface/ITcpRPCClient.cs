using RRQMCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// tcp协议RPC客户端
    /// </summary>
    public interface ITcpRPCClient : IRPCClient
    {
        /// <summary>
        /// 获取IDToken
        /// </summary>
        string IDToken { get; }

        /// <summary>
        /// 收到ByteBlock时触发
        /// </summary>
        event RRQMByteBlockEventHandler ReceivedByteBlock;

        /// <summary>
        /// 收到字节数组并返回
        /// </summary>
        event RRQMBytesEventHandler ReceivedBytesThenReturn;

        /// <summary>
        /// 函数式调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="waitTime">等待时间（秒）</param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns>服务器返回结果</returns>
        T RPCInvoke<T>(string method, ref object[] parameters, int waitTime = 3);
    }
}
