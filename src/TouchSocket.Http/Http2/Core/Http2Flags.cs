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
/// HTTP/2 帧标志位，见 RFC 7540 §6
/// </summary>
[Flags]
internal enum Http2Flags : byte
{
    /// <summary>无标志</summary>
    None = 0x0,

    /// <summary>DATA/HEADERS：流结束；SETTINGS/PING：ACK</summary>
    EndStreamOrAck = 0x1,

    /// <summary>HEADERS/PUSH_PROMISE/CONTINUATION：头部块结束</summary>
    EndHeaders = 0x4,

    /// <summary>DATA/HEADERS/PUSH_PROMISE：使用填充</summary>
    Padded = 0x8,

    /// <summary>HEADERS：含有优先级字段</summary>
    Priority = 0x20,
}
