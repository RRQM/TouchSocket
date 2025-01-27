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

namespace TouchSocket.Core;

/// <summary>
/// 等待数据对象
/// </summary>
/// <typeparam name="T"></typeparam>
public class WaitDataAsync<T> : DisposableObject, IWaitDataAsync<T>
{
    private readonly AsyncAutoResetEvent m_asyncWaitHandle;
    private volatile WaitDataStatus m_status;
    private CancellationTokenRegistration m_tokenRegistration;

    /// <summary>
    /// 构造函数
    /// </summary>
    public WaitDataAsync()
    {
        this.m_asyncWaitHandle = new AsyncAutoResetEvent(false);
    }

    /// <inheritdoc/>
    public WaitDataStatus Status { get => this.m_status; }

    /// <inheritdoc/>
    public T WaitResult { get; private set; }

    /// <inheritdoc/>
    public void Cancel()
    {
        this.m_status = WaitDataStatus.Canceled;
        this.m_asyncWaitHandle.Set();
    }

    /// <inheritdoc/>
    public void Reset()
    {
        this.m_status = WaitDataStatus.Default;
        this.WaitResult = default;
        this.m_asyncWaitHandle.Reset();
    }

    /// <inheritdoc/>
    public bool Set()
    {
        this.m_status = WaitDataStatus.SetRunning;
        return this.m_asyncWaitHandle.Set();
    }

    /// <inheritdoc/>
    public bool Set(T waitResult)
    {
        this.WaitResult = waitResult;
        this.m_status = WaitDataStatus.SetRunning;
        return this.m_asyncWaitHandle.Set();
    }

    /// <inheritdoc/>
    public void SetCancellationToken(CancellationToken cancellationToken)
    {
        if (cancellationToken.CanBeCanceled)
        {
            if (this.m_tokenRegistration == default)
            {
                this.m_tokenRegistration = cancellationToken.Register(this.Cancel);
            }
            else
            {
                this.m_tokenRegistration.Dispose();
                this.m_tokenRegistration = cancellationToken.Register(this.Cancel);
            }
        }
    }

    /// <inheritdoc/>
    public void SetResult(T result)
    {
        this.WaitResult = result;
    }

    /// <inheritdoc/>
    public async Task<WaitDataStatus> WaitAsync(TimeSpan timeSpan)
    {
        if (!await this.m_asyncWaitHandle.WaitOneAsync(timeSpan).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
        {
            this.m_status = WaitDataStatus.Overtime;
        }

        return this.m_status;
    }

    /// <inheritdoc/>
    public Task<WaitDataStatus> WaitAsync(int millisecond)
    {
        return this.WaitAsync(TimeSpan.FromMilliseconds(millisecond));
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.m_status = WaitDataStatus.Disposed;
            this.WaitResult = default;
            this.m_asyncWaitHandle.SafeDispose();
            this.m_tokenRegistration.Dispose();
        }

        base.Dispose(disposing);
    }
}

/// <summary>
/// 等待数据对象
/// </summary>
public class WaitDataAsync : WaitDataAsync<object>
{
}