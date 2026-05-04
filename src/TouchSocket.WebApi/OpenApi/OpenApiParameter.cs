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
/// 表示 OpenAPI 路径操作中的参数定义。
/// </summary>
public class OpenApiParameter
{
    /// <summary>
    /// 获取或设置参数的描述信息。
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// 获取或设置参数的位置，如 query、header、path 或 cookie。
    /// </summary>
    [JsonPropertyName("in")]
    public string In { get; set; }

    /// <summary>
    /// 获取或设置参数的名称。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 获取或设置参数的 Schema 定义。
    /// </summary>
    [JsonPropertyName("schema")]
    public OpenApiSchema Schema { get; set; }
}