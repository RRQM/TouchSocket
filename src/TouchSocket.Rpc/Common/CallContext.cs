//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Threading;
using TouchSocket.Core;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// Rpc调用上下文的基本实现
    /// </summary>
    public abstract class CallContext : ICallContext
    {
        private bool m_canceled;
        private readonly object m_locker = new object();
        private CancellationTokenSource m_tokenSource;

        /// <summary>
        /// CallContext
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="rpcMethod"></param>
        /// <param name="resolver"></param>
        public CallContext(object caller, RpcMethod rpcMethod, IResolver resolver)
        {
            this.Caller = caller;
            this.RpcMethod = rpcMethod;
            this.Resolver = resolver;
        }

        /// <summary>
        /// CallContext
        /// </summary>
        public CallContext()
        {
        }

        /// <inheritdoc/>
        public object Caller { get; protected set; }

        /// <inheritdoc/>
        public RpcMethod RpcMethod { get; protected set; }

        /// <inheritdoc/>
        public IResolver Resolver { get; protected set; }

        /// <inheritdoc/>
        public CancellationToken Token
        {
            get
            {
                lock (this.m_locker)
                {
                    if (this.m_canceled)
                    {
                        return new CancellationToken(true);
                    }

                    this.m_tokenSource ??= new CancellationTokenSource();
                    return this.m_tokenSource.Token;
                }
            }
        }

        /// <inheritdoc/>
        public void Cancel()
        {
            lock (this.m_locker)
            {
                if (this.m_tokenSource != null)
                {
                    this.m_tokenSource.Cancel();
                }
                else
                {
                    this.m_canceled = true;
                }
            }
        }
    }
}