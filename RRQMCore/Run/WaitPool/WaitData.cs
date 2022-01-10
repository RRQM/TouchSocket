//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Threading;

namespace RRQMCore.Run
{
    /// <summary>
    /// 等待数据对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WaitData<T> : IDisposable
    {
        internal bool _dispose;

        private WaitDataStatus status;

        private AutoResetEvent waitHandle;

        private T waitResult;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WaitData()
        {
            this.waitHandle = new AutoResetEvent(false);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~WaitData()
        {
            if (!this._dispose)
            {
                this.Dispose();
            }
        }

        /// <summary>
        /// 状态
        /// </summary>
        public WaitDataStatus Status { get => status; }

        /// <summary>
        /// 等待数据结果
        /// </summary>
        public T WaitResult
        {
            get { return waitResult; }
        }

        /// <summary>
        /// 取消任务
        /// </summary>
        public void Cancel()
        {
            this.status = WaitDataStatus.Canceled;
            this.waitHandle.Set();
        }

        /// <summary>
        /// 回收
        /// </summary>
        public void Dispose()
        {
            this.status = WaitDataStatus.Disposed;
            this._dispose = true;
            this.waitResult = default;
            this.waitHandle.Dispose();
        }

        /// <summary>
        /// Reset
        /// </summary>
        public bool Reset()
        {
            this.status = WaitDataStatus.Default;
            this.waitResult = default;
            return this.waitHandle.Reset();
        }

        /// <summary>
        /// 使等待的线程继续执行
        /// </summary>
        public void Set()
        {
            this.status = WaitDataStatus.SetRunning;
            this.waitHandle.Set();
        }

        /// <summary>
        /// 使等待的线程继续执行
        /// </summary>
        /// <param name="waitResult">等待结果</param>
        public void Set(T waitResult)
        {
            this.waitResult = waitResult;
            this.status = WaitDataStatus.SetRunning;
            this.waitHandle.Set();
        }

        /// <summary>
        /// 加载取消令箭
        /// </summary>
        /// <param name="cancellationToken"></param>
        public void SetCancellationToken(CancellationToken cancellationToken)
        {
            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(() =>
                {
                    this.Cancel();
                });
            }
        }

        /// <summary>
        /// 载入结果
        /// </summary>
        public void SetResult(T result)
        {
            this.waitResult = result;
        }

        /// <summary>
        /// 等待指定毫秒
        /// </summary>
        /// <param name="millisecond"></param>
        public WaitDataStatus Wait(int millisecond)
        {
            if (!this.waitHandle.WaitOne(millisecond))
            {
                this.status = WaitDataStatus.Overtime;
            }
            return this.status;
        }
    }
}