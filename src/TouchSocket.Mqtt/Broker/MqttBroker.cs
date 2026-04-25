//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//交流QQ群：234762506
// 感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Collections.Concurrent;

namespace TouchSocket.Mqtt;

/// <summary>
/// 表示一个Mqtt代理。
/// </summary>
public class MqttBroker : DisposableObject
{
    private readonly ConcurrentDictionary<string, MqttSessionActor> m_sessionActors = new();
    private readonly ThreadSafeTopicSubscriptions m_topicToActors = new();
    private Timer m_cleanupTimer;
    private TimeSpan m_sessionExpiry = TimeSpan.Zero;
    public int MessageCapacity { get; set; }

    public TimeSpan MessageExpiry { get; set; }

    /// <summary>
    /// 获取或设置离线会话的过期时长。超过该时长的离线会话将自动从代理中移除。
    /// 设置为 <see cref="TimeSpan.Zero"/> 时禁用自动清除。
    /// </summary>
    public TimeSpan SessionExpiry
    {
        get => this.m_sessionExpiry;
        set
        {
            this.m_sessionExpiry = value;
            this.ResetCleanupTimer();
        }
    }

    /// <summary>
    /// 将订阅者添加到指定主题。
    /// </summary>
    /// <param name="topic">主题名称。</param>
    /// <param name="subscriber">订阅者实例。</param>
    public void AddSubscriber(string topic, Subscription subscriber)
    {
        this.m_topicToActors.AddSubscriber(topic, subscriber);
    }

    /// <summary>
    /// 清空所有主题和订阅者。
    /// </summary>
    public void Clear()
    {
        this.m_topicToActors.Clear();
    }

    /// <summary>
    /// 移除指定主题的所有订阅者（保留空主题）。
    /// </summary>
    /// <param name="topic">主题名称。</param>
    public bool ClearTopic(string topic)
    {
        return this.m_topicToActors.ClearTopic(topic);
    }

    /// <summary>
    /// 检查指定主题是否包含特定订阅者。
    /// </summary>
    /// <param name="topic">主题名称。</param>
    /// <param name="subscriber">订阅者实例。</param>
    public bool ContainsSubscriber(string topic, Subscription subscriber)
    {
        return this.m_topicToActors.ContainsSubscriber(topic, subscriber);
    }

    /// <summary>
    /// 异步转发消息。
    /// </summary>
    /// <param name="message">到达的Mqtt消息。</param>
    public async ValueTask<int> ForwardMessageAsync(MqttArrivedMessage message)
    {
        var topic = message.TopicName;
        var subscribers = this.m_topicToActors.GetSubscribers(topic);

        if (subscribers.Count == 0)
        {
            return 0;
        }

        var sharedPayload = new SharedPayload(message.Payload, subscribers.Count);

        var deliveredCount = 0;
        foreach (var subscription in subscribers)
        {
            var actorId = subscription.ClientId;
            var actorTarget = this.FindMqttActor(actorId);
            if (actorTarget == null)
            {
                sharedPayload.Dispose();
                continue;
            }
            var qosLevel = MqttExtension.MinQosLevel(subscription.QosLevel, message.QosLevel);
            var distributeMessage = new DistributeMessage(message.TopicName, message.Retain, qosLevel, sharedPayload);
            await actorTarget.PostDistributeMessageAsync(distributeMessage).ConfigureDefaultAwait();
            deliveredCount++;
        }
        return deliveredCount;
    }

    /// <summary>
    /// 获取所有主题的快照。
    /// </summary>
    public IReadOnlyList<string> GetAllTopics()
    {
        return this.m_topicToActors.GetAllTopics();
    }

    /// <summary>
    /// 获取或创建Mqtt会话参与者。
    /// </summary>
    /// <param name="clientId">客户端ID。</param>
    /// <returns>Mqtt会话参与者。</returns>
    public MqttSessionActor GetOrCreateMqttSessionActor(string clientId)
    {
        if (this.m_sessionActors.TryGetValue(clientId, out var actor))
        {
            actor.MakeSessionPresent();
            return actor;
        }
        else
        {
            var newActor = new MqttSessionActor(this);
            newActor.MessageCapacity = this.MessageCapacity;
            newActor.MessageExpiry = this.MessageExpiry;
            if (this.m_sessionActors.TryAdd(clientId, newActor))
            {
                return newActor;
            }
            else
            {
                return this.GetOrCreateMqttSessionActor(clientId);
            }
        }
    }

    /// <summary>
    /// 获取与指定主题匹配的订阅者（支持通配符匹配）。
    /// </summary>
    public IReadOnlyList<Subscription> GetSubscribers(string topic)
    {
        return this.m_topicToActors.GetSubscribers(topic);
    }

    /// <summary>
    /// 加载配置选项。
    /// </summary>
    /// <param name="option">配置选项。</param>
    public void LoadConfig(MqttBrokerOption option)
    {
        this.SessionExpiry = option.SessionExpiry;
        this.MessageCapacity = option.MessageCapacity;
        this.MessageExpiry = option.MessageExpiry;
    }

    /// <summary>
    /// 注册参与者。
    /// </summary>
    /// <param name="clientId">客户端ID。</param>
    /// <param name="topic">主题。</param>
    /// <param name="qosLevel">服务质量等级。</param>
    public void RegisterActor(string clientId, string topic, QosLevel qosLevel)
    {
        var subscription = new Subscription(clientId, qosLevel);

        this.m_topicToActors.AddSubscriber(topic, subscription);
    }

    /// <summary>
    /// 移除Mqtt会话参与者。
    /// </summary>
    /// <param name="clientId">客户端ID。</param>
    /// <returns>是否成功移除。</returns>
    public bool RemoveMqttSessionActor(string clientId)
    {
        if (this.m_sessionActors.TryRemove(clientId, out var mqttSessionActor))
        {
            mqttSessionActor.Dispose();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 从指定主题移除订阅者。
    /// </summary>
    /// <param name="topic">主题名称。</param>
    /// <param name="subscriber">订阅者实例。</param>
    public bool RemoveSubscriber(string topic, Subscription subscriber)
    {
        return this.m_topicToActors.RemoveSubscriber(topic, subscriber);
    }

    /// <summary>
    /// 注销参与者。
    /// </summary>
    /// <param name="clientId">客户端ID。</param>
    /// <param name="topic">主题。</param>
    /// <returns>是否成功注销。</returns>
    public bool UnregisterActor(string clientId, string topic)
    {
        return this.m_topicToActors.RemoveSubscriber(topic, new Subscription(clientId));
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        this.m_cleanupTimer?.Dispose();
        base.Dispose(disposing);
    }

    private void CleanupExpiredSessions(object state)
    {
        var expiry = this.m_sessionExpiry;
        if (expiry <= TimeSpan.Zero)
        {
            return;
        }
        var now = DateTimeOffset.UtcNow;
        foreach (var kvp in this.m_sessionActors)
        {
            var offlineSince = kvp.Value.OfflineSince;
            if (offlineSince.HasValue && (now - offlineSince.Value) >= expiry)
            {
                this.RemoveMqttSessionActor(kvp.Key);
            }
        }
    }

    private MqttSessionActor FindMqttActor(string actorId)
    {
        if (this.m_sessionActors.TryGetValue(actorId, out var actor))
        {
            return actor;
        }
        return null;
    }

    private void ResetCleanupTimer()
    {
        var expiry = this.m_sessionExpiry;
        if (expiry > TimeSpan.Zero)
        {
            var intervalMs = (long)Math.Min(expiry.TotalMilliseconds, 30_000);
            if (this.m_cleanupTimer == null)
            {
                this.m_cleanupTimer = new Timer(this.CleanupExpiredSessions, null, intervalMs, intervalMs);
            }
            else
            {
                this.m_cleanupTimer.Change(intervalMs, intervalMs);
            }
        }
        else
        {
            this.m_cleanupTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}