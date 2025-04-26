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

public partial class MqttSubscribeMessage
{
    public uint SubscriptionIdentifier { get; set; }

    /// <inheritdoc/>
    protected override void BuildVariableBodyWithMqtt5<TByteBlock>(ref TByteBlock byteBlock)
    {
        byteBlock.WriteUInt16(this.MessageId, EndianType.Big);

        var variableByteIntegerRecorder = new VariableByteIntegerRecorder();
        variableByteIntegerRecorder.CheckOut(ref byteBlock);
        MqttExtension.WriteSubscriptionIdentifier(ref byteBlock, this.SubscriptionIdentifier);
        MqttExtension.WriteUserProperties(ref byteBlock, this.UserProperties);
        variableByteIntegerRecorder.CheckIn(ref byteBlock);

        foreach (var item in this.m_subscribeRequests)
        {
            MqttExtension.WriteMqttInt16String(ref byteBlock, item.Topic);
            byteBlock.WriteByte((byte)item.QosLevel);
        }
    }

    /// <inheritdoc/>
    protected override void UnpackWithMqtt5<TByteBlock>(ref TByteBlock byteBlock)
    {
        this.MessageId = byteBlock.ReadUInt16(EndianType.Big);
        var propertiesReader = new MqttV5PropertiesReader<TByteBlock>(ref byteBlock);

        while (propertiesReader.TryRead(ref byteBlock, out var mqttPropertyId))
        {
            switch (mqttPropertyId)
            {
                case MqttPropertyId.SubscriptionIdentifier:
                    this.SubscriptionIdentifier = propertiesReader.ReadSubscriptionIdentifier(ref byteBlock);
                    break;
                case MqttPropertyId.UserProperty:
                    this.AddUserProperty(propertiesReader.ReadUserProperty(ref byteBlock));
                    break;
                default:
                    ThrowHelper.ThrowInvalidEnumArgumentException(mqttPropertyId);
                    break;
            }
        }
        //this.SubscriptionIdentifier = propertiesReader.SubscriptionIdentifier;
        //this.UserProperties = propertiesReader.UserProperties;
        while (!this.EndOfByteBlock(byteBlock))
        {
            var topic = MqttExtension.ReadMqttInt16String(ref byteBlock);
            var options = byteBlock.ReadByte();
            this.m_subscribeRequests.Add(new SubscribeRequest(topic, options));
        }
    }
}