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

/// <summary>
/// 表示MQTT断开连接消息。
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
    protected override void BuildVariableBodyWithMqtt5<TByteBlock>(ref TByteBlock byteBlock)
    {
        byteBlock.WriteByte((byte)this.ReasonCode);

        var variableByteIntegerRecorder = new VariableByteIntegerRecorder();
        variableByteIntegerRecorder.CheckOut(ref byteBlock);

        MqttExtension.WriteServerReference(ref byteBlock, this.ServerReference);
        MqttExtension.WriteReasonString(ref byteBlock, this.ReasonString);
        MqttExtension.WriteSessionExpiryInterval(ref byteBlock, this.SessionExpiryInterval);
        MqttExtension.WriteUserProperties(ref byteBlock, this.UserProperties);

        variableByteIntegerRecorder.CheckIn(ref byteBlock);
    }

    /// <inheritdoc/>
    protected override void UnpackWithMqtt5<TByteBlock>(ref TByteBlock byteBlock)
    {
        this.ReasonCode = (MqttReasonCode)byteBlock.ReadByte();

        var propertiesReader = new MqttV5PropertiesReader<TByteBlock>(ref byteBlock);

        while (propertiesReader.TryRead(ref byteBlock, out var mqttPropertyId))
        {
            switch (mqttPropertyId)
            {
                case MqttPropertyId.ServerReference:
                    {
                        this.ServerReference = propertiesReader.ReadServerReference(ref byteBlock);
                        break;
                    }

                case MqttPropertyId.ReasonString:
                    {
                        this.ReasonString = propertiesReader.ReadReasonString(ref byteBlock);
                        break;
                    }

                case MqttPropertyId.SessionExpiryInterval:
                    {
                        this.SessionExpiryInterval = propertiesReader.ReadSessionExpiryInterval(ref byteBlock);
                        break;
                    }

                case MqttPropertyId.UserProperty:
                    {
                        this.AddUserProperty(propertiesReader.ReadUserProperty(ref byteBlock));
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