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

    public T CompletedData => this.m_result;
    public T PendingData { get; }
    public int Sign { get; }

    public WaitDataStatus Status => this.m_status;

    public void Cancel()
    {
        this.Set(WaitDataStatus.Canceled, default);
    }

    public void Set(T result)
    {
        this.Set(WaitDataStatus.Success, result);
    }

    public void Set(WaitDataStatus status, T result)
    {
        this.m_result = result;
        this.m_status = status;
        this.m_asyncWaitHandle.TrySetResult(result);
    }

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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.m_remove.Invoke(this.Sign);
        }
        base.Dispose(disposing);
    }
}