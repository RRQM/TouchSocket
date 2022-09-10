//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TouchSocket.Core.Extensions
{
    /// <summary>
    /// DictionaryExtension
    /// </summary>
    public static class DictionaryExtension
    {
        #region 字典扩展

        /// <summary>
        /// 移除满足条件的项目。
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="pairs"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static int Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> pairs, Func<KeyValuePair<TKey, TValue>, bool> func)
        {
            List<TKey> list = new List<TKey>();
            foreach (var item in pairs)
            {
                if (func?.Invoke(item) == true)
                {
                    list.Add(item.Key);
                }
            }

            int count = 0;
            foreach (var item in list)
            {
                if (pairs.TryRemove(item, out _))
                {
                    count++;
                }
            }
            return count;
        }

#if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER

        /// <summary>
        /// 尝试添加
        /// </summary>
        /// <typeparam name="Tkey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="tkey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryAdd<Tkey, TValue>(this Dictionary<Tkey, TValue> dictionary, Tkey tkey, TValue value)
        {
            if (dictionary.ContainsKey(tkey))
            {
                return false;
            }
            dictionary.Add(tkey, value);
            return true;
        }

#endif

        /// <summary>
        /// 尝试添加
        /// </summary>
        /// <typeparam name="Tkey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="tkey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void AddOrUpdate<Tkey, TValue>(this Dictionary<Tkey, TValue> dictionary, Tkey tkey, TValue value)
        {
            if (dictionary.ContainsKey(tkey))
            {
                dictionary[tkey] = value;
            }
            else
            {
                dictionary.Add(tkey, value);
            }
        }

        /// <summary>
        /// 获取值。如果键不存在，则返回默认值。
        /// </summary>
        /// <typeparam name="Tkey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="tkey"></param>
        /// <returns></returns>
        public static TValue GetValue<Tkey, TValue>(this Dictionary<Tkey, TValue> dictionary, Tkey tkey)
        {
            if (dictionary.TryGetValue(tkey, out TValue value))
            {
                return value;
            }
            else
            {
                return default;
            }
        }

        #endregion 字典扩展
    }
}