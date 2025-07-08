// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using TouchSocket.Sockets;

namespace TouchSocket.Mqtt;
/// <summary>
/// 表示 Mqtt 关闭事件的参数。
/// </summary>
public class MqttClosingEventArgs : ClosingEventArgs
{
    private readonly MqttDisconnectMessage m_message;

    /// <summary>
    /// 初始化 <see cref="MqttClosingEventArgs"/> 类的新实例。
    /// </summary>
    /// <param name="msg">关闭消息。</param>
    public MqttClosingEventArgs(string msg) : base(msg)
    {
    }

    /// <summary>
    /// 初始化 <see cref="MqttClosingEventArgs"/> 类的新实例。
    /// </summary>
    /// <param name="message">Mqtt 断开连接消息。</param>
    public MqttClosingEventArgs(MqttDisconnectMessage message) : base(message.ReasonString)
    {
        this.m_message = message;
    }

    public MqttDisconnectMessage MqttMessage => this.m_message;
}
