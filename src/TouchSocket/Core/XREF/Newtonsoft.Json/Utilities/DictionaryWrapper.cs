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

#region License

// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

#endregion License

using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;

#if !HAVE_LINQ

using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities.LinqBridge;

#else
using System.Linq;

#endif

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Utilities
{
    internal interface IWrappedDictionary
        : IDictionary
    {
        object UnderlyingDictionary { get; }
    }

    internal class DictionaryWrapper<TKey, TValue> : IDictionary<TKey, TValue>, IWrappedDictionary
    {
        private readonly IDictionary _dictionary;
        private readonly IDictionary<TKey, TValue> _genericDictionary;
#if HAVE_READ_ONLY_COLLECTIONS
        private readonly IReadOnlyDictionary<TKey, TValue> _readOnlyDictionary;
#endif
        private object _syncRoot;

        public DictionaryWrapper(IDictionary dictionary)
        {
            ValidationUtils.ArgumentNotNull(dictionary, nameof(dictionary));

            this._dictionary = dictionary;
        }

        public DictionaryWrapper(IDictionary<TKey, TValue> dictionary)
        {
            ValidationUtils.ArgumentNotNull(dictionary, nameof(dictionary));

            this._genericDictionary = dictionary;
        }

#if HAVE_READ_ONLY_COLLECTIONS
        public DictionaryWrapper(IReadOnlyDictionary<TKey, TValue> dictionary)
        {
            ValidationUtils.ArgumentNotNull(dictionary, nameof(dictionary));

            _readOnlyDictionary = dictionary;
        }
#endif

        public void Add(TKey key, TValue value)
        {
            if (this._dictionary != null)
            {
                this._dictionary.Add(key, value);
            }
            else if (this._genericDictionary != null)
            {
                this._genericDictionary.Add(key, value);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public bool ContainsKey(TKey key)
        {
            if (this._dictionary != null)
            {
                return this._dictionary.Contains(key);
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                return _readOnlyDictionary.ContainsKey(key);
            }
#endif
            else
            {
                return this._genericDictionary.ContainsKey(key);
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                if (this._dictionary != null)
                {
                    return this._dictionary.Keys.Cast<TKey>().ToList();
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    return _readOnlyDictionary.Keys.ToList();
                }
#endif
                else
                {
                    return this._genericDictionary.Keys;
                }
            }
        }

        public bool Remove(TKey key)
        {
            if (this._dictionary != null)
            {
                if (this._dictionary.Contains(key))
                {
                    this._dictionary.Remove(key);
                    return true;
                }
                else
                {
                    return false;
                }
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                throw new NotSupportedException();
            }
#endif
            else
            {
                return this._genericDictionary.Remove(key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (this._dictionary != null)
            {
                if (!this._dictionary.Contains(key))
                {
                    value = default(TValue);
                    return false;
                }
                else
                {
                    value = (TValue)this._dictionary[key];
                    return true;
                }
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                throw new NotSupportedException();
            }
#endif
            else
            {
                return this._genericDictionary.TryGetValue(key, out value);
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                if (this._dictionary != null)
                {
                    return this._dictionary.Values.Cast<TValue>().ToList();
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    return _readOnlyDictionary.Values.ToList();
                }
#endif
                else
                {
                    return this._genericDictionary.Values;
                }
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                if (this._dictionary != null)
                {
                    return (TValue)this._dictionary[key];
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    return _readOnlyDictionary[key];
                }
#endif
                else
                {
                    return this._genericDictionary[key];
                }
            }
            set
            {
                if (this._dictionary != null)
                {
                    this._dictionary[key] = value;
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    throw new NotSupportedException();
                }
#endif
                else
                {
                    this._genericDictionary[key] = value;
                }
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (this._dictionary != null)
            {
                ((IList)this._dictionary).Add(item);
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                throw new NotSupportedException();
            }
#endif
            else
            {
                this._genericDictionary?.Add(item);
            }
        }

        public void Clear()
        {
            if (this._dictionary != null)
            {
                this._dictionary.Clear();
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                throw new NotSupportedException();
            }
#endif
            else
            {
                this._genericDictionary.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (this._dictionary != null)
            {
                return ((IList)this._dictionary).Contains(item);
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                return _readOnlyDictionary.Contains(item);
            }
#endif
            else
            {
                return this._genericDictionary.Contains(item);
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (this._dictionary != null)
            {
                // Manual use of IDictionaryEnumerator instead of foreach to avoid DictionaryEntry box allocations.
                IDictionaryEnumerator e = this._dictionary.GetEnumerator();
                try
                {
                    while (e.MoveNext())
                    {
                        DictionaryEntry entry = e.Entry;
                        array[arrayIndex++] = new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value);
                    }
                }
                finally
                {
                    (e as IDisposable)?.Dispose();
                }
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                throw new NotSupportedException();
            }
#endif
            else
            {
                this._genericDictionary.CopyTo(array, arrayIndex);
            }
        }

        public int Count
        {
            get
            {
                if (this._dictionary != null)
                {
                    return this._dictionary.Count;
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    return _readOnlyDictionary.Count;
                }
#endif
                else
                {
                    return this._genericDictionary.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                if (this._dictionary != null)
                {
                    return this._dictionary.IsReadOnly;
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    return true;
                }
#endif
                else
                {
                    return this._genericDictionary.IsReadOnly;
                }
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (this._dictionary != null)
            {
                if (this._dictionary.Contains(item.Key))
                {
                    object value = this._dictionary[item.Key];

                    if (Equals(value, item.Value))
                    {
                        this._dictionary.Remove(item.Key);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                throw new NotSupportedException();
            }
#endif
            else
            {
                return this._genericDictionary.Remove(item);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (this._dictionary != null)
            {
                return this._dictionary.Cast<DictionaryEntry>().Select(de => new KeyValuePair<TKey, TValue>((TKey)de.Key, (TValue)de.Value)).GetEnumerator();
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                return _readOnlyDictionary.GetEnumerator();
            }
#endif
            else
            {
                return this._genericDictionary.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void IDictionary.Add(object key, object value)
        {
            if (this._dictionary != null)
            {
                this._dictionary.Add(key, value);
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                throw new NotSupportedException();
            }
#endif
            else
            {
                this._genericDictionary.Add((TKey)key, (TValue)value);
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                if (this._dictionary != null)
                {
                    return this._dictionary[key];
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    return _readOnlyDictionary[(TKey)key];
                }
#endif
                else
                {
                    return this._genericDictionary[(TKey)key];
                }
            }
            set
            {
                if (this._dictionary != null)
                {
                    this._dictionary[key] = value;
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    throw new NotSupportedException();
                }
#endif
                else
                {
                    this._genericDictionary[(TKey)key] = (TValue)value;
                }
            }
        }

        private readonly struct DictionaryEnumerator<TEnumeratorKey, TEnumeratorValue> : IDictionaryEnumerator
        {
            private readonly IEnumerator<KeyValuePair<TEnumeratorKey, TEnumeratorValue>> _e;

            public DictionaryEnumerator(IEnumerator<KeyValuePair<TEnumeratorKey, TEnumeratorValue>> e)
            {
                ValidationUtils.ArgumentNotNull(e, nameof(e));
                this._e = e;
            }

            public DictionaryEntry Entry => (DictionaryEntry)this.Current;

            public object Key => this.Entry.Key;

            public object Value => this.Entry.Value;

            public object Current => new DictionaryEntry(this._e.Current.Key, this._e.Current.Value);

            public bool MoveNext()
            {
                return this._e.MoveNext();
            }

            public void Reset()
            {
                this._e.Reset();
            }
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            if (this._dictionary != null)
            {
                return this._dictionary.GetEnumerator();
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                return new DictionaryEnumerator<TKey, TValue>(_readOnlyDictionary.GetEnumerator());
            }
#endif
            else
            {
                return new DictionaryEnumerator<TKey, TValue>(this._genericDictionary.GetEnumerator());
            }
        }

        bool IDictionary.Contains(object key)
        {
            if (this._genericDictionary != null)
            {
                return this._genericDictionary.ContainsKey((TKey)key);
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                return _readOnlyDictionary.ContainsKey((TKey)key);
            }
#endif
            else
            {
                return this._dictionary.Contains(key);
            }
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                if (this._genericDictionary != null)
                {
                    return false;
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    return true;
                }
#endif
                else
                {
                    return this._dictionary.IsFixedSize;
                }
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                if (this._genericDictionary != null)
                {
                    return this._genericDictionary.Keys.ToList();
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    return _readOnlyDictionary.Keys.ToList();
                }
#endif
                else
                {
                    return this._dictionary.Keys;
                }
            }
        }

        public void Remove(object key)
        {
            if (this._dictionary != null)
            {
                this._dictionary.Remove(key);
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                throw new NotSupportedException();
            }
#endif
            else
            {
                this._genericDictionary.Remove((TKey)key);
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                if (this._genericDictionary != null)
                {
                    return this._genericDictionary.Values.ToList();
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    return _readOnlyDictionary.Values.ToList();
                }
#endif
                else
                {
                    return this._dictionary.Values;
                }
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (this._dictionary != null)
            {
                this._dictionary.CopyTo(array, index);
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                throw new NotSupportedException();
            }
#endif
            else
            {
                this._genericDictionary.CopyTo((KeyValuePair<TKey, TValue>[])array, index);
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                if (this._dictionary != null)
                {
                    return this._dictionary.IsSynchronized;
                }
                else
                {
                    return false;
                }
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                if (this._syncRoot == null)
                {
                    Interlocked.CompareExchange(ref this._syncRoot, new object(), null);
                }

                return this._syncRoot;
            }
        }

        public object UnderlyingDictionary
        {
            get
            {
                if (this._dictionary != null)
                {
                    return this._dictionary;
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    return _readOnlyDictionary;
                }
#endif
                else
                {
                    return this._genericDictionary;
                }
            }
        }
    }
}