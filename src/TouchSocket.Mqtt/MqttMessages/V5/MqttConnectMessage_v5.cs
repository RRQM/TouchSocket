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

public partial class MqttConnectMessage
{
    private List<MqttUserProperty> m_willUserProperties;

    #region Mqtt Properties

    /// <summary>
    /// 获取或设置认证数据。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public ReadOnlyMemory<byte> AuthenticationData { get; private set; }

    /// <summary>
    /// 获取或设置认证方法。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public string AuthenticationMethod { get; private set; }

    /// <summary>
    /// 获取或设置最大数据包大小。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public uint MaximumPacketSize { get; private set; }

    /// <summary>
    /// 获取或设置接收最大值。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public ushort ReceiveMaximum { get; private set; }

    /// <summary>
    /// 获取或设置请求问题信息标志。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public bool RequestProblemInformation { get; private set; }

    /// <summary>
    /// 获取或设置请求响应信息标志。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public bool RequestResponseInformation { get; private set; }

    /// <summary>
    /// 获取或设置会话过期时间间隔。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public uint SessionExpiryInterval { get; private set; }

    /// <summary>
    /// 获取或设置主题别名最大值。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public ushort TopicAliasMaximum { get; private set; }

    #endregion Mqtt Properties

    #region Will Properties

    /// <summary>
    /// 获取或设置遗嘱内容类型。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public string WillContentType { get; private set; }

    /// <summary>
    /// 获取或设置遗嘱关联数据。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public ReadOnlyMemory<byte> WillCorrelationData { get; private set; }

    /// <summary>
    /// 获取或设置遗嘱延迟时间间隔。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public uint WillDelayInterval { get; private set; }

    /// <summary>
    /// 获取或设置遗嘱消息过期时间间隔。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public uint WillMessageExpiryInterval { get; private set; }

    /// <summary>
    /// 获取或设置遗嘱负载格式指示符。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public MqttPayloadFormatIndicator WillPayloadFormatIndicator { get; private set; }

    /// <summary>
    /// 获取或设置遗嘱响应主题。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public string WillResponseTopic { get; private set; }

    /// <summary>
    /// 获取遗嘱用户属性列表。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public IReadOnlyList<MqttUserProperty> WillUserProperties => this.m_willUserProperties ?? MqttUtility.EmptyUserProperties;

    #endregion Will Properties

    /// <summary>
    /// 获取一个值，该值指示是否新开会话。
    /// </summary>
    /// <remarks>
    /// Mqtt 5.0.0以上
    /// </remarks>
    public bool CleanStart => this.m_connectFlags.GetBit(1);

    /// <inheritdoc/>
    protected override void BuildVariableBodyWithMqtt5<TWriter>(ref TWriter writer)
    {
        MqttExtension.WriteMqttInt16String(ref writer, this.ProtocolName);
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)this.Version);
        WriterExtension.WriteValue<TWriter, byte>(ref writer, this.m_connectFlags);
        WriterExtension.WriteValue<TWriter, ushort>(ref writer, this.KeepAlive, EndianType.Big);

        #region Properties

        MqttExtension.WriteSessionExpiryInterval(ref writer, this.SessionExpiryInterval);
        MqttExtension.WriteAuthenticationMethod(ref writer, this.AuthenticationMethod);
        MqttExtension.WriteAuthenticationData(ref writer, this.AuthenticationData.Span);
        MqttExtension.WriteReceiveMaximum(ref writer, this.ReceiveMaximum);
        MqttExtension.WriteTopicAliasMaximum(ref writer, this.TopicAliasMaximum);
        MqttExtension.WriteMaximumPacketSize(ref writer, this.MaximumPacketSize);
        MqttExtension.WriteRequestResponseInformation(ref writer, this.RequestResponseInformation);
        MqttExtension.WriteRequestProblemInformation(ref writer, this.RequestProblemInformation);
        MqttExtension.WriteUserProperties(ref writer, this.UserProperties);

        #endregion Properties

        MqttExtension.WriteMqttInt16String(ref writer, this.ClientId);
        if (this.WillFlag)
        {
            MqttExtension.WritePayloadFormatIndicator(ref writer, this.WillPayloadFormatIndicator);
            MqttExtension.WriteMessageExpiryInterval(ref writer, this.WillMessageExpiryInterval);
            MqttExtension.WriteResponseTopic(ref writer, this.WillResponseTopic);
            MqttExtension.WriteCorrelationData(ref writer, this.WillCorrelationData.Span);
            MqttExtension.WriteContentType(ref writer, this.WillContentType);
            MqttExtension.WriteWillDelayInterval(ref writer, this.WillDelayInterval);
            MqttExtension.WriteMqttInt16String(ref writer, this.WillTopic);
            MqttExtension.WriteMqttInt16Memory(ref writer, this.WillPayload);
            MqttExtension.WriteUserProperties(ref writer, this.WillUserProperties);
        }

        if (this.UserNameFlag)
        {
            MqttExtension.WriteMqttInt16String(ref writer, this.UserName);
        }
        if (this.PasswordFlag)
        {
            MqttExtension.WriteMqttInt16String(ref writer, this.Password);
        }
    }

    /// <inheritdoc/>
    protected override void UnpackWithMqtt5<TReader>(ref TReader reader)
    {
        #region Properties

        var propertiesReader = new MqttV5PropertiesReader<TReader>(ref reader);

        while (propertiesReader.TryRead(ref reader, out var mqttPropertyId))
        {
            switch (mqttPropertyId)
            {
                case MqttPropertyId.SessionExpiryInterval:
                    {
                        this.SessionExpiryInterval = propertiesReader.ReadSessionExpiryInterval(ref reader);
                        break;
                    }
                case MqttPropertyId.AuthenticationMethod:
                    {
                        this.AuthenticationMethod = propertiesReader.ReadAuthenticationMethod(ref reader);
                        break;
                    }
                case MqttPropertyId.AuthenticationData:
                    {
                        this.AuthenticationData = propertiesReader.ReadAuthenticationData(ref reader);
                        break;
                    }
                case MqttPropertyId.ReceiveMaximum:
                    {
                        this.ReceiveMaximum = propertiesReader.ReadReceiveMaximum(ref reader);
                        break;
                    }
                case MqttPropertyId.TopicAliasMaximum:
                    {
                        this.TopicAliasMaximum = propertiesReader.ReadTopicAliasMaximum(ref reader);
                        break;
                    }
                case MqttPropertyId.MaximumPacketSize:
                    {
                        this.MaximumPacketSize = propertiesReader.ReadMaximumPacketSize(ref reader);
                        break;
                    }
                case MqttPropertyId.RequestResponseInformation:
                    {
                        this.RequestResponseInformation = propertiesReader.ReadRequestResponseInformation(ref reader);
                        break;
                    }
                case MqttPropertyId.RequestProblemInformation:
                    {
                        this.RequestProblemInformation = propertiesReader.ReadRequestProblemInformation(ref reader);
                        break;
                    }
                case MqttPropertyId.UserProperty:
                    {
                        this.AddUserProperty(propertiesReader.ReadUserProperty(ref reader));
                        break;
                    }
                default:
                    ThrowHelper.ThrowInvalidEnumArgumentException(mqttPropertyId);
                    break;
            }
        }

        //this.SessionExpiryInterval = propertiesReader.SessionExpiryInterval;
        //this.AuthenticationMethod = propertiesReader.AuthenticationMethod;
        //this.AuthenticationData = propertiesReader.AuthenticationData;
        //this.ReceiveMaximum = propertiesReader.ReceiveMaximum;
        //this.TopicAliasMaximum = propertiesReader.TopicAliasMaximum;
        //this.MaximumPacketSize = propertiesReader.MaximumPacketSize;
        //this.RequestResponseInformation = propertiesReader.RequestResponseInformation;
        //this.RequestProblemInformation = propertiesReader.RequestProblemInformation;
        //this.UserProperties = propertiesReader.UserProperties;

        #endregion Properties

        this.ClientId = MqttExtension.ReadMqttInt16String(ref reader);
        if (this.WillFlag)
        {
            var willPropertiesReader = new MqttV5PropertiesReader<TReader>(ref reader);

            while (willPropertiesReader.TryRead(ref reader, out var mqttPropertyId))
            {
                switch (mqttPropertyId)
                {
                    case MqttPropertyId.PayloadFormatIndicator:
                        {
                            this.WillPayloadFormatIndicator = willPropertiesReader.ReadPayloadFormatIndicator(ref reader);
                            break;
                        }
                    case MqttPropertyId.MessageExpiryInterval:
                        {
                            this.WillMessageExpiryInterval = willPropertiesReader.ReadMessageExpiryInterval(ref reader);
                            break;
                        }
                    case MqttPropertyId.ResponseTopic:
                        {
                            this.WillResponseTopic = willPropertiesReader.ReadResponseTopic(ref reader);
                            break;
                        }
                    case MqttPropertyId.CorrelationData:
                        {
                            this.WillCorrelationData = willPropertiesReader.ReadCorrelationData(ref reader);
                            break;
                        }
                    case MqttPropertyId.ContentType:
                        {
                            this.WillContentType = willPropertiesReader.ReadContentType(ref reader);
                            break;
                        }
                    case MqttPropertyId.WillDelayInterval:
                        {
                            this.WillDelayInterval = willPropertiesReader.ReadWillDelayInterval(ref reader);
                            break;
                        }
                    case MqttPropertyId.UserProperty:
                        {
                            this.AddWillUserProperties(willPropertiesReader.ReadUserProperty(ref reader));
                            break;
                        }
                    default:
                        ThrowHelper.ThrowInvalidEnumArgumentException(mqttPropertyId);
                        break;
                }
            }

            this.WillTopic = MqttExtension.ReadMqttInt16String(ref reader);
            this.WillPayload = MqttExtension.ReadMqttInt16Memory(ref reader);
        }

        if (this.UserNameFlag)
        {
            this.UserName = MqttExtension.ReadMqttInt16String(ref reader);
        }

        if (this.PasswordFlag)
        {
            this.Password = MqttExtension.ReadMqttInt16String(ref reader);
        }
    }

    public void AddWillUserProperties(MqttUserProperty mqttUserProperty)
    {
        this.m_willUserProperties ??= new List<MqttUserProperty>();
        this.m_willUserProperties.Add(mqttUserProperty);
    }
}