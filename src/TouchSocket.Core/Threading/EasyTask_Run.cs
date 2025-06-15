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
using TouchSocket.Resources;

namespace TouchSocket.Core;

public static partial class EasyTask
{
    /// <summary>
    /// 运行一个带有状态和取消令牌的异步方法。
    /// </summary>
    /// <typeparam name="T">状态的类型。</typeparam>
    /// <param name="func">要运行的异步方法。</param>
    /// <param name="status">传递给方法的状态。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    public static Task Run<T>(Func<T, CancellationToken, Task> func, T status, CancellationToken ct = default)
    {
        ThrowHelper.ThrowArgumentNullExceptionIf(func, nameof(func));

        return Task.Run(() => func(status, ct), ct);
    }

    /// <summary>
    /// 运行一个带有状态的异步方法。
    /// </summary>
    /// <typeparam name="T">状态的类型。</typeparam>
    /// <param name="func">要运行的异步方法。</param>
    /// <param name="status">传递给方法的状态。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    public static Task Run<T>(Func<T, Task> func, T status, CancellationToken ct = default)
    {
        ThrowHelper.ThrowArgumentNullExceptionIf(func, nameof(func));
        return Task.Run(() => func(status), ct);
    }

    /// <summary>
    /// 安全地运行一个带有状态的异步方法。
    /// </summary>
    /// <typeparam name="T1">状态的类型。</typeparam>
    /// <param name="func">要运行的异步方法。</param>
    /// <param name="status">传递给方法的状态。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    public static async Task SafeRun<T1>(Func<T1, Task> func, T1 status, CancellationToken ct = default)
    {
        if (func is null)
        {
            return;
        }
        if (ct.IsCancellationRequested)
        {
            return;
        }
        try
        {
            await Task.Run(() => func(status), ct).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        catch
        {
        }
    }

    /// <summary>
    /// 安全地等待一个任务完成。
    /// </summary>
    /// <param name="task">要等待的任务。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>表示任务结果的 <see cref="Result"/> 对象。</returns>
    public static async Task<Result> SafeWaitAsync(this Task task, CancellationToken ct = default)
    {
        if (task is null)
        {
            return Result.FromFail(TouchSocketCoreResource.ArgumentIsNull.Format(nameof(task)));
        }
        if (ct.IsCancellationRequested)
        {
            return Result.Canceled;
        }
        try
        {
            await task.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <summary>
    /// 安全地等待一个任务完成并返回结果。
    /// </summary>
    /// <typeparam name="T">任务结果的类型。</typeparam>
    /// <param name="task">要等待的任务。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>表示任务结果的 <see cref="Result{T}"/> 对象。</returns>
    public static async Task<Result<T>> SafeWaitAsync<T>(this Task<T> task, CancellationToken ct = default)
    {
        if (task is null)
        {
            return Result.FromFail(TouchSocketCoreResource.ArgumentIsNull.Format(nameof(task)));
        }
        if (ct.IsCancellationRequested)
        {
            return Result.Canceled;
        }
        try
        {
            return new Result<T>(await task.ConfigureAwait(EasyTask.ContinueOnCapturedContext));
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <summary>
    /// 安全地运行一个带有状态的异步方法。
    /// </summary>
    /// <param name="func">要运行的异步方法。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    public static async Task SafeRun(Func<Task> func, CancellationToken ct = default)
    {
        if (func is null)
        {
            return;
        }
        if (ct.IsCancellationRequested)
        {
            return;
        }
        try
        {
            await Task.Run(() => func(), ct).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        catch
        {
        }
    }

    /// <summary>
    /// 安全地运行一个带有两个状态的异步方法。
    /// </summary>
    /// <typeparam name="T1">第一个状态的类型。</typeparam>
    /// <typeparam name="T2">第二个状态的类型。</typeparam>
    /// <param name="func">要运行的异步方法。</param>
    /// <param name="status1">传递给方法的第一个状态。</param>
    /// <param name="status2">传递给方法的第二个状态。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    public static async Task SafeRun<T1, T2>(Func<T1, T2, Task> func, T1 status1, T2 status2, CancellationToken ct = default)
    {
        if (func is null)
        {
            return;
        }
        if (ct.IsCancellationRequested)
        {
            return;
        }
        try
        {
            await Task.Run(() => func(status1, status2), ct).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        catch
        {
        }
    }

    /// <summary>
    /// 运行一个无状态的异步方法。
    /// </summary>
    /// <param name="func">要运行的异步方法。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    public static Task Run(Func<Task> func, CancellationToken ct = default)
    {
        ThrowHelper.ThrowArgumentNullExceptionIf(func, nameof(func));

        return Task.Run(func, ct);
    }

    /// <summary>
    /// 运行一个带有状态的同步方法。
    /// </summary>
    /// <typeparam name="T">状态的类型。</typeparam>
    /// <param name="func">要运行的同步方法。</param>
    /// <param name="status">传递给方法的状态。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    public static Task Run<T>(Action<T> func, T status, CancellationToken ct = default)
    {
        ThrowHelper.ThrowArgumentNullExceptionIf(func, nameof(func));

        return Task.Run(() => func(status), ct);
    }

    /// <summary>
    /// 运行一个带有状态和取消令牌的同步方法。
    /// </summary>
    /// <typeparam name="T">状态的类型。</typeparam>
    /// <param name="func">要运行的同步方法。</param>
    /// <param name="status">传递给方法的状态。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    public static Task Run<T>(Action<T, CancellationToken> func, T status, CancellationToken ct = default)
    {
        ThrowHelper.ThrowArgumentNullExceptionIf(func, nameof(func));

        return Task.Run(() => func(status, ct), ct);
    }
}