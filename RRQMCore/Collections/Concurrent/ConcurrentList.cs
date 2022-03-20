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
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace RRQMCore.Collections.Concurrent
{
    /// <summary>
    /// 线程安全的List，其基本操作和List一致。
    /// <para>该集合虽然是线程安全，但是不支持在foreach时修改集合，仅可以遍历成员。</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentList<T> : IList<T>
    {
        private readonly List<T> list;

        [NonSerialized]
        private readonly ReaderWriterLockSlim locker;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="collection"></param>
        public ConcurrentList(IEnumerable<T> collection)
        {
            this.list = new List<T>(collection);
            this.locker = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ConcurrentList()
        {
            this.list = new List<T>();
            this.locker = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="capacity"></param>
        public ConcurrentList(int capacity)
        {
            this.list = new List<T>(capacity);
            this.locker = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// 元素数量
        /// </summary>
        public int Count
        {
            get
            {
                try
                {
                    this.locker.EnterReadLock();
                    return this.list.Count;
                }
                finally
                {
                    this.locker.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// 是否为只读
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// 获取索引元素
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                try
                {
                    this.locker.EnterReadLock();
                    return this.list[index];
                }
                finally
                {
                    this.locker.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    this.locker.EnterWriteLock();
                    this.list[index] = value;
                }
                finally
                {
                    this.locker.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            try
            {
                this.locker.EnterWriteLock();
                this.list.Add(item);
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// 清空所有元素
        /// </summary>
        public void Clear()
        {
            try
            {
                this.locker.EnterWriteLock();
                this.list.Clear();
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// 是否包含某个元素
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.Contains(item);
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        /// <summary>
        /// 复制到
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            try
            {
                this.locker.EnterReadLock();
                this.list.CopyTo(array, arrayIndex);
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        /// <summary>
        /// 返回迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            try
            {
                this.locker.EnterReadLock();
                foreach (var item in this.list)
                {
                    yield return item;
                }
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        /// <summary>
        /// 返回迭代器组合
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// 索引
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(T item)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.IndexOf(item);
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, T item)
        {
            try
            {
                this.locker.EnterWriteLock();
                this.list.Insert(index, item);
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// 移除元素
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            try
            {
                this.locker.EnterWriteLock();
                return this.list.Remove(item);
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// 按索引移除
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            try
            {
                this.locker.EnterWriteLock();
                if (index < this.list.Count)
                {
                    this.list.RemoveAt(index);
                }
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// 获取或设置容量
        /// </summary>
        public int Capacity
        {
            get
            {
                try
                {
                    this.locker.EnterReadLock();
                    return this.list.Capacity;
                }
                finally
                {
                    this.locker.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    this.locker.EnterWriteLock();
                    this.list.Capacity = value;
                }
                finally
                {
                    this.locker.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.AddRange(IEnumerable{T})"/>
        /// </summary>
        /// <param name="collection"></param>
        public void AddRange(IEnumerable<T> collection)
        {
            try
            {
                this.locker.EnterWriteLock();
                this.list.AddRange(collection);
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.BinarySearch(T)"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int BinarySearch(T item)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.BinarySearch(item);
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.BinarySearch(T, IComparer{T})"/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public int BinarySearch(T item, IComparer<T> comparer)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.BinarySearch(item, comparer);
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.BinarySearch(int, int, T, IComparer{T})"/>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <param name="item"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.BinarySearch(index, count, item, comparer);
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.ConvertAll{TOutput}(Converter{T, TOutput})"/>
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="converter"></param>
        /// <returns></returns>
        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.ConvertAll(converter);
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.Find(Predicate{T})"/>
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public T Find(Predicate<T> match)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.Find(match);
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.FindAll(Predicate{T})"/>
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public List<T> FindAll(Predicate<T> match)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.FindAll(match);
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.FindIndex(int, int, Predicate{T})"/>
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.FindIndex(startIndex, count, match);
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.FindIndex(int, Predicate{T})"/>
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public int FindIndex(int startIndex, Predicate<T> match)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.FindIndex(startIndex, match);
            }
            finally { this.locker.ExitReadLock(); }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.FindIndex(Predicate{T})"/>
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public int FindIndex(Predicate<T> match)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.FindIndex(match);
            }
            finally { this.locker.ExitReadLock(); }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.FindLast(Predicate{T})"/>
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public T FindLast(Predicate<T> match)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.FindLast(match);
            }
            finally { this.locker.ExitReadLock(); }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.FindLastIndex(int, int, Predicate{T})"/>
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.FindLastIndex(startIndex, count, match);
            }
            finally { this.locker.ExitReadLock(); }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.FindLastIndex(int, Predicate{T})"/>
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.FindLastIndex(startIndex, match);
            }
            finally { this.locker.ExitReadLock(); }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.FindLastIndex(Predicate{T})"/>
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public int FindLastIndex(Predicate<T> match)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.FindLastIndex(match);
            }
            finally { this.locker.ExitReadLock(); }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.ForEach(Action{T})"/>
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<T> action)
        {
            try
            {
                this.locker.EnterReadLock();
                this.list.ForEach(action);
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.GetRange(int, int)"/>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<T> GetRange(int index, int count)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.GetRange(index, count);
            }
            finally { this.locker.ExitReadLock(); }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.IndexOf(T, int)"/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public int IndexOf(T item, int index)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.IndexOf(item, index);
            }
            finally { this.locker.ExitReadLock(); }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.IndexOf(T, int, int)"/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int IndexOf(T item, int index, int count)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.IndexOf(item, index, count);
            }
            finally { this.locker.ExitReadLock(); }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.InsertRange(int, IEnumerable{T})"/>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="collection"></param>
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            try
            {
                this.locker.EnterWriteLock();
                this.list.InsertRange(index, collection);
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.LastIndexOf(T)"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int LastIndexOf(T item)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.IndexOf(item);
            }
            finally { this.locker.ExitReadLock(); }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.LastIndexOf(T, int)"/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public int LastIndexOf(T item, int index)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.LastIndexOf(item, index);
            }
            finally { this.locker.ExitReadLock(); }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.LastIndexOf(T, int, int)"/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int LastIndexOf(T item, int index, int count)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.LastIndexOf(item, index, count);
            }
            finally { this.locker.ExitReadLock(); }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.RemoveAll(Predicate{T})"/>
        /// </summary>
        /// <param name="match"></param>
        public void RemoveAll(Predicate<T> match)
        {
            try
            {
                this.locker.EnterWriteLock();
                this.list.RemoveAll(match);
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.RemoveRange(int, int)"/>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        public void RemoveRange(int index, int count)
        {
            try
            {
                this.locker.EnterWriteLock();
                this.list.RemoveRange(index, count);
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.Reverse()"/>
        /// </summary>
        public void Reverse()
        {
            try
            {
                this.locker.EnterWriteLock();
                this.list.Reverse();
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.Reverse(int, int)"/>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        public void Reverse(int index, int count)
        {
            try
            {
                this.locker.EnterWriteLock();
                this.list.Reverse(index, count);
            }
            finally { this.locker.ExitWriteLock(); }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.Sort()"/>
        /// </summary>
        public void Sort()
        {
            try
            {
                this.locker.EnterWriteLock();
                this.list.Sort();
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.Sort(Comparison{T})"/>
        /// </summary>
        /// <param name="comparison"></param>
        public void Sort(Comparison<T> comparison)
        {
            try
            {
                this.locker.EnterWriteLock();
                this.list.Sort(comparison);
            }
            finally { this.locker.ExitWriteLock(); }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.Sort(IComparer{T})"/>
        /// </summary>
        /// <param name="comparer"></param>
        public void Sort(IComparer<T> comparer)
        {
            try
            {
                this.locker.EnterWriteLock();
                this.list.Sort(comparer);
            }
            finally { this.locker.ExitWriteLock(); }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.Sort(int, int, IComparer{T})"/>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <param name="comparer"></param>
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            try
            {
                this.locker.EnterWriteLock();
                this.list.Sort(index, count, comparer);
            }
            finally { this.locker.ExitWriteLock(); }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.ToArray"/>
        /// </summary>
        /// <returns></returns>
        public T[] ToArray()
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.ToArray();
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.TrimExcess"/>
        /// </summary>
        public void TrimExcess()
        {
            try
            {
                this.locker.EnterWriteLock();
                this.list.TrimExcess();
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.TrueForAll(Predicate{T})"/>
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public bool TrueForAll(Predicate<T> match)
        {
            try
            {
                this.locker.EnterReadLock();
                return this.list.TrueForAll(match);
            }
            finally { this.locker.ExitReadLock(); }
        }
    }
}