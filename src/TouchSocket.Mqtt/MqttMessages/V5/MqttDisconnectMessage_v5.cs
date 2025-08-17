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
/// 表示Mqtt断开连接消息。
/// </summary>
public partial class MqttDisconnectMessage
{
    /// <summary>
    /// 获取或设置断开连接的原因代码。
    /// </summary>
    public MqttReasonCode ReasonCode { get; set; }

    /// <summary>
    /// 获取或设置断开连接的原因字符串。
    /// </summary>
    public string ReasonString { get; set; }

    /// <summary>
    /// 获取或设置服务器引用。
    /// </summary>
    public string ServerReference { get; set; }

    /// <summary>
    /// 获取或设置会话到期时间间隔。
    /// </summary>
    public uint SessionExpiryInterval { get; set; }

    /// <inheritdoc/>
    protected override void BuildVariableBodyWithMqtt5<TWriter>(ref TWriter writer)
    {
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)this.ReasonCode);

        var variableByteIntegerRecorder = new VariableByteIntegerRecorder();

        var byteBlockWriter = this.CreateVariableWriter(ref writer);
        variableByteIntegerRecorder.CheckOut(ref byteBlockWriter);

        MqttExtension.WriteServerReference(ref byteBlockWriter, this.ServerReference);
        MqttExtension.WriteReasonString(ref byteBlockWriter, this.ReasonString);
        MqttExtension.WriteSessionExpiryInterval(ref byteBlockWriter, this.SessionExpiryInterval);
        MqttExtension.WriteUserProperties(ref byteBlockWriter, this.UserProperties);

        variableByteIntegerRecorder.CheckIn(ref byteBlockWriter);
        writer.Advance(byteBlockWriter.Position);
    }

    /// <inheritdoc/>
    protected override void UnpackWithMqtt5<TReader>(ref TReader reader)
    {
        this.ReasonCode = (MqttReasonCode)ReaderExtension.ReadValue<TReader, byte>(ref reader);

        var propertiesReader = new MqttV5PropertiesReader<TReader>(ref reader);

        while (propertiesReader.TryRead(ref reader, out var mqttPropertyId))
        {
            switch (mqttPropertyId)
            {
                case MqttPropertyId.ServerReference:
                    {
                        this.ServerReference = propertiesReader.ReadServerReference(ref reader);
                        break;
                    }

                case MqttPropertyId.ReasonString:
                    {
                        this.ReasonString = propertiesReader.ReadReasonString(ref reader);
                        break;
                    }

                case MqttPropertyId.SessionExpiryInterval:
                    {
                        this.SessionExpiryInterval = propertiesReader.ReadSessionExpiryInterval(ref reader);
                        break;
                    }

                case MqttPropertyId.UserProperty:
                    {
                        this.AddUserProperty(propertiesReader.ReadUserProperty(ref reader));
                        break;
                    }

                default:
                    break;
            }
        }

        //this.ServerReference = propertiesReader.ServerReference;
        //this.ReasonString = propertiesReader.ReasonString;
        //this.SessionExpiryInterval = propertiesReader.SessionExpiryInterval;
        //this.UserProperties = propertiesReader.UserProperties;
    }
}