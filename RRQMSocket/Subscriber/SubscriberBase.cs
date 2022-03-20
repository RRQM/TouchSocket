//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
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
        internal IProtocolClientBase client;

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
        public bool CanUse => this.client == null ? false : (this.client.Online ? true : false);

        /// <summary>
        /// 客户端
        /// </summary>
        public IProtocolClientBase Client => this.client;

        /// <summary>
        /// 协议
        /// </summary>
        public short Protocol => this.protocol;

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