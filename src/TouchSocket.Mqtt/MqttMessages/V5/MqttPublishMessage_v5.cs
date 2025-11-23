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

public partial class MqttPublishMessage
{
    private readonly List<uint> m_subscriptionIdentifiers = new List<uint>();

    public string ContentType { get; set; }

    public ReadOnlyMemory<byte> CorrelationData { get; set; }

    public uint MessageExpiryInterval { get; set; }

    public MqttPayloadFormatIndicator PayloadFormatIndicator { get; set; } = MqttPayloadFormatIndicator.Unspecified;

    public string ResponseTopic { get; set; }

    public IReadOnlyList<uint> SubscriptionIdentifiers => this.m_subscriptionIdentifiers;

    public ushort TopicAlias { get; set; }

    /// <inheritdoc/>
    protected override void BuildVariableBodyWithMqtt5<TWriter>(ref TWriter writer)
    {
        MqttExtension.WriteMqttInt16String(ref writer, this.TopicName);
        if (this.QosLevel != QosLevel.AtMostOnce)
        {
            WriterExtension.WriteValue<TWriter, ushort>(ref writer, this.MessageId, EndianType.Big);
        }

        #region properties
        var variableByteIntegerRecorder = new VariableByteIntegerRecorder();
        var byteBlockWriter = this.CreateVariableWriter(ref writer);
        variableByteIntegerRecorder.CheckOut(ref byteBlockWriter);

        MqttExtension.WritePayloadFormatIndicator(ref byteBlockWriter, this.PayloadFormatIndicator);
        MqttExtension.WriteMessageExpiryInterval(ref byteBlockWriter, this.MessageExpiryInterval);
        MqttExtension.WriteTopicAlias(ref byteBlockWriter, this.TopicAlias);
        MqttExtension.WriteResponseTopic(ref byteBlockWriter, this.ResponseTopic);
        MqttExtension.WriteCorrelationData(ref byteBlockWriter, this.CorrelationData.Span);

        foreach (var item in this.m_subscriptionIdentifiers)
        {
            MqttExtension.WriteSubscriptionIdentifier(ref byteBlockWriter, item);
        }

        MqttExtension.WriteContentType(ref byteBlockWriter, this.ContentType);
        MqttExtension.WriteUserProperties(ref byteBlockWriter, this.UserProperties);
        variableByteIntegerRecorder.CheckIn(ref byteBlockWriter);
        writer.Advance(byteBlockWriter.Position);

        #endregion properties

        writer.Write(this.Payload.Span);
    }

    /// <inheritdoc/>
    protected override void UnpackWithMqtt5<TReader>(ref TReader reader)
    {
        this.TopicName = MqttExtension.ReadMqttInt16String(ref reader);
        if (this.QosLevel != QosLevel.AtMostOnce)
        {
            this.MessageId = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);
        }

        #region properties

        var propertiesReader = new MqttV5PropertiesReader<TReader>(ref reader);

        while (propertiesReader.TryRead(ref reader, out var mqttPropertyId))
        {
            switch (mqttPropertyId)
            {
                case MqttPropertyId.PayloadFormatIndicator:
                    this.PayloadFormatIndicator = propertiesReader.ReadPayloadFormatIndicator(ref reader);
                    break;
                case MqttPropertyId.MessageExpiryInterval:
                    this.MessageExpiryInterval = propertiesReader.ReadMessageExpiryInterval(ref reader);
                    break;
                case MqttPropertyId.TopicAlias:
                    this.TopicAlias = propertiesReader.ReadTopicAlias(ref reader);
                    break;
                case MqttPropertyId.ResponseTopic:
                    this.ResponseTopic = propertiesReader.ReadResponseTopic(ref reader);
                    break;
                case MqttPropertyId.CorrelationData:
                    this.CorrelationData = propertiesReader.ReadCorrelationData(ref reader);
                    break;
                case MqttPropertyId.SubscriptionIdentifier:
                    this.m_subscriptionIdentifiers.Add(propertiesReader.ReadSubscriptionIdentifier(ref reader));
                    break;
                case MqttPropertyId.ContentType:
                    this.ContentType = propertiesReader.ReadContentType(ref reader);
                    break;
                case MqttPropertyId.UserProperty:
                    this.AddUserProperty(propertiesReader.ReadUserProperty(ref reader));
                    break;
                default:
                    ThrowHelper.ThrowInvalidEnumArgumentException(mqttPropertyId);
                    break;
            }
        }


        #endregion properties

        this.Payload = this.ReadPayload(ref reader);
    }
}