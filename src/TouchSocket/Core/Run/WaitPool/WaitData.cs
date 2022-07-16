//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在TouchSocket.Core.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Threading;

namespace TouchSocket.Core.Run
{
    /// <summary>
    /// 等待数据对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WaitData<T> : DisposableObject
    {
        private WaitDataStatus m_status;

        private AutoResetEvent m_waitHandle;

        private T m_waitResult;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WaitData()
        {
            this.m_waitHandle = new AutoResetEvent(false);
        }

        /// <summary>
        /// 状态
        /// </summary>
        public WaitDataStatus Status => this.m_status;

        /// <summary>
        /// 等待数据结果
        /// </summary>
        public T WaitResult => this.m_waitResult;

        /// <summary>
        /// 取消任务
        /// </summary>
        public void Cancel()
        {
            this.m_status = WaitDataStatus.Canceled;
            this.m_waitHandle.Set();
        }

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.m_status = WaitDataStatus.Disposed;
            this.m_waitResult = default;
            this.m_waitHandle.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Reset。
        /// 设置<see cref="WaitResult"/>为null。然后重置状态为<see cref="WaitDataStatus.Default"/>，waitHandle.Reset()
        /// </summary>
        public bool Reset()
        {
            this.m_status = WaitDataStatus.Default;
            this.m_waitResult = default;
            return this.m_waitHandle.Reset();
        }

        /// <summary>
        /// 使等待的线程继续执行
        /// </summary>
        public bool Set()
        {
            this.m_status = WaitDataStatus.SetRunning;
            return this.m_waitHandle.Set();
        }

        /// <summary>
        /// 使等待的线程继续执行
        /// </summary>
        /// <param name="waitResult">等待结果</param>
        public bool Set(T waitResult)
        {
            this.m_waitResult = waitResult;
            this.m_status = WaitDataStatus.SetRunning;
            return this.m_waitHandle.Set();
        }

        /// <summary>
        /// 加载取消令箭
        /// </summary>
        /// <param name="cancellationToken"></param>
        public void SetCancellationToken(CancellationToken cancellationToken)
        {
            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(this.Cancel);
            }
        }

        /// <summary>
        /// 载入结果
        /// </summary>
        public void SetResult(T result)
        {
            this.m_waitResult = result;
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
    }
}