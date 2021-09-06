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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RRQMCore.ByteManager
{
    /// <summary>
    /// 字节块集合
    /// </summary>
    [DebuggerDisplay("Count = {bytes.Count}")]
    public class BytesCollection
    {
        internal long size;

        internal BytesCollection(long size)
        {
            this.size = size;
        }

        /// <summary>
        /// 可用空间
        /// </summary>
        public long FreeSize { get { return this.size * this.bytes.Count; } }

        /// <summary>
        /// 所属字节池
        /// </summary>
        public BytePool BytePool { get; internal set; }

        private ConcurrentQueue<ByteBlock> bytes = new ConcurrentQueue<ByteBlock>();

        /// <summary>
        /// 获取当前实例中的空闲的Block
        /// </summary>
        /// <returns></returns>
        public bool TryGet(out ByteBlock byteBlock)
        {
            return this.bytes.TryDequeue(out byteBlock);
        }

        /// <summary>
        /// 向当前集合添加Block
        /// </summary>
        /// <param name="byteBlock"></param>
        public void Add(ByteBlock byteBlock)
        {
            byteBlock.BytesCollection = this;
            this.bytes.Enqueue(byteBlock);
        }

        internal List<ByteBlock> ToList()
        {
            return this.bytes.ToList();
        }
    }
}