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
using System;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// RPC解析器
    /// </summary>
    public interface IRPCParser : IDisposable
    {
        /// <summary>
        /// 获取函数映射图
        /// </summary>
        MethodMap MethodMap { get; }

        /// <summary>
        /// 包含此解析器的服务器实例
        /// </summary>
        RPCService RPCService { get; }

        /// <summary>
        /// 执行函数
        /// </summary>
        Action<IRPCParser, MethodInvoker, MethodInstance> RRQMExecuteMethod { get; }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="methodInstances"></param>
        void OnRegisterServer(ServerProvider provider, MethodInstance[] methodInstances);

        /// <summary>
        /// 取消注册服务
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="methodInstances"></param>
        void OnUnregisterServer(ServerProvider provider, MethodInstance[] methodInstances);

        /// <summary>
        /// 结束调用
        /// </summary>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        void OnEndInvoke(MethodInvoker methodInvoker, MethodInstance methodInstance);

        /// <summary>
        /// 设置函数映射
        /// </summary>
        /// <param name="methodMap"></param>
        void SetMethodMap(MethodMap methodMap);

        /// <summary>
        /// 设置服务
        /// </summary>
        /// <param name="service"></param>
        void SetRPCService(RPCService service);

        /// <summary>
        /// 设置执行函数
        /// </summary>
        /// <param name="executeMethod"></param>
        void SetExecuteMethod(Action<IRPCParser, MethodInvoker, MethodInstance> executeMethod);
    }
}