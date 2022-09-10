//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core.Run
{
    /// <summary>
    /// 循环动作
    /// </summary>
    public class LoopAction : EasyAction, IDisposable
    {
        /// <summary>
        /// 析构函数
        /// </summary>
        ~LoopAction()
        {
            this.Dispose();
        }

        private int executedCount;

        private readonly TimeSpan interval;

        private readonly int loopCount;

        private readonly EventWaitHandle waitHandle;

        private LoopAction(int count, TimeSpan interval, Action<LoopAction> action)
        {
            this.loopCount = count;
            this.action = action;
            this.interval = interval;
            this.waitHandle = new AutoResetEvent(false);
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
            return new LoopAction(count, interval, action);
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
            return new LoopAction(count, TimeSpan.FromMilliseconds(intervalMS), action);
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
        public int ExecutedCount => this.executedCount;

        /// <summary>
        /// 执行间隔
        /// </summary>
        public TimeSpan Interval => this.interval;

        /// <summary>
        /// 循环次数
        /// </summary>
        public int LoopCount => this.loopCount;

        private readonly Action<LoopAction> action;

        /// <summary>
        /// 执行委托
        /// </summary>
        public Action<LoopAction> ExecuteAction => this.action;

        private RunStatus runStatus;

        /// <summary>
        /// 是否在运行
        /// </summary>
        public RunStatus RunStatus => this.runStatus;

        /// <summary>
        /// 运行
        /// </summary>
        public void Run()
        {
            if (this.runStatus == RunStatus.None)
            {
                this.runStatus = RunStatus.Running;
                if (this.loopCount >= 0)
                {
                    for (int i = 0; i < this.loopCount; i++)
                    {
                        if (this.runStatus == RunStatus.Disposed)
                        {
                            return;
                        }
                        this.action.Invoke(this);
                        this.executedCount++;
                        if (this.runStatus == RunStatus.Paused)
                        {
                            this.waitHandle.WaitOne();
                        }
                        this.waitHandle.WaitOne(this.interval);
                    }
                }
                else
                {
                    while (true)
                    {
                        if (this.runStatus == RunStatus.Disposed)
                        {
                            return;
                        }
                        this.action.Invoke(this);
                        this.executedCount++;
                        if (this.runStatus == RunStatus.Paused)
                        {
                            this.waitHandle.WaitOne();
                        }
                        this.waitHandle.WaitOne(this.interval);
                    }
                }
                this.runStatus = RunStatus.Completed;
            }
        }

        /// <summary>
        /// 重新运行
        /// </summary>
        public void Rerun()
        {
            if (this.runStatus == RunStatus.Disposed)
            {
                throw new Exception("无法利用已释放的资源");
            }
            this.runStatus = RunStatus.None;
            this.Run();
        }

        /// <summary>
        /// 以异步重新运行
        /// </summary>
        /// <returns></returns>
        public Task RerunAsync()
        {
            if (this.runStatus == RunStatus.Disposed)
            {
                throw new Exception("无法利用已释放的资源");
            }
            this.runStatus = RunStatus.None;
            return this.RunAsync();
        }

        /// <summary>
        /// 以异步运行
        /// </summary>
        /// <returns></returns>
        public Task RunAsync()
        {
            return Task.Run(() =>
            {
                this.Run();
            });
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            if (this.runStatus == RunStatus.Running)
            {
                this.waitHandle.Reset();
                this.runStatus = RunStatus.Paused;
            }
        }

        /// <summary>
        /// 回复
        /// </summary>
        public void Resume()
        {
            if (this.runStatus == RunStatus.Paused)
            {
                this.runStatus = RunStatus.Running;
                this.waitHandle.Set();
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (this.runStatus == RunStatus.Disposed)
            {
                return;
            }
            if (this.runStatus == RunStatus.Completed)
            {
                this.waitHandle.Dispose();
            }
            this.runStatus = RunStatus.Disposed;
        }
    }
}