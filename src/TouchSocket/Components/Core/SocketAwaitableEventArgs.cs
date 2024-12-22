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
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks.Sources;

namespace TouchSocket.Sockets
{
    internal abstract class SocketAwaitableEventArgs : SocketAsyncEventArgs, IValueTaskSource<SocketOperationResult>
    {
        private static readonly Action<object> s_continuationCompleted = _ => { };

        private volatile Action<object> m_continuation;
        private readonly SocketOperationResult m_socketOperationResult = new SocketOperationResult();

        protected override void OnCompleted(SocketAsyncEventArgs _)
        {
            var c = this.m_continuation;

            if (c != null || (c = Interlocked.CompareExchange(ref this.m_continuation, s_continuationCompleted, null)) != null)
            {
                var continuationState = this.UserToken;
                this.UserToken = null;
                this.m_continuation = s_continuationCompleted; // in case someone's polling IsCompleted

#if NET6_0_OR_GREATER
                ThreadPool.UnsafeQueueUserWorkItem(c, continuationState, true);
#else
                void Run(object o)
                {
                    c.Invoke(o);
                }

                ThreadPool.UnsafeQueueUserWorkItem(Run, continuationState);
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected SocketOperationResult GetSocketOperationResult()
        {
            if (this.SocketError != SocketError.Success)
            {
                this.m_socketOperationResult.SocketError = CreateException(this.SocketError);
                this.m_socketOperationResult.BytesTransferred = default;
                this.m_socketOperationResult.RemoteEndPoint = default;
                this.m_socketOperationResult.ReceiveMessageFromPacketInfo = default;
            }
            else
            {
                this.m_socketOperationResult.SocketError = null;
                this.m_socketOperationResult.BytesTransferred = this.BytesTransferred;
                this.m_socketOperationResult.RemoteEndPoint = this.RemoteEndPoint;
                this.m_socketOperationResult.ReceiveMessageFromPacketInfo = this.ReceiveMessageFromPacketInfo;
            }

            return this.m_socketOperationResult;
        }

        #region IValueTaskSource

        public SocketOperationResult GetResult(short token)
        {
            this.m_continuation = null;

            return this.GetSocketOperationResult();
        }

        public ValueTaskSourceStatus GetStatus(short token)
        {
            if (!ReferenceEquals(this.m_continuation, s_continuationCompleted))
            {
                return ValueTaskSourceStatus.Pending;
            }
            else
            {
                return this.SocketError == SocketError.Success ? ValueTaskSourceStatus.Succeeded :
                    ValueTaskSourceStatus.Faulted;
            }
        }

        public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            this.UserToken = state;
            var prevContinuation = Interlocked.CompareExchange(ref this.m_continuation, continuation, null);
            if (ReferenceEquals(prevContinuation, s_continuationCompleted))
            {
                this.UserToken = null;

#if NET6_0_OR_GREATER
                ThreadPool.UnsafeQueueUserWorkItem(continuation, state, true);
#else
                void Run(object o)
                {
                    continuation.Invoke(o);
                }

                ThreadPool.UnsafeQueueUserWorkItem(Run, state);
#endif
            }
        }

        #endregion IValueTaskSource

        protected static SocketException CreateException(SocketError e)
        {
            return new SocketException((int)e);
        }
    }
}