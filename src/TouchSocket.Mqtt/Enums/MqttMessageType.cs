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

namespace TouchSocket.Mqtt;

/// <summary>
/// 表示 Mqtt 协议中的控制报文类型。
/// </summary>
public enum MqttMessageType
{
    /// <summary>
    /// 0 - 禁止使用，保留。
    /// </summary>
    Reserved = 0,

    /// <summary>
    /// 1 - 客户端到服务端：客户端请求连接服务端。
    /// </summary>
    Connect = 1,

    /// <summary>
    /// 2 - 服务端到客户端：连接报文确认。
    /// </summary>
    ConnAck = 2,

    /// <summary>
    /// 3 - 双向：发布消息。
    /// </summary>
    Publish = 3,

    /// <summary>
    /// 4 - 双向：QoS 1 消息发布收到确认。
    /// </summary>
    PubAck = 4,

    /// <summary>
    /// 5 - 双向：发布收到（保证交付第一步）。
    /// </summary>
    PubRec = 5,

    /// <summary>
    /// 6 - 双向：发布释放（保证交付第二步）。
    /// </summary>
    PubRel = 6,

    /// <summary>
    /// 7 - 双向：QoS 2 消息发布完成（保证交互第三步）。
    /// </summary>
    PubComp = 7,

    /// <summary>
    /// 8 - 客户端到服务端：客户端订阅请求。
    /// </summary>
    Subscribe = 8,

    /// <summary>
    /// 9 - 服务端到客户端：订阅请求报文确认。
    /// </summary>
    SubAck = 9,

    /// <summary>
    /// 10 - 客户端到服务端：客户端取消订阅请求。
    /// </summary>
    Unsubscribe = 10,

    /// <summary>
    /// 11 - 服务端到客户端：取消订阅报文确认。
    /// </summary>
    UnsubAck = 11,

    /// <summary>
    /// 12 - 客户端到服务端：心跳请求。
    /// </summary>
    PingReq = 12,

    /// <summary>
    /// 13 - 服务端到客户端：心跳响应。
    /// </summary>
    PingResp = 13,

    /// <summary>
    /// 14 - 客户端到服务端：客户端断开连接。
    /// </summary>
    Disconnect = 14,

    /// <summary>
    /// 15 - 禁止使用，保留。表示此枚举中保留值的最大值。
    /// </summary>
    ReservedMax = 15
}