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
/// 元数据键值对。
/// </summary>
[FastConverter(typeof(MetadataFastBinaryConverter))]
[JsonConverter(typeof(MetadataJsonConverter))]
public sealed class Metadata : IEnumerable<KeyValuePair<string, string>>, IPackage
{
    private const int MaxValueByteLength = 254;
    private readonly List<KeyValuePair<string, string>> m_list = new List<KeyValuePair<string, string>>();

    /// <summary>
    /// 获取或设置指定键的值。若值的 UTF-8 编码长度超过 <see cref="MaxValueByteLength"/>，则自动拆分存储。
    /// </summary>
    /// <param name="key">键。</param>
    /// <returns>与键关联的合并值，若键不存在则返回 <see langword="null"/>。</returns>
    public string this[string key]
    {
        get
        {
            ThrowHelper.ThrowIfNull(key, nameof(key));
            return this.GetCombinedValue(key);
        }
        set
        {
            ThrowHelper.ThrowIfNull(key, nameof(key));
            this.Remove(key);
            this.SplitAndAdd(key, value);
        }
    }

    /// <summary>
    /// 获取元数据集合中唯一键的数量。
    /// </summary>
    public int Count
    {
        get
        {
            var seen = new HashSet<string>();
            foreach (var item in this.m_list)
            {
                seen.Add(item.Key);
            }
            return seen.Count;
        }
    }

    /// <summary>
    /// 获取元数据集合中所有唯一键的集合。
    /// </summary>
    public ICollection<string> Keys
    {
        get
        {
            var keys = new List<string>();
            var seen = new HashSet<string>();
            foreach (var item in this.m_list)
            {
                if (seen.Add(item.Key))
                {
                    keys.Add(item.Key);
                }
            }
            return keys;
        }
    }

    /// <summary>
    /// 获取元数据集合中所有合并值的集合，顺序与 <see cref="Keys"/> 对应。
    /// </summary>
    public ICollection<string> Values
    {
        get
        {
            var values = new List<string>();
            foreach (var key in this.Keys)
            {
                values.Add(this.GetCombinedValue(key));
            }
            return values;
        }
    }

    /// <summary>
    /// 向元数据集合添加一个键值对。如果键已经存在，则覆盖其值。若值的 UTF-8 编码长度超过 <see cref="MaxValueByteLength"/>，则自动拆分为多个条目存储。
    /// </summary>
    /// <param name="name">要添加的键。</param>
    /// <param name="value">与键关联的值。</param>
    /// <returns>返回当前元数据对象，以支持链式调用。</returns>
    public Metadata Add(string name, string value)
    {
        this.Remove(name);
        this.SplitAndAdd(name, value);
        return this;
    }

    /// <summary>
    /// 清空元数据集合中的所有键值对。
    /// </summary>
    public void Clear() => this.m_list.Clear();

    /// <summary>
    /// 判断元数据集合中是否包含指定的键。
    /// </summary>
    /// <param name="key">要查找的键。</param>
    /// <returns>若包含则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
    public bool ContainsKey(string key) => this.m_list.Exists(x => x.Key == key);

    /// <summary>
    /// 从元数据集合中移除指定键的所有键值对条目。
    /// </summary>
    /// <param name="key">要移除的键。</param>
    /// <returns>若移除成功则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
    public bool Remove(string key) => this.m_list.RemoveAll(x => x.Key == key) > 0;

    /// <summary>
    /// 尝试获取指定键对应的合并值。
    /// </summary>
    /// <param name="key">要查找的键。</param>
    /// <param name="value">若找到则为对应的合并值，否则为 <see langword="null"/>。</param>
    /// <returns>若找到则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
    public bool TryGetValue(string key, out string value)
    {
        if (this.ContainsKey(key))
        {
            value = this.GetCombinedValue(key);
            return true;
        }
        value = null;
        return false;
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        var seen = new HashSet<string>();
        foreach (var item in this.m_list)
        {
            if (seen.Add(item.Key))
            {
                yield return new KeyValuePair<string, string>(item.Key, this.GetCombinedValue(item.Key));
            }
        }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <inheritdoc/>
    public void Package<TWriter>(ref TWriter writer)
        where TWriter : IBytesWriter
    {
        WriterExtension.WriteValue<TWriter, int>(ref writer, this.m_list.Count);
        foreach (var item in this.m_list)
        {
            WriterExtension.WriteString(ref writer, item.Key, FixedHeaderType.Byte);
            WriterExtension.WriteString(ref writer, item.Value, FixedHeaderType.Byte);
        }
    }

    /// <inheritdoc/>
    public void Unpackage<TReader>(ref TReader reader)
        where TReader : IBytesReader
    {
        var count = ReaderExtension.ReadValue<TReader, int>(ref reader);
        for (var i = 0; i < count; i++)
        {
            var key = ReaderExtension.ReadString(ref reader, FixedHeaderType.Byte);
            var value = ReaderExtension.ReadString(ref reader, FixedHeaderType.Byte);
            this.m_list.Add(new KeyValuePair<string, string>(key, value));
        }
    }

    private string GetCombinedValue(string key)
    {
        string first = null;
        StringBuilder sb = null;
        foreach (var item in this.m_list)
        {
            if (item.Key != key)
            {
                continue;
            }
            if (first == null)
            {
                first = item.Value;
            }
            else
            {
                sb ??= new StringBuilder(first);
                sb.Append(item.Value);
            }
        }
        return sb != null ? sb.ToString() : first;
    }

    private void SplitAndAdd(string key, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            this.m_list.Add(new KeyValuePair<string, string>(key, value ?? string.Empty));
            return;
        }

        var segmentStart = 0;
        var currentByteCount = 0;
        var i = 0;
        while (i < value.Length)
        {
            int charByteCount;
            if (char.IsHighSurrogate(value[i]) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
            {
                charByteCount = 4;
            }
            else if (value[i] < 0x80)
            {
                charByteCount = 1;
            }
            else if (value[i] < 0x800)
            {
                charByteCount = 2;
            }
            else
            {
                charByteCount = 3;
            }

            if (currentByteCount + charByteCount > MaxValueByteLength)
            {
                this.m_list.Add(new KeyValuePair<string, string>(key, value.Substring(segmentStart, i - segmentStart)));
                segmentStart = i;
                currentByteCount = 0;
            }

            currentByteCount += charByteCount;
            i += charByteCount == 4 ? 2 : 1;
        }

        if (segmentStart < value.Length)
        {
            this.m_list.Add(new KeyValuePair<string, string>(key, value.Substring(segmentStart)));
        }
    }
}