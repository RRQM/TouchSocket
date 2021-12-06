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
using RRQMCore.Exceptions;
using System;
using System.Collections.Concurrent;

namespace RRQMCore.ByteManager
{
    /// <summary>
    /// 字节池
    /// </summary>
    public class BytePool
    {
        private static BytePool bytePool = new BytePool(1024 * 1024 * 512, 1024 * 1024 * 5);

        private ConcurrentDictionary<long, BytesQueue> bytesDictionary = new ConcurrentDictionary<long, BytesQueue>();

        private long createdBlockSize;

        private long fullSize;

        private int maxBlockSize = 1024 * 1024;

        private long maxSize = 1024 * 1024 * 100;

        private int minBlockSize = 1024 * 64;

        /// <summary>
        /// 构造函数
        /// </summary>
        public BytePool()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxSize">字节池最大值</param>
        /// <param name="maxBlockSize">单个Block最大值</param>
        public BytePool(long maxSize, int maxBlockSize)
        {
            this.maxSize = maxSize;
            this.maxBlockSize = maxBlockSize;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxSize">字节池最大值</param>
        public BytePool(long maxSize) : this(maxSize, 1024 * 1024)
        {
            this.maxSize = maxSize;
        }

        /// <summary>
        /// 默认内存池，
        /// 内存池最大512Mb，
        /// 单体最大5Mb。
        /// </summary>
        public static BytePool Default { get { return bytePool; } }

        /// <summary>
        /// 已创建的块的最大值
        /// </summary>
        public long CreatedBlockSize
        {
            get { return createdBlockSize; }
        }


        /// <summary>
        /// 单个块最大值，默认为1024*1024字节
        /// </summary>
        public int MaxBlockSize
        {
            get { return maxBlockSize; }
            set { maxBlockSize = value; }
        }

        /// <summary>
        /// 允许的内存池最大值,默认为100M Byte
        /// </summary>
        public long MaxSize
        {
            get { return maxSize; }
            set { maxSize = value; }
        }

        /// <summary>
        /// 单个块最小值，默认64*1024字节
        /// </summary>
        public int MinBlockSize
        {
            get { return minBlockSize; }
            set { minBlockSize = value; }
        }

        /// <summary>
        /// 获取ByteBlock
        /// </summary>
        /// <param name="byteSize">长度</param>
        /// <param name="equalSize">要求长度相同</param>
        /// <returns></returns>
        public ByteBlock GetByteBlock(long byteSize, bool equalSize)
        {
            ByteBlock byteBlock = new ByteBlock(this.GetBytesCore(byteSize, equalSize));
            if (byteSize < maxBlockSize)
            {
                byteBlock._bytePool = this;
            }
            return byteBlock;
        }

        /// <summary>
        /// 获取ByteBlock
        /// </summary>
        /// <param name="byteSize"></param>
        /// <returns></returns>
        public ByteBlock GetByteBlock(long byteSize)
        {
            if (byteSize < this.minBlockSize)
            {
                return this.GetByteBlock(this.minBlockSize, false);
            }
            return this.GetByteBlock(byteSize, false);
        }

        /// <summary>
        /// 获取最大长度的ByteBlock
        /// </summary>
        /// <returns></returns>
        public ByteBlock GetByteBlock()
        {
            return this.GetByteBlock(this.MaxBlockSize, true);
        }

        internal void Recycle(byte[] bytes)
        {
            this.createdBlockSize = Math.Max(CreatedBlockSize, bytes.Length);
            if (maxSize > fullSize)
            {
                this.fullSize += bytes.Length;
                BytesQueue bytesCollection = this.bytesDictionary.GetOrAdd(bytes.Length, (size) =>
                {
                    return new BytesQueue(size);
                });
                bytesCollection.Add(bytes);
            }
            else
            {
                long size = 0;
                foreach (var collection in this.bytesDictionary.Values)
                {
                    size += collection.FullSize;
                }
                this.fullSize = size;
            }
        }

        /// <summary>
        /// 清理
        /// </summary>
        public void Clear()
        {
            this.bytesDictionary.Clear();
        }

        private byte[] GetBytesCore(long byteSize, bool equalSize)
        {
            if (byteSize < 0)
            {
                throw new RRQMException("申请内存的长度不能小于0");
            }

            if (byteSize > maxBlockSize)
            {
                return new byte[byteSize];
            }

            if (this.createdBlockSize < byteSize)
            {
                return new byte[byteSize];
            }
            else
            {
                BytesQueue bytesCollection;
                //搜索已创建集合
                if (bytesDictionary.TryGetValue(byteSize, out bytesCollection))
                {
                    if (bytesCollection.TryGet(out byte[] bytes))
                    {
                        this.fullSize -= byteSize;
                        return bytes;
                    }
                }

                if (!equalSize)
                {
                    foreach (var size in bytesDictionary.Keys)
                    {
                        if (size > byteSize)
                        {
                            if (this.bytesDictionary.TryGetValue(size, out bytesCollection))
                            {
                                if (bytesCollection.TryGet(out byte[] bytes))
                                {
                                    this.fullSize -= byteSize;
                                    return bytes;
                                }
                            }
                        }
                    }
                }
                return new byte[byteSize];
            }
        }
    }
}