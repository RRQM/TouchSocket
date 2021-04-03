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
using System.Collections.Generic;

namespace RRQMSocket
{
    /// <summary>
    /// 只读
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ReadOnlyList<T> : List<T>
    {
        internal new void Add(T block)
        {
            base.Add(block);
        }

        internal new void AddRange(IEnumerable<T> collection)
        {
            base.AddRange(collection);
        }

        internal new void Remove(T block)
        {
            base.Remove(block);
        }

        internal new void RemoveAt(int index)
        {
            base.RemoveAt(index);
        }

        internal new void RemoveAll(Predicate<T> match)
        {
            base.RemoveAll(match);
        }

        internal new void RemoveRange(int index, int range)
        {
            base.RemoveRange(index, range);
        }

        internal new void Clear()
        {
            base.Clear();
        }

        internal new void Insert(int index, T item)
        {
            base.Insert(index, item);
        }

        internal new void InsertRange(int index, IEnumerable<T> collection)
        {
            base.InsertRange(index, collection);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public new T this[int index] { get { return base[index]; } }
    }
}