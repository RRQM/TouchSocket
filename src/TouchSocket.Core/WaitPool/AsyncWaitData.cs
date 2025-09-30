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
    // ManualResetValueTaskSourceCore 是一个结构体，避免了额外托管对象分配，但需要配合 token 使用。
    private ManualResetValueTaskSourceCore<T> _core; // 核心结构体，不会分配额外对象

    // 缓存的移除回调，由 WaitHandlePool 构造时传入，避免每次分配委托。
    private readonly Action<int> _remove;

    // 挂起时的临时数据
    private T _pendingData;

    // 完成时的数据
    private T _completedData;

    // 当前等待状态（成功/取消/未完成等）
    private WaitDataStatus _status;

    /// <summary>
    /// 使用指定签名和移除回调初始化一个新的 <see cref="AsyncWaitData{T}"/> 实例。
    /// </summary>
    /// <param name="sign">此等待项对应的签名（用于在池中查找）。</param>
    /// <param name="remove">完成或释放时调用的回调，用于将此实例从等待池中移除。</param>
    /// <param name="pendingData">可选的挂起数据，当创建时可以携带一个初始占位数据。</param>
    public AsyncWaitData(int sign, Action<int> remove, T pendingData)
    {
        Sign = sign;
        _remove = remove;
        _pendingData = pendingData;
        _core.RunContinuationsAsynchronously = true; // 确保续体异步执行，避免潜在的栈内联执行问题
    }

    /// <summary>
    /// 获取此等待项的签名标识。
    /// </summary>
    public int Sign { get; }

    /// <summary>
    /// 获取挂起时的原始数据（如果在创建时传入）。
    /// </summary>
    public T PendingData => _pendingData;

    /// <summary>
    /// 获取已完成时的返回数据。
    /// </summary>
    public T CompletedData => _completedData;

    /// <summary>
    /// 获取当前等待状态（例如：Success、Canceled 等）。
    /// </summary>
    public WaitDataStatus Status => _status;

    /// <summary>
    /// 取消当前等待，标记为已取消并触发等待任务的异常（OperationCanceledException）。
    /// </summary>
    public void Cancel()
    {
        Set(WaitDataStatus.Canceled, default!);
    }

    /// <summary>
    /// 将等待项设置为成功并携带结果数据。
    /// </summary>
    /// <param name="result">要设置的完成数据。</param>
    public void Set(T result)
    {
        Set(WaitDataStatus.Success, result);
    }

    /// <summary>
    /// 设置等待项的状态和数据，并完成对应的 ValueTask。
    /// </summary>
    /// <param name="status">要设置的状态。</param>
    /// <param name="result">要设置的完成数据。</param>
    public void Set(WaitDataStatus status, T result)
    {
        _status = status;
        _completedData = result;

        if (status == WaitDataStatus.Canceled)
            _core.SetException(new OperationCanceledException());
        else
            _core.SetResult(result);
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
            cancellationToken.Register(() => Cancel());
        }

        return new ValueTask<WaitDataStatus>(this, _core.Version);
    }

    /// <summary>
    /// 从核心获取结果（显式接口实现）。
    /// 注意：此方法由 ValueTask 基础设施调用，不应直接在用户代码中调用。
    /// </summary>
    WaitDataStatus IValueTaskSource<WaitDataStatus>.GetResult(short token)
    {
        _core.GetResult(token);
        return _status;
    }

    /// <summary>
    /// 获取当前 ValueTask 源的状态（显式接口实现）。
    /// </summary>
    ValueTaskSourceStatus IValueTaskSource<WaitDataStatus>.GetStatus(short token)
        => _core.GetStatus(token);

    /// <summary>
    /// 注册续体（显式接口实现）。
    /// 注意：flags 可以控制是否捕获上下文等行为。
    /// </summary>
    void IValueTaskSource<WaitDataStatus>.OnCompleted(Action<object?> continuation, object? state,
        short token, ValueTaskSourceOnCompletedFlags flags)
        => _core.OnCompleted(continuation, state, token, flags);

    /// <summary>
    /// 释放托管资源时调用，会触发传入的移除回调，从所在的等待池中移除此等待项。
    /// </summary>
    /// <param name="disposing">是否为显式释放。</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _remove(Sign);
        }
        base.Dispose(disposing);
    }
}