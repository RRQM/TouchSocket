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
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using TouchSocket.Core.XREF.Newtonsoft.Json.Bson;
using TouchSocket.Core.XREF.Newtonsoft.Json.Serialization;
using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities;

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Converters
{
    /// <summary>
    /// Converts a <see cref="Regex"/> to and from JSON and BSON.
    /// </summary>
    public class RegexConverter : JsonConverter
    {
        private const string PatternName = "Pattern";
        private const string OptionsName = "Options";

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            Regex regex = (Regex)value;

#pragma warning disable 618
            if (writer is BsonWriter bsonWriter)
            {
                this.WriteBson(bsonWriter, regex);
            }
#pragma warning restore 618
            else
            {
                this.WriteJson(writer, regex, serializer);
            }
        }

        private bool HasFlag(RegexOptions options, RegexOptions flag)
        {
            return ((options & flag) == flag);
        }

#pragma warning disable 618

        private void WriteBson(BsonWriter writer, Regex regex)
        {
            // Regular expression - The first cstring is the regex pattern, the second
            // is the regex options string. Options are identified by characters, which
            // must be stored in alphabetical order. Valid options are 'i' for case
            // insensitive matching, 'm' for multiline matching, 'x' for verbose mode,
            // 'l' to make \w, \W, etc. locale dependent, 's' for dotall mode
            // ('.' matches everything), and 'u' to make \w, \W, etc. match unicode.

            string options = null;

            if (this.HasFlag(regex.Options, RegexOptions.IgnoreCase))
            {
                options += "i";
            }

            if (this.HasFlag(regex.Options, RegexOptions.Multiline))
            {
                options += "m";
            }

            if (this.HasFlag(regex.Options, RegexOptions.Singleline))
            {
                options += "s";
            }

            options += "u";

            if (this.HasFlag(regex.Options, RegexOptions.ExplicitCapture))
            {
                options += "x";
            }

            writer.WriteRegex(regex.ToString(), options);
        }

#pragma warning restore 618

        private void WriteJson(JsonWriter writer, Regex regex, JsonSerializer serializer)
        {
            DefaultContractResolver resolver = serializer.ContractResolver as DefaultContractResolver;

            writer.WriteStartObject();
            writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(PatternName) : PatternName);
            writer.WriteValue(regex.ToString());
            writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(OptionsName) : OptionsName);
            serializer.Serialize(writer, regex.Options);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    return this.ReadRegexObject(reader, serializer);

                case JsonToken.String:
                    return this.ReadRegexString(reader);

                case JsonToken.Null:
                    return null;
            }

            throw JsonSerializationException.Create(reader, "Unexpected token when reading Regex.");
        }

        private object ReadRegexString(JsonReader reader)
        {
            string regexText = (string)reader.Value;

            if (regexText.Length > 0 && regexText[0] == '/')
            {
                int patternOptionDelimiterIndex = regexText.LastIndexOf('/');

                if (patternOptionDelimiterIndex > 0)
                {
                    string patternText = regexText.Substring(1, patternOptionDelimiterIndex - 1);
                    string optionsText = regexText.Substring(patternOptionDelimiterIndex + 1);

                    RegexOptions options = MiscellaneousUtils.GetRegexOptions(optionsText);

                    return new Regex(patternText, options);
                }
            }

            throw JsonSerializationException.Create(reader, "Regex pattern must be enclosed by slashes.");
        }

        private Regex ReadRegexObject(JsonReader reader, JsonSerializer serializer)
        {
            string pattern = null;
            RegexOptions? options = null;

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string propertyName = reader.Value.ToString();

                        if (!reader.Read())
                        {
                            throw JsonSerializationException.Create(reader, "Unexpected end when reading Regex.");
                        }

                        if (string.Equals(propertyName, PatternName, StringComparison.OrdinalIgnoreCase))
                        {
                            pattern = (string)reader.Value;
                        }
                        else if (string.Equals(propertyName, OptionsName, StringComparison.OrdinalIgnoreCase))
                        {
                            options = serializer.Deserialize<RegexOptions>(reader);
                        }
                        else
                        {
                            reader.Skip();
                        }
                        break;

                    case JsonToken.Comment:
                        break;

                    case JsonToken.EndObject:
                        if (pattern == null)
                        {
                            throw JsonSerializationException.Create(reader, "Error deserializing Regex. No pattern found.");
                        }

                        return new Regex(pattern, options ?? RegexOptions.None);
                }
            }

            throw JsonSerializationException.Create(reader, "Unexpected end when reading Regex.");
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType.Name == nameof(Regex) && this.IsRegex(objectType);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool IsRegex(Type objectType)
        {
            return (objectType == typeof(Regex));
        }
    }
}