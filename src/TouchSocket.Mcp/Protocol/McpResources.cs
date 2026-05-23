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
/// 表示 MCP 资源定义。
/// </summary>
public sealed class McpResource
{
    /// <summary>
    /// 获取或设置资源的唯一 URI。
    /// </summary>
    [JsonPropertyName("uri")]
    public string Uri { get; set; }

    /// <summary>
    /// 获取或设置资源的人类可读名称。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 获取或设置资源的描述。
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Description { get; set; }

    /// <summary>
    /// 获取或设置资源的 MIME 类型。
    /// </summary>
    [JsonPropertyName("mimeType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string MimeType { get; set; }

    /// <summary>
    /// 获取或设置资源的大小（字节数）。
    /// </summary>
    [JsonPropertyName("size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? Size { get; set; }
}

/// <summary>
/// 表示 MCP 资源模板定义（URI 模板）。
/// </summary>
public sealed class McpResourceTemplate
{
    /// <summary>
    /// 获取或设置 URI 模板（RFC 6570 格式）。
    /// </summary>
    [JsonPropertyName("uriTemplate")]
    public string UriTemplate { get; set; }

    /// <summary>
    /// 获取或设置模板的人类可读名称。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 获取或设置模板的描述。
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Description { get; set; }

    /// <summary>
    /// 获取或设置模板的 MIME 类型。
    /// </summary>
    [JsonPropertyName("mimeType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string MimeType { get; set; }
}

/// <summary>
/// 表示 MCP 资源内容（文本或二进制）。
/// </summary>
public sealed class McpResourceContent
{
    /// <summary>
    /// 获取或设置资源 URI。
    /// </summary>
    [JsonPropertyName("uri")]
    public string Uri { get; set; }

    /// <summary>
    /// 获取或设置资源 MIME 类型。
    /// </summary>
    [JsonPropertyName("mimeType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string MimeType { get; set; }

    /// <summary>
    /// 获取或设置文本内容，与 <see cref="Blob"/> 互斥。
    /// </summary>
    [JsonPropertyName("text")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Text { get; set; }

    /// <summary>
    /// 获取或设置 Base64 编码的二进制内容，与 <see cref="Text"/> 互斥。
    /// </summary>
    [JsonPropertyName("blob")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Blob { get; set; }
}

/// <summary>
/// 表示 resources/list 的响应结果。
/// </summary>
public sealed class McpListResourcesResult
{
    /// <summary>
    /// 获取或设置资源列表。
    /// </summary>
    [JsonPropertyName("resources")]
    public List<McpResource> Resources { get; set; } = new List<McpResource>();

    /// <summary>
    /// 获取或设置下一页游标，用于分页。
    /// </summary>
    [JsonPropertyName("nextCursor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string NextCursor { get; set; }
}

/// <summary>
/// 表示 resources/templates/list 的响应结果。
/// </summary>
public sealed class McpListResourceTemplatesResult
{
    /// <summary>
    /// 获取或设置资源模板列表。
    /// </summary>
    [JsonPropertyName("resourceTemplates")]
    public List<McpResourceTemplate> ResourceTemplates { get; set; } = new List<McpResourceTemplate>();

    /// <summary>
    /// 获取或设置下一页游标，用于分页。
    /// </summary>
    [JsonPropertyName("nextCursor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string NextCursor { get; set; }
}

/// <summary>
/// 表示 resources/read 的响应结果。
/// </summary>
public sealed class McpReadResourceResult
{
    /// <summary>
    /// 获取或设置资源内容列表。
    /// </summary>
    [JsonPropertyName("contents")]
    public List<McpResourceContent> Contents { get; set; } = new List<McpResourceContent>();
}
