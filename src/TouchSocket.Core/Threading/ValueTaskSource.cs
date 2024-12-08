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
    /// <summary>
    /// 提供异步操作的值任务源抽象类。
    /// </summary>
    /// <typeparam name="TResult">结果类型。</typeparam>
    public abstract class ValueTaskSource<TResult> : DisposableObject, IValueTaskSource<TResult>
    {
        #region 字段

        /// <summary>
        /// 表示继续操作已完成的静态操作委托。
        /// </summary>
        private static readonly Action<object> s_continuationCompleted = _ => { };

        /// <summary>
        /// 异步操作继续操作委托。
        /// </summary>
        private volatile Action<object> m_continuation;
        /// <summary>
        /// 异常分发信息。
        /// </summary>
        private ExceptionDispatchInfo m_exceptionDispatchInfo;
        /// <summary>
        /// 取消令牌注册。
        /// </summary>
        private CancellationTokenRegistration m_tokenRegistration;
        /// <summary>
        /// 用户状态对象。
        /// </summary>
        private object m_userState;

        #endregion 字段

        /// <summary>
        /// 获取异常分发信息。
        /// </summary>
        protected ExceptionDispatchInfo ExceptionDispatchInfo => this.m_exceptionDispatchInfo;

        /// <summary>
        /// 获取结果。
        /// </summary>
        /// <returns>操作结果。</returns>
        protected abstract TResult GetResult();

        /// <summary>
        /// 取消操作。
        /// </summary>
        protected void Cancel()
        {
            this.SetException(new OperationCanceledException());
        }

        /// <summary>
        /// 重置操作状态。
        /// </summary>
        protected virtual void Reset()
        {
            //this.m_resetEventForRead.Reset();
            this.m_continuation = null;
            this.m_exceptionDispatchInfo = null;
            this.m_tokenRegistration = default;
            this.m_userState = null;
        }

        /// <summary>
        /// 完成操作。
        /// </summary>
        /// <param name="scheduler">是否使用调度器。</param>
        protected void Complete(bool scheduler)
        {
            //this.m_resetEventForRead.Set();

            var c = this.m_continuation;

            if (c != null || (c = Interlocked.CompareExchange(ref this.m_continuation, s_continuationCompleted, null)) != null)
            {
                var continuationState = this.m_userState;
                this.m_userState = null;
                this.m_continuation = s_continuationCompleted; // 防止有人轮询IsCompleted

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

        /// <summary>
        /// 调度继续操作。
        /// </summary>
        /// <param name="action">继续操作委托。</param>
        /// <param name="state">状态对象。</param>
        protected abstract void Scheduler(Action<object> action, object state);

        /// <summary>
        /// 设置异常。
        /// </summary>
        /// <param name="exception">异常对象。</param>
        protected void SetException(Exception exception)
        {
            this.m_exceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
            this.Complete(false);
        }

        /// <summary>
        /// 值等待异步操作。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>值任务。</returns>
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

        #region IValueTaskSource

        /// <summary>
        /// 获取结果。
        /// </summary>
        /// <param name="token">令牌。</param>
        /// <returns>操作结果。</returns>
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
            return this.GetStatus(token);
        }

        /// <summary>
        /// 获取状态。
        /// </summary>
        /// <param name="token">令牌。</param>
        /// <returns>操作状态。</returns>
        protected virtual ValueTaskSourceStatus GetStatus(short token)
        {
            return !ReferenceEquals(this.m_continuation, s_continuationCompleted) ? ValueTaskSourceStatus.Pending :
                       ValueTaskSourceStatus.Succeeded;
        }

        /// <summary>
        /// 操作完成时调用。
        /// </summary>
        /// <param name="continuation">继续操作委托。</param>
        /// <param name="state">状态对象。</param>
        /// <param name="token">令牌。</param>
        /// <param name="flags">标志。</param>
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