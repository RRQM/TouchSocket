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
using System.Threading.Tasks;

namespace TouchSocket.Core;


/// <summary>
/// LoopAction 类用于在指定循环次数和间隔下执行异步操作。
/// 它支持暂停、恢复和重新运行操作。
/// </summary>
public sealed class LoopAction : SafetyDisposableObject
{
    /// <summary>
    /// 异步自动重置事件，用于控制循环执行的同步点。
    /// </summary>
    private readonly AsyncAutoResetEvent m_waitHandle;

    /// <summary>
    /// 执行操作的委托，接受 LoopAction 实例作为参数。
    /// </summary>
    private readonly Func<LoopAction, Task> m_executeAction;

    /// <summary>
    /// 初始化 LoopAction 实例。
    /// </summary>
    /// <param name="count">循环执行次数，-1 表示无限循环。</param>
    /// <param name="interval">两次执行操作之间的间隔时间。</param>
    /// <param name="action">要执行的异步操作。</param>
    public LoopAction(int count, TimeSpan interval, Func<LoopAction, Task> action)
    {
        this.LoopCount = count;
        this.m_executeAction = action;
        this.Interval = interval;
        this.m_waitHandle = new AsyncAutoResetEvent(false);
    }

    /// <summary>
    /// 创建并返回一个 LoopAction 实例，该实例将在指定次数和间隔下执行给定的异步操作。
    /// </summary>
    /// <param name="count">循环执行次数。</param>
    /// <param name="interval">两次执行操作之间的间隔时间。</param>
    /// <param name="action">要执行的异步操作。</param>
    /// <returns>一个新的 LoopAction 实例。</returns>
    public static LoopAction CreateLoopAction(int count, TimeSpan interval, Action<LoopAction> action)
    {
        Task Run(LoopAction loop)
        {
            action(loop);
            return EasyTask.CompletedTask;
        }
        return new LoopAction(count, interval, Run);
    }

    /// <summary>
    /// 创建并返回一个 LoopAction 实例，该实例将在指定次数和以毫秒为单位的间隔下执行给定的异步操作。
    /// </summary>
    /// <param name="count">循环执行次数。</param>
    /// <param name="intervalMS">两次执行操作之间的间隔时间（毫秒）。</param>
    /// <param name="action">要执行的异步操作。</param>
    /// <returns>一个新的 LoopAction 实例。</returns>
    public static LoopAction CreateLoopAction(int count, int intervalMS, Action<LoopAction> action)
    {
        return CreateLoopAction(count, TimeSpan.FromMilliseconds(intervalMS), action);
    }

    /// <summary>
    /// 创建并返回一个 LoopAction 实例，该实例将在指定次数和无间隔下执行给定的异步操作。
    /// </summary>
    /// <param name="count">循环执行次数。</param>
    /// <param name="action">要执行的异步操作。</param>
    /// <returns>一个新的 LoopAction 实例。</returns>
    public static LoopAction CreateLoopAction(int count, Action<LoopAction> action)
    {
        return CreateLoopAction(count, TimeSpan.Zero, action);
    }

    /// <summary>
    /// 创建并返回一个 LoopAction 实例，该实例将在无限次数和指定间隔下执行给定的异步操作。
    /// </summary>
    /// <param name="interval">两次执行操作之间的间隔时间。</param>
    /// <param name="action">要执行的异步操作。</param>
    /// <returns>一个新的 LoopAction 实例。</returns>
    public static LoopAction CreateLoopAction(TimeSpan interval, Action<LoopAction> action)
    {
        return CreateLoopAction(-1, interval, action);
    }

    /// <summary>
    /// 创建并返回一个 LoopAction 实例，该实例将在无限次数和无间隔下执行给定的异步操作。
    /// </summary>
    /// <param name="action">要执行的异步操作。</param>
    /// <returns>一个新的 LoopAction 实例。</returns>
    public static LoopAction CreateLoopAction(Action<LoopAction> action)
    {
        return CreateLoopAction(-1, TimeSpan.Zero, action);
    }

    /// <summary>
    /// 已执行次数。
    /// </summary>
    public int ExecutedCount { get; private set; }

    /// <summary>
    /// 执行间隔。
    /// </summary>
    public TimeSpan Interval { get; private set; }

    /// <summary>
    /// 循环次数。
    /// </summary>
    public int LoopCount { get; }

    /// <summary>
    /// 是否在运行。
    /// </summary>
    public RunStatus RunStatus { get; private set; }

    /// <summary>
    /// 运行 LoopAction 实例，如果已运行则不执行任何操作。
    /// </summary>
    public void Run()
    {
        this.RunAsync().GetFalseAwaitResult();
    }

    /// <summary>
    /// 重新运行 LoopAction 实例，重置已执行次数和运行状态。
    /// </summary>
    public void Rerun()
    {
        this.ThrowIfDisposed();
        this.RunStatus = RunStatus.None;
        this.Run();
    }

    /// <summary>
    /// 以异步方式重新运行 LoopAction 实例。
    /// </summary>
    /// <returns>异步操作任务。</returns>
    public async Task RerunAsync()
    {
        this.ThrowIfDisposed();
        this.RunStatus = RunStatus.None;
        await this.RunAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 以异步方式运行 LoopAction 实例。
    /// </summary>
    /// <returns>异步操作任务。</returns>
    public Task RunAsync()
    {
        return this.RunStatus != RunStatus.None
            ? EasyTask.CompletedTask
            : EasyTask.SafeRun(async () =>
         {
             this.RunStatus = RunStatus.Running;
             if (this.LoopCount >= 0)
             {
                 for (var i = 0; i < this.LoopCount; i++)
                 {
                     if (this.RunStatus == RunStatus.Disposed)
                     {
                         return;
                     }
                     await this.m_executeAction.Invoke(this).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                     this.ExecutedCount++;
                     if (this.RunStatus == RunStatus.Paused)
                     {
                         await this.m_waitHandle.WaitOneAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                     }
                     await this.m_waitHandle.WaitOneAsync(this.Interval).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                 }
             }
             else
             {
                 while (true)
                 {
                     if (this.RunStatus == RunStatus.Disposed)
                     {
                         return;
                     }
                     await this.m_executeAction.Invoke(this).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                     this.ExecutedCount++;
                     if (this.RunStatus == RunStatus.Paused)
                     {
                         await this.m_waitHandle.WaitOneAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                     }
                     await this.m_waitHandle.WaitOneAsync(this.Interval).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                 }
             }
             this.RunStatus = RunStatus.Completed;
         });
    }

    /// <summary>
    /// 暂停正在运行的 LoopAction 实例。
    /// </summary>
    public void Pause()
    {
        if (this.RunStatus == RunStatus.Running)
        {
            this.RunStatus = RunStatus.Paused;
        }
    }

    /// <summary>
    /// 恢复已暂停的 LoopAction 实例。
    /// </summary>
    public void Resume()
    {
        if (this.RunStatus == RunStatus.Paused)
        {
            this.RunStatus = RunStatus.Running;
            this.m_waitHandle.Set();
        }
    }

    /// <summary>
    /// 处置 LoopAction 实例，释放所有资源。
    /// </summary>
    /// <param name="disposing">是否为托管资源。</param>
    protected override void SafetyDispose(bool disposing)
    {
        if (this.DisposedValue)
        {
            return;
        }
        if (disposing)
        {
            this.RunStatus = RunStatus.Disposed;
            this.m_waitHandle.SetAll();
        }
    }
}