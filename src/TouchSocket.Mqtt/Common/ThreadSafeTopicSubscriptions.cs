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

namespace TouchSocket.Mqtt;

/// <summary>
///线程安全的主题-订阅者映射管理类（使用读写锁）
/// </summary>
class ThreadSafeTopicSubscriptions
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

                if (IsTopicMatch(topic, subscriptionTopic))
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
    private static bool IsTopicMatch(string publishTopic, string subscriptionTopic)
    {
        return MqttExtension.IsTopicMatch(publishTopic, subscriptionTopic);
    }

    private static void ValidateTopic(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
        {
            throw new ArgumentException("主题名称不能为 null 或空白", nameof(topic));
        }
    }
}