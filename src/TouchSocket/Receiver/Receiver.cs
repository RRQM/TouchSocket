using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
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