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
using System.Text.Json.Serialization.Metadata;

namespace TouchSocket.Mcp;

/// <summary>
/// 表示 MCP 协议常量。
/// </summary>
public static class McpProtocolVersion
{
    /// <summary>
    /// MCP 协议版本。
    /// </summary>
    public const string Latest = "2025-11-25";
}

/// <summary>
/// 表示 JSON-RPC 请求消息。
/// </summary>
public sealed class McpRequest
{
    /// <summary>
    /// 获取或设置 JSON-RPC 版本，固定为 "2.0"。
    /// </summary>
    [JsonPropertyName("jsonrpc")]
    public string Jsonrpc { get; set; } = "2.0";

    /// <summary>
    /// 获取或设置请求标识符，可以是数字或字符串。
    /// </summary>
    [JsonPropertyName("id")]
    public JsonElement? Id { get; set; }

    /// <summary>
    /// 获取或设置方法名称。
    /// </summary>
    [JsonPropertyName("method")]
    public string Method { get; set; }

    /// <summary>
    /// 获取或设置请求参数。
    /// </summary>
    [JsonPropertyName("params")]
    public JsonElement? Params { get; set; }
}

/// <summary>
/// 表示 JSON-RPC 响应消息。
/// </summary>
public sealed class McpResponse
{
    /// <summary>
    /// 获取或设置 JSON-RPC 版本，固定为 "2.0"。
    /// </summary>
    [JsonPropertyName("jsonrpc")]
    public string Jsonrpc { get; set; } = "2.0";

    /// <summary>
    /// 获取或设置响应标识符，与对应请求的 Id 相同。
    /// </summary>
    [JsonPropertyName("id")]
    public JsonElement? Id { get; set; }

    /// <summary>
    /// 获取或设置成功结果，与 <see cref="Error"/> 互斥。
    /// </summary>
    [JsonPropertyName("result")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? Result { get; set; }

    /// <summary>
    /// 获取或设置错误信息，与 <see cref="Result"/> 互斥。
    /// </summary>
    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public McpError Error { get; set; }

    /// <summary>
    /// 获取一个值，指示响应是否成功。
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => this.Error == null;
}

/// <summary>
/// 表示 JSON-RPC 通知消息（无需响应）。
/// </summary>
public sealed class McpNotification
{
    /// <summary>
    /// 获取或设置 JSON-RPC 版本，固定为 "2.0"。
    /// </summary>
    [JsonPropertyName("jsonrpc")]
    public string Jsonrpc { get; set; } = "2.0";

    /// <summary>
    /// 获取或设置方法名称。
    /// </summary>
    [JsonPropertyName("method")]
    public string Method { get; set; }

    /// <summary>
    /// 获取或设置通知参数。
    /// </summary>
    [JsonPropertyName("params")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? Params { get; set; }
}

/// <summary>
/// 表示 JSON-RPC 错误对象。
/// </summary>
public sealed class McpError
{
    /// <summary>
    /// 获取或设置错误代码。
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 获取或设置错误消息。
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }

    /// <summary>
    /// 获取或设置附加错误数据。
    /// </summary>
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? Data { get; set; }
}

/// <summary>
/// 表示空对象结果。
/// </summary>
public sealed class McpEmptyResult
{
}

/// <summary>
/// 定义 MCP 标准错误码常量。
/// </summary>
public static class McpErrorCodes
{
    /// <summary>
    /// 解析错误：收到的 JSON 无效。
    /// </summary>
    public const int ParseError = -32700;

    /// <summary>
    /// 无效请求：JSON 不是有效的请求对象。
    /// </summary>
    public const int InvalidRequest = -32600;

    /// <summary>
    /// 方法未找到：请求的方法不存在。
    /// </summary>
    public const int MethodNotFound = -32601;

    /// <summary>
    /// 无效参数：方法参数无效。
    /// </summary>
    public const int InvalidParams = -32602;

    /// <summary>
    /// 内部错误：JSON-RPC 内部错误。
    /// </summary>
    public const int InternalError = -32603;

    /// <summary>
    /// 资源未找到。
    /// </summary>
    public const int ResourceNotFound = -32002;
}

/// <summary>
/// 提供 MCP JSON-RPC 消息的序列化与反序列化工具方法。
/// </summary>
public static class McpMessageSerializer
{
    private static readonly JsonSerializerOptions s_options = McpOptionsBase.CreateDefaultJsonSerializerOptions();

    private static JsonTypeInfo GetJsonTypeInfo(Type type, JsonSerializerOptions options)
    {
        return (options ?? s_options).GetTypeInfo(type);
    }

    private static JsonTypeInfo GetJsonTypeInfo(object value, JsonSerializerOptions options)
    {
        return GetJsonTypeInfo(value?.GetType() ?? typeof(object), options);
    }

    /// <summary>
    /// Serializes an object to UTF-8 JSON bytes.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The UTF-8 JSON bytes.</returns>
    public static byte[] SerializeToBytes(object value)
    {
        return SerializeToBytes(value, s_options);
    }

    /// <summary>
    /// Serializes an object to UTF-8 JSON bytes with the specified serializer options.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options.</param>
    /// <returns>The UTF-8 JSON bytes.</returns>
    public static byte[] SerializeToBytes(object value, JsonSerializerOptions jsonSerializerOptions)
    {
        var options = jsonSerializerOptions ?? s_options;
        return JsonSerializer.SerializeToUtf8Bytes(value, GetJsonTypeInfo(value, options));
    }

    /// <summary>
    /// Deserializes UTF-8 JSON bytes to the specified type.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="bytes">The UTF-8 JSON bytes.</param>
    /// <returns>The deserialized object.</returns>
    public static T Deserialize<T>(ReadOnlySpan<byte> bytes)
    {
        return Deserialize<T>(bytes, s_options);
    }

    /// <summary>
    /// Deserializes UTF-8 JSON bytes to the specified type with the specified serializer options.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="bytes">The UTF-8 JSON bytes.</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options.</param>
    /// <returns>The deserialized object.</returns>
    public static T Deserialize<T>(ReadOnlySpan<byte> bytes, JsonSerializerOptions jsonSerializerOptions)
    {
        var options = jsonSerializerOptions ?? s_options;
        return (T)JsonSerializer.Deserialize(bytes, GetJsonTypeInfo(typeof(T), options));
    }

    /// <summary>
    /// Serializes an object to a JSON string.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The JSON string.</returns>
    public static string SerializeToString(object value)
    {
        return SerializeToString(value, s_options);
    }

    /// <summary>
    /// Serializes an object to a JSON string with the specified serializer options.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options.</param>
    /// <returns>The JSON string.</returns>
    public static string SerializeToString(object value, JsonSerializerOptions jsonSerializerOptions)
    {
        var options = jsonSerializerOptions ?? s_options;
        return JsonSerializer.Serialize(value, GetJsonTypeInfo(value, options));
    }

    /// <summary>
    /// Deserializes a JSON string to the specified type.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="json">The JSON string.</param>
    /// <returns>The deserialized object.</returns>
    public static T DeserializeFromString<T>(string json)
    {
        return DeserializeFromString<T>(json, s_options);
    }

    /// <summary>
    /// Deserializes a JSON string to the specified type with the specified serializer options.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="json">The JSON string.</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options.</param>
    /// <returns>The deserialized object.</returns>
    public static T DeserializeFromString<T>(string json, JsonSerializerOptions jsonSerializerOptions)
    {
        var options = jsonSerializerOptions ?? s_options;
        return (T)JsonSerializer.Deserialize(json, GetJsonTypeInfo(typeof(T), options));
    }

    /// <summary>
    /// Attempts to parse an MCP message as a request, response, or notification.
    /// </summary>
    /// <param name="bytes">The UTF-8 JSON bytes.</param>
    /// <param name="request">The parsed request, or <see langword="null"/>.</param>
    /// <param name="response">The parsed response, or <see langword="null"/>.</param>
    /// <param name="notification">The parsed notification, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the message was parsed successfully; otherwise, <see langword="false"/>.</returns>
    public static bool TryParseMessage(
        ReadOnlySpan<byte> bytes,
        out McpRequest request,
        out McpResponse response,
        out McpNotification notification)
    {
        return TryParseMessage(bytes, out request, out response, out notification, s_options);
    }

    /// <summary>
    /// Attempts to parse an MCP message as a request, response, or notification with the specified serializer options.
    /// </summary>
    /// <param name="bytes">The UTF-8 JSON bytes.</param>
    /// <param name="request">The parsed request, or <see langword="null"/>.</param>
    /// <param name="response">The parsed response, or <see langword="null"/>.</param>
    /// <param name="notification">The parsed notification, or <see langword="null"/>.</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options.</param>
    /// <returns><see langword="true"/> if the message was parsed successfully; otherwise, <see langword="false"/>.</returns>
    public static bool TryParseMessage(
        ReadOnlySpan<byte> bytes,
        out McpRequest request,
        out McpResponse response,
        out McpNotification notification,
        JsonSerializerOptions jsonSerializerOptions)
    {
        request = null;
        response = null;
        notification = null;
        var options = jsonSerializerOptions ?? s_options;

        try
        {
            var reader = new Utf8JsonReader(bytes);
            var hasMethod = false;
            var hasId = false;
            var hasResult = false;
            var hasError = false;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    if (reader.ValueTextEquals("method"u8))
                    {
                        hasMethod = true;
                    }
                    else if (reader.ValueTextEquals("id"u8))
                    {
                        hasId = true;
                    }
                    else if (reader.ValueTextEquals("result"u8))
                    {
                        hasResult = true;
                    }
                    else if (reader.ValueTextEquals("error"u8))
                    {
                        hasError = true;
                    }
                }
            }

            if (hasMethod && hasId)
            {
                request = (McpRequest)JsonSerializer.Deserialize(bytes, GetJsonTypeInfo(typeof(McpRequest), options));
                return true;
            }
            else if (hasMethod && !hasId)
            {
                notification = (McpNotification)JsonSerializer.Deserialize(bytes, GetJsonTypeInfo(typeof(McpNotification), options));
                return true;
            }
            else if (hasId && (hasResult || hasError))
            {
                response = (McpResponse)JsonSerializer.Deserialize(bytes, GetJsonTypeInfo(typeof(McpResponse), options));
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Builds a success response message.
    /// </summary>
    /// <param name="id">The request id.</param>
    /// <param name="result">The result object.</param>
    /// <returns>The serialized response bytes.</returns>
    public static byte[] BuildSuccessResponse(JsonElement? id, object result)
    {
        return BuildSuccessResponse(id, result, s_options);
    }

    /// <summary>
    /// Builds a success response message with the specified serializer options.
    /// </summary>
    /// <param name="id">The request id.</param>
    /// <param name="result">The result object.</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options.</param>
    /// <returns>The serialized response bytes.</returns>
    public static byte[] BuildSuccessResponse(JsonElement? id, object result, JsonSerializerOptions jsonSerializerOptions)
    {
        var options = jsonSerializerOptions ?? s_options;
        JsonElement? resultElement = null;
        if (result != null)
        {
            using var resultDocument = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(result, GetJsonTypeInfo(result, options)));
            resultElement = resultDocument.RootElement.Clone();
        }

        var response = new McpResponse
        {
            Id = id,
            Result = resultElement
        };
        return SerializeToBytes(response, options);
    }

    /// <summary>
    /// Builds an error response message.
    /// </summary>
    /// <param name="id">The request id.</param>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The serialized response bytes.</returns>
    public static byte[] BuildErrorResponse(JsonElement? id, int code, string message)
    {
        return BuildErrorResponse(id, code, message, s_options);
    }

    /// <summary>
    /// Builds an error response message with the specified serializer options.
    /// </summary>
    /// <param name="id">The request id.</param>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options.</param>
    /// <returns>The serialized response bytes.</returns>
    public static byte[] BuildErrorResponse(JsonElement? id, int code, string message, JsonSerializerOptions jsonSerializerOptions)
    {
        var response = new McpResponse
        {
            Id = id,
            Error = new McpError { Code = code, Message = message }
        };
        return SerializeToBytes(response, jsonSerializerOptions);
    }

    /// <summary>
    /// Builds a notification message.
    /// </summary>
    /// <param name="method">The method name.</param>
    /// <param name="params">The parameter object, or <see langword="null"/>.</param>
    /// <returns>The serialized notification bytes.</returns>
    public static byte[] BuildNotification(string method, object @params = null)
    {
        return BuildNotification(method, @params, s_options);
    }

    /// <summary>
    /// Builds a notification message with the specified serializer options.
    /// </summary>
    /// <param name="method">The method name.</param>
    /// <param name="params">The parameter object, or <see langword="null"/>.</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options.</param>
    /// <returns>The serialized notification bytes.</returns>
    public static byte[] BuildNotification(string method, object @params, JsonSerializerOptions jsonSerializerOptions)
    {
        var options = jsonSerializerOptions ?? s_options;
        JsonElement? paramsElement = null;
        if (@params != null)
        {
            using var paramsDocument = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(@params, GetJsonTypeInfo(@params, options)));
            paramsElement = paramsDocument.RootElement.Clone();
        }

        var notification = new McpNotification
        {
            Method = method,
            Params = paramsElement
        };
        return SerializeToBytes(notification, options);
    }
}
