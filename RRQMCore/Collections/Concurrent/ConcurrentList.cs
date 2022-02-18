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
using System.Collections;
using System.Collections.Generic;

namespace RRQMCore.Collections.Concurrent
{
    /// <summary>
    /// 线程安全的List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentList<T> : IList<T>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ConcurrentList()
        {
            this.list = new List<T>();
            this.locker = new object();
        }

        private List<T> list;
        private object locker;

        /// <summary>
        /// 获取或设置指定索引处的元素。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                lock (this.locker)
                {
                    return this.list[index];
                }
            }
            set
            {
                lock (this.locker)
                {
                    this.list[index] = value;
                }
            }
        }

        /// <summary>
        /// 获取集合中包含的元素数。
        /// </summary>
        public int Count
        { get { lock (this.locker) { return this.list.Count; } } }

        /// <summary>
        /// 是否只读
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// 将某项添加到 System.Collections.Generic.ICollection`1 中。
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            lock (this.locker)
            {
                this.list.Add(item);
            }
        }

        /// <summary>
        /// 从 System.Collections.Generic.ICollection`1 中移除所有项。
        /// </summary>
        public void Clear()
        {
            lock (this.locker)
            {
                this.list.Clear();
            }
        }

        /// <summary>
        /// 确定 System.Collections.Generic.ICollection`1 是否包含特定值。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            lock (this.locker)
            {
                return this.Contains(item);
            }
        }

        /// <summary>
        /// 从特定的 System.Collections.Generic.ICollection`1 索引处开始，将 System.Array 的元素复制到一个 System.Array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (this.locker)
            {
                this.list.CopyTo(array, arrayIndex);
            }
        }

        /// <summary>
        /// 返回一个循环访问集合的枚举器。
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            lock (this.locker)
            {
                return this.list.GetEnumerator();
            }
        }

        /// <summary>
        /// 确定 System.Collections.Generic.IList`1 中特定项的索引。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(T item)
        {
            lock (this.locker)
            {
                return this.IndexOf(item);
            }
        }

        /// <summary>
        /// 在 System.Collections.Generic.IList`1 中的指定索引处插入一个项。
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, T item)
        {
            lock (this.locker)
            {
                this.list.Insert(index, item);
            }
        }

        /// <summary>
        /// 从 System.Collections.Generic.ICollection`1 中移除特定对象的第一个匹配项。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            lock (this.locker)
            {
                return this.list.Remove(item);
            }
        }

        /// <summary>
        ///  从 System.Collections.Generic.List`1 中移除一定范围的元素。
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        public void RemoveRange(int index, int count)
        {
            lock (this.locker)
            {
                this.list.RemoveRange(index, count);
            }
        }

        /// <summary>
        ///  移除位于指定索引处的 System.Collections.Generic.IList`1 项。
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            lock (this.locker)
            {
                this.list.RemoveAt(index);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (this.locker)
            {
                return this.list.GetEnumerator();
            }
        }

        /// <summary>
        /// 重写ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Count={this.Count}";
        }
    }
}