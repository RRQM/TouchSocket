using System;

namespace RRQMSocket
{
    /// <summary>
    /// 订阅者基类
    /// </summary>
    public abstract class SubscriberBase : ISubscriber
    {
        /// <summary>
        /// 客户端
        /// </summary>
        internal IProtocolClient client;

        /// <summary>
        /// 协议
        /// </summary>
        protected short protocol;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="protocol"></param>
        public SubscriberBase(short protocol)
        {
            this.protocol = protocol;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~SubscriberBase()
        {
            this.Dispose();
        }

        /// <summary>
        /// 能否使用
        /// </summary>
        public bool CanUse
        {
            get { return client == null ? false : (client.Online ? true : false); }
        }

        /// <summary>
        /// 客户端
        /// </summary>
        public IProtocolClient Client
        {
            get { return client; }
        }

        /// <summary>
        /// 协议
        /// </summary>
        public short Protocol
        {
            get { return protocol; }
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

        internal void OnInternalReceived(ProtocolSubscriberEventArgs e)
        {
            this.OnReceived(e);
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="e"></param>
        protected abstract void OnReceived(ProtocolSubscriberEventArgs e);
    }
}