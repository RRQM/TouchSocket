using System;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 等待数据对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WaitDataAsync<T> : DisposableObject, IWaitData<T>
    {
        private readonly AsyncAutoResetEvent m_asyncWaitHandle;
        private volatile WaitDataStatus m_status;
        private CancellationTokenRegistration m_tokenRegistration;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WaitDataAsync()
        {
            this.m_asyncWaitHandle = new AsyncAutoResetEvent(false);
        }

        /// <inheritdoc/>
        public WaitDataStatus Status { get => this.m_status; }

        /// <inheritdoc/>
        public T WaitResult { get; private set; }

        /// <inheritdoc/>
        public void Cancel()
        {
            this.m_status = WaitDataStatus.Canceled;
            this.m_asyncWaitHandle.Set();
        }

        /// <inheritdoc/>
        public void Reset()
        {
            this.m_status = WaitDataStatus.Default;
            this.WaitResult = default;
            this.m_asyncWaitHandle.Reset();
        }

        /// <inheritdoc/>
        public bool Set()
        {
            this.m_status = WaitDataStatus.SetRunning;
            return this.m_asyncWaitHandle.Set();
        }

        /// <inheritdoc/>
        public bool Set(T waitResult)
        {
            this.WaitResult = waitResult;
            this.m_status = WaitDataStatus.SetRunning;
            return this.m_asyncWaitHandle.Set();
        }

        /// <inheritdoc/>
        public void SetCancellationToken(CancellationToken cancellationToken)
        {
            if (cancellationToken.CanBeCanceled)
            {
                if (this.m_tokenRegistration == default)
                {
                    this.m_tokenRegistration = cancellationToken.Register(this.Cancel);
                }
                else
                {
                    this.m_tokenRegistration.Dispose();
                    this.m_tokenRegistration = cancellationToken.Register(this.Cancel);
                }
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
        /// <returns></returns>
        public async Task<WaitDataStatus> WaitAsync(TimeSpan timeSpan)
        {
            if (!await this.m_asyncWaitHandle.WaitOneAsync(timeSpan))
            {
                this.m_status = WaitDataStatus.Overtime;
            }

            return this.m_status;
        }

        /// <summary>
        /// 等待指定毫秒
        /// </summary>
        /// <param name="millisecond"></param>
        public Task<WaitDataStatus> WaitAsync(int millisecond)
        {
            return this.WaitAsync(TimeSpan.FromMilliseconds(millisecond));
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.m_status = WaitDataStatus.Disposed;
            this.WaitResult = default;
            this.m_asyncWaitHandle.SafeDispose();
            this.m_tokenRegistration.Dispose();
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// 等待数据对象
    /// </summary>
    public class WaitDataAsync : WaitDataAsync<object>
    {
    }
}