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

using System;
using TouchSocket.Core;

namespace TouchSocket.Mqtt;

public partial class MqttConnAckMessage
{
    /// <summary>
    /// 获取或设置分配的客户端标识符。
    /// </summary>
    public string AssignedClientIdentifier { get; set; }

    /// <summary>
    /// 获取或设置认证数据。
    /// </summary>
    public ReadOnlyMemory<byte> AuthenticationData { get; set; }

    /// <summary>
    /// 获取或设置认证方法。
    /// </summary>
    public string AuthenticationMethod { get; set; }

    /// <summary>
    /// 获取或设置最大数据包大小。
    /// </summary>
    public uint MaximumPacketSize { get; set; }

    /// <summary>
    /// 获取或设置最大服务质量（QoS）级别。
    /// </summary>
    public QosLevel MaximumQoS { get; set; }

    /// <summary>
    /// 获取或设置原因代码。
    /// </summary>
    public MqttReasonCode ReasonCode { get; set; }

    /// <summary>
    /// 获取或设置原因字符串。
    /// </summary>
    public string ReasonString { get; set; }

    /// <summary>
    /// 获取或设置接收最大值。
    /// </summary>
    public ushort ReceiveMaximum { get; set; }

    /// <summary>
    /// 获取或设置响应信息。
    /// </summary>
    public string ResponseInformation { get; set; }

    /// <summary>
    /// 获取或设置一个值，该值指示是否可用保留。
    /// </summary>
    public bool RetainAvailable { get; set; }

    /// <summary>
    /// 获取或设置服务器保持连接时间。
    /// </summary>
    public ushort ServerKeepAlive { get; set; }

    /// <summary>
    /// 获取或设置服务器引用。
    /// </summary>
    public string ServerReference { get; set; }

    /// <summary>
    /// 获取或设置会话过期时间间隔。
    /// </summary>
    public uint SessionExpiryInterval { get; set; }

    /// <summary>
    /// 获取或设置一个值，该值指示是否可用共享订阅。
    /// </summary>
    public bool SharedSubscriptionAvailable { get; set; }

    /// <summary>
    /// 获取或设置一个值，该值指示是否可用订阅标识符。
    /// </summary>
    public bool SubscriptionIdentifiersAvailable { get; set; }

    /// <summary>
    /// 获取或设置主题别名最大值。
    /// </summary>
    public ushort TopicAliasMaximum { get; set; }

    /// <summary>
    /// 获取或设置一个值，该值指示是否可用通配符订阅。
    /// </summary>
    public bool WildcardSubscriptionAvailable { get; set; }

    /// <inheritdoc/>
    protected override void BuildVariableBodyWithMqtt5<TWriter>(ref TWriter writer)
    {
        WriterExtension.WriteValue<TWriter, byte>(ref writer, this.m_connectAcknowledgeFlags);
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)this.ReturnCode);

        var variableByteIntegerRecorder = new VariableByteIntegerRecorder();

        var byteBlockWriter = this.CreateVariableWriter(ref writer);
        variableByteIntegerRecorder.CheckOut(ref byteBlockWriter);

        MqttExtension.WriteSessionExpiryInterval(ref byteBlockWriter, this.SessionExpiryInterval);
        MqttExtension.WriteReceiveMaximum(ref byteBlockWriter, this.ReceiveMaximum);
        MqttExtension.WriteMaximumQoS(ref byteBlockWriter, this.MaximumQoS);
        MqttExtension.WriteRetainAvailable(ref byteBlockWriter, this.RetainAvailable);
        MqttExtension.WriteMaximumPacketSize(ref byteBlockWriter, this.MaximumPacketSize);
        MqttExtension.WriteAssignedClientIdentifier(ref byteBlockWriter, this.AssignedClientIdentifier);
        MqttExtension.WriteTopicAliasMaximum(ref byteBlockWriter, this.TopicAliasMaximum);
        MqttExtension.WriteReasonString(ref byteBlockWriter, this.ReasonString);
        MqttExtension.WriteWildcardSubscriptionAvailable(ref byteBlockWriter, this.WildcardSubscriptionAvailable);
        MqttExtension.WriteSubscriptionIdentifiersAvailable(ref byteBlockWriter, this.SubscriptionIdentifiersAvailable);
        MqttExtension.WriteSharedSubscriptionAvailable(ref byteBlockWriter, this.SharedSubscriptionAvailable);
        MqttExtension.WriteServerKeepAlive(ref byteBlockWriter, this.ServerKeepAlive);
        MqttExtension.WriteResponseInformation(ref byteBlockWriter, this.ResponseInformation);
        MqttExtension.WriteServerReference(ref byteBlockWriter, this.ServerReference);
        MqttExtension.WriteAuthenticationMethod(ref byteBlockWriter, this.AuthenticationMethod);
        MqttExtension.WriteAuthenticationData(ref byteBlockWriter, this.AuthenticationData.Span);
        MqttExtension.WriteUserProperties(ref byteBlockWriter, this.UserProperties);

        variableByteIntegerRecorder.CheckIn(ref byteBlockWriter);
        writer.Advance(byteBlockWriter.Position);
    }

    /// <inheritdoc/>
    protected override void UnpackWithMqtt5<TReader>(ref TReader reader)
    {
        this.m_connectAcknowledgeFlags = ReaderExtension.ReadValue<TReader, byte>(ref reader);
        this.ReturnCode = (MqttReasonCode)ReaderExtension.ReadValue<TReader, byte>(ref reader);

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
                case MqttPropertyId.ReceiveMaximum:
                    {
                        this.ReceiveMaximum = propertiesReader.ReadReceiveMaximum(ref reader);
                        break;
                    }
                case MqttPropertyId.MaximumQoS:
                    {
                        this.MaximumQoS = propertiesReader.ReadMaximumQoS(ref reader);
                        break;
                    }
                case MqttPropertyId.RetainAvailable:
                    {
                        this.RetainAvailable = propertiesReader.ReadRetainAvailable(ref reader);
                        break;
                    }
                case MqttPropertyId.MaximumPacketSize:
                    {
                        this.MaximumPacketSize = propertiesReader.ReadMaximumPacketSize(ref reader);
                        break;
                    }
                case MqttPropertyId.AssignedClientIdentifier:
                    {
                        this.AssignedClientIdentifier = propertiesReader.ReadAssignedClientIdentifier(ref reader);
                        break;
                    }
                case MqttPropertyId.TopicAliasMaximum:
                    {
                        this.TopicAliasMaximum = propertiesReader.ReadTopicAliasMaximum(ref reader);
                        break;
                    }
                case MqttPropertyId.ReasonString:
                    {
                        this.ReasonString = propertiesReader.ReadReasonString(ref reader);
                        break;
                    }
                case MqttPropertyId.WildcardSubscriptionAvailable:
                    {
                        this.WildcardSubscriptionAvailable = propertiesReader.ReadWildcardSubscriptionAvailable(ref reader);
                        break;
                    }
                case MqttPropertyId.SubscriptionIdentifiersAvailable:
                    {
                        this.SubscriptionIdentifiersAvailable = propertiesReader.ReadSubscriptionIdentifiersAvailable(ref reader);
                        break;
                    }
                case MqttPropertyId.SharedSubscriptionAvailable:
                    {
                        this.SharedSubscriptionAvailable = propertiesReader.ReadSharedSubscriptionAvailable(ref reader);
                        break;
                    }

                case MqttPropertyId.ServerKeepAlive:
                    {
                        this.ServerKeepAlive = propertiesReader.ReadServerKeepAlive(ref reader);
                        break;
                    }
                case MqttPropertyId.ResponseInformation:
                    {
                        this.ResponseInformation = propertiesReader.ReadResponseInformation(ref reader);
                        break;
                    }
                case MqttPropertyId.ServerReference:
                    {
                        this.ServerReference = propertiesReader.ReadServerReference(ref reader);
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
    }
}