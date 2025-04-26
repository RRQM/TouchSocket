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

public partial class MqttPublishMessage
{
    private readonly List<uint> m_subscriptionIdentifiers = new List<uint>();

    public string ContentType { get; set; }

    public byte[] CorrelationData { get; set; }

    public uint MessageExpiryInterval { get; set; }

    public MqttPayloadFormatIndicator PayloadFormatIndicator { get; set; } = MqttPayloadFormatIndicator.Unspecified;

    public string ResponseTopic { get; set; }

    public IReadOnlyList<uint> SubscriptionIdentifiers => this.m_subscriptionIdentifiers;

    public ushort TopicAlias { get; set; }

    /// <inheritdoc/>
    protected override void BuildVariableBodyWithMqtt5<TByteBlock>(ref TByteBlock byteBlock)
    {
        MqttExtension.WriteMqttInt16String(ref byteBlock, this.TopicName);
        if (this.QosLevel != QosLevel.AtMostOnce)
        {
            byteBlock.WriteUInt16(this.MessageId, EndianType.Big);
        }

        #region properties
        var variableByteIntegerRecorder = new VariableByteIntegerRecorder();
        variableByteIntegerRecorder.CheckOut(ref byteBlock);

        MqttExtension.WritePayloadFormatIndicator(ref byteBlock, this.PayloadFormatIndicator);
        MqttExtension.WriteMessageExpiryInterval(ref byteBlock, this.MessageExpiryInterval);
        MqttExtension.WriteTopicAlias(ref byteBlock, this.TopicAlias);
        MqttExtension.WriteResponseTopic(ref byteBlock, this.ResponseTopic);
        MqttExtension.WriteCorrelationData(ref byteBlock, this.CorrelationData);

        foreach (var item in this.m_subscriptionIdentifiers)
        {
            MqttExtension.WriteSubscriptionIdentifier(ref byteBlock, item);
        }

        MqttExtension.WriteContentType(ref byteBlock, this.ContentType);
        MqttExtension.WriteUserProperties(ref byteBlock, this.UserProperties);
        variableByteIntegerRecorder.CheckIn(ref byteBlock);

        #endregion properties

        byteBlock.Write(this.Payload.Span);
    }

    /// <inheritdoc/>
    protected override void UnpackWithMqtt5<TByteBlock>(ref TByteBlock byteBlock)
    {
        this.TopicName = MqttExtension.ReadMqttInt16String(ref byteBlock);
        if (this.QosLevel != QosLevel.AtMostOnce)
        {
            this.MessageId = byteBlock.ReadUInt16(EndianType.Big);
        }

        #region properties

        var propertiesReader = new MqttV5PropertiesReader<TByteBlock>(ref byteBlock);

        while (propertiesReader.TryRead(ref byteBlock, out var mqttPropertyId))
        {
            switch (mqttPropertyId)
            {
                case MqttPropertyId.PayloadFormatIndicator:
                    this.PayloadFormatIndicator = propertiesReader.ReadPayloadFormatIndicator(ref byteBlock);
                    break;
                case MqttPropertyId.MessageExpiryInterval:
                    this.MessageExpiryInterval = propertiesReader.ReadMessageExpiryInterval(ref byteBlock);
                    break;
                case MqttPropertyId.TopicAlias:
                    this.TopicAlias = propertiesReader.ReadTopicAlias(ref byteBlock);
                    break;
                case MqttPropertyId.ResponseTopic:
                    this.ResponseTopic = propertiesReader.ReadResponseTopic(ref byteBlock);
                    break;
                case MqttPropertyId.CorrelationData:
                    this.CorrelationData = propertiesReader.ReadCorrelationData(ref byteBlock);
                    break;
                case MqttPropertyId.SubscriptionIdentifier:
                    this.m_subscriptionIdentifiers.Add(propertiesReader.ReadSubscriptionIdentifier(ref byteBlock));
                    break;
                case MqttPropertyId.ContentType:
                    this.ContentType = propertiesReader.ReadContentType(ref byteBlock);
                    break;
                case MqttPropertyId.UserProperty:
                    this.AddUserProperty(propertiesReader.ReadUserProperty(ref byteBlock));
                    break;
                default:
                    ThrowHelper.ThrowInvalidEnumArgumentException(mqttPropertyId);
                    break;
            }
        }

        //this.PayloadFormatIndicator = propertiesReader.PayloadFormatIndicator;

        //this.MessageExpiryInterval = propertiesReader.MessageExpiryInterval;

        //this.TopicAlias = propertiesReader.TopicAlias;

        //this.ResponseTopic = propertiesReader.ResponseTopic;

        //this.CorrelationData = propertiesReader.CorrelationData;

        //this.m_subscriptionIdentifiers.Add(propertiesReader.SubscriptionIdentifier);

        //this.ContentType = propertiesReader.ContentType;

        //this.UserProperties = propertiesReader.UserProperties;

        #endregion properties

        this.Payload = this.ReadPayload(ref byteBlock);
    }
}