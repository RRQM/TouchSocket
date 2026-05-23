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
/// CoAP 响应码（RFC 7252 Section 12.1.2）
/// 编码格式：高 3 位为类别（c），低 5 位为细节（dd），即 c.dd 表示法。
/// </summary>
public enum CoAPResponseCode : byte
{
    // ---- 2.xx 成功 ----

    /// <summary>
    /// 2.01 Created，资源创建成功。
    /// </summary>
    Created = 0x41,

    /// <summary>
    /// 2.02 Deleted，资源删除成功。
    /// </summary>
    Deleted = 0x42,

    /// <summary>
    /// 2.03 Valid，请求有效，响应与缓存中的表示一致。
    /// </summary>
    Valid = 0x43,

    /// <summary>
    /// 2.04 Changed，资源已更改。
    /// </summary>
    Changed = 0x44,

    /// <summary>
    /// 2.05 Content，响应包含请求的内容。
    /// </summary>
    Content = 0x45,

    // ---- 4.xx 客户端错误 ----

    /// <summary>
    /// 4.00 Bad Request，请求格式错误。
    /// </summary>
    BadRequest = 0x80,

    /// <summary>
    /// 4.01 Unauthorized，未授权。
    /// </summary>
    Unauthorized = 0x81,

    /// <summary>
    /// 4.02 Bad Option，包含不支持的选项。
    /// </summary>
    BadOption = 0x82,

    /// <summary>
    /// 4.03 Forbidden，禁止访问。
    /// </summary>
    Forbidden = 0x83,

    /// <summary>
    /// 4.04 Not Found，资源未找到。
    /// </summary>
    NotFound = 0x84,

    /// <summary>
    /// 4.05 Method Not Allowed，方法不允许。
    /// </summary>
    MethodNotAllowed = 0x85,

    /// <summary>
    /// 4.06 Not Acceptable，响应格式不可接受。
    /// </summary>
    NotAcceptable = 0x86,

    /// <summary>
    /// 4.08 Request Entity Incomplete，请求实体不完整。
    /// </summary>
    RequestEntityIncomplete = 0x88,

    /// <summary>
    /// 4.12 Precondition Failed，前提条件不满足。
    /// </summary>
    PreconditionFailed = 0x8C,

    /// <summary>
    /// 4.13 Request Entity Too Large，请求实体过大。
    /// </summary>
    RequestEntityTooLarge = 0x8D,

    /// <summary>
    /// 4.15 Unsupported Content-Format，不支持的内容格式。
    /// </summary>
    UnsupportedContentFormat = 0x8F,

    // ---- 5.xx 服务器错误 ----

    /// <summary>
    /// 5.00 Internal Server Error，服务器内部错误。
    /// </summary>
    InternalServerError = 0xA0,

    /// <summary>
    /// 5.01 Not Implemented，功能未实现。
    /// </summary>
    NotImplemented = 0xA1,

    /// <summary>
    /// 5.02 Bad Gateway，网关错误。
    /// </summary>
    BadGateway = 0xA2,

    /// <summary>
    /// 5.03 Service Unavailable，服务不可用。
    /// </summary>
    ServiceUnavailable = 0xA3,

    /// <summary>
    /// 5.04 Gateway Timeout，网关超时。
    /// </summary>
    GatewayTimeout = 0xA4,

    /// <summary>
    /// 5.05 Proxying Not Supported，不支持代理。
    /// </summary>
    ProxyingNotSupported = 0xA5,
}
