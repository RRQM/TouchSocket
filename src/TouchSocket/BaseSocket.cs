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
        /// 通讯基类
        /// </summary>
        public BaseSocket()
        {
            this.SyncRoot = new object();
        }

        /// <summary>
        /// 数据交互缓存池限制，min=1024 byte
        /// </summary>
        public int BufferLength { get; private set; } = 64 * 1024;

        /// <summary>
        /// 设置数据交互缓存池尺寸，min=1024 byte。
        /// 一般情况下该值用于三个方面，包括：socket的发送、接收缓存，及内存池的默认申请。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual int SetBufferLength(int value)
        {
            if (value < 1024)
            {
                value = 1024;
            }
            this.BufferLength = value;
            return this.BufferLength;
        }

        /// <summary>
        /// 同步根。
        /// </summary>
        protected readonly object SyncRoot;

        /// <summary>
        /// 日志记录器
        /// </summary>
        public ILog Logger { get; set; }
    }
}