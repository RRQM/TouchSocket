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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TouchSocket.Core;

/// <summary>
/// CollectionsExtension
/// </summary>
public static class CollectionsExtension
{
    #region 字典扩展

    /// <summary>
    /// 移除满足条件的项目。
    /// </summary>
    /// <typeparam name="TKey">字典中键的类型。</typeparam>
    /// <typeparam name="TValue">字典中值的类型。</typeparam>
    /// <param name="pairs">要处理的并发字典。</param>
    /// <param name="func">用于判断项目是否应被移除的函数。</param>
    /// <returns>返回移除的项目数量。</returns>
    public static int RemoveWhen<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> pairs, Func<KeyValuePair<TKey, TValue>, bool> func)
    {
        // 存储需要移除的键的列表，以便之后统一移除
        var list = new List<TKey>();
        foreach (var item in pairs)
        {
            // 使用提供的函数判断当前项目是否应该被移除
            if (func?.Invoke(item) == true)
            {
                list.Add(item.Key);
            }
        }

        // 记录成功移除的项目数量
        var count = 0;
        foreach (var item in list)
        {
            // 尝试移除项目，如果成功则增加计数
            if (pairs.TryRemove(item, out _))
            {
                count++;
            }
        }
        // 返回成功移除的项目数量
        return count;
    }

#if NET45_OR_GREATER || NETSTANDARD2_0

    /// <summary>
    /// 尝试向字典中添加键值对。
    /// </summary>
    /// <typeparam name="TKey">键的类型。</typeparam>
    /// <typeparam name="TValue">值的类型。</typeparam>
    /// <param name="dictionary">要添加键值对的字典。</param>
    /// <param name="key">要添加的键。</param>
    /// <param name="value">要添加的值。</param>
    /// <returns>如果添加成功则返回true，否则返回false。</returns>
    public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        // 如果字典中已经包含此键，则不添加，并返回false
        if (dictionary.ContainsKey(key))
        {
            return false;
        }
        // 向字典中添加键值对
        dictionary.Add(key, value);
        return true;
    }

#endif


    /// <summary>
    /// 向字典中添加或更新指定键的值。
    /// </summary>
    /// <param name="dictionary">要操作的字典。</param>
    /// <param name="key">要添加或更新的键。</param>
    /// <param name="value">与键关联的值。</param>
    /// <typeparam name="TKey">字典中键的类型。</typeparam>
    /// <typeparam name="TValue">字典中值的类型。</typeparam>
    public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        // 尝试向字典中添加键值对。如果键不存在，则成功添加；如果键已存在，则不会添加新的键值对。
        if (!dictionary.TryAdd(key, value))
        {
            // 如果尝试添加失败，则说明键已存在，此时更新键对应的值。
            dictionary[key] = value;
        }
    }


    /// <summary>
    /// 向 ConcurrentDictionary 中添加键值对，如果键已存在，则更新其值。
    /// </summary>
    /// <param name="dictionary">要操作的 ConcurrentDictionary。</param>
    /// <param name="key">键，用于在字典中查找。</param>
    /// <param name="value">值，要添加到字典中或更新已存在的项。</param>
    /// <typeparam name="TKey">键的类型。</typeparam>
    /// <typeparam name="TValue">值的类型。</typeparam>
    public static void AddOrUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        // 尝试向字典中添加键值对。如果键已存在，则不会添加。
        if (!dictionary.TryAdd(key, value))
        {
            // 如果键已存在，更新其值。
            dictionary[key] = value;
        }
    }


    /// <summary>
    /// 获取字典中与指定键关联的值。
    /// </summary>
    /// <param name="dictionary">要搜索的字典。</param>
    /// <param name="key">要查找的键。</param>
    /// <typeparam name="TKey">键的类型。</typeparam>
    /// <typeparam name="TValue">值的类型。</typeparam>
    /// <returns>如果找到了与指定键关联的值，则返回该值；否则返回该类型的默认值。</returns>
    public static TValue GetValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
    {
        // 使用TryGetValue方法尝试获取与键关联的值，如果找到则返回值，否则返回默认值
        return dictionary.TryGetValue(key, out var value) ? value : default;
    }

    #endregion 字典扩展

    #region ConcurrentQueue

#if !NET6_0_OR_GREATER


    /// <summary>
    /// 清空并发队列中的所有元素。
    /// </summary>
    /// <typeparam name="T">队列中元素的类型。</typeparam>
    /// <param name="queue">要清空的并发队列。</param>
    public static void Clear<T>(this ConcurrentQueue<T> queue)
    {
        // 通过不断尝试出队操作来清空队列，直到队列为空。
        while (queue.TryDequeue(out _))
        {
            // 循环体内不需要做任何事情，TryDequeue方法会自动移除队列中的元素。
        }
    }

#endif


    /// <summary>
    /// 清空并发队列并执行指定操作。
    /// </summary>
    /// <typeparam name="T">队列元素的类型。</typeparam>
    /// <param name="queue">要清空的并发队列。</param>
    /// <param name="action">在每个队列元素上执行的操作。</param>
    public static void Clear<T>(this ConcurrentQueue<T> queue, Action<T> action)
    {
        // 当队列中还有元素时，持续尝试出队操作
        while (queue.TryDequeue(out var t))
        {
            // 对出队的元素执行指定的操作
            action?.Invoke(t);
        }
    }

    #endregion ConcurrentQueue

    #region IEnumerableT
    /// <summary>
    /// 循环遍历每个元素，执行Action动作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values">要遍历的集合</param>
    /// <param name="action">对每个元素执行的动作</param>
    /// <returns>返回原始集合</returns>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> values, Action<T> action)
    {
        // 遍历集合中的每个元素
        foreach (var item in values)
        {
            // 对每个元素执行指定的动作
            action.Invoke(item);
        }

        // 返回原始集合
        return values;
    }

    /// <summary>
    /// 循环遍历每个元素，执行异步动作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values">要遍历的集合</param>
    /// <param name="func">对每个元素执行的异步操作</param>
    /// <returns>返回原始集合</returns>
    public static async Task<IEnumerable<T>> ForEachAsync<T>(this IEnumerable<T> values, Func<T, Task> func)
    {
        // 遍历集合中的每个元素
        foreach (var item in values)
        {
            // 执行指定的异步操作
            await func.Invoke(item);
        }

        // 返回原始集合
        return values;
    }
    #endregion
}