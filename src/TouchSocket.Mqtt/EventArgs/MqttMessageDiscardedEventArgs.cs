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
/// 表示 Mqtt 消息被丢弃时的事件参数。
/// </summary>
public class MqttMessageDiscardedEventArgs : PluginEventArgs
{
    /// <summary>
    /// 初始化 <see cref="MqttMessageDiscardedEventArgs"/> 类的新实例。
    /// </summary>
    /// <param name="clientId">客户端 ID。</param>
    /// <param name="message">被丢弃的消息。</param>
    /// <param name="reason">丢弃原因。</param>
    public MqttMessageDiscardedEventArgs(string clientId, MqttArrivedMessage message, DiscardReason reason)
    {
        this.ClientId = clientId;
        this.Message = message;
        this.Reason = reason;
    }

    /// <summary>
    /// 获取客户端 ID。
    /// </summary>
    public string ClientId { get; }

    /// <summary>
    /// 获取被丢弃的消息。
    /// </summary>
    public MqttArrivedMessage Message { get; }

    /// <summary>
    /// 获取消息被丢弃的原因。
    /// </summary>
    public DiscardReason Reason { get; }
}