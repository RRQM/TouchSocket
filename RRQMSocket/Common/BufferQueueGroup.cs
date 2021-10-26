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
using RRQMCore.ByteManager;
using System;
using System.Threading;

namespace RRQMSocket
{
    /// <summary>
    /// 多线程队列组
    /// </summary>
    public class BufferQueueGroup : IDisposable
    {
        internal Thread Thread;
        internal BufferQueue bufferAndClient;
        internal EventWaitHandle waitHandleBuffer;
        internal BytePool bytePool;

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            if (bufferAndClient != null)
            {
                while (bufferAndClient.TryDequeue(out _))
                {
                }
            }

            if (waitHandleBuffer != null)
            {
                waitHandleBuffer.Set();
            }
        }
    }
}