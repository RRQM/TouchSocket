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
using TouchSocket.Core.XREF.Newtonsoft.Json.Serialization;

#if !HAVE_LINQ

using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities.LinqBridge;

#else
using System.Linq;
#endif

using System.Globalization;
using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities;
using TouchSocket.Core.XREF.Newtonsoft.Json.Linq;

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Schema
{
    [Obsolete("JSON Schema validation has been moved to its own package. See http://www.newtonsoft.com/jsonschema for more details.")]
    internal class JsonSchemaBuilder
    {
        private readonly IList<JsonSchema> _stack;
        private readonly JsonSchemaResolver _resolver;
        private readonly IDictionary<string, JsonSchema> _documentSchemas;
        private JsonSchema _currentSchema;
        private JObject _rootSchema;

        public JsonSchemaBuilder(JsonSchemaResolver resolver)
        {
            this._stack = new List<JsonSchema>();
            this._documentSchemas = new Dictionary<string, JsonSchema>();
            this._resolver = resolver;
        }

        private void Push(JsonSchema value)
        {
            this._currentSchema = value;
            this._stack.Add(value);
            this._resolver.LoadedSchemas.Add(value);
            this._documentSchemas.Add(value.Location, value);
        }

        private JsonSchema Pop()
        {
            JsonSchema poppedSchema = this._currentSchema;
            this._stack.RemoveAt(this._stack.Count - 1);
            this._currentSchema = this._stack.LastOrDefault();

            return poppedSchema;
        }

        private JsonSchema CurrentSchema => this._currentSchema;

        internal JsonSchema Read(JsonReader reader)
        {
            JToken schemaToken = JToken.ReadFrom(reader);

            this._rootSchema = schemaToken as JObject;

            JsonSchema schema = this.BuildSchema(schemaToken);

            this.ResolveReferences(schema);

            return schema;
        }

        private string UnescapeReference(string reference)
        {
            return Uri.UnescapeDataString(reference).Replace("~1", "/").Replace("~0", "~");
        }

        private JsonSchema ResolveReferences(JsonSchema schema)
        {
            if (schema.DeferredReference != null)
            {
                string reference = schema.DeferredReference;

                bool locationReference = (reference.StartsWith("#", StringComparison.Ordinal));
                if (locationReference)
                {
                    reference = this.UnescapeReference(reference);
                }

                JsonSchema resolvedSchema = this._resolver.GetSchema(reference);

                if (resolvedSchema == null)
                {
                    if (locationReference)
                    {
                        string[] escapedParts = schema.DeferredReference.TrimStart('#').Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        JToken currentToken = this._rootSchema;
                        foreach (string escapedPart in escapedParts)
                        {
                            string part = this.UnescapeReference(escapedPart);

                            if (currentToken.Type == JTokenType.Object)
                            {
                                currentToken = currentToken[part];
                            }
                            else if (currentToken.Type == JTokenType.Array || currentToken.Type == JTokenType.Constructor)
                            {
                                int index;
                                if (int.TryParse(part, out index) && index >= 0 && index < currentToken.Count())
                                {
                                    currentToken = currentToken[index];
                                }
                                else
                                {
                                    currentToken = null;
                                }
                            }

                            if (currentToken == null)
                            {
                                break;
                            }
                        }

                        if (currentToken != null)
                        {
                            resolvedSchema = this.BuildSchema(currentToken);
                        }
                    }

                    if (resolvedSchema == null)
                    {
                        throw new JsonException("Could not resolve schema reference '{0}'.".FormatWith(CultureInfo.InvariantCulture, schema.DeferredReference));
                    }
                }

                schema = resolvedSchema;
            }

            if (schema.ReferencesResolved)
            {
                return schema;
            }

            schema.ReferencesResolved = true;

            if (schema.Extends != null)
            {
                for (int i = 0; i < schema.Extends.Count; i++)
                {
                    schema.Extends[i] = this.ResolveReferences(schema.Extends[i]);
                }
            }

            if (schema.Items != null)
            {
                for (int i = 0; i < schema.Items.Count; i++)
                {
                    schema.Items[i] = this.ResolveReferences(schema.Items[i]);
                }
            }

            if (schema.AdditionalItems != null)
            {
                schema.AdditionalItems = this.ResolveReferences(schema.AdditionalItems);
            }

            if (schema.PatternProperties != null)
            {
                foreach (KeyValuePair<string, JsonSchema> patternProperty in schema.PatternProperties.ToList())
                {
                    schema.PatternProperties[patternProperty.Key] = this.ResolveReferences(patternProperty.Value);
                }
            }

            if (schema.Properties != null)
            {
                foreach (KeyValuePair<string, JsonSchema> property in schema.Properties.ToList())
                {
                    schema.Properties[property.Key] = this.ResolveReferences(property.Value);
                }
            }

            if (schema.AdditionalProperties != null)
            {
                schema.AdditionalProperties = this.ResolveReferences(schema.AdditionalProperties);
            }

            return schema;
        }

        private JsonSchema BuildSchema(JToken token)
        {
            JObject schemaObject = token as JObject;
            if (schemaObject == null)
            {
                throw JsonException.Create(token, token.Path, "Expected object while parsing schema object, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));
            }

            JToken referenceToken;
            if (schemaObject.TryGetValue(JsonTypeReflector.RefPropertyName, out referenceToken))
            {
                JsonSchema deferredSchema = new JsonSchema();
                deferredSchema.DeferredReference = (string)referenceToken;

                return deferredSchema;
            }

            string location = token.Path.Replace(".", "/").Replace("[", "/").Replace("]", string.Empty);
            if (!string.IsNullOrEmpty(location))
            {
                location = "/" + location;
            }
            location = "#" + location;

            JsonSchema existingSchema;
            if (this._documentSchemas.TryGetValue(location, out existingSchema))
            {
                return existingSchema;
            }

            this.Push(new JsonSchema { Location = location });

            this.ProcessSchemaProperties(schemaObject);

            return this.Pop();
        }

        private void ProcessSchemaProperties(JObject schemaObject)
        {
            foreach (KeyValuePair<string, JToken> property in schemaObject)
            {
                switch (property.Key)
                {
                    case JsonSchemaConstants.TypePropertyName:
                        this.CurrentSchema.Type = this.ProcessType(property.Value);
                        break;

                    case JsonSchemaConstants.IdPropertyName:
                        this.CurrentSchema.Id = (string)property.Value;
                        break;

                    case JsonSchemaConstants.TitlePropertyName:
                        this.CurrentSchema.Title = (string)property.Value;
                        break;

                    case JsonSchemaConstants.DescriptionPropertyName:
                        this.CurrentSchema.Description = (string)property.Value;
                        break;

                    case JsonSchemaConstants.PropertiesPropertyName:
                        this.CurrentSchema.Properties = this.ProcessProperties(property.Value);
                        break;

                    case JsonSchemaConstants.ItemsPropertyName:
                        this.ProcessItems(property.Value);
                        break;

                    case JsonSchemaConstants.AdditionalPropertiesPropertyName:
                        this.ProcessAdditionalProperties(property.Value);
                        break;

                    case JsonSchemaConstants.AdditionalItemsPropertyName:
                        this.ProcessAdditionalItems(property.Value);
                        break;

                    case JsonSchemaConstants.PatternPropertiesPropertyName:
                        this.CurrentSchema.PatternProperties = this.ProcessProperties(property.Value);
                        break;

                    case JsonSchemaConstants.RequiredPropertyName:
                        this.CurrentSchema.Required = (bool)property.Value;
                        break;

                    case JsonSchemaConstants.RequiresPropertyName:
                        this.CurrentSchema.Requires = (string)property.Value;
                        break;

                    case JsonSchemaConstants.MinimumPropertyName:
                        this.CurrentSchema.Minimum = (double)property.Value;
                        break;

                    case JsonSchemaConstants.MaximumPropertyName:
                        this.CurrentSchema.Maximum = (double)property.Value;
                        break;

                    case JsonSchemaConstants.ExclusiveMinimumPropertyName:
                        this.CurrentSchema.ExclusiveMinimum = (bool)property.Value;
                        break;

                    case JsonSchemaConstants.ExclusiveMaximumPropertyName:
                        this.CurrentSchema.ExclusiveMaximum = (bool)property.Value;
                        break;

                    case JsonSchemaConstants.MaximumLengthPropertyName:
                        this.CurrentSchema.MaximumLength = (int)property.Value;
                        break;

                    case JsonSchemaConstants.MinimumLengthPropertyName:
                        this.CurrentSchema.MinimumLength = (int)property.Value;
                        break;

                    case JsonSchemaConstants.MaximumItemsPropertyName:
                        this.CurrentSchema.MaximumItems = (int)property.Value;
                        break;

                    case JsonSchemaConstants.MinimumItemsPropertyName:
                        this.CurrentSchema.MinimumItems = (int)property.Value;
                        break;

                    case JsonSchemaConstants.DivisibleByPropertyName:
                        this.CurrentSchema.DivisibleBy = (double)property.Value;
                        break;

                    case JsonSchemaConstants.DisallowPropertyName:
                        this.CurrentSchema.Disallow = this.ProcessType(property.Value);
                        break;

                    case JsonSchemaConstants.DefaultPropertyName:
                        this.CurrentSchema.Default = property.Value.DeepClone();
                        break;

                    case JsonSchemaConstants.HiddenPropertyName:
                        this.CurrentSchema.Hidden = (bool)property.Value;
                        break;

                    case JsonSchemaConstants.ReadOnlyPropertyName:
                        this.CurrentSchema.ReadOnly = (bool)property.Value;
                        break;

                    case JsonSchemaConstants.FormatPropertyName:
                        this.CurrentSchema.Format = (string)property.Value;
                        break;

                    case JsonSchemaConstants.PatternPropertyName:
                        this.CurrentSchema.Pattern = (string)property.Value;
                        break;

                    case JsonSchemaConstants.EnumPropertyName:
                        this.ProcessEnum(property.Value);
                        break;

                    case JsonSchemaConstants.ExtendsPropertyName:
                        this.ProcessExtends(property.Value);
                        break;

                    case JsonSchemaConstants.UniqueItemsPropertyName:
                        this.CurrentSchema.UniqueItems = (bool)property.Value;
                        break;
                }
            }
        }

        private void ProcessExtends(JToken token)
        {
            IList<JsonSchema> schemas = new List<JsonSchema>();

            if (token.Type == JTokenType.Array)
            {
                foreach (JToken schemaObject in token)
                {
                    schemas.Add(this.BuildSchema(schemaObject));
                }
            }
            else
            {
                JsonSchema schema = this.BuildSchema(token);
                if (schema != null)
                {
                    schemas.Add(schema);
                }
            }

            if (schemas.Count > 0)
            {
                this.CurrentSchema.Extends = schemas;
            }
        }

        private void ProcessEnum(JToken token)
        {
            if (token.Type != JTokenType.Array)
            {
                throw JsonException.Create(token, token.Path, "Expected Array token while parsing enum values, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));
            }

            this.CurrentSchema.Enum = new List<JToken>();

            foreach (JToken enumValue in token)
            {
                this.CurrentSchema.Enum.Add(enumValue.DeepClone());
            }
        }

        private void ProcessAdditionalProperties(JToken token)
        {
            if (token.Type == JTokenType.Boolean)
            {
                this.CurrentSchema.AllowAdditionalProperties = (bool)token;
            }
            else
            {
                this.CurrentSchema.AdditionalProperties = this.BuildSchema(token);
            }
        }

        private void ProcessAdditionalItems(JToken token)
        {
            if (token.Type == JTokenType.Boolean)
            {
                this.CurrentSchema.AllowAdditionalItems = (bool)token;
            }
            else
            {
                this.CurrentSchema.AdditionalItems = this.BuildSchema(token);
            }
        }

        private IDictionary<string, JsonSchema> ProcessProperties(JToken token)
        {
            IDictionary<string, JsonSchema> properties = new Dictionary<string, JsonSchema>();

            if (token.Type != JTokenType.Object)
            {
                throw JsonException.Create(token, token.Path, "Expected Object token while parsing schema properties, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));
            }

            foreach (JProperty propertyToken in token)
            {
                if (properties.ContainsKey(propertyToken.Name))
                {
                    throw new JsonException("Property {0} has already been defined in schema.".FormatWith(CultureInfo.InvariantCulture, propertyToken.Name));
                }

                properties.Add(propertyToken.Name, this.BuildSchema(propertyToken.Value));
            }

            return properties;
        }

        private void ProcessItems(JToken token)
        {
            this.CurrentSchema.Items = new List<JsonSchema>();

            switch (token.Type)
            {
                case JTokenType.Object:
                    this.CurrentSchema.Items.Add(this.BuildSchema(token));
                    this.CurrentSchema.PositionalItemsValidation = false;
                    break;

                case JTokenType.Array:
                    this.CurrentSchema.PositionalItemsValidation = true;
                    foreach (JToken schemaToken in token)
                    {
                        this.CurrentSchema.Items.Add(this.BuildSchema(schemaToken));
                    }
                    break;

                default:
                    throw JsonException.Create(token, token.Path, "Expected array or JSON schema object, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));
            }
        }

        private JsonSchemaType? ProcessType(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Array:
                    // ensure type is in blank state before ORing values
                    JsonSchemaType? type = JsonSchemaType.None;

                    foreach (JToken typeToken in token)
                    {
                        if (typeToken.Type != JTokenType.String)
                        {
                            throw JsonException.Create(typeToken, typeToken.Path, "Expected JSON schema type string token, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));
                        }

                        type = type | MapType((string)typeToken);
                    }

                    return type;

                case JTokenType.String:
                    return MapType((string)token);

                default:
                    throw JsonException.Create(token, token.Path, "Expected array or JSON schema type string token, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));
            }
        }

        internal static JsonSchemaType MapType(string type)
        {
            JsonSchemaType mappedType;
            if (!JsonSchemaConstants.JsonSchemaTypeMapping.TryGetValue(type, out mappedType))
            {
                throw new JsonException("Invalid JSON schema type: {0}".FormatWith(CultureInfo.InvariantCulture, type));
            }

            return mappedType;
        }

        internal static string MapType(JsonSchemaType type)
        {
            return JsonSchemaConstants.JsonSchemaTypeMapping.Single(kv => kv.Value == type).Key;
        }
    }
}