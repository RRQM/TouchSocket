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

namespace TouchSocket.Mcp;

/// <summary>
/// 表示 MCP 协议常量。
/// </summary>
public static class McpProtocolVersion
{
    /// <summary>
    /// MCP 协议版本。
    /// </summary>
    public const string Latest = "2025-03-26";
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
    private static readonly JsonSerializerOptions s_options = new JsonSerializerOptions
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// 将对象序列化为 UTF-8 JSON 字节数组。
    /// </summary>
    /// <param name="value">要序列化的对象。</param>
    /// <returns>UTF-8 编码的 JSON 字节数组。</returns>
    public static byte[] SerializeToBytes(object value)
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, value.GetType(), s_options);
    }

    /// <summary>
    /// 将 UTF-8 JSON 字节反序列化为指定类型。
    /// </summary>
    /// <typeparam name="T">目标类型。</typeparam>
    /// <param name="bytes">UTF-8 编码的 JSON 字节。</param>
    /// <returns>反序列化后的对象。</returns>
    public static T Deserialize<T>(ReadOnlySpan<byte> bytes)
    {
        return JsonSerializer.Deserialize<T>(bytes, s_options);
    }

    /// <summary>
    /// 将对象序列化为 JSON 字符串。
    /// </summary>
    /// <param name="value">要序列化的对象。</param>
    /// <returns>JSON 字符串。</returns>
    public static string SerializeToString(object value)
    {
        return JsonSerializer.Serialize(value, value.GetType(), s_options);
    }

    /// <summary>
    /// 将 JSON 字符串反序列化为指定类型。
    /// </summary>
    /// <typeparam name="T">目标类型。</typeparam>
    /// <param name="json">JSON 字符串。</param>
    /// <returns>反序列化后的对象。</returns>
    public static T DeserializeFromString<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, s_options);
    }

    /// <summary>
    /// 尝试解析传入消息的类型（请求、响应或通知）。
    /// </summary>
    /// <param name="bytes">UTF-8 JSON 数据。</param>
    /// <param name="request">如果是请求，输出解析结果；否则为 <see langword="null"/>。</param>
    /// <param name="response">如果是响应，输出解析结果；否则为 <see langword="null"/>。</param>
    /// <param name="notification">如果是通知，输出解析结果；否则为 <see langword="null"/>。</param>
    /// <returns><see langword="true"/> 表示成功解析；否则为 <see langword="false"/>。</returns>
    public static bool TryParseMessage(
        ReadOnlySpan<byte> bytes,
        out McpRequest request,
        out McpResponse response,
        out McpNotification notification)
    {
        request = null;
        response = null;
        notification = null;

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
                request = JsonSerializer.Deserialize<McpRequest>(bytes, s_options);
                return true;
            }
            else if (hasMethod && !hasId)
            {
                notification = JsonSerializer.Deserialize<McpNotification>(bytes, s_options);
                return true;
            }
            else if (hasId && (hasResult || hasError))
            {
                response = JsonSerializer.Deserialize<McpResponse>(bytes, s_options);
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
    /// 构建成功响应消息。
    /// </summary>
    /// <param name="id">请求 Id。</param>
    /// <param name="result">结果对象。</param>
    /// <returns>序列化后的响应字节。</returns>
    public static byte[] BuildSuccessResponse(JsonElement? id, object result)
    {
        var resultElement = result != null
            ? JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(result, result.GetType(), s_options)).RootElement
            : (JsonElement?)null;

        var response = new McpResponse
        {
            Id = id,
            Result = resultElement
        };
        return SerializeToBytes(response);
    }

    /// <summary>
    /// 构建错误响应消息。
    /// </summary>
    /// <param name="id">请求 Id。</param>
    /// <param name="code">错误码。</param>
    /// <param name="message">错误信息。</param>
    /// <returns>序列化后的响应字节。</returns>
    public static byte[] BuildErrorResponse(JsonElement? id, int code, string message)
    {
        var response = new McpResponse
        {
            Id = id,
            Error = new McpError { Code = code, Message = message }
        };
        return SerializeToBytes(response);
    }

    /// <summary>
    /// 构建通知消息。
    /// </summary>
    /// <param name="method">方法名。</param>
    /// <param name="params">参数对象，可为 <see langword="null"/>。</param>
    /// <returns>序列化后的通知字节。</returns>
    public static byte[] BuildNotification(string method, object @params = null)
    {
        JsonElement? paramsElement = null;
        if (@params != null)
        {
            paramsElement = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(@params, @params.GetType(), s_options)).RootElement;
        }

        var notification = new McpNotification
        {
            Method = method,
            Params = paramsElement
        };
        return SerializeToBytes(notification);
    }
}
