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

namespace TouchSocket.Mqtt;

/// <summary>
/// 表示一个到达的 Mqtt 消息。
/// </summary>
public sealed class MqttArrivedMessage
{
    /// <summary>
    /// 初始化 <see cref="MqttArrivedMessage"/> 类的新实例。
    /// </summary>
    /// <param name="message">Mqtt 发布消息。</param>
    public MqttArrivedMessage(MqttPublishMessage message)
    {
        this.QosLevel = message.QosLevel;
        this.Retain = message.Retain;
        this.TopicName = message.TopicName;
        this.Payload = new ReadOnlyMemory<byte>(message.Payload.ToArray());
    }

    /// <summary>
    /// 获取消息的服务质量级别。
    /// </summary>
    public QosLevel QosLevel { get; }

    /// <summary>
    /// 获取消息的有效负载。
    /// </summary>
    public ReadOnlyMemory<byte> Payload { get; }

    /// <summary>
    /// 获取一个值，该值指示消息是否被保留。
    /// </summary>
    public bool Retain { get; }

    /// <summary>
    /// 获取消息的主题名称。
    /// </summary>
    public string TopicName { get; }
}