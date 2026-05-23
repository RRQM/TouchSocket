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

using System.Text.Json.Serialization;

namespace TouchSocket.Mcp;

/// <summary>
/// 表示 MCP 实现信息（名称和版本）。
/// </summary>
public sealed class McpImplementationInfo
{
    /// <summary>
    /// 获取或设置实现名称。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 获取或设置实现版本。
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; }
}

/// <summary>
/// 表示 MCP 客户端能力。
/// </summary>
public sealed class McpClientCapabilities
{
    /// <summary>
    /// 获取或设置根目录能力，表示客户端可提供文件系统根目录。
    /// </summary>
    [JsonPropertyName("roots")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public McpRootsCapability Roots { get; set; }

    /// <summary>
    /// 获取或设置采样能力，表示客户端支持 LLM 采样请求。
    /// </summary>
    [JsonPropertyName("sampling")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object Sampling { get; set; }

    /// <summary>
    /// 获取或设置实验性能力。
    /// </summary>
    [JsonPropertyName("experimental")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object Experimental { get; set; }
}

/// <summary>
/// 表示根目录能力配置。
/// </summary>
public sealed class McpRootsCapability
{
    /// <summary>
    /// 获取或设置一个值，指示根目录列表是否支持变更通知。
    /// </summary>
    [JsonPropertyName("listChanged")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ListChanged { get; set; }
}

/// <summary>
/// 表示 MCP 服务器能力。
/// </summary>
public sealed class McpServerCapabilities
{
    /// <summary>
    /// 获取或设置工具能力，非 <see langword="null"/> 则表示服务器支持工具功能。
    /// </summary>
    [JsonPropertyName("tools")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public McpToolsCapability Tools { get; set; }

    /// <summary>
    /// 获取或设置资源能力，非 <see langword="null"/> 则表示服务器支持资源功能。
    /// </summary>
    [JsonPropertyName("resources")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public McpResourcesCapability Resources { get; set; }

    /// <summary>
    /// 获取或设置提示模板能力，非 <see langword="null"/> 则表示服务器支持提示模板功能。
    /// </summary>
    [JsonPropertyName("prompts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public McpPromptsCapability Prompts { get; set; }

    /// <summary>
    /// 获取或设置日志能力，非 <see langword="null"/> 则表示服务器支持结构化日志。
    /// </summary>
    [JsonPropertyName("logging")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object Logging { get; set; }

    /// <summary>
    /// 获取或设置实验性能力。
    /// </summary>
    [JsonPropertyName("experimental")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object Experimental { get; set; }
}

/// <summary>
/// 表示工具能力配置。
/// </summary>
public sealed class McpToolsCapability
{
    /// <summary>
    /// 获取或设置一个值，指示工具列表是否支持变更通知。
    /// </summary>
    [JsonPropertyName("listChanged")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ListChanged { get; set; }
}

/// <summary>
/// 表示资源能力配置。
/// </summary>
public sealed class McpResourcesCapability
{
    /// <summary>
    /// 获取或设置一个值，指示是否支持订阅单个资源变更。
    /// </summary>
    [JsonPropertyName("subscribe")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Subscribe { get; set; }

    /// <summary>
    /// 获取或设置一个值，指示资源列表是否支持变更通知。
    /// </summary>
    [JsonPropertyName("listChanged")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ListChanged { get; set; }
}

/// <summary>
/// 表示提示模板能力配置。
/// </summary>
public sealed class McpPromptsCapability
{
    /// <summary>
    /// 获取或设置一个值，指示提示模板列表是否支持变更通知。
    /// </summary>
    [JsonPropertyName("listChanged")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ListChanged { get; set; }
}

/// <summary>
/// 表示 initialize 请求的参数。
/// </summary>
public sealed class McpInitializeParams
{
    /// <summary>
    /// 获取或设置客户端支持的协议版本。
    /// </summary>
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; }

    /// <summary>
    /// 获取或设置客户端能力。
    /// </summary>
    [JsonPropertyName("capabilities")]
    public McpClientCapabilities Capabilities { get; set; }

    /// <summary>
    /// 获取或设置客户端实现信息。
    /// </summary>
    [JsonPropertyName("clientInfo")]
    public McpImplementationInfo ClientInfo { get; set; }
}

/// <summary>
/// 表示 initialize 响应结果。
/// </summary>
public sealed class McpInitializeResult
{
    /// <summary>
    /// 获取或设置服务器协商的协议版本。
    /// </summary>
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; }

    /// <summary>
    /// 获取或设置服务器能力。
    /// </summary>
    [JsonPropertyName("capabilities")]
    public McpServerCapabilities Capabilities { get; set; }

    /// <summary>
    /// 获取或设置服务器实现信息。
    /// </summary>
    [JsonPropertyName("serverInfo")]
    public McpImplementationInfo ServerInfo { get; set; }

    /// <summary>
    /// 获取或设置可选的客户端操作说明。
    /// </summary>
    [JsonPropertyName("instructions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Instructions { get; set; }
}
