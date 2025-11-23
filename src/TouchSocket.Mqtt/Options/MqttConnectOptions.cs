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

using System.Diagnostics.CodeAnalysis;

namespace TouchSocket.Mqtt;

/// <summary>
/// 表示 Mqtt 连接选项。
/// </summary>
public class MqttConnectOptions
{
    /// <summary>
    /// 获取或设置一个值，该值指示是否清理会话。
    /// </summary>
    public bool CleanSession { get; set; }

    /// <summary>
    /// 获取或设置客户端 ID。
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// 获取或设置保活时长。
    /// </summary>
    public ushort KeepAlive { get; set; }

    /// <summary>
    /// 获取或设置密码。
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// 获取或设置协议名称。
    /// </summary>
    public string ProtocolName { get; set; }

    /// <summary>
    /// 获取或设置用户名。
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 获取或设置版本号。
    /// </summary>
    public MqttProtocolVersion Version { get; set; }

    /// <summary>
    /// 获取或设置一个值，该值指示是否设置遗嘱标志。
    /// </summary>
    public bool WillFlag { get; set; }

    /// <summary>
    /// 获取或设置遗嘱 QoS。
    /// </summary>
    public QosLevel WillQos { get; set; }

    /// <summary>
    /// 获取或设置一个值，该值指示是否保留遗嘱。
    /// </summary>
    public bool WillRetain { get; set; }

    /// <summary>
    /// 构建连接标志。
    /// </summary>
    /// <returns>连接标志字节。</returns>
    public byte BuildConnectFlags()
    {
        byte flags = 0;
        flags = flags.SetBit(1, this.CleanSession);
        flags = flags.SetBit(2, this.WillFlag);
        flags = flags.SetQosLevel(3, this.WillQos);
        flags = flags.SetBit(5, this.WillRetain);
        flags = flags.SetBit(6, this.Password.HasValue());
        flags = flags.SetBit(7, this.UserName.HasValue());

        return flags;
    }

    #region Mqtt Properties

    [NotNull]
    public IReadOnlyList<MqttUserProperty> UserProperties { get; set; }

    /// <summary>
    /// 获取或设置认证数据。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public byte[] AuthenticationData { get; set; }

    /// <summary>
    /// 获取或设置认证方法。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public string AuthenticationMethod { get; set; }

    /// <summary>
    /// 获取或设置最大数据包大小。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public uint MaximumPacketSize { get; set; }

    /// <summary>
    /// 获取或设置接收最大值。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public ushort ReceiveMaximum { get; set; }

    /// <summary>
    /// 获取或设置请求问题信息标志。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public bool RequestProblemInformation { get; set; }

    /// <summary>
    /// 获取或设置请求响应信息标志。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public bool RequestResponseInformation { get; set; }

    /// <summary>
    /// 获取或设置会话过期时间间隔。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public uint SessionExpiryInterval { get; set; }

    /// <summary>
    /// 获取或设置主题别名最大值。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public ushort TopicAliasMaximum { get; set; }

    #endregion Mqtt Properties

    #region Will Properties

    /// <summary>
    /// 获取或设置遗嘱内容类型。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public string WillContentType { get; set; }

    /// <summary>
    /// 获取或设置遗嘱关联数据。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public byte[] WillCorrelationData { get; set; }

    /// <summary>
    /// 获取或设置遗嘱延迟时间间隔。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public uint WillDelayInterval { get; set; }

    /// <summary>
    /// 获取或设置遗嘱消息过期时间间隔。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public uint WillMessageExpiryInterval { get; set; }

    /// <summary>
    /// 获取或设置遗嘱负载格式指示符。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public MqttPayloadFormatIndicator WillPayloadFormatIndicator { get; set; }

    /// <summary>
    /// 获取或设置遗嘱响应主题。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public string WillResponseTopic { get; set; }

    /// <summary>
    /// 获取遗嘱用户属性列表。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public IReadOnlyList<MqttUserProperty> WillUserProperties { get; set; }
    public ReadOnlyMemory<byte> WillPayload { get; set; }
    public string WillTopic { get; set; }

    #endregion Will Properties

}
