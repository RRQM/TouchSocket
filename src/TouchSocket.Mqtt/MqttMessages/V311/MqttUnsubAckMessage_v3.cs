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
/// 表示 MQTT 协议的取消订阅确认消息。
/// </summary>
public sealed partial class MqttUnsubAckMessage : MqttIdentifierMessage
{
    /// <inheritdoc/>
    public override MqttMessageType MessageType => MqttMessageType.UnsubAck;

    /// <inheritdoc/>
    protected override void BuildVariableBodyWithMqtt3<TByteBlock>(ref TByteBlock byteBlock)
    {
        byteBlock.WriteUInt16(this.MessageId, EndianType.Big);
    }

    /// <inheritdoc/>
    protected override int GetMinimumRemainingLength()
    {
        return 0;
    }

    /// <inheritdoc/>
    protected override void UnpackWithMqtt3<TByteBlock>(ref TByteBlock byteBlock)
    {
        this.MessageId = byteBlock.ReadUInt16(EndianType.Big);
    }
}