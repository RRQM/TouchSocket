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

namespace TouchSocket.Http;

/// <summary>
/// HTTP/2 错误码，见 RFC 7540 §7
/// </summary>
internal enum Http2ErrorCode : uint
{
    /// <summary>无错误</summary>
    NoError = 0x0,

    /// <summary>协议错误</summary>
    ProtocolError = 0x1,

    /// <summary>内部错误</summary>
    InternalError = 0x2,

    /// <summary>流量控制错误</summary>
    FlowControlError = 0x3,

    /// <summary>SETTINGS 应答超时</summary>
    SettingsTimeout = 0x4,

    /// <summary>在已关闭流上收到帧</summary>
    StreamClosed = 0x5,

    /// <summary>帧大小错误</summary>
    FrameSizeError = 0x6,

    /// <summary>流被拒绝，可重试</summary>
    RefusedStream = 0x7,

    /// <summary>流被取消</summary>
    Cancel = 0x8,

    /// <summary>压缩上下文损坏</summary>
    CompressionError = 0x9,

    /// <summary>CONNECT 请求错误</summary>
    ConnectError = 0xa,

    /// <summary>连接过载，降低请求速率</summary>
    EnhanceYourCalm = 0xb,

    /// <summary>TLS 安全性不足</summary>
    InadequateSecurity = 0xc,

    /// <summary>端点需要 HTTP/1.1</summary>
    Http11Required = 0xd,
}
