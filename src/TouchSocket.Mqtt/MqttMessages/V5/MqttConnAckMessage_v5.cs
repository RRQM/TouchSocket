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

public partial class MqttConnAckMessage
{
    /// <summary>
    /// 获取或设置分配的客户端标识符。
    /// </summary>
    public string AssignedClientIdentifier { get; set; }

    /// <summary>
    /// 获取或设置认证数据。
    /// </summary>
    public byte[] AuthenticationData { get; set; }

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
    protected override void BuildVariableBodyWithMqtt5<TByteBlock>(ref TByteBlock byteBlock)
    {
        byteBlock.WriteByte(this.m_connectAcknowledgeFlags);
        byteBlock.WriteByte((byte)this.ReturnCode);

        var variableByteIntegerRecorder = new VariableByteIntegerRecorder();
        variableByteIntegerRecorder.CheckOut(ref byteBlock);

        MqttExtension.WriteSessionExpiryInterval(ref byteBlock, this.SessionExpiryInterval);
        MqttExtension.WriteReceiveMaximum(ref byteBlock, this.ReceiveMaximum);
        MqttExtension.WriteMaximumQoS(ref byteBlock, this.MaximumQoS);
        MqttExtension.WriteRetainAvailable(ref byteBlock, this.RetainAvailable);
        MqttExtension.WriteMaximumPacketSize(ref byteBlock, this.MaximumPacketSize);
        MqttExtension.WriteAssignedClientIdentifier(ref byteBlock, this.AssignedClientIdentifier);
        MqttExtension.WriteTopicAliasMaximum(ref byteBlock, this.TopicAliasMaximum);
        MqttExtension.WriteReasonString(ref byteBlock, this.ReasonString);
        MqttExtension.WriteWildcardSubscriptionAvailable(ref byteBlock, this.WildcardSubscriptionAvailable);
        MqttExtension.WriteSubscriptionIdentifiersAvailable(ref byteBlock, this.SubscriptionIdentifiersAvailable);
        MqttExtension.WriteSharedSubscriptionAvailable(ref byteBlock, this.SharedSubscriptionAvailable);
        MqttExtension.WriteServerKeepAlive(ref byteBlock, this.ServerKeepAlive);
        MqttExtension.WriteResponseInformation(ref byteBlock, this.ResponseInformation);
        MqttExtension.WriteServerReference(ref byteBlock, this.ServerReference);
        MqttExtension.WriteAuthenticationMethod(ref byteBlock, this.AuthenticationMethod);
        MqttExtension.WriteAuthenticationData(ref byteBlock, this.AuthenticationData);
        MqttExtension.WriteUserProperties(ref byteBlock, this.UserProperties);

        variableByteIntegerRecorder.CheckIn(ref byteBlock);
    }

    /// <inheritdoc/>
    protected override void UnpackWithMqtt5<TByteBlock>(ref TByteBlock byteBlock)
    {
        this.m_connectAcknowledgeFlags = byteBlock.ReadByte();
        this.ReturnCode = (MqttReasonCode)byteBlock.ReadByte();

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
                case MqttPropertyId.ReceiveMaximum:
                    {
                        this.ReceiveMaximum = propertiesReader.ReadReceiveMaximum(ref byteBlock);
                        break;
                    }
                case MqttPropertyId.MaximumQoS:
                    {
                        this.MaximumQoS = propertiesReader.ReadMaximumQoS(ref byteBlock);
                        break;
                    }
                case MqttPropertyId.RetainAvailable:
                    {
                        this.RetainAvailable = propertiesReader.ReadRetainAvailable(ref byteBlock);
                        break;
                    }
                case MqttPropertyId.MaximumPacketSize:
                    {
                        this.MaximumPacketSize = propertiesReader.ReadMaximumPacketSize(ref byteBlock);
                        break;
                    }
                case MqttPropertyId.AssignedClientIdentifier:
                    {
                        this.AssignedClientIdentifier = propertiesReader.ReadAssignedClientIdentifier(ref byteBlock);
                        break;
                    }
                case MqttPropertyId.TopicAliasMaximum:
                    {
                        this.TopicAliasMaximum = propertiesReader.ReadTopicAliasMaximum(ref byteBlock);
                        break;
                    }
                case MqttPropertyId.ReasonString:
                    {
                        this.ReasonString = propertiesReader.ReadReasonString(ref byteBlock);
                        break;
                    }
                case MqttPropertyId.WildcardSubscriptionAvailable:
                    {
                        this.WildcardSubscriptionAvailable = propertiesReader.ReadWildcardSubscriptionAvailable(ref byteBlock);
                        break;
                    }
                case MqttPropertyId.SubscriptionIdentifiersAvailable:
                    {
                        this.SubscriptionIdentifiersAvailable = propertiesReader.ReadSubscriptionIdentifiersAvailable(ref byteBlock);
                        break;
                    }
                case MqttPropertyId.SharedSubscriptionAvailable:
                    {
                        this.SharedSubscriptionAvailable = propertiesReader.ReadSharedSubscriptionAvailable(ref byteBlock);
                        break;
                    }

                case MqttPropertyId.ServerKeepAlive:
                    {
                        this.ServerKeepAlive = propertiesReader.ReadServerKeepAlive(ref byteBlock);
                        break;
                    }
                case MqttPropertyId.ResponseInformation:
                    {
                        this.ResponseInformation = propertiesReader.ReadResponseInformation(ref byteBlock);
                        break;
                    }
                case MqttPropertyId.ServerReference:
                    {
                        this.ServerReference = propertiesReader.ReadServerReference(ref byteBlock);
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
    }
}