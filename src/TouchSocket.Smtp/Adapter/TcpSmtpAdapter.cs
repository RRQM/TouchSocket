using System;
using System.Collections.Generic;
using System.Threading;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Smtp
{
    /// <summary>
    /// TcpSmtpAdapter
    /// </summary>
    public class TcpSmtpAdapter : CustomFixedHeaderByteBlockDataHandlingAdapter<SmtpMessage>
    {
        private SpinLock m_locker = new SpinLock();

        /// <inheritdoc/>
        public override bool CanSendRequestInfo => true;

        /// <inheritdoc/>
        public override bool CanSplicingSend => false;

        /// <inheritdoc/>
        public override int HeaderLength => 6;

        /// <inheritdoc/>
        protected override SmtpMessage GetInstance()
        {
            return new SmtpMessage();
        }

        /// <inheritdoc/>
        protected override void OnReceivedSuccess(SmtpMessage request)
        {
            request.SafeDispose();
        }

        /// <inheritdoc/>
        protected override void PreviewSend(IRequestInfo requestInfo)
        {
            if (!(requestInfo is SmtpMessage message))
            {
                throw new Exception($"无法将{nameof(requestInfo)}转换为{nameof(SmtpMessage)}");
            }
            using (var byteBlock = new ByteBlock(message.GetLength()))
            {
                message.Build(byteBlock);
                this.GoSend(byteBlock.Buffer, 0, byteBlock.Len);
            }
        }

        /// <inheritdoc/>
        protected override void PreviewSend(IList<ArraySegment<byte>> transferBytes)
        {
            if (transferBytes.Count == 0)
            {
                return;
            }

            var length = 0;
            foreach (var item in transferBytes)
            {
                length += item.Count;
            }

            if (length > this.MaxPackageSize)
            {
                throw new Exception("发送数据大于设定值，相同解析器可能无法收到有效数据，已终止发送");
            }

            var lockTaken = false;
            try
            {
                this.m_locker.Enter(ref lockTaken);
                foreach (var item in transferBytes)
                {
                    this.GoSend(item.Array, item.Offset, item.Count);
                }
            }
            finally
            {
                if (lockTaken)
                {
                    this.m_locker.Exit(false);
                }
            }
        }
    }
}