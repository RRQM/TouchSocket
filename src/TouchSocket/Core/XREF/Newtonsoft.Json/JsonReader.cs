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
using System.Globalization;

#if HAVE_BIG_INTEGER
using System.Numerics;
#endif

using TouchSocket.Core.XREF.Newtonsoft.Json.Serialization;
using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities;

namespace TouchSocket.Core.XREF.Newtonsoft.Json
{
    /// <summary>
    /// Represents a reader that provides fast, non-cached, forward-only access to serialized JSON data.
    /// </summary>
    public abstract partial class JsonReader : IDisposable
    {
        /// <summary>
        /// Specifies the state of the reader.
        /// </summary>
        protected internal enum State
        {
            /// <summary>
            /// A <see cref="JsonReader"/> read method has not been called.
            /// </summary>
            Start,

            /// <summary>
            /// The end of the file has been reached successfully.
            /// </summary>
            Complete,

            /// <summary>
            /// Reader is at a property.
            /// </summary>
            Property,

            /// <summary>
            /// Reader is at the start of an object.
            /// </summary>
            ObjectStart,

            /// <summary>
            /// Reader is in an object.
            /// </summary>
            Object,

            /// <summary>
            /// Reader is at the start of an array.
            /// </summary>
            ArrayStart,

            /// <summary>
            /// Reader is in an array.
            /// </summary>
            Array,

            /// <summary>
            /// The <see cref="JsonReader.Close()"/> method has been called.
            /// </summary>
            Closed,

            /// <summary>
            /// Reader has just read a value.
            /// </summary>
            PostValue,

            /// <summary>
            /// Reader is at the start of a constructor.
            /// </summary>
            ConstructorStart,

            /// <summary>
            /// Reader is in a constructor.
            /// </summary>
            Constructor,

            /// <summary>
            /// An error occurred that prevents the read operation from continuing.
            /// </summary>
            Error,

            /// <summary>
            /// The end of the file has been reached successfully.
            /// </summary>
            Finished
        }

        // current Token data
        private JsonToken _tokenType;

        private object _value;
        internal char _quoteChar;
        internal State _currentState;
        private JsonPosition _currentPosition;
        private CultureInfo _culture;
        private DateTimeZoneHandling _dateTimeZoneHandling;
        private int? _maxDepth;
        private bool _hasExceededMaxDepth;
        internal DateParseHandling _dateParseHandling;
        internal FloatParseHandling _floatParseHandling;
        private string _dateFormatString;
        private List<JsonPosition> _stack;

        /// <summary>
        /// Gets the current reader state.
        /// </summary>
        /// <value>The current reader state.</value>
        protected State CurrentState => this._currentState;

        /// <summary>
        /// Gets or sets a value indicating whether the source should be closed when this reader is closed.
        /// </summary>
        /// <value>
        /// <c>true</c> to close the source when this reader is closed; otherwise <c>false</c>. The default is <c>true</c>.
        /// </value>
        public bool CloseInput { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether multiple pieces of JSON content can
        /// be read from a continuous stream without erroring.
        /// </summary>
        /// <value>
        /// <c>true</c> to support reading multiple pieces of JSON content; otherwise <c>false</c>.
        /// The default is <c>false</c>.
        /// </value>
        public bool SupportMultipleContent { get; set; }

        /// <summary>
        /// Gets the quotation mark character used to enclose the value of a string.
        /// </summary>
        public virtual char QuoteChar
        {
            get => this._quoteChar;
            protected internal set => this._quoteChar = value;
        }

        /// <summary>
        /// Gets or sets how <see cref="DateTime"/> time zones are handled when reading JSON.
        /// </summary>
        public DateTimeZoneHandling DateTimeZoneHandling
        {
            get => this._dateTimeZoneHandling;
            set
            {
                if (value < DateTimeZoneHandling.Local || value > DateTimeZoneHandling.RoundtripKind)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this._dateTimeZoneHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how date formatted strings, e.g. "\/Date(1198908717056)\/" and "2012-03-21T05:40Z", are parsed when reading JSON.
        /// </summary>
        public DateParseHandling DateParseHandling
        {
            get => this._dateParseHandling;
            set
            {
                if (value < DateParseHandling.None ||
#if HAVE_DATE_TIME_OFFSET
                    value > DateParseHandling.DateTimeOffset
#else
                    value > DateParseHandling.DateTime
#endif
                    )
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this._dateParseHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how floating point numbers, e.g. 1.0 and 9.9, are parsed when reading JSON text.
        /// </summary>
        public FloatParseHandling FloatParseHandling
        {
            get => this._floatParseHandling;
            set
            {
                if (value < FloatParseHandling.Double || value > FloatParseHandling.Decimal)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this._floatParseHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how custom date formatted strings are parsed when reading JSON.
        /// </summary>
        public string DateFormatString
        {
            get => this._dateFormatString;
            set => this._dateFormatString = value;
        }

        /// <summary>
        /// Gets or sets the maximum depth allowed when reading JSON. Reading past this depth will throw a <see cref="JsonReaderException"/>.
        /// </summary>
        public int? MaxDepth
        {
            get => this._maxDepth;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Value must be positive.", nameof(value));
                }

                this._maxDepth = value;
            }
        }

        /// <summary>
        /// Gets the type of the current JSON token.
        /// </summary>
        public virtual JsonToken TokenType => this._tokenType;

        /// <summary>
        /// Gets the text value of the current JSON token.
        /// </summary>
        public virtual object Value => this._value;

        /// <summary>
        /// Gets the .NET type for the current JSON token.
        /// </summary>
        public virtual Type ValueType => this._value?.GetType();

        /// <summary>
        /// Gets the depth of the current token in the JSON document.
        /// </summary>
        /// <value>The depth of the current token in the JSON document.</value>
        public virtual int Depth
        {
            get
            {
                int depth = this._stack?.Count ?? 0;
                if (JsonTokenUtils.IsStartToken(this.TokenType) || this._currentPosition.Type == JsonContainerType.None)
                {
                    return depth;
                }
                else
                {
                    return depth + 1;
                }
            }
        }

        /// <summary>
        /// Gets the path of the current JSON token.
        /// </summary>
        public virtual string Path
        {
            get
            {
                if (this._currentPosition.Type == JsonContainerType.None)
                {
                    return string.Empty;
                }

                bool insideContainer = (this._currentState != State.ArrayStart
                                        && this._currentState != State.ConstructorStart
                                       && this._currentState != State.ObjectStart);
                JsonPosition? current;
                if (insideContainer)
                {
                    current = this._currentPosition;
                }
                else
                {
                    current = null;
                }
                return JsonPosition.BuildPath(this._stack, current);
            }
        }

        /// <summary>
        /// Gets or sets the culture used when reading JSON. Defaults to <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        public CultureInfo Culture
        {
            get => this._culture ?? CultureInfo.InvariantCulture;
            set => this._culture = value;
        }

        internal JsonPosition GetPosition(int depth)
        {
            if (this._stack != null && depth < this._stack.Count)
            {
                return this._stack[depth];
            }

            return this._currentPosition;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonReader"/> class.
        /// </summary>
        protected JsonReader()
        {
            this._currentState = State.Start;
            this._dateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
            this._dateParseHandling = DateParseHandling.DateTime;
            this._floatParseHandling = FloatParseHandling.Double;

            this.CloseInput = true;
        }

        private void Push(JsonContainerType value)
        {
            this.UpdateScopeWithFinishedValue();

            if (this._currentPosition.Type == JsonContainerType.None)
            {
                this._currentPosition = new JsonPosition(value);
            }
            else
            {
                if (this._stack == null)
                {
                    this._stack = new List<JsonPosition>();
                }

                this._stack.Add(this._currentPosition);
                this._currentPosition = new JsonPosition(value);

                // this is a little hacky because Depth increases when first property/value is written but only testing here is faster/simpler
                if (this._maxDepth != null && this.Depth + 1 > this._maxDepth && !this._hasExceededMaxDepth)
                {
                    this._hasExceededMaxDepth = true;
                    throw JsonReaderException.Create(this, "The reader's MaxDepth of {0} has been exceeded.".FormatWith(CultureInfo.InvariantCulture, this._maxDepth));
                }
            }
        }

        private JsonContainerType Pop()
        {
            JsonPosition oldPosition;
            if (this._stack != null && this._stack.Count > 0)
            {
                oldPosition = this._currentPosition;
                this._currentPosition = this._stack[this._stack.Count - 1];
                this._stack.RemoveAt(this._stack.Count - 1);
            }
            else
            {
                oldPosition = this._currentPosition;
                this._currentPosition = new JsonPosition();
            }

            if (this._maxDepth != null && this.Depth <= this._maxDepth)
            {
                this._hasExceededMaxDepth = false;
            }

            return oldPosition.Type;
        }

        private JsonContainerType Peek()
        {
            return this._currentPosition.Type;
        }

        /// <summary>
        /// Reads the next JSON token from the source.
        /// </summary>
        /// <returns><c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.</returns>
        public abstract bool Read();

        /// <summary>
        /// Reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="Int32"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="Int32"/>. This method will return <c>null</c> at the end of an array.</returns>
        public virtual int? ReadAsInt32()
        {
            JsonToken t = this.GetContentToken();

            switch (t)
            {
                case JsonToken.None:
                case JsonToken.Null:
                case JsonToken.EndArray:
                    return null;

                case JsonToken.Integer:
                case JsonToken.Float:
                    object v = this.Value;
                    if (v is int i)
                    {
                        return i;
                    }

#if HAVE_BIG_INTEGER
                    if (v is BigInteger value)
                    {
                        i = (int)value;
                    }
                    else
#endif
                    {
                        try
                        {
                            i = Convert.ToInt32(v, CultureInfo.InvariantCulture);
                        }
                        catch (Exception ex)
                        {
                            // handle error for large integer overflow exceptions
                            throw JsonReaderException.Create(this, "Could not convert to integer: {0}.".FormatWith(CultureInfo.InvariantCulture, v), ex);
                        }
                    }

                    this.SetToken(JsonToken.Integer, i, false);
                    return i;

                case JsonToken.String:
                    string s = (string)this.Value;
                    return this.ReadInt32String(s);
            }

            throw JsonReaderException.Create(this, "Error reading integer. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
        }

        internal int? ReadInt32String(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                this.SetToken(JsonToken.Null, null, false);
                return null;
            }

            if (int.TryParse(s, NumberStyles.Integer, this.Culture, out int i))
            {
                this.SetToken(JsonToken.Integer, i, false);
                return i;
            }
            else
            {
                this.SetToken(JsonToken.String, s, false);
                throw JsonReaderException.Create(this, "Could not convert string to integer: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
            }
        }

        /// <summary>
        /// Reads the next JSON token from the source as a <see cref="String"/>.
        /// </summary>
        /// <returns>A <see cref="String"/>. This method will return <c>null</c> at the end of an array.</returns>
        public virtual string ReadAsString()
        {
            JsonToken t = this.GetContentToken();

            switch (t)
            {
                case JsonToken.None:
                case JsonToken.Null:
                case JsonToken.EndArray:
                    return null;

                case JsonToken.String:
                    return (string)this.Value;
            }

            if (JsonTokenUtils.IsPrimitiveToken(t))
            {
                object v = this.Value;
                if (v != null)
                {
                    string s;
                    if (v is IFormattable formattable)
                    {
                        s = formattable.ToString(null, this.Culture);
                    }
                    else
                    {
                        Uri uri = v as Uri;
                        s = uri != null ? uri.OriginalString : v.ToString();
                    }

                    this.SetToken(JsonToken.String, s, false);
                    return s;
                }
            }

            throw JsonReaderException.Create(this, "Error reading string. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
        }

        /// <summary>
        /// Reads the next JSON token from the source as a <see cref="Byte"/>[].
        /// </summary>
        /// <returns>A <see cref="Byte"/>[] or <c>null</c> if the next JSON token is null. This method will return <c>null</c> at the end of an array.</returns>
        public virtual byte[] ReadAsBytes()
        {
            JsonToken t = this.GetContentToken();

            switch (t)
            {
                case JsonToken.StartObject:
                    {
                        this.ReadIntoWrappedTypeObject();

                        byte[] data = this.ReadAsBytes();
                        this.ReaderReadAndAssert();

                        if (this.TokenType != JsonToken.EndObject)
                        {
                            throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, this.TokenType));
                        }

                        this.SetToken(JsonToken.Bytes, data, false);
                        return data;
                    }
                case JsonToken.String:
                    {
                        // attempt to convert possible base 64 or GUID string to bytes
                        // GUID has to have format 00000000-0000-0000-0000-000000000000
                        string s = (string)this.Value;

                        byte[] data;

                        if (s.Length == 0)
                        {
                            data = CollectionUtils.ArrayEmpty<byte>();
                        }
                        else if (ConvertUtils.TryConvertGuid(s, out Guid g1))
                        {
                            data = g1.ToByteArray();
                        }
                        else
                        {
                            data = Convert.FromBase64String(s);
                        }

                        this.SetToken(JsonToken.Bytes, data, false);
                        return data;
                    }
                case JsonToken.None:
                case JsonToken.Null:
                case JsonToken.EndArray:
                    return null;

                case JsonToken.Bytes:
                    if (this.Value is Guid g2)
                    {
                        byte[] data = g2.ToByteArray();
                        this.SetToken(JsonToken.Bytes, data, false);
                        return data;
                    }

                    return (byte[])this.Value;

                case JsonToken.StartArray:
                    return this.ReadArrayIntoByteArray();
            }

            throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
        }

        internal byte[] ReadArrayIntoByteArray()
        {
            List<byte> buffer = new List<byte>();

            while (true)
            {
                if (!this.Read())
                {
                    this.SetToken(JsonToken.None);
                }

                if (this.ReadArrayElementIntoByteArrayReportDone(buffer))
                {
                    byte[] d = buffer.ToArray();
                    this.SetToken(JsonToken.Bytes, d, false);
                    return d;
                }
            }
        }

        private bool ReadArrayElementIntoByteArrayReportDone(List<byte> buffer)
        {
            switch (this.TokenType)
            {
                case JsonToken.None:
                    throw JsonReaderException.Create(this, "Unexpected end when reading bytes.");
                case JsonToken.Integer:
                    buffer.Add(Convert.ToByte(this.Value, CultureInfo.InvariantCulture));
                    return false;

                case JsonToken.EndArray:
                    return true;

                case JsonToken.Comment:
                    return false;

                default:
                    throw JsonReaderException.Create(this, "Unexpected token when reading bytes: {0}.".FormatWith(CultureInfo.InvariantCulture, this.TokenType));
            }
        }

        /// <summary>
        /// Reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="Double"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="Double"/>. This method will return <c>null</c> at the end of an array.</returns>
        public virtual double? ReadAsDouble()
        {
            JsonToken t = this.GetContentToken();

            switch (t)
            {
                case JsonToken.None:
                case JsonToken.Null:
                case JsonToken.EndArray:
                    return null;

                case JsonToken.Integer:
                case JsonToken.Float:
                    object v = this.Value;
                    if (v is double d)
                    {
                        return d;
                    }

#if HAVE_BIG_INTEGER
                    if (v is BigInteger value)
                    {
                        d = (double)value;
                    }
                    else
#endif
                    {
                        d = Convert.ToDouble(v, CultureInfo.InvariantCulture);
                    }

                    this.SetToken(JsonToken.Float, d, false);

                    return (double)d;

                case JsonToken.String:
                    return this.ReadDoubleString((string)this.Value);
            }

            throw JsonReaderException.Create(this, "Error reading double. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
        }

        internal double? ReadDoubleString(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                this.SetToken(JsonToken.Null, null, false);
                return null;
            }

            if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, this.Culture, out double d))
            {
                this.SetToken(JsonToken.Float, d, false);
                return d;
            }
            else
            {
                this.SetToken(JsonToken.String, s, false);
                throw JsonReaderException.Create(this, "Could not convert string to double: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
            }
        }

        /// <summary>
        /// Reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="Boolean"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="Boolean"/>. This method will return <c>null</c> at the end of an array.</returns>
        public virtual bool? ReadAsBoolean()
        {
            JsonToken t = this.GetContentToken();

            switch (t)
            {
                case JsonToken.None:
                case JsonToken.Null:
                case JsonToken.EndArray:
                    return null;

                case JsonToken.Integer:
                case JsonToken.Float:
                    bool b;
#if HAVE_BIG_INTEGER
                    if (Value is BigInteger integer)
                    {
                        b = integer != 0;
                    }
                    else
#endif
                    {
                        b = Convert.ToBoolean(this.Value, CultureInfo.InvariantCulture);
                    }

                    this.SetToken(JsonToken.Boolean, b, false);
                    return b;

                case JsonToken.String:
                    return this.ReadBooleanString((string)this.Value);

                case JsonToken.Boolean:
                    return (bool)this.Value;
            }

            throw JsonReaderException.Create(this, "Error reading boolean. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
        }

        internal bool? ReadBooleanString(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                this.SetToken(JsonToken.Null, null, false);
                return null;
            }

            if (bool.TryParse(s, out bool b))
            {
                this.SetToken(JsonToken.Boolean, b, false);
                return b;
            }
            else
            {
                this.SetToken(JsonToken.String, s, false);
                throw JsonReaderException.Create(this, "Could not convert string to boolean: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
            }
        }

        /// <summary>
        /// Reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="Decimal"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="Decimal"/>. This method will return <c>null</c> at the end of an array.</returns>
        public virtual decimal? ReadAsDecimal()
        {
            JsonToken t = this.GetContentToken();

            switch (t)
            {
                case JsonToken.None:
                case JsonToken.Null:
                case JsonToken.EndArray:
                    return null;

                case JsonToken.Integer:
                case JsonToken.Float:
                    object v = this.Value;

                    if (v is decimal d)
                    {
                        return d;
                    }

#if HAVE_BIG_INTEGER
                    if (v is BigInteger value)
                    {
                        d = (decimal)value;
                    }
                    else
#endif
                    {
                        try
                        {
                            d = Convert.ToDecimal(v, CultureInfo.InvariantCulture);
                        }
                        catch (Exception ex)
                        {
                            // handle error for large integer overflow exceptions
                            throw JsonReaderException.Create(this, "Could not convert to decimal: {0}.".FormatWith(CultureInfo.InvariantCulture, v), ex);
                        }
                    }

                    this.SetToken(JsonToken.Float, d, false);
                    return d;

                case JsonToken.String:
                    return this.ReadDecimalString((string)this.Value);
            }

            throw JsonReaderException.Create(this, "Error reading decimal. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
        }

        internal decimal? ReadDecimalString(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                this.SetToken(JsonToken.Null, null, false);
                return null;
            }

            if (decimal.TryParse(s, NumberStyles.Number, this.Culture, out decimal d))
            {
                this.SetToken(JsonToken.Float, d, false);
                return d;
            }
            else
            {
                this.SetToken(JsonToken.String, s, false);
                throw JsonReaderException.Create(this, "Could not convert string to decimal: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
            }
        }

        /// <summary>
        /// Reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="DateTime"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="DateTime"/>. This method will return <c>null</c> at the end of an array.</returns>
        public virtual DateTime? ReadAsDateTime()
        {
            switch (this.GetContentToken())
            {
                case JsonToken.None:
                case JsonToken.Null:
                case JsonToken.EndArray:
                    return null;

                case JsonToken.Date:
#if HAVE_DATE_TIME_OFFSET
                    if (Value is DateTimeOffset offset)
                    {
                        SetToken(JsonToken.Date, offset.DateTime, false);
                    }
#endif

                    return (DateTime)this.Value;

                case JsonToken.String:
                    string s = (string)this.Value;
                    return this.ReadDateTimeString(s);
            }

            throw JsonReaderException.Create(this, "Error reading date. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, this.TokenType));
        }

        internal DateTime? ReadDateTimeString(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                this.SetToken(JsonToken.Null, null, false);
                return null;
            }

            if (DateTimeUtils.TryParseDateTime(s, this.DateTimeZoneHandling, this._dateFormatString, this.Culture, out DateTime dt))
            {
                dt = DateTimeUtils.EnsureDateTime(dt, this.DateTimeZoneHandling);
                this.SetToken(JsonToken.Date, dt, false);
                return dt;
            }

            if (DateTime.TryParse(s, this.Culture, DateTimeStyles.RoundtripKind, out dt))
            {
                dt = DateTimeUtils.EnsureDateTime(dt, this.DateTimeZoneHandling);
                this.SetToken(JsonToken.Date, dt, false);
                return dt;
            }

            throw JsonReaderException.Create(this, "Could not convert string to DateTime: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
        }

#if HAVE_DATE_TIME_OFFSET
        /// <summary>
        /// Reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/>. This method will return <c>null</c> at the end of an array.</returns>
        public virtual DateTimeOffset? ReadAsDateTimeOffset()
        {
            JsonToken t = GetContentToken();

            switch (t)
            {
                case JsonToken.None:
                case JsonToken.Null:
                case JsonToken.EndArray:
                    return null;

                case JsonToken.Date:
                    if (Value is DateTime time)
                    {
                        SetToken(JsonToken.Date, new DateTimeOffset(time), false);
                    }

                    return (DateTimeOffset)Value;

                case JsonToken.String:
                    string s = (string)Value;
                    return ReadDateTimeOffsetString(s);

                default:
                    throw JsonReaderException.Create(this, "Error reading date. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
            }
        }

        internal DateTimeOffset? ReadDateTimeOffsetString(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                SetToken(JsonToken.Null, null, false);
                return null;
            }

            if (DateTimeUtils.TryParseDateTimeOffset(s, _dateFormatString, Culture, out DateTimeOffset dt))
            {
                SetToken(JsonToken.Date, dt, false);
                return dt;
            }

            if (DateTimeOffset.TryParse(s, Culture, DateTimeStyles.RoundtripKind, out dt))
            {
                SetToken(JsonToken.Date, dt, false);
                return dt;
            }

            SetToken(JsonToken.String, s, false);
            throw JsonReaderException.Create(this, "Could not convert string to DateTimeOffset: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
        }
#endif

        internal void ReaderReadAndAssert()
        {
            if (!this.Read())
            {
                throw this.CreateUnexpectedEndException();
            }
        }

        internal JsonReaderException CreateUnexpectedEndException()
        {
            return JsonReaderException.Create(this, "Unexpected end when reading JSON.");
        }

        internal void ReadIntoWrappedTypeObject()
        {
            this.ReaderReadAndAssert();
            if (this.Value != null && this.Value.ToString() == JsonTypeReflector.TypePropertyName)
            {
                this.ReaderReadAndAssert();
                if (this.Value != null && this.Value.ToString().StartsWith("System.Byte[]", StringComparison.Ordinal))
                {
                    this.ReaderReadAndAssert();
                    if (this.Value.ToString() == JsonTypeReflector.ValuePropertyName)
                    {
                        return;
                    }
                }
            }

            throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, JsonToken.StartObject));
        }

        /// <summary>
        /// Skips the children of the current token.
        /// </summary>
        public void Skip()
        {
            if (this.TokenType == JsonToken.PropertyName)
            {
                this.Read();
            }

            if (JsonTokenUtils.IsStartToken(this.TokenType))
            {
                int depth = this.Depth;

                while (this.Read() && (depth < this.Depth))
                {
                }
            }
        }

        /// <summary>
        /// Sets the current token.
        /// </summary>
        /// <param name="newToken">The new token.</param>
        protected void SetToken(JsonToken newToken)
        {
            this.SetToken(newToken, null, true);
        }

        /// <summary>
        /// Sets the current token and value.
        /// </summary>
        /// <param name="newToken">The new token.</param>
        /// <param name="value">The value.</param>
        protected void SetToken(JsonToken newToken, object value)
        {
            this.SetToken(newToken, value, true);
        }

        /// <summary>
        /// Sets the current token and value.
        /// </summary>
        /// <param name="newToken">The new token.</param>
        /// <param name="value">The value.</param>
        /// <param name="updateIndex">A flag indicating whether the position index inside an array should be updated.</param>
        protected void SetToken(JsonToken newToken, object value, bool updateIndex)
        {
            this._tokenType = newToken;
            this._value = value;

            switch (newToken)
            {
                case JsonToken.StartObject:
                    this._currentState = State.ObjectStart;
                    this.Push(JsonContainerType.Object);
                    break;

                case JsonToken.StartArray:
                    this._currentState = State.ArrayStart;
                    this.Push(JsonContainerType.Array);
                    break;

                case JsonToken.StartConstructor:
                    this._currentState = State.ConstructorStart;
                    this.Push(JsonContainerType.Constructor);
                    break;

                case JsonToken.EndObject:
                    this.ValidateEnd(JsonToken.EndObject);
                    break;

                case JsonToken.EndArray:
                    this.ValidateEnd(JsonToken.EndArray);
                    break;

                case JsonToken.EndConstructor:
                    this.ValidateEnd(JsonToken.EndConstructor);
                    break;

                case JsonToken.PropertyName:
                    this._currentState = State.Property;

                    this._currentPosition.PropertyName = (string)value;
                    break;

                case JsonToken.Undefined:
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.Boolean:
                case JsonToken.Null:
                case JsonToken.Date:
                case JsonToken.String:
                case JsonToken.Raw:
                case JsonToken.Bytes:
                    this.SetPostValueState(updateIndex);
                    break;
            }
        }

        internal void SetPostValueState(bool updateIndex)
        {
            if (this.Peek() != JsonContainerType.None || this.SupportMultipleContent)
            {
                this._currentState = State.PostValue;
            }
            else
            {
                this.SetFinished();
            }

            if (updateIndex)
            {
                this.UpdateScopeWithFinishedValue();
            }
        }

        private void UpdateScopeWithFinishedValue()
        {
            if (this._currentPosition.HasIndex)
            {
                this._currentPosition.Position++;
            }
        }

        private void ValidateEnd(JsonToken endToken)
        {
            JsonContainerType currentObject = this.Pop();

            if (this.GetTypeForCloseToken(endToken) != currentObject)
            {
                throw JsonReaderException.Create(this, "JsonToken {0} is not valid for closing JsonType {1}.".FormatWith(CultureInfo.InvariantCulture, endToken, currentObject));
            }

            if (this.Peek() != JsonContainerType.None || this.SupportMultipleContent)
            {
                this._currentState = State.PostValue;
            }
            else
            {
                this.SetFinished();
            }
        }

        /// <summary>
        /// Sets the state based on current token type.
        /// </summary>
        protected void SetStateBasedOnCurrent()
        {
            JsonContainerType currentObject = this.Peek();

            switch (currentObject)
            {
                case JsonContainerType.Object:
                    this._currentState = State.Object;
                    break;

                case JsonContainerType.Array:
                    this._currentState = State.Array;
                    break;

                case JsonContainerType.Constructor:
                    this._currentState = State.Constructor;
                    break;

                case JsonContainerType.None:
                    this.SetFinished();
                    break;

                default:
                    throw JsonReaderException.Create(this, "While setting the reader state back to current object an unexpected JsonType was encountered: {0}".FormatWith(CultureInfo.InvariantCulture, currentObject));
            }
        }

        private void SetFinished()
        {
            this._currentState = this.SupportMultipleContent ? State.Start : State.Finished;
        }

        private JsonContainerType GetTypeForCloseToken(JsonToken token)
        {
            switch (token)
            {
                case JsonToken.EndObject:
                    return JsonContainerType.Object;

                case JsonToken.EndArray:
                    return JsonContainerType.Array;

                case JsonToken.EndConstructor:
                    return JsonContainerType.Constructor;

                default:
                    throw JsonReaderException.Create(this, "Not a valid close JsonToken: {0}".FormatWith(CultureInfo.InvariantCulture, token));
            }
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this._currentState != State.Closed && disposing)
            {
                this.Close();
            }
        }

        /// <summary>
        /// Changes the reader's state to <see cref="JsonReader.State.Closed"/>.
        /// If <see cref="JsonReader.CloseInput"/> is set to <c>true</c>, the source is also closed.
        /// </summary>
        public virtual void Close()
        {
            this._currentState = State.Closed;
            this._tokenType = JsonToken.None;
            this._value = null;
        }

        internal void ReadAndAssert()
        {
            if (!this.Read())
            {
                throw JsonSerializationException.Create(this, "Unexpected end when reading JSON.");
            }
        }

        internal void ReadForTypeAndAssert(JsonContract contract, bool hasConverter)
        {
            if (!this.ReadForType(contract, hasConverter))
            {
                throw JsonSerializationException.Create(this, "Unexpected end when reading JSON.");
            }
        }

        internal bool ReadForType(JsonContract contract, bool hasConverter)
        {
            // don't read properties with converters as a specific value
            // the value might be a string which will then get converted which will error if read as date for example
            if (hasConverter)
            {
                return this.Read();
            }

            ReadType t = contract?.InternalReadType ?? ReadType.Read;

            switch (t)
            {
                case ReadType.Read:
                    return this.ReadAndMoveToContent();

                case ReadType.ReadAsInt32:
                    this.ReadAsInt32();
                    break;

                case ReadType.ReadAsInt64:
                    bool result = this.ReadAndMoveToContent();
                    if (this.TokenType == JsonToken.Undefined)
                    {
                        throw JsonReaderException.Create(this, "An undefined token is not a valid {0}.".FormatWith(CultureInfo.InvariantCulture, contract?.UnderlyingType ?? typeof(long)));
                    }
                    return result;

                case ReadType.ReadAsDecimal:
                    this.ReadAsDecimal();
                    break;

                case ReadType.ReadAsDouble:
                    this.ReadAsDouble();
                    break;

                case ReadType.ReadAsBytes:
                    this.ReadAsBytes();
                    break;

                case ReadType.ReadAsBoolean:
                    this.ReadAsBoolean();
                    break;

                case ReadType.ReadAsString:
                    this.ReadAsString();
                    break;

                case ReadType.ReadAsDateTime:
                    this.ReadAsDateTime();
                    break;
#if HAVE_DATE_TIME_OFFSET
                case ReadType.ReadAsDateTimeOffset:
                    ReadAsDateTimeOffset();
                    break;
#endif
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return (this.TokenType != JsonToken.None);
        }

        internal bool ReadAndMoveToContent()
        {
            return this.Read() && this.MoveToContent();
        }

        internal bool MoveToContent()
        {
            JsonToken t = this.TokenType;
            while (t == JsonToken.None || t == JsonToken.Comment)
            {
                if (!this.Read())
                {
                    return false;
                }

                t = this.TokenType;
            }

            return true;
        }

        private JsonToken GetContentToken()
        {
            JsonToken t;
            do
            {
                if (!this.Read())
                {
                    this.SetToken(JsonToken.None);
                    return JsonToken.None;
                }
                else
                {
                    t = this.TokenType;
                }
            } while (t == JsonToken.Comment);

            return t;
        }
    }
}