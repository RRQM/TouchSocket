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

#if HAVE_BIG_INTEGER
using System.Numerics;
#endif

using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities;
using System.Globalization;

#if !HAVE_LINQ

using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities.LinqBridge;

#else
using System.Linq;

#endif

namespace TouchSocket.Core.XREF.Newtonsoft.Json
{
    /// <summary>
    /// Represents a writer that provides a fast, non-cached, forward-only way of generating JSON data.
    /// </summary>
    public abstract partial class JsonWriter : IDisposable
    {
        internal enum State
        {
            Start = 0,
            Property = 1,
            ObjectStart = 2,
            Object = 3,
            ArrayStart = 4,
            Array = 5,
            ConstructorStart = 6,
            Constructor = 7,
            Closed = 8,
            Error = 9
        }

        // array that gives a new state based on the current state an the token being written
        private static readonly State[][] StateArray;

        internal static readonly State[][] StateArrayTempate = new[]
        {
            //                                      Start                    PropertyName            ObjectStart         Object            ArrayStart              Array                   ConstructorStart        Constructor             Closed       Error
            //
            /* None                        */new[] { State.Error,            State.Error,            State.Error,        State.Error,      State.Error,            State.Error,            State.Error,            State.Error,            State.Error, State.Error },
            /* StartObject                 */new[] { State.ObjectStart,      State.ObjectStart,      State.Error,        State.Error,      State.ObjectStart,      State.ObjectStart,      State.ObjectStart,      State.ObjectStart,      State.Error, State.Error },
            /* StartArray                  */new[] { State.ArrayStart,       State.ArrayStart,       State.Error,        State.Error,      State.ArrayStart,       State.ArrayStart,       State.ArrayStart,       State.ArrayStart,       State.Error, State.Error },
            /* StartConstructor            */new[] { State.ConstructorStart, State.ConstructorStart, State.Error,        State.Error,      State.ConstructorStart, State.ConstructorStart, State.ConstructorStart, State.ConstructorStart, State.Error, State.Error },
            /* Property                    */new[] { State.Property,         State.Error,            State.Property,     State.Property,   State.Error,            State.Error,            State.Error,            State.Error,            State.Error, State.Error },
            /* Comment                     */new[] { State.Start,            State.Property,         State.ObjectStart,  State.Object,     State.ArrayStart,       State.Array,            State.Constructor,      State.Constructor,      State.Error, State.Error },
            /* Raw                         */new[] { State.Start,            State.Property,         State.ObjectStart,  State.Object,     State.ArrayStart,       State.Array,            State.Constructor,      State.Constructor,      State.Error, State.Error },
            /* Value (this will be copied) */new[] { State.Start,            State.Object,           State.Error,        State.Error,      State.Array,            State.Array,            State.Constructor,      State.Constructor,      State.Error, State.Error }
        };

        internal static State[][] BuildStateArray()
        {
            List<State[]> allStates = StateArrayTempate.ToList();
            State[] errorStates = StateArrayTempate[0];
            State[] valueStates = StateArrayTempate[7];

            EnumInfo enumValuesAndNames = EnumUtils.GetEnumValuesAndNames(typeof(JsonToken));

            foreach (ulong valueToken in enumValuesAndNames.Values)
            {
                if (allStates.Count <= (int)valueToken)
                {
                    JsonToken token = (JsonToken)valueToken;
                    switch (token)
                    {
                        case JsonToken.Integer:
                        case JsonToken.Float:
                        case JsonToken.String:
                        case JsonToken.Boolean:
                        case JsonToken.Null:
                        case JsonToken.Undefined:
                        case JsonToken.Date:
                        case JsonToken.Bytes:
                            allStates.Add(valueStates);
                            break;

                        default:
                            allStates.Add(errorStates);
                            break;
                    }
                }
            }

            return allStates.ToArray();
        }

        static JsonWriter()
        {
            StateArray = BuildStateArray();
        }

        private List<JsonPosition> _stack;
        private JsonPosition _currentPosition;
        private State _currentState;
        private Formatting _formatting;

        /// <summary>
        /// Gets or sets a value indicating whether the destination should be closed when this writer is closed.
        /// </summary>
        /// <value>
        /// <c>true</c> to close the destination when this writer is closed; otherwise <c>false</c>. The default is <c>true</c>.
        /// </value>
        public bool CloseOutput { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the JSON should be auto-completed when this writer is closed.
        /// </summary>
        /// <value>
        /// <c>true</c> to auto-complete the JSON when this writer is closed; otherwise <c>false</c>. The default is <c>true</c>.
        /// </value>
        public bool AutoCompleteOnClose { get; set; }

        /// <summary>
        /// Gets the top.
        /// </summary>
        /// <value>The top.</value>
        protected internal int Top
        {
            get
            {
                int depth = this._stack?.Count ?? 0;
                if (this.Peek() != JsonContainerType.None)
                {
                    depth++;
                }

                return depth;
            }
        }

        /// <summary>
        /// Gets the state of the writer.
        /// </summary>
        public WriteState WriteState
        {
            get
            {
                switch (this._currentState)
                {
                    case State.Error:
                        return WriteState.Error;

                    case State.Closed:
                        return WriteState.Closed;

                    case State.Object:
                    case State.ObjectStart:
                        return WriteState.Object;

                    case State.Array:
                    case State.ArrayStart:
                        return WriteState.Array;

                    case State.Constructor:
                    case State.ConstructorStart:
                        return WriteState.Constructor;

                    case State.Property:
                        return WriteState.Property;

                    case State.Start:
                        return WriteState.Start;

                    default:
                        throw JsonWriterException.Create(this, "Invalid state: " + this._currentState, null);
                }
            }
        }

        internal string ContainerPath
        {
            get
            {
                if (this._currentPosition.Type == JsonContainerType.None || this._stack == null)
                {
                    return string.Empty;
                }

                return JsonPosition.BuildPath(this._stack, null);
            }
        }

        /// <summary>
        /// Gets the path of the writer.
        /// </summary>
        public string Path
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

        private DateFormatHandling _dateFormatHandling;
        private DateTimeZoneHandling _dateTimeZoneHandling;
        private StringEscapeHandling _stringEscapeHandling;
        private FloatFormatHandling _floatFormatHandling;
        private string _dateFormatString;
        private CultureInfo _culture;

        /// <summary>
        /// Gets or sets a value indicating how JSON text output should be formatted.
        /// </summary>
        public Formatting Formatting
        {
            get => this._formatting;
            set
            {
                if (value < Formatting.None || value > Formatting.Indented)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this._formatting = value;
            }
        }

        /// <summary>
        /// Gets or sets how dates are written to JSON text.
        /// </summary>
        public DateFormatHandling DateFormatHandling
        {
            get => this._dateFormatHandling;
            set
            {
                if (value < DateFormatHandling.IsoDateFormat || value > DateFormatHandling.MicrosoftDateFormat)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this._dateFormatHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how <see cref="DateTime"/> time zones are handled when writing JSON text.
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
        /// Gets or sets how strings are escaped when writing JSON text.
        /// </summary>
        public StringEscapeHandling StringEscapeHandling
        {
            get => this._stringEscapeHandling;
            set
            {
                if (value < StringEscapeHandling.Default || value > StringEscapeHandling.EscapeHtml)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this._stringEscapeHandling = value;
                this.OnStringEscapeHandlingChanged();
            }
        }

        internal virtual void OnStringEscapeHandlingChanged()
        {
            // hacky but there is a calculated value that relies on StringEscapeHandling
        }

        /// <summary>
        /// Gets or sets how special floating point numbers, e.g. <see cref="Double.NaN"/>,
        /// <see cref="Double.PositiveInfinity"/> and <see cref="Double.NegativeInfinity"/>,
        /// are written to JSON text.
        /// </summary>
        public FloatFormatHandling FloatFormatHandling
        {
            get => this._floatFormatHandling;
            set
            {
                if (value < FloatFormatHandling.String || value > FloatFormatHandling.DefaultValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this._floatFormatHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how <see cref="DateTime"/> and <see cref="DateTimeOffset"/> values are formatted when writing JSON text.
        /// </summary>
        public string DateFormatString
        {
            get => this._dateFormatString;
            set => this._dateFormatString = value;
        }

        /// <summary>
        /// Gets or sets the culture used when writing JSON. Defaults to <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        public CultureInfo Culture
        {
            get => this._culture ?? CultureInfo.InvariantCulture;
            set => this._culture = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonWriter"/> class.
        /// </summary>
        protected JsonWriter()
        {
            this._currentState = State.Start;
            this._formatting = Formatting.None;
            this._dateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;

            this.CloseOutput = true;
            this.AutoCompleteOnClose = true;
        }

        internal void UpdateScopeWithFinishedValue()
        {
            if (this._currentPosition.HasIndex)
            {
                this._currentPosition.Position++;
            }
        }

        private void Push(JsonContainerType value)
        {
            if (this._currentPosition.Type != JsonContainerType.None)
            {
                if (this._stack == null)
                {
                    this._stack = new List<JsonPosition>();
                }

                this._stack.Add(this._currentPosition);
            }

            this._currentPosition = new JsonPosition(value);
        }

        private JsonContainerType Pop()
        {
            JsonPosition oldPosition = this._currentPosition;

            if (this._stack != null && this._stack.Count > 0)
            {
                this._currentPosition = this._stack[this._stack.Count - 1];
                this._stack.RemoveAt(this._stack.Count - 1);
            }
            else
            {
                this._currentPosition = new JsonPosition();
            }

            return oldPosition.Type;
        }

        private JsonContainerType Peek()
        {
            return this._currentPosition.Type;
        }

        /// <summary>
        /// Flushes whatever is in the buffer to the destination and also flushes the destination.
        /// </summary>
        public abstract void Flush();

        /// <summary>
        /// Closes this writer.
        /// If <see cref="CloseOutput"/> is set to <c>true</c>, the destination is also closed.
        /// If <see cref="AutoCompleteOnClose"/> is set to <c>true</c>, the JSON is auto-completed.
        /// </summary>
        public virtual void Close()
        {
            if (this.AutoCompleteOnClose)
            {
                this.AutoCompleteAll();
            }
        }

        /// <summary>
        /// Writes the beginning of a JSON object.
        /// </summary>
        public virtual void WriteStartObject()
        {
            this.InternalWriteStart(JsonToken.StartObject, JsonContainerType.Object);
        }

        /// <summary>
        /// Writes the end of a JSON object.
        /// </summary>
        public virtual void WriteEndObject()
        {
            this.InternalWriteEnd(JsonContainerType.Object);
        }

        /// <summary>
        /// Writes the beginning of a JSON array.
        /// </summary>
        public virtual void WriteStartArray()
        {
            this.InternalWriteStart(JsonToken.StartArray, JsonContainerType.Array);
        }

        /// <summary>
        /// Writes the end of an array.
        /// </summary>
        public virtual void WriteEndArray()
        {
            this.InternalWriteEnd(JsonContainerType.Array);
        }

        /// <summary>
        /// Writes the start of a constructor with the given name.
        /// </summary>
        /// <param name="name">The name of the constructor.</param>
        public virtual void WriteStartConstructor(string name)
        {
            this.InternalWriteStart(JsonToken.StartConstructor, JsonContainerType.Constructor);
        }

        /// <summary>
        /// Writes the end constructor.
        /// </summary>
        public virtual void WriteEndConstructor()
        {
            this.InternalWriteEnd(JsonContainerType.Constructor);
        }

        /// <summary>
        /// Writes the property name of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        public virtual void WritePropertyName(string name)
        {
            this.InternalWritePropertyName(name);
        }

        /// <summary>
        /// Writes the property name of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="escape">A flag to indicate whether the text should be escaped when it is written as a JSON property name.</param>
        public virtual void WritePropertyName(string name, bool escape)
        {
            this.WritePropertyName(name);
        }

        /// <summary>
        /// Writes the end of the current JSON object or array.
        /// </summary>
        public virtual void WriteEnd()
        {
            this.WriteEnd(this.Peek());
        }

        /// <summary>
        /// Writes the current <see cref="JsonReader"/> token and its children.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read the token from.</param>
        public void WriteToken(JsonReader reader)
        {
            this.WriteToken(reader, true);
        }

        /// <summary>
        /// Writes the current <see cref="JsonReader"/> token.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read the token from.</param>
        /// <param name="writeChildren">A flag indicating whether the current token's children should be written.</param>
        public void WriteToken(JsonReader reader, bool writeChildren)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            this.WriteToken(reader, writeChildren, true, true);
        }

        /// <summary>
        /// Writes the <see cref="JsonToken"/> token and its value.
        /// </summary>
        /// <param name="token">The <see cref="JsonToken"/> to write.</param>
        /// <param name="value">
        /// The value to write.
        /// A value is only required for tokens that have an associated value, e.g. the <see cref="String"/> property name for <see cref="JsonToken.PropertyName"/>.
        /// <c>null</c> can be passed to the method for tokens that don't have a value, e.g. <see cref="JsonToken.StartObject"/>.
        /// </param>
        public void WriteToken(JsonToken token, object value)
        {
            switch (token)
            {
                case JsonToken.None:
                    // read to next
                    break;

                case JsonToken.StartObject:
                    this.WriteStartObject();
                    break;

                case JsonToken.StartArray:
                    this.WriteStartArray();
                    break;

                case JsonToken.StartConstructor:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    this.WriteStartConstructor(value.ToString());
                    break;

                case JsonToken.PropertyName:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    this.WritePropertyName(value.ToString());
                    break;

                case JsonToken.Comment:
                    this.WriteComment(value?.ToString());
                    break;

                case JsonToken.Integer:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
#if HAVE_BIG_INTEGER
                    if (value is BigInteger integer)
                    {
                        WriteValue(integer);
                    }
                    else
#endif
                    {
                        this.WriteValue(Convert.ToInt64(value, CultureInfo.InvariantCulture));
                    }
                    break;

                case JsonToken.Float:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    if (value is decimal d)
                    {
                        this.WriteValue(d);
                    }
                    else if (value is double)
                    {
                        this.WriteValue((double)value);
                    }
                    else if (value is float)
                    {
                        this.WriteValue((float)value);
                    }
                    else
                    {
                        this.WriteValue(Convert.ToDouble(value, CultureInfo.InvariantCulture));
                    }
                    break;

                case JsonToken.String:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    this.WriteValue(value.ToString());
                    break;

                case JsonToken.Boolean:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    this.WriteValue(Convert.ToBoolean(value, CultureInfo.InvariantCulture));
                    break;

                case JsonToken.Null:
                    this.WriteNull();
                    break;

                case JsonToken.Undefined:
                    this.WriteUndefined();
                    break;

                case JsonToken.EndObject:
                    this.WriteEndObject();
                    break;

                case JsonToken.EndArray:
                    this.WriteEndArray();
                    break;

                case JsonToken.EndConstructor:
                    this.WriteEndConstructor();
                    break;

                case JsonToken.Date:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
#if HAVE_DATE_TIME_OFFSET
                    if (value is DateTimeOffset dt)
                    {
                        WriteValue(dt);
                    }
                    else
#endif
                    {
                        this.WriteValue(Convert.ToDateTime(value, CultureInfo.InvariantCulture));
                    }
                    break;

                case JsonToken.Raw:
                    this.WriteRawValue(value?.ToString());
                    break;

                case JsonToken.Bytes:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    if (value is Guid guid)
                    {
                        this.WriteValue(guid);
                    }
                    else
                    {
                        this.WriteValue((byte[])value);
                    }
                    break;

                default:
                    throw MiscellaneousUtils.CreateArgumentOutOfRangeException(nameof(token), token, "Unexpected token type.");
            }
        }

        /// <summary>
        /// Writes the <see cref="JsonToken"/> token.
        /// </summary>
        /// <param name="token">The <see cref="JsonToken"/> to write.</param>
        public void WriteToken(JsonToken token)
        {
            this.WriteToken(token, null);
        }

        internal virtual void WriteToken(JsonReader reader, bool writeChildren, bool writeDateConstructorAsDate, bool writeComments)
        {
            int initialDepth = this.CalculateWriteTokenInitialDepth(reader);

            do
            {
                // write a JValue date when the constructor is for a date
                if (writeDateConstructorAsDate && reader.TokenType == JsonToken.StartConstructor && string.Equals(reader.Value.ToString(), "Date", StringComparison.Ordinal))
                {
                    this.WriteConstructorDate(reader);
                }
                else
                {
                    if (writeComments || reader.TokenType != JsonToken.Comment)
                    {
                        this.WriteToken(reader.TokenType, reader.Value);
                    }
                }
            } while (
                // stop if we have reached the end of the token being read
                initialDepth - 1 < reader.Depth - (JsonTokenUtils.IsEndToken(reader.TokenType) ? 1 : 0)
                && writeChildren
                && reader.Read());

            if (initialDepth < this.CalculateWriteTokenFinalDepth(reader))
            {
                throw JsonWriterException.Create(this, "Unexpected end when reading token.", null);
            }
        }

        private int CalculateWriteTokenInitialDepth(JsonReader reader)
        {
            JsonToken type = reader.TokenType;
            if (type == JsonToken.None)
            {
                return -1;
            }

            return JsonTokenUtils.IsStartToken(type) ? reader.Depth : reader.Depth + 1;
        }

        private int CalculateWriteTokenFinalDepth(JsonReader reader)
        {
            JsonToken type = reader.TokenType;
            if (type == JsonToken.None)
            {
                return -1;
            }

            return JsonTokenUtils.IsEndToken(type) ? reader.Depth - 1 : reader.Depth;
        }

        private void WriteConstructorDate(JsonReader reader)
        {
            if (!reader.Read())
            {
                throw JsonWriterException.Create(this, "Unexpected end when reading date constructor.", null);
            }
            if (reader.TokenType != JsonToken.Integer)
            {
                throw JsonWriterException.Create(this, "Unexpected token when reading date constructor. Expected Integer, got " + reader.TokenType, null);
            }

            long ticks = (long)reader.Value;
            DateTime date = DateTimeUtils.ConvertJavaScriptTicksToDateTime(ticks);

            if (!reader.Read())
            {
                throw JsonWriterException.Create(this, "Unexpected end when reading date constructor.", null);
            }
            if (reader.TokenType != JsonToken.EndConstructor)
            {
                throw JsonWriterException.Create(this, "Unexpected token when reading date constructor. Expected EndConstructor, got " + reader.TokenType, null);
            }

            this.WriteValue(date);
        }

        private void WriteEnd(JsonContainerType type)
        {
            switch (type)
            {
                case JsonContainerType.Object:
                    this.WriteEndObject();
                    break;

                case JsonContainerType.Array:
                    this.WriteEndArray();
                    break;

                case JsonContainerType.Constructor:
                    this.WriteEndConstructor();
                    break;

                default:
                    throw JsonWriterException.Create(this, "Unexpected type when writing end: " + type, null);
            }
        }

        private void AutoCompleteAll()
        {
            while (this.Top > 0)
            {
                this.WriteEnd();
            }
        }

        private JsonToken GetCloseTokenForType(JsonContainerType type)
        {
            switch (type)
            {
                case JsonContainerType.Object:
                    return JsonToken.EndObject;

                case JsonContainerType.Array:
                    return JsonToken.EndArray;

                case JsonContainerType.Constructor:
                    return JsonToken.EndConstructor;

                default:
                    throw JsonWriterException.Create(this, "No close token for type: " + type, null);
            }
        }

        private void AutoCompleteClose(JsonContainerType type)
        {
            int levelsToComplete = this.CalculateLevelsToComplete(type);

            for (int i = 0; i < levelsToComplete; i++)
            {
                JsonToken token = this.GetCloseTokenForType(this.Pop());

                if (this._currentState == State.Property)
                {
                    this.WriteNull();
                }

                if (this._formatting == Formatting.Indented)
                {
                    if (this._currentState != State.ObjectStart && this._currentState != State.ArrayStart)
                    {
                        this.WriteIndent();
                    }
                }

                this.WriteEnd(token);

                this.UpdateCurrentState();
            }
        }

        private int CalculateLevelsToComplete(JsonContainerType type)
        {
            int levelsToComplete = 0;

            if (this._currentPosition.Type == type)
            {
                levelsToComplete = 1;
            }
            else
            {
                int top = this.Top - 2;
                for (int i = top; i >= 0; i--)
                {
                    int currentLevel = top - i;

                    if (this._stack[currentLevel].Type == type)
                    {
                        levelsToComplete = i + 2;
                        break;
                    }
                }
            }

            if (levelsToComplete == 0)
            {
                throw JsonWriterException.Create(this, "No token to close.", null);
            }

            return levelsToComplete;
        }

        private void UpdateCurrentState()
        {
            JsonContainerType currentLevelType = this.Peek();

            switch (currentLevelType)
            {
                case JsonContainerType.Object:
                    this._currentState = State.Object;
                    break;

                case JsonContainerType.Array:
                    this._currentState = State.Array;
                    break;

                case JsonContainerType.Constructor:
                    this._currentState = State.Array;
                    break;

                case JsonContainerType.None:
                    this._currentState = State.Start;
                    break;

                default:
                    throw JsonWriterException.Create(this, "Unknown JsonType: " + currentLevelType, null);
            }
        }

        /// <summary>
        /// Writes the specified end token.
        /// </summary>
        /// <param name="token">The end token to write.</param>
        protected virtual void WriteEnd(JsonToken token)
        {
        }

        /// <summary>
        /// Writes indent characters.
        /// </summary>
        protected virtual void WriteIndent()
        {
        }

        /// <summary>
        /// Writes the JSON value delimiter.
        /// </summary>
        protected virtual void WriteValueDelimiter()
        {
        }

        /// <summary>
        /// Writes an indent space.
        /// </summary>
        protected virtual void WriteIndentSpace()
        {
        }

        internal void AutoComplete(JsonToken tokenBeingWritten)
        {
            // gets new state based on the current state and what is being written
            State newState = StateArray[(int)tokenBeingWritten][(int)this._currentState];

            if (newState == State.Error)
            {
                throw JsonWriterException.Create(this, "Token {0} in state {1} would result in an invalid JSON object.".FormatWith(CultureInfo.InvariantCulture, tokenBeingWritten.ToString(), this._currentState.ToString()), null);
            }

            if ((this._currentState == State.Object || this._currentState == State.Array || this._currentState == State.Constructor) && tokenBeingWritten != JsonToken.Comment)
            {
                this.WriteValueDelimiter();
            }

            if (this._formatting == Formatting.Indented)
            {
                if (this._currentState == State.Property)
                {
                    this.WriteIndentSpace();
                }

                // don't indent a property when it is the first token to be written (i.e. at the start)
                if ((this._currentState == State.Array || this._currentState == State.ArrayStart || this._currentState == State.Constructor || this._currentState == State.ConstructorStart)
                    || (tokenBeingWritten == JsonToken.PropertyName && this._currentState != State.Start))
                {
                    this.WriteIndent();
                }
            }

            this._currentState = newState;
        }

        #region WriteValue methods

        /// <summary>
        /// Writes a null value.
        /// </summary>
        public virtual void WriteNull()
        {
            this.InternalWriteValue(JsonToken.Null);
        }

        /// <summary>
        /// Writes an undefined value.
        /// </summary>
        public virtual void WriteUndefined()
        {
            this.InternalWriteValue(JsonToken.Undefined);
        }

        /// <summary>
        /// Writes raw JSON without changing the writer's state.
        /// </summary>
        /// <param name="json">The raw JSON to write.</param>
        public virtual void WriteRaw(string json)
        {
            this.InternalWriteRaw();
        }

        /// <summary>
        /// Writes raw JSON where a value is expected and updates the writer's state.
        /// </summary>
        /// <param name="json">The raw JSON to write.</param>
        public virtual void WriteRawValue(string json)
        {
            // hack. want writer to change state as if a value had been written
            this.UpdateScopeWithFinishedValue();
            this.AutoComplete(JsonToken.Undefined);
            this.WriteRaw(json);
        }

        /// <summary>
        /// Writes a <see cref="String"/> value.
        /// </summary>
        /// <param name="value">The <see cref="String"/> value to write.</param>
        public virtual void WriteValue(string value)
        {
            this.InternalWriteValue(JsonToken.String);
        }

        /// <summary>
        /// Writes a <see cref="Int32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int32"/> value to write.</param>
        public virtual void WriteValue(int value)
        {
            this.InternalWriteValue(JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="UInt32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt32"/> value to write.</param>

        public virtual void WriteValue(uint value)
        {
            this.InternalWriteValue(JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="Int64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int64"/> value to write.</param>
        public virtual void WriteValue(long value)
        {
            this.InternalWriteValue(JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="UInt64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt64"/> value to write.</param>
        public virtual void WriteValue(ulong value)
        {
            this.InternalWriteValue(JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="Single"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Single"/> value to write.</param>
        public virtual void WriteValue(float value)
        {
            this.InternalWriteValue(JsonToken.Float);
        }

        /// <summary>
        /// Writes a <see cref="Double"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Double"/> value to write.</param>
        public virtual void WriteValue(double value)
        {
            this.InternalWriteValue(JsonToken.Float);
        }

        /// <summary>
        /// Writes a <see cref="Boolean"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Boolean"/> value to write.</param>
        public virtual void WriteValue(bool value)
        {
            this.InternalWriteValue(JsonToken.Boolean);
        }

        /// <summary>
        /// Writes a <see cref="Int16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int16"/> value to write.</param>
        public virtual void WriteValue(short value)
        {
            this.InternalWriteValue(JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="UInt16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt16"/> value to write.</param>
        public virtual void WriteValue(ushort value)
        {
            this.InternalWriteValue(JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="Char"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Char"/> value to write.</param>
        public virtual void WriteValue(char value)
        {
            this.InternalWriteValue(JsonToken.String);
        }

        /// <summary>
        /// Writes a <see cref="Byte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Byte"/> value to write.</param>
        public virtual void WriteValue(byte value)
        {
            this.InternalWriteValue(JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="SByte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="SByte"/> value to write.</param>
        public virtual void WriteValue(sbyte value)
        {
            this.InternalWriteValue(JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="Decimal"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Decimal"/> value to write.</param>
        public virtual void WriteValue(decimal value)
        {
            this.InternalWriteValue(JsonToken.Float);
        }

        /// <summary>
        /// Writes a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> value to write.</param>
        public virtual void WriteValue(DateTime value)
        {
            this.InternalWriteValue(JsonToken.Date);
        }

#if HAVE_DATE_TIME_OFFSET
        /// <summary>
        /// Writes a <see cref="DateTimeOffset"/> value.
        /// </summary>
        /// <param name="value">The <see cref="DateTimeOffset"/> value to write.</param>
        public virtual void WriteValue(DateTimeOffset value)
        {
            InternalWriteValue(JsonToken.Date);
        }
#endif

        /// <summary>
        /// Writes a <see cref="Guid"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Guid"/> value to write.</param>
        public virtual void WriteValue(Guid value)
        {
            this.InternalWriteValue(JsonToken.String);
        }

        /// <summary>
        /// Writes a <see cref="TimeSpan"/> value.
        /// </summary>
        /// <param name="value">The <see cref="TimeSpan"/> value to write.</param>
        public virtual void WriteValue(TimeSpan value)
        {
            this.InternalWriteValue(JsonToken.String);
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="Int32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Int32"/> value to write.</param>
        public virtual void WriteValue(int? value)
        {
            if (value == null)
            {
                this.WriteNull();
            }
            else
            {
                this.WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="UInt32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="UInt32"/> value to write.</param>

        public virtual void WriteValue(uint? value)
        {
            if (value == null)
            {
                this.WriteNull();
            }
            else
            {
                this.WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="Int64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Int64"/> value to write.</param>
        public virtual void WriteValue(long? value)
        {
            if (value == null)
            {
                this.WriteNull();
            }
            else
            {
                this.WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="UInt64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="UInt64"/> value to write.</param>

        public virtual void WriteValue(ulong? value)
        {
            if (value == null)
            {
                this.WriteNull();
            }
            else
            {
                this.WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="Single"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Single"/> value to write.</param>
        public virtual void WriteValue(float? value)
        {
            if (value == null)
            {
                this.WriteNull();
            }
            else
            {
                this.WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="Double"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Double"/> value to write.</param>
        public virtual void WriteValue(double? value)
        {
            if (value == null)
            {
                this.WriteNull();
            }
            else
            {
                this.WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="Boolean"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Boolean"/> value to write.</param>
        public virtual void WriteValue(bool? value)
        {
            if (value == null)
            {
                this.WriteNull();
            }
            else
            {
                this.WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="Int16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Int16"/> value to write.</param>
        public virtual void WriteValue(short? value)
        {
            if (value == null)
            {
                this.WriteNull();
            }
            else
            {
                this.WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="UInt16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="UInt16"/> value to write.</param>

        public virtual void WriteValue(ushort? value)
        {
            if (value == null)
            {
                this.WriteNull();
            }
            else
            {
                this.WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="Char"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Char"/> value to write.</param>
        public virtual void WriteValue(char? value)
        {
            if (value == null)
            {
                this.WriteNull();
            }
            else
            {
                this.WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="Byte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Byte"/> value to write.</param>
        public virtual void WriteValue(byte? value)
        {
            if (value == null)
            {
                this.WriteNull();
            }
            else
            {
                this.WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="SByte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="SByte"/> value to write.</param>

        public virtual void WriteValue(sbyte? value)
        {
            if (value == null)
            {
                this.WriteNull();
            }
            else
            {
                this.WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="Decimal"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Decimal"/> value to write.</param>
        public virtual void WriteValue(decimal? value)
        {
            if (value == null)
            {
                this.WriteNull();
            }
            else
            {
                this.WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="DateTime"/> value to write.</param>
        public virtual void WriteValue(DateTime? value)
        {
            if (value == null)
            {
                this.WriteNull();
            }
            else
            {
                this.WriteValue(value.GetValueOrDefault());
            }
        }

#if HAVE_DATE_TIME_OFFSET
        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/> value to write.</param>
        public virtual void WriteValue(DateTimeOffset? value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                WriteValue(value.GetValueOrDefault());
            }
        }
#endif

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="Guid"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Guid"/> value to write.</param>
        public virtual void WriteValue(Guid? value)
        {
            if (value == null)
            {
                this.WriteNull();
            }
            else
            {
                this.WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="TimeSpan"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="TimeSpan"/> value to write.</param>
        public virtual void WriteValue(TimeSpan? value)
        {
            if (value == null)
            {
                this.WriteNull();
            }
            else
            {
                this.WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Byte"/>[] value.
        /// </summary>
        /// <param name="value">The <see cref="Byte"/>[] value to write.</param>
        public virtual void WriteValue(byte[] value)
        {
            if (value == null)
            {
                this.WriteNull();
            }
            else
            {
                this.InternalWriteValue(JsonToken.Bytes);
            }
        }

        /// <summary>
        /// Writes a <see cref="Uri"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Uri"/> value to write.</param>
        public virtual void WriteValue(Uri value)
        {
            if (value == null)
            {
                this.WriteNull();
            }
            else
            {
                this.InternalWriteValue(JsonToken.String);
            }
        }

        /// <summary>
        /// Writes a <see cref="Object"/> value.
        /// An error will raised if the value cannot be written as a single JSON token.
        /// </summary>
        /// <param name="value">The <see cref="Object"/> value to write.</param>
        public virtual void WriteValue(object value)
        {
            if (value == null)
            {
                this.WriteNull();
            }
            else
            {
#if HAVE_BIG_INTEGER
                // this is here because adding a WriteValue(BigInteger) to JsonWriter will
                // mean the user has to add a reference to System.Numerics.dll
                if (value is BigInteger)
                {
                    throw CreateUnsupportedTypeException(this, value);
                }
#endif

                WriteValue(this, ConvertUtils.GetTypeCode(value.GetType()), value);
            }
        }

        #endregion WriteValue methods

        /// <summary>
        /// Writes a comment <c>/*...*/</c> containing the specified text.
        /// </summary>
        /// <param name="text">Text to place inside the comment.</param>
        public virtual void WriteComment(string text)
        {
            this.InternalWriteComment();
        }

        /// <summary>
        /// Writes the given white space.
        /// </summary>
        /// <param name="ws">The string of white space characters.</param>
        public virtual void WriteWhitespace(string ws)
        {
            this.InternalWriteWhitespace(ws);
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

        internal static void WriteValue(JsonWriter writer, PrimitiveTypeCode typeCode, object value)
        {
            while (true)
            {
                switch (typeCode)
                {
                    case PrimitiveTypeCode.Char:
                        writer.WriteValue((char)value);
                        return;

                    case PrimitiveTypeCode.CharNullable:
                        if (value == null)
                        {
                            writer.WriteValue((char?)value);
                        }
                        else
                        {
                            writer.WriteValue((char)value);
                        }

                        return;

                    case PrimitiveTypeCode.Boolean:
                        writer.WriteValue((bool)value);
                        return;

                    case PrimitiveTypeCode.BooleanNullable:
                        if (value == null)
                        {
                            writer.WriteValue((bool?)value);
                        }
                        else
                        {
                            writer.WriteValue((bool)value);
                        }

                        return;

                    case PrimitiveTypeCode.SByte:
                        writer.WriteValue((sbyte)value);
                        return;

                    case PrimitiveTypeCode.SByteNullable:
                        if ((value == null))
                        {
                            writer.WriteValue((sbyte?)value);
                        }
                        else
                        {
                            writer.WriteValue((sbyte)value);
                        }

                        return;

                    case PrimitiveTypeCode.Int16:
                        writer.WriteValue((short)value);
                        return;

                    case PrimitiveTypeCode.Int16Nullable:
                        if ((value == null))
                        {
                            writer.WriteValue((short?)value);
                        }
                        else
                        {
                            writer.WriteValue((short)value);
                        }

                        return;

                    case PrimitiveTypeCode.UInt16:
                        writer.WriteValue((ushort)value);
                        return;

                    case PrimitiveTypeCode.UInt16Nullable:
                        if (value == null)
                        {
                            writer.WriteValue((ushort?)value);
                        }
                        else
                        {
                            writer.WriteValue((ushort)value);
                        }
                        return;

                    case PrimitiveTypeCode.Int32:
                        writer.WriteValue((int)value);
                        return;

                    case PrimitiveTypeCode.Int32Nullable:
                        if (value == null)
                        {
                            writer.WriteValue((int?)value);
                        }
                        else
                        {
                            writer.WriteValue((int)value);
                        }
                        return;

                    case PrimitiveTypeCode.Byte:
                        writer.WriteValue((byte)value);
                        return;

                    case PrimitiveTypeCode.ByteNullable:
                        if (value == null)
                        {
                            writer.WriteValue((byte?)value);
                        }
                        else
                        {
                            writer.WriteValue((byte)value);
                        }
                        return;

                    case PrimitiveTypeCode.UInt32:
                        writer.WriteValue((uint)value);
                        return;

                    case PrimitiveTypeCode.UInt32Nullable:
                        if (value == null)
                        {
                            writer.WriteValue((uint?)value);
                        }
                        else
                        {
                            writer.WriteValue((uint)value);
                        }
                        return;

                    case PrimitiveTypeCode.Int64:
                        writer.WriteValue((long)value);
                        return;

                    case PrimitiveTypeCode.Int64Nullable:
                        if (value == null)
                        {
                            writer.WriteValue((long?)value);
                        }
                        else
                        {
                            writer.WriteValue((long)value);
                        }
                        return;

                    case PrimitiveTypeCode.UInt64:
                        writer.WriteValue((ulong)value);
                        return;

                    case PrimitiveTypeCode.UInt64Nullable:
                        if (value == null)
                        {
                            writer.WriteValue((ulong?)value);
                        }
                        else
                        {
                            writer.WriteValue((ulong)value);
                        }
                        return;

                    case PrimitiveTypeCode.Single:
                        writer.WriteValue((float)value);
                        return;

                    case PrimitiveTypeCode.SingleNullable:
                        if (value == null)
                        {
                            writer.WriteValue((float?)value);
                        }
                        else
                        {
                            writer.WriteValue((float)value);
                        }
                        return;

                    case PrimitiveTypeCode.Double:
                        writer.WriteValue((double)value);
                        return;

                    case PrimitiveTypeCode.DoubleNullable:
                        if (value == null)
                        {
                            writer.WriteValue((double?)value);
                        }
                        else
                        {
                            writer.WriteValue((double)value);
                        }
                        return;

                    case PrimitiveTypeCode.DateTime:
                        writer.WriteValue((DateTime)value);
                        return;

                    case PrimitiveTypeCode.DateTimeNullable:
                        if (value == null)
                        {
                            writer.WriteValue((DateTime?)value);
                        }
                        else
                        {
                            writer.WriteValue((DateTime)value);
                        }
                        return;

#if HAVE_DATE_TIME_OFFSET
                    case PrimitiveTypeCode.DateTimeOffset:
                        writer.WriteValue((DateTimeOffset)value);
                        return;

                    case PrimitiveTypeCode.DateTimeOffsetNullable:
                        writer.WriteValue((value == null) ? (DateTimeOffset?)null : (DateTimeOffset)value);
                        return;
#endif
                    case PrimitiveTypeCode.Decimal:
                        writer.WriteValue((decimal)value);
                        return;

                    case PrimitiveTypeCode.DecimalNullable:
                        if (value == null)
                        {
                            writer.WriteValue((decimal?)value);
                        }
                        else
                        {
                            writer.WriteValue((decimal)value);
                        }
                        return;

                    case PrimitiveTypeCode.Guid:
                        writer.WriteValue((Guid)value);
                        return;

                    case PrimitiveTypeCode.GuidNullable:
                        if (value == null)
                        {
                            writer.WriteValue((Guid?)value);
                        }
                        else
                        {
                            writer.WriteValue((Guid)value);
                        }
                        return;

                    case PrimitiveTypeCode.TimeSpan:
                        writer.WriteValue((TimeSpan)value);
                        return;

                    case PrimitiveTypeCode.TimeSpanNullable:
                        if (value == null)
                        {
                            writer.WriteValue((TimeSpan?)value);
                        }
                        else
                        {
                            writer.WriteValue((TimeSpan)value);
                        }
                        return;

#if HAVE_BIG_INTEGER
                    case PrimitiveTypeCode.BigInteger:
                        // this will call to WriteValue(object)
                        writer.WriteValue((BigInteger)value);
                        return;

                    case PrimitiveTypeCode.BigIntegerNullable:
                        // this will call to WriteValue(object)
                        writer.WriteValue((value == null) ? (BigInteger?)null : (BigInteger)value);
                        return;
#endif
                    case PrimitiveTypeCode.Uri:
                        writer.WriteValue((Uri)value);
                        return;

                    case PrimitiveTypeCode.String:
                        writer.WriteValue((string)value);
                        return;

                    case PrimitiveTypeCode.Bytes:
                        writer.WriteValue((byte[])value);
                        return;

#if HAVE_DB_NULL_TYPE_CODE
                    case PrimitiveTypeCode.DBNull:
                        writer.WriteNull();
                        return;
#endif
                    default:
#if HAVE_ICONVERTIBLE
                        if (value is IConvertible convertible)
                        {
                            ResolveConvertibleValue(convertible, out typeCode, out value);
                            continue;
                        }
#endif

                        // write an unknown null value, fix https://github.com/JamesNK/TouchSocket.Core.XREF.Newtonsoft.Json/issues/1460
                        if (value == null)
                        {
                            writer.WriteNull();
                            return;
                        }

                        throw CreateUnsupportedTypeException(writer, value);
                }
            }
        }

#if HAVE_ICONVERTIBLE
        private static void ResolveConvertibleValue(IConvertible convertible, out PrimitiveTypeCode typeCode, out object value)
        {
            // the value is a non-standard IConvertible
            // convert to the underlying value and retry
            TypeInformation typeInformation = ConvertUtils.GetTypeInformation(convertible);

            // if convertible has an underlying typecode of Object then attempt to convert it to a string
            typeCode = typeInformation.TypeCode == PrimitiveTypeCode.Object ? PrimitiveTypeCode.String : typeInformation.TypeCode;
            Type resolvedType = typeInformation.TypeCode == PrimitiveTypeCode.Object ? typeof(string) : typeInformation.Type;
            value = convertible.ToType(resolvedType, CultureInfo.InvariantCulture);
        }
#endif

        private static JsonWriterException CreateUnsupportedTypeException(JsonWriter writer, object value)
        {
            return JsonWriterException.Create(writer, "Unsupported type: {0}. Use the JsonSerializer class to get the object's JSON representation.".FormatWith(CultureInfo.InvariantCulture, value.GetType()), null);
        }

        /// <summary>
        /// Sets the state of the <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="token">The <see cref="JsonToken"/> being written.</param>
        /// <param name="value">The value being written.</param>
        protected void SetWriteState(JsonToken token, object value)
        {
            switch (token)
            {
                case JsonToken.StartObject:
                    this.InternalWriteStart(token, JsonContainerType.Object);
                    break;

                case JsonToken.StartArray:
                    this.InternalWriteStart(token, JsonContainerType.Array);
                    break;

                case JsonToken.StartConstructor:
                    this.InternalWriteStart(token, JsonContainerType.Constructor);
                    break;

                case JsonToken.PropertyName:
                    if (!(value is string))
                    {
                        throw new ArgumentException("A name is required when setting property name state.", nameof(value));
                    }

                    this.InternalWritePropertyName((string)value);
                    break;

                case JsonToken.Comment:
                    this.InternalWriteComment();
                    break;

                case JsonToken.Raw:
                    this.InternalWriteRaw();
                    break;

                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.String:
                case JsonToken.Boolean:
                case JsonToken.Date:
                case JsonToken.Bytes:
                case JsonToken.Null:
                case JsonToken.Undefined:
                    this.InternalWriteValue(token);
                    break;

                case JsonToken.EndObject:
                    this.InternalWriteEnd(JsonContainerType.Object);
                    break;

                case JsonToken.EndArray:
                    this.InternalWriteEnd(JsonContainerType.Array);
                    break;

                case JsonToken.EndConstructor:
                    this.InternalWriteEnd(JsonContainerType.Constructor);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(token));
            }
        }

        internal void InternalWriteEnd(JsonContainerType container)
        {
            this.AutoCompleteClose(container);
        }

        internal void InternalWritePropertyName(string name)
        {
            this._currentPosition.PropertyName = name;
            this.AutoComplete(JsonToken.PropertyName);
        }

        internal void InternalWriteRaw()
        {
        }

        internal void InternalWriteStart(JsonToken token, JsonContainerType container)
        {
            this.UpdateScopeWithFinishedValue();
            this.AutoComplete(token);
            this.Push(container);
        }

        internal void InternalWriteValue(JsonToken token)
        {
            this.UpdateScopeWithFinishedValue();
            this.AutoComplete(token);
        }

        internal void InternalWriteWhitespace(string ws)
        {
            if (ws != null)
            {
                if (!StringUtils.IsWhiteSpace(ws))
                {
                    throw JsonWriterException.Create(this, "Only white space characters should be used.", null);
                }
            }
        }

        internal void InternalWriteComment()
        {
            this.AutoComplete(JsonToken.Comment);
        }
    }
}