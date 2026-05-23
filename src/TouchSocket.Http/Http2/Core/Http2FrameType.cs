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
/// HTTP/2 帧类型，见 RFC 7540 §6
/// </summary>
internal enum Http2FrameType : byte
{
    /// <summary>DATA 帧，用于传输请求/响应 body</summary>
    Data = 0x0,

    /// <summary>HEADERS 帧，用于传输头部块片段</summary>
    Headers = 0x1,

    /// <summary>PRIORITY 帧，用于指定流优先级（已在 RFC 9113 中弃用）</summary>
    Priority = 0x2,

    /// <summary>RST_STREAM 帧，用于立即终止一个流</summary>
    RstStream = 0x3,

    /// <summary>SETTINGS 帧，用于协商连接参数</summary>
    Settings = 0x4,

    /// <summary>PUSH_PROMISE 帧，用于服务器推送</summary>
    PushPromise = 0x5,

    /// <summary>PING 帧，用于测量往返时间和连接活跃性</summary>
    Ping = 0x6,

    /// <summary>GOAWAY 帧，用于发起连接关闭或严重错误信号</summary>
    GoAway = 0x7,

    /// <summary>WINDOW_UPDATE 帧，用于流量控制</summary>
    WindowUpdate = 0x8,

    /// <summary>CONTINUATION 帧，用于延续头部块片段</summary>
    Continuation = 0x9,
}
