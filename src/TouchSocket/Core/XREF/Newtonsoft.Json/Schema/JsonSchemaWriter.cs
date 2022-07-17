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
using TouchSocket.Core.XREF.Newtonsoft.Json.Linq;
using TouchSocket.Core.XREF.Newtonsoft.Json.Serialization;
using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities;

#if !HAVE_LINQ

using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities.LinqBridge;

#else
using System.Linq;

#endif

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Schema
{
    [Obsolete("JSON Schema validation has been moved to its own package. See http://www.newtonsoft.com/jsonschema for more details.")]
    internal class JsonSchemaWriter
    {
        private readonly JsonWriter _writer;
        private readonly JsonSchemaResolver _resolver;

        public JsonSchemaWriter(JsonWriter writer, JsonSchemaResolver resolver)
        {
            ValidationUtils.ArgumentNotNull(writer, nameof(writer));
            this._writer = writer;
            this._resolver = resolver;
        }

        private void ReferenceOrWriteSchema(JsonSchema schema)
        {
            if (schema.Id != null && this._resolver.GetSchema(schema.Id) != null)
            {
                this._writer.WriteStartObject();
                this._writer.WritePropertyName(JsonTypeReflector.RefPropertyName);
                this._writer.WriteValue(schema.Id);
                this._writer.WriteEndObject();
            }
            else
            {
                this.WriteSchema(schema);
            }
        }

        public void WriteSchema(JsonSchema schema)
        {
            ValidationUtils.ArgumentNotNull(schema, nameof(schema));

            if (!this._resolver.LoadedSchemas.Contains(schema))
            {
                this._resolver.LoadedSchemas.Add(schema);
            }

            this._writer.WriteStartObject();
            this.WritePropertyIfNotNull(this._writer, JsonSchemaConstants.IdPropertyName, schema.Id);
            this.WritePropertyIfNotNull(this._writer, JsonSchemaConstants.TitlePropertyName, schema.Title);
            this.WritePropertyIfNotNull(this._writer, JsonSchemaConstants.DescriptionPropertyName, schema.Description);
            this.WritePropertyIfNotNull(this._writer, JsonSchemaConstants.RequiredPropertyName, schema.Required);
            this.WritePropertyIfNotNull(this._writer, JsonSchemaConstants.ReadOnlyPropertyName, schema.ReadOnly);
            this.WritePropertyIfNotNull(this._writer, JsonSchemaConstants.HiddenPropertyName, schema.Hidden);
            this.WritePropertyIfNotNull(this._writer, JsonSchemaConstants.TransientPropertyName, schema.Transient);
            if (schema.Type != null)
            {
                this.WriteType(JsonSchemaConstants.TypePropertyName, this._writer, schema.Type.GetValueOrDefault());
            }
            if (!schema.AllowAdditionalProperties)
            {
                this._writer.WritePropertyName(JsonSchemaConstants.AdditionalPropertiesPropertyName);
                this._writer.WriteValue(schema.AllowAdditionalProperties);
            }
            else
            {
                if (schema.AdditionalProperties != null)
                {
                    this._writer.WritePropertyName(JsonSchemaConstants.AdditionalPropertiesPropertyName);
                    this.ReferenceOrWriteSchema(schema.AdditionalProperties);
                }
            }
            if (!schema.AllowAdditionalItems)
            {
                this._writer.WritePropertyName(JsonSchemaConstants.AdditionalItemsPropertyName);
                this._writer.WriteValue(schema.AllowAdditionalItems);
            }
            else
            {
                if (schema.AdditionalItems != null)
                {
                    this._writer.WritePropertyName(JsonSchemaConstants.AdditionalItemsPropertyName);
                    this.ReferenceOrWriteSchema(schema.AdditionalItems);
                }
            }
            this.WriteSchemaDictionaryIfNotNull(this._writer, JsonSchemaConstants.PropertiesPropertyName, schema.Properties);
            this.WriteSchemaDictionaryIfNotNull(this._writer, JsonSchemaConstants.PatternPropertiesPropertyName, schema.PatternProperties);
            this.WriteItems(schema);
            this.WritePropertyIfNotNull(this._writer, JsonSchemaConstants.MinimumPropertyName, schema.Minimum);
            this.WritePropertyIfNotNull(this._writer, JsonSchemaConstants.MaximumPropertyName, schema.Maximum);
            this.WritePropertyIfNotNull(this._writer, JsonSchemaConstants.ExclusiveMinimumPropertyName, schema.ExclusiveMinimum);
            this.WritePropertyIfNotNull(this._writer, JsonSchemaConstants.ExclusiveMaximumPropertyName, schema.ExclusiveMaximum);
            this.WritePropertyIfNotNull(this._writer, JsonSchemaConstants.MinimumLengthPropertyName, schema.MinimumLength);
            this.WritePropertyIfNotNull(this._writer, JsonSchemaConstants.MaximumLengthPropertyName, schema.MaximumLength);
            this.WritePropertyIfNotNull(this._writer, JsonSchemaConstants.MinimumItemsPropertyName, schema.MinimumItems);
            this.WritePropertyIfNotNull(this._writer, JsonSchemaConstants.MaximumItemsPropertyName, schema.MaximumItems);
            this.WritePropertyIfNotNull(this._writer, JsonSchemaConstants.DivisibleByPropertyName, schema.DivisibleBy);
            this.WritePropertyIfNotNull(this._writer, JsonSchemaConstants.FormatPropertyName, schema.Format);
            this.WritePropertyIfNotNull(this._writer, JsonSchemaConstants.PatternPropertyName, schema.Pattern);
            if (schema.Enum != null)
            {
                this._writer.WritePropertyName(JsonSchemaConstants.EnumPropertyName);
                this._writer.WriteStartArray();
                foreach (JToken token in schema.Enum)
                {
                    token.WriteTo(this._writer);
                }
                this._writer.WriteEndArray();
            }
            if (schema.Default != null)
            {
                this._writer.WritePropertyName(JsonSchemaConstants.DefaultPropertyName);
                schema.Default.WriteTo(this._writer);
            }
            if (schema.Disallow != null)
            {
                this.WriteType(JsonSchemaConstants.DisallowPropertyName, this._writer, schema.Disallow.GetValueOrDefault());
            }
            if (schema.Extends != null && schema.Extends.Count > 0)
            {
                this._writer.WritePropertyName(JsonSchemaConstants.ExtendsPropertyName);
                if (schema.Extends.Count == 1)
                {
                    this.ReferenceOrWriteSchema(schema.Extends[0]);
                }
                else
                {
                    this._writer.WriteStartArray();
                    foreach (JsonSchema jsonSchema in schema.Extends)
                    {
                        this.ReferenceOrWriteSchema(jsonSchema);
                    }
                    this._writer.WriteEndArray();
                }
            }
            this._writer.WriteEndObject();
        }

        private void WriteSchemaDictionaryIfNotNull(JsonWriter writer, string propertyName, IDictionary<string, JsonSchema> properties)
        {
            if (properties != null)
            {
                writer.WritePropertyName(propertyName);
                writer.WriteStartObject();
                foreach (KeyValuePair<string, JsonSchema> property in properties)
                {
                    writer.WritePropertyName(property.Key);
                    this.ReferenceOrWriteSchema(property.Value);
                }
                writer.WriteEndObject();
            }
        }

        private void WriteItems(JsonSchema schema)
        {
            if (schema.Items == null && !schema.PositionalItemsValidation)
            {
                return;
            }

            this._writer.WritePropertyName(JsonSchemaConstants.ItemsPropertyName);

            if (!schema.PositionalItemsValidation)
            {
                if (schema.Items != null && schema.Items.Count > 0)
                {
                    this.ReferenceOrWriteSchema(schema.Items[0]);
                }
                else
                {
                    this._writer.WriteStartObject();
                    this._writer.WriteEndObject();
                }
                return;
            }

            this._writer.WriteStartArray();
            if (schema.Items != null)
            {
                foreach (JsonSchema itemSchema in schema.Items)
                {
                    this.ReferenceOrWriteSchema(itemSchema);
                }
            }
            this._writer.WriteEndArray();
        }

        private void WriteType(string propertyName, JsonWriter writer, JsonSchemaType type)
        {
            if (Enum.IsDefined(typeof(JsonSchemaType), type))
            {
                writer.WritePropertyName(propertyName);
                writer.WriteValue(JsonSchemaBuilder.MapType(type));
            }
            else
            {
                IEnumerator<JsonSchemaType> en = EnumUtils.GetFlagsValues(type).Where(v => v != JsonSchemaType.None).GetEnumerator();
                if (en.MoveNext())
                {
                    writer.WritePropertyName(propertyName);
                    JsonSchemaType first = en.Current;
                    if (en.MoveNext())
                    {
                        writer.WriteStartArray();
                        writer.WriteValue(JsonSchemaBuilder.MapType(first));
                        do
                        {
                            writer.WriteValue(JsonSchemaBuilder.MapType(en.Current));
                        } while (en.MoveNext());
                        writer.WriteEndArray();
                    }
                    else
                    {
                        writer.WriteValue(JsonSchemaBuilder.MapType(first));
                    }
                }
            }
        }

        private void WritePropertyIfNotNull(JsonWriter writer, string propertyName, object value)
        {
            if (value != null)
            {
                writer.WritePropertyName(propertyName);
                writer.WriteValue(value);
            }
        }
    }
}