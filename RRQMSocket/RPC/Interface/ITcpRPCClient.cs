//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
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
