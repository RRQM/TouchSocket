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
    private readonly IEqualityComparer<string> m_comparer;
    private readonly Dictionary<string, TextValues> m_dictionary;
    private readonly List<KeyValuePair<string, TextValues>> m_pendingItems = new List<KeyValuePair<string, TextValues>>();
    private bool m_hasDuplicateKeys;
    private bool m_hasNonPredefinedKeys;
    private bool m_isDictionaryBuilt;

    protected InternalHttpCollection(IEqualityComparer<string> comparer = null)
    {
        this.m_comparer = comparer ?? StringComparer.OrdinalIgnoreCase;
        this.m_dictionary = new Dictionary<string, TextValues>(this.m_comparer);
    }

    public int Count
    {
        get
        {
            this.EnsureDictionaryBuilt();
            return this.m_dictionary.Count;
        }
    }

    public bool IsReadOnly => false;

    public ICollection<string> Keys
    {
        get
        {
            this.EnsureDictionaryBuilt();
            return this.m_dictionary.Keys;
        }
    }

    public ICollection<TextValues> Values
    {
        get
        {
            this.EnsureDictionaryBuilt();
            return this.m_dictionary.Values;
        }
    }

    public TextValues this[string key]
    {
        get
        {
            ThrowHelper.ThrowIfNull(key, nameof(key));
            this.EnsureDictionaryBuilt();
            return this.m_dictionary.TryGetValue(key, out var value) ? value : TextValues.Empty;
        }

        set => this.Add(key, value);
    }

    public void Add(string key, TextValues value)
    {
        ThrowHelper.ThrowIfNull(key, nameof(key));

        if (this.m_isDictionaryBuilt)
        {
            if (this.m_dictionary.TryGetValue(key, out var old))
            {
                this.m_dictionary[key] = Merge(old, value);
            }
            else
            {
                this.m_dictionary.Add(key, value);
            }
        }
        else
        {
            if (!this.m_hasNonPredefinedKeys && !HttpHeaders.IsPredefinedHeader(key))
            {
                this.m_hasNonPredefinedKeys = true;
            }

            if (!this.m_hasDuplicateKeys && this.m_pendingItems.Count > 0)
            {
                for (var i = 0; i < this.m_pendingItems.Count; i++)
                {
                    if (this.m_comparer.Equals(this.m_pendingItems[i].Key, key))
                    {
                        this.m_hasDuplicateKeys = true;
                        break;
                    }
                }
            }

            this.m_pendingItems.Add(new KeyValuePair<string, TextValues>(key, value));
        }
    }

    public void Add(KeyValuePair<string, TextValues> item) => this.Add(item.Key, item.Value);

    public void Clear()
    {
        this.m_dictionary.Clear();
        this.m_pendingItems.Clear();
        this.m_hasDuplicateKeys = false;
        this.m_hasNonPredefinedKeys = false;
        this.m_isDictionaryBuilt = false;
    }

    public bool Contains(KeyValuePair<string, TextValues> item)
    {
        this.EnsureDictionaryBuilt();
        return ((IDictionary<string, TextValues>)this.m_dictionary).Contains(item);
    }

    public bool ContainsKey(string key)
    {
        ThrowHelper.ThrowIfNull(key, nameof(key));
        this.EnsureDictionaryBuilt();
        return this.m_dictionary.ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<string, TextValues>[] array, int arrayIndex)
    {
        this.EnsureDictionaryBuilt();
        ((IDictionary<string, TextValues>)this.m_dictionary).CopyTo(array, arrayIndex);
    }

    public TextValues Get(string key)
    {
        ThrowHelper.ThrowIfNull(key, nameof(key));
        this.EnsureDictionaryBuilt();
        return this.m_dictionary.TryGetValue(key, out var value) ? value : TextValues.Empty;
    }

    public IEnumerator<KeyValuePair<string, TextValues>> GetEnumerator()
    {
        if (this.m_isDictionaryBuilt)
        {
            return this.m_dictionary.GetEnumerator();
        }
        else if (this.m_hasDuplicateKeys || this.m_hasNonPredefinedKeys)
        {
            this.EnsureDictionaryBuilt();
            return this.m_dictionary.GetEnumerator();
        }
        else
        {
            return this.m_pendingItems.GetEnumerator();
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

    public bool Remove(string key)
    {
        ThrowHelper.ThrowIfNull(key, nameof(key));
        this.EnsureDictionaryBuilt();
        return this.m_dictionary.Remove(key);
    }

    public bool Remove(KeyValuePair<string, TextValues> item)
    {
        this.EnsureDictionaryBuilt();
        return ((IDictionary<string, TextValues>)this.m_dictionary).Remove(item);
    }

    public bool TryGetValue(string key, out TextValues value)
    {
        ThrowHelper.ThrowIfNull(key, nameof(key));
        this.EnsureDictionaryBuilt();
        return this.m_dictionary.TryGetValue(key, out value);
    }

    internal void AddInternal(string key, TextValues value)
    {
        this.m_pendingItems.Add(new KeyValuePair<string, TextValues>(key, value));
    }

    protected static TextValues Merge(TextValues a, TextValues b)
    {
        if (a.IsEmpty)
        {
            return b;
        }

        if (b.IsEmpty)
        {
            return a;
        }

        var arrA = a.ToArray();
        var arrB = b.ToArray();
        var ac = arrA.Length;
        var bc = arrB.Length;

        var newArr = new string[ac + bc];
        Array.Copy(arrA, 0, newArr, 0, ac);
        Array.Copy(arrB, 0, newArr, ac, bc);
        return new TextValues(newArr);
    }

    private void EnsureDictionaryBuilt()
    {
        if (this.m_isDictionaryBuilt)
        {
            return;
        }
        if (!this.m_hasDuplicateKeys && !this.m_hasNonPredefinedKeys && this.m_dictionary.Count == 0)
        {
            foreach (var item in this.m_pendingItems)
            {
                this.m_dictionary.Add(item.Key, item.Value);
            }
        }
        else
        {
            foreach (var item in this.m_pendingItems)
            {
                if (this.m_dictionary.TryGetValue(item.Key, out var old))
                {
                    this.m_dictionary[item.Key] = Merge(old, item.Value);
                }
                else
                {
                    this.m_dictionary.Add(item.Key, item.Value);
                }
            }
        }

        this.m_pendingItems.Clear();
        this.m_hasDuplicateKeys = false;
        this.m_hasNonPredefinedKeys = false;
        this.m_isDictionaryBuilt = true;
    }
}