//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace TouchSocket.Core.ByteManager
{
    /// <summary>
    /// 字节块集合
    /// </summary>
    [DebuggerDisplay("Count = {bytesQueue.Count}")]
    internal class BytesQueue
    {
        internal int size;

        internal BytesQueue(int size)
        {
            this.size = size;
        }

        /// <summary>
        /// 占用空间
        /// </summary>
        public long FullSize => this.size * this.bytesQueue.Count;

        private readonly ConcurrentQueue<byte[]> bytesQueue = new ConcurrentQueue<byte[]>();

        internal long referenced;

        /// <summary>
        /// 获取当前实例中的空闲的Block
        /// </summary>
        /// <returns></returns>
        public bool TryGet(out byte[] bytes)
        {
            this.referenced++;
            return this.bytesQueue.TryDequeue(out bytes);
        }

        /// <summary>
        /// 向当前集合添加Block
        /// </summary>
        /// <param name="bytes"></param>
        public void Add(byte[] bytes)
        {
            this.bytesQueue.Enqueue(bytes);
        }

        internal void Clear()
        {
            this.bytesQueue.Clear();
        }
    }
}