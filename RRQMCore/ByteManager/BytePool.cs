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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RRQMCore.ByteManager
{
    /// <summary>
    /// 字节池
    /// </summary>
    public static class BytePool
    {
        private static ConcurrentDictionary<long, BytesQueue> bytesDictionary = new ConcurrentDictionary<long, BytesQueue>();

        private static bool autoZero;
        private static long fullSize;
        private static int keyCapacity;
        private static int maxBlockSize;
        private static long maxSize;
        private static int minBlockSize;

        static BytePool()
        {
            keyCapacity = 100;
            autoZero = false;
            maxSize = 1024 * 1024 * 512;
            SetBlockSize(1024, 1024 * 1024 * 20);
            AddSizeKey(10240);
        }

        /// <summary>
        /// 回收内存时，自动归零
        /// </summary>
        public static bool AutoZero
        {
            get { return autoZero; }
            set { autoZero = value; }
        }

        /// <summary>
        /// 键容量
        /// </summary>
        public static int KeyCapacity
        {
            get { return keyCapacity; }
            set { keyCapacity = value; }
        }

        /// <summary>
        /// 单个块最大值
        /// </summary>
        public static int MaxBlockSize
        {
            get { return maxBlockSize; }
        }

        /// <summary>
        /// 允许的内存池最大值
        /// </summary>
        public static long MaxSize
        {
            get { return maxSize; }
            set
            {
                if (value < 1024)
                {
                    value = 1024;
                }
                maxSize = value;
            }
        }

        /// <summary>
        /// 单个块最小值
        /// </summary>
        public static int MinBlockSize
        {
            get { return minBlockSize; }
        }

        /// <summary>
        /// 添加尺寸键
        /// </summary>
        /// <param name="byteSize"></param>
        /// <returns></returns>
        public static bool AddSizeKey(int byteSize)
        {
            if (bytesDictionary.TryAdd(byteSize, new BytesQueue(byteSize)))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 清理
        /// </summary>
        public static void Clear()
        {
            bytesDictionary.Clear();
            GC.Collect();
        }

        /// <summary>
        /// 确定是否包含指定尺寸键
        /// </summary>
        /// <param name="byteSize"></param>
        /// <returns></returns>
        public static bool ContainsSizeKey(int byteSize)
        {
            return bytesDictionary.ContainsKey(byteSize);
        }

        /// <summary>
        /// 获取所以内存键
        /// </summary>
        /// <returns></returns>
        public static long[] GetAllSizeKeys()
        {
            return bytesDictionary.Keys.ToArray();
        }

        /// <summary>
        /// 获取ByteBlock
        /// </summary>
        /// <param name="byteSize">长度</param>
        /// <param name="equalSize">要求长度相同</param>
        /// <returns></returns>
        public static ByteBlock GetByteBlock(int byteSize, bool equalSize)
        {
            ByteBlock byteBlock = new ByteBlock(GetByteCore(byteSize, equalSize));
            return byteBlock;
        }

        /// <summary>
        /// 获取ByteBlock
        /// </summary>
        /// <param name="byteSize"></param>
        /// <returns></returns>
        public static ByteBlock GetByteBlock(int byteSize)
        {
            if (byteSize < minBlockSize)
            {
                byteSize = minBlockSize;
            }
            return GetByteBlock(byteSize, false);
        }

        /// <summary>
        /// 获取最大长度的ByteBlock
        /// </summary>
        /// <returns></returns>
        public static ByteBlock GetByteBlock()
        {
            return GetByteBlock(maxBlockSize, true);
        }

        /// <summary>
        /// 获取内存池容量
        /// </summary>
        /// <returns></returns>
        public static long GetPoolSize()
        {
            long size = 0;
            foreach (var item in bytesDictionary.Values)
            {
                size += item.FullSize;
            }
            return size;
        }

        /// <summary>
        /// 移除尺寸键
        /// </summary>
        /// <param name="byteSize"></param>
        /// <returns></returns>
        public static bool RemoveSizeKey(int byteSize)
        {
            if (bytesDictionary.TryRemove(byteSize, out BytesQueue queue))
            {
                queue.Clear();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 设置内存块参数
        /// </summary>
        /// <param name="minBlockSize"></param>
        /// <param name="maxBlockSize"></param>
        public static void SetBlockSize(int minBlockSize, int maxBlockSize)
        {
            BytePool.maxBlockSize = maxBlockSize;
            BytePool.minBlockSize = minBlockSize;
            bytesDictionary.Clear();
        }

        /// <summary>
        /// 获取内存核心
        /// </summary>
        /// <param name="byteSize"></param>
        /// <param name="equalSize"></param>
        /// <returns></returns>
        public static byte[] GetByteCore(int byteSize, bool equalSize)
        {
            BytesQueue bytesCollection;
            if (equalSize)
            {
                //等长
                if (bytesDictionary.TryGetValue(byteSize, out bytesCollection))
                {
                    if (bytesCollection.TryGet(out byte[] bytes))
                    {
                        fullSize -= byteSize;
                        return bytes;
                    }
                }
                else
                {
                    CheckKeyCapacity(byteSize);
                }
                return new byte[byteSize];
            }
            else
            {
                byteSize = HitSize(byteSize);
                //搜索已创建集合
                if (bytesDictionary.TryGetValue(byteSize, out bytesCollection))
                {
                    if (bytesCollection.TryGet(out byte[] bytes))
                    {
                        fullSize -= byteSize;
                        return bytes;
                    }
                }
                else
                {
                    CheckKeyCapacity(byteSize);
                }
                return new byte[byteSize];
            }
        }

        /// <summary>
        /// 回收内存核心
        /// </summary>
        /// <param name="bytes"></param>
        public static void Recycle(byte[] bytes)
        {
            if (maxSize > fullSize)
            {
                if (bytesDictionary.TryGetValue(bytes.Length, out BytesQueue bytesQueue))
                {
                    if (autoZero)
                    {
                        Array.Clear(bytes, 0, bytes.Length);
                    }
                    fullSize += bytes.Length;
                    bytesQueue.Add(bytes);
                }
            }
            else
            {
                long size = 0;
                foreach (var collection in bytesDictionary.Values)
                {
                    size += collection.FullSize;
                }
                fullSize = size;
            }
        }

        private static void CheckKeyCapacity(int byteSize)
        {
            if (byteSize < minBlockSize || byteSize > maxBlockSize)
            {
                return;
            }
            if (bytesDictionary.Count < keyCapacity)
            {
                bytesDictionary.TryAdd(byteSize, new BytesQueue(byteSize));
            }
            else
            {
                List<BytesQueue> bytesQueues = bytesDictionary.Values.ToList();
                bytesQueues.Sort((x, y) => { return x.referenced > y.referenced ? -1 : 1; });
                for (int i = (int)(bytesQueues.Count * 0.2); i < bytesQueues.Count; i++)
                {
                    if (bytesDictionary.TryRemove(bytesQueues[i].size, out BytesQueue queue))
                    {
                        queue.Clear();
                    }
                }
            }
        }

        private static int HitSize(int num)
        {
            switch (num)
            {
                case <= 1024:
                    {
                        return 1024;
                    }
                case <= 2048:
                    {
                        return 2048;
                    }
                case <= 4096:
                    {
                        return 4096;
                    }
                case <= 8192:
                    {
                        return 8192;
                    }
                case <= 10240:
                    {
                        return 10240;
                    }
                case <= 16384:
                    {
                        return 16384;
                    }
                case <= 32768:
                    {
                        return 32768;
                    }
                case <= 65536:
                    {
                        return 65536;
                    }
                case <= 131072:
                    {
                        return 131072;
                    }
                case <= 262144:
                    {
                        return 262144;
                    }
                case <= 524288:
                    {
                        return 524288;
                    }
                case <= 1048576:
                    {
                        return 1048576;
                    }
                case <= 2097152:
                    {
                        return 2097152;
                    }
                case <= 4194304:
                    {
                        return 4194304;
                    }
                case <= 8388608:
                    {
                        return 8388608;
                    }
                case <= 16777216:
                    {
                        return 16777216;
                    }
                case <= 33554432:
                    {
                        return 33554432;
                    }
                case <= 67108864:
                    {
                        return 67108864;
                    }
                case <= 134217728:
                    {
                        return 134217728;
                    }
                default:
                    return num;
            }

            //U3D无法编译时替换。

            //if (num <= 1024)
            //{
            //    return 1024;
            //}
            //else if (num <= 2048)
            //{
            //    return 2048;
            //}
            //else if (num <= 4096)
            //{
            //    return 4096;
            //}
            //else if (num <= 8192)
            //{
            //    return 8192;
            //}
            //else if (num <= 10240)
            //{
            //    return 10240;
            //}
            //else if (num <= 16384)
            //{
            //    return 16384;
            //}
            //else if (num <= 32768)
            //{
            //    return 32768;
            //}
            //else if (num <= 65536)
            //{
            //    return 65536;
            //}
            //else if (num <= 131072)
            //{
            //    return 131072;
            //}
            //else if (num <= 262144)
            //{
            //    return 262144;
            //}
            //else if (num <= 524288)
            //{
            //    return 524288;
            //}
            //else if (num <= 1048576)
            //{
            //    return 1048576;
            //}
            //else if (num <= 2097152)
            //{
            //    return 2097152;
            //}
            //else if (num <= 4194304)
            //{
            //    return 4194304;
            //}
            //else if (num <= 8388608)
            //{
            //    return 8388608;
            //}
            //else if (num <= 16777216)
            //{
            //    return 16777216;
            //}
            //else if (num <= 33554432)
            //{
            //    return 33554432;
            //}
            //else if (num <= 67108864)
            //{
            //    return 67108864;
            //}
            //else if (num <= 134217728)
            //{
            //    return 134217728;
            //}
            //else
            //{
            //    return num;
            //}
        }
    }
}