//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;

namespace RRQMSocket
{
    /// <summary>
    /// 只读
    /// </summary>
    /// <typeparam name="T"></typeparam>
    
    public class ReadOnlyList<T>:IEnumerable<T>
    {
        private List<T> list = new List<T>();

        internal  void Add(T block)
        {
            list.Add(block);
        }

        internal  void AddRange(IEnumerable<T> collection)
        {
            list.AddRange(collection);
        }

        internal  void Remove(T block)
        {
            list.Remove(block);
        }

        internal  void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        internal  void RemoveAll(Predicate<T> match)
        {
            list.RemoveAll(match);
        }

        internal  void RemoveRange(int index, int range)
        {
            list.RemoveRange(index, range);
        }

        internal  void Clear()
        {
            list.Clear();
        }

        internal  void Insert(int index, T item)
        {
            list.Insert(index, item);
        }

        internal  void InsertRange(int index, IEnumerable<T> collection)
        {
            list.InsertRange(index, collection);
        }

        /// <summary>
        /// 返回迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
           return this.list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
           return  this.list.GetEnumerator();
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public  T this[int index] { get { return list[index]; } }
    }
}