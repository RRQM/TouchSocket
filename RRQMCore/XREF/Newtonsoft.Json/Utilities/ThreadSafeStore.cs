//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
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

using System.Collections.Generic;

#if !HAVE_LINQ
#endif
#if HAVE_CONCURRENT_DICTIONARY
using System.Collections.Concurrent;
#endif

namespace RRQMCore.XREF.Newtonsoft.Json.Utilities
{
    internal class ThreadSafeStore<TKey, TValue>
    {
#if HAVE_CONCURRENT_DICTIONARY
        private readonly ConcurrentDictionary<TKey, TValue> _concurrentStore;
#else
        private readonly object _lock = new object();
        private Dictionary<TKey, TValue> _store;
#endif
        private readonly Serialization.Func<TKey, TValue> _creator;

        public ThreadSafeStore(Serialization.Func<TKey, TValue> creator)
        {
            ValidationUtils.ArgumentNotNull(creator, nameof(creator));

            _creator = creator;
#if HAVE_CONCURRENT_DICTIONARY
            _concurrentStore = new ConcurrentDictionary<TKey, TValue>();
#else
            _store = new Dictionary<TKey, TValue>();
#endif
        }

        public TValue Get(TKey key)
        {
#if HAVE_CONCURRENT_DICTIONARY
            return _concurrentStore.GetOrAdd(key, _creator);
#else
            if (!_store.TryGetValue(key, out TValue value))
            {
                return AddValue(key);
            }

            return value;
#endif
        }

#if !HAVE_CONCURRENT_DICTIONARY

        private TValue AddValue(TKey key)
        {
            TValue value = _creator(key);

            lock (_lock)
            {
                if (_store == null)
                {
                    _store = new Dictionary<TKey, TValue>();
                    _store[key] = value;
                }
                else
                {
                    // double check locking
                    if (_store.TryGetValue(key, out TValue checkValue))
                    {
                        return checkValue;
                    }

                    Dictionary<TKey, TValue> newStore = new Dictionary<TKey, TValue>(_store);
                    newStore[key] = value;

#if HAVE_MEMORY_BARRIER
                    Thread.MemoryBarrier();
#endif
                    _store = newStore;
                }

                return value;
            }
        }

#endif
    }
}