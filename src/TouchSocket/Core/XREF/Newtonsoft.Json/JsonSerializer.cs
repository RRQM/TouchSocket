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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using TouchSocket.Core.XREF.Newtonsoft.Json.Serialization;
using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities;
using ErrorEventArgs = TouchSocket.Core.XREF.Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace TouchSocket.Core.XREF.Newtonsoft.Json
{
    /// <summary>
    /// Serializes and deserializes objects into and from the JSON format.
    /// The <see cref="JsonSerializer"/> enables you to control how objects are encoded into JSON.
    /// </summary>
    public class JsonSerializer
    {
        internal TypeNameHandling _typeNameHandling;
        internal TypeNameAssemblyFormatHandling _typeNameAssemblyFormatHandling;
        internal PreserveReferencesHandling _preserveReferencesHandling;
        internal ReferenceLoopHandling _referenceLoopHandling;
        internal MissingMemberHandling _missingMemberHandling;
        internal ObjectCreationHandling _objectCreationHandling;
        internal NullValueHandling _nullValueHandling;
        internal DefaultValueHandling _defaultValueHandling;
        internal ConstructorHandling _constructorHandling;
        internal MetadataPropertyHandling _metadataPropertyHandling;
        internal JsonConverterCollection _converters;
        internal IContractResolver _contractResolver;
        internal ITraceWriter _traceWriter;
        internal IEqualityComparer _equalityComparer;
        internal ISerializationBinder _serializationBinder;
        internal StreamingContext _context;
        private IReferenceResolver _referenceResolver;

        private Formatting? _formatting;
        private DateFormatHandling? _dateFormatHandling;
        private DateTimeZoneHandling? _dateTimeZoneHandling;
        private DateParseHandling? _dateParseHandling;
        private FloatFormatHandling? _floatFormatHandling;
        private FloatParseHandling? _floatParseHandling;
        private StringEscapeHandling? _stringEscapeHandling;
        private CultureInfo _culture;
        private int? _maxDepth;
        private bool _maxDepthSet;
        private bool? _checkAdditionalContent;
        private string _dateFormatString;
        private bool _dateFormatStringSet;

        /// <summary>
        /// Occurs when the <see cref="JsonSerializer"/> errors during serialization and deserialization.
        /// </summary>
        public virtual event System.EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// Gets or sets the <see cref="IReferenceResolver"/> used by the serializer when resolving references.
        /// </summary>
        public virtual IReferenceResolver ReferenceResolver
        {
            get => this.GetReferenceResolver();
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Reference resolver cannot be null.");
                }

                this._referenceResolver = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="SerializationBinder"/> used by the serializer when resolving type names.
        /// </summary>
        [Obsolete("Binder is obsolete. Use SerializationBinder instead.")]
        public virtual SerializationBinder Binder
        {
            get
            {
                if (this._serializationBinder == null)
                {
                    return null;
                }

                if (this._serializationBinder is SerializationBinder legacySerializationBinder)
                {
                    return legacySerializationBinder;
                }

                if (this._serializationBinder is SerializationBinderAdapter adapter)
                {
                    return adapter.SerializationBinder;
                }

                throw new InvalidOperationException("Cannot get SerializationBinder because an ISerializationBinder was previously set.");
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Serialization binder cannot be null.");
                }

                this._serializationBinder = value as ISerializationBinder ?? new SerializationBinderAdapter(value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ISerializationBinder"/> used by the serializer when resolving type names.
        /// </summary>
        public virtual ISerializationBinder SerializationBinder
        {
            get => this._serializationBinder;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Serialization binder cannot be null.");
                }

                this._serializationBinder = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ITraceWriter"/> used by the serializer when writing trace messages.
        /// </summary>
        /// <value>The trace writer.</value>
        public virtual ITraceWriter TraceWriter
        {
            get => this._traceWriter;
            set => this._traceWriter = value;
        }

        /// <summary>
        /// Gets or sets the equality comparer used by the serializer when comparing references.
        /// </summary>
        /// <value>The equality comparer.</value>
        public virtual IEqualityComparer EqualityComparer
        {
            get => this._equalityComparer;
            set => this._equalityComparer = value;
        }

        /// <summary>
        /// Gets or sets how type name writing and reading is handled by the serializer.
        /// The default value is <see cref="Json.TypeNameHandling.None" />.
        /// </summary>
        /// <remarks>
        /// <see cref="JsonSerializer.TypeNameHandling"/> should be used with caution when your application deserializes JSON from an external source.
        /// Incoming types should be validated with a custom <see cref="JsonSerializer.SerializationBinder"/>
        /// when deserializing with a value other than <see cref="TypeNameHandling.None"/>.
        /// </remarks>
        public virtual TypeNameHandling TypeNameHandling
        {
            get => this._typeNameHandling;
            set
            {
                if (value < TypeNameHandling.None || value > TypeNameHandling.Auto)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this._typeNameHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how a type name assembly is written and resolved by the serializer.
        /// The default value is <see cref="FormatterAssemblyStyle.Simple" />.
        /// </summary>
        /// <value>The type name assembly format.</value>
        [Obsolete("TypeNameAssemblyFormat is obsolete. Use TypeNameAssemblyFormatHandling instead.")]
        public virtual FormatterAssemblyStyle TypeNameAssemblyFormat
        {
            get => (FormatterAssemblyStyle)this._typeNameAssemblyFormatHandling;
            set
            {
                if (value < FormatterAssemblyStyle.Simple || value > FormatterAssemblyStyle.Full)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this._typeNameAssemblyFormatHandling = (TypeNameAssemblyFormatHandling)value;
            }
        }

        /// <summary>
        /// Gets or sets how a type name assembly is written and resolved by the serializer.
        /// The default value is <see cref="Json.TypeNameAssemblyFormatHandling.Simple" />.
        /// </summary>
        /// <value>The type name assembly format.</value>
        public virtual TypeNameAssemblyFormatHandling TypeNameAssemblyFormatHandling
        {
            get => this._typeNameAssemblyFormatHandling;
            set
            {
                if (value < TypeNameAssemblyFormatHandling.Simple || value > TypeNameAssemblyFormatHandling.Full)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this._typeNameAssemblyFormatHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how object references are preserved by the serializer.
        /// The default value is <see cref="Json.PreserveReferencesHandling.None" />.
        /// </summary>
        public virtual PreserveReferencesHandling PreserveReferencesHandling
        {
            get => this._preserveReferencesHandling;
            set
            {
                if (value < PreserveReferencesHandling.None || value > PreserveReferencesHandling.All)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this._preserveReferencesHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how reference loops (e.g. a class referencing itself) is handled.
        /// The default value is <see cref="Json.ReferenceLoopHandling.Error" />.
        /// </summary>
        public virtual ReferenceLoopHandling ReferenceLoopHandling
        {
            get => this._referenceLoopHandling;
            set
            {
                if (value < ReferenceLoopHandling.Error || value > ReferenceLoopHandling.Serialize)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this._referenceLoopHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how missing members (e.g. JSON contains a property that isn't a member on the object) are handled during deserialization.
        /// The default value is <see cref="Json.MissingMemberHandling.Ignore" />.
        /// </summary>
        public virtual MissingMemberHandling MissingMemberHandling
        {
            get => this._missingMemberHandling;
            set
            {
                if (value < MissingMemberHandling.Ignore || value > MissingMemberHandling.Error)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this._missingMemberHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how null values are handled during serialization and deserialization.
        /// The default value is <see cref="Json.NullValueHandling.Include" />.
        /// </summary>
        public virtual NullValueHandling NullValueHandling
        {
            get => this._nullValueHandling;
            set
            {
                if (value < NullValueHandling.Include || value > NullValueHandling.Ignore)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this._nullValueHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how default values are handled during serialization and deserialization.
        /// The default value is <see cref="Json.DefaultValueHandling.Include" />.
        /// </summary>
        public virtual DefaultValueHandling DefaultValueHandling
        {
            get => this._defaultValueHandling;
            set
            {
                if (value < DefaultValueHandling.Include || value > DefaultValueHandling.IgnoreAndPopulate)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this._defaultValueHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how objects are created during deserialization.
        /// The default value is <see cref="Json.ObjectCreationHandling.Auto" />.
        /// </summary>
        /// <value>The object creation handling.</value>
        public virtual ObjectCreationHandling ObjectCreationHandling
        {
            get => this._objectCreationHandling;
            set
            {
                if (value < ObjectCreationHandling.Auto || value > ObjectCreationHandling.Replace)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this._objectCreationHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how constructors are used during deserialization.
        /// The default value is <see cref="Json.ConstructorHandling.Default" />.
        /// </summary>
        /// <value>The constructor handling.</value>
        public virtual ConstructorHandling ConstructorHandling
        {
            get => this._constructorHandling;
            set
            {
                if (value < ConstructorHandling.Default || value > ConstructorHandling.AllowNonPublicDefaultConstructor)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this._constructorHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how metadata properties are used during deserialization.
        /// The default value is <see cref="Json.MetadataPropertyHandling.Default" />.
        /// </summary>
        /// <value>The metadata properties handling.</value>
        public virtual MetadataPropertyHandling MetadataPropertyHandling
        {
            get => this._metadataPropertyHandling;
            set
            {
                if (value < MetadataPropertyHandling.Default || value > MetadataPropertyHandling.Ignore)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this._metadataPropertyHandling = value;
            }
        }

        /// <summary>
        /// Gets a collection <see cref="JsonConverter"/> that will be used during serialization.
        /// </summary>
        /// <value>Collection <see cref="JsonConverter"/> that will be used during serialization.</value>
        public virtual JsonConverterCollection Converters
        {
            get
            {
                if (this._converters == null)
                {
                    this._converters = new JsonConverterCollection();
                }

                return this._converters;
            }
        }

        /// <summary>
        /// Gets or sets the contract resolver used by the serializer when
        /// serializing .NET objects to JSON and vice versa.
        /// </summary>
        public virtual IContractResolver ContractResolver
        {
            get => this._contractResolver;
            set => this._contractResolver = value ?? DefaultContractResolver.Instance;
        }

        /// <summary>
        /// Gets or sets the <see cref="StreamingContext"/> used by the serializer when invoking serialization callback methods.
        /// </summary>
        /// <value>The context.</value>
        public virtual StreamingContext Context
        {
            get => this._context;
            set => this._context = value;
        }

        /// <summary>
        /// Indicates how JSON text output is formatted.
        /// The default value is <see cref="Json.Formatting.None" />.
        /// </summary>
        public virtual Formatting Formatting
        {
            get => this._formatting ?? JsonSerializerSettings.DefaultFormatting;
            set => this._formatting = value;
        }

        /// <summary>
        /// Gets or sets how dates are written to JSON text.
        /// The default value is <see cref="Json.DateFormatHandling.IsoDateFormat" />.
        /// </summary>
        public virtual DateFormatHandling DateFormatHandling
        {
            get => this._dateFormatHandling ?? JsonSerializerSettings.DefaultDateFormatHandling;
            set => this._dateFormatHandling = value;
        }

        /// <summary>
        /// Gets or sets how <see cref="DateTime"/> time zones are handled during serialization and deserialization.
        /// The default value is <see cref="Json.DateTimeZoneHandling.RoundtripKind" />.
        /// </summary>
        public virtual DateTimeZoneHandling DateTimeZoneHandling
        {
            get => this._dateTimeZoneHandling ?? JsonSerializerSettings.DefaultDateTimeZoneHandling;
            set => this._dateTimeZoneHandling = value;
        }

        /// <summary>
        /// Gets or sets how date formatted strings, e.g. <c>"\/Date(1198908717056)\/"</c> and <c>"2012-03-21T05:40Z"</c>, are parsed when reading JSON.
        /// The default value is <see cref="Json.DateParseHandling.DateTime" />.
        /// </summary>
        public virtual DateParseHandling DateParseHandling
        {
            get => this._dateParseHandling ?? JsonSerializerSettings.DefaultDateParseHandling;
            set => this._dateParseHandling = value;
        }

        /// <summary>
        /// Gets or sets how floating point numbers, e.g. 1.0 and 9.9, are parsed when reading JSON text.
        /// The default value is <see cref="Json.FloatParseHandling.Double" />.
        /// </summary>
        public virtual FloatParseHandling FloatParseHandling
        {
            get => this._floatParseHandling ?? JsonSerializerSettings.DefaultFloatParseHandling;
            set => this._floatParseHandling = value;
        }

        /// <summary>
        /// Gets or sets how special floating point numbers, e.g. <see cref="Double.NaN"/>,
        /// <see cref="Double.PositiveInfinity"/> and <see cref="Double.NegativeInfinity"/>,
        /// are written as JSON text.
        /// The default value is <see cref="Json.FloatFormatHandling.String" />.
        /// </summary>
        public virtual FloatFormatHandling FloatFormatHandling
        {
            get => this._floatFormatHandling ?? JsonSerializerSettings.DefaultFloatFormatHandling;
            set => this._floatFormatHandling = value;
        }

        /// <summary>
        /// Gets or sets how strings are escaped when writing JSON text.
        /// The default value is <see cref="Json.StringEscapeHandling.Default" />.
        /// </summary>
        public virtual StringEscapeHandling StringEscapeHandling
        {
            get => this._stringEscapeHandling ?? JsonSerializerSettings.DefaultStringEscapeHandling;
            set => this._stringEscapeHandling = value;
        }

        /// <summary>
        /// Gets or sets how <see cref="DateTime"/> and <see cref="DateTimeOffset"/> values are formatted when writing JSON text,
        /// and the expected date format when reading JSON text.
        /// The default value is <c>"yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK"</c>.
        /// </summary>
        public virtual string DateFormatString
        {
            get => this._dateFormatString ?? JsonSerializerSettings.DefaultDateFormatString;
            set
            {
                this._dateFormatString = value;
                this._dateFormatStringSet = true;
            }
        }

        /// <summary>
        /// Gets or sets the culture used when reading JSON.
        /// The default value is <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        public virtual CultureInfo Culture
        {
            get => this._culture ?? JsonSerializerSettings.DefaultCulture;
            set => this._culture = value;
        }

        /// <summary>
        /// Gets or sets the maximum depth allowed when reading JSON. Reading past this depth will throw a <see cref="JsonReaderException"/>.
        /// A null value means there is no maximum.
        /// The default value is <c>null</c>.
        /// </summary>
        public virtual int? MaxDepth
        {
            get => this._maxDepth;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Value must be positive.", nameof(value));
                }

                this._maxDepth = value;
                this._maxDepthSet = true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether there will be a check for additional JSON content after deserializing an object.
        /// The default value is <c>false</c>.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if there will be a check for additional JSON content after deserializing an object; otherwise, <c>false</c>.
        /// </value>
        public virtual bool CheckAdditionalContent
        {
            get => this._checkAdditionalContent ?? JsonSerializerSettings.DefaultCheckAdditionalContent;
            set => this._checkAdditionalContent = value;
        }

        internal bool IsCheckAdditionalContentSet()
        {
            return (this._checkAdditionalContent != null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializer"/> class.
        /// </summary>
        public JsonSerializer()
        {
            this._referenceLoopHandling = JsonSerializerSettings.DefaultReferenceLoopHandling;
            this._missingMemberHandling = JsonSerializerSettings.DefaultMissingMemberHandling;
            this._nullValueHandling = JsonSerializerSettings.DefaultNullValueHandling;
            this._defaultValueHandling = JsonSerializerSettings.DefaultDefaultValueHandling;
            this._objectCreationHandling = JsonSerializerSettings.DefaultObjectCreationHandling;
            this._preserveReferencesHandling = JsonSerializerSettings.DefaultPreserveReferencesHandling;
            this._constructorHandling = JsonSerializerSettings.DefaultConstructorHandling;
            this._typeNameHandling = JsonSerializerSettings.DefaultTypeNameHandling;
            this._metadataPropertyHandling = JsonSerializerSettings.DefaultMetadataPropertyHandling;
            this._context = JsonSerializerSettings.DefaultContext;
            this._serializationBinder = DefaultSerializationBinder.Instance;

            this._culture = JsonSerializerSettings.DefaultCulture;
            this._contractResolver = DefaultContractResolver.Instance;
        }

        /// <summary>
        /// Creates a new <see cref="JsonSerializer"/> instance.
        /// The <see cref="JsonSerializer"/> will not use default settings
        /// from <see cref="JsonConvert.DefaultSettings"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="JsonSerializer"/> instance.
        /// The <see cref="JsonSerializer"/> will not use default settings
        /// from <see cref="JsonConvert.DefaultSettings"/>.
        /// </returns>
        public static JsonSerializer Create()
        {
            return new JsonSerializer();
        }

        /// <summary>
        /// Creates a new <see cref="JsonSerializer"/> instance using the specified <see cref="JsonSerializerSettings"/>.
        /// The <see cref="JsonSerializer"/> will not use default settings
        /// from <see cref="JsonConvert.DefaultSettings"/>.
        /// </summary>
        /// <param name="settings">The settings to be applied to the <see cref="JsonSerializer"/>.</param>
        /// <returns>
        /// A new <see cref="JsonSerializer"/> instance using the specified <see cref="JsonSerializerSettings"/>.
        /// The <see cref="JsonSerializer"/> will not use default settings
        /// from <see cref="JsonConvert.DefaultSettings"/>.
        /// </returns>
        public static JsonSerializer Create(JsonSerializerSettings settings)
        {
            JsonSerializer serializer = Create();

            if (settings != null)
            {
                ApplySerializerSettings(serializer, settings);
            }

            return serializer;
        }

        /// <summary>
        /// Creates a new <see cref="JsonSerializer"/> instance.
        /// The <see cref="JsonSerializer"/> will use default settings
        /// from <see cref="JsonConvert.DefaultSettings"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="JsonSerializer"/> instance.
        /// The <see cref="JsonSerializer"/> will use default settings
        /// from <see cref="JsonConvert.DefaultSettings"/>.
        /// </returns>
        public static JsonSerializer CreateDefault()
        {
            // copy static to local variable to avoid concurrency issues
            JsonSerializerSettings defaultSettings = JsonConvert.DefaultSettings?.Invoke();

            return Create(defaultSettings);
        }

        /// <summary>
        /// Creates a new <see cref="JsonSerializer"/> instance using the specified <see cref="JsonSerializerSettings"/>.
        /// The <see cref="JsonSerializer"/> will use default settings
        /// from <see cref="JsonConvert.DefaultSettings"/> as well as the specified <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="settings">The settings to be applied to the <see cref="JsonSerializer"/>.</param>
        /// <returns>
        /// A new <see cref="JsonSerializer"/> instance using the specified <see cref="JsonSerializerSettings"/>.
        /// The <see cref="JsonSerializer"/> will use default settings
        /// from <see cref="JsonConvert.DefaultSettings"/> as well as the specified <see cref="JsonSerializerSettings"/>.
        /// </returns>
        public static JsonSerializer CreateDefault(JsonSerializerSettings settings)
        {
            JsonSerializer serializer = CreateDefault();
            if (settings != null)
            {
                ApplySerializerSettings(serializer, settings);
            }

            return serializer;
        }

        private static void ApplySerializerSettings(JsonSerializer serializer, JsonSerializerSettings settings)
        {
            if (!CollectionUtils.IsNullOrEmpty(settings.Converters))
            {
                // insert settings converters at the beginning so they take precedence
                // if user wants to remove one of the default converters they will have to do it manually
                for (int i = 0; i < settings.Converters.Count; i++)
                {
                    serializer.Converters.Insert(i, settings.Converters[i]);
                }
            }

            // serializer specific
            if (settings._typeNameHandling != null)
            {
                serializer.TypeNameHandling = settings.TypeNameHandling;
            }
            if (settings._metadataPropertyHandling != null)
            {
                serializer.MetadataPropertyHandling = settings.MetadataPropertyHandling;
            }
            if (settings._typeNameAssemblyFormatHandling != null)
            {
                serializer.TypeNameAssemblyFormatHandling = settings.TypeNameAssemblyFormatHandling;
            }
            if (settings._preserveReferencesHandling != null)
            {
                serializer.PreserveReferencesHandling = settings.PreserveReferencesHandling;
            }
            if (settings._referenceLoopHandling != null)
            {
                serializer.ReferenceLoopHandling = settings.ReferenceLoopHandling;
            }
            if (settings._missingMemberHandling != null)
            {
                serializer.MissingMemberHandling = settings.MissingMemberHandling;
            }
            if (settings._objectCreationHandling != null)
            {
                serializer.ObjectCreationHandling = settings.ObjectCreationHandling;
            }
            if (settings._nullValueHandling != null)
            {
                serializer.NullValueHandling = settings.NullValueHandling;
            }
            if (settings._defaultValueHandling != null)
            {
                serializer.DefaultValueHandling = settings.DefaultValueHandling;
            }
            if (settings._constructorHandling != null)
            {
                serializer.ConstructorHandling = settings.ConstructorHandling;
            }
            if (settings._context != null)
            {
                serializer.Context = settings.Context;
            }
            if (settings._checkAdditionalContent != null)
            {
                serializer._checkAdditionalContent = settings._checkAdditionalContent;
            }

            if (settings.Error != null)
            {
                serializer.Error += settings.Error;
            }

            if (settings.ContractResolver != null)
            {
                serializer.ContractResolver = settings.ContractResolver;
            }
            if (settings.ReferenceResolverProvider != null)
            {
                serializer.ReferenceResolver = settings.ReferenceResolverProvider();
            }
            if (settings.TraceWriter != null)
            {
                serializer.TraceWriter = settings.TraceWriter;
            }
            if (settings.EqualityComparer != null)
            {
                serializer.EqualityComparer = settings.EqualityComparer;
            }
            if (settings.SerializationBinder != null)
            {
                serializer.SerializationBinder = settings.SerializationBinder;
            }

            // reader/writer specific
            // unset values won't override reader/writer set values
            if (settings._formatting != null)
            {
                serializer._formatting = settings._formatting;
            }
            if (settings._dateFormatHandling != null)
            {
                serializer._dateFormatHandling = settings._dateFormatHandling;
            }
            if (settings._dateTimeZoneHandling != null)
            {
                serializer._dateTimeZoneHandling = settings._dateTimeZoneHandling;
            }
            if (settings._dateParseHandling != null)
            {
                serializer._dateParseHandling = settings._dateParseHandling;
            }
            if (settings._dateFormatStringSet)
            {
                serializer._dateFormatString = settings._dateFormatString;
                serializer._dateFormatStringSet = settings._dateFormatStringSet;
            }
            if (settings._floatFormatHandling != null)
            {
                serializer._floatFormatHandling = settings._floatFormatHandling;
            }
            if (settings._floatParseHandling != null)
            {
                serializer._floatParseHandling = settings._floatParseHandling;
            }
            if (settings._stringEscapeHandling != null)
            {
                serializer._stringEscapeHandling = settings._stringEscapeHandling;
            }
            if (settings._culture != null)
            {
                serializer._culture = settings._culture;
            }
            if (settings._maxDepthSet)
            {
                serializer._maxDepth = settings._maxDepth;
                serializer._maxDepthSet = settings._maxDepthSet;
            }
        }

        /// <summary>
        /// Populates the JSON values onto the target object.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> that contains the JSON structure to reader values from.</param>
        /// <param name="target">The target object to populate values onto.</param>
        public void Populate(TextReader reader, object target)
        {
            this.Populate(new JsonTextReader(reader), target);
        }

        /// <summary>
        /// Populates the JSON values onto the target object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> that contains the JSON structure to reader values from.</param>
        /// <param name="target">The target object to populate values onto.</param>
        public void Populate(JsonReader reader, object target)
        {
            this.PopulateInternal(reader, target);
        }

        internal virtual void PopulateInternal(JsonReader reader, object target)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));
            ValidationUtils.ArgumentNotNull(target, nameof(target));

            // set serialization options onto reader
            CultureInfo previousCulture;
            DateTimeZoneHandling? previousDateTimeZoneHandling;
            DateParseHandling? previousDateParseHandling;
            FloatParseHandling? previousFloatParseHandling;
            int? previousMaxDepth;
            string previousDateFormatString;
            this.SetupReader(reader, out previousCulture, out previousDateTimeZoneHandling, out previousDateParseHandling, out previousFloatParseHandling, out previousMaxDepth, out previousDateFormatString);

            TraceJsonReader traceJsonReader = (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Verbose)
                ? this.CreateTraceJsonReader(reader)
                : null;

            JsonSerializerInternalReader serializerReader = new JsonSerializerInternalReader(this);
            serializerReader.Populate(traceJsonReader ?? reader, target);

            if (traceJsonReader != null)
            {
                this.TraceWriter.Trace(TraceLevel.Verbose, traceJsonReader.GetDeserializedJsonMessage(), null);
            }

            this.ResetReader(reader, previousCulture, previousDateTimeZoneHandling, previousDateParseHandling, previousFloatParseHandling, previousMaxDepth, previousDateFormatString);
        }

        /// <summary>
        /// Deserializes the JSON structure contained by the specified <see cref="JsonReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> that contains the JSON structure to deserialize.</param>
        /// <returns>The <see cref="Object"/> being deserialized.</returns>
        public object Deserialize(JsonReader reader)
        {
            return this.Deserialize(reader, null);
        }

        /// <summary>
        /// Deserializes the JSON structure contained by the specified <see cref="StringReader"/>
        /// into an instance of the specified type.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> containing the object.</param>
        /// <param name="objectType">The <see cref="Type"/> of object being deserialized.</param>
        /// <returns>The instance of <paramref name="objectType"/> being deserialized.</returns>
        public object Deserialize(TextReader reader, Type objectType)
        {
            return this.Deserialize(new JsonTextReader(reader), objectType);
        }

        /// <summary>
        /// Deserializes the JSON structure contained by the specified <see cref="JsonReader"/>
        /// into an instance of the specified type.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> containing the object.</param>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <returns>The instance of <typeparamref name="T"/> being deserialized.</returns>
        public T Deserialize<T>(JsonReader reader)
        {
            return (T)this.Deserialize(reader, typeof(T));
        }

        /// <summary>
        /// Deserializes the JSON structure contained by the specified <see cref="JsonReader"/>
        /// into an instance of the specified type.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> containing the object.</param>
        /// <param name="objectType">The <see cref="Type"/> of object being deserialized.</param>
        /// <returns>The instance of <paramref name="objectType"/> being deserialized.</returns>
        public object Deserialize(JsonReader reader, Type objectType)
        {
            return this.DeserializeInternal(reader, objectType);
        }

        internal virtual object DeserializeInternal(JsonReader reader, Type objectType)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            // set serialization options onto reader
            CultureInfo previousCulture;
            DateTimeZoneHandling? previousDateTimeZoneHandling;
            DateParseHandling? previousDateParseHandling;
            FloatParseHandling? previousFloatParseHandling;
            int? previousMaxDepth;
            string previousDateFormatString;
            this.SetupReader(reader, out previousCulture, out previousDateTimeZoneHandling, out previousDateParseHandling, out previousFloatParseHandling, out previousMaxDepth, out previousDateFormatString);

            TraceJsonReader traceJsonReader = (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Verbose)
                ? this.CreateTraceJsonReader(reader)
                : null;

            JsonSerializerInternalReader serializerReader = new JsonSerializerInternalReader(this);
            object value = serializerReader.Deserialize(traceJsonReader ?? reader, objectType, this.CheckAdditionalContent);

            if (traceJsonReader != null)
            {
                this.TraceWriter.Trace(TraceLevel.Verbose, traceJsonReader.GetDeserializedJsonMessage(), null);
            }

            this.ResetReader(reader, previousCulture, previousDateTimeZoneHandling, previousDateParseHandling, previousFloatParseHandling, previousMaxDepth, previousDateFormatString);

            return value;
        }

        private void SetupReader(JsonReader reader, out CultureInfo previousCulture, out DateTimeZoneHandling? previousDateTimeZoneHandling, out DateParseHandling? previousDateParseHandling, out FloatParseHandling? previousFloatParseHandling, out int? previousMaxDepth, out string previousDateFormatString)
        {
            if (this._culture != null && !this._culture.Equals(reader.Culture))
            {
                previousCulture = reader.Culture;
                reader.Culture = this._culture;
            }
            else
            {
                previousCulture = null;
            }

            if (this._dateTimeZoneHandling != null && reader.DateTimeZoneHandling != this._dateTimeZoneHandling)
            {
                previousDateTimeZoneHandling = reader.DateTimeZoneHandling;
                reader.DateTimeZoneHandling = this._dateTimeZoneHandling.GetValueOrDefault();
            }
            else
            {
                previousDateTimeZoneHandling = null;
            }

            if (this._dateParseHandling != null && reader.DateParseHandling != this._dateParseHandling)
            {
                previousDateParseHandling = reader.DateParseHandling;
                reader.DateParseHandling = this._dateParseHandling.GetValueOrDefault();
            }
            else
            {
                previousDateParseHandling = null;
            }

            if (this._floatParseHandling != null && reader.FloatParseHandling != this._floatParseHandling)
            {
                previousFloatParseHandling = reader.FloatParseHandling;
                reader.FloatParseHandling = this._floatParseHandling.GetValueOrDefault();
            }
            else
            {
                previousFloatParseHandling = null;
            }

            if (this._maxDepthSet && reader.MaxDepth != this._maxDepth)
            {
                previousMaxDepth = reader.MaxDepth;
                reader.MaxDepth = this._maxDepth;
            }
            else
            {
                previousMaxDepth = null;
            }

            if (this._dateFormatStringSet && reader.DateFormatString != this._dateFormatString)
            {
                previousDateFormatString = reader.DateFormatString;
                reader.DateFormatString = this._dateFormatString;
            }
            else
            {
                previousDateFormatString = null;
            }

            if (reader is JsonTextReader textReader)
            {
                if (this._contractResolver is DefaultContractResolver resolver)
                {
                    textReader.NameTable = resolver.GetNameTable();
                }
            }
        }

        private void ResetReader(JsonReader reader, CultureInfo previousCulture, DateTimeZoneHandling? previousDateTimeZoneHandling, DateParseHandling? previousDateParseHandling, FloatParseHandling? previousFloatParseHandling, int? previousMaxDepth, string previousDateFormatString)
        {
            // reset reader back to previous options
            if (previousCulture != null)
            {
                reader.Culture = previousCulture;
            }
            if (previousDateTimeZoneHandling != null)
            {
                reader.DateTimeZoneHandling = previousDateTimeZoneHandling.GetValueOrDefault();
            }
            if (previousDateParseHandling != null)
            {
                reader.DateParseHandling = previousDateParseHandling.GetValueOrDefault();
            }
            if (previousFloatParseHandling != null)
            {
                reader.FloatParseHandling = previousFloatParseHandling.GetValueOrDefault();
            }
            if (this._maxDepthSet)
            {
                reader.MaxDepth = previousMaxDepth;
            }
            if (this._dateFormatStringSet)
            {
                reader.DateFormatString = previousDateFormatString;
            }

            if (reader is JsonTextReader textReader)
            {
                textReader.NameTable = null;
            }
        }

        /// <summary>
        /// Serializes the specified <see cref="Object"/> and writes the JSON structure
        /// using the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> used to write the JSON structure.</param>
        /// <param name="value">The <see cref="Object"/> to serialize.</param>
        public void Serialize(TextWriter textWriter, object value)
        {
            this.Serialize(new JsonTextWriter(textWriter), value);
        }

        /// <summary>
        /// Serializes the specified <see cref="Object"/> and writes the JSON structure
        /// using the specified <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="jsonWriter">The <see cref="JsonWriter"/> used to write the JSON structure.</param>
        /// <param name="value">The <see cref="Object"/> to serialize.</param>
        /// <param name="objectType">
        /// The type of the value being serialized.
        /// This parameter is used when <see cref="JsonSerializer.TypeNameHandling"/> is <see cref="Json.TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.
        /// </param>
        public void Serialize(JsonWriter jsonWriter, object value, Type objectType)
        {
            this.SerializeInternal(jsonWriter, value, objectType);
        }

        /// <summary>
        /// Serializes the specified <see cref="Object"/> and writes the JSON structure
        /// using the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> used to write the JSON structure.</param>
        /// <param name="value">The <see cref="Object"/> to serialize.</param>
        /// <param name="objectType">
        /// The type of the value being serialized.
        /// This parameter is used when <see cref="TypeNameHandling"/> is Auto to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.
        /// </param>
        public void Serialize(TextWriter textWriter, object value, Type objectType)
        {
            this.Serialize(new JsonTextWriter(textWriter), value, objectType);
        }

        /// <summary>
        /// Serializes the specified <see cref="Object"/> and writes the JSON structure
        /// using the specified <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="jsonWriter">The <see cref="JsonWriter"/> used to write the JSON structure.</param>
        /// <param name="value">The <see cref="Object"/> to serialize.</param>
        public void Serialize(JsonWriter jsonWriter, object value)
        {
            this.SerializeInternal(jsonWriter, value, null);
        }

        private TraceJsonReader CreateTraceJsonReader(JsonReader reader)
        {
            TraceJsonReader traceReader = new TraceJsonReader(reader);
            if (reader.TokenType != JsonToken.None)
            {
                traceReader.WriteCurrentToken();
            }

            return traceReader;
        }

        internal virtual void SerializeInternal(JsonWriter jsonWriter, object value, Type objectType)
        {
            ValidationUtils.ArgumentNotNull(jsonWriter, nameof(jsonWriter));

            // set serialization options onto writer
            Formatting? previousFormatting = null;
            if (this._formatting != null && jsonWriter.Formatting != this._formatting)
            {
                previousFormatting = jsonWriter.Formatting;
                jsonWriter.Formatting = this._formatting.GetValueOrDefault();
            }

            DateFormatHandling? previousDateFormatHandling = null;
            if (this._dateFormatHandling != null && jsonWriter.DateFormatHandling != this._dateFormatHandling)
            {
                previousDateFormatHandling = jsonWriter.DateFormatHandling;
                jsonWriter.DateFormatHandling = this._dateFormatHandling.GetValueOrDefault();
            }

            DateTimeZoneHandling? previousDateTimeZoneHandling = null;
            if (this._dateTimeZoneHandling != null && jsonWriter.DateTimeZoneHandling != this._dateTimeZoneHandling)
            {
                previousDateTimeZoneHandling = jsonWriter.DateTimeZoneHandling;
                jsonWriter.DateTimeZoneHandling = this._dateTimeZoneHandling.GetValueOrDefault();
            }

            FloatFormatHandling? previousFloatFormatHandling = null;
            if (this._floatFormatHandling != null && jsonWriter.FloatFormatHandling != this._floatFormatHandling)
            {
                previousFloatFormatHandling = jsonWriter.FloatFormatHandling;
                jsonWriter.FloatFormatHandling = this._floatFormatHandling.GetValueOrDefault();
            }

            StringEscapeHandling? previousStringEscapeHandling = null;
            if (this._stringEscapeHandling != null && jsonWriter.StringEscapeHandling != this._stringEscapeHandling)
            {
                previousStringEscapeHandling = jsonWriter.StringEscapeHandling;
                jsonWriter.StringEscapeHandling = this._stringEscapeHandling.GetValueOrDefault();
            }

            CultureInfo previousCulture = null;
            if (this._culture != null && !this._culture.Equals(jsonWriter.Culture))
            {
                previousCulture = jsonWriter.Culture;
                jsonWriter.Culture = this._culture;
            }

            string previousDateFormatString = null;
            if (this._dateFormatStringSet && jsonWriter.DateFormatString != this._dateFormatString)
            {
                previousDateFormatString = jsonWriter.DateFormatString;
                jsonWriter.DateFormatString = this._dateFormatString;
            }

            TraceJsonWriter traceJsonWriter = (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Verbose)
                ? new TraceJsonWriter(jsonWriter)
                : null;

            JsonSerializerInternalWriter serializerWriter = new JsonSerializerInternalWriter(this);
            serializerWriter.Serialize(traceJsonWriter ?? jsonWriter, value, objectType);

            if (traceJsonWriter != null)
            {
                this.TraceWriter.Trace(TraceLevel.Verbose, traceJsonWriter.GetSerializedJsonMessage(), null);
            }

            // reset writer back to previous options
            if (previousFormatting != null)
            {
                jsonWriter.Formatting = previousFormatting.GetValueOrDefault();
            }
            if (previousDateFormatHandling != null)
            {
                jsonWriter.DateFormatHandling = previousDateFormatHandling.GetValueOrDefault();
            }
            if (previousDateTimeZoneHandling != null)
            {
                jsonWriter.DateTimeZoneHandling = previousDateTimeZoneHandling.GetValueOrDefault();
            }
            if (previousFloatFormatHandling != null)
            {
                jsonWriter.FloatFormatHandling = previousFloatFormatHandling.GetValueOrDefault();
            }
            if (previousStringEscapeHandling != null)
            {
                jsonWriter.StringEscapeHandling = previousStringEscapeHandling.GetValueOrDefault();
            }
            if (this._dateFormatStringSet)
            {
                jsonWriter.DateFormatString = previousDateFormatString;
            }
            if (previousCulture != null)
            {
                jsonWriter.Culture = previousCulture;
            }
        }

        internal IReferenceResolver GetReferenceResolver()
        {
            if (this._referenceResolver == null)
            {
                this._referenceResolver = new DefaultReferenceResolver();
            }

            return this._referenceResolver;
        }

        internal JsonConverter GetMatchingConverter(Type type)
        {
            return GetMatchingConverter(this._converters, type);
        }

        internal static JsonConverter GetMatchingConverter(IList<JsonConverter> converters, Type objectType)
        {
#if DEBUG
            ValidationUtils.ArgumentNotNull(objectType, nameof(objectType));
#endif

            if (converters != null)
            {
                for (int i = 0; i < converters.Count; i++)
                {
                    JsonConverter converter = converters[i];

                    if (converter.CanConvert(objectType))
                    {
                        return converter;
                    }
                }
            }

            return null;
        }

        internal void OnError(ErrorEventArgs e)
        {
            Error?.Invoke(this, e);
        }
    }
}