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
        /// <summary>
        /// 构造函数
        /// </summary>
        public WaitData()
        {
            this.waitHandle = new AutoResetEvent(false);
        }

        private EventWaitHandle waitHandle;

        private T waitResult;
        private WaitDataStatus status;

        /// <summary>
        /// 等待数据结果
        /// </summary>
        public T WaitResult
        {
            get { return waitResult; }
        }

        /// <summary>
        /// 状态
        /// </summary>
        public WaitDataStatus Status { get => status; }

        /// <summary>
        /// 载入结果
        /// </summary>
        public void LoadResult(T result)
        {
            this.waitResult = result;
        }

        /// <summary>
        /// 等待指定毫秒
        /// </summary>
        /// <param name="millisecond"></param>
        public bool Wait(int millisecond)
        {
            this.status = WaitDataStatus.Waiting;
            bool signSuccess = this.waitHandle.WaitOne(millisecond);
            this.status = WaitDataStatus.Running;
            return signSuccess;
        }

        /// <summary>
        /// 使等待的线程继续执行
        /// </summary>
        public void Set()
        {
            this.waitHandle.Set();
        }

        /// <summary>
        /// Reset
        /// </summary>
        public bool Reset()
        {
            return this.waitHandle.Reset();
        }

        /// <summary>
        /// 使等待的线程继续执行
        /// </summary>
        /// <param name="waitResult">等待结果</param>
        public void Set(T waitResult)
        {
            this.waitResult = waitResult;
            this.waitHandle.Set();
        }

        internal bool _dispose;

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
    }
}