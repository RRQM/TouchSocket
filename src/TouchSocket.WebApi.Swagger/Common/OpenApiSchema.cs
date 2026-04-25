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
/// 表示 OpenAPI 规范中的 Schema 定义，用于描述请求或响应数据的结构。
/// </summary>
public class OpenApiSchema
{
    /// <summary>
    /// 获取或设置指向其他 Schema 组件的引用路径。
    /// </summary>
    [JsonPropertyName("$ref")]
    public string Ref { get; set; }

    /// <summary>
    /// 获取或设置 Schema 的数据类型。
    /// </summary>
    [JsonPropertyName("type")]
    public OpenApiDataTypes? Type { get; set; }

    /// <summary>
    /// 获取或设置 Schema 的格式说明，如 int32、int64、float、double、byte、date、date-time 等。
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; }

    /// <summary>
    /// 获取或设置对象类型 Schema 的属性字典，键为属性名称。
    /// </summary>
    [JsonPropertyName("properties")]
    public Dictionary<string, OpenApiProperty> Properties { get; set; }

    /// <summary>
    /// 获取或设置数组类型 Schema 的元素 Schema 定义。
    /// </summary>
    [JsonPropertyName("items")]
    public OpenApiSchema Items { get; set; }

    /// <summary>
    /// 获取或设置枚举类型的可选值列表。
    /// </summary>
    [JsonPropertyName("enum")]
    public long[] Enum { get; set; }
}