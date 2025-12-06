// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TouchSocket.Core;

/// <summary>
/// 表示多文本值集合，可包含单个或多个值。
/// </summary>
[DebuggerDisplay("{ToString()}")]
[DebuggerTypeProxy(typeof(TextValuesDebugView))]
public readonly struct TextValues : IEnumerable<string>, IEquatable<TextValues>
{
    /// <summary>
    /// 空集合。
    /// </summary>
    public static readonly TextValues Empty = default;

    private readonly object m_values; // null|string|string[]

    /// <summary>
    /// 使用单个值初始化。
    /// </summary>
    public TextValues(string value)
    {
        this.m_values = value;
    }

    /// <summary>
    /// 使用多个值初始化。
    /// </summary>
    public TextValues(string[] values)
    {
        this.m_values = values;
    }

    /// <summary>
    /// 获取值数量。
    /// </summary>
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var value = this.m_values;
            if (value is null)
            {
                return 0;
            }
            if (value is string)
            {
                return 1;
            }
            return Unsafe.As<string[]>(value).Length;
        }
    }

    /// <summary>
    /// 获取第一个值，如果不存在则返回<see langword="null"/>。
    /// </summary>
    public string First
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var value = this.m_values;
            if (value is null)
            {
                return null;
            }

            if (value is string s)
            {
                return s;
            }

            var arr = Unsafe.As<string[]>(value);
            return arr.Length > 0 ? arr[0] : null;
        }
    }

    /// <summary>
    /// 指示是否为空集合。
    /// </summary>
    public bool IsEmpty => this.m_values is null;

    /// <summary>
    /// 通过索引获取指定值。
    /// </summary>
    public string this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var value = this.m_values ?? throw new IndexOutOfRangeException();
            if (value is string s)
            {
                return index == 0 ? s : throw new IndexOutOfRangeException();
            }
            return Unsafe.As<string[]>(value)[index];
        }
    }

    /// <summary>
    /// 隐式转换：<see cref="TextValues"/>到字符串，返回第一个值。
    /// </summary>
    public static implicit operator string(TextValues values) => values.First;

    /// <summary>
    /// 隐式转换：<see cref="TextValues"/>到字符串数组。
    /// </summary>
    public static implicit operator string[](TextValues values) => values.ToArray();

    /// <summary>
    /// 隐式转换：字符串到<see cref="TextValues"/>。
    /// </summary>
    public static implicit operator TextValues(string value) => new TextValues(value);

    /// <summary>
    /// 隐式转换：字符串数组到<see cref="TextValues"/>。
    /// </summary>
    public static implicit operator TextValues(string[] values) => new TextValues(values);

    /// <summary>
    /// 判断是否为空。
    /// </summary>
    public static bool IsNullOrEmpty(TextValues values) => values.Count == 0;

    /// <summary>
    /// 不相等运算符。
    /// </summary>
    public static bool operator !=(TextValues left, TextValues right) => !left.Equals(right);

    /// <summary>
    /// 相等运算符。
    /// </summary>
    public static bool operator ==(TextValues left, TextValues right) => left.Equals(right);

    /// <summary>
    /// 添加一个值，返回新集合。
    /// </summary>
    public TextValues Add(string value)
    {
        if (value is null)
        {
            return this;
        }
        var current = this.m_values;
        if (current is null)
        {
            return new TextValues(value);
        }
        if (current is string s)
        {
            return new TextValues(new[] { s, value });
        }
        var arr = Unsafe.As<string[]>(current);
        var newArr = new string[arr.Length + 1];
        Array.Copy(arr, newArr, arr.Length);
        newArr[arr.Length] = value;
        return new TextValues(newArr);
    }

    /// <summary>
    /// 判断相等。
    /// </summary>
    public bool Equals(string other, StringComparison comparison)
    {
        return this.Equals(new TextValues(other));
    }

    /// <summary>
    /// 判断相等。
    /// </summary>
    public bool Equals(TextValues other)
    {
        if (ReferenceEquals(this.m_values, other.m_values)) return true;
        var count = this.Count;
        if (count != other.Count) return false;
        if (count == 0) return true;
        if (count == 1)
        {
            return string.Equals(this.First, other.First, StringComparison.Ordinal);
        }
        var a = this.ToArray();
        var b = other.ToArray();
        for (var i = 0; i < a.Length; i++)
        {
            if (!string.Equals(a[i], b[i], StringComparison.Ordinal)) return false;
        }
        return true;
    }

    /// <summary>
    /// 判断相等。
    /// </summary>
    public override bool Equals(object obj) => obj is TextValues v && this.Equals(v);

    /// <summary>
    /// 获取枚举器。
    /// </summary>
    public Enumerator GetEnumerator() => new Enumerator(this.m_values);

    IEnumerator<string> IEnumerable<string>.GetEnumerator() => this.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <summary>
    /// 获取哈希码。
    /// </summary>
    public override int GetHashCode()
    {
        var value = this.m_values;
        if (value is null) return 0;
        if (value is string s)
        {
            return s?.GetHashCode() ?? 0;
        }
        var arr = Unsafe.As<string[]>(value);
        var hash = 17;
        for (var i = 0; i < arr.Length; i++)
        {
            hash = hash * 31 + (arr[i]?.GetHashCode() ?? 0);
        }
        return hash;
    }

    /// <summary>
    /// 转换为数组。
    /// </summary>
    public string[] ToArray()
    {
        var value = this.m_values;
        if (value is null)
        {
            return [];
        }
        if (value is string s)
        {
            return new[] { s };
        }
        return Unsafe.As<string[]>(value);
    }

    /// <summary>
    /// 返回字符串表示。单值返回该值，多个值使用","分隔。
    /// </summary>
    public override string ToString()
    {
        var value = this.m_values;
        if (value is null)
        {
            return string.Empty;
        }
        if (value is string s)
        {
            return s ?? string.Empty;
        }
        var arr = Unsafe.As<string[]>(value);
        if (arr.Length == 0)
        {
            return string.Empty;
        }

        if (arr.Length == 1)
        {
            return arr[0] ?? string.Empty;
        }

        return string.Join(",", arr);
    }

    /// <summary>
    /// 枚举器。
    /// </summary>
    public struct Enumerator : IEnumerator<string>
    {
        private readonly object m_values; // null|string|string[]
        private string m_current;
        private int m_index;

        internal Enumerator(object values)
        {
            this.m_values = values;
            this.m_index = -1;
            this.m_current = null;
        }

        /// <summary>
        /// 当前值。
        /// </summary>
        public readonly string Current => this.m_current;

        readonly object IEnumerator.Current => this.Current;

        /// <summary>
        /// 释放资源。
        /// </summary>
        public readonly void Dispose()
        {
        }

        /// <summary>
        /// 移动到下一项。
        /// </summary>
        public bool MoveNext()
        {
            if (this.m_values is null)
            {
                return false;
            }
            if (this.m_values is string s)
            {
                if (this.m_index < 0)
                {
                    this.m_index = 0;
                    this.m_current = s;
                    return true;
                }
                return false;
            }
            var arr = Unsafe.As<string[]>(this.m_values);
            var next = this.m_index + 1;
            if ((uint)next < (uint)arr.Length)
            {
                this.m_index = next;
                this.m_current = arr[next];
                return true;
            }
            return false;
        }

        /// <summary>
        /// 重置。
        /// </summary>
        public void Reset()
        {
            this.m_index = -1;
            this.m_current = null;
        }
    }

    private sealed class TextValuesDebugView(TextValues values)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public string[] Items => values.ToArray();
    }
}