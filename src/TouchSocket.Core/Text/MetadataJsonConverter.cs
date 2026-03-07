//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TouchSocket.Core;

/// <summary>
/// <see cref="Metadata"/> 的 JSON 转换器，将其序列化为 Key/Value 对象数组格式。
/// </summary>
public sealed class MetadataJsonConverter : JsonConverter<Metadata>
{
    /// <inheritdoc/>
    public override Metadata Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }

        var metadata = new Metadata();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            string key = null;
            string value = null;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();
                    if (string.Equals(propertyName, "Key", StringComparison.OrdinalIgnoreCase))
                    {
                        key = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                    }
                    else if (string.Equals(propertyName, "Value", StringComparison.OrdinalIgnoreCase))
                    {
                        value = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                    }
                }
            }

            if (key != null)
            {
                metadata.Add(key, value);
            }
        }

        return metadata;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Metadata value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var item in value)
        {
            writer.WriteStartObject();
            writer.WriteString("Key", item.Key);
            writer.WriteString("Value", item.Value);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }
}
