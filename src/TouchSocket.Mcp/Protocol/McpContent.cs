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
/// 表示 MCP 内容项的抽象基类。
/// </summary>
[JsonConverter(typeof(McpContentConverter))]
public abstract class McpContent
{
    /// <summary>
    /// 获取内容类型标识符。
    /// </summary>
    [JsonPropertyName("type")]
    public abstract string Type { get; }

    /// <summary>
    /// 获取或设置内容注解。
    /// </summary>
    [JsonPropertyName("annotations")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public McpAnnotations Annotations { get; set; }

    /// <summary>
    /// 获取或设置内容元数据。
    /// </summary>
    [JsonPropertyName("_meta")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object> Meta { get; set; }
}

/// <summary>
/// 表示文本类型内容。
/// </summary>
public sealed class McpTextContent : McpContent
{
    /// <inheritdoc/>
    [JsonPropertyName("type")]
    public override string Type => "text";

    /// <summary>
    /// 获取或设置文本内容。
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; set; }
}

/// <summary>
/// 表示图像类型内容（Base64 编码）。
/// </summary>
public sealed class McpImageContent : McpContent
{
    /// <inheritdoc/>
    [JsonPropertyName("type")]
    public override string Type => "image";

    /// <summary>
    /// 获取或设置 Base64 编码的图像数据。
    /// </summary>
    [JsonPropertyName("data")]
    public string Data { get; set; }

    /// <summary>
    /// 获取或设置图像 MIME 类型，例如 "image/png"。
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; }
}

/// <summary>
/// 表示音频类型内容（Base64 编码）。
/// </summary>
public sealed class McpAudioContent : McpContent
{
    /// <inheritdoc/>
    [JsonPropertyName("type")]
    public override string Type => "audio";

    /// <summary>
    /// 获取或设置 Base64 编码的音频数据。
    /// </summary>
    [JsonPropertyName("data")]
    public string Data { get; set; }

    /// <summary>
    /// 获取或设置音频 MIME 类型，例如 "audio/mpeg"。
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; }
}

/// <summary>
/// 表示资源链接类型内容。
/// </summary>
public sealed class McpResourceLinkContent : McpContent
{
    /// <inheritdoc/>
    [JsonPropertyName("type")]
    public override string Type => "resource_link";

    /// <summary>
    /// 获取或设置资源 URI。
    /// </summary>
    [JsonPropertyName("uri")]
    public string Uri { get; set; }

    /// <summary>
    /// 获取或设置资源名称。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 获取或设置资源显示标题。
    /// </summary>
    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Title { get; set; }

    /// <summary>
    /// 获取或设置资源描述。
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Description { get; set; }

    /// <summary>
    /// 获取或设置资源 MIME 类型。
    /// </summary>
    [JsonPropertyName("mimeType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string MimeType { get; set; }

    /// <summary>
    /// 获取或设置资源大小（字节数）。
    /// </summary>
    [JsonPropertyName("size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? Size { get; set; }

    /// <summary>
    /// 获取或设置资源图标集合。
    /// </summary>
    [JsonPropertyName("icons")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<McpIcon> Icons { get; set; }
}

/// <summary>
/// 表示嵌入资源类型内容。
/// </summary>
public sealed class McpEmbeddedResourceContent : McpContent
{
    /// <inheritdoc/>
    [JsonPropertyName("type")]
    public override string Type => "resource";

    /// <summary>
    /// 获取或设置嵌入的资源内容。
    /// </summary>
    [JsonPropertyName("resource")]
    public McpResourceContent Resource { get; set; }
}
