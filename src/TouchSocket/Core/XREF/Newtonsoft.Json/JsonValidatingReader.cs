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

using TouchSocket.Core.XREF.Newtonsoft.Json.Linq;
using TouchSocket.Core.XREF.Newtonsoft.Json.Schema;
using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;

#if !HAVE_LINQ

using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities.LinqBridge;

#else
using System.Linq;

#endif

namespace TouchSocket.Core.XREF.Newtonsoft.Json
{
    /// <summary>
    /// <para>
    /// Represents a reader that provides <see cref="JsonSchema"/> validation.
    /// </para>
    /// <note type="caution">
    /// JSON Schema validation has been moved to its own package. See <see href="http://www.newtonsoft.com/jsonschema">http://www.newtonsoft.com/jsonschema</see> for more details.
    /// </note>
    /// </summary>
    [Obsolete("JSON Schema validation has been moved to its own package. See http://www.newtonsoft.com/jsonschema for more details.")]
    public class JsonValidatingReader : JsonReader, IJsonLineInfo
    {
        private class SchemaScope
        {
            private readonly JTokenType _tokenType;
            private readonly IList<JsonSchemaModel> _schemas;
            private readonly Dictionary<string, bool> _requiredProperties;

            public string CurrentPropertyName { get; set; }
            public int ArrayItemCount { get; set; }
            public bool IsUniqueArray { get; }
            public IList<JToken> UniqueArrayItems { get; }
            public JTokenWriter CurrentItemWriter { get; set; }

            public IList<JsonSchemaModel> Schemas => this._schemas;

            public Dictionary<string, bool> RequiredProperties => this._requiredProperties;

            public JTokenType TokenType => this._tokenType;

            public SchemaScope(JTokenType tokenType, IList<JsonSchemaModel> schemas)
            {
                this._tokenType = tokenType;
                this._schemas = schemas;

                this._requiredProperties = schemas.SelectMany<JsonSchemaModel, string>(this.GetRequiredProperties).Distinct().ToDictionary(p => p, p => false);

                if (tokenType == JTokenType.Array && schemas.Any(s => s.UniqueItems))
                {
                    this.IsUniqueArray = true;
                    this.UniqueArrayItems = new List<JToken>();
                }
            }

            private IEnumerable<string> GetRequiredProperties(JsonSchemaModel schema)
            {
                if (schema?.Properties == null)
                {
                    return Enumerable.Empty<string>();
                }

                return schema.Properties.Where(p => p.Value.Required).Select(p => p.Key);
            }
        }

        private readonly JsonReader _reader;
        private readonly Stack<SchemaScope> _stack;
        private JsonSchema _schema;
        private JsonSchemaModel _model;
        private SchemaScope _currentScope;

        /// <summary>
        /// Sets an event handler for receiving schema validation errors.
        /// </summary>
        public event ValidationEventHandler ValidationEventHandler;

        /// <summary>
        /// Gets the text value of the current JSON token.
        /// </summary>
        /// <value></value>
        public override object Value => this._reader.Value;

        /// <summary>
        /// Gets the depth of the current token in the JSON document.
        /// </summary>
        /// <value>The depth of the current token in the JSON document.</value>
        public override int Depth => this._reader.Depth;

        /// <summary>
        /// Gets the path of the current JSON token.
        /// </summary>
        public override string Path => this._reader.Path;

        /// <summary>
        /// Gets the quotation mark character used to enclose the value of a string.
        /// </summary>
        /// <value></value>
        public override char QuoteChar
        {
            get => this._reader.QuoteChar;
            protected internal set { }
        }

        /// <summary>
        /// Gets the type of the current JSON token.
        /// </summary>
        /// <value></value>
        public override JsonToken TokenType => this._reader.TokenType;

        /// <summary>
        /// Gets the .NET type for the current JSON token.
        /// </summary>
        /// <value></value>
        public override Type ValueType => this._reader.ValueType;

        private void Push(SchemaScope scope)
        {
            this._stack.Push(scope);
            this._currentScope = scope;
        }

        private SchemaScope Pop()
        {
            SchemaScope poppedScope = this._stack.Pop();
            this._currentScope = (this._stack.Count != 0)
                ? this._stack.Peek()
                : null;

            return poppedScope;
        }

        private IList<JsonSchemaModel> CurrentSchemas => this._currentScope.Schemas;

        private static readonly IList<JsonSchemaModel> EmptySchemaList = new List<JsonSchemaModel>();

        private IList<JsonSchemaModel> CurrentMemberSchemas
        {
            get
            {
                if (this._currentScope == null)
                {
                    return new List<JsonSchemaModel>(new[] { this._model });
                }

                if (this._currentScope.Schemas == null || this._currentScope.Schemas.Count == 0)
                {
                    return EmptySchemaList;
                }

                switch (this._currentScope.TokenType)
                {
                    case JTokenType.None:
                        return this._currentScope.Schemas;

                    case JTokenType.Object:
                        {
                            if (this._currentScope.CurrentPropertyName == null)
                            {
                                throw new JsonReaderException("CurrentPropertyName has not been set on scope.");
                            }

                            IList<JsonSchemaModel> schemas = new List<JsonSchemaModel>();

                            foreach (JsonSchemaModel schema in this.CurrentSchemas)
                            {
                                JsonSchemaModel propertySchema;
                                if (schema.Properties != null && schema.Properties.TryGetValue(this._currentScope.CurrentPropertyName, out propertySchema))
                                {
                                    schemas.Add(propertySchema);
                                }
                                if (schema.PatternProperties != null)
                                {
                                    foreach (KeyValuePair<string, JsonSchemaModel> patternProperty in schema.PatternProperties)
                                    {
                                        if (Regex.IsMatch(this._currentScope.CurrentPropertyName, patternProperty.Key))
                                        {
                                            schemas.Add(patternProperty.Value);
                                        }
                                    }
                                }

                                if (schemas.Count == 0 && schema.AllowAdditionalProperties && schema.AdditionalProperties != null)
                                {
                                    schemas.Add(schema.AdditionalProperties);
                                }
                            }

                            return schemas;
                        }
                    case JTokenType.Array:
                        {
                            IList<JsonSchemaModel> schemas = new List<JsonSchemaModel>();

                            foreach (JsonSchemaModel schema in this.CurrentSchemas)
                            {
                                if (!schema.PositionalItemsValidation)
                                {
                                    if (schema.Items != null && schema.Items.Count > 0)
                                    {
                                        schemas.Add(schema.Items[0]);
                                    }
                                }
                                else
                                {
                                    if (schema.Items != null && schema.Items.Count > 0)
                                    {
                                        if (schema.Items.Count > (this._currentScope.ArrayItemCount - 1))
                                        {
                                            schemas.Add(schema.Items[this._currentScope.ArrayItemCount - 1]);
                                        }
                                    }

                                    if (schema.AllowAdditionalItems && schema.AdditionalItems != null)
                                    {
                                        schemas.Add(schema.AdditionalItems);
                                    }
                                }
                            }

                            return schemas;
                        }
                    case JTokenType.Constructor:
                        return EmptySchemaList;

                    default:
                        throw new ArgumentOutOfRangeException("TokenType", "Unexpected token type: {0}".FormatWith(CultureInfo.InvariantCulture, this._currentScope.TokenType));
                }
            }
        }

        private void RaiseError(string message, JsonSchemaModel schema)
        {
            IJsonLineInfo lineInfo = this;

            string exceptionMessage = (lineInfo.HasLineInfo())
                ? message + " Line {0}, position {1}.".FormatWith(CultureInfo.InvariantCulture, lineInfo.LineNumber, lineInfo.LinePosition)
                : message;

            this.OnValidationEvent(new JsonSchemaException(exceptionMessage, null, this.Path, lineInfo.LineNumber, lineInfo.LinePosition));
        }

        private void OnValidationEvent(JsonSchemaException exception)
        {
            ValidationEventHandler handler = ValidationEventHandler;
            if (handler != null)
            {
                handler(this, new ValidationEventArgs(exception));
            }
            else
            {
                throw exception;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonValidatingReader"/> class that
        /// validates the content returned from the given <see cref="JsonReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from while validating.</param>
        public JsonValidatingReader(JsonReader reader)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));
            this._reader = reader;
            this._stack = new Stack<SchemaScope>();
        }

        /// <summary>
        /// Gets or sets the schema.
        /// </summary>
        /// <value>The schema.</value>
        public JsonSchema Schema
        {
            get => this._schema;
            set
            {
                if (this.TokenType != JsonToken.None)
                {
                    throw new InvalidOperationException("Cannot change schema while validating JSON.");
                }

                this._schema = value;
                this._model = null;
            }
        }

        /// <summary>
        /// Gets the <see cref="JsonReader"/> used to construct this <see cref="JsonValidatingReader"/>.
        /// </summary>
        /// <value>The <see cref="JsonReader"/> specified in the constructor.</value>
        public JsonReader Reader => this._reader;

        /// <summary>
        /// Changes the reader's state to <see cref="JsonReader.State.Closed"/>.
        /// If <see cref="JsonReader.CloseInput"/> is set to <c>true</c>, the underlying <see cref="JsonReader"/> is also closed.
        /// </summary>
        public override void Close()
        {
            base.Close();
            if (this.CloseInput)
            {
                this._reader?.Close();
            }
        }

        private void ValidateNotDisallowed(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return;
            }

            JsonSchemaType? currentNodeType = this.GetCurrentNodeSchemaType();
            if (currentNodeType != null)
            {
                if (JsonSchemaGenerator.HasFlag(schema.Disallow, currentNodeType.GetValueOrDefault()))
                {
                    this.RaiseError("Type {0} is disallowed.".FormatWith(CultureInfo.InvariantCulture, currentNodeType), schema);
                }
            }
        }

        private JsonSchemaType? GetCurrentNodeSchemaType()
        {
            switch (this._reader.TokenType)
            {
                case JsonToken.StartObject:
                    return JsonSchemaType.Object;

                case JsonToken.StartArray:
                    return JsonSchemaType.Array;

                case JsonToken.Integer:
                    return JsonSchemaType.Integer;

                case JsonToken.Float:
                    return JsonSchemaType.Float;

                case JsonToken.String:
                    return JsonSchemaType.String;

                case JsonToken.Boolean:
                    return JsonSchemaType.Boolean;

                case JsonToken.Null:
                    return JsonSchemaType.Null;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="JsonReader"/> as a <see cref="Nullable{T}"/> of <see cref="Int32"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="Int32"/>.</returns>
        public override int? ReadAsInt32()
        {
            int? i = this._reader.ReadAsInt32();

            this.ValidateCurrentToken();
            return i;
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="JsonReader"/> as a <see cref="Byte"/>[].
        /// </summary>
        /// <returns>
        /// A <see cref="Byte"/>[] or <c>null</c> if the next JSON token is null.
        /// </returns>
        public override byte[] ReadAsBytes()
        {
            byte[] data = this._reader.ReadAsBytes();

            this.ValidateCurrentToken();
            return data;
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="JsonReader"/> as a <see cref="Nullable{T}"/> of <see cref="Decimal"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="Decimal"/>.</returns>
        public override decimal? ReadAsDecimal()
        {
            decimal? d = this._reader.ReadAsDecimal();

            this.ValidateCurrentToken();
            return d;
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="JsonReader"/> as a <see cref="Nullable{T}"/> of <see cref="Double"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="Double"/>.</returns>
        public override double? ReadAsDouble()
        {
            double? d = this._reader.ReadAsDouble();

            this.ValidateCurrentToken();
            return d;
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="JsonReader"/> as a <see cref="Nullable{T}"/> of <see cref="Boolean"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="Boolean"/>.</returns>
        public override bool? ReadAsBoolean()
        {
            bool? b = this._reader.ReadAsBoolean();

            this.ValidateCurrentToken();
            return b;
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="JsonReader"/> as a <see cref="String"/>.
        /// </summary>
        /// <returns>A <see cref="String"/>. This method will return <c>null</c> at the end of an array.</returns>
        public override string ReadAsString()
        {
            string s = this._reader.ReadAsString();

            this.ValidateCurrentToken();
            return s;
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="JsonReader"/> as a <see cref="Nullable{T}"/> of <see cref="DateTime"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="DateTime"/>. This method will return <c>null</c> at the end of an array.</returns>
        public override DateTime? ReadAsDateTime()
        {
            DateTime? dateTime = this._reader.ReadAsDateTime();

            this.ValidateCurrentToken();
            return dateTime;
        }

#if HAVE_DATE_TIME_OFFSET
        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="JsonReader"/> as a <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/>.</returns>
        public override DateTimeOffset? ReadAsDateTimeOffset()
        {
            DateTimeOffset? dateTimeOffset = _reader.ReadAsDateTimeOffset();

            ValidateCurrentToken();
            return dateTimeOffset;
        }
#endif

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="JsonReader"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.
        /// </returns>
        public override bool Read()
        {
            if (!this._reader.Read())
            {
                return false;
            }

            if (this._reader.TokenType == JsonToken.Comment)
            {
                return true;
            }

            this.ValidateCurrentToken();
            return true;
        }

        private void ValidateCurrentToken()
        {
            // first time validate has been called. build model
            if (this._model == null)
            {
                JsonSchemaModelBuilder builder = new JsonSchemaModelBuilder();
                this._model = builder.Build(this._schema);

                if (!JsonTokenUtils.IsStartToken(this._reader.TokenType))
                {
                    this.Push(new SchemaScope(JTokenType.None, this.CurrentMemberSchemas));
                }
            }

            switch (this._reader.TokenType)
            {
                case JsonToken.StartObject:
                    this.ProcessValue();
                    IList<JsonSchemaModel> objectSchemas = this.CurrentMemberSchemas.Where(this.ValidateObject).ToList();
                    this.Push(new SchemaScope(JTokenType.Object, objectSchemas));
                    this.WriteToken(this.CurrentSchemas);
                    break;

                case JsonToken.StartArray:
                    this.ProcessValue();
                    IList<JsonSchemaModel> arraySchemas = this.CurrentMemberSchemas.Where(this.ValidateArray).ToList();
                    this.Push(new SchemaScope(JTokenType.Array, arraySchemas));
                    this.WriteToken(this.CurrentSchemas);
                    break;

                case JsonToken.StartConstructor:
                    this.ProcessValue();
                    this.Push(new SchemaScope(JTokenType.Constructor, null));
                    this.WriteToken(this.CurrentSchemas);
                    break;

                case JsonToken.PropertyName:
                    this.WriteToken(this.CurrentSchemas);
                    foreach (JsonSchemaModel schema in this.CurrentSchemas)
                    {
                        this.ValidatePropertyName(schema);
                    }
                    break;

                case JsonToken.Raw:
                    this.ProcessValue();
                    break;

                case JsonToken.Integer:
                    this.ProcessValue();
                    this.WriteToken(this.CurrentMemberSchemas);
                    foreach (JsonSchemaModel schema in this.CurrentMemberSchemas)
                    {
                        this.ValidateInteger(schema);
                    }
                    break;

                case JsonToken.Float:
                    this.ProcessValue();
                    this.WriteToken(this.CurrentMemberSchemas);
                    foreach (JsonSchemaModel schema in this.CurrentMemberSchemas)
                    {
                        this.ValidateFloat(schema);
                    }
                    break;

                case JsonToken.String:
                    this.ProcessValue();
                    this.WriteToken(this.CurrentMemberSchemas);
                    foreach (JsonSchemaModel schema in this.CurrentMemberSchemas)
                    {
                        this.ValidateString(schema);
                    }
                    break;

                case JsonToken.Boolean:
                    this.ProcessValue();
                    this.WriteToken(this.CurrentMemberSchemas);
                    foreach (JsonSchemaModel schema in this.CurrentMemberSchemas)
                    {
                        this.ValidateBoolean(schema);
                    }
                    break;

                case JsonToken.Null:
                    this.ProcessValue();
                    this.WriteToken(this.CurrentMemberSchemas);
                    foreach (JsonSchemaModel schema in this.CurrentMemberSchemas)
                    {
                        this.ValidateNull(schema);
                    }
                    break;

                case JsonToken.EndObject:
                    this.WriteToken(this.CurrentSchemas);
                    foreach (JsonSchemaModel schema in this.CurrentSchemas)
                    {
                        this.ValidateEndObject(schema);
                    }
                    this.Pop();
                    break;

                case JsonToken.EndArray:
                    this.WriteToken(this.CurrentSchemas);
                    foreach (JsonSchemaModel schema in this.CurrentSchemas)
                    {
                        this.ValidateEndArray(schema);
                    }
                    this.Pop();
                    break;

                case JsonToken.EndConstructor:
                    this.WriteToken(this.CurrentSchemas);
                    this.Pop();
                    break;

                case JsonToken.Undefined:
                case JsonToken.Date:
                case JsonToken.Bytes:
                    // these have no equivalent in JSON schema
                    this.WriteToken(this.CurrentMemberSchemas);
                    break;

                case JsonToken.None:
                    // no content, do nothing
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void WriteToken(IList<JsonSchemaModel> schemas)
        {
            foreach (SchemaScope schemaScope in this._stack)
            {
                bool isInUniqueArray = (schemaScope.TokenType == JTokenType.Array && schemaScope.IsUniqueArray && schemaScope.ArrayItemCount > 0);

                if (isInUniqueArray || schemas.Any(s => s.Enum != null))
                {
                    if (schemaScope.CurrentItemWriter == null)
                    {
                        if (JsonTokenUtils.IsEndToken(this._reader.TokenType))
                        {
                            continue;
                        }

                        schemaScope.CurrentItemWriter = new JTokenWriter();
                    }

                    schemaScope.CurrentItemWriter.WriteToken(this._reader, false);

                    // finished writing current item
                    if (schemaScope.CurrentItemWriter.Top == 0 && this._reader.TokenType != JsonToken.PropertyName)
                    {
                        JToken finishedItem = schemaScope.CurrentItemWriter.Token;

                        // start next item with new writer
                        schemaScope.CurrentItemWriter = null;

                        if (isInUniqueArray)
                        {
                            if (schemaScope.UniqueArrayItems.Contains(finishedItem, JToken.EqualityComparer))
                            {
                                this.RaiseError("Non-unique array item at index {0}.".FormatWith(CultureInfo.InvariantCulture, schemaScope.ArrayItemCount - 1), schemaScope.Schemas.First(s => s.UniqueItems));
                            }

                            schemaScope.UniqueArrayItems.Add(finishedItem);
                        }
                        else if (schemas.Any(s => s.Enum != null))
                        {
                            foreach (JsonSchemaModel schema in schemas)
                            {
                                if (schema.Enum != null)
                                {
                                    if (!schema.Enum.ContainsValue(finishedItem, JToken.EqualityComparer))
                                    {
                                        StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
                                        finishedItem.WriteTo(new JsonTextWriter(sw));

                                        this.RaiseError("Value {0} is not defined in enum.".FormatWith(CultureInfo.InvariantCulture, sw.ToString()), schema);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ValidateEndObject(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return;
            }

            Dictionary<string, bool> requiredProperties = this._currentScope.RequiredProperties;

            if (requiredProperties != null && requiredProperties.Values.Any(v => !v))
            {
                IEnumerable<string> unmatchedRequiredProperties = requiredProperties.Where(kv => !kv.Value).Select(kv => kv.Key);
                this.RaiseError("Required properties are missing from object: {0}.".FormatWith(CultureInfo.InvariantCulture, string.Join(", ", unmatchedRequiredProperties
#if !HAVE_STRING_JOIN_WITH_ENUMERABLE
                    .ToArray()
#endif
                    )), schema);
            }
        }

        private void ValidateEndArray(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return;
            }

            int arrayItemCount = this._currentScope.ArrayItemCount;

            if (schema.MaximumItems != null && arrayItemCount > schema.MaximumItems)
            {
                this.RaiseError("Array item count {0} exceeds maximum count of {1}.".FormatWith(CultureInfo.InvariantCulture, arrayItemCount, schema.MaximumItems), schema);
            }

            if (schema.MinimumItems != null && arrayItemCount < schema.MinimumItems)
            {
                this.RaiseError("Array item count {0} is less than minimum count of {1}.".FormatWith(CultureInfo.InvariantCulture, arrayItemCount, schema.MinimumItems), schema);
            }
        }

        private void ValidateNull(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return;
            }

            if (!this.TestType(schema, JsonSchemaType.Null))
            {
                return;
            }

            this.ValidateNotDisallowed(schema);
        }

        private void ValidateBoolean(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return;
            }

            if (!this.TestType(schema, JsonSchemaType.Boolean))
            {
                return;
            }

            this.ValidateNotDisallowed(schema);
        }

        private void ValidateString(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return;
            }

            if (!this.TestType(schema, JsonSchemaType.String))
            {
                return;
            }

            this.ValidateNotDisallowed(schema);

            string value = this._reader.Value.ToString();

            if (schema.MaximumLength != null && value.Length > schema.MaximumLength)
            {
                this.RaiseError("String '{0}' exceeds maximum length of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.MaximumLength), schema);
            }

            if (schema.MinimumLength != null && value.Length < schema.MinimumLength)
            {
                this.RaiseError("String '{0}' is less than minimum length of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.MinimumLength), schema);
            }

            if (schema.Patterns != null)
            {
                foreach (string pattern in schema.Patterns)
                {
                    if (!Regex.IsMatch(value, pattern))
                    {
                        this.RaiseError("String '{0}' does not match regex pattern '{1}'.".FormatWith(CultureInfo.InvariantCulture, value, pattern), schema);
                    }
                }
            }
        }

        private void ValidateInteger(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return;
            }

            if (!this.TestType(schema, JsonSchemaType.Integer))
            {
                return;
            }

            this.ValidateNotDisallowed(schema);

            object value = this._reader.Value;

            if (schema.Maximum != null)
            {
                if (JValue.Compare(JTokenType.Integer, value, schema.Maximum) > 0)
                {
                    this.RaiseError("Integer {0} exceeds maximum value of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.Maximum), schema);
                }
                if (schema.ExclusiveMaximum && JValue.Compare(JTokenType.Integer, value, schema.Maximum) == 0)
                {
                    this.RaiseError("Integer {0} equals maximum value of {1} and exclusive maximum is true.".FormatWith(CultureInfo.InvariantCulture, value, schema.Maximum), schema);
                }
            }

            if (schema.Minimum != null)
            {
                if (JValue.Compare(JTokenType.Integer, value, schema.Minimum) < 0)
                {
                    this.RaiseError("Integer {0} is less than minimum value of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.Minimum), schema);
                }
                if (schema.ExclusiveMinimum && JValue.Compare(JTokenType.Integer, value, schema.Minimum) == 0)
                {
                    this.RaiseError("Integer {0} equals minimum value of {1} and exclusive minimum is true.".FormatWith(CultureInfo.InvariantCulture, value, schema.Minimum), schema);
                }
            }

            if (schema.DivisibleBy != null)
            {
                bool notDivisible;
#if HAVE_BIG_INTEGER
                if (value is BigInteger)
                {
                    // not that this will lose any decimal point on DivisibleBy
                    // so manually raise an error if DivisibleBy is not an integer and value is not zero
                    BigInteger i = (BigInteger)value;
                    bool divisibleNonInteger = !Math.Abs(schema.DivisibleBy.Value - Math.Truncate(schema.DivisibleBy.Value)).Equals(0);
                    if (divisibleNonInteger)
                    {
                        notDivisible = i != 0;
                    }
                    else
                    {
                        notDivisible = i % new BigInteger(schema.DivisibleBy.Value) != 0;
                    }
                }
                else
#endif
                {
                    notDivisible = !IsZero(Convert.ToInt64(value, CultureInfo.InvariantCulture) % schema.DivisibleBy.GetValueOrDefault());
                }

                if (notDivisible)
                {
                    this.RaiseError("Integer {0} is not evenly divisible by {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.DivisibleBy), schema);
                }
            }
        }

        private void ProcessValue()
        {
            if (this._currentScope != null && this._currentScope.TokenType == JTokenType.Array)
            {
                this._currentScope.ArrayItemCount++;

                foreach (JsonSchemaModel currentSchema in this.CurrentSchemas)
                {
                    // if there is positional validation and the array index is past the number of item validation schemas and there are no additional items then error
                    if (currentSchema != null
                        && currentSchema.PositionalItemsValidation
                        && !currentSchema.AllowAdditionalItems
                        && (currentSchema.Items == null || this._currentScope.ArrayItemCount - 1 >= currentSchema.Items.Count))
                    {
                        this.RaiseError("Index {0} has not been defined and the schema does not allow additional items.".FormatWith(CultureInfo.InvariantCulture, this._currentScope.ArrayItemCount), currentSchema);
                    }
                }
            }
        }

        private void ValidateFloat(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return;
            }

            if (!this.TestType(schema, JsonSchemaType.Float))
            {
                return;
            }

            this.ValidateNotDisallowed(schema);

            double value = Convert.ToDouble(this._reader.Value, CultureInfo.InvariantCulture);

            if (schema.Maximum != null)
            {
                if (value > schema.Maximum)
                {
                    this.RaiseError("Float {0} exceeds maximum value of {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.Maximum), schema);
                }
                if (schema.ExclusiveMaximum && value == schema.Maximum)
                {
                    this.RaiseError("Float {0} equals maximum value of {1} and exclusive maximum is true.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.Maximum), schema);
                }
            }

            if (schema.Minimum != null)
            {
                if (value < schema.Minimum)
                {
                    this.RaiseError("Float {0} is less than minimum value of {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.Minimum), schema);
                }
                if (schema.ExclusiveMinimum && value == schema.Minimum)
                {
                    this.RaiseError("Float {0} equals minimum value of {1} and exclusive minimum is true.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.Minimum), schema);
                }
            }

            if (schema.DivisibleBy != null)
            {
                double remainder = FloatingPointRemainder(value, schema.DivisibleBy.GetValueOrDefault());

                if (!IsZero(remainder))
                {
                    this.RaiseError("Float {0} is not evenly divisible by {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.DivisibleBy), schema);
                }
            }
        }

        private static double FloatingPointRemainder(double dividend, double divisor)
        {
            return dividend - Math.Floor(dividend / divisor) * divisor;
        }

        private static bool IsZero(double value)
        {
            const double epsilon = 2.2204460492503131e-016;

            return Math.Abs(value) < 20.0 * epsilon;
        }

        private void ValidatePropertyName(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return;
            }

            string propertyName = Convert.ToString(this._reader.Value, CultureInfo.InvariantCulture);

            if (this._currentScope.RequiredProperties.ContainsKey(propertyName))
            {
                this._currentScope.RequiredProperties[propertyName] = true;
            }

            if (!schema.AllowAdditionalProperties)
            {
                bool propertyDefinied = this.IsPropertyDefinied(schema, propertyName);

                if (!propertyDefinied)
                {
                    this.RaiseError("Property '{0}' has not been defined and the schema does not allow additional properties.".FormatWith(CultureInfo.InvariantCulture, propertyName), schema);
                }
            }

            this._currentScope.CurrentPropertyName = propertyName;
        }

        private bool IsPropertyDefinied(JsonSchemaModel schema, string propertyName)
        {
            if (schema.Properties != null && schema.Properties.ContainsKey(propertyName))
            {
                return true;
            }

            if (schema.PatternProperties != null)
            {
                foreach (string pattern in schema.PatternProperties.Keys)
                {
                    if (Regex.IsMatch(propertyName, pattern))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool ValidateArray(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return true;
            }

            return (this.TestType(schema, JsonSchemaType.Array));
        }

        private bool ValidateObject(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return true;
            }

            return (this.TestType(schema, JsonSchemaType.Object));
        }

        private bool TestType(JsonSchemaModel currentSchema, JsonSchemaType currentType)
        {
            if (!JsonSchemaGenerator.HasFlag(currentSchema.Type, currentType))
            {
                this.RaiseError("Invalid type. Expected {0} but got {1}.".FormatWith(CultureInfo.InvariantCulture, currentSchema.Type, currentType), currentSchema);
                return false;
            }

            return true;
        }

        bool IJsonLineInfo.HasLineInfo()
        {
            IJsonLineInfo lineInfo = this._reader as IJsonLineInfo;
            return lineInfo != null && lineInfo.HasLineInfo();
        }

        int IJsonLineInfo.LineNumber
        {
            get
            {
                IJsonLineInfo lineInfo = this._reader as IJsonLineInfo;
                return (lineInfo != null) ? lineInfo.LineNumber : 0;
            }
        }

        int IJsonLineInfo.LinePosition
        {
            get
            {
                IJsonLineInfo lineInfo = this._reader as IJsonLineInfo;
                return (lineInfo != null) ? lineInfo.LinePosition : 0;
            }
        }
    }
}