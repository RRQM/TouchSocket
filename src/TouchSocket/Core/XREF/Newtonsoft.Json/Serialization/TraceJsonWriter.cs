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
using System.Globalization;
using System.IO;

#if HAVE_BIG_INTEGER
using System.Numerics;
#endif

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Serialization
{
    internal class TraceJsonWriter : JsonWriter
    {
        private readonly JsonWriter _innerWriter;
        private readonly JsonTextWriter _textWriter;
        private readonly StringWriter _sw;

        public TraceJsonWriter(JsonWriter innerWriter)
        {
            this._innerWriter = innerWriter;

            this._sw = new StringWriter(CultureInfo.InvariantCulture);
            // prefix the message in the stringwriter to avoid concat with a potentially large JSON string
            this._sw.Write("Serialized JSON: " + Environment.NewLine);

            this._textWriter = new JsonTextWriter(this._sw);
            this._textWriter.Formatting = Formatting.Indented;
            this._textWriter.Culture = innerWriter.Culture;
            this._textWriter.DateFormatHandling = innerWriter.DateFormatHandling;
            this._textWriter.DateFormatString = innerWriter.DateFormatString;
            this._textWriter.DateTimeZoneHandling = innerWriter.DateTimeZoneHandling;
            this._textWriter.FloatFormatHandling = innerWriter.FloatFormatHandling;
        }

        public string GetSerializedJsonMessage()
        {
            return this._sw.ToString();
        }

        public override void WriteValue(decimal value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(decimal? value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteUndefined();
            }
        }

        public override void WriteValue(bool value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(bool? value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteUndefined();
            }
        }

        public override void WriteValue(byte value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(byte? value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteUndefined();
            }
        }

        public override void WriteValue(char value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(char? value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteUndefined();
            }
        }

        public override void WriteValue(byte[] value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            if (value == null)
            {
                base.WriteUndefined();
            }
            else
            {
                base.WriteValue(value);
            }
        }

        public override void WriteValue(DateTime value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(DateTime? value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteUndefined();
            }
        }

#if HAVE_DATE_TIME_OFFSET
        public override void WriteValue(DateTimeOffset value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(DateTimeOffset? value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteUndefined();
            }
        }
#endif

        public override void WriteValue(double value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(double? value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteUndefined();
            }
        }

        public override void WriteUndefined()
        {
            this._textWriter.WriteUndefined();
            this._innerWriter.WriteUndefined();
            base.WriteUndefined();
        }

        public override void WriteNull()
        {
            this._textWriter.WriteNull();
            this._innerWriter.WriteNull();
            base.WriteUndefined();
        }

        public override void WriteValue(float value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(float? value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteUndefined();
            }
        }

        public override void WriteValue(Guid value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(Guid? value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteUndefined();
            }
        }

        public override void WriteValue(int value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(int? value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteUndefined();
            }
        }

        public override void WriteValue(long value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(long? value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteUndefined();
            }
        }

        public override void WriteValue(object value)
        {
#if HAVE_BIG_INTEGER
            if (value is BigInteger)
            {
                _textWriter.WriteValue(value);
                _innerWriter.WriteValue(value);
                InternalWriteValue(JsonToken.Integer);
            }
            else
#endif
            {
                this._textWriter.WriteValue(value);
                this._innerWriter.WriteValue(value);
                if (value == null)
                {
                    base.WriteUndefined();
                }
                else
                {
                    // base.WriteValue(value) will error
                    this.InternalWriteValue(JsonToken.String);
                }
            }
        }

        public override void WriteValue(sbyte value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(sbyte? value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteUndefined();
            }
        }

        public override void WriteValue(short value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(short? value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteUndefined();
            }
        }

        public override void WriteValue(string value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(TimeSpan value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(TimeSpan? value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteUndefined();
            }
        }

        public override void WriteValue(uint value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(uint? value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteUndefined();
            }
        }

        public override void WriteValue(ulong value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(ulong? value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteUndefined();
            }
        }

        public override void WriteValue(Uri value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            if (value == null)
            {
                base.WriteUndefined();
            }
            else
            {
                base.WriteValue(value);
            }
        }

        public override void WriteValue(ushort value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(ushort? value)
        {
            this._textWriter.WriteValue(value);
            this._innerWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteUndefined();
            }
        }

        public override void WriteWhitespace(string ws)
        {
            this._textWriter.WriteWhitespace(ws);
            this._innerWriter.WriteWhitespace(ws);
            base.WriteWhitespace(ws);
        }

        public override void WriteComment(string text)
        {
            this._textWriter.WriteComment(text);
            this._innerWriter.WriteComment(text);
            base.WriteComment(text);
        }

        public override void WriteStartArray()
        {
            this._textWriter.WriteStartArray();
            this._innerWriter.WriteStartArray();
            base.WriteStartArray();
        }

        public override void WriteEndArray()
        {
            this._textWriter.WriteEndArray();
            this._innerWriter.WriteEndArray();
            base.WriteEndArray();
        }

        public override void WriteStartConstructor(string name)
        {
            this._textWriter.WriteStartConstructor(name);
            this._innerWriter.WriteStartConstructor(name);
            base.WriteStartConstructor(name);
        }

        public override void WriteEndConstructor()
        {
            this._textWriter.WriteEndConstructor();
            this._innerWriter.WriteEndConstructor();
            base.WriteEndConstructor();
        }

        public override void WritePropertyName(string name)
        {
            this._textWriter.WritePropertyName(name);
            this._innerWriter.WritePropertyName(name);
            base.WritePropertyName(name);
        }

        public override void WritePropertyName(string name, bool escape)
        {
            this._textWriter.WritePropertyName(name, escape);
            this._innerWriter.WritePropertyName(name, escape);

            // method with escape will error
            base.WritePropertyName(name);
        }

        public override void WriteStartObject()
        {
            this._textWriter.WriteStartObject();
            this._innerWriter.WriteStartObject();
            base.WriteStartObject();
        }

        public override void WriteEndObject()
        {
            this._textWriter.WriteEndObject();
            this._innerWriter.WriteEndObject();
            base.WriteEndObject();
        }

        public override void WriteRawValue(string json)
        {
            this._textWriter.WriteRawValue(json);
            this._innerWriter.WriteRawValue(json);

            // calling base method will write json twice
            this.InternalWriteValue(JsonToken.Undefined);
        }

        public override void WriteRaw(string json)
        {
            this._textWriter.WriteRaw(json);
            this._innerWriter.WriteRaw(json);
            base.WriteRaw(json);
        }

        public override void Close()
        {
            this._textWriter.Close();
            this._innerWriter.Close();
            base.Close();
        }

        public override void Flush()
        {
            this._textWriter.Flush();
            this._innerWriter.Flush();
        }
    }
}