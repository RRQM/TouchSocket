using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TouchSocket.Core;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// Rpc调用上下文的基本实现
    /// </summary>
    public abstract class CallContext : ICallContext
    {
        private CancellationTokenSource m_tokenSource;

        /// <summary>
        /// CallContext
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="methodInstance"></param>
        /// <param name="resolver"></param>
        public CallContext(object caller, MethodInstance methodInstance, IResolver resolver)
        {
            this.Caller = caller;
            this.MethodInstance = methodInstance;
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
        public MethodInstance MethodInstance { get; protected set; }

        /// <inheritdoc/>
        public IResolver Resolver { get; protected set; }

        /// <inheritdoc/>
        public CancellationToken Token
        {
            get
            {
                this.m_tokenSource ??= new CancellationTokenSource();
                return this.m_tokenSource.Token;
            }
        }

        /// <inheritdoc/>
        [Obsolete("此配置已被弃用，请使用Token和Cancel()代替该功能。", true)]
        public CancellationTokenSource TokenSource => throw new NotImplementedException();

        /// <inheritdoc/>
        public void Cancel()
        {
            this.m_tokenSource?.Cancel();
        }
    }
}