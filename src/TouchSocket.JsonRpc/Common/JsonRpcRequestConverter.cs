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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TouchSocket.JsonRpc;

internal class JsonRpcRequestConverter : JsonConverter<InternalJsonRpcRequest>
{
    public override InternalJsonRpcRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            return default;
        }

        var jsonRpcRequest = new InternalJsonRpcRequest();

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("jsonrpc", out var jsonrpcProp))
        {
            return default;
        }
        jsonRpcRequest.Jsonrpc = jsonrpcProp.GetString();

        if (!root.TryGetProperty("method", out var methodProp))
        {
            return default;
        }
        jsonRpcRequest.Method = methodProp.GetString();

        if (!root.TryGetProperty("params", out var paramsProp))
        {
            return default;
        }
        jsonRpcRequest.ParamsObject = paramsProp.Clone();

        if (!root.TryGetProperty("id", out var idProp))
        {
            return default;
        }

        if (idProp.ValueKind == JsonValueKind.Number)
        {
            jsonRpcRequest.Id = idProp.GetInt32();
        }
        else if (idProp.ValueKind == JsonValueKind.Null)
        {
            jsonRpcRequest.Id = null;
        }

        return jsonRpcRequest;
    }

    public override void Write(Utf8JsonWriter writer, InternalJsonRpcRequest value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        writer.WriteString("jsonrpc", value.Jsonrpc);
        writer.WriteString("method", value.Method);
        
        writer.WritePropertyName("params");
        writer.WriteStartArray();
        if (value.ParamsStrings != null)
        {
            foreach (var item in value.ParamsStrings)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    using var doc = JsonDocument.Parse(item);
                    doc.WriteTo(writer);
                }
                else
                {
                    writer.WriteNullValue();
                }
            }
        }
        writer.WriteEndArray();
        
        if (value.Id.HasValue)
        {
            writer.WriteNumber("id", value.Id.Value);
        }
        else
        {
            writer.WriteNull("id");
        }
        
        writer.WriteEndObject();
    }
}