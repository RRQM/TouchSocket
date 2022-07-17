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
using System.IO;
using System.Globalization;

#if HAVE_BIG_INTEGER
using System.Numerics;
#endif

using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities;

namespace TouchSocket.Core.XREF.Newtonsoft.Json
{
    internal enum ReadType
    {
        Read,
        ReadAsInt32,
        ReadAsInt64,
        ReadAsBytes,
        ReadAsString,
        ReadAsDecimal,
        ReadAsDateTime,
#if HAVE_DATE_TIME_OFFSET
        ReadAsDateTimeOffset,
#endif
        ReadAsDouble,
        ReadAsBoolean
    }

    /// <summary>
    /// Represents a reader that provides fast, non-cached, forward-only access to JSON text data.
    /// </summary>
    public partial class JsonTextReader : JsonReader, IJsonLineInfo
    {
        private const char UnicodeReplacementChar = '\uFFFD';
        private const int MaximumJavascriptIntegerCharacterLength = 380;

        private const int LargeBufferLength = int.MaxValue / 2;
        private readonly TextReader _reader;
        private char[] _chars;
        private int _charsUsed;
        private int _charPos;
        private int _lineStartPos;
        private int _lineNumber;
        private bool _isEndOfFile;
        private StringBuffer _stringBuffer;
        private StringReference _stringReference;
        private IArrayPool<char> _arrayPool;
        internal PropertyNameTable NameTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTextReader"/> class with the specified <see cref="TextReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> containing the JSON data to read.</param>
        public JsonTextReader(TextReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            this._reader = reader;
            this._lineNumber = 1;

#if HAVE_ASYNC
            _safeAsync = GetType() == typeof(JsonTextReader);
#endif
        }

#if DEBUG

        internal char[] CharBuffer
        {
            get => this._chars;
            set => this._chars = value;
        }

        internal int CharPos => this._charPos;
#endif

        /// <summary>
        /// Gets or sets the reader's character buffer pool.
        /// </summary>
        public IArrayPool<char> ArrayPool
        {
            get => this._arrayPool;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                this._arrayPool = value;
            }
        }

        private void EnsureBufferNotEmpty()
        {
            if (this._stringBuffer.IsEmpty)
            {
                this._stringBuffer = new StringBuffer(this._arrayPool, 1024);
            }
        }

        private void SetNewLine(bool hasNextChar)
        {
            if (hasNextChar && this._chars[this._charPos] == StringUtils.LineFeed)
            {
                this._charPos++;
            }

            this.OnNewLine(this._charPos);
        }

        private void OnNewLine(int pos)
        {
            this._lineNumber++;
            this._lineStartPos = pos;
        }

        private void ParseString(char quote, ReadType readType)
        {
            this._charPos++;

            this.ShiftBufferIfNeeded();
            this.ReadStringIntoBuffer(quote);
            this.ParseReadString(quote, readType);
        }

        private void ParseReadString(char quote, ReadType readType)
        {
            this.SetPostValueState(true);

            switch (readType)
            {
                case ReadType.ReadAsBytes:
                    Guid g;
                    byte[] data;
                    if (this._stringReference.Length == 0)
                    {
                        data = CollectionUtils.ArrayEmpty<byte>();
                    }
                    else if (this._stringReference.Length == 36 && ConvertUtils.TryConvertGuid(this._stringReference.ToString(), out g))
                    {
                        data = g.ToByteArray();
                    }
                    else
                    {
                        data = Convert.FromBase64CharArray(this._stringReference.Chars, this._stringReference.StartIndex, this._stringReference.Length);
                    }

                    this.SetToken(JsonToken.Bytes, data, false);
                    break;

                case ReadType.ReadAsString:
                    string text = this._stringReference.ToString();

                    this.SetToken(JsonToken.String, text, false);
                    this._quoteChar = quote;
                    break;

                case ReadType.ReadAsInt32:
                case ReadType.ReadAsDecimal:
                case ReadType.ReadAsBoolean:
                    // caller will convert result
                    break;

                default:
                    if (this._dateParseHandling != DateParseHandling.None)
                    {
                        DateParseHandling dateParseHandling;
                        if (readType == ReadType.ReadAsDateTime)
                        {
                            dateParseHandling = DateParseHandling.DateTime;
                        }
#if HAVE_DATE_TIME_OFFSET
                        else if (readType == ReadType.ReadAsDateTimeOffset)
                        {
                            dateParseHandling = DateParseHandling.DateTimeOffset;
                        }
#endif
                        else
                        {
                            dateParseHandling = this._dateParseHandling;
                        }

                        if (dateParseHandling == DateParseHandling.DateTime)
                        {
                            if (DateTimeUtils.TryParseDateTime(this._stringReference, this.DateTimeZoneHandling, this.DateFormatString, this.Culture, out DateTime dt))
                            {
                                this.SetToken(JsonToken.Date, dt, false);
                                return;
                            }
                        }
#if HAVE_DATE_TIME_OFFSET
                        else
                        {
                            if (DateTimeUtils.TryParseDateTimeOffset(_stringReference, DateFormatString, Culture, out DateTimeOffset dt))
                            {
                                SetToken(JsonToken.Date, dt, false);
                                return;
                            }
                        }
#endif
                    }

                    this.SetToken(JsonToken.String, this._stringReference.ToString(), false);
                    this._quoteChar = quote;
                    break;
            }
        }

        private static void BlockCopyChars(char[] src, int srcOffset, char[] dst, int dstOffset, int count)
        {
            const int charByteCount = 2;

            Buffer.BlockCopy(src, srcOffset * charByteCount, dst, dstOffset * charByteCount, count * charByteCount);
        }

        private void ShiftBufferIfNeeded()
        {
            // once in the last 10% of the buffer, or buffer is already very large then
            // shift the remaining content to the start to avoid unnecessarily increasing
            // the buffer size when reading numbers/strings
            int length = this._chars.Length;
            if (length - this._charPos <= length * 0.1 || length >= LargeBufferLength)
            {
                int count = this._charsUsed - this._charPos;
                if (count > 0)
                {
                    BlockCopyChars(this._chars, this._charPos, this._chars, 0, count);
                }

                this._lineStartPos -= this._charPos;
                this._charPos = 0;
                this._charsUsed = count;
                this._chars[this._charsUsed] = '\0';
            }
        }

        private int ReadData(bool append)
        {
            return this.ReadData(append, 0);
        }

        private void PrepareBufferForReadData(bool append, int charsRequired)
        {
            // char buffer is full
            if (this._charsUsed + charsRequired >= this._chars.Length - 1)
            {
                if (append)
                {
                    int doubledArrayLength = this._chars.Length * 2;

                    // copy to new array either double the size of the current or big enough to fit required content
                    int newArrayLength = Math.Max(
                        doubledArrayLength < 0 ? int.MaxValue : doubledArrayLength, // handle overflow
                        this._charsUsed + charsRequired + 1);

                    // increase the size of the buffer
                    char[] dst = BufferUtils.RentBuffer(this._arrayPool, newArrayLength);

                    BlockCopyChars(this._chars, 0, dst, 0, this._chars.Length);

                    BufferUtils.ReturnBuffer(this._arrayPool, this._chars);

                    this._chars = dst;
                }
                else
                {
                    int remainingCharCount = this._charsUsed - this._charPos;

                    if (remainingCharCount + charsRequired + 1 >= this._chars.Length)
                    {
                        // the remaining count plus the required is bigger than the current buffer size
                        char[] dst = BufferUtils.RentBuffer(this._arrayPool, remainingCharCount + charsRequired + 1);

                        if (remainingCharCount > 0)
                        {
                            BlockCopyChars(this._chars, this._charPos, dst, 0, remainingCharCount);
                        }

                        BufferUtils.ReturnBuffer(this._arrayPool, this._chars);

                        this._chars = dst;
                    }
                    else
                    {
                        // copy any remaining data to the beginning of the buffer if needed and reset positions
                        if (remainingCharCount > 0)
                        {
                            BlockCopyChars(this._chars, this._charPos, this._chars, 0, remainingCharCount);
                        }
                    }

                    this._lineStartPos -= this._charPos;
                    this._charPos = 0;
                    this._charsUsed = remainingCharCount;
                }
            }
        }

        private int ReadData(bool append, int charsRequired)
        {
            if (this._isEndOfFile)
            {
                return 0;
            }

            this.PrepareBufferForReadData(append, charsRequired);

            int attemptCharReadCount = this._chars.Length - this._charsUsed - 1;

            int charsRead = this._reader.Read(this._chars, this._charsUsed, attemptCharReadCount);

            this._charsUsed += charsRead;

            if (charsRead == 0)
            {
                this._isEndOfFile = true;
            }

            this._chars[this._charsUsed] = '\0';
            return charsRead;
        }

        private bool EnsureChars(int relativePosition, bool append)
        {
            if (this._charPos + relativePosition >= this._charsUsed)
            {
                return this.ReadChars(relativePosition, append);
            }

            return true;
        }

        private bool ReadChars(int relativePosition, bool append)
        {
            if (this._isEndOfFile)
            {
                return false;
            }

            int charsRequired = this._charPos + relativePosition - this._charsUsed + 1;

            int totalCharsRead = 0;

            // it is possible that the TextReader doesn't return all data at once
            // repeat read until the required text is returned or the reader is out of content
            do
            {
                int charsRead = this.ReadData(append, charsRequired - totalCharsRead);

                // no more content
                if (charsRead == 0)
                {
                    break;
                }

                totalCharsRead += charsRead;
            } while (totalCharsRead < charsRequired);

            if (totalCharsRead < charsRequired)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="TextReader"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.
        /// </returns>
        public override bool Read()
        {
            this.EnsureBuffer();

            while (true)
            {
                switch (this._currentState)
                {
                    case State.Start:
                    case State.Property:
                    case State.Array:
                    case State.ArrayStart:
                    case State.Constructor:
                    case State.ConstructorStart:
                        return this.ParseValue();

                    case State.Object:
                    case State.ObjectStart:
                        return this.ParseObject();

                    case State.PostValue:
                        // returns true if it hits
                        // end of object or array
                        if (this.ParsePostValue(false))
                        {
                            return true;
                        }
                        break;

                    case State.Finished:
                        if (this.EnsureChars(0, false))
                        {
                            this.EatWhitespace();
                            if (this._isEndOfFile)
                            {
                                this.SetToken(JsonToken.None);
                                return false;
                            }
                            if (this._chars[this._charPos] == '/')
                            {
                                this.ParseComment(true);
                                return true;
                            }

                            throw JsonReaderException.Create(this, "Additional text encountered after finished reading JSON content: {0}.".FormatWith(CultureInfo.InvariantCulture, this._chars[this._charPos]));
                        }
                        this.SetToken(JsonToken.None);
                        return false;

                    default:
                        throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, this.CurrentState));
                }
            }
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="TextReader"/> as a <see cref="Nullable{T}"/> of <see cref="Int32"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="Int32"/>. This method will return <c>null</c> at the end of an array.</returns>
        public override int? ReadAsInt32()
        {
            return (int?)this.ReadNumberValue(ReadType.ReadAsInt32);
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="TextReader"/> as a <see cref="Nullable{T}"/> of <see cref="DateTime"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="DateTime"/>. This method will return <c>null</c> at the end of an array.</returns>
        public override DateTime? ReadAsDateTime()
        {
            return (DateTime?)this.ReadStringValue(ReadType.ReadAsDateTime);
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="TextReader"/> as a <see cref="String"/>.
        /// </summary>
        /// <returns>A <see cref="String"/>. This method will return <c>null</c> at the end of an array.</returns>
        public override string ReadAsString()
        {
            return (string)this.ReadStringValue(ReadType.ReadAsString);
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="TextReader"/> as a <see cref="Byte"/>[].
        /// </summary>
        /// <returns>A <see cref="Byte"/>[] or <c>null</c> if the next JSON token is null. This method will return <c>null</c> at the end of an array.</returns>
        public override byte[] ReadAsBytes()
        {
            this.EnsureBuffer();
            bool isWrapped = false;

            switch (this._currentState)
            {
                case State.PostValue:
                    if (this.ParsePostValue(true))
                    {
                        return null;
                    }
                    goto case State.Start;
                case State.Start:
                case State.Property:
                case State.Array:
                case State.ArrayStart:
                case State.Constructor:
                case State.ConstructorStart:
                    while (true)
                    {
                        char currentChar = this._chars[this._charPos];

                        switch (currentChar)
                        {
                            case '\0':
                                if (this.ReadNullChar())
                                {
                                    this.SetToken(JsonToken.None, null, false);
                                    return null;
                                }
                                break;

                            case '"':
                            case '\'':
                                this.ParseString(currentChar, ReadType.ReadAsBytes);
                                byte[] data = (byte[])this.Value;
                                if (isWrapped)
                                {
                                    this.ReaderReadAndAssert();
                                    if (this.TokenType != JsonToken.EndObject)
                                    {
                                        throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, this.TokenType));
                                    }
                                    this.SetToken(JsonToken.Bytes, data, false);
                                }
                                return data;

                            case '{':
                                this._charPos++;
                                this.SetToken(JsonToken.StartObject);
                                this.ReadIntoWrappedTypeObject();
                                isWrapped = true;
                                break;

                            case '[':
                                this._charPos++;
                                this.SetToken(JsonToken.StartArray);
                                return this.ReadArrayIntoByteArray();

                            case 'n':
                                this.HandleNull();
                                return null;

                            case '/':
                                this.ParseComment(false);
                                break;

                            case ',':
                                this.ProcessValueComma();
                                break;

                            case ']':
                                this._charPos++;
                                if (this._currentState == State.Array || this._currentState == State.ArrayStart || this._currentState == State.PostValue)
                                {
                                    this.SetToken(JsonToken.EndArray);
                                    return null;
                                }
                                throw this.CreateUnexpectedCharacterException(currentChar);
                            case StringUtils.CarriageReturn:
                                this.ProcessCarriageReturn(false);
                                break;

                            case StringUtils.LineFeed:
                                this.ProcessLineFeed();
                                break;

                            case ' ':
                            case StringUtils.Tab:
                                // eat
                                this._charPos++;
                                break;

                            default:
                                this._charPos++;

                                if (!char.IsWhiteSpace(currentChar))
                                {
                                    throw this.CreateUnexpectedCharacterException(currentChar);
                                }

                                // eat
                                break;
                        }
                    }
                case State.Finished:
                    this.ReadFinished();
                    return null;

                default:
                    throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, this.CurrentState));
            }
        }

        private object ReadStringValue(ReadType readType)
        {
            this.EnsureBuffer();

            switch (this._currentState)
            {
                case State.PostValue:
                    if (this.ParsePostValue(true))
                    {
                        return null;
                    }
                    goto case State.Start;
                case State.Start:
                case State.Property:
                case State.Array:
                case State.ArrayStart:
                case State.Constructor:
                case State.ConstructorStart:
                    while (true)
                    {
                        char currentChar = this._chars[this._charPos];

                        switch (currentChar)
                        {
                            case '\0':
                                if (this.ReadNullChar())
                                {
                                    this.SetToken(JsonToken.None, null, false);
                                    return null;
                                }
                                break;

                            case '"':
                            case '\'':
                                this.ParseString(currentChar, readType);
                                return this.FinishReadQuotedStringValue(readType);

                            case '-':
                                if (this.EnsureChars(1, true) && this._chars[this._charPos + 1] == 'I')
                                {
                                    return this.ParseNumberNegativeInfinity(readType);
                                }
                                else
                                {
                                    this.ParseNumber(readType);
                                    return this.Value;
                                }
                            case '.':
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                                if (readType != ReadType.ReadAsString)
                                {
                                    this._charPos++;
                                    throw this.CreateUnexpectedCharacterException(currentChar);
                                }
                                this.ParseNumber(ReadType.ReadAsString);
                                return this.Value;

                            case 't':
                            case 'f':
                                if (readType != ReadType.ReadAsString)
                                {
                                    this._charPos++;
                                    throw this.CreateUnexpectedCharacterException(currentChar);
                                }
                                string expected = currentChar == 't' ? JsonConvert.True : JsonConvert.False;
                                if (!this.MatchValueWithTrailingSeparator(expected))
                                {
                                    throw this.CreateUnexpectedCharacterException(this._chars[this._charPos]);
                                }
                                this.SetToken(JsonToken.String, expected);
                                return expected;

                            case 'I':
                                return this.ParseNumberPositiveInfinity(readType);

                            case 'N':
                                return this.ParseNumberNaN(readType);

                            case 'n':
                                this.HandleNull();
                                return null;

                            case '/':
                                this.ParseComment(false);
                                break;

                            case ',':
                                this.ProcessValueComma();
                                break;

                            case ']':
                                this._charPos++;
                                if (this._currentState == State.Array || this._currentState == State.ArrayStart || this._currentState == State.PostValue)
                                {
                                    this.SetToken(JsonToken.EndArray);
                                    return null;
                                }
                                throw this.CreateUnexpectedCharacterException(currentChar);
                            case StringUtils.CarriageReturn:
                                this.ProcessCarriageReturn(false);
                                break;

                            case StringUtils.LineFeed:
                                this.ProcessLineFeed();
                                break;

                            case ' ':
                            case StringUtils.Tab:
                                // eat
                                this._charPos++;
                                break;

                            default:
                                this._charPos++;

                                if (!char.IsWhiteSpace(currentChar))
                                {
                                    throw this.CreateUnexpectedCharacterException(currentChar);
                                }

                                // eat
                                break;
                        }
                    }
                case State.Finished:
                    this.ReadFinished();
                    return null;

                default:
                    throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, this.CurrentState));
            }
        }

        private object FinishReadQuotedStringValue(ReadType readType)
        {
            switch (readType)
            {
                case ReadType.ReadAsBytes:
                case ReadType.ReadAsString:
                    return this.Value;

                case ReadType.ReadAsDateTime:
                    if (this.Value is DateTime time)
                    {
                        return time;
                    }

                    return this.ReadDateTimeString((string)this.Value);
#if HAVE_DATE_TIME_OFFSET
                case ReadType.ReadAsDateTimeOffset:
                    if (Value is DateTimeOffset offset)
                    {
                        return offset;
                    }

                    return ReadDateTimeOffsetString((string)Value);
#endif
                default:
                    throw new ArgumentOutOfRangeException(nameof(readType));
            }
        }

        private JsonReaderException CreateUnexpectedCharacterException(char c)
        {
            return JsonReaderException.Create(this, "Unexpected character encountered while parsing value: {0}.".FormatWith(CultureInfo.InvariantCulture, c));
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="TextReader"/> as a <see cref="Nullable{T}"/> of <see cref="Boolean"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="Boolean"/>. This method will return <c>null</c> at the end of an array.</returns>
        public override bool? ReadAsBoolean()
        {
            this.EnsureBuffer();

            switch (this._currentState)
            {
                case State.PostValue:
                    if (this.ParsePostValue(true))
                    {
                        return null;
                    }
                    goto case State.Start;
                case State.Start:
                case State.Property:
                case State.Array:
                case State.ArrayStart:
                case State.Constructor:
                case State.ConstructorStart:
                    while (true)
                    {
                        char currentChar = this._chars[this._charPos];

                        switch (currentChar)
                        {
                            case '\0':
                                if (this.ReadNullChar())
                                {
                                    this.SetToken(JsonToken.None, null, false);
                                    return null;
                                }
                                break;

                            case '"':
                            case '\'':
                                this.ParseString(currentChar, ReadType.Read);
                                return this.ReadBooleanString(this._stringReference.ToString());

                            case 'n':
                                this.HandleNull();
                                return null;

                            case '-':
                            case '.':
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                                this.ParseNumber(ReadType.Read);
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

                            case 't':
                            case 'f':
                                bool isTrue = currentChar == 't';
                                string expected = isTrue ? JsonConvert.True : JsonConvert.False;

                                if (!this.MatchValueWithTrailingSeparator(expected))
                                {
                                    throw this.CreateUnexpectedCharacterException(this._chars[this._charPos]);
                                }
                                this.SetToken(JsonToken.Boolean, isTrue);
                                return isTrue;

                            case '/':
                                this.ParseComment(false);
                                break;

                            case ',':
                                this.ProcessValueComma();
                                break;

                            case ']':
                                this._charPos++;
                                if (this._currentState == State.Array || this._currentState == State.ArrayStart || this._currentState == State.PostValue)
                                {
                                    this.SetToken(JsonToken.EndArray);
                                    return null;
                                }
                                throw this.CreateUnexpectedCharacterException(currentChar);
                            case StringUtils.CarriageReturn:
                                this.ProcessCarriageReturn(false);
                                break;

                            case StringUtils.LineFeed:
                                this.ProcessLineFeed();
                                break;

                            case ' ':
                            case StringUtils.Tab:
                                // eat
                                this._charPos++;
                                break;

                            default:
                                this._charPos++;

                                if (!char.IsWhiteSpace(currentChar))
                                {
                                    throw this.CreateUnexpectedCharacterException(currentChar);
                                }

                                // eat
                                break;
                        }
                    }
                case State.Finished:
                    this.ReadFinished();
                    return null;

                default:
                    throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, this.CurrentState));
            }
        }

        private void ProcessValueComma()
        {
            this._charPos++;

            if (this._currentState != State.PostValue)
            {
                this.SetToken(JsonToken.Undefined);
                JsonReaderException ex = this.CreateUnexpectedCharacterException(',');
                // so the comma will be parsed again
                this._charPos--;

                throw ex;
            }

            this.SetStateBasedOnCurrent();
        }

        private object ReadNumberValue(ReadType readType)
        {
            this.EnsureBuffer();

            switch (this._currentState)
            {
                case State.PostValue:
                    if (this.ParsePostValue(true))
                    {
                        return null;
                    }
                    goto case State.Start;
                case State.Start:
                case State.Property:
                case State.Array:
                case State.ArrayStart:
                case State.Constructor:
                case State.ConstructorStart:
                    while (true)
                    {
                        char currentChar = this._chars[this._charPos];

                        switch (currentChar)
                        {
                            case '\0':
                                if (this.ReadNullChar())
                                {
                                    this.SetToken(JsonToken.None, null, false);
                                    return null;
                                }
                                break;

                            case '"':
                            case '\'':
                                this.ParseString(currentChar, readType);
                                return this.FinishReadQuotedNumber(readType);

                            case 'n':
                                this.HandleNull();
                                return null;

                            case 'N':
                                return this.ParseNumberNaN(readType);

                            case 'I':
                                return this.ParseNumberPositiveInfinity(readType);

                            case '-':
                                if (this.EnsureChars(1, true) && this._chars[this._charPos + 1] == 'I')
                                {
                                    return this.ParseNumberNegativeInfinity(readType);
                                }
                                else
                                {
                                    this.ParseNumber(readType);
                                    return this.Value;
                                }
                            case '.':
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                                this.ParseNumber(readType);
                                return this.Value;

                            case '/':
                                this.ParseComment(false);
                                break;

                            case ',':
                                this.ProcessValueComma();
                                break;

                            case ']':
                                this._charPos++;
                                if (this._currentState == State.Array || this._currentState == State.ArrayStart || this._currentState == State.PostValue)
                                {
                                    this.SetToken(JsonToken.EndArray);
                                    return null;
                                }
                                throw this.CreateUnexpectedCharacterException(currentChar);
                            case StringUtils.CarriageReturn:
                                this.ProcessCarriageReturn(false);
                                break;

                            case StringUtils.LineFeed:
                                this.ProcessLineFeed();
                                break;

                            case ' ':
                            case StringUtils.Tab:
                                // eat
                                this._charPos++;
                                break;

                            default:
                                this._charPos++;

                                if (!char.IsWhiteSpace(currentChar))
                                {
                                    throw this.CreateUnexpectedCharacterException(currentChar);
                                }

                                // eat
                                break;
                        }
                    }
                case State.Finished:
                    this.ReadFinished();
                    return null;

                default:
                    throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, this.CurrentState));
            }
        }

        private object FinishReadQuotedNumber(ReadType readType)
        {
            switch (readType)
            {
                case ReadType.ReadAsInt32:
                    return this.ReadInt32String(this._stringReference.ToString());

                case ReadType.ReadAsDecimal:
                    return this.ReadDecimalString(this._stringReference.ToString());

                case ReadType.ReadAsDouble:
                    return this.ReadDoubleString(this._stringReference.ToString());

                default:
                    throw new ArgumentOutOfRangeException(nameof(readType));
            }
        }

#if HAVE_DATE_TIME_OFFSET
        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="TextReader"/> as a <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/>. This method will return <c>null</c> at the end of an array.</returns>
        public override DateTimeOffset? ReadAsDateTimeOffset()
        {
            return (DateTimeOffset?)ReadStringValue(ReadType.ReadAsDateTimeOffset);
        }
#endif

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="TextReader"/> as a <see cref="Nullable{T}"/> of <see cref="Decimal"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="Decimal"/>. This method will return <c>null</c> at the end of an array.</returns>
        public override decimal? ReadAsDecimal()
        {
            return (decimal?)this.ReadNumberValue(ReadType.ReadAsDecimal);
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="TextReader"/> as a <see cref="Nullable{T}"/> of <see cref="Double"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="Double"/>. This method will return <c>null</c> at the end of an array.</returns>
        public override double? ReadAsDouble()
        {
            return (double?)this.ReadNumberValue(ReadType.ReadAsDouble);
        }

        private void HandleNull()
        {
            if (this.EnsureChars(1, true))
            {
                char next = this._chars[this._charPos + 1];

                if (next == 'u')
                {
                    this.ParseNull();
                    return;
                }

                this._charPos += 2;
                throw this.CreateUnexpectedCharacterException(this._chars[this._charPos - 1]);
            }

            this._charPos = this._charsUsed;
            throw this.CreateUnexpectedEndException();
        }

        private void ReadFinished()
        {
            if (this.EnsureChars(0, false))
            {
                this.EatWhitespace();
                if (this._isEndOfFile)
                {
                    return;
                }
                if (this._chars[this._charPos] == '/')
                {
                    this.ParseComment(false);
                }
                else
                {
                    throw JsonReaderException.Create(this, "Additional text encountered after finished reading JSON content: {0}.".FormatWith(CultureInfo.InvariantCulture, this._chars[this._charPos]));
                }
            }

            this.SetToken(JsonToken.None);
        }

        private bool ReadNullChar()
        {
            if (this._charsUsed == this._charPos)
            {
                if (this.ReadData(false) == 0)
                {
                    this._isEndOfFile = true;
                    return true;
                }
            }
            else
            {
                this._charPos++;
            }

            return false;
        }

        private void EnsureBuffer()
        {
            if (this._chars == null)
            {
                this._chars = BufferUtils.RentBuffer(this._arrayPool, 1024);
                this._chars[0] = '\0';
            }
        }

        private void ReadStringIntoBuffer(char quote)
        {
            int charPos = this._charPos;
            int initialPosition = this._charPos;
            int lastWritePosition = this._charPos;
            this._stringBuffer.Position = 0;

            while (true)
            {
                switch (this._chars[charPos++])
                {
                    case '\0':
                        if (this._charsUsed == charPos - 1)
                        {
                            charPos--;

                            if (this.ReadData(true) == 0)
                            {
                                this._charPos = charPos;
                                throw JsonReaderException.Create(this, "Unterminated string. Expected delimiter: {0}.".FormatWith(CultureInfo.InvariantCulture, quote));
                            }
                        }
                        break;

                    case '\\':
                        this._charPos = charPos;
                        if (!this.EnsureChars(0, true))
                        {
                            throw JsonReaderException.Create(this, "Unterminated string. Expected delimiter: {0}.".FormatWith(CultureInfo.InvariantCulture, quote));
                        }

                        // start of escape sequence
                        int escapeStartPos = charPos - 1;

                        char currentChar = this._chars[charPos];
                        charPos++;

                        char writeChar;

                        switch (currentChar)
                        {
                            case 'b':
                                writeChar = '\b';
                                break;

                            case 't':
                                writeChar = '\t';
                                break;

                            case 'n':
                                writeChar = '\n';
                                break;

                            case 'f':
                                writeChar = '\f';
                                break;

                            case 'r':
                                writeChar = '\r';
                                break;

                            case '\\':
                                writeChar = '\\';
                                break;

                            case '"':
                            case '\'':
                            case '/':
                                writeChar = currentChar;
                                break;

                            case 'u':
                                this._charPos = charPos;
                                writeChar = this.ParseUnicode();

                                if (StringUtils.IsLowSurrogate(writeChar))
                                {
                                    // low surrogate with no preceding high surrogate; this char is replaced
                                    writeChar = UnicodeReplacementChar;
                                }
                                else if (StringUtils.IsHighSurrogate(writeChar))
                                {
                                    bool anotherHighSurrogate;

                                    // loop for handling situations where there are multiple consecutive high surrogates
                                    do
                                    {
                                        anotherHighSurrogate = false;

                                        // potential start of a surrogate pair
                                        if (this.EnsureChars(2, true) && this._chars[this._charPos] == '\\' && this._chars[this._charPos + 1] == 'u')
                                        {
                                            char highSurrogate = writeChar;

                                            this._charPos += 2;
                                            writeChar = this.ParseUnicode();

                                            if (StringUtils.IsLowSurrogate(writeChar))
                                            {
                                                // a valid surrogate pair!
                                            }
                                            else if (StringUtils.IsHighSurrogate(writeChar))
                                            {
                                                // another high surrogate; replace current and start check over
                                                highSurrogate = UnicodeReplacementChar;
                                                anotherHighSurrogate = true;
                                            }
                                            else
                                            {
                                                // high surrogate not followed by low surrogate; original char is replaced
                                                highSurrogate = UnicodeReplacementChar;
                                            }

                                            this.EnsureBufferNotEmpty();

                                            this.WriteCharToBuffer(highSurrogate, lastWritePosition, escapeStartPos);
                                            lastWritePosition = this._charPos;
                                        }
                                        else
                                        {
                                            // there are not enough remaining chars for the low surrogate or is not follow by unicode sequence
                                            // replace high surrogate and continue on as usual
                                            writeChar = UnicodeReplacementChar;
                                        }
                                    } while (anotherHighSurrogate);
                                }

                                charPos = this._charPos;
                                break;

                            default:
                                this._charPos = charPos;
                                throw JsonReaderException.Create(this, "Bad JSON escape sequence: {0}.".FormatWith(CultureInfo.InvariantCulture, @"\" + currentChar));
                        }

                        this.EnsureBufferNotEmpty();
                        this.WriteCharToBuffer(writeChar, lastWritePosition, escapeStartPos);

                        lastWritePosition = charPos;
                        break;

                    case StringUtils.CarriageReturn:
                        this._charPos = charPos - 1;
                        this.ProcessCarriageReturn(true);
                        charPos = this._charPos;
                        break;

                    case StringUtils.LineFeed:
                        this._charPos = charPos - 1;
                        this.ProcessLineFeed();
                        charPos = this._charPos;
                        break;

                    case '"':
                    case '\'':
                        if (this._chars[charPos - 1] == quote)
                        {
                            this.FinishReadStringIntoBuffer(charPos - 1, initialPosition, lastWritePosition);
                            return;
                        }
                        break;
                }
            }
        }

        private void FinishReadStringIntoBuffer(int charPos, int initialPosition, int lastWritePosition)
        {
            if (initialPosition == lastWritePosition)
            {
                this._stringReference = new StringReference(this._chars, initialPosition, charPos - initialPosition);
            }
            else
            {
                this.EnsureBufferNotEmpty();

                if (charPos > lastWritePosition)
                {
                    this._stringBuffer.Append(this._arrayPool, this._chars, lastWritePosition, charPos - lastWritePosition);
                }

                this._stringReference = new StringReference(this._stringBuffer.InternalBuffer, 0, this._stringBuffer.Position);
            }

            this._charPos = charPos + 1;
        }

        private void WriteCharToBuffer(char writeChar, int lastWritePosition, int writeToPosition)
        {
            if (writeToPosition > lastWritePosition)
            {
                this._stringBuffer.Append(this._arrayPool, this._chars, lastWritePosition, writeToPosition - lastWritePosition);
            }

            this._stringBuffer.Append(this._arrayPool, writeChar);
        }

        private char ConvertUnicode(bool enoughChars)
        {
            if (enoughChars)
            {
                if (ConvertUtils.TryHexTextToInt(this._chars, this._charPos, this._charPos + 4, out int value))
                {
                    char hexChar = Convert.ToChar(value);
                    this._charPos += 4;
                    return hexChar;
                }
                else
                {
                    throw JsonReaderException.Create(this, @"Invalid Unicode escape sequence: \u{0}.".FormatWith(CultureInfo.InvariantCulture, new string(this._chars, this._charPos, 4)));
                }
            }
            else
            {
                throw JsonReaderException.Create(this, "Unexpected end while parsing Unicode escape sequence.");
            }
        }

        private char ParseUnicode()
        {
            return this.ConvertUnicode(this.EnsureChars(4, true));
        }

        private void ReadNumberIntoBuffer()
        {
            int charPos = this._charPos;

            while (true)
            {
                char currentChar = this._chars[charPos];
                if (currentChar == '\0')
                {
                    this._charPos = charPos;

                    if (this._charsUsed == charPos)
                    {
                        if (this.ReadData(true) == 0)
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else if (this.ReadNumberCharIntoBuffer(currentChar, charPos))
                {
                    return;
                }
                else
                {
                    charPos++;
                }
            }
        }

        private bool ReadNumberCharIntoBuffer(char currentChar, int charPos)
        {
            switch (currentChar)
            {
                case '-':
                case '+':
                case 'a':
                case 'A':
                case 'b':
                case 'B':
                case 'c':
                case 'C':
                case 'd':
                case 'D':
                case 'e':
                case 'E':
                case 'f':
                case 'F':
                case 'x':
                case 'X':
                case '.':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return false;

                default:
                    this._charPos = charPos;

                    if (char.IsWhiteSpace(currentChar) || currentChar == ',' || currentChar == '}' || currentChar == ']' || currentChar == ')' || currentChar == '/')
                    {
                        return true;
                    }

                    throw JsonReaderException.Create(this, "Unexpected character encountered while parsing number: {0}.".FormatWith(CultureInfo.InvariantCulture, currentChar));
            }
        }

        private void ClearRecentString()
        {
            this._stringBuffer.Position = 0;
            this._stringReference = new StringReference();
        }

        private bool ParsePostValue(bool ignoreComments)
        {
            while (true)
            {
                char currentChar = this._chars[this._charPos];

                switch (currentChar)
                {
                    case '\0':
                        if (this._charsUsed == this._charPos)
                        {
                            if (this.ReadData(false) == 0)
                            {
                                this._currentState = State.Finished;
                                return false;
                            }
                        }
                        else
                        {
                            this._charPos++;
                        }
                        break;

                    case '}':
                        this._charPos++;
                        this.SetToken(JsonToken.EndObject);
                        return true;

                    case ']':
                        this._charPos++;
                        this.SetToken(JsonToken.EndArray);
                        return true;

                    case ')':
                        this._charPos++;
                        this.SetToken(JsonToken.EndConstructor);
                        return true;

                    case '/':
                        this.ParseComment(!ignoreComments);
                        if (!ignoreComments)
                        {
                            return true;
                        }
                        break;

                    case ',':
                        this._charPos++;

                        // finished parsing
                        this.SetStateBasedOnCurrent();
                        return false;

                    case ' ':
                    case StringUtils.Tab:
                        // eat
                        this._charPos++;
                        break;

                    case StringUtils.CarriageReturn:
                        this.ProcessCarriageReturn(false);
                        break;

                    case StringUtils.LineFeed:
                        this.ProcessLineFeed();
                        break;

                    default:
                        if (char.IsWhiteSpace(currentChar))
                        {
                            // eat
                            this._charPos++;
                        }
                        else
                        {
                            // handle multiple content without comma delimiter
                            if (this.SupportMultipleContent && this.Depth == 0)
                            {
                                this.SetStateBasedOnCurrent();
                                return false;
                            }

                            throw JsonReaderException.Create(this, "After parsing a value an unexpected character was encountered: {0}.".FormatWith(CultureInfo.InvariantCulture, currentChar));
                        }
                        break;
                }
            }
        }

        private bool ParseObject()
        {
            while (true)
            {
                char currentChar = this._chars[this._charPos];

                switch (currentChar)
                {
                    case '\0':
                        if (this._charsUsed == this._charPos)
                        {
                            if (this.ReadData(false) == 0)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            this._charPos++;
                        }
                        break;

                    case '}':
                        this.SetToken(JsonToken.EndObject);
                        this._charPos++;
                        return true;

                    case '/':
                        this.ParseComment(true);
                        return true;

                    case StringUtils.CarriageReturn:
                        this.ProcessCarriageReturn(false);
                        break;

                    case StringUtils.LineFeed:
                        this.ProcessLineFeed();
                        break;

                    case ' ':
                    case StringUtils.Tab:
                        // eat
                        this._charPos++;
                        break;

                    default:
                        if (char.IsWhiteSpace(currentChar))
                        {
                            // eat
                            this._charPos++;
                        }
                        else
                        {
                            return this.ParseProperty();
                        }
                        break;
                }
            }
        }

        private bool ParseProperty()
        {
            char firstChar = this._chars[this._charPos];
            char quoteChar;

            if (firstChar == '"' || firstChar == '\'')
            {
                this._charPos++;
                quoteChar = firstChar;
                this.ShiftBufferIfNeeded();
                this.ReadStringIntoBuffer(quoteChar);
            }
            else if (this.ValidIdentifierChar(firstChar))
            {
                quoteChar = '\0';
                this.ShiftBufferIfNeeded();
                this.ParseUnquotedProperty();
            }
            else
            {
                throw JsonReaderException.Create(this, "Invalid property identifier character: {0}.".FormatWith(CultureInfo.InvariantCulture, this._chars[this._charPos]));
            }

            string propertyName;

            if (this.NameTable != null)
            {
                propertyName = this.NameTable.Get(this._stringReference.Chars, this._stringReference.StartIndex, this._stringReference.Length);

                // no match in name table
                if (propertyName == null)
                {
                    propertyName = this._stringReference.ToString();
                }
            }
            else
            {
                propertyName = this._stringReference.ToString();
            }

            this.EatWhitespace();

            if (this._chars[this._charPos] != ':')
            {
                throw JsonReaderException.Create(this, "Invalid character after parsing property name. Expected ':' but got: {0}.".FormatWith(CultureInfo.InvariantCulture, this._chars[this._charPos]));
            }

            this._charPos++;

            this.SetToken(JsonToken.PropertyName, propertyName);
            this._quoteChar = quoteChar;
            this.ClearRecentString();

            return true;
        }

        private bool ValidIdentifierChar(char value)
        {
            return (char.IsLetterOrDigit(value) || value == '_' || value == '$');
        }

        private void ParseUnquotedProperty()
        {
            int initialPosition = this._charPos;

            // parse unquoted property name until whitespace or colon
            while (true)
            {
                char currentChar = this._chars[this._charPos];
                if (currentChar == '\0')
                {
                    if (this._charsUsed == this._charPos)
                    {
                        if (this.ReadData(true) == 0)
                        {
                            throw JsonReaderException.Create(this, "Unexpected end while parsing unquoted property name.");
                        }

                        continue;
                    }

                    this._stringReference = new StringReference(this._chars, initialPosition, this._charPos - initialPosition);
                    return;
                }

                if (this.ReadUnquotedPropertyReportIfDone(currentChar, initialPosition))
                {
                    return;
                }
            }
        }

        private bool ReadUnquotedPropertyReportIfDone(char currentChar, int initialPosition)
        {
            if (this.ValidIdentifierChar(currentChar))
            {
                this._charPos++;
                return false;
            }

            if (char.IsWhiteSpace(currentChar) || currentChar == ':')
            {
                this._stringReference = new StringReference(this._chars, initialPosition, this._charPos - initialPosition);
                return true;
            }

            throw JsonReaderException.Create(this, "Invalid JavaScript property identifier character: {0}.".FormatWith(CultureInfo.InvariantCulture, currentChar));
        }

        private bool ParseValue()
        {
            while (true)
            {
                char currentChar = this._chars[this._charPos];

                switch (currentChar)
                {
                    case '\0':
                        if (this._charsUsed == this._charPos)
                        {
                            if (this.ReadData(false) == 0)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            this._charPos++;
                        }
                        break;

                    case '"':
                    case '\'':
                        this.ParseString(currentChar, ReadType.Read);
                        return true;

                    case 't':
                        this.ParseTrue();
                        return true;

                    case 'f':
                        this.ParseFalse();
                        return true;

                    case 'n':
                        if (this.EnsureChars(1, true))
                        {
                            char next = this._chars[this._charPos + 1];

                            if (next == 'u')
                            {
                                this.ParseNull();
                            }
                            else if (next == 'e')
                            {
                                this.ParseConstructor();
                            }
                            else
                            {
                                throw this.CreateUnexpectedCharacterException(this._chars[this._charPos]);
                            }
                        }
                        else
                        {
                            this._charPos++;
                            throw this.CreateUnexpectedEndException();
                        }
                        return true;

                    case 'N':
                        this.ParseNumberNaN(ReadType.Read);
                        return true;

                    case 'I':
                        this.ParseNumberPositiveInfinity(ReadType.Read);
                        return true;

                    case '-':
                        if (this.EnsureChars(1, true) && this._chars[this._charPos + 1] == 'I')
                        {
                            this.ParseNumberNegativeInfinity(ReadType.Read);
                        }
                        else
                        {
                            this.ParseNumber(ReadType.Read);
                        }
                        return true;

                    case '/':
                        this.ParseComment(true);
                        return true;

                    case 'u':
                        this.ParseUndefined();
                        return true;

                    case '{':
                        this._charPos++;
                        this.SetToken(JsonToken.StartObject);
                        return true;

                    case '[':
                        this._charPos++;
                        this.SetToken(JsonToken.StartArray);
                        return true;

                    case ']':
                        this._charPos++;
                        this.SetToken(JsonToken.EndArray);
                        return true;

                    case ',':
                        // don't increment position, the next call to read will handle comma
                        // this is done to handle multiple empty comma values
                        this.SetToken(JsonToken.Undefined);
                        return true;

                    case ')':
                        this._charPos++;
                        this.SetToken(JsonToken.EndConstructor);
                        return true;

                    case StringUtils.CarriageReturn:
                        this.ProcessCarriageReturn(false);
                        break;

                    case StringUtils.LineFeed:
                        this.ProcessLineFeed();
                        break;

                    case ' ':
                    case StringUtils.Tab:
                        // eat
                        this._charPos++;
                        break;

                    default:
                        if (char.IsWhiteSpace(currentChar))
                        {
                            // eat
                            this._charPos++;
                            break;
                        }
                        if (char.IsNumber(currentChar) || currentChar == '-' || currentChar == '.')
                        {
                            this.ParseNumber(ReadType.Read);
                            return true;
                        }

                        throw this.CreateUnexpectedCharacterException(currentChar);
                }
            }
        }

        private void ProcessLineFeed()
        {
            this._charPos++;
            this.OnNewLine(this._charPos);
        }

        private void ProcessCarriageReturn(bool append)
        {
            this._charPos++;

            this.SetNewLine(this.EnsureChars(1, append));
        }

        private void EatWhitespace()
        {
            while (true)
            {
                char currentChar = this._chars[this._charPos];

                switch (currentChar)
                {
                    case '\0':
                        if (this._charsUsed == this._charPos)
                        {
                            if (this.ReadData(false) == 0)
                            {
                                return;
                            }
                        }
                        else
                        {
                            this._charPos++;
                        }
                        break;

                    case StringUtils.CarriageReturn:
                        this.ProcessCarriageReturn(false);
                        break;

                    case StringUtils.LineFeed:
                        this.ProcessLineFeed();
                        break;

                    default:
                        if (currentChar == ' ' || char.IsWhiteSpace(currentChar))
                        {
                            this._charPos++;
                        }
                        else
                        {
                            return;
                        }
                        break;
                }
            }
        }

        private void ParseConstructor()
        {
            if (this.MatchValueWithTrailingSeparator("new"))
            {
                this.EatWhitespace();

                int initialPosition = this._charPos;
                int endPosition;

                while (true)
                {
                    char currentChar = this._chars[this._charPos];
                    if (currentChar == '\0')
                    {
                        if (this._charsUsed == this._charPos)
                        {
                            if (this.ReadData(true) == 0)
                            {
                                throw JsonReaderException.Create(this, "Unexpected end while parsing constructor.");
                            }
                        }
                        else
                        {
                            endPosition = this._charPos;
                            this._charPos++;
                            break;
                        }
                    }
                    else if (char.IsLetterOrDigit(currentChar))
                    {
                        this._charPos++;
                    }
                    else if (currentChar == StringUtils.CarriageReturn)
                    {
                        endPosition = this._charPos;
                        this.ProcessCarriageReturn(true);
                        break;
                    }
                    else if (currentChar == StringUtils.LineFeed)
                    {
                        endPosition = this._charPos;
                        this.ProcessLineFeed();
                        break;
                    }
                    else if (char.IsWhiteSpace(currentChar))
                    {
                        endPosition = this._charPos;
                        this._charPos++;
                        break;
                    }
                    else if (currentChar == '(')
                    {
                        endPosition = this._charPos;
                        break;
                    }
                    else
                    {
                        throw JsonReaderException.Create(this, "Unexpected character while parsing constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, currentChar));
                    }
                }

                this._stringReference = new StringReference(this._chars, initialPosition, endPosition - initialPosition);
                string constructorName = this._stringReference.ToString();

                this.EatWhitespace();

                if (this._chars[this._charPos] != '(')
                {
                    throw JsonReaderException.Create(this, "Unexpected character while parsing constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, this._chars[this._charPos]));
                }

                this._charPos++;

                this.ClearRecentString();

                this.SetToken(JsonToken.StartConstructor, constructorName);
            }
            else
            {
                throw JsonReaderException.Create(this, "Unexpected content while parsing JSON.");
            }
        }

        private void ParseNumber(ReadType readType)
        {
            this.ShiftBufferIfNeeded();

            char firstChar = this._chars[this._charPos];
            int initialPosition = this._charPos;

            this.ReadNumberIntoBuffer();

            this.ParseReadNumber(readType, firstChar, initialPosition);
        }

        private void ParseReadNumber(ReadType readType, char firstChar, int initialPosition)
        {
            // set state to PostValue now so that if there is an error parsing the number then the reader can continue
            this.SetPostValueState(true);

            this._stringReference = new StringReference(this._chars, initialPosition, this._charPos - initialPosition);

            object numberValue;
            JsonToken numberType;

            bool singleDigit = (char.IsDigit(firstChar) && this._stringReference.Length == 1);
            bool nonBase10 = (firstChar == '0' && this._stringReference.Length > 1 && this._stringReference.Chars[this._stringReference.StartIndex + 1] != '.' && this._stringReference.Chars[this._stringReference.StartIndex + 1] != 'e' && this._stringReference.Chars[this._stringReference.StartIndex + 1] != 'E');

            if (readType == ReadType.ReadAsString)
            {
                string number = this._stringReference.ToString();

                // validate that the string is a valid number
                if (nonBase10)
                {
                    try
                    {
                        if (number.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        {
                            Convert.ToInt64(number, 16);
                        }
                        else
                        {
                            Convert.ToInt64(number, 8);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw this.ThrowReaderError("Input string '{0}' is not a valid number.".FormatWith(CultureInfo.InvariantCulture, number), ex);
                    }
                }
                else
                {
                    if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                    {
                        throw this.ThrowReaderError("Input string '{0}' is not a valid number.".FormatWith(CultureInfo.InvariantCulture, this._stringReference.ToString()));
                    }
                }

                numberType = JsonToken.String;
                numberValue = number;
            }
            else if (readType == ReadType.ReadAsInt32)
            {
                if (singleDigit)
                {
                    // digit char values start at 48
                    numberValue = firstChar - 48;
                }
                else if (nonBase10)
                {
                    string number = this._stringReference.ToString();

                    try
                    {
                        int integer = number.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt32(number, 16) : Convert.ToInt32(number, 8);

                        numberValue = integer;
                    }
                    catch (Exception ex)
                    {
                        throw this.ThrowReaderError("Input string '{0}' is not a valid integer.".FormatWith(CultureInfo.InvariantCulture, number), ex);
                    }
                }
                else
                {
                    ParseResult parseResult = ConvertUtils.Int32TryParse(this._stringReference.Chars, this._stringReference.StartIndex, this._stringReference.Length, out int value);
                    if (parseResult == ParseResult.Success)
                    {
                        numberValue = value;
                    }
                    else if (parseResult == ParseResult.Overflow)
                    {
                        throw this.ThrowReaderError("JSON integer {0} is too large or small for an Int32.".FormatWith(CultureInfo.InvariantCulture, this._stringReference.ToString()));
                    }
                    else
                    {
                        throw this.ThrowReaderError("Input string '{0}' is not a valid integer.".FormatWith(CultureInfo.InvariantCulture, this._stringReference.ToString()));
                    }
                }

                numberType = JsonToken.Integer;
            }
            else if (readType == ReadType.ReadAsDecimal)
            {
                if (singleDigit)
                {
                    // digit char values start at 48
                    numberValue = (decimal)firstChar - 48;
                }
                else if (nonBase10)
                {
                    string number = this._stringReference.ToString();

                    try
                    {
                        // decimal.Parse doesn't support parsing hexadecimal values
                        long integer = number.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt64(number, 16) : Convert.ToInt64(number, 8);

                        numberValue = Convert.ToDecimal(integer);
                    }
                    catch (Exception ex)
                    {
                        throw this.ThrowReaderError("Input string '{0}' is not a valid decimal.".FormatWith(CultureInfo.InvariantCulture, number), ex);
                    }
                }
                else
                {
                    ParseResult parseResult = ConvertUtils.DecimalTryParse(this._stringReference.Chars, this._stringReference.StartIndex, this._stringReference.Length, out decimal value);
                    if (parseResult == ParseResult.Success)
                    {
                        numberValue = value;
                    }
                    else
                    {
                        throw this.ThrowReaderError("Input string '{0}' is not a valid decimal.".FormatWith(CultureInfo.InvariantCulture, this._stringReference.ToString()));
                    }
                }

                numberType = JsonToken.Float;
            }
            else if (readType == ReadType.ReadAsDouble)
            {
                if (singleDigit)
                {
                    // digit char values start at 48
                    numberValue = (double)firstChar - 48;
                }
                else if (nonBase10)
                {
                    string number = this._stringReference.ToString();

                    try
                    {
                        // double.Parse doesn't support parsing hexadecimal values
                        long integer = number.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt64(number, 16) : Convert.ToInt64(number, 8);

                        numberValue = Convert.ToDouble(integer);
                    }
                    catch (Exception ex)
                    {
                        throw this.ThrowReaderError("Input string '{0}' is not a valid double.".FormatWith(CultureInfo.InvariantCulture, number), ex);
                    }
                }
                else
                {
                    string number = this._stringReference.ToString();

                    if (double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                    {
                        numberValue = value;
                    }
                    else
                    {
                        throw this.ThrowReaderError("Input string '{0}' is not a valid double.".FormatWith(CultureInfo.InvariantCulture, this._stringReference.ToString()));
                    }
                }

                numberType = JsonToken.Float;
            }
            else
            {
                if (singleDigit)
                {
                    // digit char values start at 48
                    numberValue = (long)firstChar - 48;
                    numberType = JsonToken.Integer;
                }
                else if (nonBase10)
                {
                    string number = this._stringReference.ToString();

                    try
                    {
                        numberValue = number.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt64(number, 16) : Convert.ToInt64(number, 8);
                    }
                    catch (Exception ex)
                    {
                        throw this.ThrowReaderError("Input string '{0}' is not a valid number.".FormatWith(CultureInfo.InvariantCulture, number), ex);
                    }

                    numberType = JsonToken.Integer;
                }
                else
                {
                    ParseResult parseResult = ConvertUtils.Int64TryParse(this._stringReference.Chars, this._stringReference.StartIndex, this._stringReference.Length, out long value);
                    if (parseResult == ParseResult.Success)
                    {
                        numberValue = value;
                        numberType = JsonToken.Integer;
                    }
                    else if (parseResult == ParseResult.Overflow)
                    {
#if HAVE_BIG_INTEGER
                        string number = _stringReference.ToString();

                        if (number.Length > MaximumJavascriptIntegerCharacterLength)
                        {
                            throw ThrowReaderError("JSON integer {0} is too large to parse.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
                        }

                        numberValue = BigIntegerParse(number, CultureInfo.InvariantCulture);
                        numberType = JsonToken.Integer;
#else
                        throw this.ThrowReaderError("JSON integer {0} is too large or small for an Int64.".FormatWith(CultureInfo.InvariantCulture, this._stringReference.ToString()));
#endif
                    }
                    else
                    {
                        if (this._floatParseHandling == FloatParseHandling.Decimal)
                        {
                            parseResult = ConvertUtils.DecimalTryParse(this._stringReference.Chars, this._stringReference.StartIndex, this._stringReference.Length, out decimal d);
                            if (parseResult == ParseResult.Success)
                            {
                                numberValue = d;
                            }
                            else
                            {
                                throw this.ThrowReaderError("Input string '{0}' is not a valid decimal.".FormatWith(CultureInfo.InvariantCulture, this._stringReference.ToString()));
                            }
                        }
                        else
                        {
                            string number = this._stringReference.ToString();

                            if (double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                            {
                                numberValue = d;
                            }
                            else
                            {
                                throw this.ThrowReaderError("Input string '{0}' is not a valid number.".FormatWith(CultureInfo.InvariantCulture, this._stringReference.ToString()));
                            }
                        }

                        numberType = JsonToken.Float;
                    }
                }
            }

            this.ClearRecentString();

            // index has already been updated
            this.SetToken(numberType, numberValue, false);
        }

        private JsonReaderException ThrowReaderError(string message, Exception ex = null)
        {
            this.SetToken(JsonToken.Undefined, null, false);
            return JsonReaderException.Create(this, message, ex);
        }

#if HAVE_BIG_INTEGER
        // By using the BigInteger type in a separate method,
        // the runtime can execute the ParseNumber even if
        // the System.Numerics.BigInteger.Parse method is
        // missing, which happens in some versions of Mono
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static object BigIntegerParse(string number, CultureInfo culture)
        {
            return System.Numerics.BigInteger.Parse(number, culture);
        }
#endif

        private void ParseComment(bool setToken)
        {
            // should have already parsed / character before reaching this method
            this._charPos++;

            if (!this.EnsureChars(1, false))
            {
                throw JsonReaderException.Create(this, "Unexpected end while parsing comment.");
            }

            bool singlelineComment;

            if (this._chars[this._charPos] == '*')
            {
                singlelineComment = false;
            }
            else if (this._chars[this._charPos] == '/')
            {
                singlelineComment = true;
            }
            else
            {
                throw JsonReaderException.Create(this, "Error parsing comment. Expected: *, got {0}.".FormatWith(CultureInfo.InvariantCulture, this._chars[this._charPos]));
            }

            this._charPos++;

            int initialPosition = this._charPos;

            while (true)
            {
                switch (this._chars[this._charPos])
                {
                    case '\0':
                        if (this._charsUsed == this._charPos)
                        {
                            if (this.ReadData(true) == 0)
                            {
                                if (!singlelineComment)
                                {
                                    throw JsonReaderException.Create(this, "Unexpected end while parsing comment.");
                                }

                                this.EndComment(setToken, initialPosition, this._charPos);
                                return;
                            }
                        }
                        else
                        {
                            this._charPos++;
                        }
                        break;

                    case '*':
                        this._charPos++;

                        if (!singlelineComment)
                        {
                            if (this.EnsureChars(0, true))
                            {
                                if (this._chars[this._charPos] == '/')
                                {
                                    this.EndComment(setToken, initialPosition, this._charPos - 1);

                                    this._charPos++;
                                    return;
                                }
                            }
                        }
                        break;

                    case StringUtils.CarriageReturn:
                        if (singlelineComment)
                        {
                            this.EndComment(setToken, initialPosition, this._charPos);
                            return;
                        }
                        this.ProcessCarriageReturn(true);
                        break;

                    case StringUtils.LineFeed:
                        if (singlelineComment)
                        {
                            this.EndComment(setToken, initialPosition, this._charPos);
                            return;
                        }
                        this.ProcessLineFeed();
                        break;

                    default:
                        this._charPos++;
                        break;
                }
            }
        }

        private void EndComment(bool setToken, int initialPosition, int endPosition)
        {
            if (setToken)
            {
                this.SetToken(JsonToken.Comment, new string(this._chars, initialPosition, endPosition - initialPosition));
            }
        }

        private bool MatchValue(string value)
        {
            return this.MatchValue(this.EnsureChars(value.Length - 1, true), value);
        }

        private bool MatchValue(bool enoughChars, string value)
        {
            if (!enoughChars)
            {
                this._charPos = this._charsUsed;
                throw this.CreateUnexpectedEndException();
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (this._chars[this._charPos + i] != value[i])
                {
                    this._charPos += i;
                    return false;
                }
            }

            this._charPos += value.Length;

            return true;
        }

        private bool MatchValueWithTrailingSeparator(string value)
        {
            // will match value and then move to the next character, checking that it is a separator character
            bool match = this.MatchValue(value);

            if (!match)
            {
                return false;
            }

            if (!this.EnsureChars(0, false))
            {
                return true;
            }

            return this.IsSeparator(this._chars[this._charPos]) || this._chars[this._charPos] == '\0';
        }

        private bool IsSeparator(char c)
        {
            switch (c)
            {
                case '}':
                case ']':
                case ',':
                    return true;

                case '/':
                    // check next character to see if start of a comment
                    if (!this.EnsureChars(1, false))
                    {
                        return false;
                    }

                    char nextChart = this._chars[this._charPos + 1];

                    return (nextChart == '*' || nextChart == '/');

                case ')':
                    if (this.CurrentState == State.Constructor || this.CurrentState == State.ConstructorStart)
                    {
                        return true;
                    }
                    break;

                case ' ':
                case StringUtils.Tab:
                case StringUtils.LineFeed:
                case StringUtils.CarriageReturn:
                    return true;

                default:
                    if (char.IsWhiteSpace(c))
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }

        private void ParseTrue()
        {
            // check characters equal 'true'
            // and that it is followed by either a separator character
            // or the text ends
            if (this.MatchValueWithTrailingSeparator(JsonConvert.True))
            {
                this.SetToken(JsonToken.Boolean, true);
            }
            else
            {
                throw JsonReaderException.Create(this, "Error parsing boolean value.");
            }
        }

        private void ParseNull()
        {
            if (this.MatchValueWithTrailingSeparator(JsonConvert.Null))
            {
                this.SetToken(JsonToken.Null);
            }
            else
            {
                throw JsonReaderException.Create(this, "Error parsing null value.");
            }
        }

        private void ParseUndefined()
        {
            if (this.MatchValueWithTrailingSeparator(JsonConvert.Undefined))
            {
                this.SetToken(JsonToken.Undefined);
            }
            else
            {
                throw JsonReaderException.Create(this, "Error parsing undefined value.");
            }
        }

        private void ParseFalse()
        {
            if (this.MatchValueWithTrailingSeparator(JsonConvert.False))
            {
                this.SetToken(JsonToken.Boolean, false);
            }
            else
            {
                throw JsonReaderException.Create(this, "Error parsing boolean value.");
            }
        }

        private object ParseNumberNegativeInfinity(ReadType readType)
        {
            return this.ParseNumberNegativeInfinity(readType, this.MatchValueWithTrailingSeparator(JsonConvert.NegativeInfinity));
        }

        private object ParseNumberNegativeInfinity(ReadType readType, bool matched)
        {
            if (matched)
            {
                switch (readType)
                {
                    case ReadType.Read:
                    case ReadType.ReadAsDouble:
                        if (this._floatParseHandling == FloatParseHandling.Double)
                        {
                            this.SetToken(JsonToken.Float, double.NegativeInfinity);
                            return double.NegativeInfinity;
                        }
                        break;

                    case ReadType.ReadAsString:
                        this.SetToken(JsonToken.String, JsonConvert.NegativeInfinity);
                        return JsonConvert.NegativeInfinity;
                }

                throw JsonReaderException.Create(this, "Cannot read -Infinity value.");
            }

            throw JsonReaderException.Create(this, "Error parsing -Infinity value.");
        }

        private object ParseNumberPositiveInfinity(ReadType readType)
        {
            return this.ParseNumberPositiveInfinity(readType, this.MatchValueWithTrailingSeparator(JsonConvert.PositiveInfinity));
        }

        private object ParseNumberPositiveInfinity(ReadType readType, bool matched)
        {
            if (matched)
            {
                switch (readType)
                {
                    case ReadType.Read:
                    case ReadType.ReadAsDouble:
                        if (this._floatParseHandling == FloatParseHandling.Double)
                        {
                            this.SetToken(JsonToken.Float, double.PositiveInfinity);
                            return double.PositiveInfinity;
                        }
                        break;

                    case ReadType.ReadAsString:
                        this.SetToken(JsonToken.String, JsonConvert.PositiveInfinity);
                        return JsonConvert.PositiveInfinity;
                }

                throw JsonReaderException.Create(this, "Cannot read Infinity value.");
            }

            throw JsonReaderException.Create(this, "Error parsing Infinity value.");
        }

        private object ParseNumberNaN(ReadType readType)
        {
            return this.ParseNumberNaN(readType, this.MatchValueWithTrailingSeparator(JsonConvert.NaN));
        }

        private object ParseNumberNaN(ReadType readType, bool matched)
        {
            if (matched)
            {
                switch (readType)
                {
                    case ReadType.Read:
                    case ReadType.ReadAsDouble:
                        if (this._floatParseHandling == FloatParseHandling.Double)
                        {
                            this.SetToken(JsonToken.Float, double.NaN);
                            return double.NaN;
                        }
                        break;

                    case ReadType.ReadAsString:
                        this.SetToken(JsonToken.String, JsonConvert.NaN);
                        return JsonConvert.NaN;
                }

                throw JsonReaderException.Create(this, "Cannot read NaN value.");
            }

            throw JsonReaderException.Create(this, "Error parsing NaN value.");
        }

        /// <summary>
        /// Changes the reader's state to <see cref="JsonReader.State.Closed"/>.
        /// If <see cref="JsonReader.CloseInput"/> is set to <c>true</c>, the underlying <see cref="TextReader"/> is also closed.
        /// </summary>
        public override void Close()
        {
            base.Close();

            if (this._chars != null)
            {
                BufferUtils.ReturnBuffer(this._arrayPool, this._chars);
                this._chars = null;
            }

            if (this.CloseInput)
            {
#if HAVE_STREAM_READER_WRITER_CLOSE
                _reader?.Close();
#else
                this._reader?.Dispose();
#endif
            }

            this._stringBuffer.Clear(this._arrayPool);
        }

        /// <summary>
        /// Gets a value indicating whether the class can return line information.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if <see cref="JsonTextReader.LineNumber"/> and <see cref="JsonTextReader.LinePosition"/> can be provided; otherwise, <c>false</c>.
        /// </returns>
        public bool HasLineInfo()
        {
            return true;
        }

        /// <summary>
        /// Gets the current line number.
        /// </summary>
        /// <value>
        /// The current line number or 0 if no line information is available (for example, <see cref="JsonTextReader.HasLineInfo"/> returns <c>false</c>).
        /// </value>
        public int LineNumber
        {
            get
            {
                if (this.CurrentState == State.Start && this.LinePosition == 0 && this.TokenType != JsonToken.Comment)
                {
                    return 0;
                }

                return this._lineNumber;
            }
        }

        /// <summary>
        /// Gets the current line position.
        /// </summary>
        /// <value>
        /// The current line position or 0 if no line information is available (for example, <see cref="JsonTextReader.HasLineInfo"/> returns <c>false</c>).
        /// </value>
        public int LinePosition => this._charPos - this._lineStartPos;
    }
}