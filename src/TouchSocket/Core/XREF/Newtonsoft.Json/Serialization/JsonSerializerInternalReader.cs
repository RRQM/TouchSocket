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

#if HAVE_DYNAMIC
using System.ComponentModel;
using System.Dynamic;
#endif

using System.Globalization;

#if HAVE_BIG_INTEGER
using System.Numerics;
#endif

using TouchSocket.Core.XREF.Newtonsoft.Json.Linq;
using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities;

#if !HAVE_LINQ

using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities.LinqBridge;

#else
using System.Linq;

#endif

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Serialization
{
    internal class JsonSerializerInternalReader : JsonSerializerInternalBase
    {
        internal enum PropertyPresence
        {
            None = 0,
            Null = 1,
            Value = 2
        }

        public JsonSerializerInternalReader(JsonSerializer serializer)
            : base(serializer)
        {
        }

        public void Populate(JsonReader reader, object target)
        {
            ValidationUtils.ArgumentNotNull(target, nameof(target));

            Type objectType = target.GetType();

            JsonContract contract = this.Serializer._contractResolver.ResolveContract(objectType);

            if (!reader.MoveToContent())
            {
                throw JsonSerializationException.Create(reader, "No JSON content found.");
            }

            if (reader.TokenType == JsonToken.StartArray)
            {
                if (contract.ContractType == JsonContractType.Array)
                {
                    JsonArrayContract arrayContract = (JsonArrayContract)contract;

                    this.PopulateList((arrayContract.ShouldCreateWrapper) ? arrayContract.CreateWrapper(target) : (IList)target, reader, arrayContract, null, null);
                }
                else
                {
                    throw JsonSerializationException.Create(reader, "Cannot populate JSON array onto type '{0}'.".FormatWith(CultureInfo.InvariantCulture, objectType));
                }
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                reader.ReadAndAssert();

                string id = null;
                if (this.Serializer.MetadataPropertyHandling != MetadataPropertyHandling.Ignore
                    && reader.TokenType == JsonToken.PropertyName
                    && string.Equals(reader.Value.ToString(), JsonTypeReflector.IdPropertyName, StringComparison.Ordinal))
                {
                    reader.ReadAndAssert();
                    id = reader.Value?.ToString();
                    reader.ReadAndAssert();
                }

                if (contract.ContractType == JsonContractType.Dictionary)
                {
                    JsonDictionaryContract dictionaryContract = (JsonDictionaryContract)contract;
                    this.PopulateDictionary((dictionaryContract.ShouldCreateWrapper) ? dictionaryContract.CreateWrapper(target) : (IDictionary)target, reader, dictionaryContract, null, id);
                }
                else if (contract.ContractType == JsonContractType.Object)
                {
                    this.PopulateObject(target, reader, (JsonObjectContract)contract, null, id);
                }
                else
                {
                    throw JsonSerializationException.Create(reader, "Cannot populate JSON object onto type '{0}'.".FormatWith(CultureInfo.InvariantCulture, objectType));
                }
            }
            else
            {
                throw JsonSerializationException.Create(reader, "Unexpected initial token '{0}' when populating object. Expected JSON object or array.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }
        }

        private JsonContract GetContractSafe(Type type)
        {
            if (type == null)
            {
                return null;
            }

            return this.Serializer._contractResolver.ResolveContract(type);
        }

        public object Deserialize(JsonReader reader, Type objectType, bool checkAdditionalContent)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            JsonContract contract = this.GetContractSafe(objectType);

            try
            {
                JsonConverter converter = this.GetConverter(contract, null, null, null);

                if (reader.TokenType == JsonToken.None && !reader.ReadForType(contract, converter != null))
                {
                    if (contract != null && !contract.IsNullable)
                    {
                        throw JsonSerializationException.Create(reader, "No JSON content found and type '{0}' is not nullable.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                    }

                    return null;
                }

                object deserializedValue;

                if (converter != null && converter.CanRead)
                {
                    deserializedValue = this.DeserializeConvertable(converter, reader, objectType, null);
                }
                else
                {
                    deserializedValue = this.CreateValueInternal(reader, objectType, contract, null, null, null, null);
                }

                if (checkAdditionalContent)
                {
                    while (reader.Read())
                    {
                        if (reader.TokenType != JsonToken.Comment)
                        {
                            throw JsonSerializationException.Create(reader, "Additional text found in JSON string after finishing deserializing object.");
                        }
                    }
                }

                return deserializedValue;
            }
            catch (Exception ex)
            {
                if (this.IsErrorHandled(null, contract, null, reader as IJsonLineInfo, reader.Path, ex))
                {
                    this.HandleError(reader, false, 0);
                    return null;
                }
                else
                {
                    // clear context in case serializer is being used inside a converter
                    // if the converter wraps the error then not clearing the context will cause this error:
                    // "Current error context error is different to requested error."
                    this.ClearErrorContext();
                    throw;
                }
            }
        }

        private JsonSerializerProxy GetInternalSerializer()
        {
            if (this.InternalSerializer == null)
            {
                this.InternalSerializer = new JsonSerializerProxy(this);
            }

            return this.InternalSerializer;
        }

        private JToken CreateJToken(JsonReader reader, JsonContract contract)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            if (contract != null)
            {
                if (contract.UnderlyingType == typeof(JRaw))
                {
                    return JRaw.Create(reader);
                }
                if (reader.TokenType == JsonToken.Null
                    && !(contract.UnderlyingType == typeof(JValue) || contract.UnderlyingType == typeof(JToken)))
                {
                    return null;
                }
            }

            JToken token;
            using (JTokenWriter writer = new JTokenWriter())
            {
                writer.WriteToken(reader);
                token = writer.Token;
            }

            return token;
        }

        private JToken CreateJObject(JsonReader reader)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            // this is needed because we've already read inside the object, looking for metadata properties
            using (JTokenWriter writer = new JTokenWriter())
            {
                writer.WriteStartObject();

                do
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        string propertyName = (string)reader.Value;
                        if (!reader.ReadAndMoveToContent())
                        {
                            break;
                        }

                        if (this.CheckPropertyName(reader, propertyName))
                        {
                            continue;
                        }

                        writer.WritePropertyName(propertyName);
                        writer.WriteToken(reader, true, true, false);
                    }
                    else if (reader.TokenType == JsonToken.Comment)
                    {
                        // eat
                    }
                    else
                    {
                        writer.WriteEndObject();
                        return writer.Token;
                    }
                } while (reader.Read());

                throw JsonSerializationException.Create(reader, "Unexpected end when deserializing object.");
            }
        }

        private object CreateValueInternal(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerMember, object existingValue)
        {
            if (contract != null && contract.ContractType == JsonContractType.Linq)
            {
                return this.CreateJToken(reader, contract);
            }

            do
            {
                switch (reader.TokenType)
                {
                    // populate a typed object or generic dictionary/array
                    // depending upon whether an objectType was supplied
                    case JsonToken.StartObject:
                        return this.CreateObject(reader, objectType, contract, member, containerContract, containerMember, existingValue);

                    case JsonToken.StartArray:
                        return this.CreateList(reader, objectType, contract, member, existingValue, null);

                    case JsonToken.Integer:
                    case JsonToken.Float:
                    case JsonToken.Boolean:
                    case JsonToken.Date:
                    case JsonToken.Bytes:
                        return this.EnsureType(reader, reader.Value, CultureInfo.InvariantCulture, contract, objectType);

                    case JsonToken.String:
                        string s = (string)reader.Value;

                        // convert empty string to null automatically for nullable types
                        if (CoerceEmptyStringToNull(objectType, contract, s))
                        {
                            return null;
                        }

                        // string that needs to be returned as a byte array should be base 64 decoded
                        if (objectType == typeof(byte[]))
                        {
                            return Convert.FromBase64String(s);
                        }

                        return this.EnsureType(reader, s, CultureInfo.InvariantCulture, contract, objectType);

                    case JsonToken.StartConstructor:
                        string constructorName = reader.Value.ToString();

                        return this.EnsureType(reader, constructorName, CultureInfo.InvariantCulture, contract, objectType);

                    case JsonToken.Null:
                    case JsonToken.Undefined:
#if HAVE_ADO_NET
                        if (objectType == typeof(DBNull))
                        {
                            return DBNull.Value;
                        }
#endif

                        return this.EnsureType(reader, reader.Value, CultureInfo.InvariantCulture, contract, objectType);

                    case JsonToken.Raw:
                        return new JRaw((string)reader.Value);

                    case JsonToken.Comment:
                        // ignore
                        break;

                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected token while deserializing object: " + reader.TokenType);
                }
            } while (reader.Read());

            throw JsonSerializationException.Create(reader, "Unexpected end when deserializing object.");
        }

        private static bool CoerceEmptyStringToNull(Type objectType, JsonContract contract, string s)
        {
            return string.IsNullOrEmpty(s) && objectType != null && objectType != typeof(string) && objectType != typeof(object) && contract != null && contract.IsNullable;
        }

        internal string GetExpectedDescription(JsonContract contract)
        {
            switch (contract.ContractType)
            {
                case JsonContractType.Object:
                case JsonContractType.Dictionary:
#if HAVE_BINARY_SERIALIZATION
                case JsonContractType.Serializable:
#endif
#if HAVE_DYNAMIC
                case JsonContractType.Dynamic:
#endif
                    return @"JSON object (e.g. {""name"":""value""})";

                case JsonContractType.Array:
                    return @"JSON array (e.g. [1,2,3])";

                case JsonContractType.Primitive:
                    return @"JSON primitive value (e.g. string, number, boolean, null)";

                case JsonContractType.String:
                    return @"JSON string value";

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private JsonConverter GetConverter(JsonContract contract, JsonConverter memberConverter, JsonContainerContract containerContract, JsonProperty containerProperty)
        {
            JsonConverter converter = null;
            if (memberConverter != null)
            {
                // member attribute converter
                converter = memberConverter;
            }
            else if (containerProperty?.ItemConverter != null)
            {
                converter = containerProperty.ItemConverter;
            }
            else if (containerContract?.ItemConverter != null)
            {
                converter = containerContract.ItemConverter;
            }
            else if (contract != null)
            {
                JsonConverter matchingConverter;
                if (contract.Converter != null)
                {
                    // class attribute converter
                    converter = contract.Converter;
                }
                else if ((matchingConverter = this.Serializer.GetMatchingConverter(contract.UnderlyingType)) != null)
                {
                    // passed in converters
                    converter = matchingConverter;
                }
                else if (contract.InternalConverter != null)
                {
                    // internally specified converter
                    converter = contract.InternalConverter;
                }
            }
            return converter;
        }

        private object CreateObject(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerMember, object existingValue)
        {
            string id;
            Type resolvedObjectType = objectType;

            if (this.Serializer.MetadataPropertyHandling == MetadataPropertyHandling.Ignore)
            {
                // don't look for metadata properties
                reader.ReadAndAssert();
                id = null;
            }
            else if (this.Serializer.MetadataPropertyHandling == MetadataPropertyHandling.ReadAhead)
            {
                if (!(reader is JTokenReader tokenReader))
                {
                    JToken t = JToken.ReadFrom(reader);
                    tokenReader = (JTokenReader)t.CreateReader();
                    tokenReader.Culture = reader.Culture;
                    tokenReader.DateFormatString = reader.DateFormatString;
                    tokenReader.DateParseHandling = reader.DateParseHandling;
                    tokenReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
                    tokenReader.FloatParseHandling = reader.FloatParseHandling;
                    tokenReader.SupportMultipleContent = reader.SupportMultipleContent;

                    // start
                    tokenReader.ReadAndAssert();

                    reader = tokenReader;
                }

                if (this.ReadMetadataPropertiesToken(tokenReader, ref resolvedObjectType, ref contract, member, containerContract, containerMember, existingValue, out object newValue, out id))
                {
                    return newValue;
                }
            }
            else
            {
                reader.ReadAndAssert();
                if (this.ReadMetadataProperties(reader, ref resolvedObjectType, ref contract, member, containerContract, containerMember, existingValue, out object newValue, out id))
                {
                    return newValue;
                }
            }

            if (this.HasNoDefinedType(contract))
            {
                return this.CreateJObject(reader);
            }

            switch (contract.ContractType)
            {
                case JsonContractType.Object:
                    {
                        bool createdFromNonDefaultCreator = false;
                        JsonObjectContract objectContract = (JsonObjectContract)contract;
                        object targetObject;
                        // check that if type name handling is being used that the existing value is compatible with the specified type
                        if (existingValue != null && (resolvedObjectType == objectType || resolvedObjectType.IsAssignableFrom(existingValue.GetType())))
                        {
                            targetObject = existingValue;
                        }
                        else
                        {
                            targetObject = this.CreateNewObject(reader, objectContract, member, containerMember, id, out createdFromNonDefaultCreator);
                        }

                        // don't populate if read from non-default creator because the object has already been read
                        if (createdFromNonDefaultCreator)
                        {
                            return targetObject;
                        }

                        return this.PopulateObject(targetObject, reader, objectContract, member, id);
                    }
                case JsonContractType.Primitive:
                    {
                        JsonPrimitiveContract primitiveContract = (JsonPrimitiveContract)contract;
                        // if the content is inside $value then read past it
                        if (this.Serializer.MetadataPropertyHandling != MetadataPropertyHandling.Ignore
                            && reader.TokenType == JsonToken.PropertyName
                            && string.Equals(reader.Value.ToString(), JsonTypeReflector.ValuePropertyName, StringComparison.Ordinal))
                        {
                            reader.ReadAndAssert();

                            // the token should not be an object because the $type value could have been included in the object
                            // without needing the $value property
                            if (reader.TokenType == JsonToken.StartObject)
                            {
                                throw JsonSerializationException.Create(reader, "Unexpected token when deserializing primitive value: " + reader.TokenType);
                            }

                            object value = this.CreateValueInternal(reader, resolvedObjectType, primitiveContract, member, null, null, existingValue);

                            reader.ReadAndAssert();
                            return value;
                        }
                        break;
                    }
                case JsonContractType.Dictionary:
                    {
                        JsonDictionaryContract dictionaryContract = (JsonDictionaryContract)contract;
                        object targetDictionary;

                        if (existingValue == null)
                        {
                            IDictionary dictionary = this.CreateNewDictionary(reader, dictionaryContract, out bool createdFromNonDefaultCreator);

                            if (createdFromNonDefaultCreator)
                            {
                                if (id != null)
                                {
                                    throw JsonSerializationException.Create(reader, "Cannot preserve reference to readonly dictionary, or dictionary created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                                }

                                if (contract.OnSerializingCallbacks.Count > 0)
                                {
                                    throw JsonSerializationException.Create(reader, "Cannot call OnSerializing on readonly dictionary, or dictionary created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                                }

                                if (contract.OnErrorCallbacks.Count > 0)
                                {
                                    throw JsonSerializationException.Create(reader, "Cannot call OnError on readonly list, or dictionary created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                                }

                                if (!dictionaryContract.HasParameterizedCreatorInternal)
                                {
                                    throw JsonSerializationException.Create(reader, "Cannot deserialize readonly or fixed size dictionary: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                                }
                            }

                            this.PopulateDictionary(dictionary, reader, dictionaryContract, member, id);

                            if (createdFromNonDefaultCreator)
                            {
                                ObjectConstructor<object> creator = dictionaryContract.OverrideCreator ?? dictionaryContract.ParameterizedCreator;

                                return creator(dictionary);
                            }
                            else if (dictionary is IWrappedDictionary)
                            {
                                return ((IWrappedDictionary)dictionary).UnderlyingDictionary;
                            }

                            targetDictionary = dictionary;
                        }
                        else
                        {
                            targetDictionary = this.PopulateDictionary(dictionaryContract.ShouldCreateWrapper || !(existingValue is IDictionary) ? dictionaryContract.CreateWrapper(existingValue) : (IDictionary)existingValue, reader, dictionaryContract, member, id);
                        }

                        return targetDictionary;
                    }
#if HAVE_DYNAMIC
                case JsonContractType.Dynamic:
                    JsonDynamicContract dynamicContract = (JsonDynamicContract)contract;
                    return CreateDynamic(reader, dynamicContract, member, id);
#endif
#if HAVE_BINARY_SERIALIZATION
                case JsonContractType.Serializable:
                    JsonISerializableContract serializableContract = (JsonISerializableContract)contract;
                    return CreateISerializable(reader, serializableContract, member, id);
#endif
            }

            string message = @"Cannot deserialize the current JSON object (e.g. {{""name"":""value""}}) into type '{0}' because the type requires a {1} to deserialize correctly." + Environment.NewLine +
                             @"To fix this error either change the JSON to a {1} or change the deserialized type so that it is a normal .NET type (e.g. not a primitive type like integer, not a collection type like an array or List<T>) that can be deserialized from a JSON object. JsonObjectAttribute can also be added to the type to force it to deserialize from a JSON object." + Environment.NewLine;
            message = message.FormatWith(CultureInfo.InvariantCulture, resolvedObjectType, this.GetExpectedDescription(contract));

            throw JsonSerializationException.Create(reader, message);
        }

        private bool ReadMetadataPropertiesToken(JTokenReader reader, ref Type objectType, ref JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerMember, object existingValue, out object newValue, out string id)
        {
            id = null;
            newValue = null;

            if (reader.TokenType == JsonToken.StartObject)
            {
                JObject current = (JObject)reader.CurrentToken;

                JToken refToken = current[JsonTypeReflector.RefPropertyName];
                if (refToken != null)
                {
                    if (refToken.Type != JTokenType.String && refToken.Type != JTokenType.Null)
                    {
                        throw JsonSerializationException.Create(refToken, refToken.Path, "JSON reference {0} property must have a string or null value.".FormatWith(CultureInfo.InvariantCulture, JsonTypeReflector.RefPropertyName), null);
                    }

                    JToken property = refToken.Parent;
                    JToken additionalContent = null;
                    if (property.Next != null)
                    {
                        additionalContent = property.Next;
                    }
                    else if (property.Previous != null)
                    {
                        additionalContent = property.Previous;
                    }

                    string reference = (string)refToken;

                    if (reference != null)
                    {
                        if (additionalContent != null)
                        {
                            throw JsonSerializationException.Create(additionalContent, additionalContent.Path, "Additional content found in JSON reference object. A JSON reference object should only have a {0} property.".FormatWith(CultureInfo.InvariantCulture, JsonTypeReflector.RefPropertyName), null);
                        }

                        newValue = this.Serializer.GetReferenceResolver().ResolveReference(this, reference);

                        if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Info)
                        {
                            this.TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader, reader.Path, "Resolved object reference '{0}' to {1}.".FormatWith(CultureInfo.InvariantCulture, reference, newValue.GetType())), null);
                        }

                        reader.Skip();
                        return true;
                    }
                }
                JToken typeToken = current[JsonTypeReflector.TypePropertyName];
                if (typeToken != null)
                {
                    string qualifiedTypeName = (string)typeToken;
                    JsonReader typeTokenReader = typeToken.CreateReader();
                    typeTokenReader.ReadAndAssert();
                    this.ResolveTypeName(typeTokenReader, ref objectType, ref contract, member, containerContract, containerMember, qualifiedTypeName);

                    JToken valueToken = current[JsonTypeReflector.ValuePropertyName];
                    if (valueToken != null)
                    {
                        while (true)
                        {
                            reader.ReadAndAssert();
                            if (reader.TokenType == JsonToken.PropertyName)
                            {
                                if ((string)reader.Value == JsonTypeReflector.ValuePropertyName)
                                {
                                    return false;
                                }
                            }

                            reader.ReadAndAssert();
                            reader.Skip();
                        }
                    }
                }
                JToken idToken = current[JsonTypeReflector.IdPropertyName];
                if (idToken != null)
                {
                    id = (string)idToken;
                }
                JToken valuesToken = current[JsonTypeReflector.ArrayValuesPropertyName];
                if (valuesToken != null)
                {
                    JsonReader listReader = valuesToken.CreateReader();
                    listReader.ReadAndAssert();
                    newValue = this.CreateList(listReader, objectType, contract, member, existingValue, id);

                    reader.Skip();
                    return true;
                }
            }

            reader.ReadAndAssert();
            return false;
        }

        private bool ReadMetadataProperties(JsonReader reader, ref Type objectType, ref JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerMember, object existingValue, out object newValue, out string id)
        {
            id = null;
            newValue = null;

            if (reader.TokenType == JsonToken.PropertyName)
            {
                string propertyName = reader.Value.ToString();

                if (propertyName.Length > 0 && propertyName[0] == '$')
                {
                    // read metadata properties
                    // $type, $id, $ref, etc
                    bool metadataProperty;

                    do
                    {
                        propertyName = reader.Value.ToString();

                        if (string.Equals(propertyName, JsonTypeReflector.RefPropertyName, StringComparison.Ordinal))
                        {
                            reader.ReadAndAssert();
                            if (reader.TokenType != JsonToken.String && reader.TokenType != JsonToken.Null)
                            {
                                throw JsonSerializationException.Create(reader, "JSON reference {0} property must have a string or null value.".FormatWith(CultureInfo.InvariantCulture, JsonTypeReflector.RefPropertyName));
                            }

                            string reference = reader.Value?.ToString();

                            reader.ReadAndAssert();

                            if (reference != null)
                            {
                                if (reader.TokenType == JsonToken.PropertyName)
                                {
                                    throw JsonSerializationException.Create(reader, "Additional content found in JSON reference object. A JSON reference object should only have a {0} property.".FormatWith(CultureInfo.InvariantCulture, JsonTypeReflector.RefPropertyName));
                                }

                                newValue = this.Serializer.GetReferenceResolver().ResolveReference(this, reference);

                                if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Info)
                                {
                                    this.TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Resolved object reference '{0}' to {1}.".FormatWith(CultureInfo.InvariantCulture, reference, newValue.GetType())), null);
                                }

                                return true;
                            }
                            else
                            {
                                metadataProperty = true;
                            }
                        }
                        else if (string.Equals(propertyName, JsonTypeReflector.TypePropertyName, StringComparison.Ordinal))
                        {
                            reader.ReadAndAssert();
                            string qualifiedTypeName = reader.Value.ToString();

                            this.ResolveTypeName(reader, ref objectType, ref contract, member, containerContract, containerMember, qualifiedTypeName);

                            reader.ReadAndAssert();

                            metadataProperty = true;
                        }
                        else if (string.Equals(propertyName, JsonTypeReflector.IdPropertyName, StringComparison.Ordinal))
                        {
                            reader.ReadAndAssert();

                            id = reader.Value?.ToString();

                            reader.ReadAndAssert();
                            metadataProperty = true;
                        }
                        else if (string.Equals(propertyName, JsonTypeReflector.ArrayValuesPropertyName, StringComparison.Ordinal))
                        {
                            reader.ReadAndAssert();
                            object list = this.CreateList(reader, objectType, contract, member, existingValue, id);
                            reader.ReadAndAssert();
                            newValue = list;
                            return true;
                        }
                        else
                        {
                            metadataProperty = false;
                        }
                    } while (metadataProperty && reader.TokenType == JsonToken.PropertyName);
                }
            }
            return false;
        }

        private void ResolveTypeName(JsonReader reader, ref Type objectType, ref JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerMember, string qualifiedTypeName)
        {
            TypeNameHandling resolvedTypeNameHandling =
                member?.TypeNameHandling
                ?? containerContract?.ItemTypeNameHandling
                ?? containerMember?.ItemTypeNameHandling
                ?? this.Serializer._typeNameHandling;

            if (resolvedTypeNameHandling != TypeNameHandling.None)
            {
                TypeNameKey typeNameKey = ReflectionUtils.SplitFullyQualifiedTypeName(qualifiedTypeName);

                Type specifiedType;
                try
                {
                    specifiedType = this.Serializer._serializationBinder.BindToType(typeNameKey.AssemblyName, typeNameKey.TypeName);
                }
                catch (Exception ex)
                {
                    throw JsonSerializationException.Create(reader, "Error resolving type specified in JSON '{0}'.".FormatWith(CultureInfo.InvariantCulture, qualifiedTypeName), ex);
                }

                if (specifiedType == null)
                {
                    throw JsonSerializationException.Create(reader, "Type specified in JSON '{0}' was not resolved.".FormatWith(CultureInfo.InvariantCulture, qualifiedTypeName));
                }

                if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Verbose)
                {
                    this.TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Resolved type '{0}' to {1}.".FormatWith(CultureInfo.InvariantCulture, qualifiedTypeName, specifiedType)), null);
                }

                if (objectType != null
#if HAVE_DYNAMIC
                    && objectType != typeof(IDynamicMetaObjectProvider)
#endif
                    && !objectType.IsAssignableFrom(specifiedType))
                {
                    throw JsonSerializationException.Create(reader, "Type specified in JSON '{0}' is not compatible with '{1}'.".FormatWith(CultureInfo.InvariantCulture, specifiedType.AssemblyQualifiedName, objectType.AssemblyQualifiedName));
                }

                objectType = specifiedType;
                contract = this.GetContractSafe(specifiedType);
            }
        }

        private JsonArrayContract EnsureArrayContract(JsonReader reader, Type objectType, JsonContract contract)
        {
            if (contract == null)
            {
                throw JsonSerializationException.Create(reader, "Could not resolve type '{0}' to a JsonContract.".FormatWith(CultureInfo.InvariantCulture, objectType));
            }

            if (!(contract is JsonArrayContract arrayContract))
            {
                string message = @"Cannot deserialize the current JSON array (e.g. [1,2,3]) into type '{0}' because the type requires a {1} to deserialize correctly." + Environment.NewLine +
                                 @"To fix this error either change the JSON to a {1} or change the deserialized type to an array or a type that implements a collection interface (e.g. ICollection, IList) like List<T> that can be deserialized from a JSON array. JsonArrayAttribute can also be added to the type to force it to deserialize from a JSON array." + Environment.NewLine;
                message = message.FormatWith(CultureInfo.InvariantCulture, objectType, this.GetExpectedDescription(contract));

                throw JsonSerializationException.Create(reader, message);
            }

            return arrayContract;
        }

        private object CreateList(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, object existingValue, string id)
        {
            object value;

            if (this.HasNoDefinedType(contract))
            {
                return this.CreateJToken(reader, contract);
            }

            JsonArrayContract arrayContract = this.EnsureArrayContract(reader, objectType, contract);

            if (existingValue == null)
            {
                IList list = this.CreateNewList(reader, arrayContract, out bool createdFromNonDefaultCreator);

                if (createdFromNonDefaultCreator)
                {
                    if (id != null)
                    {
                        throw JsonSerializationException.Create(reader, "Cannot preserve reference to array or readonly list, or list created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                    }

                    if (contract.OnSerializingCallbacks.Count > 0)
                    {
                        throw JsonSerializationException.Create(reader, "Cannot call OnSerializing on an array or readonly list, or list created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                    }

                    if (contract.OnErrorCallbacks.Count > 0)
                    {
                        throw JsonSerializationException.Create(reader, "Cannot call OnError on an array or readonly list, or list created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                    }

                    if (!arrayContract.HasParameterizedCreatorInternal && !arrayContract.IsArray)
                    {
                        throw JsonSerializationException.Create(reader, "Cannot deserialize readonly or fixed size list: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                    }
                }

                if (!arrayContract.IsMultidimensionalArray)
                {
                    this.PopulateList(list, reader, arrayContract, member, id);
                }
                else
                {
                    this.PopulateMultidimensionalArray(list, reader, arrayContract, member, id);
                }

                if (createdFromNonDefaultCreator)
                {
                    if (arrayContract.IsMultidimensionalArray)
                    {
                        list = CollectionUtils.ToMultidimensionalArray(list, arrayContract.CollectionItemType, contract.CreatedType.GetArrayRank());
                    }
                    else if (arrayContract.IsArray)
                    {
                        Array a = Array.CreateInstance(arrayContract.CollectionItemType, list.Count);
                        list.CopyTo(a, 0);
                        list = a;
                    }
                    else
                    {
                        ObjectConstructor<object> creator = arrayContract.OverrideCreator ?? arrayContract.ParameterizedCreator;

                        return creator(list);
                    }
                }
                else if (list is IWrappedCollection)
                {
                    return ((IWrappedCollection)list).UnderlyingCollection;
                }

                value = list;
            }
            else
            {
                if (!arrayContract.CanDeserialize)
                {
                    throw JsonSerializationException.Create(reader, "Cannot populate list type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.CreatedType));
                }

                value = this.PopulateList((arrayContract.ShouldCreateWrapper || !(existingValue is IList)) ? arrayContract.CreateWrapper(existingValue) : (IList)existingValue, reader, arrayContract, member, id);
            }

            return value;
        }

        private bool HasNoDefinedType(JsonContract contract)
        {
            return (contract == null || contract.UnderlyingType == typeof(object) || contract.ContractType == JsonContractType.Linq
#if HAVE_DYNAMIC
                    || contract.UnderlyingType == typeof(IDynamicMetaObjectProvider)
#endif
                );
        }

        private object EnsureType(JsonReader reader, object value, CultureInfo culture, JsonContract contract, Type targetType)
        {
            if (targetType == null)
            {
                return value;
            }

            Type valueType = ReflectionUtils.GetObjectType(value);

            // type of value and type of target don't match
            // attempt to convert value's type to target's type
            if (valueType != targetType)
            {
                if (value == null && contract.IsNullable)
                {
                    return null;
                }

                try
                {
                    if (contract.IsConvertable)
                    {
                        JsonPrimitiveContract primitiveContract = (JsonPrimitiveContract)contract;

                        if (contract.IsEnum)
                        {
                            if (value is string)
                            {
                                return EnumUtils.ParseEnum(contract.NonNullableUnderlyingType, value.ToString(), false);
                            }
                            if (ConvertUtils.IsInteger(primitiveContract.TypeCode))
                            {
                                return Enum.ToObject(contract.NonNullableUnderlyingType, value);
                            }
                        }

#if HAVE_BIG_INTEGER
                        if (value is BigInteger integer)
                        {
                            return ConvertUtils.FromBigInteger(integer, contract.NonNullableUnderlyingType);
                        }
#endif

                        // this won't work when converting to a custom IConvertible
                        return Convert.ChangeType(value, contract.NonNullableUnderlyingType, culture);
                    }

                    return ConvertUtils.ConvertOrCast(value, culture, contract.NonNullableUnderlyingType);
                }
                catch (Exception ex)
                {
                    throw JsonSerializationException.Create(reader, "Error converting value {0} to type '{1}'.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.FormatValueForPrint(value), targetType), ex);
                }
            }

            return value;
        }

        private bool SetPropertyValue(JsonProperty property, JsonConverter propertyConverter, JsonContainerContract containerContract, JsonProperty containerProperty, JsonReader reader, object target)
        {
            if (this.CalculatePropertyDetails(property, ref propertyConverter, containerContract, containerProperty, reader, target, out bool useExistingValue, out object currentValue, out JsonContract propertyContract, out bool gottenCurrentValue))
            {
                return false;
            }

            object value;

            if (propertyConverter != null && propertyConverter.CanRead)
            {
                if (!gottenCurrentValue && target != null && property.Readable)
                {
                    currentValue = property.ValueProvider.GetValue(target);
                }

                value = this.DeserializeConvertable(propertyConverter, reader, property.PropertyType, currentValue);
            }
            else
            {
                value = this.CreateValueInternal(reader, property.PropertyType, propertyContract, property, containerContract, containerProperty, (useExistingValue) ? currentValue : null);
            }

            // always set the value if useExistingValue is false,
            // otherwise also set it if CreateValue returns a new value compared to the currentValue
            // this could happen because of a JsonConverter against the type
            if ((!useExistingValue || value != currentValue)
                && this.ShouldSetPropertyValue(property, containerContract as JsonObjectContract, value))
            {
                property.ValueProvider.SetValue(target, value);

                if (property.SetIsSpecified != null)
                {
                    if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Verbose)
                    {
                        this.TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "IsSpecified for property '{0}' on {1} set to true.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, property.DeclaringType)), null);
                    }

                    property.SetIsSpecified(target, true);
                }

                return true;
            }

            // the value wasn't set be JSON was populated onto the existing value
            return useExistingValue;
        }

        private bool CalculatePropertyDetails(JsonProperty property, ref JsonConverter propertyConverter, JsonContainerContract containerContract, JsonProperty containerProperty, JsonReader reader, object target, out bool useExistingValue, out object currentValue, out JsonContract propertyContract, out bool gottenCurrentValue)
        {
            currentValue = null;
            useExistingValue = false;
            propertyContract = null;
            gottenCurrentValue = false;

            if (property.Ignored)
            {
                return true;
            }

            JsonToken tokenType = reader.TokenType;

            if (property.PropertyContract == null)
            {
                property.PropertyContract = this.GetContractSafe(property.PropertyType);
            }

            ObjectCreationHandling objectCreationHandling =
                property.ObjectCreationHandling.GetValueOrDefault(this.Serializer._objectCreationHandling);

            if ((objectCreationHandling != ObjectCreationHandling.Replace)
                && (tokenType == JsonToken.StartArray || tokenType == JsonToken.StartObject || propertyConverter != null)
                && property.Readable)
            {
                currentValue = property.ValueProvider.GetValue(target);
                gottenCurrentValue = true;

                if (currentValue != null)
                {
                    propertyContract = this.GetContractSafe(currentValue.GetType());

                    useExistingValue = (!propertyContract.IsReadOnlyOrFixedSize && !propertyContract.UnderlyingType.IsValueType());
                }
            }

            if (!property.Writable && !useExistingValue)
            {
                return true;
            }

            // test tokenType here because null might not be convertible to some types, e.g. ignoring null when applied to DateTime
            if (tokenType == JsonToken.Null && this.ResolvedNullValueHandling(containerContract as JsonObjectContract, property) == NullValueHandling.Ignore)
            {
                return true;
            }

            // test tokenType here because default value might not be convertible to actual type, e.g. default of "" for DateTime
            if (this.HasFlag(property.DefaultValueHandling.GetValueOrDefault(this.Serializer._defaultValueHandling), DefaultValueHandling.Ignore)
                && !this.HasFlag(property.DefaultValueHandling.GetValueOrDefault(this.Serializer._defaultValueHandling), DefaultValueHandling.Populate)
                && JsonTokenUtils.IsPrimitiveToken(tokenType)
                && MiscellaneousUtils.ValueEquals(reader.Value, property.GetResolvedDefaultValue()))
            {
                return true;
            }

            if (currentValue == null)
            {
                propertyContract = property.PropertyContract;
            }
            else
            {
                propertyContract = this.GetContractSafe(currentValue.GetType());

                if (propertyContract != property.PropertyContract)
                {
                    propertyConverter = this.GetConverter(propertyContract, property.Converter, containerContract, containerProperty);
                }
            }

            return false;
        }

        private void AddReference(JsonReader reader, string id, object value)
        {
            try
            {
                if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Verbose)
                {
                    this.TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Read object reference Id '{0}' for {1}.".FormatWith(CultureInfo.InvariantCulture, id, value.GetType())), null);
                }

                this.Serializer.GetReferenceResolver().AddReference(this, id, value);
            }
            catch (Exception ex)
            {
                throw JsonSerializationException.Create(reader, "Error reading object reference '{0}'.".FormatWith(CultureInfo.InvariantCulture, id), ex);
            }
        }

        private bool HasFlag(DefaultValueHandling value, DefaultValueHandling flag)
        {
            return ((value & flag) == flag);
        }

        private bool ShouldSetPropertyValue(JsonProperty property, JsonObjectContract contract, object value)
        {
            if (value == null && this.ResolvedNullValueHandling(contract, property) == NullValueHandling.Ignore)
            {
                return false;
            }

            if (this.HasFlag(property.DefaultValueHandling.GetValueOrDefault(this.Serializer._defaultValueHandling), DefaultValueHandling.Ignore)
                && !this.HasFlag(property.DefaultValueHandling.GetValueOrDefault(this.Serializer._defaultValueHandling), DefaultValueHandling.Populate)
                && MiscellaneousUtils.ValueEquals(value, property.GetResolvedDefaultValue()))
            {
                return false;
            }

            if (!property.Writable)
            {
                return false;
            }

            return true;
        }

        private IList CreateNewList(JsonReader reader, JsonArrayContract contract, out bool createdFromNonDefaultCreator)
        {
            // some types like non-generic IEnumerable can be serialized but not deserialized
            if (!contract.CanDeserialize)
            {
                throw JsonSerializationException.Create(reader, "Cannot create and populate list type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.CreatedType));
            }

            if (contract.OverrideCreator != null)
            {
                if (contract.HasParameterizedCreator)
                {
                    createdFromNonDefaultCreator = true;
                    return contract.CreateTemporaryCollection();
                }
                else
                {
                    object list = contract.OverrideCreator();

                    if (contract.ShouldCreateWrapper)
                    {
                        list = contract.CreateWrapper(list);
                    }

                    createdFromNonDefaultCreator = false;
                    return (IList)list;
                }
            }
            else if (contract.IsReadOnlyOrFixedSize)
            {
                createdFromNonDefaultCreator = true;
                IList list = contract.CreateTemporaryCollection();

                if (contract.ShouldCreateWrapper)
                {
                    list = contract.CreateWrapper(list);
                }

                return list;
            }
            else if (contract.DefaultCreator != null && (!contract.DefaultCreatorNonPublic || this.Serializer._constructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
            {
                object list = contract.DefaultCreator();

                if (contract.ShouldCreateWrapper)
                {
                    list = contract.CreateWrapper(list);
                }

                createdFromNonDefaultCreator = false;
                return (IList)list;
            }
            else if (contract.HasParameterizedCreatorInternal)
            {
                createdFromNonDefaultCreator = true;
                return contract.CreateTemporaryCollection();
            }
            else
            {
                if (!contract.IsInstantiable)
                {
                    throw JsonSerializationException.Create(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                }

                throw JsonSerializationException.Create(reader, "Unable to find a constructor to use for type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
            }
        }

        private IDictionary CreateNewDictionary(JsonReader reader, JsonDictionaryContract contract, out bool createdFromNonDefaultCreator)
        {
            if (contract.OverrideCreator != null)
            {
                if (contract.HasParameterizedCreator)
                {
                    createdFromNonDefaultCreator = true;
                    return contract.CreateTemporaryDictionary();
                }
                else
                {
                    createdFromNonDefaultCreator = false;
                    return (IDictionary)contract.OverrideCreator();
                }
            }
            else if (contract.IsReadOnlyOrFixedSize)
            {
                createdFromNonDefaultCreator = true;
                return contract.CreateTemporaryDictionary();
            }
            else if (contract.DefaultCreator != null && (!contract.DefaultCreatorNonPublic || this.Serializer._constructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
            {
                object dictionary = contract.DefaultCreator();

                if (contract.ShouldCreateWrapper)
                {
                    dictionary = contract.CreateWrapper(dictionary);
                }

                createdFromNonDefaultCreator = false;
                return (IDictionary)dictionary;
            }
            else if (contract.HasParameterizedCreatorInternal)
            {
                createdFromNonDefaultCreator = true;
                return contract.CreateTemporaryDictionary();
            }
            else
            {
                if (!contract.IsInstantiable)
                {
                    throw JsonSerializationException.Create(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                }

                throw JsonSerializationException.Create(reader, "Unable to find a default constructor to use for type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
            }
        }

        private void OnDeserializing(JsonReader reader, JsonContract contract, object value)
        {
            if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Info)
            {
                this.TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Started deserializing {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);
            }

            contract.InvokeOnDeserializing(value, this.Serializer._context);
        }

        private void OnDeserialized(JsonReader reader, JsonContract contract, object value)
        {
            if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Info)
            {
                this.TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Finished deserializing {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);
            }

            contract.InvokeOnDeserialized(value, this.Serializer._context);
        }

        private object PopulateDictionary(IDictionary dictionary, JsonReader reader, JsonDictionaryContract contract, JsonProperty containerProperty, string id)
        {
            object underlyingDictionary = dictionary is IWrappedDictionary wrappedDictionary ? wrappedDictionary.UnderlyingDictionary : dictionary;

            if (id != null)
            {
                this.AddReference(reader, id, underlyingDictionary);
            }

            this.OnDeserializing(reader, contract, underlyingDictionary);

            int initialDepth = reader.Depth;

            if (contract.KeyContract == null)
            {
                contract.KeyContract = this.GetContractSafe(contract.DictionaryKeyType);
            }

            if (contract.ItemContract == null)
            {
                contract.ItemContract = this.GetContractSafe(contract.DictionaryValueType);
            }

            JsonConverter dictionaryValueConverter = contract.ItemConverter ?? this.GetConverter(contract.ItemContract, null, contract, containerProperty);
            PrimitiveTypeCode keyTypeCode = (contract.KeyContract is JsonPrimitiveContract keyContract) ? keyContract.TypeCode : PrimitiveTypeCode.Empty;

            bool finished = false;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        object keyValue = reader.Value;
                        if (this.CheckPropertyName(reader, keyValue.ToString()))
                        {
                            continue;
                        }

                        try
                        {
                            try
                            {
                                // this is for correctly reading ISO and MS formatted dictionary keys
                                switch (keyTypeCode)
                                {
                                    case PrimitiveTypeCode.DateTime:
                                    case PrimitiveTypeCode.DateTimeNullable:
                                        {
                                            keyValue = DateTimeUtils.TryParseDateTime(keyValue.ToString(), reader.DateTimeZoneHandling, reader.DateFormatString, reader.Culture, out DateTime dt)
                                                ? dt
                                                : this.EnsureType(reader, keyValue, CultureInfo.InvariantCulture, contract.KeyContract, contract.DictionaryKeyType);
                                            break;
                                        }
#if HAVE_DATE_TIME_OFFSET
                                    case PrimitiveTypeCode.DateTimeOffset:
                                    case PrimitiveTypeCode.DateTimeOffsetNullable:
                                    {
                                        keyValue = DateTimeUtils.TryParseDateTimeOffset(keyValue.ToString(), reader.DateFormatString, reader.Culture, out DateTimeOffset dt)
                                            ? dt
                                            : EnsureType(reader, keyValue, CultureInfo.InvariantCulture, contract.KeyContract, contract.DictionaryKeyType);
                                        break;
                                    }
#endif
                                    default:
                                        keyValue = this.EnsureType(reader, keyValue, CultureInfo.InvariantCulture, contract.KeyContract, contract.DictionaryKeyType);
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                throw JsonSerializationException.Create(reader, "Could not convert string '{0}' to dictionary key type '{1}'. Create a TypeConverter to convert from the string to the key type object.".FormatWith(CultureInfo.InvariantCulture, reader.Value, contract.DictionaryKeyType), ex);
                            }

                            if (!reader.ReadForType(contract.ItemContract, dictionaryValueConverter != null))
                            {
                                throw JsonSerializationException.Create(reader, "Unexpected end when deserializing object.");
                            }

                            object itemValue;
                            if (dictionaryValueConverter != null && dictionaryValueConverter.CanRead)
                            {
                                itemValue = this.DeserializeConvertable(dictionaryValueConverter, reader, contract.DictionaryValueType, null);
                            }
                            else
                            {
                                itemValue = this.CreateValueInternal(reader, contract.DictionaryValueType, contract.ItemContract, null, contract, containerProperty, null);
                            }

                            dictionary[keyValue] = itemValue;
                        }
                        catch (Exception ex)
                        {
                            if (this.IsErrorHandled(underlyingDictionary, contract, keyValue, reader as IJsonLineInfo, reader.Path, ex))
                            {
                                this.HandleError(reader, true, initialDepth);
                            }
                            else
                            {
                                throw;
                            }
                        }
                        break;

                    case JsonToken.Comment:
                        break;

                    case JsonToken.EndObject:
                        finished = true;
                        break;

                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
                }
            } while (!finished && reader.Read());

            if (!finished)
            {
                this.ThrowUnexpectedEndException(reader, contract, underlyingDictionary, "Unexpected end when deserializing object.");
            }

            this.OnDeserialized(reader, contract, underlyingDictionary);
            return underlyingDictionary;
        }

        private object PopulateMultidimensionalArray(IList list, JsonReader reader, JsonArrayContract contract, JsonProperty containerProperty, string id)
        {
            int rank = contract.UnderlyingType.GetArrayRank();

            if (id != null)
            {
                this.AddReference(reader, id, list);
            }

            this.OnDeserializing(reader, contract, list);

            JsonContract collectionItemContract = this.GetContractSafe(contract.CollectionItemType);
            JsonConverter collectionItemConverter = this.GetConverter(collectionItemContract, null, contract, containerProperty);

            int? previousErrorIndex = null;
            Stack<IList> listStack = new Stack<IList>();
            listStack.Push(list);
            IList currentList = list;

            bool finished = false;
            do
            {
                int initialDepth = reader.Depth;

                if (listStack.Count == rank)
                {
                    try
                    {
                        if (reader.ReadForType(collectionItemContract, collectionItemConverter != null))
                        {
                            switch (reader.TokenType)
                            {
                                case JsonToken.EndArray:
                                    listStack.Pop();
                                    currentList = listStack.Peek();
                                    previousErrorIndex = null;
                                    break;

                                case JsonToken.Comment:
                                    break;

                                default:
                                    object value;

                                    if (collectionItemConverter != null && collectionItemConverter.CanRead)
                                    {
                                        value = this.DeserializeConvertable(collectionItemConverter, reader, contract.CollectionItemType, null);
                                    }
                                    else
                                    {
                                        value = this.CreateValueInternal(reader, contract.CollectionItemType, collectionItemContract, null, contract, containerProperty, null);
                                    }

                                    currentList.Add(value);
                                    break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        JsonPosition errorPosition = reader.GetPosition(initialDepth);

                        if (this.IsErrorHandled(list, contract, errorPosition.Position, reader as IJsonLineInfo, reader.Path, ex))
                        {
                            this.HandleError(reader, true, initialDepth + 1);

                            if (previousErrorIndex != null && previousErrorIndex == errorPosition.Position)
                            {
                                // reader index has not moved since previous error handling
                                // break out of reading array to prevent infinite loop
                                throw JsonSerializationException.Create(reader, "Infinite loop detected from error handling.", ex);
                            }
                            else
                            {
                                previousErrorIndex = errorPosition.Position;
                            }
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    if (reader.Read())
                    {
                        switch (reader.TokenType)
                        {
                            case JsonToken.StartArray:
                                IList newList = new List<object>();
                                currentList.Add(newList);
                                listStack.Push(newList);
                                currentList = newList;
                                break;

                            case JsonToken.EndArray:
                                listStack.Pop();

                                if (listStack.Count > 0)
                                {
                                    currentList = listStack.Peek();
                                }
                                else
                                {
                                    finished = true;
                                }
                                break;

                            case JsonToken.Comment:
                                break;

                            default:
                                throw JsonSerializationException.Create(reader, "Unexpected token when deserializing multidimensional array: " + reader.TokenType);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            } while (!finished);

            if (!finished)
            {
                this.ThrowUnexpectedEndException(reader, contract, list, "Unexpected end when deserializing array.");
            }

            this.OnDeserialized(reader, contract, list);
            return list;
        }

        private void ThrowUnexpectedEndException(JsonReader reader, JsonContract contract, object currentObject, string message)
        {
            try
            {
                throw JsonSerializationException.Create(reader, message);
            }
            catch (Exception ex)
            {
                if (this.IsErrorHandled(currentObject, contract, null, reader as IJsonLineInfo, reader.Path, ex))
                {
                    this.HandleError(reader, false, 0);
                }
                else
                {
                    throw;
                }
            }
        }

        private object PopulateList(IList list, JsonReader reader, JsonArrayContract contract, JsonProperty containerProperty, string id)
        {
            object underlyingList = list is IWrappedCollection wrappedCollection ? wrappedCollection.UnderlyingCollection : list;

            if (id != null)
            {
                this.AddReference(reader, id, underlyingList);
            }

            // can't populate an existing array
            if (list.IsFixedSize)
            {
                reader.Skip();
                return underlyingList;
            }

            this.OnDeserializing(reader, contract, underlyingList);

            int initialDepth = reader.Depth;

            if (contract.ItemContract == null)
            {
                contract.ItemContract = this.GetContractSafe(contract.CollectionItemType);
            }

            JsonConverter collectionItemConverter = this.GetConverter(contract.ItemContract, null, contract, containerProperty);

            int? previousErrorIndex = null;

            bool finished = false;
            do
            {
                try
                {
                    if (reader.ReadForType(contract.ItemContract, collectionItemConverter != null))
                    {
                        switch (reader.TokenType)
                        {
                            case JsonToken.EndArray:
                                finished = true;
                                break;

                            case JsonToken.Comment:
                                break;

                            default:
                                object value;

                                if (collectionItemConverter != null && collectionItemConverter.CanRead)
                                {
                                    value = this.DeserializeConvertable(collectionItemConverter, reader, contract.CollectionItemType, null);
                                }
                                else
                                {
                                    value = this.CreateValueInternal(reader, contract.CollectionItemType, contract.ItemContract, null, contract, containerProperty, null);
                                }

                                list.Add(value);
                                break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    JsonPosition errorPosition = reader.GetPosition(initialDepth);

                    if (this.IsErrorHandled(underlyingList, contract, errorPosition.Position, reader as IJsonLineInfo, reader.Path, ex))
                    {
                        this.HandleError(reader, true, initialDepth + 1);

                        if (previousErrorIndex != null && previousErrorIndex == errorPosition.Position)
                        {
                            // reader index has not moved since previous error handling
                            // break out of reading array to prevent infinite loop
                            throw JsonSerializationException.Create(reader, "Infinite loop detected from error handling.", ex);
                        }
                        else
                        {
                            previousErrorIndex = errorPosition.Position;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            } while (!finished);

            if (!finished)
            {
                this.ThrowUnexpectedEndException(reader, contract, underlyingList, "Unexpected end when deserializing array.");
            }

            this.OnDeserialized(reader, contract, underlyingList);
            return underlyingList;
        }

#if HAVE_BINARY_SERIALIZATION
        private object CreateISerializable(JsonReader reader, JsonISerializableContract contract, JsonProperty member, string id)
        {
            Type objectType = contract.UnderlyingType;

            if (!JsonTypeReflector.FullyTrusted)
            {
                string message = @"Type '{0}' implements ISerializable but cannot be deserialized using the ISerializable interface because the current application is not fully trusted and ISerializable can expose secure data." + Environment.NewLine +
                                 @"To fix this error either change the environment to be fully trusted, change the application to not deserialize the type, add JsonObjectAttribute to the type or change the JsonSerializer setting ContractResolver to use a new DefaultContractResolver with IgnoreSerializableInterface set to true." + Environment.NewLine;
                message = message.FormatWith(CultureInfo.InvariantCulture, objectType);

                throw JsonSerializationException.Create(reader, message);
            }

            if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
            {
                TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Deserializing {0} using ISerializable constructor.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);
            }

            SerializationInfo serializationInfo = new SerializationInfo(contract.UnderlyingType, new JsonFormatterConverter(this, contract, member));

            bool finished = false;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string memberName = reader.Value.ToString();
                        if (!reader.Read())
                        {
                            throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));
                        }
                        serializationInfo.AddValue(memberName, JToken.ReadFrom(reader));
                        break;

                    case JsonToken.Comment:
                        break;

                    case JsonToken.EndObject:
                        finished = true;
                        break;

                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
                }
            } while (!finished && reader.Read());

            if (!finished)
            {
                ThrowUnexpectedEndException(reader, contract, serializationInfo, "Unexpected end when deserializing object.");
            }

            if (contract.ISerializableCreator == null)
            {
                throw JsonSerializationException.Create(reader, "ISerializable type '{0}' does not have a valid constructor. To correctly implement ISerializable a constructor that takes SerializationInfo and StreamingContext parameters should be present.".FormatWith(CultureInfo.InvariantCulture, objectType));
            }

            object createdObject = contract.ISerializableCreator(serializationInfo, Serializer._context);

            if (id != null)
            {
                AddReference(reader, id, createdObject);
            }

            // these are together because OnDeserializing takes an object but for an ISerializable the object is fully created in the constructor
            OnDeserializing(reader, contract, createdObject);
            OnDeserialized(reader, contract, createdObject);

            return createdObject;
        }

        internal object CreateISerializableItem(JToken token, Type type, JsonISerializableContract contract, JsonProperty member)
        {
            JsonContract itemContract = GetContractSafe(type);
            JsonConverter itemConverter = GetConverter(itemContract, null, contract, member);

            JsonReader tokenReader = token.CreateReader();
            tokenReader.ReadAndAssert(); // Move to first token

            object result;
            if (itemConverter != null && itemConverter.CanRead)
            {
                result = DeserializeConvertable(itemConverter, tokenReader, type, null);
            }
            else
            {
                result = CreateValueInternal(tokenReader, type, itemContract, null, contract, member, null);
            }

            return result;
        }
#endif

#if HAVE_DYNAMIC
        private object CreateDynamic(JsonReader reader, JsonDynamicContract contract, JsonProperty member, string id)
        {
            IDynamicMetaObjectProvider newObject;

            if (!contract.IsInstantiable)
            {
                throw JsonSerializationException.Create(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
            }

            if (contract.DefaultCreator != null &&
                (!contract.DefaultCreatorNonPublic || Serializer._constructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
            {
                newObject = (IDynamicMetaObjectProvider)contract.DefaultCreator();
            }
            else
            {
                throw JsonSerializationException.Create(reader, "Unable to find a default constructor to use for type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
            }

            if (id != null)
            {
                AddReference(reader, id, newObject);
            }

            OnDeserializing(reader, contract, newObject);

            int initialDepth = reader.Depth;

            bool finished = false;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string memberName = reader.Value.ToString();

                        try
                        {
                            if (!reader.Read())
                            {
                                throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));
                            }

                            // first attempt to find a settable property, otherwise fall back to a dynamic set without type
                            JsonProperty property = contract.Properties.GetClosestMatchProperty(memberName);

                            if (property != null && property.Writable && !property.Ignored)
                            {
                                if (property.PropertyContract == null)
                                {
                                    property.PropertyContract = GetContractSafe(property.PropertyType);
                                }

                                JsonConverter propertyConverter = GetConverter(property.PropertyContract, property.Converter, null, null);

                                if (!SetPropertyValue(property, propertyConverter, null, member, reader, newObject))
                                {
                                    reader.Skip();
                                }
                            }
                            else
                            {
                                Type t = (JsonTokenUtils.IsPrimitiveToken(reader.TokenType)) ? reader.ValueType : typeof(IDynamicMetaObjectProvider);

                                JsonContract dynamicMemberContract = GetContractSafe(t);
                                JsonConverter dynamicMemberConverter = GetConverter(dynamicMemberContract, null, null, member);

                                object value;
                                if (dynamicMemberConverter != null && dynamicMemberConverter.CanRead)
                                {
                                    value = DeserializeConvertable(dynamicMemberConverter, reader, t, null);
                                }
                                else
                                {
                                    value = CreateValueInternal(reader, t, dynamicMemberContract, null, null, member, null);
                                }

                                contract.TrySetMember(newObject, memberName, value);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (IsErrorHandled(newObject, contract, memberName, reader as IJsonLineInfo, reader.Path, ex))
                            {
                                HandleError(reader, true, initialDepth);
                            }
                            else
                            {
                                throw;
                            }
                        }
                        break;

                    case JsonToken.EndObject:
                        finished = true;
                        break;

                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
                }
            } while (!finished && reader.Read());

            if (!finished)
            {
                ThrowUnexpectedEndException(reader, contract, newObject, "Unexpected end when deserializing object.");
            }

            OnDeserialized(reader, contract, newObject);

            return newObject;
        }
#endif

        internal class CreatorPropertyContext
        {
            public string Name;
            public JsonProperty Property;
            public JsonProperty ConstructorProperty;
            public PropertyPresence? Presence;
            public object Value;
            public bool Used;
        }

        private object CreateObjectUsingCreatorWithParameters(JsonReader reader, JsonObjectContract contract, JsonProperty containerProperty, ObjectConstructor<object> creator, string id)
        {
            ValidationUtils.ArgumentNotNull(creator, nameof(creator));

            // only need to keep a track of properties' presence if they are required or a value should be defaulted if missing
            bool trackPresence = (contract.HasRequiredOrDefaultValueProperties || this.HasFlag(this.Serializer._defaultValueHandling, DefaultValueHandling.Populate));

            Type objectType = contract.UnderlyingType;

            if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Info)
            {
                string parameters = string.Join(", ", contract.CreatorParameters.Select(p => p.PropertyName)
#if !HAVE_STRING_JOIN_WITH_ENUMERABLE
                    .ToArray()
#endif
                    );
                this.TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Deserializing {0} using creator with parameters: {1}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType, parameters)), null);
            }

            List<CreatorPropertyContext> propertyContexts = this.ResolvePropertyAndCreatorValues(contract, containerProperty, reader, objectType);
            if (trackPresence)
            {
                foreach (JsonProperty property in contract.Properties)
                {
                    if (propertyContexts.All(p => p.Property != property))
                    {
                        propertyContexts.Add(new CreatorPropertyContext
                        {
                            Property = property,
                            Name = property.PropertyName,
                            Presence = PropertyPresence.None
                        });
                    }
                }
            }

            object[] creatorParameterValues = new object[contract.CreatorParameters.Count];

            foreach (CreatorPropertyContext context in propertyContexts)
            {
                // set presence of read values
                if (trackPresence)
                {
                    if (context.Property != null && context.Presence == null)
                    {
                        object v = context.Value;
                        PropertyPresence propertyPresence;
                        if (v == null)
                        {
                            propertyPresence = PropertyPresence.Null;
                        }
                        else if (v is string)
                        {
                            propertyPresence = CoerceEmptyStringToNull(context.Property.PropertyType, context.Property.PropertyContract, (string)v)
                                ? PropertyPresence.Null
                                : PropertyPresence.Value;
                        }
                        else
                        {
                            propertyPresence = PropertyPresence.Value;
                        }

                        context.Presence = propertyPresence;
                    }
                }

                JsonProperty constructorProperty = context.ConstructorProperty;
                if (constructorProperty == null && context.Property != null)
                {
                    constructorProperty = contract.CreatorParameters.ForgivingCaseSensitiveFind(p => p.PropertyName, context.Property.UnderlyingName);
                }

                if (constructorProperty != null && !constructorProperty.Ignored)
                {
                    // handle giving default values to creator parameters
                    // this needs to happen before the call to creator
                    if (trackPresence)
                    {
                        if (context.Presence == PropertyPresence.None || context.Presence == PropertyPresence.Null)
                        {
                            if (constructorProperty.PropertyContract == null)
                            {
                                constructorProperty.PropertyContract = this.GetContractSafe(constructorProperty.PropertyType);
                            }

                            if (this.HasFlag(constructorProperty.DefaultValueHandling.GetValueOrDefault(this.Serializer._defaultValueHandling), DefaultValueHandling.Populate))
                            {
                                context.Value = this.EnsureType(
                                    reader,
                                    constructorProperty.GetResolvedDefaultValue(),
                                    CultureInfo.InvariantCulture,
                                    constructorProperty.PropertyContract,
                                    constructorProperty.PropertyType);
                            }
                        }
                    }

                    int i = contract.CreatorParameters.IndexOf(constructorProperty);
                    creatorParameterValues[i] = context.Value;

                    context.Used = true;
                }
            }

            object createdObject = creator(creatorParameterValues);

            if (id != null)
            {
                this.AddReference(reader, id, createdObject);
            }

            this.OnDeserializing(reader, contract, createdObject);

            // go through unused values and set the newly created object's properties
            foreach (CreatorPropertyContext context in propertyContexts)
            {
                if (context.Used ||
                    context.Property == null ||
                    context.Property.Ignored ||
                    context.Presence == PropertyPresence.None)
                {
                    continue;
                }

                JsonProperty property = context.Property;
                object value = context.Value;

                if (this.ShouldSetPropertyValue(property, contract, value))
                {
                    property.ValueProvider.SetValue(createdObject, value);
                    context.Used = true;
                }
                else if (!property.Writable && value != null)
                {
                    // handle readonly collection/dictionary properties
                    JsonContract propertyContract = this.Serializer._contractResolver.ResolveContract(property.PropertyType);

                    if (propertyContract.ContractType == JsonContractType.Array)
                    {
                        JsonArrayContract propertyArrayContract = (JsonArrayContract)propertyContract;

                        if (propertyArrayContract.CanDeserialize && !propertyArrayContract.IsReadOnlyOrFixedSize)
                        {
                            object createdObjectCollection = property.ValueProvider.GetValue(createdObject);
                            if (createdObjectCollection != null)
                            {
                                IList createdObjectCollectionWrapper = (propertyArrayContract.ShouldCreateWrapper) ? propertyArrayContract.CreateWrapper(createdObjectCollection) : (IList)createdObjectCollection;
                                IList newValues = (propertyArrayContract.ShouldCreateWrapper) ? propertyArrayContract.CreateWrapper(value) : (IList)value;

                                foreach (object newValue in newValues)
                                {
                                    createdObjectCollectionWrapper.Add(newValue);
                                }
                            }
                        }
                    }
                    else if (propertyContract.ContractType == JsonContractType.Dictionary)
                    {
                        JsonDictionaryContract dictionaryContract = (JsonDictionaryContract)propertyContract;

                        if (!dictionaryContract.IsReadOnlyOrFixedSize)
                        {
                            object createdObjectDictionary = property.ValueProvider.GetValue(createdObject);
                            if (createdObjectDictionary != null)
                            {
                                IDictionary targetDictionary = (dictionaryContract.ShouldCreateWrapper) ? dictionaryContract.CreateWrapper(createdObjectDictionary) : (IDictionary)createdObjectDictionary;
                                IDictionary newValues = (dictionaryContract.ShouldCreateWrapper) ? dictionaryContract.CreateWrapper(value) : (IDictionary)value;

                                // Manual use of IDictionaryEnumerator instead of foreach to avoid DictionaryEntry box allocations.
                                IDictionaryEnumerator e = newValues.GetEnumerator();
                                try
                                {
                                    while (e.MoveNext())
                                    {
                                        DictionaryEntry entry = e.Entry;
                                        targetDictionary[entry.Key] = entry.Value;
                                    }
                                }
                                finally
                                {
                                    (e as IDisposable)?.Dispose();
                                }
                            }
                        }
                    }

                    context.Used = true;
                }
            }

            if (contract.ExtensionDataSetter != null)
            {
                foreach (CreatorPropertyContext propertyValue in propertyContexts)
                {
                    if (!propertyValue.Used)
                    {
                        contract.ExtensionDataSetter(createdObject, propertyValue.Name, propertyValue.Value);
                    }
                }
            }

            if (trackPresence)
            {
                foreach (CreatorPropertyContext context in propertyContexts)
                {
                    if (context.Property == null)
                    {
                        continue;
                    }

                    this.EndProcessProperty(
                        createdObject,
                        reader,
                        contract,
                        reader.Depth,
                        context.Property,
                        context.Presence.GetValueOrDefault(),
                        !context.Used);
                }
            }

            this.OnDeserialized(reader, contract, createdObject);
            return createdObject;
        }

        private object DeserializeConvertable(JsonConverter converter, JsonReader reader, Type objectType, object existingValue)
        {
            if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Info)
            {
                this.TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Started deserializing {0} with converter {1}.".FormatWith(CultureInfo.InvariantCulture, objectType, converter.GetType())), null);
            }

            object value = converter.ReadJson(reader, objectType, existingValue, this.GetInternalSerializer());

            if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Info)
            {
                this.TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Finished deserializing {0} with converter {1}.".FormatWith(CultureInfo.InvariantCulture, objectType, converter.GetType())), null);
            }

            return value;
        }

        private List<CreatorPropertyContext> ResolvePropertyAndCreatorValues(JsonObjectContract contract, JsonProperty containerProperty, JsonReader reader, Type objectType)
        {
            List<CreatorPropertyContext> propertyValues = new List<CreatorPropertyContext>();
            bool exit = false;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string memberName = reader.Value.ToString();

                        CreatorPropertyContext creatorPropertyContext = new CreatorPropertyContext
                        {
                            Name = reader.Value.ToString(),
                            ConstructorProperty = contract.CreatorParameters.GetClosestMatchProperty(memberName),
                            Property = contract.Properties.GetClosestMatchProperty(memberName)
                        };
                        propertyValues.Add(creatorPropertyContext);

                        JsonProperty property = creatorPropertyContext.ConstructorProperty ?? creatorPropertyContext.Property;
                        if (property != null && !property.Ignored)
                        {
                            if (property.PropertyContract == null)
                            {
                                property.PropertyContract = this.GetContractSafe(property.PropertyType);
                            }

                            JsonConverter propertyConverter = this.GetConverter(property.PropertyContract, property.Converter, contract, containerProperty);

                            if (!reader.ReadForType(property.PropertyContract, propertyConverter != null))
                            {
                                throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));
                            }

                            if (propertyConverter != null && propertyConverter.CanRead)
                            {
                                creatorPropertyContext.Value = this.DeserializeConvertable(propertyConverter, reader, property.PropertyType, null);
                            }
                            else
                            {
                                creatorPropertyContext.Value = this.CreateValueInternal(reader, property.PropertyType, property.PropertyContract, property, contract, containerProperty, null);
                            }

                            continue;
                        }
                        else
                        {
                            if (!reader.Read())
                            {
                                throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));
                            }

                            if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Verbose)
                            {
                                this.TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Could not find member '{0}' on {1}.".FormatWith(CultureInfo.InvariantCulture, memberName, contract.UnderlyingType)), null);
                            }

                            if (this.Serializer._missingMemberHandling == MissingMemberHandling.Error)
                            {
                                throw JsonSerializationException.Create(reader, "Could not find member '{0}' on object of type '{1}'".FormatWith(CultureInfo.InvariantCulture, memberName, objectType.Name));
                            }
                        }

                        if (contract.ExtensionDataSetter != null)
                        {
                            creatorPropertyContext.Value = this.ReadExtensionDataValue(contract, containerProperty, reader);
                        }
                        else
                        {
                            reader.Skip();
                        }
                        break;

                    case JsonToken.Comment:
                        break;

                    case JsonToken.EndObject:
                        exit = true;
                        break;

                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
                }
            } while (!exit && reader.Read());

            if (!exit)
            {
                this.ThrowUnexpectedEndException(reader, contract, null, "Unexpected end when deserializing object.");
            }

            return propertyValues;
        }

        public object CreateNewObject(JsonReader reader, JsonObjectContract objectContract, JsonProperty containerMember, JsonProperty containerProperty, string id, out bool createdFromNonDefaultCreator)
        {
            object newObject = null;

            if (objectContract.OverrideCreator != null)
            {
                if (objectContract.CreatorParameters.Count > 0)
                {
                    createdFromNonDefaultCreator = true;
                    return this.CreateObjectUsingCreatorWithParameters(reader, objectContract, containerMember, objectContract.OverrideCreator, id);
                }

                newObject = objectContract.OverrideCreator(CollectionUtils.ArrayEmpty<object>());
            }
            else if (objectContract.DefaultCreator != null &&
                     (!objectContract.DefaultCreatorNonPublic || this.Serializer._constructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor || objectContract.ParameterizedCreator == null))
            {
                // use the default constructor if it is...
                // public
                // non-public and the user has change constructor handling settings
                // non-public and there is no other creator
                newObject = objectContract.DefaultCreator();
            }
            else if (objectContract.ParameterizedCreator != null)
            {
                createdFromNonDefaultCreator = true;
                return this.CreateObjectUsingCreatorWithParameters(reader, objectContract, containerMember, objectContract.ParameterizedCreator, id);
            }

            if (newObject == null)
            {
                if (!objectContract.IsInstantiable)
                {
                    throw JsonSerializationException.Create(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, objectContract.UnderlyingType));
                }

                throw JsonSerializationException.Create(reader, "Unable to find a constructor to use for type {0}. A class should either have a default constructor, one constructor with arguments or a constructor marked with the JsonConstructor attribute.".FormatWith(CultureInfo.InvariantCulture, objectContract.UnderlyingType));
            }

            createdFromNonDefaultCreator = false;
            return newObject;
        }

        private object PopulateObject(object newObject, JsonReader reader, JsonObjectContract contract, JsonProperty member, string id)
        {
            this.OnDeserializing(reader, contract, newObject);

            // only need to keep a track of properties' presence if they are required or a value should be defaulted if missing
            Dictionary<JsonProperty, PropertyPresence> propertiesPresence = (contract.HasRequiredOrDefaultValueProperties || this.HasFlag(this.Serializer._defaultValueHandling, DefaultValueHandling.Populate))
                ? contract.Properties.ToDictionary(m => m, m => PropertyPresence.None)
                : null;

            if (id != null)
            {
                this.AddReference(reader, id, newObject);
            }

            int initialDepth = reader.Depth;

            bool finished = false;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        {
                            string memberName = reader.Value.ToString();

                            if (this.CheckPropertyName(reader, memberName))
                            {
                                continue;
                            }

                            try
                            {
                                // attempt exact case match first
                                // then try match ignoring case
                                JsonProperty property = contract.Properties.GetClosestMatchProperty(memberName);

                                if (property == null)
                                {
                                    if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Verbose)
                                    {
                                        this.TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Could not find member '{0}' on {1}".FormatWith(CultureInfo.InvariantCulture, memberName, contract.UnderlyingType)), null);
                                    }

                                    if (this.Serializer._missingMemberHandling == MissingMemberHandling.Error)
                                    {
                                        throw JsonSerializationException.Create(reader, "Could not find member '{0}' on object of type '{1}'".FormatWith(CultureInfo.InvariantCulture, memberName, contract.UnderlyingType.Name));
                                    }

                                    if (!reader.Read())
                                    {
                                        break;
                                    }

                                    this.SetExtensionData(contract, member, reader, memberName, newObject);
                                    continue;
                                }

                                if (property.Ignored || !this.ShouldDeserialize(reader, property, newObject))
                                {
                                    if (!reader.Read())
                                    {
                                        break;
                                    }

                                    this.SetPropertyPresence(reader, property, propertiesPresence);
                                    this.SetExtensionData(contract, member, reader, memberName, newObject);
                                }
                                else
                                {
                                    if (property.PropertyContract == null)
                                    {
                                        property.PropertyContract = this.GetContractSafe(property.PropertyType);
                                    }

                                    JsonConverter propertyConverter = this.GetConverter(property.PropertyContract, property.Converter, contract, member);

                                    if (!reader.ReadForType(property.PropertyContract, propertyConverter != null))
                                    {
                                        throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));
                                    }

                                    this.SetPropertyPresence(reader, property, propertiesPresence);

                                    // set extension data if property is ignored or readonly
                                    if (!this.SetPropertyValue(property, propertyConverter, contract, member, reader, newObject))
                                    {
                                        this.SetExtensionData(contract, member, reader, memberName, newObject);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                if (this.IsErrorHandled(newObject, contract, memberName, reader as IJsonLineInfo, reader.Path, ex))
                                {
                                    this.HandleError(reader, true, initialDepth);
                                }
                                else
                                {
                                    throw;
                                }
                            }
                            break;
                        }
                    case JsonToken.EndObject:
                        finished = true;
                        break;

                    case JsonToken.Comment:
                        // ignore
                        break;

                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
                }
            } while (!finished && reader.Read());

            if (!finished)
            {
                this.ThrowUnexpectedEndException(reader, contract, newObject, "Unexpected end when deserializing object.");
            }

            if (propertiesPresence != null)
            {
                foreach (KeyValuePair<JsonProperty, PropertyPresence> propertyPresence in propertiesPresence)
                {
                    JsonProperty property = propertyPresence.Key;
                    PropertyPresence presence = propertyPresence.Value;

                    this.EndProcessProperty(newObject, reader, contract, initialDepth, property, presence, true);
                }
            }

            this.OnDeserialized(reader, contract, newObject);
            return newObject;
        }

        private bool ShouldDeserialize(JsonReader reader, JsonProperty property, object target)
        {
            if (property.ShouldDeserialize == null)
            {
                return true;
            }

            bool shouldDeserialize = property.ShouldDeserialize(target);

            if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Verbose)
            {
                this.TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, reader.Path, "ShouldDeserialize result for property '{0}' on {1}: {2}".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, property.DeclaringType, shouldDeserialize)), null);
            }

            return shouldDeserialize;
        }

        private bool CheckPropertyName(JsonReader reader, string memberName)
        {
            if (this.Serializer.MetadataPropertyHandling == MetadataPropertyHandling.ReadAhead)
            {
                switch (memberName)
                {
                    case JsonTypeReflector.IdPropertyName:
                    case JsonTypeReflector.RefPropertyName:
                    case JsonTypeReflector.TypePropertyName:
                    case JsonTypeReflector.ArrayValuesPropertyName:
                        reader.Skip();
                        return true;
                }
            }
            return false;
        }

        private void SetExtensionData(JsonObjectContract contract, JsonProperty member, JsonReader reader, string memberName, object o)
        {
            if (contract.ExtensionDataSetter != null)
            {
                try
                {
                    object value = this.ReadExtensionDataValue(contract, member, reader);

                    contract.ExtensionDataSetter(o, memberName, value);
                }
                catch (Exception ex)
                {
                    throw JsonSerializationException.Create(reader, "Error setting value in extension data for type '{0}'.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType), ex);
                }
            }
            else
            {
                reader.Skip();
            }
        }

        private object ReadExtensionDataValue(JsonObjectContract contract, JsonProperty member, JsonReader reader)
        {
            object value;
            if (contract.ExtensionDataIsJToken)
            {
                value = JToken.ReadFrom(reader);
            }
            else
            {
                value = this.CreateValueInternal(reader, null, null, null, contract, member, null);
            }
            return value;
        }

        private void EndProcessProperty(object newObject, JsonReader reader, JsonObjectContract contract, int initialDepth, JsonProperty property, PropertyPresence presence, bool setDefaultValue)
        {
            if (presence == PropertyPresence.None || presence == PropertyPresence.Null)
            {
                try
                {
                    Required resolvedRequired = property.Ignored ? Required.Default : property._required ?? contract.ItemRequired ?? Required.Default;

                    switch (presence)
                    {
                        case PropertyPresence.None:
                            if (resolvedRequired == Required.AllowNull || resolvedRequired == Required.Always)
                            {
                                throw JsonSerializationException.Create(reader, "Required property '{0}' not found in JSON.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));
                            }

                            if (setDefaultValue && !property.Ignored)
                            {
                                if (property.PropertyContract == null)
                                {
                                    property.PropertyContract = this.GetContractSafe(property.PropertyType);
                                }

                                if (this.HasFlag(property.DefaultValueHandling.GetValueOrDefault(this.Serializer._defaultValueHandling), DefaultValueHandling.Populate) && property.Writable)
                                {
                                    property.ValueProvider.SetValue(newObject, this.EnsureType(reader, property.GetResolvedDefaultValue(), CultureInfo.InvariantCulture, property.PropertyContract, property.PropertyType));
                                }
                            }
                            break;

                        case PropertyPresence.Null:
                            if (resolvedRequired == Required.Always)
                            {
                                throw JsonSerializationException.Create(reader, "Required property '{0}' expects a value but got null.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));
                            }
                            if (resolvedRequired == Required.DisallowNull)
                            {
                                throw JsonSerializationException.Create(reader, "Required property '{0}' expects a non-null value.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    if (this.IsErrorHandled(newObject, contract, property.PropertyName, reader as IJsonLineInfo, reader.Path, ex))
                    {
                        this.HandleError(reader, true, initialDepth);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private void SetPropertyPresence(JsonReader reader, JsonProperty property, Dictionary<JsonProperty, PropertyPresence> requiredProperties)
        {
            if (property != null && requiredProperties != null)
            {
                PropertyPresence propertyPresence;
                switch (reader.TokenType)
                {
                    case JsonToken.String:
                        propertyPresence = (CoerceEmptyStringToNull(property.PropertyType, property.PropertyContract, (string)reader.Value))
                            ? PropertyPresence.Null
                            : PropertyPresence.Value;
                        break;

                    case JsonToken.Null:
                    case JsonToken.Undefined:
                        propertyPresence = PropertyPresence.Null;
                        break;

                    default:
                        propertyPresence = PropertyPresence.Value;
                        break;
                }

                requiredProperties[property] = propertyPresence;
            }
        }

        private void HandleError(JsonReader reader, bool readPastError, int initialDepth)
        {
            this.ClearErrorContext();

            if (readPastError)
            {
                reader.Skip();

                while (reader.Depth > initialDepth)
                {
                    if (!reader.Read())
                    {
                        break;
                    }
                }
            }
        }
    }
}