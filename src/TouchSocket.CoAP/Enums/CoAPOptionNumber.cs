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

namespace TouchSocket.CoAP;

/// <summary>
/// CoAP 选项编号（RFC 7252 Section 12.2）
/// </summary>
public enum CoAPOptionNumber : ushort
{
    /// <summary>
    /// If-Match，选项编号 1。
    /// </summary>
    IfMatch = 1,

    /// <summary>
    /// Uri-Host，选项编号 3，请求的目标主机名。
    /// </summary>
    UriHost = 3,

    /// <summary>
    /// ETag，实体标签，选项编号 4。
    /// </summary>
    ETag = 4,

    /// <summary>
    /// If-None-Match，选项编号 5。
    /// </summary>
    IfNoneMatch = 5,

    /// <summary>
    /// Observe，订阅资源变化，选项编号 6（RFC 7641）。
    /// </summary>
    Observe = 6,

    /// <summary>
    /// Uri-Port，选项编号 7，请求的目标端口。
    /// </summary>
    UriPort = 7,

    /// <summary>
    /// Location-Path，选项编号 8，响应中的资源路径。
    /// </summary>
    LocationPath = 8,

    /// <summary>
    /// Uri-Path，选项编号 11，请求的资源路径分段。
    /// </summary>
    UriPath = 11,

    /// <summary>
    /// Content-Format，选项编号 12，有效载荷的内容格式。
    /// </summary>
    ContentFormat = 12,

    /// <summary>
    /// Max-Age，选项编号 14，响应的最大缓存时间（秒）。
    /// </summary>
    MaxAge = 14,

    /// <summary>
    /// Uri-Query，选项编号 15，请求的查询参数分段。
    /// </summary>
    UriQuery = 15,

    /// <summary>
    /// Accept，选项编号 17，请求接受的响应内容格式。
    /// </summary>
    Accept = 17,

    /// <summary>
    /// Location-Query，选项编号 20，响应中的查询参数分段。
    /// </summary>
    LocationQuery = 20,

    /// <summary>
    /// Block2，选项编号 23，块传输响应控制（RFC 7959）。
    /// </summary>
    Block2 = 23,

    /// <summary>
    /// Block1，选项编号 27，块传输请求控制（RFC 7959）。
    /// </summary>
    Block1 = 27,

    /// <summary>
    /// Size2，选项编号 28，响应体大小（RFC 7959）。
    /// </summary>
    Size2 = 28,

    /// <summary>
    /// Proxy-Uri，选项编号 35，代理请求的完整 URI。
    /// </summary>
    ProxyUri = 35,

    /// <summary>
    /// Proxy-Scheme，选项编号 39，代理请求的 URI 方案。
    /// </summary>
    ProxyScheme = 39,

    /// <summary>
    /// Size1，选项编号 60，请求体大小（RFC 7959）。
    /// </summary>
    Size1 = 60,
}
