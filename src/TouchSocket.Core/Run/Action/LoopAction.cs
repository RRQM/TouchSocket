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

namespace TouchSocket.Core
{
    /// <summary>
    /// 循环动作
    /// </summary>
    public sealed class LoopAction : DisposableObject
    {
        private readonly AsyncAutoResetEvent m_waitHandle;
        private readonly Func<LoopAction, Task> m_executeAction;

        public LoopAction(int count, TimeSpan interval, Func<LoopAction, Task> action)
        {
            this.LoopCount = count;
            this.m_executeAction = action;
            this.Interval = interval;
            this.m_waitHandle = new AsyncAutoResetEvent(false);
        }

        /// <summary>
        /// 创建可循环操作体
        /// </summary>
        /// <param name="count">循环次数，设为-1时一直循环</param>
        /// <param name="interval">每次循环间隔</param>
        /// <param name="action">执行委托</param>
        /// <returns></returns>
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
        /// 创建可循环操作体
        /// </summary>
        /// <param name="count">循环次数，设为-1时一直循环</param>
        /// <param name="intervalMS">每次循环间隔，毫秒</param>
        /// <param name="action">执行委托</param>
        /// <returns></returns>
        public static LoopAction CreateLoopAction(int count, int intervalMS, Action<LoopAction> action)
        {
            return CreateLoopAction(count, TimeSpan.FromMilliseconds(intervalMS), action);
        }

        /// <summary>
        /// 创建可循环操作体
        /// </summary>
        /// <param name="count">循环次数，设为-1时一直循环</param>
        /// <param name="action">执行委托</param>
        /// <returns></returns>
        public static LoopAction CreateLoopAction(int count, Action<LoopAction> action)
        {
            return CreateLoopAction(count, TimeSpan.Zero, action);
        }

        /// <summary>
        /// 创建可循环操作体
        /// </summary>
        /// <param name="interval">每次循环间隔</param>
        /// <param name="action">执行委托</param>
        /// <returns></returns>
        public static LoopAction CreateLoopAction(TimeSpan interval, Action<LoopAction> action)
        {
            return CreateLoopAction(-1, interval, action);
        }

        /// <summary>
        /// 创建可循环操作体
        /// </summary>
        /// <param name="action">执行委托</param>
        /// <returns></returns>
        public static LoopAction CreateLoopAction(Action<LoopAction> action)
        {
            return CreateLoopAction(-1, TimeSpan.Zero, action);
        }

        /// <summary>
        /// 已执行次数
        /// </summary>
        public int ExecutedCount { get; private set; }

        /// <summary>
        /// 执行间隔
        /// </summary>
        public TimeSpan Interval { get; private set; }

        /// <summary>
        /// 循环次数
        /// </summary>
        public int LoopCount { get; }

        /// <summary>
        /// 是否在运行
        /// </summary>
        public RunStatus RunStatus { get; private set; }

        /// <summary>
        /// 运行
        /// </summary>
        public void Run()
        {
            this.RunAsync().GetFalseAwaitResult();
        }

        /// <summary>
        /// 重新运行
        /// </summary>
        public void Rerun()
        {
            this.ThrowIfDisposed();
            this.RunStatus = RunStatus.None;
            this.Run();
        }

        /// <summary>
        /// 以异步重新运行
        /// </summary>
        /// <returns></returns>
        public async Task RerunAsync()
        {
            this.ThrowIfDisposed();
            this.RunStatus = RunStatus.None;
            await this.RunAsync().ConfigureFalseAwait();
        }

        /// <summary>
        /// 以异步运行
        /// </summary>
        /// <returns></returns>
        public Task RunAsync()
        {
            if (this.RunStatus != RunStatus.None)
            {
                return EasyTask.CompletedTask;
            }

            return Task.Run(async () =>
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
                         await this.m_executeAction.Invoke(this).ConfigureFalseAwait();
                         this.ExecutedCount++;
                         if (this.RunStatus == RunStatus.Paused)
                         {
                             await this.m_waitHandle.WaitOneAsync().ConfigureFalseAwait();
                         }
                         await this.m_waitHandle.WaitOneAsync(this.Interval).ConfigureFalseAwait();
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
                         await this.m_executeAction.Invoke(this).ConfigureFalseAwait();
                         this.ExecutedCount++;
                         if (this.RunStatus == RunStatus.Paused)
                         {
                             await this.m_waitHandle.WaitOneAsync().ConfigureFalseAwait();
                         }
                         await this.m_waitHandle.WaitOneAsync(this.Interval).ConfigureFalseAwait();
                     }
                 }
                 this.RunStatus = RunStatus.Completed;
             });
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            if (this.RunStatus == RunStatus.Running)
            {
                this.m_waitHandle.Reset();
                this.RunStatus = RunStatus.Paused;
            }
        }

        /// <summary>
        /// 恢复
        /// </summary>
        public void Resume()
        {
            if (this.RunStatus == RunStatus.Paused)
            {
                this.RunStatus = RunStatus.Running;
                this.m_waitHandle.Set();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.DisposedValue)
            {
                return;
            }
            this.m_waitHandle.Dispose();
            this.RunStatus = RunStatus.Disposed;
            base.Dispose(disposing);
        }
    }
}