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

using System;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core;

/// <summary>
/// 表示一个异步等待数据容器，提供基于<see cref="Task"/>的异步等待机制。
/// 继承自<see cref="DisposableObject"/>类。
/// </summary>
/// <typeparam name="T">等待数据的类型。</typeparam>
/// <remarks>
/// AsyncWaitData用于异步等待特定数据的到达，支持取消操作和状态跟踪。
/// 当数据到达或操作被取消时，等待的任务将被完成。
/// 提供了完成数据、挂起数据、签名标识和状态管理等功能。
/// </remarks>
public sealed class AsyncWaitData<T> : DisposableObject
{
    private readonly TaskCompletionSource<T> m_asyncWaitHandle = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Action<int> m_remove;
    private T m_result;
    private WaitDataStatus m_status;

    internal AsyncWaitData(int sign, Action<int> remove, T pendingData)
    {
        this.Sign = sign;
        this.m_remove = remove;
        this.PendingData = pendingData;
    }

    /// <summary>
    /// 获取已完成的数据。
    /// </summary>
    /// <value>当等待完成后返回的结果数据。</value>
    /// <remarks>
    /// 此属性在等待操作完成后包含实际的结果数据。
    /// 在等待完成之前，此值可能为默认值或未定义。
    /// </remarks>
    public T CompletedData => this.m_result;

    /// <summary>
    /// 获取挂起状态下的数据。
    /// </summary>
    /// <value>在等待期间保持的挂起数据。</value>
    /// <remarks>
    /// 此属性包含在创建AsyncWaitData实例时提供的初始数据，
    /// 通常用于在等待期间保存上下文信息或中间状态。
    /// </remarks>
    public T PendingData { get; }

    /// <summary>
    /// 获取等待数据的唯一签名标识。
    /// </summary>
    /// <value>用于标识此等待数据实例的唯一整数值。</value>
    /// <remarks>
    /// 签名用于在等待池中唯一标识此等待数据，便于管理和检索。
    /// 当对象被释放时，此签名将用于从等待池中移除相应的条目。
    /// </remarks>
    public int Sign { get; }

    /// <summary>
    /// 获取当前等待数据的状态。
    /// </summary>
    /// <value>表示等待操作当前状态的<see cref="WaitDataStatus"/>枚举值。</value>
    /// <remarks>
    /// 状态会随着等待操作的进行而发生变化，包括默认、成功、超时、取消和已释放等状态。
    /// </remarks>
    public WaitDataStatus Status => this.m_status;

    /// <summary>
    /// 取消等待操作。
    /// </summary>
    /// <remarks>
    /// 此方法将等待状态设置为<see cref="WaitDataStatus.Canceled"/>，
    /// 并使用默认值完成等待任务。调用此方法后，正在等待的任务将以取消状态完成。
    /// </remarks>
    public void Cancel()
    {
        this.Set(WaitDataStatus.Canceled, default);
    }

    /// <summary>
    /// 使用指定结果完成等待操作。
    /// </summary>
    /// <param name="result">要设置的结果数据。</param>
    /// <remarks>
    /// 此方法将等待状态设置为<see cref="WaitDataStatus.Success"/>，
    /// 并使用提供的结果完成等待任务。
    /// </remarks>
    public void Set(T result)
    {
        this.Set(WaitDataStatus.Success, result);
    }

    /// <summary>
    /// 使用指定状态和结果完成等待操作。
    /// </summary>
    /// <param name="status">要设置的等待状态。</param>
    /// <param name="result">要设置的结果数据。</param>
    /// <remarks>
    /// 此方法设置等待操作的最终状态和结果，并完成底层的任务。
    /// 一旦调用此方法，等待的任务将被标记为完成。
    /// </remarks>
    public void Set(WaitDataStatus status, T result)
    {
        this.m_result = result;
        this.m_status = status;
        this.m_asyncWaitHandle.TrySetResult(result);
    }

    /// <summary>
    /// 异步等待操作完成，支持取消操作。
    /// </summary>
    /// <param name="token">用于取消等待操作的取消令牌。</param>
    /// <returns>
    /// 表示异步等待操作的<see cref="ValueTask{TResult}"/>，
    /// 返回等待操作的最终状态。
    /// </returns>
    /// <remarks>
    /// 如果取消令牌可以被取消，则在取消时会将状态设置为<see cref="WaitDataStatus.Canceled"/>。
    /// 否则将一直等待直到操作完成。此方法配置为不捕获同步上下文。
    /// </remarks>
    public async ValueTask<WaitDataStatus> WaitAsync(CancellationToken token)
    {
        if (token.CanBeCanceled)
        {
            try
            {
                await this.m_asyncWaitHandle.Task.WithCancellation(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            catch (OperationCanceledException)
            {
                this.m_status = WaitDataStatus.Canceled;
            }
        }
        else
        {
            await this.m_asyncWaitHandle.Task.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        return this.m_status;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.m_remove.Invoke(this.Sign);
        }
        base.Dispose(disposing);
    }
}