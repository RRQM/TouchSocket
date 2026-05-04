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

namespace TouchSocket.WebApi.OpenApi;

/// <summary>
/// 表示 OpenAPI 操作的响应定义。
/// </summary>
public class OpenApiResponse
{
    /// <summary>
    /// 获取或设置响应的描述信息。
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// 获取或设置响应支持的媒体类型内容字典，键为媒体类型（如 application/json）。
    /// </summary>
    [JsonPropertyName("content")]
    public Dictionary<string, OpenApiContent> Content { get; set; }
}