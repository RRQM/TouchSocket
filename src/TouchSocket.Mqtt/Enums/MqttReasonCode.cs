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
/// Mqtt协议中的原因码枚举，用于表示不同 Mqtt 报文处理结果或错误情况。
/// </summary>
public enum MqttReasonCode
{
    /// <summary>
    /// 此 ReasonCode 可以用在所有存在 ReasonCode 的报文中，例如 <see cref="MqttConnAckMessage"/>、<see cref="MqttDisconnectMessage"/> 报文等等。
    /// 它通常用于表示成功，比如连接成功、取消订阅成功、消息接收成功和认证成功等等。
    /// 适用报文：<see cref="MqttConnAckMessage"/>, <see cref="MqttPubAckMessage"/> <see cref="MqttPubAckMessage"/>, <see cref="MqttPubRecMessage"/> <see cref="MqttPubRecMessage"/>, <see cref="MqttPubRelMessage"/>, <see cref="MqttPubCompMessage"/> , <see cref="MqttUnsubAckMessage"/>, AUTH
    /// </summary>
    Success = 0,

    /// <summary>
    /// 在 <see cref="MqttDisconnectMessage"/> 报文中，该值表示连接正常断开，这种情况下遗嘱消息不会被发布。
    /// 适用报文：<see cref="MqttDisconnectMessage"/>
    /// </summary>
    NormalDisconnection = 0,

    /// <summary>
    /// 连接已接受。
    /// </summary>
    ConnectionAccepted = 0,

    /// <summary>
    /// 在 <see cref="MqttSubAckMessage"/> 订阅确认报文中，该值用来指示订阅结果，表示订阅成功，
    /// 同时向订阅端指示最终被授予的最大 QoS 等级为 0。
    /// 因为服务端最终授予的最大 QoS 等级，可能小于订阅时请求的最大 QoS 等级。
    /// 适用报文：<see cref="MqttSubAckMessage"/>
    /// </summary>
    GrantedQoS0 = 0,

    /// <summary>
    /// 在 <see cref="MqttSubAckMessage"/> 订阅确认报文中，该值用来指示订阅结果，表示订阅成功，
    /// 同时向订阅端指示最终被授予的最大 QoS 等级为 1。
    /// 因为服务端最终授予的最大 QoS 等级，可能小于订阅时请求的最大 QoS 等级。
    /// 适用报文：<see cref="MqttSubAckMessage"/>
    /// </summary>
    GrantedQoS1 = 1,

    /// <summary>
    /// 连接被拒绝，不可接受的协议版本。
    /// </summary>
    ConnectionRefusedUnacceptableProtocolVersion = 1,

    /// <summary>
    /// 在 <see cref="MqttSubAckMessage"/> 订阅确认报文中，该值用来指示订阅结果，表示订阅成功，
    /// 同时向订阅端指示最终被授予的最大 QoS 等级为 2。
    /// 因为服务端最终授予的最大 QoS 等级，可能小于订阅时请求的最大 QoS 等级。
    /// 适用报文：<see cref="MqttSubAckMessage"/>
    /// </summary>
    GrantedQoS2 = 2,

    /// <summary>
    /// 连接被拒绝，标识符被拒绝。
    /// </summary>
    ConnectionRefusedIdentifierRejected = 2,

    /// <summary>
    /// 连接被拒绝，服务器不可用。
    /// </summary>
    ConnectionRefusedServerUnavailable = 3,

    /// <summary>
    /// 仅用于 <see cref="MqttDisconnectMessage"/> 报文，适用于客户端希望正常断开连接但服务端仍然需要发布遗嘱消息的情况，
    /// 比如客户端希望会话过期时可以对外发出通知。
    /// 适用报文：<see cref="MqttDisconnectMessage"/>
    /// </summary>
    DisconnectWithWillMessage = 4,

    /// <summary>
    /// 连接被拒绝，用户名或密码错误。
    /// </summary>
    ConnectionRefusedBadUsernameOrPassword = 4,

    /// <summary>
    /// 连接被拒绝，未授权。
    /// </summary>
    ConnectionRefusedNotAuthorized = 5,

    /// <summary>
    /// 该 ReasonCode 用于向发送方指示，消息已经收到，但是当前没有匹配的订阅者，所以只有服务端可以使用这个 ReasonCode。
    /// 可以通过收到 ReasonCode 为该值的响应报文得知当前没有人会收到自己的消息，
    /// 但不能通过没有收到该值的响应报文来假定所有人都会收到自己的消息，除非最多只会存在一个订阅者。
    /// 注意，没有匹配的订阅者时使用该值替代 0，并不是一个必须实现的行为，这取决于服务端的具体实现。
    /// 适用报文：<see cref="MqttPubAckMessage"/>, <see cref="MqttPubRecMessage"/>
    /// </summary>
    NoMatchingSubscribers = 16,

    /// <summary>
    /// 仅用于 <see cref="MqttUnsubAckMessage"/> 报文，表示取消订阅时没有发现匹配的订阅。
    /// 适用报文：<see cref="MqttUnsubAckMessage"/>
    /// </summary>
    NoSubscriptionExisted = 17,

    /// <summary>
    /// 仅用于 AUTH 报文，表示继续认证，通过这个 ReasonCode，客户端和服务端之间可以进行任意次数的 AUTH 报文交换，
    /// 以满足不同的认证方法的需要。
    /// 适用报文：AUTH
    /// </summary>
    ContinueAuthentication = 24,

    /// <summary>
    /// 仅用于 AUTH 报文，在增强认证成功后客户端可以随时通过发送 ReasonCode 为该值的 AUTH 报文发起重新认证。
    /// 重新认证期间，其他报文收发会正常继续，如果重新认证失败，连接就会被关闭。
    /// 适用报文：AUTH
    /// </summary>
    ReAuthenticate = 25,

    /// <summary>
    /// 表示未指明的错误。当一方不希望向另一方透露错误的具体原因，
    /// 或者协议规范中没有能够匹配当前情况的 ReasonCode 时，那么它可以在报文中使用这个 ReasonCode。
    /// 适用报文：<see cref="MqttConnAckMessage"/>, <see cref="MqttPubAckMessage"/>, <see cref="MqttPubRecMessage"/>, <see cref="MqttSubAckMessage"/>, <see cref="MqttUnsubAckMessage"/>, <see cref="MqttDisconnectMessage"/>
    /// </summary>
    UnspecifiedError = 128,

    /// <summary>
    /// 当收到了无法根据协议规范正确解析的控制报文时，接收方需要发送 ReasonCode 为该值的 <see cref="MqttDisconnectMessage"/> 报文来断开连接。
    /// 如果是 CONNECT 报文存在问题，那么服务端应该使用 <see cref="MqttConnAckMessage"/> 报文。
    /// 当控制报文中出现固定报头中的保留位没有按照协议要求置 0、QoS 被指定为 3、UTF - 8 字符串中包含了一个空字符等等这些情况时，
    /// 都将被认为是一个畸形的报文。
    /// 适用报文：<see cref="MqttConnAckMessage"/>, <see cref="MqttDisconnectMessage"/>
    /// </summary>
    MalformedPacket = 129,

    /// <summary>
    /// 在控制报文被按照协议规范解析后检测到的错误，比如包含协议不允许的数据，行为与协议要求不符等等，
    /// 都会被认为是协议错误。接收方需要发送 ReasonCode 为该值的 <see cref="MqttDisconnectMessage"/> 报文来断开连接。
    /// 如果是 CONNECT 报文存在问题，那么服务端应该使用 <see cref="MqttConnAckMessage"/> 报文。
    /// 常见的协议错误包括，客户端在一个连接内发送了两个 CONNECT 报文、一个报文中包含了多个相同的属性，
    /// 以及某个属性被设置成了一个协议不允许的值等等。
    /// 但是当有其他更具体的 ReasonCode 时，就不会使用该值 (Malformed Packet) 或者 130 (Protocol Error) 了。
    /// 例如，服务端已经声明自己不支持保留消息，但客户端仍然向服务端发送保留消息，
    /// 这本质上也属于协议错误，但会选择使用 154 (Retain not supported) 这个能够更清楚指明错误原因的 ReasonCode。
    /// 适用报文：<see cref="MqttConnAckMessage"/>, <see cref="MqttDisconnectMessage"/>
    /// </summary>
    ProtocolError = 130,

    /// <summary>
    /// 报文有效，但是不被当前接收方的实现所接受。
    /// 适用报文：<see cref="MqttConnAckMessage"/>, <see cref="MqttPubAckMessage"/>, <see cref="MqttPubRecMessage"/>, <see cref="MqttSubAckMessage"/>, <see cref="MqttUnsubAckMessage"/>, <see cref="MqttDisconnectMessage"/>
    /// </summary>
    ImplementationSpecificError = 131,

    /// <summary>
    /// 仅用于 <see cref="MqttConnAckMessage"/> 报文。对于支持了 Mqtt 5.0 的服务端来说，如果不支持客户端当前使用的 Mqtt 协议版本，
    /// 或者客户端指定了一个错误的协议版本或协议名。例如，客户端将协议版本设置为 6，
    /// 那么服务端可以发送 ReasonCode 为该值的 <see cref="MqttConnAckMessage"/> 报文，表示不支持该协议版本并且表明自己 Mqtt 服务端的身份，
    /// 然后关闭网络连接。当然服务端也可以选择直接关闭网络连接，因为使用 Mqtt 3.1 或 3.1.1 的 Mqtt 客户端
    /// 可能并不能理解该值这个 ReasonCode 的含义。这两个版本都是在 <see cref="MqttConnAckMessage"/> 报文使用 1 来表示不支持客户端指定的协议版本。
    /// 适用报文：<see cref="MqttConnAckMessage"/>
    /// </summary>
    UnsupportedProtocolVersion = 132,

    /// <summary>
    /// 仅用于 <see cref="MqttConnAckMessage"/> 报文，表示 Client ID 是有效的字符串，但是服务端不允许。
    /// 可能的情形有 Clean Start 为 0 但 Client ID 为空、或者 Client ID 超出了服务端允许的最大长度等等。
    /// 适用报文：<see cref="MqttConnAckMessage"/>
    /// </summary>
    ClientIdentifierNotValid = 133,

    /// <summary>
    /// 仅用于 <see cref="MqttConnAckMessage"/> 报文，表示客户端使用了错误的用户名或密码，这也意味着客户端将被拒绝连接。
    /// 适用报文：<see cref="MqttConnAckMessage"/>
    /// </summary>
    BadUserNameOrPassword = 134,

    /// <summary>
    /// 当客户端使用 Token 认证或者增强认证时，使用该值来表示客户端没有被授权连接会比 134 更加合适。
    /// 当客户端进行发布、订阅等操作时，如果没有通过服务端的授权检查，
    /// 那么服务端也可以在 <see cref="MqttPubAckMessage"/> 等应答报文中指定该值这个 ReasonCode 来指示授权结果。
    /// 适用报文：<see cref="MqttConnAckMessage"/>, <see cref="MqttPubAckMessage"/>, <see cref="MqttPubRecMessage"/>, <see cref="MqttSubAckMessage"/>, <see cref="MqttUnsubAckMessage"/>, <see cref="MqttDisconnectMessage"/>
    /// </summary>
    NotAuthorized = 135,

    /// <summary>
    /// 仅用于 <see cref="MqttConnAckMessage"/> 报文，向客户端指示当前服务端不可用。比如当前服务端认证服务异常无法接入新客户端等等。
    /// 适用报文：<see cref="MqttConnAckMessage"/>
    /// </summary>
    ServerUnavailable = 136,

    /// <summary>
    /// 向客户端指示服务端正忙，请稍后再试。
    /// 适用报文：<see cref="MqttConnAckMessage"/>, <see cref="MqttDisconnectMessage"/>
    /// </summary>
    ServerBusy = 137,

    /// <summary>
    /// 仅用于 <see cref="MqttConnAckMessage"/> 报文，表示客户端被禁止登录。
    /// 例如服务端检测到客户端的异常连接行为，所以将这个客户端的 Client ID 或者 IP 地址加入到了黑名单列表中，
    /// 又或者是后台管理人员手动封禁了这个客户端，当然以上这些通常需要视服务端的具体实现而定。
    /// 适用报文：<see cref="MqttConnAckMessage"/>
    /// </summary>
    Banned = 138,

    /// <summary>
    /// 仅用于 <see cref="MqttDisconnectMessage"/> 报文，并且只有服务端可以使用。
    /// 如果服务端正在或即将关闭，它可以通过主动发送 ReasonCode 为该值的 <see cref="MqttDisconnectMessage"/> 报文的方式告知客户端连接因为服务端正在关闭而被终止。
    /// 这可以帮助客户端避免在连接关闭后继续向此服务端发起连接请求。
    /// 适用报文：<see cref="MqttDisconnectMessage"/>
    /// </summary>
    ServerShuttingDown = 139,

    /// <summary>
    /// 当服务端不支持客户端指定的增强认证方法，或者客户端在重新认证时使用了和之前认证不同的认证方法时，
    /// 那么服务端就会发送 ReasonCode 为该值的 <see cref="MqttConnAckMessage"/> 或者 <see cref="MqttDisconnectMessage"/> 报文。
    /// 适用报文：<see cref="MqttConnAckMessage"/>, <see cref="MqttDisconnectMessage"/>
    /// </summary>
    BadAuthenticationMethod = 140,

    /// <summary>
    /// 仅用于 <see cref="MqttDisconnectMessage"/> 报文，并且只有服务端可以使用。
    /// 如果客户端没能在 1.5 倍的 Keep Alive 时间内保持通信，服务端将会发送 ReasonCode 为该值的 <see cref="MqttDisconnectMessage"/> 报文然后关闭网络连接。
    /// 适用报文：<see cref="MqttDisconnectMessage"/>
    /// </summary>
    KeepAliveTimeout = 141,

    /// <summary>
    /// 仅用于 <see cref="MqttDisconnectMessage"/> 报文，并且只有服务端可以使用。
    /// 当客户端连接到服务端时，如果服务端中已经存在使用相同 Client ID 的客户端连接，
    /// 那么服务端就会向原有的客户端发送 ReasonCode 为该值的 <see cref="MqttDisconnectMessage"/> 报文，表示会话被新的客户端连接接管，
    /// 然后关闭原有的网络连接。不管新的客户端连接中的 Clean Start 是 0 还是 1，
    /// 服务端都会使用这个 ReasonCode 向原有客户端指示会话被接管。
    /// 适用报文：<see cref="MqttDisconnectMessage"/>
    /// </summary>
    SessionTakenOver = 142,

    /// <summary>
    /// 主题过滤器的格式正确，但是不被服务端接受。
    /// 比如主题过滤器的层级超过了服务端允许的最大数量限制，或者主题过滤器中包含了空格等不被当前服务端接受的字符。
    /// 适用报文：<see cref="MqttSubAckMessage"/>, <see cref="MqttUnsubAckMessage"/>, <see cref="MqttDisconnectMessage"/>
    /// </summary>
    TopicFilterInvalid = 143,

    /// <summary>
    /// 主题名的格式正确，但是不被客户端或服务端接受。
    /// 适用报文：<see cref="MqttConnAckMessage"/>, <see cref="MqttPubAckMessage"/>, <see cref="MqttPubRecMessage"/>, <see cref="MqttDisconnectMessage"/>
    /// </summary>
    TopicNameInvalid = 144,

    /// <summary>
    /// 表示收到报文中的 Packet ID 正在被使用，例如发送方发送了一个 Packet ID 为 100 的 QoS 1 消息，
    /// 但是接收方认为当前有一个使用相同 Packet ID 的 QoS 2 消息还没有按成它的报文流程。
    /// 这通常意味着当前客户端和服务端之前的会话状态不匹配，可能需要通过设置 Clean Start 为 1 重新连接来重置会话状态。
    /// 适用报文：<see cref="MqttPubAckMessage"/>, <see cref="MqttPubRecMessage"/>, <see cref="MqttSubAckMessage"/>, <see cref="MqttUnsubAckMessage"/>
    /// </summary>
    PacketIdentifierInUse = 145,

    /// <summary>
    /// 表示未找到对应的 Packet ID，这只会在 QoS 2 的报文交互流程中发生。
    /// 比如当接收方回复 <see cref="MqttPubRecMessage"/> 报文时，发送方未找到使用相同 Packet ID 的等待确认的 PUBLISH 报文，
    /// 或者当发送方发送 <see cref="MqttPubRelMessage"/> 报文时，接收方未找到使用相同 Packet ID 的 <see cref="MqttPubRecMessage"/> 报文。
    /// 这通常意味着当前客户端和服务端之间的会话状态不匹配，可能需要通过设置 Clean Start 为 1 重新连接来重置会话状态。
    /// 适用报文：<see cref="MqttPubRelMessage"/>, <see cref="MqttPubCompMessage"/>
    /// </summary>
    PacketIdentifierNotFound = 146,

    /// <summary>
    /// 仅用于 <see cref="MqttDisconnectMessage"/> 报文，表示超出了接收最大值。
    /// Mqtt 5.0 增加了流控机制，客户端和服务端在连接时通过 Receive Maximum 属性约定它们愿意并发处理的可靠消息数（QoS > 0）。
    /// 所以一旦发送方发送的没有完成确认的消息超过了这一数量限制，接收方就会发送 ReasonCode 为该值的 <see cref="MqttDisconnectMessage"/> 报文然后关闭网络连接。
    /// 适用报文：<see cref="MqttDisconnectMessage"/>
    /// </summary>
    ReceiveMaximumExceeded = 147,

    /// <summary>
    /// 仅用于 <see cref="MqttDisconnectMessage"/> 报文，表示主题别名不合法。
    /// 如果 PUBLISH 报文中的主题别名值为 0 或者大于连接时约定的最大主题别名，
    /// 接收方会将此视为协议错误，它将发送 ReasonCode 为该值的 <see cref="MqttDisconnectMessage"/> 报文然后关闭网络连接。
    /// 适用报文：<see cref="MqttDisconnectMessage"/>
    /// </summary>
    TopicAliasInvalid = 148,

    /// <summary>
    /// 用于表示报文超过了最大允许长度。
    /// 客户端和服务端各自允许的最大报文长度，可以在 CONNECT 和 <see cref="MqttConnAckMessage"/> 报文中通过 Maximum Packet Size 属性约定。
    /// 当一方发送了过大的报文，那么另一方将发送 ReasonCode 为该值的 <see cref="MqttDisconnectMessage"/> 报文，然后关闭网络连接。
    /// 由于客户端可以在连接时设置遗嘱消息，因此 CONNECT 报文也有可能超过服务端能够处理的最大报文长度限制，
    /// 此时服务端需要在 <see cref="MqttConnAckMessage"/> 报文中使用这个 ReasonCode。
    /// 适用报文：<see cref="MqttConnAckMessage"/>, <see cref="MqttDisconnectMessage"/>
    /// </summary>
    PacketTooLarge = 149,

    /// <summary>
    /// 仅用于 <see cref="MqttDisconnectMessage"/> 报文，表示超过了允许的最大消息发布速率。
    /// 需要注意它与 Quota exceeded 的区别，Message rate 限制消息的发布速率，比如每秒最高可发布多少消息，
    /// Quota 限制的是资源的配额，比如客户端每天可以发布的消息数量，但客户端可能在一小时内耗尽它的配额。
    /// 适用报文：<see cref="MqttDisconnectMessage"/>
    /// </summary>
    MessageRateTooHigh = 150,

    /// <summary>
    /// 用于表示超出了配额限制。
    /// 服务端可能会对发布端的发送配额进行限制，比如每天最多为其转发 1000 条消息。
    /// 当发布端的配额耗尽，服务端就会在 <see cref="MqttPubAckMessage"/> 等确认报文中使用这个 ReasonCode 提醒对方。
    /// 另一方面，服务端还可能限制客户端的连接数量和订阅数量，当超出这一限制时，
    /// 服务端就会通过 <see cref="MqttConnAckMessage"/> 或者 <see cref="MqttSubAckMessage"/> 报文向客户端指示当前超出了配额。
    /// 一些严格的客户端和服务端，在发现对端超出配额时，可能会选择发送 <see cref="MqttDisconnectMessage"/> 报文然后关闭连接。
    /// 适用报文：<see cref="MqttConnAckMessage"/>, <see cref="MqttPubAckMessage"/>, <see cref="MqttPubRecMessage"/>, <see cref="MqttSubAckMessage"/>, <see cref="MqttDisconnectMessage"/>
    /// </summary>
    QuotaExceeded = 151,

    /// <summary>
    /// 仅用于 <see cref="MqttDisconnectMessage"/> 报文，向客户端指示连接因为管理操作而被关闭，例如运维人员在后台踢除了这个客户端连接等等。
    /// 适用报文：<see cref="MqttDisconnectMessage"/>
    /// </summary>
    AdministrativeAction = 152,

    /// <summary>
    /// 当消息中包含 Payload Format Indicator 属性时，接收方可以检查消息中 Payload 的格式与该属性是否匹配。
    /// 如果不匹配，接收方需要发送 ReasonCode 为该值的确认报文。一些严格的客户端或者服务器，
    /// 可能会直接发送 <see cref="MqttDisconnectMessage"/> 报文然后关闭网络连接。如果是 CONNECT 报文中的遗嘱消息存在问题，
    /// 服务端将发送 ReasonCode 为该值的 <see cref="MqttConnAckMessage"/> 报文然后关闭网络连接。
    /// 适用报文：<see cref="MqttConnAckMessage"/>, <see cref="MqttPubAckMessage"/>, <see cref="MqttPubRecMessage"/>, <see cref="MqttDisconnectMessage"/>
    /// </summary>
    PayloadFormatInvalid = 153,

    /// <summary>
    /// 当服务端不支持保留消息，但是客户端发送了保留消息时，服务端就会向它发送 ReasonCode 为该值的 <see cref="MqttDisconnectMessage"/> 报文然后关闭网络连接。
    /// 由于客户端还可以在连接时将遗嘱消息设置为保留消息，所以服务端也可能在 <see cref="MqttConnAckMessage"/> 报文中使用这个 ReasonCode。
    /// 适用报文：<see cref="MqttConnAckMessage"/>, <see cref="MqttDisconnectMessage"/>
    /// </summary>
    RetainNotSupported = 154,

    /// <summary>
    /// 用于表示不支持当前的 QoS 等级。如果客户端在消息（包括遗嘱消息）中指定的 QoS 大于服务端支持的最大 QoS，
    /// 服务端将会发送 ReasonCode 为该值的 <see cref="MqttDisconnectMessage"/> 或者 <see cref="MqttConnAckMessage"/> 报文然后关闭网络连接。
    /// 在大部份情况下，这个 ReasonCode 都是由服务端使用。但是在客户端收到不是来自订阅的消息，
    /// 并且消息的 QoS 大于它支持的最大 QoS 时，它也会发送 ReasonCode 为该值的 <see cref="MqttDisconnectMessage"/> 报文然后关闭网络连接。
    /// 这种情况通常意味着服务端的实现可能存在问题。
    /// 适用报文：<see cref="MqttConnAckMessage"/>, <see cref="MqttDisconnectMessage"/>
    /// </summary>
    QoSNotSupported = 155,

    /// <summary>
    /// 服务端在 <see cref="MqttConnAckMessage"/> 或者 <see cref="MqttDisconnectMessage"/> 报文中通过这个 ReasonCode 告知客户端应该临时切换到另一个服务端。
    /// 如果另一个服务端不是客户端已知的，那么这个 ReasonCode 还需要配合 Server Reference 属性一起使用，
    /// 以告知客户端新的服务端的地址。
    /// 适用报文：<see cref="MqttConnAckMessage"/>, <see cref="MqttDisconnectMessage"/>
    /// </summary>
    UseAnotherServer = 156,

    /// <summary>
    /// 服务端在 <see cref="MqttConnAckMessage"/> 或者 <see cref="MqttDisconnectMessage"/> 报文中通过这个 ReasonCode 告知客户端应该永久切换到另一个服务端。
    /// 如果另一个服务端不是客户端已知的，那么这个 ReasonCode 还需要配合 Server Reference 属性一起使用，
    /// 以告知客户端新的服务端的地址。
    /// 适用报文：<see cref="MqttConnAckMessage"/>, <see cref="MqttDisconnectMessage"/>
    /// </summary>
    ServerMoved = 157,

    /// <summary>
    /// 当服务端不支持共享订阅，但是客户端尝试建立共享订阅时，服务端可以发送 ReasonCode 为该值的 <see cref="MqttSubAckMessage"/> 报文拒绝这次订阅请求，
    /// 也可以直接发送 ReasonCode 为该值的 <see cref="MqttDisconnectMessage"/> 报文然后关闭网络连接。
    /// 适用报文：<see cref="MqttSubAckMessage"/>, <see cref="MqttDisconnectMessage"/>
    /// </summary>
    SharedSubscriptionsNotSupported = 158,

    /// <summary>
    /// 用于表示客户端已超过连接速率限制。服务端可以对客户端的连接速率做出限制，
    /// 客户端连接过快时，服务端可以发送 ReasonCode 为该值的<see cref="MqttConnAckMessage"/>报文来拒绝新的连接。
    /// 当然这并不是绝对的情况，考虑到不是所有的客户端都会等待一段时间再重新发起连接，
    /// 一些服务端实现可能会选择暂时挂起连接而不是返回 <see cref="MqttConnAckMessage"/>。
    /// 适用报文：<see cref="MqttConnAckMessage"/>, <see cref="MqttDisconnectMessage"/>
    /// </summary>
    ConnectionRateExceeded = 159,

    /// <summary>
    /// 仅用于 <see cref="MqttDisconnectMessage"/> 报文，并且只有服务端可以使用。出于安全性的考虑，
    /// 服务端可以限制单次授权中客户端的最大连接时间，比如在使用 JWT 认证时，
    /// 客户端连接不应在 JWT 过期后继续保持。这种情况下，服务端可以发送 ReasonCode 为该值的 <see cref="MqttDisconnectMessage"/> 报文，
    /// 向客户端指示连接因为超过授权的最大连接时间而被关闭。客户端可以在收到包含这个 ReasonCode 的 <see cref="MqttDisconnectMessage"/> 报文后，
    /// 重新获取认证凭据然后再次请求连接。
    /// 适用报文：<see cref="MqttDisconnectMessage"/>
    /// </summary>
    MaximumConnectTime = 160,

    /// <summary>
    /// 当服务端不支持订阅标识符，但是客户端的订阅请求中包含了订阅标识符时，
    /// 服务端可以发送 ReasonCode 为该值的 <see cref="MqttSubAckMessage"/> 报文拒绝这次订阅请求，
    /// 也可以直接发送 ReasonCode 为该值的 <see cref="MqttDisconnectMessage"/> 报文然后关闭网络连接。
    /// 适用报文：<see cref="MqttSubAckMessage"/>, <see cref="MqttDisconnectMessage"/>
    /// </summary>
    SubscriptionIdentifiersNotSupported = 161,

    /// <summary>
    /// 当服务端不支持通配符订阅，但是客户端的订阅请求中包含了主题通配符时，
    /// 服务端可以发送 ReasonCode 为该值的 <see cref="MqttSubAckMessage"/> 报文拒绝这次订阅请求，
    /// 也可以直接发送 ReasonCode 为该值的 <see cref="MqttDisconnectMessage"/> 报文然后关闭网络连接。
    /// 适用报文：<see cref="MqttSubAckMessage"/>, <see cref="MqttDisconnectMessage"/>
    /// </summary>
    WildcardSubscriptionsNotSupported = 162
}