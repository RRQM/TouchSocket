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

namespace TouchSocket.Sockets;

/// <summary>
/// 提供从任务异步模式到 APM 异步模式的转换。
/// </summary>
/// <remarks>
/// PR:https://github.com/RRQM/TouchSocket/pull/140
/// </remarks>
internal static class TaskApmAdapter
{
    /// <summary>
    /// 将任务转换为 APM 异步结果。
    /// </summary>
    public static IAsyncResult Begin(Task task, AsyncCallback callback, object state)
    {
        ThrowHelper.ThrowIfNull(task, nameof(task));

        if (task.IsCompleted)
        {
            var completedResult = new TaskAsyncResult(task, state, true);
            callback?.Invoke(completedResult);
            return completedResult;
        }

        var asyncResult = ReferenceEquals(task.AsyncState, state)
            ? (IAsyncResult)task
            : new TaskAsyncResult(task, state, false);

        if (callback != null)
        {
            InvokeCallbackWhenTaskCompletes(task, callback, asyncResult);
        }

        return asyncResult;
    }

    /// <summary>
    /// 结束不返回结果的 APM 异步操作。
    /// </summary>
    public static void End(IAsyncResult asyncResult)
    {
        GetTask(asyncResult).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 结束返回结果的 APM 异步操作。
    /// </summary>
    public static TResult End<TResult>(IAsyncResult asyncResult)
    {
        var task = GetTask(asyncResult) as Task<TResult>;
        if (task == null)
        {
            throw new ArgumentException("异步结果类型无效。", nameof(asyncResult));
        }

        return task.GetAwaiter().GetResult();
    }

    private static Task GetTask(IAsyncResult asyncResult)
    {
        ThrowHelper.ThrowIfNull(asyncResult, nameof(asyncResult));

        var task = asyncResult is TaskAsyncResult taskAsyncResult
            ? taskAsyncResult.Task
            : asyncResult as Task;

        return task ?? throw new ArgumentException("异步结果无效。", nameof(asyncResult));
    }

    private static void InvokeCallbackWhenTaskCompletes(Task task, AsyncCallback callback, IAsyncResult asyncResult)
    {
        task.ConfigureAwait(false)
            .GetAwaiter()
            .OnCompleted(() => callback(asyncResult));
    }

    private sealed class TaskAsyncResult : IAsyncResult
    {
        private readonly bool m_completedSynchronously;
        private readonly object m_state;

        public TaskAsyncResult(Task task, object state, bool completedSynchronously)
        {
            this.Task = task;
            this.m_state = state;
            this.m_completedSynchronously = completedSynchronously;
        }

        public Task Task { get; }

        object IAsyncResult.AsyncState => this.m_state;

        WaitHandle IAsyncResult.AsyncWaitHandle => ((IAsyncResult)this.Task).AsyncWaitHandle;

        bool IAsyncResult.CompletedSynchronously => this.m_completedSynchronously;

        bool IAsyncResult.IsCompleted => this.Task.IsCompleted;
    }
}