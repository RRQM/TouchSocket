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
public class MqttBroker
{
    private readonly ConcurrentDictionary<string, MqttSessionActor> m_sessionActors = new();

    private readonly ThreadSafeTopicSubscriptions m_topicToActors = new();

    /// <summary>
    /// 异步转发消息。
    /// </summary>
    /// <param name="message">到达的Mqtt消息。</param>
    public async Task ForwardMessageAsync(MqttArrivedMessage message)
    {
        var topic = message.TopicName;
        foreach (var subscription in this.m_topicToActors.GetSubscribers(topic))
        {
            var actorId = subscription.ClientId;
            var actorTarget = this.FindMqttActor(actorId);
            if (actorTarget == null)
            {
                continue;
            }
            var qosLevel = MqttExtension.MinQosLevel(subscription.QosLevel, message.QosLevel);
            await actorTarget.PostDistributeMessageAsync(new DistributeMessage(message, qosLevel)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
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
    /// 移除Mqtt会话参与者。
    /// </summary>
    /// <param name="m_mqttActor">Mqtt会话参与者。</param>
    /// <returns>是否成功移除。</returns>
    public bool RemoveMqttSessionActor(MqttSessionActor m_mqttActor)
    {
        if (this.m_sessionActors.TryRemove(m_mqttActor.Id, out var mqttSessionActor))
        {
            mqttSessionActor.Dispose();
            return true;
        }
        return false;
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

    private MqttSessionActor FindMqttActor(string actorId)
    {
        if (this.m_sessionActors.TryGetValue(actorId, out var actor))
        {
            return actor;
        }
        return null;
    }

    #region Class

    private readonly struct Subscription : IEquatable<Subscription>
    {
        public Subscription(string clientId, QosLevel qosLevel) : this(clientId)
        {
            this.QosLevel = qosLevel;
        }

        public Subscription(string clientId)
        {
            this.ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            this.QosLevel = default;
        }

        public string ClientId { get; }
        public QosLevel QosLevel { get; }

        public static bool operator !=(Subscription left, Subscription right)
        {
            return !(left == right);
        }

        public static bool operator ==(Subscription left, Subscription right)
        {
            return left.Equals(right);
        }

        public bool Equals(Subscription other)
        {
            return this.ClientId.Equals(other.ClientId);
        }

        public override bool Equals(object obj)
        {
            return obj is Subscription subscription && this.Equals(subscription);
        }

        public override int GetHashCode()
        {
            return this.ClientId.GetHashCode();
        }
    }

    /// <summary>
    ///线程安全的主题-订阅者映射管理类（使用读写锁）
    /// </summary>
    private class ThreadSafeTopicSubscriptions
    {
        private readonly ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        private readonly IEqualityComparer<Subscription> m_subscriberComparer;
        private readonly Dictionary<string, HashSet<Subscription>> m_topicToSubscribers = new Dictionary<string, HashSet<Subscription>>();

        public ThreadSafeTopicSubscriptions(IEqualityComparer<Subscription> subscriberComparer = null)
        {
            this.m_subscriberComparer = subscriberComparer ?? EqualityComparer<Subscription>.Default;
        }

        /// <summary>
        /// 添加订阅者到指定主题
        /// </summary>
        /// <param name="topic">主题名称（非空）</param>
        /// <param name="subscriber">订阅者实例（非空）</param>
        public void AddSubscriber(string topic, Subscription subscriber)
        {
            ValidateTopic(topic);

            this.m_lock.EnterWriteLock();
            try
            {
                // 如果主题不存在则创建新的订阅者集合
                if (!this.m_topicToSubscribers.TryGetValue(topic, out var subscribers))
                {
                    subscribers = new HashSet<Subscription>(this.m_subscriberComparer);
                    this.m_topicToSubscribers[topic] = subscribers;
                }

                subscribers.Remove(subscriber);
                // 添加新的订阅者（若已存在会被自动去重，但此处因已提前移除，必然添加成功）
                subscribers.Add(subscriber);
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 清空所有主题和订阅者
        /// </summary>
        public void Clear()
        {
            this.m_lock.EnterWriteLock();
            try
            {
                this.m_topicToSubscribers.Clear();
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 移除指定主题的所有订阅者（保留空主题）
        /// </summary>
        public bool ClearTopic(string topic)
        {
            ValidateTopic(topic);

            this.m_lock.EnterWriteLock();
            try
            {
                return this.m_topicToSubscribers.Remove(topic);
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 检查指定主题是否包含特定订阅者
        /// </summary>
        public bool ContainsSubscriber(string topic, Subscription subscriber)
        {
            ValidateTopic(topic);

            this.m_lock.EnterReadLock();
            try
            {
                return this.m_topicToSubscribers.TryGetValue(topic, out var subscribers)
                       && subscribers.Contains(subscriber);
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 获取所有存在的主题名称（返回只读副本）
        /// </summary>
        public IReadOnlyList<string> GetAllTopics()
        {
            this.m_lock.EnterReadLock();
            try
            {
                // 返回键的副本防止外部修改字典
                return this.m_topicToSubscribers.Keys.ToArray();
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 获取指定主题的所有订阅者（支持通配符匹配）
        /// </summary>
        public IReadOnlyList<Subscription> GetSubscribers(string topic)
        {
            ValidateTopic(topic);

            this.m_lock.EnterReadLock();
            try
            {
                var matchingSubscriptions = new List<Subscription>();

                foreach (var kv in this.m_topicToSubscribers)
                {
                    var subscriptionTopic = kv.Key;
                    var subscribers = kv.Value;

                    if (this.IsTopicMatch(topic, subscriptionTopic))
                    {
                        matchingSubscriptions.AddRange(subscribers);
                    }
                }

                return matchingSubscriptions.ToArray();
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 从指定主题移除订阅者
        /// </summary>
        public bool RemoveSubscriber(string topic, Subscription subscriber)
        {
            ValidateTopic(topic);

            this.m_lock.EnterWriteLock();
            try
            {
                if (this.m_topicToSubscribers.TryGetValue(topic, out var subscribers))
                {
                    var isRemoved = subscribers.Remove(subscriber);
                    if (isRemoved && subscribers.Count == 0)
                    {
                        this.m_topicToSubscribers.Remove(topic);
                    }
                    return isRemoved;
                }
                return false;
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        // topic 匹配逻辑，支持 + 和 # 通配符
        private bool IsTopicMatch(string publishTopic, string subscriptionTopic)
        {
            if (subscriptionTopic.IndexOf('+') < 0 && subscriptionTopic.IndexOf('#') < 0)
            {
                return string.Equals(publishTopic, subscriptionTopic, StringComparison.Ordinal);
            }

            return this.CompareTopicWithWildcard(publishTopic, subscriptionTopic);
        }

        private bool CompareTopicWithWildcard(ReadOnlySpan<char> topic, ReadOnlySpan<char> filter)
        {
            const char LevelSeparator = '/';
            const char MultiLevelWildcard = '#';
            const char SingleLevelWildcard = '+';
            const char ReservedTopicPrefix = '$';

            if (topic.IsEmpty || filter.IsEmpty)
            {
                return false;
            }

            var filterOffset = 0;
            var filterLength = filter.Length;
            var topicOffset = 0;
            var topicLength = topic.Length;

            // 检查过滤器长度
            if (filterLength > topicLength)
            {
                var lastFilterChar = filter[filterLength - 1];
                if (lastFilterChar != MultiLevelWildcard && lastFilterChar != SingleLevelWildcard)
                {
                    return false;
                }
            }

            var isMultiLevelFilter = filter[filterLength - 1] == MultiLevelWildcard;
            var isReservedTopic = topic[0] == ReservedTopicPrefix;

            // 保留主题的特殊规则
            if (isReservedTopic && filterLength == 1 && isMultiLevelFilter)
            {
                return false; // 不允许用 '#' 订阅 '$foo/bar'
            }

            if (isReservedTopic && filter[0] == SingleLevelWildcard)
            {
                return false; // 不允许用 '+/monitor/Clients' 订阅 '$SYS/monitor/Clients'
            }

            if (filterLength == 1 && isMultiLevelFilter)
            {
                return true; // '#' 匹配所有内容
            }

            // 逐字符比较
            while (filterOffset < filterLength && topicOffset < topicLength)
            {
                // 检查多级通配符是否在最后位置
                if (filter[filterOffset] == MultiLevelWildcard && filterOffset != filterLength - 1)
                {
                    return false; // 多级通配符只能在最后
                }

                if (filter[filterOffset] == topic[topicOffset])
                {
                    if (topicOffset == topicLength - 1)
                    {
                        if (filterOffset == filterLength - 3 && filter[filterOffset + 1] == LevelSeparator && isMultiLevelFilter)
                        {
                            return true;
                        }

                        if (filterOffset == filterLength - 2 && filter[filterOffset] == LevelSeparator && isMultiLevelFilter)
                        {
                            return true;
                        }
                    }

                    filterOffset++;
                    topicOffset++;

                    // 检查是否完全匹配
                    if (filterOffset == filterLength && topicOffset == topicLength)
                    {
                        return true;
                    }

                    var endOfTopic = topicOffset == topicLength;

                    if (endOfTopic && filterOffset == filterLength - 1 && filter[filterOffset] == SingleLevelWildcard)
                    {
                        if (filterOffset > 0 && filter[filterOffset - 1] != LevelSeparator)
                        {
                            return false;
                        }
                        return true;
                    }
                }
                else
                {
                    if (filter[filterOffset] == SingleLevelWildcard)
                    {
                        // 检查单级通配符的有效性
                        if (filterOffset > 0 && filter[filterOffset - 1] != LevelSeparator)
                        {
                            return false; // 无效的 "+foo" 或 "a/+foo"
                        }

                        if (filterOffset < filterLength - 1 && filter[filterOffset + 1] != LevelSeparator)
                        {
                            return false; // 无效的 "foo+" 或 "foo+/a"
                        }

                        filterOffset++;
                        // 跳过主题中的当前级别
                        while (topicOffset < topicLength && topic[topicOffset] != LevelSeparator)
                        {
                            topicOffset++;
                        }

                        if (topicOffset == topicLength && filterOffset == filterLength)
                        {
                            return true;
                        }
                    }
                    else if (filter[filterOffset] == MultiLevelWildcard)
                    {
                        if (filterOffset > 0 && filter[filterOffset - 1] != LevelSeparator)
                        {
                            return false;
                        }

                        if (filterOffset + 1 != filterLength)
                        {
                            return false;
                        }

                        return true; // 多级通配符匹配剩余所有内容
                    }
                    else
                    {
                        // 检查 "foo/bar" 匹配 "foo/+/#"
                        if (filterOffset > 0 &&
                            filterOffset + 2 == filterLength &&
                            topicOffset == topicLength &&
                            filter[filterOffset - 1] == SingleLevelWildcard &&
                            filter[filterOffset] == LevelSeparator &&
                            isMultiLevelFilter)
                        {
                            return true;
                        }

                        return false;
                    }
                }
            }

            return false;
        }

        private static void ValidateTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentException("主题名称不能为 null 或空白", nameof(topic));
            }
        }
    }

    #endregion Class
}