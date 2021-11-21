using RRQMCore.ByteManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 协议订阅
    /// </summary>
    public class ProtocolSubscriber : SubscriberBase,  ISendBase, IDisposable
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="receivedAction"></param>
        public ProtocolSubscriber(short protocol, Action<ProtocolSubscriber, ProtocolSubscriberEventArgs> receivedAction):base(protocol)
        {
            this.Received = receivedAction;
        }


        private Action<ProtocolSubscriber, ProtocolSubscriberEventArgs> Received;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnReceived(ProtocolSubscriberEventArgs e)
        {
            this.Received?.Invoke(this, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void Send(byte[] buffer, int offset, int length)
        {
            client.Send(this.Protocol, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        public void Send(byte[] buffer)
        {
            this.Send(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        public void Send(ByteBlock byteBlock)
        {
            this.Send(byteBlock.Buffer, 0, byteBlock.Len);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void SendAsync(byte[] buffer, int offset, int length)
        {
            this.client.SendAsync(this.Protocol, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        public void SendAsync(byte[] buffer)
        {
            this.SendAsync(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        public void SendAsync(ByteBlock byteBlock)
        {
            this.SendAsync(byteBlock.Buffer, 0, byteBlock.Len);
        }
    }
}
