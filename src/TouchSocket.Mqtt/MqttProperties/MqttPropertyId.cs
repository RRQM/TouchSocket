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
/// 表示 MQTT 属性的标识符。
/// </summary>
public enum MqttPropertyId
{
    /// <summary>
    /// 无属性。
    /// </summary>
    None = 0,

    /// <summary>
    /// 负载格式指示符。
    /// </summary>
    PayloadFormatIndicator = 1,

    /// <summary>
    /// 消息过期时间间隔。
    /// </summary>
    MessageExpiryInterval = 2,

    /// <summary>
    /// 内容类型。
    /// </summary>
    ContentType = 3,

    /// <summary>
    /// 响应主题。
    /// </summary>
    ResponseTopic = 8,

    /// <summary>
    /// 相关性数据。
    /// </summary>
    CorrelationData = 9,

    /// <summary>
    /// 订阅标识符。
    /// </summary>
    SubscriptionIdentifier = 11,

    /// <summary>
    /// 会话过期时间间隔。
    /// </summary>
    SessionExpiryInterval = 17,

    /// <summary>
    /// 分配的客户端标识符。
    /// </summary>
    AssignedClientIdentifier = 18,

    /// <summary>
    /// 服务器保持连接时间。
    /// </summary>
    ServerKeepAlive = 19,

    /// <summary>
    /// 认证方法。
    /// </summary>
    AuthenticationMethod = 21,

    /// <summary>
    /// 认证数据。
    /// </summary>
    AuthenticationData = 22,

    /// <summary>
    /// 请求问题信息。
    /// </summary>
    RequestProblemInformation = 23,

    /// <summary>
    /// 遗嘱延迟时间间隔。
    /// </summary>
    WillDelayInterval = 24,

    /// <summary>
    /// 请求响应信息。
    /// </summary>
    RequestResponseInformation = 25,

    /// <summary>
    /// 响应信息。
    /// </summary>
    ResponseInformation = 26,

    /// <summary>
    /// 服务器引用。
    /// </summary>
    ServerReference = 28,

    /// <summary>
    /// 原因字符串。
    /// </summary>
    ReasonString = 31,

    /// <summary>
    /// 接收最大值。
    /// </summary>
    ReceiveMaximum = 33,

    /// <summary>
    /// 主题别名最大值。
    /// </summary>
    TopicAliasMaximum = 34,

    /// <summary>
    /// 主题别名。
    /// </summary>
    TopicAlias = 35,

    /// <summary>
    /// 最大 QoS。
    /// </summary>
    MaximumQoS = 36,

    /// <summary>
    /// 保留可用。
    /// </summary>
    RetainAvailable = 37,

    /// <summary>
    /// 用户属性。
    /// </summary>
    UserProperty = 38,

    /// <summary>
    /// 最大数据包大小。
    /// </summary>
    MaximumPacketSize = 39,

    /// <summary>
    /// 通配符订阅可用。
    /// </summary>
    WildcardSubscriptionAvailable = 40,

    /// <summary>
    /// 订阅标识符可用。
    /// </summary>
    SubscriptionIdentifiersAvailable = 41,

    /// <summary>
    /// 共享订阅可用。
    /// </summary>
    SharedSubscriptionAvailable = 42
}