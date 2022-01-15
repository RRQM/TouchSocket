//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Log;
using System;

namespace RRQMSocket
{
    /// <summary>
    /// 通讯基类
    /// </summary>
    public abstract class BaseSocket : ISocket, IDisposable
    {
        private int bufferLength;

        /// <summary>
        /// 日志记录器
        /// </summary>
        protected ILog logger;

        /// <summary>
        /// 判断是否已释放资源
        /// </summary>
        protected bool disposable = false;

        /// <summary>
        /// 数据交互缓存池限制，min=1024 byte，Max=10Mb byte
        /// </summary>
        public int BufferLength
        {
            get { return bufferLength; }
            set
            {
                if (value < 1024 || value > 1024 * 1024 * 10)
                {
                    value = 1024 * 64;
                }
                bufferLength = value;
            }
        }

        /// <summary>
        /// 日志记录器
        /// </summary>
        public ILog Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            this.disposable = true;
        }
    }
}