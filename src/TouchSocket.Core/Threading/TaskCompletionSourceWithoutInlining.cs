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

using System.Threading.Tasks;

namespace TouchSocket.Core;

/// <summary>
/// A <see cref="TaskCompletionSource{TResult}"/>-derivative that
/// does not inline continuations if so configured.
/// </summary>
/// <typeparam name="T">The type of the task's resulting value.</typeparam>
internal class TaskCompletionSourceWithoutInlining<T> : TaskCompletionSource<T>
{
    /// <summary>
    /// The Task that we expose to others that may not inline continuations.
    /// </summary>
    private readonly Task<T> exposedTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskCompletionSourceWithoutInlining{T}"/> class.
    /// </summary>
    /// <param name="allowInliningContinuations">
    /// <see langword="true" /> to allow continuations to be inlined; otherwise <see langword="false" />.
    /// </param>
    /// <param name="options">
    /// TaskCreationOptions to pass on to the base constructor.
    /// </param>
    /// <param name="state">The state to set on the Task.</param>
    internal TaskCompletionSourceWithoutInlining(bool allowInliningContinuations, TaskCreationOptions options = TaskCreationOptions.None, object? state = null)
        : base(state, AdjustFlags(options, allowInliningContinuations))
    {
        this.exposedTask = base.Task;
    }

    /// <summary>
    /// Gets the <see cref="Task"/> that may never complete inline with completion of this <see cref="TaskCompletionSource{TResult}"/>.
    /// </summary>
    /// <devremarks>
    /// Return the base.Task if it is already completed since inlining continuations
    /// on the completer is no longer a concern. Also, when we are not inlining continuations,
    /// this.exposedTask completes slightly later than base.Task, and callers expect
    /// the Task we return to be complete as soon as they call TrySetResult.
    /// </devremarks>
    internal new Task<T> Task => base.Task.IsCompleted ? base.Task : this.exposedTask;

    /// <summary>
    /// Modifies the specified flags to include RunContinuationsAsynchronously
    /// if wanted by the caller and supported by the platform.
    /// </summary>
    /// <param name="options">The base options supplied by the caller.</param>
    /// <param name="allowInliningContinuations"><see langword="true" /> to allow inlining continuations.</param>
    /// <returns>The possibly modified flags.</returns>
    private static TaskCreationOptions AdjustFlags(TaskCreationOptions options, bool allowInliningContinuations)
    {
        return allowInliningContinuations
            ? (options & ~TaskCreationOptions.RunContinuationsAsynchronously)
            : (options | TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
