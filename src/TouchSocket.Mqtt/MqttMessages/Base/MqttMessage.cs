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
/// 表示一个Mqtt消息的抽象基类。
/// </summary>
public abstract class MqttMessage : IRequestInfo, IRequestInfoBuilder
{
    private long m_endPosition;

    /// <summary>
    /// 获取消息的最大长度。
    /// </summary>
    public virtual int MaxLength => 1024 * 64;

    /// <summary>
    /// 获取消息类型。
    /// </summary>
    public abstract MqttMessageType MessageType { get; }

    /// <summary>
    /// 获取或设置协议版本号。
    /// </summary>
    public MqttProtocolVersion Version { get; protected set; }

    /// <summary>
    /// 获取结束位置。
    /// </summary>
    protected long EndPosition => this.m_endPosition;

    /// <summary>
    /// 获取或设置标志位。
    /// </summary>
    protected byte Flags { get; private set; }

    /// <summary>
    /// 获取或设置可变数据的剩余长度。
    /// </summary>
    protected uint RemainingLength { get; set; }

    /// <summary>
    /// 创建Mqtt消息实例。
    /// </summary>
    /// <param name="mqttDataType">Mqtt数据类型。</param>
    /// <returns>Mqtt消息实例。</returns>
    public static MqttMessage CreateMqttMessage(MqttMessageType mqttDataType)
    {
        //Console.WriteLine(mqttDataType);
        return mqttDataType switch
        {
            MqttMessageType.Connect => new MqttConnectMessage(),
            MqttMessageType.ConnAck => new MqttConnAckMessage(),
            MqttMessageType.Publish => new MqttPublishMessage(),
            MqttMessageType.PubAck => new MqttPubAckMessage(),
            MqttMessageType.PubRec => new MqttPubRecMessage(),
            MqttMessageType.PubRel => new MqttPubRelMessage(),
            MqttMessageType.PubComp => new MqttPubCompMessage(),
            MqttMessageType.Subscribe => new MqttSubscribeMessage(),
            MqttMessageType.SubAck => new MqttSubAckMessage(),
            MqttMessageType.Unsubscribe => new MqttUnsubscribeMessage(),
            MqttMessageType.UnsubAck => new MqttUnsubAckMessage(),
            MqttMessageType.PingReq => new MqttPingReqMessage(),
            MqttMessageType.PingResp => new MqttPingRespMessage(),
            MqttMessageType.Disconnect => new MqttDisconnectMessage(),
            //_ => new MqttMessage()
            _ => throw ThrowHelper.CreateInvalidEnumArgumentException(mqttDataType)
        };
    }

    protected BytesWriter CreateVariableWriter<TWriter>(ref TWriter writer)
        where TWriter : IBytesWriter
    {
        return new BytesWriter(writer.GetMemory(this.GetMinimumRemainingLength() + 1024));
    }

    #region Build

    /// <summary>
    /// 构建Mqtt消息。
    /// </summary>
    /// <typeparam name="TWriter">字节块类型。</typeparam>
    /// <param name="writer">字节块引用。</param>
    public void Build<TWriter>(ref TWriter writer)
        where TWriter : IBytesWriter

    {
        MqttExtension.WriteMqttFixedHeader(ref writer, this.MessageType, this.Flags);
        var variableByteIntegerRecorder = new VariableByteIntegerRecorder();

        var byteBlockWriter = this.CreateVariableWriter(ref writer);
        variableByteIntegerRecorder.CheckOut(ref byteBlockWriter, this.GetMinimumRemainingLength());

        this.BuildVariableBody(ref byteBlockWriter);
        this.RemainingLength = (uint)variableByteIntegerRecorder.CheckIn(ref byteBlockWriter);

        writer.Advance(byteBlockWriter.Position);
    }

    protected abstract void BuildVariableBodyWithMqtt3<TWriter>(ref TWriter writer)
        where TWriter : IBytesWriter

        ;

    protected abstract void BuildVariableBodyWithMqtt5<TWriter>(ref TWriter writer)
        where TWriter : IBytesWriter

        ;

    /// <summary>
    /// 获取最小剩余长度。
    /// </summary>
    /// <returns>剩余长度。</returns>
    protected abstract int GetMinimumRemainingLength();

    private void BuildVariableBody<TWriter>(ref TWriter writer)
        where TWriter : IBytesWriter

    {
        switch (this.Version)
        {
            case MqttProtocolVersion.V310:
            case MqttProtocolVersion.V311:
                this.BuildVariableBodyWithMqtt3(ref writer);
                break;

            case MqttProtocolVersion.V500:
                this.BuildVariableBodyWithMqtt5(ref writer);
                break;

            case MqttProtocolVersion.Unknown:
            default:
                ThrowHelper.ThrowInvalidEnumArgumentException(this.Version);
                return;
        }
    }

    #endregion Build

    #region Unpack

    /// <summary>
    /// 解包Mqtt消息。
    /// </summary>
    /// <typeparam name="TReader">字节块类型。</typeparam>
    /// <param name="reader">字节块引用。</param>
    public virtual void Unpack<TReader>(ref TReader reader)
        where TReader : IBytesReader
    {
        var firstByte = ReaderExtension.ReadValue<TReader, byte>(ref reader);
        this.SetFlags((byte)firstByte.GetLow4());
        this.RemainingLength = MqttExtension.ReadVariableByteInteger(ref reader);
        this.m_endPosition = (reader.BytesRead + this.RemainingLength);
        switch (this.Version)
        {
            case MqttProtocolVersion.V310:
            case MqttProtocolVersion.V311:
                this.UnpackWithMqtt3(ref reader);
                break;

            case MqttProtocolVersion.V500:
                this.UnpackWithMqtt5(ref reader);
                break;

            case MqttProtocolVersion.Unknown:
            default:
                ThrowHelper.ThrowInvalidEnumArgumentException(this.Version);
                return;
        }
    }

    protected bool EndOfByteBlock<TReader>(in TReader reader)
        where TReader : IBytesReader
    {
        return this.m_endPosition == reader.BytesRead;
    }

    protected abstract void UnpackWithMqtt3<TReader>(ref TReader reader)
        where TReader : IBytesReader;

    protected abstract void UnpackWithMqtt5<TReader>(ref TReader reader)
        where TReader : IBytesReader;

    #endregion Unpack

    internal void InternalSetVersion(MqttProtocolVersion version)
    {
        this.Version = version;
    }

    /// <summary>
    /// 设置标志位为0010。
    /// </summary>
    protected void SetFixedFlagsWith0010()
    {
        this.SetFlags(2);
    }

    /// <summary>
    /// 设置标志位。
    /// </summary>
    /// <param name="value">标志位值。</param>
    protected virtual void SetFlags(byte value)
    {
        this.Flags = value;
    }

    #region Throw

    /// <summary>
    /// 如果标志位不为0010则抛出异常。
    /// </summary>
    protected void ThrowIfFlagsNot0010()
    {
        if (this.Flags != 2)
        {
            ThrowHelper.ThrowException($"{this.MessageType}消息的固定头的flags不合法");
        }
    }

    #endregion Throw
}