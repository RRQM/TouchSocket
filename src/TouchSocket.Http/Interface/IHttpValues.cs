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

namespace TouchSocket.Http;

/// <summary>
/// 表示HTTP键值对集合接口
/// </summary>
public interface IHttpValues : IEnumerable<KeyValuePair<string, TextValues>>
{
    /// <summary>
    /// 获取集合中键值对的总数量
    /// </summary>
    int Count { get; }

    /// <summary>
    /// 获取或设置指定键的值。设置时会覆盖该键的所有现有值。
    /// </summary>
    /// <param name="key">键名</param>
    /// <returns>返回指定键的值，如果不存在则返回<see cref="TextValues.Empty"/></returns>
    TextValues this[string key] { get; set; }

    /// <summary>
    /// 获取指定键的值，会合并所有匹配键的值
    /// </summary>
    /// <param name="key">要获取的键</param>
    /// <returns>返回指定键的值，如果不存在则返回<see cref="TextValues.Empty"/></returns>
    TextValues Get(string key);

    /// <summary>
    /// 获取指定键的所有条目
    /// </summary>
    /// <param name="key">要获取的键</param>
    /// <returns>返回指定键的所有键值对数组</returns>
    KeyValuePair<string, TextValues>[] GetAll(string key);

    /// <summary>
    /// 添加指定的键和值。即使键已存在，也会在集合中新增一条。
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">值</param>
    void Add(string key, TextValues value);

    /// <summary>
    /// 追加值到指定的键。如果键已存在，则将值追加到现有的最后一个键值对中；否则添加新的键值对。
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">要追加的值</param>
    void Append(string key, TextValues value);

    /// <summary>
    /// 尝试追加值到指定的键。仅在键已存在且值不重复时追加。
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">要追加的值</param>
    /// <returns>如果键存在且成功追加新值则返回<see langword="true"/>，如果键不存在或值已存在则返回<see langword="false"/></returns>
    bool TryAppend(string key, TextValues value);

    /// <summary>
    /// 清空集合中的所有键值对
    /// </summary>
    void Clear();

    /// <summary>
    /// 确定集合是否包含指定的键
    /// </summary>
    /// <param name="key">要查找的键</param>
    /// <returns>如果包含该键则返回<see langword="true"/>，否则返回<see langword="false"/></returns>
    bool ContainsKey(string key);

    /// <summary>
    /// 移除指定键的第一个匹配项
    /// </summary>
    /// <param name="key">要移除的键</param>
    /// <returns>如果成功移除则返回<see langword="true"/>，否则返回<see langword="false"/></returns>
    bool Remove(string key);

    /// <summary>
    /// 移除指定键的所有匹配项
    /// </summary>
    /// <param name="key">要移除的键</param>
    /// <returns>如果成功移除则返回<see langword="true"/>，否则返回<see langword="false"/></returns>
    bool RemoveAll(string key);

    /// <summary>
    /// 尝试获取指定键的值
    /// </summary>
    /// <param name="key">要获取的键</param>
    /// <param name="value">获取到的值</param>
    /// <returns>如果键存在则返回<see langword="true"/>，否则返回<see langword="false"/></returns>
    bool TryGetValue(string key, out TextValues value);

    /// <summary>
    /// 尝试添加指定的键和值。如果键已存在则不添加。
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">值</param>
    /// <returns>如果成功添加则返回<see langword="true"/>，否则返回<see langword="false"/></returns>
    bool TryAdd(string key, TextValues value);
}
