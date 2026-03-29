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
/// 表示主题订阅管理配置选项，用于配置主题过滤器与消息处理器的映射关系。
/// </summary>
/// <typeparam name="TClient">客户端类型，需实现 <see cref="IMqttClient"/>。</typeparam>
public sealed class SubscriptionManagementOption<TClient> where TClient : class, IMqttClient
{
    private readonly List<SubscriptionItem<TClient>> m_subscriptionItems = new();

    internal IReadOnlyList<SubscriptionItem<TClient>> SubscriptionItems => this.m_subscriptionItems;

    /// <summary>
    /// 添加主题订阅及其处理器，使用默认的 <see cref="QosLevel.AtMostOnce"/> 服务质量等级。
    /// </summary>
    /// <param name="topic">订阅主题过滤器，支持 <c>+</c>（单级通配符）和 <c>#</c>（多级通配符）。</param>
    /// <param name="handler">消息处理器。</param>
    /// <returns>当前实例，支持链式调用。</returns>
    public SubscriptionManagementOption<TClient> AddSubscription(string topic, Func<TClient, MqttArrivedMessage, Task> handler)
    {
        this.m_subscriptionItems.Add(new SubscriptionItem<TClient>(topic, QosLevel.AtMostOnce, handler));
        return this;
    }

    /// <summary>
    /// 添加主题订阅及其处理器，并指定服务质量等级。
    /// </summary>
    /// <param name="topic">订阅主题过滤器，支持 <c>+</c>（单级通配符）和 <c>#</c>（多级通配符）。</param>
    /// <param name="qosLevel">服务质量等级。</param>
    /// <param name="handler">消息处理器。</param>
    /// <returns>当前实例，支持链式调用。</returns>
    public SubscriptionManagementOption<TClient> AddSubscription(string topic, QosLevel qosLevel, Func<TClient, MqttArrivedMessage, Task> handler)
    {
        this.m_subscriptionItems.Add(new SubscriptionItem<TClient>(topic, qosLevel, handler));
        return this;
    }
}
