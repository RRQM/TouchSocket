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

/// <summary>
/// 表示一个MQTT发布消息。
/// </summary>
public sealed partial class MqttPublishMessage : MqttIdentifierMessage
{
    /// <summary>
    /// 初始化 <see cref="MqttPublishMessage"/> 类的新实例。
    /// </summary>
    /// <param name="topicName">主题名称。</param>
    /// <param name="retain">是否保留消息。</param>
    /// <param name="qosLevel">服务质量级别。</param>
    /// <param name="payload">消息负载。</param>
    public MqttPublishMessage(string topicName, bool retain, QosLevel qosLevel, ReadOnlyMemory<byte> payload)
    {
        this.TopicName = topicName;
        this.Payload = payload;
        this.SetFlags(retain, qosLevel, false);
    }

    /// <summary>
    /// 初始化 <see cref="MqttPublishMessage"/> 类的新实例。
    /// </summary>
    public MqttPublishMessage()
    {
    }

    /// <summary>
    /// 初始化 <see cref="MqttPublishMessage"/> 类的新实例。
    /// </summary>
    /// <param name="topicName">主题名称。</param>
    /// <param name="payload">消息负载。</param>
    internal MqttPublishMessage(string topicName, ReadOnlyMemory<byte> payload)
    {
        this.TopicName = topicName;
        this.Payload = payload;
    }

    /// <summary>
    /// 获取一个值，该值指示是否为重复消息。
    /// </summary>
    public bool DUP => base.Flags.GetBit(3);

    /// <summary>
    /// 获取消息类型。
    /// </summary>
    public override MqttMessageType MessageType => MqttMessageType.Publish;

    /// <summary>
    /// 获取消息负载。
    /// </summary>
    public ReadOnlyMemory<byte> Payload { get; private set; }

    /// <summary>
    /// 获取服务质量级别。
    /// </summary>
    public QosLevel QosLevel => base.Flags.GetQosLevel(1);

    /// <summary>
    /// 获取一个值，该值指示是否保留消息。
    /// </summary>
    public bool Retain => base.Flags.GetBit(0);

    /// <summary>
    /// 获取主题名称。
    /// </summary>
    public string TopicName { get; private set; }

    /// <inheritdoc/>
    protected override void UnpackWithMqtt3<TByteBlock>(ref TByteBlock byteBlock)
    {
        this.TopicName = MqttExtension.ReadMqttInt16String(ref byteBlock);
        if (this.QosLevel != QosLevel.AtMostOnce)
        {
            this.MessageId = byteBlock.ReadUInt16(EndianType.Big);
        }

        this.Payload = this.ReadPayload(ref byteBlock);
    }

    private ReadOnlyMemory<byte> ReadPayload<TByteBlock>(ref TByteBlock byteBlock)
        where TByteBlock : IByteBlock
    {
        var payloadLength = this.EndPosition - byteBlock.Position;
        var payload = byteBlock.Memory.Slice(byteBlock.Position, payloadLength);
        byteBlock.Position = this.EndPosition;
        return payload;
    }


    internal void SetFlags(bool retain, QosLevel qosLevel, bool dup)
    {
        byte flags = 0;
        flags = flags.SetBit(0, retain);
        flags = flags.SetQosLevel(1, qosLevel);
        flags = flags.SetBit(3, dup);
        base.SetFlags(flags);
    }

    /// <inheritdoc/>
    protected override void BuildVariableBodyWithMqtt3<TByteBlock>(ref TByteBlock byteBlock)
    {
        MqttExtension.WriteMqttInt16String(ref byteBlock, this.TopicName);
        if (this.QosLevel != QosLevel.AtMostOnce)
        {
            byteBlock.WriteUInt16(this.MessageId, EndianType.Big);
        }
        byteBlock.Write(this.Payload.Span);
    }

    /// <inheritdoc/>
    protected override int GetMinimumRemainingLength()
    {
        return 0;
    }
}