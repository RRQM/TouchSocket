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

namespace TouchSocket.WebApi.Swagger;

/// <summary>
/// 表示 OpenAPI 路径中某个 HTTP 方法的操作定义。
/// </summary>
public class OpenApiPathValue
{
    /// <summary>
    /// 获取或设置操作所属的标签列表，用于在 UI 中对操作分组。
    /// </summary>
    [JsonPropertyName("tags")]
    public IEnumerable<string> Tags { get; set; }

    /// <summary>
    /// 获取或设置操作的简短摘要。
    /// </summary>
    [JsonPropertyName("summary")]
    public string Summary { get; set; }

    /// <summary>
    /// 获取或设置操作的详细描述信息。
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// 获取或设置操作的唯一标识符。
    /// </summary>
    [JsonPropertyName("operationId")]
    public string OperationId { get; set; }

    /// <summary>
    /// 获取或设置操作的请求体定义。
    /// </summary>
    [JsonPropertyName("requestBody")]
    public OpenApiRequestBody RequestBody { get; set; }

    /// <summary>
    /// 获取或设置操作的参数列表。
    /// </summary>
    [JsonPropertyName("parameters")]
    public IEnumerable<OpenApiParameter> Parameters { get; set; }

    /// <summary>
    /// 获取或设置操作可能返回的响应字典，键为 HTTP 状态码。
    /// </summary>
    [JsonPropertyName("responses")]
    public Dictionary<string, OpenApiResponse> Responses { get; set; }
}