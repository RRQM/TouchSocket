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

namespace TouchSocket.Core
{
    /// <summary>
    /// 阻塞式操作。
    /// </summary>
    public abstract class BlockSegment<T> : ValueTaskSource<IBlockResult<T>>
    {
        #region 字段

        private readonly AsyncAutoResetEvent m_resetEventForComplateRead = new AsyncAutoResetEvent(false);
        private readonly BlockResult m_result;

        #endregion 字段

        public IBlockResult<T> Result => this.m_result;

        public BlockSegment()
        {
            this.m_result = new BlockResult(this.ComplateRead);
        }

        protected override void Reset()
        {
            this.m_resetEventForComplateRead.Reset();
            this.m_result.IsCompleted = false;
            this.m_result.Memory = default;
            this.m_result.Message = default;
            base.Reset();
        }

        protected override void Scheduler(Action<object> action, object state)
        {
            void Run(object o)
            {
                action.Invoke(o);
            }
            ThreadPool.UnsafeQueueUserWorkItem(Run, state);
        }

        protected override IBlockResult<T> GetResult()
        {
            return this.m_result;
        }

        protected async Task InputAsync(ReadOnlyMemory<T> memory)
        {
            this.m_result.Memory = memory;
            this.Complete(false);
            await this.m_resetEventForComplateRead.WaitOneAsync().ConfigureAwait(false);
        }

        protected async Task Complete(string msg)
        {
            try
            {
                this.m_result.IsCompleted = true;
                this.m_result.Message = msg;
                await this.InputAsync(default).ConfigureAwait(false);
            }
            catch
            {
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
                this.m_resetEventForComplateRead.Set();
                this.m_resetEventForComplateRead.SafeDispose();
            }
            base.Dispose(disposing);
        }

        private void ComplateRead()
        {
            this.m_resetEventForComplateRead.Set();
        }

        #region Class

        internal class BlockResult : IBlockResult<T>
        {
            private readonly Action m_disAction;

            /// <summary>
            /// ReceiverResult
            /// </summary>
            /// <param name="disAction"></param>
            public BlockResult(Action disAction)
            {
                this.m_disAction = disAction;
            }

            public ReadOnlyMemory<T> Memory { get; set; }
            public bool IsCompleted { get; set; }
            public string Message { get; set; }

            public void Dispose()
            {
                this.m_disAction.Invoke();
            }
        }

        #endregion Class
    }
}