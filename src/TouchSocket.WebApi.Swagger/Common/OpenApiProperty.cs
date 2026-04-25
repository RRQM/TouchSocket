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
/// 表示 OpenAPI Schema 中的属性定义。
/// </summary>
public class OpenApiProperty
{
    /// <summary>
    /// 获取或设置属性的描述信息。
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// 获取或设置属性的格式说明，如 int32、int64、float、double、byte、date、date-time 等。
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; }

    /// <summary>
    /// 获取或设置数组类型属性的元素属性定义。
    /// </summary>
    [JsonPropertyName("items")]
    public OpenApiProperty Items { get; set; }

    /// <summary>
    /// 获取或设置属性是否为只读。
    /// </summary>
    [JsonPropertyName("readOnly")]
    public bool? ReadOnly { get; set; }

    /// <summary>
    /// 获取或设置指向其他 Schema 组件的引用路径。
    /// </summary>
    [JsonPropertyName("$ref")]
    public string Ref { get; set; }

    /// <summary>
    /// 获取或设置属性的数据类型。
    /// </summary>
    [JsonPropertyName("type")]
    public OpenApiDataTypes? Type { get; set; }
}