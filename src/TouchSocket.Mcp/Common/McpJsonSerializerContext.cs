//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Text.Json.Serialization;

namespace TouchSocket.Mcp;

/// <summary>
/// 表示 MCP 协议内置类型的 JSON 源生成上下文。
/// </summary>
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true)]
[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(McpRequest))]
[JsonSerializable(typeof(McpResponse))]
[JsonSerializable(typeof(McpNotification))]
[JsonSerializable(typeof(McpError))]
[JsonSerializable(typeof(McpEmptyResult))]
[JsonSerializable(typeof(McpImplementationInfo))]
[JsonSerializable(typeof(McpClientCapabilities))]
[JsonSerializable(typeof(McpRootsCapability))]
[JsonSerializable(typeof(McpServerCapabilities))]
[JsonSerializable(typeof(McpToolsCapability))]
[JsonSerializable(typeof(McpResourcesCapability))]
[JsonSerializable(typeof(McpPromptsCapability))]
[JsonSerializable(typeof(McpInitializeParams))]
[JsonSerializable(typeof(McpInitializeResult))]
[JsonSerializable(typeof(McpToolDefinition))]
[JsonSerializable(typeof(McpToolInputSchema))]
[JsonSerializable(typeof(McpToolProperty))]
[JsonSerializable(typeof(McpToolAnnotations))]
[JsonSerializable(typeof(McpCallToolParams))]
[JsonSerializable(typeof(McpCallToolResult))]
[JsonSerializable(typeof(McpListToolsResult))]
[JsonSerializable(typeof(McpContent))]
[JsonSerializable(typeof(McpTextContent))]
[JsonSerializable(typeof(McpImageContent))]
[JsonSerializable(typeof(McpEmbeddedResourceContent))]
[JsonSerializable(typeof(McpResource))]
[JsonSerializable(typeof(McpResourceTemplate))]
[JsonSerializable(typeof(McpResourceContent))]
[JsonSerializable(typeof(McpListResourcesResult))]
[JsonSerializable(typeof(McpListResourceTemplatesResult))]
[JsonSerializable(typeof(McpReadResourceResult))]
[JsonSerializable(typeof(McpPrompt))]
[JsonSerializable(typeof(McpPromptArgument))]
[JsonSerializable(typeof(McpPromptMessage))]
[JsonSerializable(typeof(McpListPromptsResult))]
[JsonSerializable(typeof(McpGetPromptParams))]
[JsonSerializable(typeof(McpGetPromptResult))]
internal partial class McpJsonSerializerContext : JsonSerializerContext
{
}
