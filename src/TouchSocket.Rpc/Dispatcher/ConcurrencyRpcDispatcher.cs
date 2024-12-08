using System;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 并发RPC调度器，用于管理RPC方法的并发调用。
    /// </summary>
    /// <typeparam name="TRpcActor">RPC行为的类型，必须是IDependencyObject的子类。</typeparam>
    /// <typeparam name="TCallContext">调用上下文的类型，必须是ICallContext的子类。</typeparam>
    public class ConcurrencyRpcDispatcher<TRpcActor, TCallContext> : IRpcDispatcher<TRpcActor, TCallContext>
        where TRpcActor : class, IDependencyObject
        where TCallContext : class, ICallContext
    {
        /// <inheritdoc/>
        public bool Reenterable => true;

        /// <inheritdoc/>
        public Task Dispatcher(TRpcActor actor, TCallContext callContext, Func<object, Task> func)
        {
            // 获取当前调用上下文中定义的RPC方法信息
            var rpcMethod = callContext.RpcMethod;

            // 如果RPC方法不允许重新进入，则直接执行该方法，避免并发问题
            if (rpcMethod != null && rpcMethod.Reenterable == false)
            {
                return func(callContext);
            }

            // 否则，使用Task.Factory.StartNew在新的线程上执行RPC方法，以支持并发调用
            _ = Task.Factory.StartNew(func, callContext);

            return EasyTask.CompletedTask;
        }
    }
}
