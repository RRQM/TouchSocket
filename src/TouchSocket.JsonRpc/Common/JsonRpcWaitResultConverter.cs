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

internal class JsonRpcWaitResultConverter : JsonConverter<JsonRpcWaitResult>
{
    public override JsonRpcWaitResult Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            return default;
        }

        var jsonRpcWaitResult = new JsonRpcWaitResult();

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (root.TryGetProperty("jsonrpc", out var jsonrpcProp))
        {
            jsonRpcWaitResult.Jsonrpc = jsonrpcProp.GetString();
        }

        if (root.TryGetProperty("id", out var idProp))
        {
            if (idProp.ValueKind == JsonValueKind.Number)
            {
                jsonRpcWaitResult.Id = idProp.GetInt32();
            }
            else if (idProp.ValueKind == JsonValueKind.Null)
            {
                jsonRpcWaitResult.Id = null;
            }
        }

        if (root.TryGetProperty("error", out var errorProp))
        {
            if (errorProp.TryGetProperty("code", out var codeProp))
            {
                jsonRpcWaitResult.ErrorCode = codeProp.GetInt32();
            }

            if (errorProp.TryGetProperty("message", out var messageProp))
            {
                jsonRpcWaitResult.ErrorMessage = messageProp.GetString();
            }
        }

        if (root.TryGetProperty("result", out var resultProp))
        {
            jsonRpcWaitResult.Result = resultProp.GetRawText();
        }

        return jsonRpcWaitResult;
    }

    public override void Write(Utf8JsonWriter writer, JsonRpcWaitResult value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        writer.WriteString("jsonrpc", value.Jsonrpc);
        
        if (value.Id.HasValue)
        {
            writer.WriteNumber("id", value.Id.Value);
        }
        else
        {
            writer.WriteNull("id");
        }
        
        if (value.ErrorCode == 0)
        {
            writer.WritePropertyName("result");
            if (!string.IsNullOrEmpty(value.Result))
            {
                using var doc = JsonDocument.Parse(value.Result);
                doc.WriteTo(writer);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
        else
        {
            writer.WritePropertyName("error");
            writer.WriteStartObject();
            writer.WriteNumber("code", value.ErrorCode);
            writer.WriteString("message", value.ErrorMessage);
            writer.WriteEndObject();
        }
        
        writer.WriteEndObject();
    }
}