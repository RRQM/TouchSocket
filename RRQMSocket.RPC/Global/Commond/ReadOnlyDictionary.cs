//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
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

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 只读字典
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ReadOnlyDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private Dictionary<TKey, TValue> dic = new Dictionary<TKey, TValue>();

        /// <summary>
        /// 值集合
        /// </summary>
        public ICollection<TValue> Values { get { return this.dic.Values; } }

        /// <summary>
        /// 键集合
        /// </summary>
        public ICollection<TKey> Keys { get { return this.dic.Keys; } }

        internal void Add(TKey key, TValue value)
        {
            dic.Add(key, value);
        }

        internal bool Remove(TKey key)
        {
            return dic.Remove(key);
        }

        internal void Clear()
        {
            dic.Clear();
        }

        /// <summary>
        /// 键值对数目
        /// </summary>
        public int Count { get { return this.dic.Count; } }

        /// <summary>
        /// 尝试获取值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.dic.TryGetValue(key, out value);
        }

        /// <summary>
        /// 返回迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.dic.GetEnumerator();
        }

        /// <summary>
        /// 迭代器
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.dic.GetEnumerator();
        }
    }
}