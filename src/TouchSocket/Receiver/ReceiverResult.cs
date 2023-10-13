using System;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// ReceiverResult
    /// </summary>
    public readonly struct ReceiverResult : IDisposable
    {
        private readonly Action m_disAction;

        /// <summary>
        /// SocketReceiveResult
        /// </summary>
        /// <param name="disAction"></param>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        public ReceiverResult(Action disAction, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.m_disAction = disAction;
            this.ByteBlock = byteBlock;
            this.RequestInfo = requestInfo;
        }

        /// <summary>
        /// 字节块
        /// </summary>
        public ByteBlock ByteBlock { get; }

        /// <summary>
        /// 数据对象
        /// </summary>
        public IRequestInfo RequestInfo { get; }

        /// <summary>
        /// 连接已关闭
        /// </summary>
        public bool IsClosed => this.ByteBlock == null && this.RequestInfo == null;

        /// <inheritdoc/>
        public void Dispose()
        {
            m_disAction?.Invoke();
        }
    }
}
