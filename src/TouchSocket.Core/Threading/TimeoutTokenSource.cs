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

namespace TouchSocket.Core;

/// <summary>
/// 超时令牌源的状态
/// </summary>
public enum TimeoutTokenState:byte
{
    /// <summary>
    /// 初始化状态，操作尚未开始或正在进行
    /// </summary>
    Initialized,

    /// <summary>
    /// 操作成功完成
    /// </summary>
    Completed,

    /// <summary>
    /// 操作因超时而取消
    /// </summary>
    TimedOut,

    /// <summary>
    /// 操作被用户主动取消
    /// </summary>
    Cancelled
}

/// <summary>
/// 带超时功能的取消令牌管理器
/// </summary>
public sealed class TimeoutTokenSource : DisposableObject
{
    private readonly CancellationTokenSource m_timeoutCts;
    private readonly CancellationTokenSource m_combinedCts;
    private readonly CancellationToken m_originalToken;
    private readonly int m_timeoutMs;
    private TimeoutTokenState m_state;

    public TimeoutTokenSource(int timeoutMs, CancellationToken cancellationToken)
    {
        m_timeoutMs = timeoutMs;
        m_originalToken = cancellationToken;
        m_timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
        m_combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, m_timeoutCts.Token);
        m_state = TimeoutTokenState.Initialized;
    }

    /// <summary>
    /// 获取当前状态
    /// </summary>
    public TimeoutTokenState State => m_state;

    /// <summary>
    /// 获取组合后的取消令牌
    /// </summary>
    public CancellationToken Token => m_combinedCts.Token;

    /// <summary>
    /// 标记操作成功完成
    /// </summary>
    public void MarkCompleted()
    {
        if (m_state == TimeoutTokenState.Initialized)
        {
            m_state = TimeoutTokenState.Completed;
        }
    }

    /// <summary>
    /// 处理操作取消异常，转换为适当的异常类型并更新状态
    /// </summary>
    /// <param name="ex">操作取消异常</param>
    /// <exception cref="TimeoutException">操作超时</exception>
    /// <exception cref="OperationCanceledException">操作被取消</exception>
    public void HandleCancellation(OperationCanceledException ex)
    {
        if (m_timeoutCts.Token.IsCancellationRequested && !m_originalToken.IsCancellationRequested)
        {
            // 如果是超时取消（超时令牌被取消，但原始令牌没有被取消）
            m_state = TimeoutTokenState.TimedOut;
            throw new TimeoutException($"操作在 {m_timeoutMs} 毫秒内未完成");
        }
        else if (m_originalToken.IsCancellationRequested)
        {
            // 如果是用户主动取消
            m_state = TimeoutTokenState.Cancelled;
            throw new OperationCanceledException("操作被用户取消", m_originalToken);
        }
        else
        {
            // 其他情况，重新抛出原异常
            throw ex;
        }
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            m_timeoutCts?.Dispose();
            m_combinedCts?.Dispose();
        }
        base.Dispose(disposing);
    }

    public Result CheckCancellationResult(Result result)
    {
        if (result.ResultCode== ResultCode.Canceled)
        {
            if (m_timeoutCts.Token.IsCancellationRequested && !m_originalToken.IsCancellationRequested)
            {
                // 如果是超时取消（超时令牌被取消，但原始令牌没有被取消）
                m_state = TimeoutTokenState.TimedOut;
                return Result.Overtime;
            }
        }

        return result;
    }
}