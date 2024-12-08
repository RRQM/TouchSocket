using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 队列RPC调度器类，用于管理和调度RPC调用请求。
    /// </summary>
    /// <typeparam name="TRpcActor">RPC行为者的类型，必须是类类型。</typeparam>
    /// <typeparam name="TCallContext">调用上下文的类型，必须是类类型并且实现ICallContext接口。</typeparam>
    public class QueueRpcDispatcher<TRpcActor, TCallContext> : IRpcDispatcher<TRpcActor, TCallContext>
        where TRpcActor : class
        where TCallContext : class, ICallContext
    {

        /// <inheritdoc/>
        public bool Reenterable => false;

        private readonly ConcurrentQueue<InvokeContext> m_queue = new ConcurrentQueue<InvokeContext>();
        private readonly SemaphoreSlim m_semaphore = new SemaphoreSlim(1, 1);

        /// <inheritdoc/>
        public async Task Dispatcher(TRpcActor actor, TCallContext callContext, Func<object, Task> func)
        {
            this.m_queue.Enqueue(new InvokeContext(callContext, func));
            await Task.Factory.StartNew(this.RpcTrigger).ConfigureAwait(false);
        }

        private async Task RpcTrigger()
        {
            await this.m_semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                while (true)
                {
                    if (this.m_queue.TryDequeue(out var invokeContext))
                    {
                        var callContext = invokeContext.IDmtpRpcCallContext;
                        var func = invokeContext.Func;
                        try
                        {
                            await func(callContext).ConfigureAwait(false);
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch
            {
            }
            finally
            {
                this.m_semaphore.Release();
            }
        }

        private readonly struct InvokeContext
        {
            public InvokeContext(TCallContext iDmtpRpcCallContext, Func<object, Task> func)
            {
                this.IDmtpRpcCallContext = iDmtpRpcCallContext;
                this.Func = func;
            }

            public Func<object, Task> Func { get; }
            public TCallContext IDmtpRpcCallContext { get; }
        }
    }
}