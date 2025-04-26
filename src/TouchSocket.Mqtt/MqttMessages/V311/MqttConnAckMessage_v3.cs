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
/// 表示 MQTT 连接确认消息。
/// </summary>
public sealed partial class MqttConnAckMessage : MqttUserPropertiesMessage
{
    private byte m_connectAcknowledgeFlags;

    /// <summary>
    /// 初始化 <see cref="MqttConnAckMessage"/> 类的新实例。
    /// </summary>
    public MqttConnAckMessage()
    {
    }

    /// <summary>
    /// 使用指定的连接确认标志和返回代码初始化 <see cref="MqttConnAckMessage"/> 类的新实例。
    /// </summary>
    /// <param name="m_connectAcknowledgeFlags">连接确认标志。</param>
    public MqttConnAckMessage(byte m_connectAcknowledgeFlags)
    {
        this.m_connectAcknowledgeFlags = m_connectAcknowledgeFlags;
    }

    /// <summary>
    /// 使用指定的会话存在标志和返回代码初始化 <see cref="MqttConnAckMessage"/> 类的新实例。
    /// </summary>
    /// <param name="sessionPresent">会话存在标志。</param>
    public MqttConnAckMessage(bool sessionPresent)
    {
        this.m_connectAcknowledgeFlags = this.m_connectAcknowledgeFlags.SetBit(0, sessionPresent);
    }

    /// <summary>
    /// 获取消息类型。
    /// </summary>
    public override MqttMessageType MessageType => MqttMessageType.ConnAck;

    /// <summary>
    /// 获取返回代码。
    /// </summary>
    public MqttReasonCode ReturnCode { get; set; }

    /// <summary>
    /// 获取会话存在标志。
    /// </summary>
    public bool SessionPresent => this.m_connectAcknowledgeFlags.GetBit(0);

    /// <inheritdoc/>
    protected override void BuildVariableBodyWithMqtt3<TByteBlock>(ref TByteBlock byteBlock)
    {
        byteBlock.WriteByte(this.m_connectAcknowledgeFlags);
        byteBlock.WriteByte((byte)this.ReturnCode);
    }

    /// <inheritdoc/>
    protected override int GetMinimumRemainingLength()
    {
        return 0;
    }

    /// <inheritdoc/>
    protected override void UnpackWithMqtt3<TByteBlock>(ref TByteBlock byteBlock)
    {
        this.m_connectAcknowledgeFlags = byteBlock.ReadByte();
        this.ReturnCode = (MqttReasonCode)byteBlock.ReadByte();
    }
}