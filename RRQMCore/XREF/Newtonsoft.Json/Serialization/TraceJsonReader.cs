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

using System;
using System.Globalization;
using System.IO;

namespace RRQMCore.XREF.Newtonsoft.Json.Serialization
{
    internal class TraceJsonReader : JsonReader, IJsonLineInfo
    {
        private readonly JsonReader _innerReader;
        private readonly JsonTextWriter _textWriter;
        private readonly StringWriter _sw;

        public TraceJsonReader(JsonReader innerReader)
        {
            _innerReader = innerReader;

            _sw = new StringWriter(CultureInfo.InvariantCulture);
            // prefix the message in the stringwriter to avoid concat with a potentially large JSON string
            _sw.Write("Deserialized JSON: " + Environment.NewLine);

            _textWriter = new JsonTextWriter(_sw);
            _textWriter.Formatting = Formatting.Indented;
        }

        public string GetDeserializedJsonMessage()
        {
            return _sw.ToString();
        }

        public override bool Read()
        {
            bool value = _innerReader.Read();
            WriteCurrentToken();
            return value;
        }

        public override int? ReadAsInt32()
        {
            int? value = _innerReader.ReadAsInt32();
            WriteCurrentToken();
            return value;
        }

        public override string ReadAsString()
        {
            string value = _innerReader.ReadAsString();
            WriteCurrentToken();
            return value;
        }

        public override byte[] ReadAsBytes()
        {
            byte[] value = _innerReader.ReadAsBytes();
            WriteCurrentToken();
            return value;
        }

        public override decimal? ReadAsDecimal()
        {
            decimal? value = _innerReader.ReadAsDecimal();
            WriteCurrentToken();
            return value;
        }

        public override double? ReadAsDouble()
        {
            double? value = _innerReader.ReadAsDouble();
            WriteCurrentToken();
            return value;
        }

        public override bool? ReadAsBoolean()
        {
            bool? value = _innerReader.ReadAsBoolean();
            WriteCurrentToken();
            return value;
        }

        public override DateTime? ReadAsDateTime()
        {
            DateTime? value = _innerReader.ReadAsDateTime();
            WriteCurrentToken();
            return value;
        }

#if HAVE_DATE_TIME_OFFSET
        public override DateTimeOffset? ReadAsDateTimeOffset()
        {
            DateTimeOffset? value = _innerReader.ReadAsDateTimeOffset();
            WriteCurrentToken();
            return value;
        }
#endif

        public void WriteCurrentToken()
        {
            _textWriter.WriteToken(_innerReader, false, false, true);
        }

        public override int Depth => _innerReader.Depth;

        public override string Path => _innerReader.Path;

        public override char QuoteChar
        {
            get => _innerReader.QuoteChar;
            protected internal set => _innerReader.QuoteChar = value;
        }

        public override JsonToken TokenType => _innerReader.TokenType;

        public override object Value => _innerReader.Value;

        public override Type ValueType => _innerReader.ValueType;

        public override void Close()
        {
            _innerReader.Close();
        }

        bool IJsonLineInfo.HasLineInfo()
        {
            return _innerReader is IJsonLineInfo lineInfo && lineInfo.HasLineInfo();
        }

        int IJsonLineInfo.LineNumber
        {
            get
            {
                return (_innerReader is IJsonLineInfo lineInfo) ? lineInfo.LineNumber : 0;
            }
        }

        int IJsonLineInfo.LinePosition
        {
            get
            {
                return (_innerReader is IJsonLineInfo lineInfo) ? lineInfo.LinePosition : 0;
            }
        }
    }
}