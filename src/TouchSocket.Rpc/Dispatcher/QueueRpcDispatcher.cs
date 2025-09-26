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

namespace TouchSocket.Rpc;

/// <summary>
/// 队列RPC调度器类，用于管理和调度RPC调用请求。
/// </summary>
/// <typeparam name="TRpcActor">RPC行为者的类型，必须是类类型。</typeparam>
/// <typeparam name="TCallContext">调用上下文的类型，必须是类类型并且实现<see cref="ICallContext"/>接口。</typeparam>
public class QueueRpcDispatcher<TRpcActor, TCallContext> : DisposableObject, IRpcDispatcher<TRpcActor, TCallContext>
    where TRpcActor : class
    where TCallContext : class, ICallContext
{
    private readonly CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource();

    private readonly AsyncQueue<InvokeContext> m_queue = new();

    /// <summary>
    /// 初始化<see cref="QueueRpcDispatcher{TRpcActor, TCallContext}"/>类的新实例。
    /// </summary>
    public QueueRpcDispatcher()
    {
        _ = EasyTask.SafeRun(this.RpcTrigger);
    }

    /// <inheritdoc/>
    public bool Reenterable => false;

    /// <inheritdoc/>
    public Task Dispatcher(TRpcActor actor, TCallContext callContext, Func<TCallContext, Task> func)
    {
        this.m_queue.Enqueue(new InvokeContext(callContext, func));
        return EasyTask.CompletedTask;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.m_cancellationTokenSource.SafeCancel();
            this.m_cancellationTokenSource.SafeDispose();
        }
        base.Dispose(disposing);
    }

    private async Task RpcTrigger()
    {
        while (true)
        {
            if (this.m_cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }
            var invokeContext = await this.m_queue.DequeueAsync(this.m_cancellationTokenSource.Token);
            var callContext = invokeContext.IDmtpRpcCallContext;
            var func = invokeContext.Func;
            try
            {
                await func(callContext).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            catch
            {
            }
        }
    }

    private readonly struct InvokeContext
    {
        public InvokeContext(TCallContext iDmtpRpcCallContext, Func<TCallContext, Task> func)
        {
            this.IDmtpRpcCallContext = iDmtpRpcCallContext;
            this.Func = func;
        }

        public Func<TCallContext, Task> Func { get; }
        public TCallContext IDmtpRpcCallContext { get; }
    }
}