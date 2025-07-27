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

/// <summary>
/// 表示一个Mqtt订阅消息。
/// </summary>
public sealed partial class MqttSubscribeMessage : MqttIdentifierMessage
{
    private readonly List<SubscribeRequest> m_subscribeRequests = new List<SubscribeRequest>();

    /// <summary>
    /// 初始化 <see cref="MqttSubscribeMessage"/> 类的新实例。
    /// </summary>
    /// <param name="subscribes">订阅请求的数组。</param>
    public MqttSubscribeMessage(params SubscribeRequest[] subscribes)
    {
        this.m_subscribeRequests.AddRange(subscribes);
        this.SetFixedFlagsWith0010();
    }

    internal MqttSubscribeMessage()
    {
    }

    /// <inheritdoc/>
    public override MqttMessageType MessageType => MqttMessageType.Subscribe;

    /// <summary>
    /// 获取订阅请求的只读列表。
    /// </summary>
    public IReadOnlyList<SubscribeRequest> SubscribeRequests => this.m_subscribeRequests;

    /// <inheritdoc/>
    protected override void BuildVariableBodyWithMqtt3<TWriter>(ref TWriter writer)
    {
        WriterExtension.WriteValue<TWriter,ushort>(ref writer,this.MessageId, EndianType.Big);
        foreach (var item in this.m_subscribeRequests)
        {
            MqttExtension.WriteMqttInt16String(ref writer, item.Topic);
            WriterExtension.WriteValue<TWriter,byte>(ref writer,(byte)item.QosLevel);
        }
    }

    /// <inheritdoc/>
    protected override int GetMinimumRemainingLength()
    {
        return 0;
    }

    /// <inheritdoc/>
    protected override void SetFlags(byte value)
    {
        base.SetFlags(value);

        this.ThrowIfFlagsNot0010();
    }

    /// <inheritdoc/>
    protected override void UnpackWithMqtt3<TReader>(ref TReader reader)
    {
        this.MessageId = ReaderExtension.ReadValue<TReader,ushort>(ref reader,EndianType.Big);

        while (!this.EndOfByteBlock(reader))
        {
            var topic = MqttExtension.ReadMqttInt16String(ref reader);
            var options = ReaderExtension.ReadValue<TReader,byte>(ref reader);
            this.m_subscribeRequests.Add(new SubscribeRequest(topic, options));
        }
    }
}