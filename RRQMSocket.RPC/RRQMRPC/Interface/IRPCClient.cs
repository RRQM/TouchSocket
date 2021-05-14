//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.Log;
using System;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// 客户端RPC接口
    /// </summary>
    public interface IRPCClient : IDisposable
    {
        /// <summary>
        /// 收到ByteBlock时触发
        /// </summary>
        event RRQMByteBlockEventHandler ReceivedByteBlock;

        /// <summary>
        /// 收到字节数组并返回
        /// </summary>
        event RRQMBytesEventHandler ReceivedBytesThenReturn;

        /// <summary>
        /// 获取ID
        /// </summary>
        string ID { get; }

        /// <summary>
        /// 日志记录器
        /// </summary>
        ILog Logger { get; set; }

        /// <summary>
        /// 获取内存池实例
        /// </summary>
        BytePool BytePool { get; }

        /// <summary>
        /// 序列化生成器
        /// </summary>
        SerializeConverter SerializeConverter { get; set; }

        /// <summary>
        /// 获取远程服务器RPC服务文件
        /// </summary>
        /// <param name="ipHost">IP和端口</param>
        /// <param name="verifyToken">连接验证</param>
        /// <param name="proxyToken">代理令箭</param>
        /// <returns></returns>
        /// <exception cref="RRQMRPCException"></exception>
        /// <exception cref="RRQMTimeoutException"></exception>
        RPCProxyInfo GetProxyInfo(IPHost ipHost, string verifyToken = null, string proxyToken = null);

        /// <summary>
        /// 初始化RPC
        /// </summary>
        /// <param name="ipHost"></param>
        /// <param name="verifyToken"></param>
        /// <param name="typeDic"></param>
        void InitializedRPC(IPHost ipHost, string verifyToken = null, TypeInitializeDic typeDic = null);

        /// <summary>
        /// 函数式调用
        /// </summary>
        /// <param name="method">函数名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">RPC调用设置</param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        void RPCInvoke(string method, InvokeOption invokeOption = null, params object[] parameters);

        /// <summary>
        /// 函数式调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">RPC调用设置</param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns>服务器返回结果</returns>
        T RPCInvoke<T>(string method, InvokeOption invokeOption = null, params object[] parameters);

        /// <summary>
        /// 函数式调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">RPC调用设置</param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns>服务器返回结果</returns>
        object Invoke(string method, InvokeOption invokeOption, ref object[] parameters);
    }
}