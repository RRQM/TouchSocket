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
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace TouchSocket.Core
{
    public abstract class ValueTaskSource<TResult> : DisposableObject, IValueTaskSource<TResult>
    {
        #region 字段

        private static readonly Action<object> s_continuationCompleted = _ => { };
        private readonly AsyncAutoResetEvent m_resetEventForRead = new AsyncAutoResetEvent(false);
        private volatile Action<object> m_continuation;
        private ExceptionDispatchInfo m_exceptionDispatchInfo;
        private CancellationTokenRegistration m_tokenRegistration;
        private object m_userState;

        #endregion 字段

        public ExceptionDispatchInfo ExceptionDispatchInfo { get => this.m_exceptionDispatchInfo; }

        public abstract TResult GetResult();

        protected void Cancel()
        {
            this.SetException(new OperationCanceledException());
        }

        protected virtual void Reset()
        {
            this.m_resetEventForRead.Reset();
            this.m_continuation = null;
            this.m_exceptionDispatchInfo = null;
            this.m_tokenRegistration = default;
            this.m_userState = null;
        }

        protected void Complete(bool scheduler)
        {
            this.m_resetEventForRead.Set();

            var c = this.m_continuation;

            if (c != null || (c = Interlocked.CompareExchange(ref this.m_continuation, s_continuationCompleted, null)) != null)
            {
                var continuationState = this.m_userState;
                this.m_userState = null;
                this.m_continuation = s_continuationCompleted; // in case someone's polling IsCompleted

                if (scheduler)
                {
                    this.Scheduler(c, continuationState);
                }
                else
                {
                    c.Invoke(continuationState);
                }
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.DisposedValue)
            {
                return;
            }

            if (disposing)
            {
                this.Cancel();
            }
            base.Dispose(disposing);
        }

        protected abstract void Scheduler(Action<object> action, object state);

        protected void SetException(Exception exception)
        {
            this.m_exceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
            this.Complete(false);
        }

        protected ValueTask<TResult> ValueWaitAsync(CancellationToken token)
        {
            this.ThrowIfDisposed();
            token.ThrowIfCancellationRequested();

            if (token.CanBeCanceled)
            {
                if (this.m_tokenRegistration == default)
                {
                    this.m_tokenRegistration = token.Register(this.Cancel);
                }
                else
                {
                    this.m_tokenRegistration.Dispose();
                    this.m_tokenRegistration = token.Register(this.Cancel);
                }
            }

            return new ValueTask<TResult>(this, 0);
        }

        protected async Task<TResult> WaitAsync(CancellationToken token)
        {
            this.ThrowIfDisposed();
            token.ThrowIfCancellationRequested();

            if (token.CanBeCanceled)
            {
                if (this.m_tokenRegistration == default)
                {
                    this.m_tokenRegistration = token.Register(this.Cancel);
                }
                else
                {
                    this.m_tokenRegistration.Dispose();
                    this.m_tokenRegistration = token.Register(this.Cancel);
                }
            }

            await this.m_resetEventForRead.WaitOneAsync(token).ConfigureFalseAwait();
            return this.GetResult();
        }

        #region IValueTaskSource

        TResult IValueTaskSource<TResult>.GetResult(short token)
        {
            this.m_continuation = null;
            if (this.m_exceptionDispatchInfo != null)
            {
                var exceptionDispatchInfo = this.m_exceptionDispatchInfo;
                this.m_exceptionDispatchInfo = null;
                exceptionDispatchInfo.Throw();
            }

            return this.GetResult();
        }

        ValueTaskSourceStatus IValueTaskSource<TResult>.GetStatus(short token)
        {
            return !ReferenceEquals(this.m_continuation, s_continuationCompleted) ? ValueTaskSourceStatus.Pending :
                   ValueTaskSourceStatus.Succeeded;
        }

        void IValueTaskSource<TResult>.OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            this.m_userState = state;
            //Interlocked.CompareExchange(ref this.m_continuation, continuation, null);
            var prevContinuation = Interlocked.CompareExchange(ref this.m_continuation, continuation, null);
            if (ReferenceEquals(prevContinuation, s_continuationCompleted))
            {
                this.m_userState = null;
                this.Scheduler(continuation, state);
            }
        }

        #endregion IValueTaskSource
    }
}