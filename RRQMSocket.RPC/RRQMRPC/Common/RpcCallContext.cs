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
using RRQMCore.Serialization;
using System.Threading;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// RRQMRPC上下文
    /// </summary>
    public class RpcCallContext : ICallContext
    {
        private ICaller caller;
        private CancellationTokenSource tokenSource;
        private RpcContext context;
        private MethodInstance methodInstance;
        private MethodInvoker methodInvoker;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="context"></param>
        /// <param name="methodInstance"></param>
        /// <param name="methodInvoker"></param>
        public RpcCallContext(ICaller caller, RpcContext context, MethodInstance methodInstance, MethodInvoker methodInvoker)
        {
            this.caller = caller;
            this.context = context;
            this.methodInstance = methodInstance;
            this.methodInvoker = methodInvoker;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICaller Caller => this.caller;

        /// <summary>
        /// 能取消的调用令箭，在客户端主动取消或网络故障时生效
        /// </summary>
        public CancellationTokenSource TokenSource
        {
            get
            {
                if (this.tokenSource == null)
                {
                    this.tokenSource = new CancellationTokenSource();
                }
                return this.tokenSource;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IRpcContext Context => this.context;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public MethodInstance MethodInstance => this.methodInstance;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public MethodInvoker MethodInvoker => this.methodInvoker;

        /// <summary>
        /// 序列化类型
        /// </summary>
        public SerializationType SerializationType => this.context == null ? (SerializationType)byte.MaxValue : this.context.SerializationType;
    }
}