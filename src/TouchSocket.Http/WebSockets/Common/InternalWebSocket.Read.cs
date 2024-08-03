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
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Http.WebSockets
{
    internal sealed partial class InternalWebSocket : ValueTaskSource<IWebSocketReceiveResult>, IWebSocket
    {
        private readonly WebSocketReceiveResult m_receiverResult;
        private readonly AsyncAutoResetEvent m_resetEventForComplateRead = new AsyncAutoResetEvent(false);

        public Task<IWebSocketReceiveResult> ReadAsync(CancellationToken token)
        {
            this.ThrowIfNotAllowAsyncRead();
            return this.m_receiverResult.IsCompleted ? Task.FromResult<IWebSocketReceiveResult>(this.m_receiverResult) : this.WaitAsync(token);
        }

        public ValueTask<IWebSocketReceiveResult> ValueReadAsync(CancellationToken token)
        {
            this.ThrowIfNotAllowAsyncRead();
            return this.m_receiverResult.IsCompleted
                ? EasyValueTask.FromResult<IWebSocketReceiveResult>(this.m_receiverResult)
                : this.ValueWaitAsync(token);
        }

        private void ThrowIfNotAllowAsyncRead()
        {
            if (!this.m_allowAsyncRead)
            {
                ThrowHelper.ThrowNotSupportedException(TouchSocketHttpResource.NotAllowAsyncRead);
            }
        }

        protected override void Scheduler(Action<object> action, object state)
        {
            void Run(object o)
            {
                action.Invoke(o);
            }
            ThreadPool.UnsafeQueueUserWorkItem(Run, state);
        }

        protected override IWebSocketReceiveResult GetResult()
        {
            return this.m_receiverResult;
        }

        internal Task InputReceiveAsync(WSDataFrame dataFrame)
        {
            this.m_receiverResult.DataFrame = dataFrame;
            base.Complete(false);
            return this.m_resetEventForComplateRead.WaitOneAsync();
        }

        public async Task Complete(string msg)
        {
            try
            {
                this.m_receiverResult.IsCompleted = true;
                this.m_receiverResult.Message = msg;
                await this.InputReceiveAsync(default).ConfigureAwait(false);
            }
            catch
            {
            }
        }

        private void ComplateRead()
        {
            this.m_receiverResult.DataFrame = default;
            this.m_receiverResult.Message = default;
            this.m_resetEventForComplateRead.Set();
        }

        #region Class

        internal sealed class WebSocketReceiveResult : IWebSocketReceiveResult
        {
            private readonly Action m_disAction;

            public WebSocketReceiveResult(Action disAction)
            {
                this.m_disAction = disAction;
            }

            public void Dispose()
            {
                this.m_disAction.Invoke();
            }

            public WSDataFrame DataFrame { get; set; }

            public bool IsCompleted { get; set; }

            public string Message { get; set; }
        }

        #endregion Class
    }
}