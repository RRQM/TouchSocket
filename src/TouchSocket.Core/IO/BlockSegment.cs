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

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace TouchSocket.Core;

/// <summary>
/// 表示一个块段，用于异步操作中作为值任务的源，提供 <see cref="IBlockResult"/> 类型的结果。
/// </summary>
/// <typeparam name="TBlockResult">块段中元素的类型，必须实现 <see cref="IBlockResult"/> 接口。</typeparam>
public abstract class BlockSegment<TBlockResult> : DisposableObject, IValueTaskSource<TBlockResult>
    where TBlockResult : IBlockResult
{
    #region 字段

    private readonly SemaphoreSlim m_resetEventForCompleteRead = new(0, 1);
    private CancellationTokenRegistration m_tokenRegistration;
    private ManualResetValueTaskSourceCore<TBlockResult> m_valueTaskSourceCore;
    private readonly TBlockResult m_blockResult;

    #endregion 字段

    /// <summary>
    /// 初始化 <see cref="BlockSegment{TBlockResult}"/> 类的新实例。
    /// </summary>
    /// <param name="runContinuationsAsynchronously">指示是否异步运行延续。</param>
    public BlockSegment(bool runContinuationsAsynchronously = false)
    {
        this.m_valueTaskSourceCore = new ManualResetValueTaskSourceCore<TBlockResult>()
        {
            RunContinuationsAsynchronously = runContinuationsAsynchronously
        };

        this.m_blockResult = this.CreateResult(this.CompleteRead);
    }

    /// <summary>
    /// 取消当前操作。
    /// </summary>
    protected void Cancel()
    {
        this.m_valueTaskSourceCore.SetException(new OperationCanceledException());
    }

    /// <summary>
    /// 完成读取操作。
    /// </summary>
    protected virtual void CompleteRead()
    {
        this.m_valueTaskSourceCore.Reset();
        this.m_resetEventForCompleteRead.Release();
    }

    /// <summary>
    /// 创建块段结果。
    /// </summary>
    /// <param name="actionForDispose">用于释放的操作。</param>
    /// <returns>块段结果。</returns>
    protected abstract TBlockResult CreateResult(Action actionForDispose);

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (this.DisposedValue)
        {
            return;
        }

        if (disposing)
        {
            this.m_resetEventForCompleteRead.SafeDispose();
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// 异步读取块段。
    /// </summary>
    /// <param name="token">取消令牌。</param>
    /// <returns>值任务，表示异步读取操作。</returns>
    protected ValueTask<TBlockResult> ProtectedReadAsync(CancellationToken token)
    {
        this.ThrowIfDisposed();
        token.ThrowIfCancellationRequested();

        if (this.m_blockResult.IsCompleted)
        {
            return EasyValueTask.FromResult<TBlockResult>(this.m_blockResult);
        }

        if (token.CanBeCanceled)
        {
            if (this.m_tokenRegistration == default)
            {
                this.m_tokenRegistration = token.Register(this.Cancel);
            }
            else
            {
                this.m_tokenRegistration.Dispose();
                this.m_tokenRegistration = token.Register(this.Cancel);
            }
        }

        return new ValueTask<TBlockResult>(this, this.m_valueTaskSourceCore.Version);
    }

    /// <summary>
    /// 设置异常。
    /// </summary>
    /// <param name="ex">异常实例。</param>
    protected void SetException(Exception ex)
    {
        this.m_valueTaskSourceCore.SetException(ex);
    }

    /// <summary>
    /// 触发异步操作。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    protected async Task TriggerAsync()
    {
        this.m_valueTaskSourceCore.SetResult(this.m_blockResult);
        await this.m_resetEventForCompleteRead.WaitAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #region IValueTaskSource

    /// <inheritdoc/>
    TBlockResult IValueTaskSource<TBlockResult>.GetResult(short token)
    {
        return this.m_valueTaskSourceCore.GetResult(token);
    }

    /// <inheritdoc/>
    ValueTaskSourceStatus IValueTaskSource<TBlockResult>.GetStatus(short token)
    {
        return this.m_valueTaskSourceCore.GetStatus(token);
    }

    /// <inheritdoc/>
    void IValueTaskSource<TBlockResult>.OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
    {
        this.m_valueTaskSourceCore.OnCompleted(continuation, state, token, flags);
    }

    #endregion IValueTaskSource
}

///// <summary>
///// 表示一个块段，用于异步操作中作为值任务的源，提供 <see cref="IBlockResult{T}"/> 类型的结果。
///// </summary>
///// <typeparam name="T">块段中元素的类型。</typeparam>
//public abstract class BlockSegment<T> : DisposableObject, IValueTaskSource<IBlockResult<T>>
//{
//    public static readonly IBlockResult<T> Completed = new InternalCompletedBlockResult();

//    #region 字段

//    private readonly AsyncAutoResetEvent m_resetEventForCompleteRead = new AsyncAutoResetEvent(false);
//    private readonly InternalBlockResult m_result;
//    private CancellationTokenRegistration m_tokenRegistration;
//    private ManualResetValueTaskSourceCore<IBlockResult<T>> m_valueTaskSourceCore;

//    #endregion 字段

//    /// <summary>
//    /// 初始化BlockSegment类的新实例。
//    /// </summary>
//    public BlockSegment(bool runContinuationsAsynchronously = false)
//    {
//        m_valueTaskSourceCore = new ManualResetValueTaskSourceCore<IBlockResult<T>>()
//        {
//            RunContinuationsAsynchronously = runContinuationsAsynchronously
//        };
//        // 初始化块结果，与CompleteRead方法关联
//        this.m_result = new InternalBlockResult(this.CompleteRead);
//    }

//    /// <summary>
//    /// 获取当前块段的结果。
//    /// </summary>
//    protected IBlockResult<T> BlockResult => this.m_result;

//    public static IBlockResult<T> FromResult(T result)
//    {
//        return new InternalBlockResult(() => { }) { IsCompleted = true };
//    }

//    protected void Cancel()
//    {
//        this.m_valueTaskSourceCore.SetException(new OperationCanceledException());
//    }

//    /// <inheritdoc/>
//    protected override void Dispose(bool disposing)
//    {
//        if (this.DisposedValue)
//        {
//            return;
//        }

//        if (disposing)
//        {
//            this.m_resetEventForCompleteRead.Set();
//            this.m_resetEventForCompleteRead.SafeDispose();
//        }
//        base.Dispose(disposing);
//    }

//    protected async Task InputAsync(T result)
//    {
//        // 设置结果中的内存数据
//        this.m_result.Result = result;

//        this.m_valueTaskSourceCore.SetResult(this.m_result);
//        // 等待读取完成
//        await this.m_resetEventForCompleteRead.WaitOneAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
//    }

//    protected async Task<Result> ProtectedComplete(string msg)
//    {
//        try
//        {
//            this.m_result.IsCompleted = true;
//            this.m_result.Message = msg;
//            await this.InputAsync(default).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
//            return Result.Success;
//        }
//        catch (Exception ex)
//        {
//            return Result.FromException(ex);
//        }
//    }

//    /// <summary>
//    /// 重置块段的状态，为下一次使用做准备。
//    /// </summary>
//    protected virtual void Reset()
//    {
//        // 重置等待读取完成的事件
//        this.m_resetEventForCompleteRead.Reset();
//        // 将块结果标记为未完成
//        this.m_result.IsCompleted = false;
//        // 清除结果中的内存数据
//        this.m_result.Result = default;
//        // 清除结果中的消息
//        this.m_result.Message = default;
//        this.m_valueTaskSourceCore.Reset();
//    }

//    /// <summary>
//    /// 值等待异步操作。
//    /// </summary>
//    /// <param name="token">取消令牌。</param>
//    /// <returns>值任务。</returns>
//    protected ValueTask<IBlockResult<T>> ValueWaitAsync(CancellationToken token)
//    {
//        this.ThrowIfDisposed();
//        token.ThrowIfCancellationRequested();

//        if (this.m_result.IsCompleted)
//        {
//            return EasyValueTask.FromResult<IBlockResult<T>>(this.m_result);
//        }

//        if (token.CanBeCanceled)
//        {
//            if (this.m_tokenRegistration == default)
//            {
//                this.m_tokenRegistration = token.Register(this.Cancel);
//            }
//            else
//            {
//                this.m_tokenRegistration.Dispose();
//                this.m_tokenRegistration = token.Register(this.Cancel);
//            }
//        }
//        this.Reset();
//        return new ValueTask<IBlockResult<T>>(this, this.m_valueTaskSourceCore.Version);
//    }

//    private void CompleteRead()
//    {
//        this.m_resetEventForCompleteRead.Set();
//    }

//    #region IValueTaskSource

//    IBlockResult<T> IValueTaskSource<IBlockResult<T>>.GetResult(short token)
//    {
//        return this.m_valueTaskSourceCore.GetResult(token);
//    }

//    ValueTaskSourceStatus IValueTaskSource<IBlockResult<T>>.GetStatus(short token)
//    {
//        return this.m_valueTaskSourceCore.GetStatus(token);
//    }

//    void IValueTaskSource<IBlockResult<T>>.OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
//    {
//        this.m_valueTaskSourceCore.OnCompleted(continuation, state, token, flags);
//    }

//    #endregion IValueTaskSource

//    #region Class

//    internal sealed class InternalBlockResult : IBlockResult<T>
//    {
//        private readonly Action m_disAction;

//        /// <summary>
//        /// ReceiverResult
//        /// </summary>
//        /// <param name="disAction"></param>
//        public InternalBlockResult(Action disAction)
//        {
//            this.m_disAction = disAction;
//        }

//        public bool IsCompleted { get; set; }
//        public string Message { get; set; }

//        public T Result { get; set; }

//        public void Dispose()
//        {
//            this.m_disAction.Invoke();
//        }
//    }

//    private sealed class InternalCompletedBlockResult : IBlockResult<T>
//    {
//        public bool IsCompleted => true;
//        public string Message => string.Empty;
//        public T Result => default;

//        public void Dispose()
//        {
//        }
//    }

//    #endregion Class
//}