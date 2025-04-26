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
/// 表示一个MQTT消息的抽象基类。
/// </summary>
public abstract class MqttMessage : IRequestInfo, IRequestInfoBuilder
{
    private int m_endPosition;

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
    protected int EndPosition => this.m_endPosition;

    /// <summary>
    /// 获取或设置标志位。
    /// </summary>
    protected byte Flags { get; private set; }

    /// <summary>
    /// 获取或设置可变数据的剩余长度。
    /// </summary>
    protected uint RemainingLength { get; set; }

    /// <summary>
    /// 创建MQTT消息实例。
    /// </summary>
    /// <param name="mqttDataType">MQTT数据类型。</param>
    /// <returns>MQTT消息实例。</returns>
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

    #region Build

    /// <summary>
    /// 构建MQTT消息。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块类型。</typeparam>
    /// <param name="byteBlock">字节块引用。</param>
    public void Build<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        MqttExtension.WriteMqttFixedHeader(ref byteBlock, this.MessageType, this.Flags);
        var variableByteIntegerRecorder = new VariableByteIntegerRecorder();
        variableByteIntegerRecorder.CheckOut(ref byteBlock, this.GetMinimumRemainingLength());

        this.BuildVariableBody(ref byteBlock);
        this.RemainingLength = (uint)variableByteIntegerRecorder.CheckIn(ref byteBlock);
    }

    protected abstract void BuildVariableBodyWithMqtt3<TByteBlock>(ref TByteBlock byteBlock)
        where TByteBlock : IByteBlock;

    protected abstract void BuildVariableBodyWithMqtt5<TByteBlock>(ref TByteBlock byteBlock)
        where TByteBlock : IByteBlock;

    /// <summary>
    /// 获取最小剩余长度。
    /// </summary>
    /// <returns>剩余长度。</returns>
    protected abstract int GetMinimumRemainingLength();

    private void BuildVariableBody<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        switch (this.Version)
        {
            case MqttProtocolVersion.V310:
            case MqttProtocolVersion.V311:
                this.BuildVariableBodyWithMqtt3(ref byteBlock);
                break;

            case MqttProtocolVersion.V500:
                this.BuildVariableBodyWithMqtt5(ref byteBlock);
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
    /// 解包MQTT消息。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块类型。</typeparam>
    /// <param name="byteBlock">字节块引用。</param>
    public virtual void Unpack<TByteBlock>(ref TByteBlock byteBlock)
        where TByteBlock : IByteBlock
    {
        var firstByte = byteBlock.ReadByte();
        this.SetFlags((byte)firstByte.GetLow4());
        this.RemainingLength = MqttExtension.ReadVariableByteInteger(ref byteBlock);
        this.m_endPosition = (int)(byteBlock.Position + this.RemainingLength);
        switch (this.Version)
        {
            case MqttProtocolVersion.V310:
            case MqttProtocolVersion.V311:
                this.UnpackWithMqtt3(ref byteBlock);
                break;

            case MqttProtocolVersion.V500:
                this.UnpackWithMqtt5(ref byteBlock);
                break;

            case MqttProtocolVersion.Unknown:
            default:
                ThrowHelper.ThrowInvalidEnumArgumentException(this.Version);
                return;
        }
    }

    protected bool EndOfByteBlock<TByteBlock>(in TByteBlock byteBlock)
        where TByteBlock : IByteBlock
    {
        return this.m_endPosition == byteBlock.Position;
    }

    protected abstract void UnpackWithMqtt3<TByteBlock>(ref TByteBlock byteBlock)
        where TByteBlock : IByteBlock;

    protected abstract void UnpackWithMqtt5<TByteBlock>(ref TByteBlock byteBlock)
        where TByteBlock : IByteBlock;

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