//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Threading;

namespace TouchSocket.Core
{
    /// <summary>
    /// 等待数据对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WaitData<T> : DisposableObject, IWaitData<T>
    {
        private readonly AutoResetEvent m_waitHandle;
        private volatile WaitDataStatus m_status;

        /// <summary>
        /// WaitData
        /// </summary>
        public WaitData()
        {
            this.m_waitHandle = new AutoResetEvent(false);
        }

        /// <inheritdoc/>
        public WaitDataStatus Status { get => this.m_status; }

        /// <inheritdoc/>
        public T WaitResult { get; private set; }

        /// <inheritdoc/>
        public void Cancel()
        {
            this.m_status = WaitDataStatus.Canceled;
            this.m_waitHandle.Set();
        }

        /// <inheritdoc/>
        public void Reset()
        {
            this.m_status = WaitDataStatus.Default;
            this.WaitResult = default;
            this.m_waitHandle.Reset();
        }

        /// <inheritdoc/>
        public bool Set()
        {
            this.m_status = WaitDataStatus.SetRunning;
            return this.m_waitHandle.Set();
        }

        /// <inheritdoc/>
        public bool Set(T waitResult)
        {
            this.WaitResult = waitResult;
            this.m_status = WaitDataStatus.SetRunning;
            return this.m_waitHandle.Set();
        }

        /// <inheritdoc/>
        public void SetCancellationToken(CancellationToken cancellationToken)
        {
            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(this.Cancel);
            }
        }

        /// <inheritdoc/>
        public void SetResult(T result)
        {
            this.WaitResult = result;
        }

        /// <summary>
        /// 等待指定时间
        /// </summary>
        /// <param name="timeSpan"></param>
        public WaitDataStatus Wait(TimeSpan timeSpan)
        {
            return this.Wait((int)timeSpan.TotalMilliseconds);
        }

        /// <summary>
        /// 等待指定毫秒
        /// </summary>
        /// <param name="millisecond"></param>
        public WaitDataStatus Wait(int millisecond)
        {
            if (!this.m_waitHandle.WaitOne(millisecond))
            {
                this.m_status = WaitDataStatus.Overtime;
            }
            return this.m_status;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.m_status = WaitDataStatus.Disposed;
            this.WaitResult = default;
            this.m_waitHandle.SafeDispose();
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// 等待数据对象
    /// </summary>
    public class WaitData : WaitData<object>
    {
    }
}