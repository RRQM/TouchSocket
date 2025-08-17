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
using System.Collections.Generic;
using TouchSocket.Core;

namespace TouchSocket.Mqtt;

/// <summary>
/// 表示 Mqtt 订阅确认消息。
/// </summary>
public sealed partial class MqttSubAckMessage : MqttIdentifierMessage
{
    private readonly List<MqttReasonCode> m_returnCodes = new List<MqttReasonCode>();

    /// <inheritdoc/>
    public override MqttMessageType MessageType => MqttMessageType.SubAck;

    /// <summary>
    /// 获取返回码的只读列表。
    /// </summary>
    public IReadOnlyList<MqttReasonCode> ReturnCodes => this.m_returnCodes;

    /// <summary>
    /// 添加返回码。
    /// </summary>
    /// <param name="returnCode">返回码。</param>
    public void AddReturnCode(MqttReasonCode returnCode)
    {
        this.m_returnCodes.Add(returnCode);
    }

    /// <inheritdoc/>
    protected override int GetMinimumRemainingLength()
    {
        return 0;
    }

    /// <summary>
    /// 根据 QoS 级别添加返回码。
    /// </summary>
    /// <param name="qosLevel">QoS 级别。</param>
    public void AddReturnCode(QosLevel qosLevel)
    {
        switch (qosLevel)
        {
            case QosLevel.AtMostOnce:
                this.AddReturnCode(MqttReasonCode.GrantedQoS0);
                break;

            case QosLevel.AtLeastOnce:
                this.AddReturnCode(MqttReasonCode.GrantedQoS1);
                break;

            case QosLevel.ExactlyOnce:
                this.AddReturnCode(MqttReasonCode.GrantedQoS2);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(qosLevel), qosLevel, null);
        }
    }

    /// <inheritdoc/>
    protected override void BuildVariableBodyWithMqtt3<TWriter>(ref TWriter writer)
    {
        WriterExtension.WriteValue<TWriter, ushort>(ref writer, this.MessageId, EndianType.Big);
        foreach (var item in this.ReturnCodes)
        {
            WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)item);
        }
    }

    /// <inheritdoc/>
    protected override void UnpackWithMqtt3<TReader>(ref TReader reader)
    {
        this.MessageId = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);
        while (!this.EndOfByteBlock(reader))
        {
            this.m_returnCodes.Add((MqttReasonCode)ReaderExtension.ReadValue<TReader, byte>(ref reader));
        }
    }
}