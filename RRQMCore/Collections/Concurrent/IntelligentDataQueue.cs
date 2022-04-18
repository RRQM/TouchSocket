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
using System.Threading;

namespace RRQMCore.Collections.Concurrent
{
    /// <summary>
    /// 智能数据安全队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IntelligentDataQueue<T> : ConcurrentQueue<T> where T : IQueueData
    {
        private bool m_overflowWait;

        /// <summary>
        /// 溢出等待
        /// </summary>
        public bool OverflowWait
        {
            get => this.m_overflowWait;
            set => this.m_overflowWait = value;
        }

        private Action<bool> m_onQueueChanged;

        /// <summary>
        /// 在队列修改时
        /// </summary>
        public Action<bool> OnQueueChanged
        {
            get => this.m_onQueueChanged;
            set => this.m_onQueueChanged = value;
        }

        private bool m_free;

        /// <summary>
        /// 是否有空位允许入队
        /// </summary>
        public bool Free => this.m_free;

        private long m_actualSize;

        private long m_maxSize;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxSize"></param>
        public IntelligentDataQueue(long maxSize)
        {
            this.m_free = true;
            this.m_overflowWait = true;
            this.MaxSize = maxSize;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public IntelligentDataQueue() : this(1024 * 1024 * 10)
        {
        }

        /// <summary>
        /// 允许的最大长度
        /// </summary>
        public long MaxSize
        {
            get => this.m_maxSize;
            set
            {
                if (value < 1)
                {
                    value = 1;
                }
                this.m_maxSize = value;
            }
        }

        /// <summary>
        /// 实际尺寸
        /// </summary>
        public long ActualSize => this.m_actualSize;

        /// <summary>
        /// 清空队列
        /// </summary>
        public void Clear(Action<T> onClear)
        {
            while (base.TryDequeue(out T t))
            {
                onClear?.Invoke(t);
            }
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="item"></param>
        public new void Enqueue(T item)
        {
            lock (this)
            {
                bool free = this.m_actualSize < this.m_maxSize;
                if (this.m_free != free)
                {
                    this.m_free = free;
                    this.m_onQueueChanged?.Invoke(this.m_free);
                }

                if (this.m_overflowWait)
                {
                    SpinWait.SpinUntil(this.Check);
                }

                Interlocked.Add(ref this.m_actualSize, item.Size);
                base.Enqueue(item);
            }
           
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public new bool TryDequeue(out T result)
        {
            if (base.TryDequeue(out result))
            {
                Interlocked.Add(ref this.m_actualSize, -result.Size);
                bool free = this.m_actualSize < this.m_maxSize;
                if (this.m_free != free)
                {
                    this.m_free = free;
                    this.m_onQueueChanged?.Invoke(this.m_free);
                }
                return true;
            }
            return false;
        }

        private bool Check()
        {
            return this.m_actualSize < this.m_maxSize;
        }
    }

    /// <summary>
    /// 队列数据
    /// </summary>
    public interface IQueueData
    {
        /// <summary>
        /// 数据长度
        /// </summary>
        int Size { get; }
    }

    /// <summary>
    /// 传输字节
    /// </summary>
    public readonly struct QueueDataBytes : IQueueData
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public QueueDataBytes(byte[] buffer, int offset, int length)
        {
            this.Offset = offset;
            this.Length = length;
            this.Buffer = buffer;
            this.Size = length;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="buffer"></param>
        public QueueDataBytes(byte[] buffer) : this(buffer, 0, buffer.Length)
        {
        }

        /// <summary>
        /// 数据内存
        /// </summary>
        public byte[] Buffer { get; }

        /// <summary>
        /// 偏移
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// 长度
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// 尺寸
        /// </summary>
        public int Size { get; }
    }
}