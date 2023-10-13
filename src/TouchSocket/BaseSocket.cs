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
//------------------------------------------------------------------------------
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 通讯基类
    /// </summary>
    public abstract class BaseSocket : DependencyObject, ISocket
    {
        /// <summary>
        /// 同步根。
        /// </summary>
        protected readonly object SyncRoot = new object();

        private int m_receiveBufferSize = 1024 * 10;
        private int m_sendBufferSize = 1024 * 10;

        /// <inheritdoc/>
        public virtual int SendBufferSize
        {
            get => this.m_sendBufferSize;
            set => this.m_sendBufferSize = value < 1024 ? 1024 : value;
        }

        /// <inheritdoc/>
        public virtual int ReceiveBufferSize
        {
            get => this.m_receiveBufferSize;
            set => this.m_receiveBufferSize = value < 1024 ? 1024 : value;
        }

        /// <inheritdoc/>
        public ILog Logger { get; set; }
    }
}