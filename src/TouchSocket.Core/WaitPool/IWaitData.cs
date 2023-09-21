using System;
using System.Threading;

namespace TouchSocket.Core
{
    /// <summary>
    /// IWaitData
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IWaitData<T> : IDisposable
    {
        /// <summary>
        /// 等待对象的状态
        /// </summary>
        WaitDataStatus Status { get; }

        /// <summary>
        /// 等待结果
        /// </summary>
        T WaitResult { get; }

        /// <summary>
        /// 取消等待
        /// </summary>
        void Cancel();

        /// <summary>
        /// Reset。
        /// 设置<see cref="WaitResult"/>为null。然后重置状态为<see cref="WaitDataStatus.Default"/>
        /// </summary>
        void Reset();

        /// <summary>
        /// 使等待的线程继续执行
        /// </summary>
        bool Set();

        /// <summary>
        /// 使等待的线程继续执行
        /// </summary>
        /// <param name="waitResult">等待结果</param>
        bool Set(T waitResult);

        /// <summary>
        /// 加载取消令箭
        /// </summary>
        /// <param name="cancellationToken"></param>
        void SetCancellationToken(CancellationToken cancellationToken);

        /// <summary>
        /// 载入结果
        /// </summary>
        void SetResult(T result);
    }
}