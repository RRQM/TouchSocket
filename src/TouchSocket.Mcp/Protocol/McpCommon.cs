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
/// 表示可在用户界面中显示的图标。
/// </summary>
public sealed class McpIcon
{
    /// <summary>
    /// 获取或设置图标资源 URI。
    /// </summary>
    [JsonPropertyName("src")]
    public string Src { get; set; }

    /// <summary>
    /// 获取或设置图标 MIME 类型。
    /// </summary>
    [JsonPropertyName("mimeType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string MimeType { get; set; }

    /// <summary>
    /// 获取或设置图标尺寸集合。
    /// </summary>
    [JsonPropertyName("sizes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> Sizes { get; set; }

    /// <summary>
    /// 获取或设置图标主题，通常为 "light" 或 "dark"。
    /// </summary>
    [JsonPropertyName("theme")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Theme { get; set; }
}

/// <summary>
/// 表示内容或资源的可选注解。
/// </summary>
public sealed class McpAnnotations
{
    /// <summary>
    /// 获取或设置目标受众。
    /// </summary>
    [JsonPropertyName("audience")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> Audience { get; set; }

    /// <summary>
    /// 获取或设置优先级，范围通常为 0 到 1。
    /// </summary>
    [JsonPropertyName("priority")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Priority { get; set; }

    /// <summary>
    /// 获取或设置资源最后修改时间，ISO 8601 格式。
    /// </summary>
    [JsonPropertyName("lastModified")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string LastModified { get; set; }
}

/// <summary>
/// 表示请求通用元数据。
/// </summary>
public sealed class McpRequestMeta
{
    /// <summary>
    /// 获取或设置进度通知令牌。
    /// </summary>
    [JsonPropertyName("progressToken")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? ProgressToken { get; set; }
}

/// <summary>
/// 表示任务增强请求的元数据。
/// </summary>
public sealed class McpTaskMetadata
{
    /// <summary>
    /// 获取或设置任务保留时长，单位为毫秒。
    /// </summary>
    [JsonPropertyName("ttl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? Ttl { get; set; }
}
