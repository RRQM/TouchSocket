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

[JsonConverter(typeof(OpenApiStringEnumConverter))]
internal enum OpenApiDataTypes
{
    /// <summary>
    /// 字符串
    /// </summary>
    String,

    /// <summary>
    /// 数值
    /// </summary>
    Number,

    /// <summary>
    /// 整数
    /// </summary>
    Integer,

    /// <summary>
    /// 布尔值
    /// </summary>
    Boolean,

    /// <summary>
    /// 二进制（如文件）
    /// </summary>
    Binary,

    /// <summary>
    /// 二进制集合
    /// </summary>
    BinaryCollection,

    /// <summary>
    /// 记录值（字典）
    /// </summary>
    Record,

    /// <summary>
    /// 元组值
    /// </summary>
    Tuple,

    /// <summary>
    /// 数组
    /// </summary>
    Array,

    /// <summary>
    /// 对象
    /// </summary>
    Object,

    /// <summary>
    /// 结构
    /// </summary>
    Struct,

    /// <summary>
    /// Any
    /// </summary>
    Any
}