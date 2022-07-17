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

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Serialization
{
    internal class TraceJsonReader : JsonReader, IJsonLineInfo
    {
        private readonly JsonReader _innerReader;
        private readonly JsonTextWriter _textWriter;
        private readonly StringWriter _sw;

        public TraceJsonReader(JsonReader innerReader)
        {
            this._innerReader = innerReader;

            this._sw = new StringWriter(CultureInfo.InvariantCulture);
            // prefix the message in the stringwriter to avoid concat with a potentially large JSON string
            this._sw.Write("Deserialized JSON: " + Environment.NewLine);

            this._textWriter = new JsonTextWriter(this._sw);
            this._textWriter.Formatting = Formatting.Indented;
        }

        public string GetDeserializedJsonMessage()
        {
            return this._sw.ToString();
        }

        public override bool Read()
        {
            bool value = this._innerReader.Read();
            this.WriteCurrentToken();
            return value;
        }

        public override int? ReadAsInt32()
        {
            int? value = this._innerReader.ReadAsInt32();
            this.WriteCurrentToken();
            return value;
        }

        public override string ReadAsString()
        {
            string value = this._innerReader.ReadAsString();
            this.WriteCurrentToken();
            return value;
        }

        public override byte[] ReadAsBytes()
        {
            byte[] value = this._innerReader.ReadAsBytes();
            this.WriteCurrentToken();
            return value;
        }

        public override decimal? ReadAsDecimal()
        {
            decimal? value = this._innerReader.ReadAsDecimal();
            this.WriteCurrentToken();
            return value;
        }

        public override double? ReadAsDouble()
        {
            double? value = this._innerReader.ReadAsDouble();
            this.WriteCurrentToken();
            return value;
        }

        public override bool? ReadAsBoolean()
        {
            bool? value = this._innerReader.ReadAsBoolean();
            this.WriteCurrentToken();
            return value;
        }

        public override DateTime? ReadAsDateTime()
        {
            DateTime? value = this._innerReader.ReadAsDateTime();
            this.WriteCurrentToken();
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
            this._textWriter.WriteToken(this._innerReader, false, false, true);
        }

        public override int Depth => this._innerReader.Depth;

        public override string Path => this._innerReader.Path;

        public override char QuoteChar
        {
            get => this._innerReader.QuoteChar;
            protected internal set => this._innerReader.QuoteChar = value;
        }

        public override JsonToken TokenType => this._innerReader.TokenType;

        public override object Value => this._innerReader.Value;

        public override Type ValueType => this._innerReader.ValueType;

        public override void Close()
        {
            this._innerReader.Close();
        }

        bool IJsonLineInfo.HasLineInfo()
        {
            return this._innerReader is IJsonLineInfo lineInfo && lineInfo.HasLineInfo();
        }

        int IJsonLineInfo.LineNumber => (this._innerReader is IJsonLineInfo lineInfo) ? lineInfo.LineNumber : 0;

        int IJsonLineInfo.LinePosition => (this._innerReader is IJsonLineInfo lineInfo) ? lineInfo.LinePosition : 0;
    }
}