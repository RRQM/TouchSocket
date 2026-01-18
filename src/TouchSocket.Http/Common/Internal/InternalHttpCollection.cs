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
using System.Diagnostics;

namespace TouchSocket.Http;

[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(InternalHttpCollectionDebugView))]
internal abstract class InternalHttpCollection : IHttpValues
{
    private readonly IEqualityComparer<string> m_comparer;
    private readonly List<KeyValuePair<string, TextValues>> m_items = new List<KeyValuePair<string, TextValues>>();

    protected InternalHttpCollection(IEqualityComparer<string> comparer = null)
    {
        this.m_comparer = comparer ?? StringComparer.OrdinalIgnoreCase;
    }

    public int Count => this.m_items.Count;

    public KeyValuePair<string, TextValues> this[int index] => throw new NotImplementedException();

    public TextValues this[string key]
    {
        get
        {
            ThrowHelper.ThrowIfNull(key, nameof(key));
            return this.Get(key);
        }

        set
        {
            ThrowHelper.ThrowIfNull(key, nameof(key));
            this.RemoveAll(key);
            this.m_items.Add(new KeyValuePair<string, TextValues>(key, value));
        }
    }

    public void Add(string key, TextValues value)
    {
        ThrowHelper.ThrowIfNull(key, nameof(key));
        this.m_items.Add(new KeyValuePair<string, TextValues>(key, value));
    }

    public KeyValuePair<string, TextValues>[] GetAll(string key)
    {
        ThrowHelper.ThrowIfNull(key, nameof(key));

        var results = new List<KeyValuePair<string, TextValues>>();
        for (var i = 0; i < this.m_items.Count; i++)
        {
            if (this.m_comparer.Equals(this.m_items[i].Key, key))
            {
                results.Add(this.m_items[i]);
            }
        }

        return results.ToArray();
    }

    public void Append(string key, TextValues value)
    {
        ThrowHelper.ThrowIfNull(key, nameof(key));

        var existingIndex = -1;
        for (var i = this.m_items.Count - 1; i >= 0; i--)
        {
            if (this.m_comparer.Equals(this.m_items[i].Key, key))
            {
                existingIndex = i;
                break;
            }
        }

        if (existingIndex >= 0)
        {
            var existingValue = this.m_items[existingIndex].Value;
            var newValues = existingValue;

            foreach (var val in value)
            {
                newValues = newValues.Add(val);
            }

            this.m_items[existingIndex] = new KeyValuePair<string, TextValues>(key, newValues);
        }
        else
        {
            this.m_items.Add(new KeyValuePair<string, TextValues>(key, value));
        }
    }

    public bool TryAppend(string key, TextValues value)
    {
        ThrowHelper.ThrowIfNull(key, nameof(key));

        var existingIndex = -1;
        for (var i = this.m_items.Count - 1; i >= 0; i--)
        {
            if (this.m_comparer.Equals(this.m_items[i].Key, key))
            {
                existingIndex = i;
                break;
            }
        }

        if (existingIndex >= 0)
        {
            var existingValue = this.m_items[existingIndex].Value;
            var existingArray = existingValue.ToArray();
            var hasNewValue = false;
            var newValues = existingValue;

            foreach (var val in value)
            {
                var isDuplicate = false;
                for (var i = 0; i < existingArray.Length; i++)
                {
                    if (string.Equals(existingArray[i], val, StringComparison.Ordinal))
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                if (!isDuplicate)
                {
                    newValues = newValues.Add(val);
                    hasNewValue = true;
                }
            }

            if (hasNewValue)
            {
                this.m_items[existingIndex] = new KeyValuePair<string, TextValues>(key, newValues);
                return true;
            }

            return false;
        }

        return false;
    }

    public void Clear()
    {
        this.m_items.Clear();
    }

    public bool ContainsKey(string key)
    {
        ThrowHelper.ThrowIfNull(key, nameof(key));

        for (var i = 0; i < this.m_items.Count; i++)
        {
            if (this.m_comparer.Equals(this.m_items[i].Key, key))
            {
                return true;
            }
        }
        return false;
    }

    public TextValues Get(string key)
    {
        ThrowHelper.ThrowIfNull(key, nameof(key));

        for (var i = 0; i < this.m_items.Count; i++)
        {
            var item = this.m_items[i];
            if (this.m_comparer.Equals(item.Key, key))
            {
                return item.Value;
            }
        }
        return TextValues.Empty;
    }

    public IEnumerator<KeyValuePair<string, TextValues>> GetEnumerator()
    {
        return this.m_items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public bool Remove(string key)
    {
        ThrowHelper.ThrowIfNull(key, nameof(key));

        for (var i = 0; i < this.m_items.Count; i++)
        {
            if (this.m_comparer.Equals(this.m_items[i].Key, key))
            {
                this.m_items.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public bool RemoveAll(string key)
    {
        ThrowHelper.ThrowIfNull(key, nameof(key));

        var removed = false;
        for (var i = this.m_items.Count - 1; i >= 0; i--)
        {
            if (this.m_comparer.Equals(this.m_items[i].Key, key))
            {
                this.m_items.RemoveAt(i);
                removed = true;
            }
        }
        return removed;
    }

    public bool TryAdd(string key, TextValues value)
    {
        ThrowHelper.ThrowIfNull(key, nameof(key));
        if (this.ContainsKey(key))
        {
            return false;
        }
        this.m_items.Add(new KeyValuePair<string, TextValues>(key, value));
        return true;
    }

    public bool TryGetValue(string key, out TextValues value)
    {
        ThrowHelper.ThrowIfNull(key, nameof(key));
        for (var i = 0; i < this.m_items.Count; i++)
        {
            var item = this.m_items[i];
            if (this.m_comparer.Equals(item.Key, key))
            {
                value= item.Value;
                return true;
            }
        }
        value =TextValues.Empty;
        return false;
    }

    internal void AddInternal(string key, TextValues value)
    {
        this.m_items.Add(new KeyValuePair<string, TextValues>(key, value));
    }

    [DebuggerDisplay("{Key}: {Value}")]
    private readonly struct HttpHeaderDebugItem
    {
        public HttpHeaderDebugItem(string key, TextValues value)
        {
            this.Key = key;
            this.Value = value.ToString();
            this.ValueCount = value.Count;
            this.Values = value.ToArray();
        }

        public string Key { get; }

        public string Value { get; }

        public int ValueCount { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public string[] Values { get; }
    }

    private sealed class InternalHttpCollectionDebugView
    {
        private readonly InternalHttpCollection m_collection;

        public InternalHttpCollectionDebugView(InternalHttpCollection collection)
        {
            this.m_collection = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        public int Count => this.m_collection.Count;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public HttpHeaderDebugItem[] Items
        {
            get
            {
                if (this.m_collection.Count == 0)
                {
                    return [];
                }

                var items = new HttpHeaderDebugItem[this.m_collection.Count];
                var index = 0;

                foreach (var kvp in this.m_collection)
                {
                    items[index++] = new HttpHeaderDebugItem(kvp.Key, kvp.Value);
                }

                return items;
            }
        }
    }
}