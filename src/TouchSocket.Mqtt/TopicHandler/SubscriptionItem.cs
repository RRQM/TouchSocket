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

namespace TouchSocket.Mqtt;

/// <summary>
/// 表示一个主题订阅项，包含主题过滤器、服务质量等级和消息处理器。
/// </summary>
/// <typeparam name="TClient">客户端类型，需实现 <see cref="IMqttClient"/>。</typeparam>
sealed class SubscriptionItem<TClient> where TClient : class, IMqttClient
{
    /// <summary>
    /// 初始化 <see cref="SubscriptionItem{TClient}"/> 类的新实例。
    /// </summary>
    /// <param name="topic">订阅主题过滤器。</param>
    /// <param name="qosLevel">服务质量等级。</param>
    /// <param name="handler">消息处理器。</param>
    public SubscriptionItem(string topic, QosLevel qosLevel, Func<TClient, MqttArrivedMessage, Task> handler)
    {
        this.Topic = topic;
        this.QosLevel = qosLevel;
        this.Handler = handler;
    }

    /// <summary>
    /// 获取订阅主题过滤器。
    /// </summary>
    public string Topic { get; }

    /// <summary>
    /// 获取服务质量等级。
    /// </summary>
    public QosLevel QosLevel { get; }

    /// <summary>
    /// 获取消息处理器。
    /// </summary>
    public Func<TClient, MqttArrivedMessage, Task> Handler { get; }

    /// <summary>
    /// 判断指定的发布主题是否与当前订阅过滤器匹配。
    /// </summary>
    /// <param name="topicName">实际发布的主题名称。</param>
    /// <returns>若匹配则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
    public bool IsMatch(string topicName) => MqttExtension.IsTopicMatch(topicName, this.Topic);
}
