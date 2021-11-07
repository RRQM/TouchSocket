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
    public class ProtocolSubscriber : IClientBase, IDisposable
    {
        /// <summary>
        /// 析构函数
        /// </summary>
        ~ProtocolSubscriber()
        {
            this.Dispose();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="receivedAction"></param>
        public ProtocolSubscriber(short protocol, Action<ProtocolSubscriber, ProtocolSubscriberEventArgs> receivedAction)
        {
            this.protocol = protocol;
            this.Received = receivedAction;
        }

        /// <summary>
        /// 客户端
        /// </summary>
        internal IProtocolClient client;
        /// <summary>
        /// 客户端
        /// </summary>
        public IProtocolClient Client
        {
            get { return client; }
        }

        /// <summary>
        /// 能否使用
        /// </summary>
        public bool CanUse
        {
            get { return client == null ? false : (client.Online ? true : false); }
        }

        private Action<ProtocolSubscriber, ProtocolSubscriberEventArgs> Received;

        private short protocol;
        /// <summary>
        /// 协议
        /// </summary>
        public short Protocol
        {
            get { return protocol; }
        }


        internal void OnReceived(ProtocolSubscriberEventArgs e)
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            if (this.client != null)
            {
                this.client.RemoveProtocolSubscriber(this);
            }
        }
    }
}
