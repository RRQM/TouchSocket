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
/// CoAP 协议常量定义。
/// </summary>
public static class CoAPConstant
{
    /// <summary>
    /// CoAP 默认 UDP 端口（5683）。
    /// </summary>
    public const int DefaultPort = 5683;

    /// <summary>
    /// CoAP over DTLS 默认端口（5684）。
    /// </summary>
    public const int DefaultSecurePort = 5684;

    /// <summary>
    /// 有效载荷标记字节（0xFF）。
    /// </summary>
    public const byte PayloadMarker = 0xFF;

    /// <summary>
    /// CoAP 协议版本号（1）。
    /// </summary>
    public const byte Version = 1;

    /// <summary>
    /// CoAP 消息最小字节长度（4 字节头部）。
    /// </summary>
    public const int MinMessageLength = 4;
}
