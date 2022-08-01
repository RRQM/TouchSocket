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
using System.Threading;
using TouchSocket.Core.Serialization;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// TouchRpc上下文
    /// </summary>
    public class TouchRpcCallContext : ICallContext
    {
        private readonly object caller;
        private CancellationTokenSource tokenSource;
        private readonly TouchRpcPackage context;
        private readonly MethodInstance methodInstance;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="context"></param>
        /// <param name="methodInstance"></param>
        public TouchRpcCallContext(object caller, TouchRpcPackage context, MethodInstance methodInstance)
        {
            this.caller = caller;
            this.context = context;
            this.methodInstance = methodInstance;
        }

        /// <summary>
        /// 当<see cref="TokenSource"/>不为空时，调用<see cref="CancellationTokenSource.Cancel()"/>
        /// </summary>
        /// <returns></returns>
        public bool TryCancel()
        {
            if (this.tokenSource != null)
            {
                this.tokenSource.Cancel();
                return true;
            }
            return false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public object Caller => this.caller;

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
        /// TouchRpcContext
        /// </summary>
        public TouchRpcPackage Context => this.context;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public MethodInstance MethodInstance => this.methodInstance;

        /// <summary>
        /// 序列化类型
        /// </summary>
        public SerializationType SerializationType => this.context == null ? (SerializationType)byte.MaxValue : this.context.SerializationType;
    }
}