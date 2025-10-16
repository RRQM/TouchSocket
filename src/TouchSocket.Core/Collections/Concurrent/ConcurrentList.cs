//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Collections;

namespace TouchSocket.Core;

/// <summary>
/// 线程安全的<see cref="List{T}"/>，其基本操作和<see cref="List{T}"/>一致。
/// </summary>
/// <typeparam name="T">元素类型</typeparam>
public class ConcurrentList<T> : IList<T>, IReadOnlyList<T>
{
    private readonly List<T> m_list;

    /// <summary>
    /// 初始化<see cref="ConcurrentList{T}"/>类的新实例。
    /// </summary>
    /// <param name="collection">用于填充列表的集合。</param>
    public ConcurrentList(IEnumerable<T> collection)
    {
        this.m_list = new List<T>(collection);
    }

    /// <summary>
    /// 初始化<see cref="ConcurrentList{T}"/>类的新实例。
    /// </summary>
    public ConcurrentList()
    {
        this.m_list = new List<T>();
    }

    /// <summary>
    /// 初始化<see cref="ConcurrentList{T}"/>类的新实例。
    /// </summary>
    /// <param name="capacity">初始容量。</param>
    public ConcurrentList(int capacity)
    {
        this.m_list = new List<T>(capacity);
    }

    /// <inheritdoc/>
    public int Count
    {
        get
        {
            lock (((ICollection)this.m_list).SyncRoot)
            {
                return this.m_list.Count;
            }
        }
    }

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public T this[int index]
    {
        get
        {
            lock (((ICollection)this.m_list).SyncRoot)
            {
                return this.m_list[index];
            }
        }
        set
        {
            lock (((ICollection)this.m_list).SyncRoot)
            {
                this.m_list[index] = value;
            }
        }
    }

    /// <inheritdoc/>
    public void Add(T item)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            this.m_list.Add(item);
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            this.m_list.Clear();
        }
    }

    /// <inheritdoc/>
    public bool Contains(T item)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.Contains(item);
        }
    }

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            this.m_list.CopyTo(array, arrayIndex);
        }
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.ToList().GetEnumerator();
        }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.GetEnumerator();
        }
    }

    /// <inheritdoc/>
    public int IndexOf(T item)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.IndexOf(item);
        }
    }

    /// <inheritdoc/>
    public void Insert(int index, T item)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            this.m_list.Insert(index, item);
        }
    }

    /// <inheritdoc/>
    public bool Remove(T item)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.Remove(item);
        }
    }

    /// <inheritdoc/>
    public void RemoveAt(int index)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            if (index < this.m_list.Count)
            {
                this.m_list.RemoveAt(index);
            }
        }
    }

    /// <summary>
    /// 获取或设置容量。
    /// </summary>
    public int Capacity
    {
        get
        {
            lock (((ICollection)this.m_list).SyncRoot)
            {
                return this.m_list.Capacity;
            }
        }
        set
        {
            lock (((ICollection)this.m_list).SyncRoot)
            {
                this.m_list.Capacity = value;
            }
        }
    }

    /// <inheritdoc/>
    public void AddRange(IEnumerable<T> collection)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            this.m_list.AddRange(collection);
        }
    }

    /// <inheritdoc/>
    public int BinarySearch(T item)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.BinarySearch(item);
        }
    }

    /// <inheritdoc/>
    public int BinarySearch(T item, IComparer<T> comparer)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.BinarySearch(item, comparer);
        }
    }

    /// <inheritdoc/>
    public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.BinarySearch(index, count, item, comparer);
        }
    }

    /// <inheritdoc/>
    public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.ConvertAll(converter);
        }
    }

    /// <inheritdoc/>
    public T Find(Predicate<T> match)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.Find(match);
        }
    }

    /// <inheritdoc/>
    public List<T> FindAll(Predicate<T> match)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.FindAll(match);
        }
    }

    /// <inheritdoc/>
    public int FindIndex(int startIndex, int count, Predicate<T> match)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.FindIndex(startIndex, count, match);
        }
    }

    /// <inheritdoc/>
    public int FindIndex(int startIndex, Predicate<T> match)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.FindIndex(startIndex, match);
        }
    }

    /// <inheritdoc/>
    public int FindIndex(Predicate<T> match)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.FindIndex(match);
        }
    }

    /// <inheritdoc/>
    public T FindLast(Predicate<T> match)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.FindLast(match);
        }
    }

    /// <inheritdoc/>
    public int FindLastIndex(int startIndex, int count, Predicate<T> match)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.FindLastIndex(startIndex, count, match);
        }
    }

    /// <inheritdoc/>
    public int FindLastIndex(int startIndex, Predicate<T> match)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.FindLastIndex(startIndex, match);
        }
    }

    /// <inheritdoc/>
    public int FindLastIndex(Predicate<T> match)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.FindLastIndex(match);
        }
    }

    /// <inheritdoc/>
    public void ForEach(Action<T> action)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            this.m_list.ForEach(action);
        }
    }

    /// <inheritdoc/>
    public List<T> GetRange(int index, int count)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.GetRange(index, count);
        }
    }

    /// <inheritdoc/>
    public int IndexOf(T item, int index)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.IndexOf(item, index);
        }
    }

    /// <inheritdoc/>
    public int IndexOf(T item, int index, int count)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.IndexOf(item, index, count);
        }
    }

    /// <inheritdoc/>
    public void InsertRange(int index, IEnumerable<T> collection)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            this.m_list.InsertRange(index, collection);
        }
    }

    /// <inheritdoc/>
    public int LastIndexOf(T item)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.LastIndexOf(item);
        }
    }

    /// <inheritdoc/>
    public int LastIndexOf(T item, int index)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.LastIndexOf(item, index);
        }
    }

    /// <inheritdoc/>
    public int LastIndexOf(T item, int index, int count)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.LastIndexOf(item, index, count);
        }
    }

    /// <inheritdoc/>
    public void RemoveAll(Predicate<T> match)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            this.m_list.RemoveAll(match);
        }
    }

    /// <inheritdoc/>
    public void RemoveRange(int index, int count)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            this.m_list.RemoveRange(index, count);
        }
    }

    /// <inheritdoc/>
    public void Reverse()
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            this.m_list.Reverse();
        }
    }

    /// <inheritdoc/>
    public void Reverse(int index, int count)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            this.m_list.Reverse(index, count);
        }
    }

    /// <inheritdoc/>
    public void Sort()
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            this.m_list.Sort();
        }
    }

    /// <inheritdoc/>
    public void Sort(Comparison<T> comparison)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            this.m_list.Sort(comparison);
        }
    }

    /// <inheritdoc/>
    public void Sort(IComparer<T> comparer)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            this.m_list.Sort(comparer);
        }
    }

    /// <inheritdoc/>
    public void Sort(int index, int count, IComparer<T> comparer)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            this.m_list.Sort(index, count, comparer);
        }
    }

    /// <inheritdoc/>
    public T[] ToArray()
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.ToArray();
        }
    }

    /// <inheritdoc/>
    public void TrimExcess()
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            this.m_list.TrimExcess();
        }
    }

    /// <inheritdoc/>
    public bool TrueForAll(Predicate<T> match)
    {
        lock (((ICollection)this.m_list).SyncRoot)
        {
            return this.m_list.TrueForAll(match);
        }
    }
}