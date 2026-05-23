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
/// 表示 MCP 提示模板定义。
/// </summary>
public sealed class McpPrompt
{
    /// <summary>
    /// 获取或设置提示模板的唯一名称。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 获取或设置提示模板的描述。
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Description { get; set; }

    /// <summary>
    /// 获取或设置提示模板的参数列表。
    /// </summary>
    [JsonPropertyName("arguments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<McpPromptArgument> Arguments { get; set; }
}

/// <summary>
/// 表示提示模板参数定义。
/// </summary>
public sealed class McpPromptArgument
{
    /// <summary>
    /// 获取或设置参数名称。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 获取或设置参数描述。
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Description { get; set; }

    /// <summary>
    /// 获取或设置一个值，指示该参数是否必填。
    /// </summary>
    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Required { get; set; }
}

/// <summary>
/// 表示提示模板中的单条消息。
/// </summary>
public sealed class McpPromptMessage
{
    /// <summary>
    /// 获取或设置消息角色，"user" 或 "assistant"。
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; }

    /// <summary>
    /// 获取或设置消息内容。
    /// </summary>
    [JsonPropertyName("content")]
    public McpContent Content { get; set; }
}

/// <summary>
/// 表示 prompts/list 的响应结果。
/// </summary>
public sealed class McpListPromptsResult
{
    /// <summary>
    /// 获取或设置提示模板列表。
    /// </summary>
    [JsonPropertyName("prompts")]
    public List<McpPrompt> Prompts { get; set; } = new List<McpPrompt>();

    /// <summary>
    /// 获取或设置下一页游标，用于分页。
    /// </summary>
    [JsonPropertyName("nextCursor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string NextCursor { get; set; }
}

/// <summary>
/// 表示 prompts/get 请求的参数。
/// </summary>
public sealed class McpGetPromptParams
{
    /// <summary>
    /// 获取或设置要获取的提示模板名称。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 获取或设置参数值映射，键为参数名称，值为字符串值。
    /// </summary>
    [JsonPropertyName("arguments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string> Arguments { get; set; }
}

/// <summary>
/// 表示 prompts/get 的响应结果。
/// </summary>
public sealed class McpGetPromptResult
{
    /// <summary>
    /// 获取或设置提示模板描述。
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Description { get; set; }

    /// <summary>
    /// 获取或设置提示消息列表。
    /// </summary>
    [JsonPropertyName("messages")]
    public List<McpPromptMessage> Messages { get; set; } = new List<McpPromptMessage>();
}
