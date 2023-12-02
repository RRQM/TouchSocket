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

using System;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// Receiver
    /// </summary>
    public class Receiver : DisposableObject, IReceiver
    {
        /// <summary>
        /// Receiver
        /// </summary>
        ~Receiver()
        {
            this.Dispose(false);
        }

        private readonly IClient m_client;
        private readonly AutoResetEvent m_resetEventForComplateRead = new AutoResetEvent(false);
        private readonly AsyncAutoResetEvent m_resetEventForRead = new AsyncAutoResetEvent(false);
        private ByteBlock m_byteBlock;
        private IRequestInfo m_requestInfo;

        /// <summary>
        /// Receiver
        /// </summary>
        /// <param name="client"></param>
        public Receiver(IClient client)
        {
            this.m_client = client;
        }

        /// <inheritdoc/>
        public async Task<ReceiverResult> ReadAsync(CancellationToken token)
        {
            this.ThrowIfDisposed();
            await this.m_resetEventForRead.WaitOneAsync(token).ConfigureFalseAwait();
            return new ReceiverResult(this.ComplateRead, this.m_byteBlock, this.m_requestInfo);
        }

#if NET6_0_OR_GREATER
        /// <inheritdoc/>
        public async ValueTask<ReceiverResult> ValueReadAsync(CancellationToken token)
        {
            this.ThrowIfDisposed();
            await this.m_resetEventForRead.WaitOneAsync(token).ConfigureFalseAwait();
            return new ReceiverResult(this.ComplateRead, this.m_byteBlock, this.m_requestInfo);
        }
#endif

        /// <inheritdoc/>
        public bool TryInputReceive(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.DisposedValue)
            {
                return false;
            }
            this.m_byteBlock = byteBlock;
            this.m_requestInfo = requestInfo;
            this.m_resetEventForRead.Set();
            if (byteBlock == null && requestInfo == null)
            {
                return true;
            }
            if (this.m_resetEventForComplateRead.WaitOne(TimeSpan.FromSeconds(10)))
            {
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_client.ClearReceiver();
            }
            else
            {
                this.m_resetEventForComplateRead.SafeDispose();
                this.m_resetEventForRead.SafeDispose();
            }
            this.m_byteBlock = null;
            base.Dispose(disposing);
        }

        private void ComplateRead()
        {
            this.m_byteBlock = default;
            this.m_requestInfo = default;
            this.m_resetEventForComplateRead.Set();
        }
    }
}