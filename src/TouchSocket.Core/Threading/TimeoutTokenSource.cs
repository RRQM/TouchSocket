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

namespace TouchSocket.Core;

/// <summary>
/// 表示超时令牌源的状态。
/// </summary>
/// <remarks>
/// 此枚举用于跟踪异步操作的执行状态，特别是在涉及超时和取消的场景中。
/// 提供了从初始化到完成或取消的完整状态转换。
/// </remarks>
public enum TimeoutTokenState : byte
{
    /// <summary>
    /// 初始化状态，操作尚未开始或正在进行中。
    /// </summary>
    Initialized,

    /// <summary>
    /// 操作成功完成。
    /// </summary>
    Completed,

    /// <summary>
    /// 操作因超时而被取消。
    /// </summary>
    TimedOut,

    /// <summary>
    /// 操作被用户主动取消。
    /// </summary>
    Cancelled
}

/// <summary>
/// 带超时功能的取消令牌管理器。
/// </summary>
/// <remarks>
/// 此类继承自<see cref="DisposableObject"/>，提供了组合超时和用户取消令牌的功能。
/// 能够区分操作是因超时还是用户主动取消而终止，并提供相应的异常处理机制。
/// 适用于需要精确控制超时行为的异步操作场景。
/// </remarks>
public sealed class TimeoutTokenSource : DisposableObject
{
    private readonly CancellationTokenSource m_timeoutCts;
    private readonly CancellationTokenSource m_combinedCts;
    private readonly CancellationToken m_originalToken;
    private readonly int m_timeoutMs;
    private TimeoutTokenState m_state;

    /// <summary>
    /// 初始化<see cref="TimeoutTokenSource"/>类的新实例。
    /// </summary>
    /// <param name="timeoutMs">超时时间（以毫秒为单位）。</param>
    /// <param name="cancellationToken">用户提供的取消令牌。</param>
    /// <remarks>
    /// 构造函数创建一个组合的取消令牌，该令牌会在超时或用户取消时被触发。
    /// 初始状态设置为<see cref="TimeoutTokenState.Initialized"/>。
    /// </remarks>
    public TimeoutTokenSource(int timeoutMs, CancellationToken cancellationToken)
    {
        this.m_timeoutMs = timeoutMs;
        this.m_originalToken = cancellationToken;
        this.m_timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
        this.m_combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.m_timeoutCts.Token);
        this.m_state = TimeoutTokenState.Initialized;
    }

    /// <summary>
    /// 获取当前超时令牌源的状态。
    /// </summary>
    /// <value>表示当前状态的<see cref="TimeoutTokenState"/>枚举值。</value>
    /// <remarks>
    /// 此属性反映了操作的当前执行状态，包括初始化、完成、超时或取消。
    /// </remarks>
    public TimeoutTokenState State => this.m_state;

    /// <summary>
    /// 获取组合后的取消令牌。
    /// </summary>
    /// <value>一个<see cref="CancellationToken"/>，当超时或用户取消时会被触发。</value>
    /// <remarks>
    /// 此令牌结合了超时机制和用户取消请求，可用于需要同时响应这两种取消条件的异步操作。
    /// </remarks>
    public CancellationToken Token => this.m_combinedCts.Token;

    /// <summary>
    /// 标记操作成功完成。
    /// </summary>
    /// <remarks>
    /// 调用此方法将状态从<see cref="TimeoutTokenState.Initialized"/>更改为<see cref="TimeoutTokenState.Completed"/>。
    /// 如果状态已经不是初始化状态，则不会进行任何更改。
    /// </remarks>
    public void MarkCompleted()
    {
        if (this.m_state == TimeoutTokenState.Initialized)
        {
            this.m_state = TimeoutTokenState.Completed;
        }
    }

    /// <summary>
    /// 处理操作取消异常，转换为适当的异常类型并更新状态。
    /// </summary>
    /// <param name="ex">捕获的操作取消异常。</param>
    /// <exception cref="TimeoutException">当操作因超时而取消时抛出。</exception>
    /// <exception cref="OperationCanceledException">当操作被用户主动取消时抛出。</exception>
    /// <remarks>
    /// <para>此方法分析取消的原因并相应地更新状态：</para>
    /// <list type="bullet">
    /// <item>如果超时令牌被触发但原始令牌未被触发，则认为是超时，状态更新为<see cref="TimeoutTokenState.TimedOut"/>。</item>
    /// <item>如果原始令牌被触发，则认为是用户取消，状态更新为<see cref="TimeoutTokenState.Cancelled"/>。</item>
    /// <item>其他情况下重新抛出原始异常。</item>
    /// </list>
    /// </remarks>
    public void HandleCancellation(OperationCanceledException ex)
    {
        if (this.m_timeoutCts.Token.IsCancellationRequested && !this.m_originalToken.IsCancellationRequested)
        {
            // 如果是超时取消（超时令牌被取消，但原始令牌没有被取消）
            this.m_state = TimeoutTokenState.TimedOut;
            throw new TimeoutException($"操作在 {this.m_timeoutMs} 毫秒内未完成");
        }
        else if (this.m_originalToken.IsCancellationRequested)
        {
            // 如果是用户主动取消
            this.m_state = TimeoutTokenState.Cancelled;
            throw new OperationCanceledException("操作被用户取消", this.m_originalToken);
        }
        else
        {
            // 其他情况，重新抛出原异常
            throw ex;
        }
    }

    /// <summary>
    /// 释放由<see cref="TimeoutTokenSource"/>使用的托管资源。
    /// </summary>
    /// <param name="disposing">如果为<see langword="true"/>，则释放托管资源；否则仅释放非托管资源。</param>
    /// <remarks>
    /// 此方法重写基类的<see cref="DisposableObject.Dispose(bool)"/>方法，
    /// 负责释放内部创建的<see cref="CancellationTokenSource"/>实例。
    /// </remarks>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.m_timeoutCts?.Dispose();
            this.m_combinedCts?.Dispose();
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// 检查取消结果并转换为适当的结果类型。
    /// </summary>
    /// <param name="result">要检查的操作结果。</param>
    /// <returns>
    /// 如果结果表示取消且是由超时引起的，则返回<see cref="Result.Overtime"/>；
    /// 否则返回原始的<paramref name="result"/>。
    /// </returns>
    /// <remarks>
    /// 此方法用于将通用的取消结果转换为更具体的超时结果，
    /// 便于调用者区分取消的具体原因。
    /// </remarks>
    public Result CheckCancellationResult(Result result)
    {
        if (result.ResultCode == ResultCode.Canceled)
        {
            if (this.m_timeoutCts.Token.IsCancellationRequested && !this.m_originalToken.IsCancellationRequested)
            {
                // 如果是超时取消（超时令牌被取消，但原始令牌没有被取消）
                this.m_state = TimeoutTokenState.TimedOut;
                return Result.Overtime;
            }
        }

        return result;
    }
}