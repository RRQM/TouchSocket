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

//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Threading.Tasks.Sources;

//namespace TouchSocket.Core;

///// <summary>
///// 表示一个块段，用于异步操作中作为值任务的源，提供 <see cref="IBlockResult"/> 类型的结果。
///// </summary>
///// <typeparam name="TBlockResult">块段中元素的类型，必须实现 <see cref="IBlockResult"/> 接口。</typeparam>
//public abstract class BlockSegment<TBlockResult> : SafetyDisposableObject, IValueTaskSource<TBlockResult>
//    where TBlockResult : IBlockResult
//{

//    #region 字段

//    private readonly TBlockResult m_blockResult;
//    private readonly SemaphoreSlim m_resetEventForCompleteRead = new(0, 1);
//    private CancellationTokenRegistration m_tokenRegistration;
//    private ManualResetValueTaskSourceCore<TBlockResult> m_valueTaskSourceCore;

//    #endregion 字段

//    /// <summary>
//    /// 初始化 <see cref="BlockSegment{TBlockResult}"/> 类的新实例。
//    /// </summary>
//    /// <param name="runContinuationsAsynchronously">指示是否异步运行延续。</param>
//    public BlockSegment(bool runContinuationsAsynchronously = false)
//    {
//        this.m_valueTaskSourceCore = new ManualResetValueTaskSourceCore<TBlockResult>()
//        {
//            RunContinuationsAsynchronously = runContinuationsAsynchronously
//        };

//        this.m_blockResult = this.CreateResult(this.CompleteRead);
//    }

//    /// <summary>
//    /// 取消当前操作。
//    /// </summary>
//    protected void Cancel()
//    {
//        var sourceStatus = this.m_valueTaskSourceCore.GetStatus(this.m_valueTaskSourceCore.Version);
//        // 如果当前状态是已完成或已取消，则不需要再次设置异常
//        if (sourceStatus != ValueTaskSourceStatus.Pending)
//        {
//            return;
//        }
//        this.m_valueTaskSourceCore.SetException(new OperationCanceledException());
//    }

//    /// <summary>
//    /// 完成读取操作。
//    /// </summary>
//    protected virtual void CompleteRead()
//    {
//        if (this.DisposedValue)
//        {
//            return;
//        }
//        try
//        {
//            this.m_valueTaskSourceCore.Reset();
//            this.m_resetEventForCompleteRead.Release();
//        }
//        catch
//        {
//        }

//    }

//    /// <summary>
//    /// 创建块段结果。
//    /// </summary>
//    /// <param name="actionForDispose">用于释放的操作。</param>
//    /// <returns>块段结果。</returns>
//    protected abstract TBlockResult CreateResult(Action actionForDispose);

//    /// <summary>
//    /// 异步读取块段。
//    /// </summary>
//    /// <param name="token">取消令牌。</param>
//    /// <returns>值任务，表示异步读取操作。</returns>
//    protected ValueTask<TBlockResult> ProtectedReadAsync(CancellationToken token)
//    {
//        this.ThrowIfDisposed();
//        token.ThrowIfCancellationRequested();

//        if (this.m_blockResult.IsCompleted)
//        {
//            return EasyValueTask.FromResult<TBlockResult>(this.m_blockResult);
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

//        return new ValueTask<TBlockResult>(this, this.m_valueTaskSourceCore.Version);
//    }

//    /// <inheritdoc/>
//    protected override void SafetyDispose(bool disposing)
//    {
//        if (disposing)
//        {
//            this.m_resetEventForCompleteRead.SafeDispose();
//        }
//    }

//    /// <summary>
//    /// 设置异常。
//    /// </summary>
//    /// <param name="ex">异常实例。</param>
//    protected void SetException(Exception ex)
//    {
//        this.m_valueTaskSourceCore.SetException(ex);
//    }

//    /// <summary>
//    /// 触发异步操作。
//    /// </summary>
//    /// <returns>表示异步操作的任务。</returns>
//    protected async Task TriggerAsync(CancellationToken token)
//    {
//        this.m_valueTaskSourceCore.SetResult(this.m_blockResult);
//        await this.m_resetEventForCompleteRead.WaitAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
//    }

//    #region IValueTaskSource

//    /// <inheritdoc/>
//    TBlockResult IValueTaskSource<TBlockResult>.GetResult(short token)
//    {
//        return this.m_valueTaskSourceCore.GetResult(token);
//    }

//    /// <inheritdoc/>
//    ValueTaskSourceStatus IValueTaskSource<TBlockResult>.GetStatus(short token)
//    {
//        return this.m_valueTaskSourceCore.GetStatus(token);
//    }

//    /// <inheritdoc/>
//    void IValueTaskSource<TBlockResult>.OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
//    {
//        this.m_valueTaskSourceCore.OnCompleted(continuation, state, token, flags);
//    }

//    #endregion IValueTaskSource
//}