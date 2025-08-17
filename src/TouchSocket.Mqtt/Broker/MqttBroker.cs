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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

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
    /// 线程安全的主题-订阅者映射管理类（使用读写锁）
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
        /// <exception cref="ArgumentException">主题名称为空</exception>
        /// <exception cref="ArgumentNullException">订阅者为 null</exception>
        public void AddSubscriber(string topic, Subscription subscriber)
        {
            this.ValidateTopic(topic);

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
        /// <param name="topic">主题名称（非空）</param>
        /// <returns>是否成功移除（主题存在时为 true）</returns>
        /// <exception cref="ArgumentException">主题名称为空</exception>
        public bool ClearTopic(string topic)
        {
            this.ValidateTopic(topic);

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
        /// <param name="topic">主题名称（非空）</param>
        /// <param name="subscriber">要检查的订阅者实例（非空）</param>
        /// <returns>是否存在该订阅者</returns>
        /// <exception cref="ArgumentException">主题名称为空</exception>
        /// <exception cref="ArgumentNullException">订阅者为 null</exception>
        public bool ContainsSubscriber(string topic, Subscription subscriber)
        {
            this.ValidateTopic(topic);

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
        /// <returns>主题名称的只读集合</returns>
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
        /// 获取指定主题的所有订阅者（返回只读副本，防止外部修改）
        /// </summary>
        /// <param name="topic">主题名称（非空）</param>
        /// <returns>订阅者集合的只读副本（可能为空）</returns>
        /// <exception cref="ArgumentException">主题名称为空</exception>
        public IReadOnlyList<Subscription> GetSubscribers(string topic)
        {
            this.ValidateTopic(topic);

            this.m_lock.EnterReadLock();
            try
            {
                if (this.m_topicToSubscribers.TryGetValue(topic, out var subscribers))
                {
                    // 返回副本确保线程安全（避免外部修改内部集合）
                    return subscribers.ToArray();
                }
                return [];
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 从指定主题移除订阅者
        /// </summary>
        /// <param name="topic">主题名称（非空）</param>
        /// <param name="subscriber">要移除的订阅者实例（非空）</param>
        /// <returns>是否成功移除</returns>
        /// <exception cref="ArgumentException">主题名称为空</exception>
        /// <exception cref="ArgumentNullException">订阅者为 null</exception>
        public bool RemoveSubscriber(string topic, Subscription subscriber)
        {
            this.ValidateTopic(topic);

            this.m_lock.EnterWriteLock();
            try
            {
                if (this.m_topicToSubscribers.TryGetValue(topic, out var subscribers))
                {
                    var isRemoved = subscribers.Remove(subscriber);
                    // 如果主题已无订阅者，清理空主题
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

        #region 辅助方法

        private void ValidateTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentException("主题名称不能为 null 或空白", nameof(topic));
            }
        }

        #endregion 辅助方法
    }

    #endregion Class
}