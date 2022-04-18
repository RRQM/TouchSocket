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
        private readonly List<T> m_list;

        [NonSerialized]
        private readonly ReaderWriterLockSlim m_locker;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="collection"></param>
        public ConcurrentList(IEnumerable<T> collection)
        {
            this.m_list = new List<T>(collection);
            this.m_locker = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ConcurrentList()
        {
            this.m_list = new List<T>();
            this.m_locker = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="capacity"></param>
        public ConcurrentList(int capacity)
        {
            this.m_list = new List<T>(capacity);
            this.m_locker = new ReaderWriterLockSlim();
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
                    this.m_locker.EnterReadLock();
                    return this.m_list.Count;
                }
                finally
                {
                    this.m_locker.ExitReadLock();
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
                    this.m_locker.EnterReadLock();
                    return this.m_list[index];
                }
                finally
                {
                    this.m_locker.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    this.m_locker.EnterWriteLock();
                    this.m_list[index] = value;
                }
                finally
                {
                    this.m_locker.ExitWriteLock();
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
                this.m_locker.EnterWriteLock();
                this.m_list.Add(item);
            }
            finally
            {
                this.m_locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// 清空所有元素
        /// </summary>
        public void Clear()
        {
            try
            {
                this.m_locker.EnterWriteLock();
                this.m_list.Clear();
            }
            finally
            {
                this.m_locker.ExitWriteLock();
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
                this.m_locker.EnterReadLock();
                return this.m_list.Contains(item);
            }
            finally
            {
                this.m_locker.ExitReadLock();
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
                this.m_locker.EnterReadLock();
                this.m_list.CopyTo(array, arrayIndex);
            }
            finally
            {
                this.m_locker.ExitReadLock();
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
                this.m_locker.EnterReadLock();
                foreach (var item in this.m_list)
                {
                    yield return item;
                }
            }
            finally
            {
                this.m_locker.ExitReadLock();
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
                this.m_locker.EnterReadLock();
                return this.m_list.IndexOf(item);
            }
            finally
            {
                this.m_locker.ExitReadLock();
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
                this.m_locker.EnterWriteLock();
                this.m_list.Insert(index, item);
            }
            finally
            {
                this.m_locker.ExitWriteLock();
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
                this.m_locker.EnterWriteLock();
                return this.m_list.Remove(item);
            }
            finally
            {
                this.m_locker.ExitWriteLock();
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
                this.m_locker.EnterWriteLock();
                if (index < this.m_list.Count)
                {
                    this.m_list.RemoveAt(index);
                }
            }
            finally
            {
                this.m_locker.ExitWriteLock();
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
                    this.m_locker.EnterReadLock();
                    return this.m_list.Capacity;
                }
                finally
                {
                    this.m_locker.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    this.m_locker.EnterWriteLock();
                    this.m_list.Capacity = value;
                }
                finally
                {
                    this.m_locker.ExitWriteLock();
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
                this.m_locker.EnterWriteLock();
                this.m_list.AddRange(collection);
            }
            finally
            {
                this.m_locker.ExitWriteLock();
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
                this.m_locker.EnterReadLock();
                return this.m_list.BinarySearch(item);
            }
            finally
            {
                this.m_locker.ExitReadLock();
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
                this.m_locker.EnterReadLock();
                return this.m_list.BinarySearch(item, comparer);
            }
            finally
            {
                this.m_locker.ExitReadLock();
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
                this.m_locker.EnterReadLock();
                return this.m_list.BinarySearch(index, count, item, comparer);
            }
            finally
            {
                this.m_locker.ExitReadLock();
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
                this.m_locker.EnterReadLock();
                return this.m_list.ConvertAll(converter);
            }
            finally
            {
                this.m_locker.ExitReadLock();
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
                this.m_locker.EnterReadLock();
                return this.m_list.Find(match);
            }
            finally
            {
                this.m_locker.ExitReadLock();
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
                this.m_locker.EnterReadLock();
                return this.m_list.FindAll(match);
            }
            finally
            {
                this.m_locker.ExitReadLock();
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
                this.m_locker.EnterReadLock();
                return this.m_list.FindIndex(startIndex, count, match);
            }
            finally
            {
                this.m_locker.ExitReadLock();
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
                this.m_locker.EnterReadLock();
                return this.m_list.FindIndex(startIndex, match);
            }
            finally { this.m_locker.ExitReadLock(); }
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
                this.m_locker.EnterReadLock();
                return this.m_list.FindIndex(match);
            }
            finally { this.m_locker.ExitReadLock(); }
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
                this.m_locker.EnterReadLock();
                return this.m_list.FindLast(match);
            }
            finally { this.m_locker.ExitReadLock(); }
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
                this.m_locker.EnterReadLock();
                return this.m_list.FindLastIndex(startIndex, count, match);
            }
            finally { this.m_locker.ExitReadLock(); }
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
                this.m_locker.EnterReadLock();
                return this.m_list.FindLastIndex(startIndex, match);
            }
            finally { this.m_locker.ExitReadLock(); }
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
                this.m_locker.EnterReadLock();
                return this.m_list.FindLastIndex(match);
            }
            finally { this.m_locker.ExitReadLock(); }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.ForEach(Action{T})"/>
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<T> action)
        {
            try
            {
                this.m_locker.EnterReadLock();
                this.m_list.ForEach(action);
            }
            finally
            {
                this.m_locker.ExitReadLock();
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
                this.m_locker.EnterReadLock();
                return this.m_list.GetRange(index, count);
            }
            finally { this.m_locker.ExitReadLock(); }
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
                this.m_locker.EnterReadLock();
                return this.m_list.IndexOf(item, index);
            }
            finally { this.m_locker.ExitReadLock(); }
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
                this.m_locker.EnterReadLock();
                return this.m_list.IndexOf(item, index, count);
            }
            finally { this.m_locker.ExitReadLock(); }
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
                this.m_locker.EnterWriteLock();
                this.m_list.InsertRange(index, collection);
            }
            finally
            {
                this.m_locker.ExitWriteLock();
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
                this.m_locker.EnterReadLock();
                return this.m_list.IndexOf(item);
            }
            finally { this.m_locker.ExitReadLock(); }
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
                this.m_locker.EnterReadLock();
                return this.m_list.LastIndexOf(item, index);
            }
            finally { this.m_locker.ExitReadLock(); }
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
                this.m_locker.EnterReadLock();
                return this.m_list.LastIndexOf(item, index, count);
            }
            finally { this.m_locker.ExitReadLock(); }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.RemoveAll(Predicate{T})"/>
        /// </summary>
        /// <param name="match"></param>
        public void RemoveAll(Predicate<T> match)
        {
            try
            {
                this.m_locker.EnterWriteLock();
                this.m_list.RemoveAll(match);
            }
            finally
            {
                this.m_locker.ExitWriteLock();
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
                this.m_locker.EnterWriteLock();
                this.m_list.RemoveRange(index, count);
            }
            finally
            {
                this.m_locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.Reverse()"/>
        /// </summary>
        public void Reverse()
        {
            try
            {
                this.m_locker.EnterWriteLock();
                this.m_list.Reverse();
            }
            finally
            {
                this.m_locker.ExitWriteLock();
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
                this.m_locker.EnterWriteLock();
                this.m_list.Reverse(index, count);
            }
            finally { this.m_locker.ExitWriteLock(); }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.Sort()"/>
        /// </summary>
        public void Sort()
        {
            try
            {
                this.m_locker.EnterWriteLock();
                this.m_list.Sort();
            }
            finally
            {
                this.m_locker.ExitWriteLock();
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
                this.m_locker.EnterWriteLock();
                this.m_list.Sort(comparison);
            }
            finally { this.m_locker.ExitWriteLock(); }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.Sort(IComparer{T})"/>
        /// </summary>
        /// <param name="comparer"></param>
        public void Sort(IComparer<T> comparer)
        {
            try
            {
                this.m_locker.EnterWriteLock();
                this.m_list.Sort(comparer);
            }
            finally { this.m_locker.ExitWriteLock(); }
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
                this.m_locker.EnterWriteLock();
                this.m_list.Sort(index, count, comparer);
            }
            finally { this.m_locker.ExitWriteLock(); }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.ToArray"/>
        /// </summary>
        /// <returns></returns>
        public T[] ToArray()
        {
            try
            {
                this.m_locker.EnterReadLock();
                return this.m_list.ToArray();
            }
            finally
            {
                this.m_locker.ExitReadLock();
            }
        }

        /// <summary>
        /// <inheritdoc cref="List{T}.TrimExcess"/>
        /// </summary>
        public void TrimExcess()
        {
            try
            {
                this.m_locker.EnterWriteLock();
                this.m_list.TrimExcess();
            }
            finally
            {
                this.m_locker.ExitWriteLock();
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
                this.m_locker.EnterReadLock();
                return this.m_list.TrueForAll(match);
            }
            finally { this.m_locker.ExitReadLock(); }
        }
    }
}