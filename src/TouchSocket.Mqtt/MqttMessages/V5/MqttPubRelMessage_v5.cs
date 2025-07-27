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

public partial class MqttPubRelMessage
{
    public MqttReasonCode ReasonCode { get; set; }

    public string ReasonString { get; set; }

    /// <inheritdoc/>
    protected override void BuildVariableBodyWithMqtt5<TWriter>(ref TWriter writer)
    {
        WriterExtension.WriteValue<TWriter,ushort>(ref writer,this.MessageId, EndianType.Big);

        if (this.ReasonCode != MqttReasonCode.Success || this.ReasonString.HasValue() || this.UserProperties.Count > 0)
        {
            WriterExtension.WriteValue<TWriter,byte>(ref writer,(byte)this.ReasonCode);

            var variableByteIntegerRecorder = new VariableByteIntegerRecorder();
            var byteBlockWriter = this.CreateVariableWriter(ref writer);
            variableByteIntegerRecorder.CheckOut(ref byteBlockWriter);
            MqttExtension.WriteReasonString(ref byteBlockWriter, this.ReasonString);
            MqttExtension.WriteUserProperties(ref byteBlockWriter, this.UserProperties);
            variableByteIntegerRecorder.CheckIn(ref byteBlockWriter);
            writer.Advance(byteBlockWriter.Position);
        }
    }

    /// <inheritdoc/>
    protected override void UnpackWithMqtt5<TReader>(ref TReader reader)
    {
        this.MessageId = ReaderExtension.ReadValue<TReader,ushort>(ref reader,EndianType.Big);

        if (this.EndOfByteBlock(reader))
        {
            return;
        }

        this.ReasonCode = (MqttReasonCode)ReaderExtension.ReadValue<TReader,byte>(ref reader);

        if (this.EndOfByteBlock(reader))
        {
            return;
        }

        var propertiesReader = new Mqtt.MqttV5PropertiesReader<TReader>(ref reader);

        while (propertiesReader.TryRead(ref reader, out var mqttPropertyId))
        {
            switch (mqttPropertyId)
            {
                case MqttPropertyId.ReasonString:
                    this.ReasonString = propertiesReader.ReadReasonString(ref reader);
                    break;
                case MqttPropertyId.UserProperty:
                    this.AddUserProperty(propertiesReader.ReadUserProperty(ref reader));
                    break;
                default:
                    ThrowHelper.ThrowInvalidEnumArgumentException(mqttPropertyId);
                    break;
            }
        }

        //this.ReasonString = propertiesReader.ReasonString;
        //this.UserProperties = propertiesReader.UserProperties;
    }
}