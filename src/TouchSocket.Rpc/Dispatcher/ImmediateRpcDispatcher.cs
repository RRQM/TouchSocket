using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 立即执行RPC调度器类，用于同步执行RPC调用。
    /// </summary>
    /// <typeparam name="TRpcActor">RPC行为者类型。</typeparam>
    /// <typeparam name="TCallContext">调用上下文类型，必须是ICallContext的子类。</typeparam>
    public class ImmediateRpcDispatcher<TRpcActor, TCallContext> : IRpcDispatcher<TRpcActor, TCallContext>
        where TRpcActor : class
        where TCallContext : class, ICallContext
    {

        /// <inheritdoc/>
        public bool Reenterable => false;

        /// <inheritdoc/>
        public Task Dispatcher(TRpcActor actor, TCallContext callContext, Func<object, Task> func)
        {
            // 直接调用传入的函数并传递调用上下文，不进行任何处理或延迟执行。
            return func(callContext);
        }
    }
}
