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
/// 表示 <see cref="MqttBroker"/> 的配置选项。
/// </summary>
public sealed class MqttBrokerOption
{
    private int m_messageCapacity = 1000;

    /// <summary>
    /// 获取或设置消息队列的最大容量。达到上限时，新消息将被丢弃。默认值为 1000。
    /// </summary>
    public int MessageCapacity
    {
        get => this.m_messageCapacity;
        set => this.m_messageCapacity = value > 0 ? value : 1000;
    }

    /// <summary>
    /// 获取或设置消息的过期时间。从服务端接收到消息起计算，超过此时间的消息将被丢弃。
    /// 设置为 <see cref="TimeSpan.Zero"/> 时表示永不过期。
    /// </summary>
    public TimeSpan MessageExpiry { get; set; }

    /// <summary>
    /// 获取或设置离线会话的过期时长。超过该时长的离线会话将自动从代理中移除。
    /// 设置为 <see cref="TimeSpan.Zero"/> 时禁用自动清除。
    /// </summary>
    public TimeSpan SessionExpiry { get; set; }
}