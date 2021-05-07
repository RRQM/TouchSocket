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
    public abstract class RPCParser : IDisposable
    {
        /// <summary>
        /// 获取函数映射图
        /// </summary>
        protected MethodMap MethodMap { get; private set; }

        /// <summary>
        /// 包含此解析器的服务器实例
        /// </summary>
        public RPCService RPCService { get; internal set; }

        internal Action<RPCParser, MethodInvoker, MethodInstance> RRQMExecuteMethod;

        internal void RRQMInitializeServers(MethodInstance[] methodInstances)
        {
            InitializeServers(methodInstances);
        }

        internal void RRQMEndInvokeMethod(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            EndInvokeMethod(methodInvoker, methodInstance);
        }

        internal void RRQMSetMethodMap(MethodMap methodMap)
        {
            this.MethodMap = methodMap;
        }

        /// <summary>
        /// 初始化服务
        /// </summary>
        /// <param name="methodInstances"></param>
        protected abstract void InitializeServers(MethodInstance[] methodInstances);

        /// <summary>
        /// 在函数调用完成后调用
        /// </summary>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        protected abstract void EndInvokeMethod(MethodInvoker methodInvoker, MethodInstance methodInstance);

        /// <summary>
        /// 执行函数
        /// </summary>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        protected void ExecuteMethod(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            RRQMExecuteMethod?.Invoke(this, methodInvoker, methodInstance);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public abstract void Dispose();
    }
}