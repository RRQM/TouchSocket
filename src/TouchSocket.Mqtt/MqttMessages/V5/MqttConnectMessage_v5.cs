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

using System.Collections.Generic;
using TouchSocket.Core;

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
    public byte[] AuthenticationData { get; private set; }

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
    public byte[] WillCorrelationData { get; private set; }

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
    protected override void BuildVariableBodyWithMqtt5<TByteBlock>(ref TByteBlock byteBlock)
    {
        MqttExtension.WriteMqttInt16String(ref byteBlock, this.ProtocolName);
        byteBlock.WriteByte((byte)this.Version);
        byteBlock.WriteByte(this.m_connectFlags);
        byteBlock.WriteUInt16(this.KeepAlive, EndianType.Big);

        #region Properties

        MqttExtension.WriteSessionExpiryInterval(ref byteBlock, this.SessionExpiryInterval);
        MqttExtension.WriteAuthenticationMethod(ref byteBlock, this.AuthenticationMethod);
        MqttExtension.WriteAuthenticationData(ref byteBlock, this.AuthenticationData);
        MqttExtension.WriteReceiveMaximum(ref byteBlock, this.ReceiveMaximum);
        MqttExtension.WriteTopicAliasMaximum(ref byteBlock, this.TopicAliasMaximum);
        MqttExtension.WriteMaximumPacketSize(ref byteBlock, this.MaximumPacketSize);
        MqttExtension.WriteRequestResponseInformation(ref byteBlock, this.RequestResponseInformation);
        MqttExtension.WriteRequestProblemInformation(ref byteBlock, this.RequestProblemInformation);
        MqttExtension.WriteUserProperties(ref byteBlock, this.UserProperties);

        #endregion Properties

        MqttExtension.WriteMqttInt16String(ref byteBlock, this.ClientId);
        if (this.WillFlag)
        {
            MqttExtension.WritePayloadFormatIndicator(ref byteBlock, this.WillPayloadFormatIndicator);
            MqttExtension.WriteMessageExpiryInterval(ref byteBlock, this.WillMessageExpiryInterval);
            MqttExtension.WriteResponseTopic(ref byteBlock, this.WillResponseTopic);
            MqttExtension.WriteCorrelationData(ref byteBlock, this.WillCorrelationData);
            MqttExtension.WriteContentType(ref byteBlock, this.WillContentType);
            MqttExtension.WriteWillDelayInterval(ref byteBlock, this.WillDelayInterval);
            MqttExtension.WriteMqttInt16String(ref byteBlock, this.WillTopic);
            MqttExtension.WriteMqttInt16String(ref byteBlock, this.WillMessage);
            MqttExtension.WriteUserProperties(ref byteBlock, this.WillUserProperties);
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
    protected override void UnpackWithMqtt5<TByteBlock>(ref TByteBlock byteBlock)
    {
        #region Properties

        var propertiesReader = new MqttV5PropertiesReader<TByteBlock>(ref byteBlock);

        while (propertiesReader.TryRead(ref byteBlock, out var mqttPropertyId))
        {
            switch (mqttPropertyId)
            {
                case MqttPropertyId.SessionExpiryInterval:
                    {
                        this.SessionExpiryInterval = propertiesReader.ReadSessionExpiryInterval(ref byteBlock);
                        break;
                    }
                case MqttPropertyId.AuthenticationMethod:
                    {
                        this.AuthenticationMethod = propertiesReader.ReadAuthenticationMethod(ref byteBlock);
                        break;
                    }
                case MqttPropertyId.AuthenticationData:
                    {
                        this.AuthenticationData = propertiesReader.ReadAuthenticationData(ref byteBlock);
                        break;
                    }
                case MqttPropertyId.ReceiveMaximum:
                    {
                        this.ReceiveMaximum = propertiesReader.ReadReceiveMaximum(ref byteBlock);
                        break;
                    }
                case MqttPropertyId.TopicAliasMaximum:
                    {
                        this.TopicAliasMaximum = propertiesReader.ReadTopicAliasMaximum(ref byteBlock);
                        break;
                    }
                case MqttPropertyId.MaximumPacketSize:
                    {
                        this.MaximumPacketSize = propertiesReader.ReadMaximumPacketSize(ref byteBlock);
                        break;
                    }
                case MqttPropertyId.RequestResponseInformation:
                    {
                        this.RequestResponseInformation = propertiesReader.ReadRequestResponseInformation(ref byteBlock);
                        break;
                    }
                case MqttPropertyId.RequestProblemInformation:
                    {
                        this.RequestProblemInformation = propertiesReader.ReadRequestProblemInformation(ref byteBlock);
                        break;
                    }
                case MqttPropertyId.UserProperty:
                    {
                        this.AddUserProperty(propertiesReader.ReadUserProperty(ref byteBlock));
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

        this.ClientId = MqttExtension.ReadMqttInt16String(ref byteBlock);
        if (this.WillFlag)
        {
            var willPropertiesReader = new MqttV5PropertiesReader<TByteBlock>(ref byteBlock);

            while (willPropertiesReader.TryRead(ref byteBlock, out var mqttPropertyId))
            {
                switch (mqttPropertyId)
                {
                    case MqttPropertyId.PayloadFormatIndicator:
                        {
                            this.WillPayloadFormatIndicator = willPropertiesReader.ReadPayloadFormatIndicator(ref byteBlock);
                            break;
                        }
                    case MqttPropertyId.MessageExpiryInterval:
                        {
                            this.WillMessageExpiryInterval = willPropertiesReader.ReadMessageExpiryInterval(ref byteBlock);
                            break;
                        }
                    case MqttPropertyId.ResponseTopic:
                        {
                            this.WillResponseTopic = willPropertiesReader.ReadResponseTopic(ref byteBlock);
                            break;
                        }
                    case MqttPropertyId.CorrelationData:
                        {
                            this.WillCorrelationData = willPropertiesReader.ReadCorrelationData(ref byteBlock);
                            break;
                        }
                    case MqttPropertyId.ContentType:
                        {
                            this.WillContentType = willPropertiesReader.ReadContentType(ref byteBlock);
                            break;
                        }
                    case MqttPropertyId.WillDelayInterval:
                        {
                            this.WillDelayInterval = willPropertiesReader.ReadWillDelayInterval(ref byteBlock);
                            break;
                        }
                    case MqttPropertyId.UserProperty:
                        {
                            this.AddWillUserProperties(willPropertiesReader.ReadUserProperty(ref byteBlock));
                            break;
                        }
                    default:
                        ThrowHelper.ThrowInvalidEnumArgumentException(mqttPropertyId);
                        break;
                }
            }

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

    public void AddWillUserProperties(MqttUserProperty mqttUserProperty)
    {
        this.m_willUserProperties ??= new List<MqttUserProperty>();
        this.m_willUserProperties.Add(mqttUserProperty);
    }
}