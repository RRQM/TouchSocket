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

using TouchSocket.Core;

namespace TouchSocket.Mqtt;

/// <summary>
/// 表示MQTT连接消息。
/// </summary>
public sealed partial class MqttConnectMessage : MqttUserPropertiesMessage
{
    private byte m_connectFlags;

    /// <summary>
    /// 初始化 <see cref="MqttConnectMessage"/> 类的新实例。
    /// </summary>
    public MqttConnectMessage()
    {
        this.ProtocolName = MqttUtility.MqttProtocolName;
    }

    /// <summary>
    /// 初始化 <see cref="MqttConnectMessage"/> 类的新实例。
    /// </summary>
    /// <param name="options">MQTT连接选项。</param>
    public MqttConnectMessage(MqttConnectOptions options)
    {
        this.ProtocolName = MqttUtility.MqttProtocolName;
        this.Version = options.Version;
        this.ClientId = options.ClientId;
        this.KeepAlive = options.KeepAlive;
        this.UserName = options.UserName;
        this.Password = options.Password;
        this.m_connectFlags = options.BuildConnectFlags();

        //MQTT5
        this.AuthenticationData = options.AuthenticationData;
        this.AuthenticationMethod = options.AuthenticationMethod;
        this.MaximumPacketSize = options.MaximumPacketSize;
        this.ReceiveMaximum = options.ReceiveMaximum;
        this.RequestProblemInformation = options.RequestProblemInformation;
        this.RequestResponseInformation = options.RequestResponseInformation;
        this.SessionExpiryInterval = options.SessionExpiryInterval;
        this.TopicAliasMaximum = options.TopicAliasMaximum;

        this.WillContentType = options.WillContentType;
        this.WillCorrelationData = options.WillCorrelationData;
        this.WillDelayInterval = options.WillDelayInterval;

        this.WillDelayInterval = options.WillDelayInterval;
        this.WillMessageExpiryInterval = options.WillMessageExpiryInterval;
        this.WillPayloadFormatIndicator = options.WillPayloadFormatIndicator;

        this.WillMessage = options.WillMessage;
        this.WillTopic = options.WillTopic;

        foreach (var item in options.UserProperties.GetSafeEnumerator())
        {
            this.AddUserProperty(item);
        }

        foreach (var item in options.WillUserProperties.GetSafeEnumerator())
        {
            this.AddWillUserProperties(item);
        }

    }

    /// <summary>
    /// 获取或设置客户端Id。
    /// </summary>
    public string ClientId { get; private set; }

    /// <summary>
    /// 获取一个值，该值指示是否为重复消息。
    /// </summary>
    public bool DUP => base.Flags.GetBit(3);

    /// <summary>
    /// 获取或设置保活时长。
    /// </summary>
    public ushort KeepAlive { get; private set; }

    /// <summary>
    /// 获取MQTT数据包类型。
    /// </summary>
    public override MqttMessageType MessageType => MqttMessageType.Connect;

    /// <summary>
    /// 获取或设置密码。
    /// </summary>
    public string Password { get; private set; }

    /// <summary>
    /// 获取或设置协议名称。
    /// </summary>
    public string ProtocolName { get; private set; }

    /// <summary>
    /// 获取服务质量级别。
    /// </summary>
    public QosLevel QosLevel => base.Flags.GetQosLevel(1);

    /// <summary>
    /// 获取一个值，该值指示是否保留消息。
    /// </summary>
    public bool Retain => base.Flags.GetBit(0);

    /// <summary>
    /// 获取或设置用户名。
    /// </summary>
    public string UserName { get; private set; }

    /// <inheritdoc/>
    public override void Unpack<TByteBlock>(ref TByteBlock byteBlock)
    {
        var firstByte = byteBlock.ReadByte();
        this.SetFlags((byte)firstByte.GetLow4());
        this.RemainingLength = MqttExtension.ReadVariableByteInteger(ref byteBlock);

        this.ProtocolName = MqttExtension.ReadMqttInt16String(ref byteBlock);

        this.Version = (MqttProtocolVersion)byteBlock.ReadByte();

        this.m_connectFlags = byteBlock.ReadByte();
        this.KeepAlive = byteBlock.ReadUInt16(EndianType.Big);

        switch (this.Version)
        {
            case MqttProtocolVersion.V310:
            case MqttProtocolVersion.V311:
                this.UnpackWithMqtt3(ref byteBlock);
                break;

            case MqttProtocolVersion.V500:
                this.UnpackWithMqtt5(ref byteBlock);
                break;

            case MqttProtocolVersion.Unknown:
            default:
                ThrowHelper.ThrowInvalidEnumArgumentException(this.Version);
                return;
        }
    }

    /// <inheritdoc/>
    protected override void BuildVariableBodyWithMqtt3<TByteBlock>(ref TByteBlock byteBlock)
    {
        MqttExtension.WriteMqttInt16String(ref byteBlock, this.ProtocolName);
        byteBlock.WriteByte((byte)this.Version);
        byteBlock.WriteByte(this.m_connectFlags);
        byteBlock.WriteUInt16(this.KeepAlive, EndianType.Big);

        MqttExtension.WriteMqttInt16String(ref byteBlock, this.ClientId);
        if (this.WillFlag)
        {
            MqttExtension.WriteMqttInt16String(ref byteBlock, this.WillTopic);
            MqttExtension.WriteMqttInt16String(ref byteBlock, this.WillMessage);
        }

        if (this.UserNameFlag)
        {
            MqttExtension.WriteMqttInt16String(ref byteBlock, this.UserName);
        }
        if (this.PasswordFlag)
        {
            MqttExtension.WriteMqttInt16String(ref byteBlock, this.Password);
        }
    }

    /// <inheritdoc/>
    protected override int GetMinimumRemainingLength()
    {
        return 0;
    }

    #region ConnectFlags

    /// <summary>
    /// 获取一个值，该值指示是否清理会话。
    /// </summary>
    /// <remarks>
    /// MQTT 5.0.0以下
    /// </remarks>
    public bool CleanSession => this.m_connectFlags.GetBit(1);

    /// <summary>
    /// 获取一个值，该值指示是否包含密码标志。
    /// </summary>
    public bool PasswordFlag => this.m_connectFlags.GetBit(6);

    /// <summary>
    /// 获取一个值，该值指示是否为保留位。
    /// </summary>
    public bool Reserved => this.m_connectFlags.GetBit(0);

    /// <summary>
    /// 获取一个值，该值指示是否包含用户名标志。
    /// </summary>
    public bool UserNameFlag => this.m_connectFlags.GetBit(7);

    /// <summary>
    /// 获取一个值，该值指示是否包含遗嘱标志。
    /// </summary>
    public bool WillFlag => this.m_connectFlags.GetBit(2);

    /// <summary>
    /// 获取或设置遗嘱消息。
    /// </summary>
    public string WillMessage { get; set; }

    /// <summary>
    /// 获取遗嘱服务质量级别。
    /// </summary>
    public QosLevel WillQos => this.m_connectFlags.GetQosLevel(3);

    /// <summary>
    /// 获取一个值，该值指示是否保留遗嘱消息。
    /// </summary>
    public bool WillRetain => this.m_connectFlags.GetBit(5);

    /// <summary>
    /// 获取或设置遗嘱主题。
    /// </summary>
    public string WillTopic { get; set; }

    #endregion ConnectFlags

    /// <inheritdoc/>
    protected override void UnpackWithMqtt3<TByteBlock>(ref TByteBlock byteBlock)
    {
        this.ClientId = MqttExtension.ReadMqttInt16String(ref byteBlock);
        if (this.WillFlag)
        {
            this.WillTopic = MqttExtension.ReadMqttInt16String(ref byteBlock);
            this.WillMessage = MqttExtension.ReadMqttInt16String(ref byteBlock);
        }

        if (this.UserNameFlag)
        {
            this.UserName = MqttExtension.ReadMqttInt16String(ref byteBlock);
        }

        if (this.PasswordFlag)
        {
            this.Password = MqttExtension.ReadMqttInt16String(ref byteBlock);
        }
    }
}