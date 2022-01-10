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
using System.Threading;

namespace RRQMCore.Collections.Concurrent
{
    /// <summary>
    /// 智能安全队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IntelligentConcurrentQueue<T> : ConcurrentQueue<T>
    {
        private int count;

        private int maxCount;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxCount"></param>
        public IntelligentConcurrentQueue(int maxCount)
        {
            this.maxCount = maxCount;
        }

        /// <summary>
        /// 允许的最大长度
        /// </summary>
        public int MaxCount
        {
            get { return maxCount; }
        }

        /// <summary>
        /// 长度
        /// </summary>
        public new int Count
        {
            get { return this.count; }
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="item"></param>
        public new void Enqueue(T item)
        {
            SpinWait.SpinUntil(this.Check);
            Interlocked.Increment(ref count);
            base.Enqueue(item);
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
                Interlocked.Decrement(ref count);
                return true;
            }
            return false;
        }

        private bool Check()
        {
            return this.count < this.maxCount;
        }
    }
}