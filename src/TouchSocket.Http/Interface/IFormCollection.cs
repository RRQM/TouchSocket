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

using System.Collections.Generic;

namespace TouchSocket.Http;

/// <summary>
/// 表示一个键值对集合，通常用于表示HTTP表单中的数据
/// </summary>
public interface IFormCollection : IEnumerable<KeyValuePair<string, string>>
{
    /// <summary>
    /// 获取集合中键值对的数量
    /// </summary>
    int Count { get; }

    /// <summary>
    /// 获取包含上传文件的集合
    /// </summary>
    IMultifileCollection Files { get; }

    /// <summary>
    /// 获取集合中所有键的集合
    /// </summary>
    ICollection<string> Keys { get; }

    /// <summary>
    /// 根据键获取对应的值
    /// </summary>
    /// <param name="key">要获取值的键</param>
    /// <returns>与指定键关联的值</returns>
    string this[string key] { get; }

    /// <summary>
    /// 根据键获取对应的值
    /// </summary>
    /// <param name="key">要获取的键</param>
    /// <returns>与指定键关联的值</returns>
    string Get(string key);

    /// <summary>
    /// 判断集合中是否包含指定的键
    /// </summary>
    /// <param name="key">要检查的键</param>
    /// <returns>如果集合包含指定的键，则返回true；否则返回false</returns>
    bool ContainsKey(string key);

    /// <summary>
    /// 尝试根据键获取对应的值
    /// </summary>
    /// <param name="key">要获取值的键</param>
    /// <param name="value">与指定键关联的值，如果键不存在则为null</param>
    /// <returns>如果键存在于集合中，则返回true；否则返回false</returns>
    bool TryGetValue(string key, out string value);
}