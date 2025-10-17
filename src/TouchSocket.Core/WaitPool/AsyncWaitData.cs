// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Threading.Tasks.Sources;

namespace TouchSocket.Core;

/// <summary>
/// 表示一个用于异步等待结果的容器，基于 <see cref="ValueTask{TResult}"/>/ <see cref="IValueTaskSource{TResult}"/> 实现。
/// </summary>
/// <typeparam name="T">等待的数据类型。</typeparam>
/// <remarks>
/// 此类用于在等待池中挂起并等待特定签名的数据到达。它使用 <see cref="ManualResetValueTaskSourceCore{TResult}"/>
/// 来实现高性能的 ValueTask 等待，并通过构造时传入的 <see cref="Action{Int32}"/> 回调在释放时将自身从池中移除。
/// </remarks>
public sealed class AsyncWaitData<T> : DisposableObject, IValueTaskSource<WaitDataStatus>
{
    private T m_pendingData;
    private readonly Action<int> m_remove;
    private T m_completedData;
    private ManualResetValueTaskSourceCore<T> m_core;
    private CancellationTokenRegistration m_registration;
    private WaitDataStatus m_status;
    private volatile int m_isCompleted; // 0 = 未完成, 1 = 已完成
    private readonly Action m_cancel;


    private static readonly ConcurrentQueue<AsyncWaitData<T>> m_pool = new();

    internal static AsyncWaitData<T> GetOrCreate(Action<int> remove, int sign, T pending)
    {
        if (m_pool.TryDequeue(out var item))
        {
            item.Reset(sign, pending);
            return item;
        }
        return new AsyncWaitData<T>(sign, remove, pending);
    }

    /// <summary>
    /// 使用指定签名和移除回调初始化一个新的 <see cref="AsyncWaitData{T}"/> 实例。
    /// </summary>
    /// <param name="sign">此等待项对应的签名（用于在池中查找）。</param>
    /// <param name="remove">完成或释放时调用的回调，用于将此实例从等待池中移除。</param>
    /// <param name="pendingData">可选的挂起数据，当创建时可以携带一个初始占位数据。</param>
    private AsyncWaitData(int sign, Action<int> remove,  T pendingData)
    {
        this.Sign = sign;
        this.m_remove = remove;
     
        this.m_pendingData = pendingData;
        this.m_core.RunContinuationsAsynchronously = true;
        this.m_cancel = this.Cancel;
    }

    /// <summary>
    /// 获取已完成时的返回数据。
    /// </summary>
    public T CompletedData => this.m_completedData;

    /// <summary>
    /// 获取挂起时的原始数据（如果在创建时传入）。
    /// </summary>
    public T PendingData => this.m_pendingData;

    /// <summary>
    /// 获取此等待项的签名标识。
    /// </summary>
    public int Sign { get; private set; }

    /// <summary>
    /// 获取当前等待状态（例如：Success、Canceled 等）。
    /// </summary>
    public WaitDataStatus Status => this.m_status;

    /// <summary>
    /// 取消当前等待，标记为已取消并触发等待任务的异常（<see cref="OperationCanceledException"/>）。
    /// </summary>
    public void Cancel()
    {
        this.Set(WaitDataStatus.Canceled, default!);
    }

    WaitDataStatus IValueTaskSource<WaitDataStatus>.GetResult(short token)
    {
        this.m_core.GetResult(token);
        return this.m_status;
    }

    ValueTaskSourceStatus IValueTaskSource<WaitDataStatus>.GetStatus(short token)
            => this.m_core.GetStatus(token);

    void IValueTaskSource<WaitDataStatus>.OnCompleted(Action<object> continuation, object state,
            short token, ValueTaskSourceOnCompletedFlags flags)
            => this.m_core.OnCompleted(continuation, state, token, flags);

    /// <summary>
    /// 将等待项设置为成功并携带结果数据。
    /// </summary>
    /// <param name="result">要设置的完成数据。</param>
    public void Set(T result)
    {
        this.Set(WaitDataStatus.Success, result);
    }

    /// <summary>
    /// 设置等待项的状态和数据，并完成对应的 ValueTask。
    /// </summary>
    /// <param name="status">要设置的状态。</param>
    /// <param name="result">要设置的完成数据。</param>
    public void Set(WaitDataStatus status, T result)
    {
        // 使用 Interlocked 确保只设置一次
        if (Interlocked.CompareExchange(ref this.m_isCompleted, 1, 0) != 0)
        {
            return; // 已经完成,直接返回
        }

        this.m_status = status;
        this.m_completedData = result;

        if (status == WaitDataStatus.Canceled)
        {
            this.m_core.SetException(new OperationCanceledException());
        }
        else
        {
            this.m_core.SetResult(result);
        }
    }
    /// <summary>
    /// 异步等待此项完成，返回一个 <see cref="ValueTask{WaitDataStatus}"/>，可传入取消令牌以取消等待。
    /// </summary>
    /// <param name="cancellationToken">可选的取消令牌。若触发则会调用 <see cref="Cancel"/>。</param>
    /// <returns>表示等待状态的 ValueTask。</returns>
    public ValueTask<WaitDataStatus> WaitAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.CanBeCanceled)
        {
            this.m_registration = cancellationToken.Register(this.m_cancel);
        }

        return new ValueTask<WaitDataStatus>(this, this.m_core.Version);
    }

    /// <summary>
    /// 重置以便复用
    /// </summary>
    internal void Reset(int sign, T pendingData)
    {
        this.m_isCompleted = 0;
        this.m_status = default;
        this.m_completedData = default!;
        this.m_pendingData = pendingData;
        this.Sign = sign;
        this.m_core.Reset();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // 确保取消令牌已释放
            this.m_registration.Dispose();
            this.m_remove(this.Sign);
            m_pool.Enqueue(this);
        }
        base.Dispose(disposing);
    }



}