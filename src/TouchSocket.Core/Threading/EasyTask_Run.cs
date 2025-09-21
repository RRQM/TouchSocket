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
    #region SafeRun

    /// <summary>
    /// 安全地运行一个异步方法。
    /// </summary>
    /// <param name="func">要运行的异步方法。</param>
    /// <returns>表示异步操作的任务。</returns>
    public static async Task<Result> SafeRun(Func<Task> func)
    {
        if (func is null)
        {
            return Result.FromFail(TouchSocketCoreResource.ArgumentIsNull.Format(nameof(func)));
        }

        try
        {
            await func().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <summary>
    /// 安全地运行一个支持取消令牌的异步方法。
    /// </summary>
    /// <param name="func">要运行的异步方法。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    public static async Task<Result> SafeRun(Func<CancellationToken, Task> func, CancellationToken ct)
    {
        if (func is null)
        {
            return Result.FromFail(TouchSocketCoreResource.ArgumentIsNull.Format(nameof(func)));
        }

        if (ct.IsCancellationRequested)
        {
            return Result.Canceled;
        }

        try
        {
            await func(ct).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return Result.Success;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Result.Canceled;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <summary>
    /// 安全地运行一个带有返回值的异步方法。
    /// </summary>
    /// <typeparam name="TResult">返回值的类型。</typeparam>
    /// <param name="func">要运行的异步方法。</param>
    /// <returns>表示异步操作的任务。</returns>
    public static async Task<Result<TResult>> SafeRun<TResult>(Func<Task<TResult>> func)
    {
        if (func is null)
        {
            return new Result<TResult>(ResultCode.Failure, TouchSocketCoreResource.ArgumentIsNull.Format(nameof(func)));
        }

        try
        {
            var result = await func().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return new Result<TResult>(result);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <summary>
    /// 安全地运行一个支持取消令牌且带有返回值的异步方法。
    /// </summary>
    /// <typeparam name="TResult">返回值的类型。</typeparam>
    /// <param name="func">要运行的异步方法。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    public static async Task<Result<TResult>> SafeRun<TResult>(Func<CancellationToken, Task<TResult>> func, CancellationToken ct)
    {
        if (func is null)
        {
            return new Result<TResult>(ResultCode.Failure, TouchSocketCoreResource.ArgumentIsNull.Format(nameof(func)));
        }

        if (ct.IsCancellationRequested)
        {
            return new Result<TResult>(ResultCode.Canceled, Result.Canceled.Message);
        }

        try
        {
            var result = await func(ct).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return new Result<TResult>(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return new Result<TResult>(ResultCode.Canceled, Result.Canceled.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <summary>
    /// 安全地运行一个带有状态的异步方法。
    /// </summary>
    /// <typeparam name="T1">状态的类型。</typeparam>
    /// <param name="func">要运行的异步方法。</param>
    /// <param name="status">传递给方法的状态。</param>
    /// <returns>表示异步操作的任务。</returns>
    public static async Task<Result> SafeRun<T1>(Func<T1, Task> func, T1 status)
    {
        if (func is null)
        {
            return Result.FromFail(TouchSocketCoreResource.ArgumentIsNull.Format(nameof(func)));
        }

        try
        {
            await func(status).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <summary>
    /// 安全地运行一个支持取消令牌且带有状态的异步方法。
    /// </summary>
    /// <typeparam name="T1">状态的类型。</typeparam>
    /// <param name="func">要运行的异步方法。</param>
    /// <param name="status">传递给方法的状态。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    public static async Task<Result> SafeRun<T1>(Func<T1, CancellationToken, Task> func, T1 status, CancellationToken ct)
    {
        if (func is null)
        {
            return Result.FromFail(TouchSocketCoreResource.ArgumentIsNull.Format(nameof(func)));
        }

        if (ct.IsCancellationRequested)
        {
            return Result.Canceled;
        }

        try
        {
            await func(status, ct).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return Result.Success;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Result.Canceled;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <summary>
    /// 安全地运行一个带有状态和返回值的异步方法。
    /// </summary>
    /// <typeparam name="T1">状态的类型。</typeparam>
    /// <typeparam name="TResult">返回值的类型。</typeparam>
    /// <param name="func">要运行的异步方法。</param>
    /// <param name="status">传递给方法的状态。</param>
    /// <returns>表示异步操作的任务。</returns>
    public static async Task<Result<TResult>> SafeRun<T1, TResult>(Func<T1, Task<TResult>> func, T1 status)
    {
        if (func is null)
        {
            return new Result<TResult>(ResultCode.Failure, TouchSocketCoreResource.ArgumentIsNull.Format(nameof(func)));
        }

        try
        {
            var result = await func(status).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return new Result<TResult>(result);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <summary>
    /// 安全地运行一个支持取消令牌且带有状态和返回值的异步方法。
    /// </summary>
    /// <typeparam name="T1">状态的类型。</typeparam>
    /// <typeparam name="TResult">返回值的类型。</typeparam>
    /// <param name="func">要运行的异步方法。</param>
    /// <param name="status">传递给方法的状态。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    public static async Task<Result<TResult>> SafeRun<T1, TResult>(Func<T1, CancellationToken, Task<TResult>> func, T1 status, CancellationToken ct)
    {
        if (func is null)
        {
            return new Result<TResult>(ResultCode.Failure, TouchSocketCoreResource.ArgumentIsNull.Format(nameof(func)));
        }

        if (ct.IsCancellationRequested)
        {
            return new Result<TResult>(ResultCode.Canceled, Result.Canceled.Message);
        }

        try
        {
            var result = await func(status, ct).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return new Result<TResult>(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return new Result<TResult>(ResultCode.Canceled, Result.Canceled.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
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
    /// <returns>表示异步操作的任务。</returns>
    public static async Task<Result> SafeRun<T1, T2>(Func<T1, T2, Task> func, T1 status1, T2 status2)
    {
        if (func is null)
        {
            return Result.FromFail(TouchSocketCoreResource.ArgumentIsNull.Format(nameof(func)));
        }

        try
        {
            await func(status1, status2).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <summary>
    /// 安全地运行一个支持取消令牌且带有两个状态的异步方法。
    /// </summary>
    /// <typeparam name="T1">第一个状态的类型。</typeparam>
    /// <typeparam name="T2">第二个状态的类型。</typeparam>
    /// <param name="func">要运行的异步方法。</param>
    /// <param name="status1">传递给方法的第一个状态。</param>
    /// <param name="status2">传递给方法的第二个状态。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    public static async Task<Result> SafeRun<T1, T2>(Func<T1, T2, CancellationToken, Task> func, T1 status1, T2 status2, CancellationToken ct)
    {
        if (func is null)
        {
            return Result.FromFail(TouchSocketCoreResource.ArgumentIsNull.Format(nameof(func)));
        }

        if (ct.IsCancellationRequested)
        {
            return Result.Canceled;
        }

        try
        {
            await func(status1, status2, ct).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return Result.Success;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Result.Canceled;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <summary>
    /// 安全地运行一个带有两个状态和返回值的异步方法。
    /// </summary>
    /// <typeparam name="T1">第一个状态的类型。</typeparam>
    /// <typeparam name="T2">第二个状态的类型。</typeparam>
    /// <typeparam name="TResult">返回值的类型。</typeparam>
    /// <param name="func">要运行的异步方法。</param>
    /// <param name="status1">传递给方法的第一个状态。</param>
    /// <param name="status2">传递给方法的第二个状态。</param>
    /// <returns>表示异步操作的任务。</returns>
    public static async Task<Result<TResult>> SafeRun<T1, T2, TResult>(Func<T1, T2, Task<TResult>> func, T1 status1, T2 status2)
    {
        if (func is null)
        {
            return new Result<TResult>(ResultCode.Failure, TouchSocketCoreResource.ArgumentIsNull.Format(nameof(func)));
        }

        try
        {
            var result = await func(status1, status2).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return new Result<TResult>(result);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <summary>
    /// 安全地运行一个支持取消令牌且带有两个状态和返回值的异步方法。
    /// </summary>
    /// <typeparam name="T1">第一个状态的类型。</typeparam>
    /// <typeparam name="T2">第二个状态的类型。</typeparam>
    /// <typeparam name="TResult">返回值的类型。</typeparam>
    /// <param name="func">要运行的异步方法。</param>
    /// <param name="status1">传递给方法的第一个状态。</param>
    /// <param name="status2">传递给方法的第二个状态。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    public static async Task<Result<TResult>> SafeRun<T1, T2, TResult>(Func<T1, T2, CancellationToken, Task<TResult>> func, T1 status1, T2 status2, CancellationToken ct)
    {
        if (func is null)
        {
            return new Result<TResult>(ResultCode.Failure, TouchSocketCoreResource.ArgumentIsNull.Format(nameof(func)));
        }

        if (ct.IsCancellationRequested)
        {
            return new Result<TResult>(ResultCode.Canceled, Result.Canceled.Message);
        }

        try
        {
            var result = await func(status1, status2, ct).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return new Result<TResult>(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return new Result<TResult>(ResultCode.Canceled, Result.Canceled.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    #endregion SafeRun

    /// <summary>
    /// 安全地等待一个任务完成。
    /// </summary>
    /// <param name="task">要等待的任务。</param>
    /// <returns>表示任务结果的 <see cref="Result"/> 对象。</returns>
    public static async Task<Result> SafeWaitAsync(this Task task)
    {
        if (task is null)
        {
            return Result.FromFail(TouchSocketCoreResource.ArgumentIsNull.Format(nameof(task)));
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
    /// <returns>表示任务结果的 <see cref="Result{T}"/> 对象。</returns>
    public static async Task<Result<T>> SafeWaitAsync<T>(this Task<T> task)
    {
        if (task is null)
        {
            return new Result<T>(ResultCode.Failure, TouchSocketCoreResource.ArgumentIsNull.Format(nameof(task)));
        }

        try
        {
            var result = await task.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return new Result<T>(result);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }
}