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

using System.Buffers;
using System.Text.Json;

namespace TouchSocket.JsonRpc;

/// <summary>
/// 提供 JsonRpc 协议消息的低分配序列化与反序列化能力，支持 AOT。
/// </summary>
internal sealed class JsonRpcConverter
{
    private static ReadOnlySpan<byte> JsonrpcBytes => "jsonrpc"u8;
    private static ReadOnlySpan<byte> MethodBytes => "method"u8;
    private static ReadOnlySpan<byte> ParamsBytes => "params"u8;
    private static ReadOnlySpan<byte> IdBytes => "id"u8;
    private static ReadOnlySpan<byte> ResultBytes => "result"u8;
    private static ReadOnlySpan<byte> ErrorBytes => "error"u8;
    private static ReadOnlySpan<byte> CodeBytes => "code"u8;
    private static ReadOnlySpan<byte> MessageBytes => "message"u8;
    private static ReadOnlySpan<byte> Version2_0Bytes => "2.0"u8;

    /// <summary>
    /// 将请求序列化写入 <see cref="IBufferWriter{T}"/>。直接接收对象以避免中间字符串分配。
    /// </summary>
    public void WriteRequest(IBufferWriter<byte> bufferWriter, string method, int? id, object[] parameters, JsonSerializerOptions options)
    {
        using var writer = new Utf8JsonWriter(bufferWriter);
        writer.WriteStartObject();
        writer.WriteString(JsonrpcBytes, Version2_0Bytes);
        writer.WriteString(MethodBytes, method);
        writer.WritePropertyName(ParamsBytes);
        writer.WriteStartArray();
        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                if (param != null)
                {
                    JsonSerializer.Serialize(writer, param, param.GetType(), options);
                }
                else
                {
                    writer.WriteNullValue();
                }
            }
        }
        writer.WriteEndArray();
        if (id.HasValue)
        {
            writer.WriteNumber(IdBytes, id.Value);
        }
        else
        {
            writer.WriteNull(IdBytes);
        }
        writer.WriteEndObject();
    }

    /// <summary>
    /// 将响应序列化写入 <see cref="IBufferWriter{T}"/>。直接接收对象以避免中间字符串分配。
    /// </summary>
    public void WriteResponse(IBufferWriter<byte> bufferWriter, int? id, object result, int errorCode, string errorMessage, JsonSerializerOptions options)
    {
        using var writer = new Utf8JsonWriter(bufferWriter);
        writer.WriteStartObject();
        writer.WriteString(JsonrpcBytes, Version2_0Bytes);
        if (id.HasValue)
        {
            writer.WriteNumber(IdBytes, id.Value);
        }
        else
        {
            writer.WriteNull(IdBytes);
        }
        if (errorCode == 0)
        {
            writer.WritePropertyName(ResultBytes);
            if (result != null)
            {
                JsonSerializer.Serialize(writer, result, result.GetType(), options);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
        else
        {
            writer.WritePropertyName(ErrorBytes);
            writer.WriteStartObject();
            writer.WriteNumber(CodeBytes, errorCode);
            writer.WriteString(MessageBytes, errorMessage);
            writer.WriteEndObject();
        }
        writer.WriteEndObject();
    }

    /// <summary>
    /// 从字节数据中读取 <see cref="InternalJsonRpcRequest"/>。
    /// </summary>
    public bool TryReadRequest(ReadOnlySpan<byte> data, out InternalJsonRpcRequest request)
    {
        var reader = new Utf8JsonReader(data);
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
        {
            request = default;
            return false;
        }

        var result = new InternalJsonRpcRequest();
        var hasMethod = false;
        var hasParams = false;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                request = default;
                return false;
            }

            if (reader.ValueTextEquals(JsonrpcBytes))
            {
                reader.Read();
                result.Jsonrpc = reader.GetString();
            }
            else if (reader.ValueTextEquals(MethodBytes))
            {
                reader.Read();
                result.Method = reader.GetString();
                hasMethod = true;
            }
            else if (reader.ValueTextEquals(ParamsBytes))
            {
                reader.Read();
                result.ParamsObject = JsonElement.ParseValue(ref reader);
                hasParams = true;
            }
            else if (reader.ValueTextEquals(IdBytes))
            {
                reader.Read();
                if (reader.TokenType == JsonTokenType.Number)
                {
                    result.Id = reader.GetInt32();
                }
                // null id 保持为 null
            }
            else
            {
                reader.Read();
                reader.Skip();
            }
        }

        if (!hasMethod || !hasParams)
        {
            request = default;
            return false;
        }

        request = result;
        return true;
    }

    /// <summary>
    /// 从字节数据中读取 <see cref="JsonRpcWaitResult"/>。
    /// </summary>
    public bool TryReadResponse(ReadOnlySpan<byte> data, out JsonRpcWaitResult response)
    {
        var reader = new Utf8JsonReader(data);
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
        {
            response = default;
            return false;
        }

        var result = new JsonRpcWaitResult();
        var hasId = false;
        var hasResultOrError = false;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                response = default;
                return false;
            }

            if (reader.ValueTextEquals(JsonrpcBytes))
            {
                reader.Read();
                result.Jsonrpc = reader.GetString();
            }
            else if (reader.ValueTextEquals(IdBytes))
            {
                reader.Read();
                if (reader.TokenType == JsonTokenType.Number)
                {
                    result.Id = reader.GetInt32();
                }
                // null id 保持 null
                hasId = true;
            }
            else if (reader.ValueTextEquals(ResultBytes))
            {
                reader.Read();
                if (reader.TokenType != JsonTokenType.Null)
                {
                    using var doc = JsonDocument.ParseValue(ref reader);
                    result.Result = doc.RootElement.GetRawText();
                }
                hasResultOrError = true;
            }
            else if (reader.ValueTextEquals(ErrorBytes))
            {
                reader.Read(); // StartObject
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.ValueTextEquals(CodeBytes))
                    {
                        reader.Read();
                        result.ErrorCode = reader.GetInt32();
                    }
                    else if (reader.ValueTextEquals(MessageBytes))
                    {
                        reader.Read();
                        result.ErrorMessage = reader.GetString();
                    }
                    else
                    {
                        reader.Read();
                        reader.Skip();
                    }
                }
                hasResultOrError = true;
            }
            else
            {
                reader.Read();
                reader.Skip();
            }
        }

        if (!hasId || !hasResultOrError)
        {
            response = default;
            return false;
        }

        response = result;
        return true;
    }
}