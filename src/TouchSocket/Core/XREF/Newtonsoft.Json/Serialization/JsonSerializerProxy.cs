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
using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities;

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Serialization
{
    internal class JsonSerializerProxy : JsonSerializer
    {
        private readonly JsonSerializerInternalReader _serializerReader;
        private readonly JsonSerializerInternalWriter _serializerWriter;
        private readonly JsonSerializer _serializer;

        public override event System.EventHandler<ErrorEventArgs> Error
        {
            add => this._serializer.Error += value;
            remove => this._serializer.Error -= value;
        }

        public override IReferenceResolver ReferenceResolver
        {
            get => this._serializer.ReferenceResolver;
            set => this._serializer.ReferenceResolver = value;
        }

        public override ITraceWriter TraceWriter
        {
            get => this._serializer.TraceWriter;
            set => this._serializer.TraceWriter = value;
        }

        public override IEqualityComparer EqualityComparer
        {
            get => this._serializer.EqualityComparer;
            set => this._serializer.EqualityComparer = value;
        }

        public override JsonConverterCollection Converters => this._serializer.Converters;

        public override DefaultValueHandling DefaultValueHandling
        {
            get => this._serializer.DefaultValueHandling;
            set => this._serializer.DefaultValueHandling = value;
        }

        public override IContractResolver ContractResolver
        {
            get => this._serializer.ContractResolver;
            set => this._serializer.ContractResolver = value;
        }

        public override MissingMemberHandling MissingMemberHandling
        {
            get => this._serializer.MissingMemberHandling;
            set => this._serializer.MissingMemberHandling = value;
        }

        public override NullValueHandling NullValueHandling
        {
            get => this._serializer.NullValueHandling;
            set => this._serializer.NullValueHandling = value;
        }

        public override ObjectCreationHandling ObjectCreationHandling
        {
            get => this._serializer.ObjectCreationHandling;
            set => this._serializer.ObjectCreationHandling = value;
        }

        public override ReferenceLoopHandling ReferenceLoopHandling
        {
            get => this._serializer.ReferenceLoopHandling;
            set => this._serializer.ReferenceLoopHandling = value;
        }

        public override PreserveReferencesHandling PreserveReferencesHandling
        {
            get => this._serializer.PreserveReferencesHandling;
            set => this._serializer.PreserveReferencesHandling = value;
        }

        public override TypeNameHandling TypeNameHandling
        {
            get => this._serializer.TypeNameHandling;
            set => this._serializer.TypeNameHandling = value;
        }

        public override MetadataPropertyHandling MetadataPropertyHandling
        {
            get => this._serializer.MetadataPropertyHandling;
            set => this._serializer.MetadataPropertyHandling = value;
        }

        [Obsolete("TypeNameAssemblyFormat is obsolete. Use TypeNameAssemblyFormatHandling instead.")]
        public override FormatterAssemblyStyle TypeNameAssemblyFormat
        {
            get => this._serializer.TypeNameAssemblyFormat;
            set => this._serializer.TypeNameAssemblyFormat = value;
        }

        public override TypeNameAssemblyFormatHandling TypeNameAssemblyFormatHandling
        {
            get => this._serializer.TypeNameAssemblyFormatHandling;
            set => this._serializer.TypeNameAssemblyFormatHandling = value;
        }

        public override ConstructorHandling ConstructorHandling
        {
            get => this._serializer.ConstructorHandling;
            set => this._serializer.ConstructorHandling = value;
        }

        [Obsolete("Binder is obsolete. Use SerializationBinder instead.")]
        public override SerializationBinder Binder
        {
            get => this._serializer.Binder;
            set => this._serializer.Binder = value;
        }

        public override ISerializationBinder SerializationBinder
        {
            get => this._serializer.SerializationBinder;
            set => this._serializer.SerializationBinder = value;
        }

        public override StreamingContext Context
        {
            get => this._serializer.Context;
            set => this._serializer.Context = value;
        }

        public override Formatting Formatting
        {
            get => this._serializer.Formatting;
            set => this._serializer.Formatting = value;
        }

        public override DateFormatHandling DateFormatHandling
        {
            get => this._serializer.DateFormatHandling;
            set => this._serializer.DateFormatHandling = value;
        }

        public override DateTimeZoneHandling DateTimeZoneHandling
        {
            get => this._serializer.DateTimeZoneHandling;
            set => this._serializer.DateTimeZoneHandling = value;
        }

        public override DateParseHandling DateParseHandling
        {
            get => this._serializer.DateParseHandling;
            set => this._serializer.DateParseHandling = value;
        }

        public override FloatFormatHandling FloatFormatHandling
        {
            get => this._serializer.FloatFormatHandling;
            set => this._serializer.FloatFormatHandling = value;
        }

        public override FloatParseHandling FloatParseHandling
        {
            get => this._serializer.FloatParseHandling;
            set => this._serializer.FloatParseHandling = value;
        }

        public override StringEscapeHandling StringEscapeHandling
        {
            get => this._serializer.StringEscapeHandling;
            set => this._serializer.StringEscapeHandling = value;
        }

        public override string DateFormatString
        {
            get => this._serializer.DateFormatString;
            set => this._serializer.DateFormatString = value;
        }

        public override CultureInfo Culture
        {
            get => this._serializer.Culture;
            set => this._serializer.Culture = value;
        }

        public override int? MaxDepth
        {
            get => this._serializer.MaxDepth;
            set => this._serializer.MaxDepth = value;
        }

        public override bool CheckAdditionalContent
        {
            get => this._serializer.CheckAdditionalContent;
            set => this._serializer.CheckAdditionalContent = value;
        }

        internal JsonSerializerInternalBase GetInternalSerializer()
        {
            if (this._serializerReader != null)
            {
                return this._serializerReader;
            }
            else
            {
                return this._serializerWriter;
            }
        }

        public JsonSerializerProxy(JsonSerializerInternalReader serializerReader)
        {
            ValidationUtils.ArgumentNotNull(serializerReader, nameof(serializerReader));

            this._serializerReader = serializerReader;
            this._serializer = serializerReader.Serializer;
        }

        public JsonSerializerProxy(JsonSerializerInternalWriter serializerWriter)
        {
            ValidationUtils.ArgumentNotNull(serializerWriter, nameof(serializerWriter));

            this._serializerWriter = serializerWriter;
            this._serializer = serializerWriter.Serializer;
        }

        internal override object DeserializeInternal(JsonReader reader, Type objectType)
        {
            if (this._serializerReader != null)
            {
                return this._serializerReader.Deserialize(reader, objectType, false);
            }
            else
            {
                return this._serializer.Deserialize(reader, objectType);
            }
        }

        internal override void PopulateInternal(JsonReader reader, object target)
        {
            if (this._serializerReader != null)
            {
                this._serializerReader.Populate(reader, target);
            }
            else
            {
                this._serializer.Populate(reader, target);
            }
        }

        internal override void SerializeInternal(JsonWriter jsonWriter, object value, Type rootType)
        {
            if (this._serializerWriter != null)
            {
                this._serializerWriter.Serialize(jsonWriter, value, rootType);
            }
            else
            {
                this._serializer.Serialize(jsonWriter, value);
            }
        }
    }
}