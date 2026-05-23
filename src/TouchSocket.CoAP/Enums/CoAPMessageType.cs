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
/// CoAP 消息类型（RFC 7252 Section 4.1）
/// </summary>
public enum CoAPMessageType : byte
{
    /// <summary>
    /// 确认消息，需要接收方返回 <see cref="ACK"/> 或 <see cref="RST"/>。
    /// </summary>
    CON = 0,

    /// <summary>
    /// 非确认消息，不需要接收方回应。
    /// </summary>
    NON = 1,

    /// <summary>
    /// 确认应答，用于确认已收到 <see cref="CON"/> 消息。
    /// </summary>
    ACK = 2,

    /// <summary>
    /// 重置消息，表示无法处理接收到的消息。
    /// </summary>
    RST = 3,
}
