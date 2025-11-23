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

internal abstract class InternalHttpCollection : IDictionary<string, TextValues>
{
    private readonly Dictionary<string, TextValues> m_dictionary;

    protected InternalHttpCollection(IEqualityComparer<string> comparer = null)
    {
        m_dictionary = new Dictionary<string, TextValues>(comparer ?? StringComparer.OrdinalIgnoreCase);
    }

    public TextValues this[string key]
    {
        get
        {
            ThrowHelper.ThrowIfNull(key, nameof(key));
            return m_dictionary.TryGetValue(key, out var value) ? value : TextValues.Empty;
        }
        set
        {
            ThrowHelper.ThrowIfNull(key, nameof(key));
            m_dictionary[key] = value;
        }
    }

    public ICollection<string> Keys => m_dictionary.Keys;

    public ICollection<TextValues> Values => m_dictionary.Values;

    public int Count => m_dictionary.Count;

    public bool IsReadOnly => false;

    public void Add(string key, TextValues value)
    {
        ThrowHelper.ThrowIfNull(key, nameof(key));
        if (m_dictionary.TryGetValue(key, out var old))
        {
            m_dictionary[key] = Merge(old, value);
        }
        else
        {
            m_dictionary.Add(key, value);
        }
    }

    public bool ContainsKey(string key)
    {
        ThrowHelper.ThrowIfNull(key, nameof(key));
        return m_dictionary.ContainsKey(key);
    }

    public bool Remove(string key)
    {
        ThrowHelper.ThrowIfNull(key, nameof(key));
        return m_dictionary.Remove(key);
    }

    public bool TryGetValue(string key, out TextValues value)
    {
        ThrowHelper.ThrowIfNull(key, nameof(key));
        return m_dictionary.TryGetValue(key, out value);
    }

    public void Add(KeyValuePair<string, TextValues> item) => Add(item.Key, item.Value);

    public void Clear() => m_dictionary.Clear();

    public bool Contains(KeyValuePair<string, TextValues> item) => ((IDictionary<string, TextValues>)m_dictionary).Contains(item);

    public void CopyTo(KeyValuePair<string, TextValues>[] array, int arrayIndex) => ((IDictionary<string, TextValues>)m_dictionary).CopyTo(array, arrayIndex);

    public bool Remove(KeyValuePair<string, TextValues> item) => ((IDictionary<string, TextValues>)m_dictionary).Remove(item);

    public IEnumerator<KeyValuePair<string, TextValues>> GetEnumerator() => m_dictionary.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    protected static TextValues Merge(TextValues a, TextValues b)
    {
        if (a.IsEmpty) return b;
        if (b.IsEmpty) return a;
        var ac = a.Count;
        var bc = b.Count;
        var arrA = a.ToArray();
        var arrB = b.ToArray();
        var newArr = new string[ac + bc];
        Array.Copy(arrA, 0, newArr, 0, ac);
        Array.Copy(arrB, 0, newArr, ac, bc);
        return new TextValues(newArr);
    }

    public TextValues Get(string key)
    {
        ThrowHelper.ThrowIfNull(key, nameof(key));
        return m_dictionary.TryGetValue(key, out var value) ? value : TextValues.Empty;
    }
}
