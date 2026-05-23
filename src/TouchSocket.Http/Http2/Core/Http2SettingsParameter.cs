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
/// HTTP/2 SETTINGS 参数标识符，见 RFC 7540 §6.5.2
/// </summary>
internal enum Http2SettingsParameter : ushort
{
    /// <summary>头部压缩表大小，默认 4096</summary>
    HeaderTableSize = 0x1,

    /// <summary>是否允许服务器推送，默认 1（允许）</summary>
    EnablePush = 0x2,

    /// <summary>最大并发流数，默认无限制</summary>
    MaxConcurrentStreams = 0x3,

    /// <summary>初始流量控制窗口大小，默认 65535</summary>
    InitialWindowSize = 0x4,

    /// <summary>最大帧负载大小，默认 16384</summary>
    MaxFrameSize = 0x5,

    /// <summary>最大头部列表大小，默认无限制</summary>
    MaxHeaderListSize = 0x6,
}
