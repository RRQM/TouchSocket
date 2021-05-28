//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Net.Sockets;
using System.Threading;

namespace RRQMSocket
{
    internal class AcceptQueue : IDisposable
    {
        internal AcceptQueue()
        {
            this.waitHandle = new AutoResetEvent(false);
            this.sockets = new System.Collections.Concurrent.ConcurrentQueue<Socket>();
        }

        internal Thread Thread;
        internal EventWaitHandle waitHandle;
        internal System.Collections.Concurrent.ConcurrentQueue<Socket> sockets;

        public void Dispose()
        {
            this.waitHandle.Dispose();
            while (this.sockets.TryDequeue(out _))
            {
            }
        }
    }
}