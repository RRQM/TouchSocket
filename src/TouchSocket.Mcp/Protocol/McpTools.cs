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
/// 表示 MCP 工具定义。
/// </summary>
public sealed class McpToolDefinition
{
    /// <summary>
    /// 获取或设置工具的显示标题。
    /// </summary>
    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Title { get; set; }

    /// <summary>
    /// 获取或设置工具的唯一名称。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 获取或设置工具的人类可读描述。
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Description { get; set; }

    /// <summary>
    /// 获取或设置工具输入参数的 JSON Schema。
    /// </summary>
    [JsonPropertyName("inputSchema")]
    public McpToolInputSchema InputSchema { get; set; }

    /// <summary>
    /// 获取或设置工具结构化输出的 JSON Schema。
    /// </summary>
    [JsonPropertyName("outputSchema")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public McpToolProperty OutputSchema { get; set; }

    /// <summary>
    /// 获取或设置工具图标集合。
    /// </summary>
    [JsonPropertyName("icons")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<McpIcon> Icons { get; set; }

    /// <summary>
    /// 获取或设置工具执行相关属性。
    /// </summary>
    [JsonPropertyName("execution")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public McpToolExecution Execution { get; set; }

    /// <summary>
    /// 获取或设置工具的可选注解。
    /// </summary>
    [JsonPropertyName("annotations")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public McpToolAnnotations Annotations { get; set; }
}

/// <summary>
/// 表示工具输入参数的 JSON Schema 定义。
/// </summary>
public sealed class McpToolInputSchema
{
    /// <summary>
    /// 获取或设置 JSON Schema 类型，固定为 "object"。
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "object";

    /// <summary>
    /// 获取或设置参数属性的 Schema 映射，键为参数名称。
    /// </summary>
    [JsonPropertyName("properties")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, McpToolProperty> Properties { get; set; }

    /// <summary>
    /// 获取或设置必填参数名称列表。
    /// </summary>
    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> Required { get; set; }
}

/// <summary>
/// 表示工具单个参数的 JSON Schema 属性。
/// </summary>
public sealed class McpToolProperty
{
    /// <summary>
    /// 获取或设置参数的 JSON 类型，如 "string"、"integer"、"number"、"boolean"、"object"、"array"。
    /// </summary>
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Type { get; set; }

    /// <summary>
    /// 获取或设置参数的描述。
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Description { get; set; }

    /// <summary>
    /// 获取或设置对象属性的子级 Schema。
    /// </summary>
    [JsonPropertyName("properties")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, McpToolProperty> Properties { get; set; }

    /// <summary>
    /// 获取或设置数组元素的 Schema。
    /// </summary>
    [JsonPropertyName("items")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public McpToolProperty Items { get; set; }

    /// <summary>
    /// 获取或设置对象必填属性名称列表。
    /// </summary>
    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> Required { get; set; }

    /// <summary>
    /// 获取或设置枚举允许值列表。
    /// </summary>
    [JsonPropertyName("enum")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object[] Enum { get; set; }
}

/// <summary>
/// 表示工具的可选注解信息。
/// </summary>
public sealed class McpToolAnnotations
{
    /// <summary>
    /// 获取或设置工具的显示标题（供人类阅读）。
    /// </summary>
    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Title { get; set; }

    /// <summary>
    /// 获取或设置一个值，指示工具是否为只读（不修改状态）。
    /// </summary>
    [JsonPropertyName("readOnlyHint")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ReadOnlyHint { get; set; }

    /// <summary>
    /// 获取或设置一个值，指示工具是否有破坏性操作。
    /// </summary>
    [JsonPropertyName("destructiveHint")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? DestructiveHint { get; set; }

    /// <summary>
    /// 获取或设置一个值，指示工具是否幂等。
    /// </summary>
    [JsonPropertyName("idempotentHint")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IdempotentHint { get; set; }

    /// <summary>
    /// 获取或设置一个值，指示工具是否与外部系统交互。
    /// </summary>
    [JsonPropertyName("openWorldHint")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? OpenWorldHint { get; set; }
}

/// <summary>
/// 表示工具执行相关属性。
/// </summary>
public sealed class McpToolExecution
{
    /// <summary>
    /// 获取或设置任务增强执行支持级别，取值通常为 "forbidden"、"optional" 或 "required"。
    /// </summary>
    [JsonPropertyName("taskSupport")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string TaskSupport { get; set; }
}

/// <summary>
/// 表示分页列表请求的参数。
/// </summary>
public sealed class McpListRequestParams
{
    /// <summary>
    /// 获取或设置分页游标。
    /// </summary>
    [JsonPropertyName("cursor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Cursor { get; set; }
}

/// <summary>
/// 表示 tools/call 请求的参数。
/// </summary>
public sealed class McpCallToolParams
{
    /// <summary>
    /// 获取或设置要调用的工具名称。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 获取或设置工具调用参数（JSON 对象）。
    /// </summary>
    [JsonPropertyName("arguments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? Arguments { get; set; }

    /// <summary>
    /// 获取或设置任务增强请求元数据。
    /// </summary>
    [JsonPropertyName("task")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public McpTaskMetadata Task { get; set; }

    /// <summary>
    /// 获取或设置请求元数据。
    /// </summary>
    [JsonPropertyName("_meta")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public McpRequestMeta Meta { get; set; }
}

/// <summary>
/// 表示 tools/call 的响应结果。
/// </summary>
public sealed class McpCallToolResult
{
    /// <summary>
    /// 获取或设置工具调用返回的内容列表。
    /// </summary>
    [JsonPropertyName("content")]
    public List<McpContent> Content { get; set; } = new List<McpContent>();

    /// <summary>
    /// 获取或设置工具调用返回的结构化内容。
    /// </summary>
    [JsonPropertyName("structuredContent")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? StructuredContent { get; set; }

    /// <summary>
    /// 获取或设置一个值，指示工具执行是否发生错误。
    /// </summary>
    [JsonPropertyName("isError")]
    public bool IsError { get; set; }
}

/// <summary>
/// 表示 tools/list 的响应结果。
/// </summary>
public sealed class McpListToolsResult
{
    /// <summary>
    /// 获取或设置工具列表。
    /// </summary>
    [JsonPropertyName("tools")]
    public List<McpToolDefinition> Tools { get; set; } = new List<McpToolDefinition>();

    /// <summary>
    /// 获取或设置下一页游标，用于分页。
    /// </summary>
    [JsonPropertyName("nextCursor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string NextCursor { get; set; }
}
