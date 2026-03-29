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
using System.Text.Json.Serialization;

namespace TouchSocket.Core;

/// <summary>
/// 元数据键值对。区分大小写，高性能字典存取。
/// </summary>
[FastConverter(typeof(MetadataFastBinaryConverter))]
[JsonConverter(typeof(MetadataJsonConverter))]
public sealed class Metadata : IEnumerable<KeyValuePair<string, string>>, IPackage
{
    private readonly Dictionary<string, string> m_dict = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// 获取或设置指定键的值。键区分大小写。
    /// </summary>
    /// <param name="key">键。</param>
    /// <returns>与键关联的值，若键不存在则返回 <see langword="null"/>。</returns>
    public string this[string key]
    {
        get
        {
            ThrowHelper.ThrowIfNull(key, nameof(key));
            this.m_dict.TryGetValue(key, out var value);
            return value;
        }
        set
        {
            ThrowHelper.ThrowIfNull(key, nameof(key));
            this.m_dict[key] = value;
        }
    }

    /// <summary>
    /// 获取元数据集合中键值对的数量。
    /// </summary>
    public int Count => this.m_dict.Count;

    /// <summary>
    /// 获取元数据集合中所有键的集合。
    /// </summary>
    public ICollection<string> Keys => this.m_dict.Keys;

    /// <summary>
    /// 获取元数据集合中所有值的集合。
    /// </summary>
    public ICollection<string> Values => this.m_dict.Values;

    /// <summary>
    /// 向元数据集合添加或覆盖一个键值对。键区分大小写。
    /// </summary>
    /// <param name="name">键。</param>
    /// <param name="value">值。</param>
    /// <returns>返回当前元数据对象，以支持链式调用。</returns>
    public Metadata Add(string name, string value)
    {
        this.m_dict[name] = value;
        return this;
    }

    /// <summary>
    /// 清空元数据集合中的所有键值对。
    /// </summary>
    public void Clear() => this.m_dict.Clear();

    /// <summary>
    /// 判断元数据集合中是否包含指定的键。键区分大小写。
    /// </summary>
    /// <param name="key">要查找的键。</param>
    /// <returns>若包含则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
    public bool ContainsKey(string key) => this.m_dict.ContainsKey(key);

    /// <summary>
    /// 从元数据集合中移除指定键的键值对。键区分大小写。
    /// </summary>
    /// <param name="key">要移除的键。</param>
    /// <returns>若移除成功则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
    public bool Remove(string key) => this.m_dict.Remove(key);

    /// <summary>
    /// 尝试获取指定键对应的值。键区分大小写。
    /// </summary>
    /// <param name="key">要查找的键。</param>
    /// <param name="value">若找到则为对应的值，否则为 <see langword="null"/>。</param>
    /// <returns>若找到则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
    public bool TryGetValue(string key, out string value) => this.m_dict.TryGetValue(key, out value);

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => this.m_dict.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => this.m_dict.GetEnumerator();

    /// <inheritdoc/>
    public void Package<TWriter>(ref TWriter writer)
        where TWriter : IBytesWriter
    {
        WriterExtension.WriteVarUInt32(ref writer, (uint)this.m_dict.Count);
        foreach (var item in this.m_dict)
        {
            WriterExtension.WriteVarString(ref writer, item.Key);
            WriterExtension.WriteVarString(ref writer, item.Value);
        }
    }

    /// <inheritdoc/>
    public void Unpackage<TReader>(ref TReader reader)
        where TReader : IBytesReader
    {
        var count = (int)ReaderExtension.ReadVarUInt32(ref reader);
        this.m_dict.Clear();
        for (var i = 0; i < count; i++)
        {
            var key = ReaderExtension.ReadVarString(ref reader);
            var value = ReaderExtension.ReadVarString(ref reader);
            this.m_dict[key] = value;
        }
    }
}