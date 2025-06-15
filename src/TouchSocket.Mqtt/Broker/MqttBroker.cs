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
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Mqtt;

/// <summary>
/// 表示一个MQTT代理。
/// </summary>
public class MqttBroker
{
    private readonly ConcurrentDictionary<string, MqttSessionActor> m_sessionActors = new();

    private readonly Dictionary<string, HashSet<Subscription>> m_topicToActors = new();

    /// <summary>
    /// 异步转发消息。
    /// </summary>
    /// <param name="message">到达的MQTT消息。</param>
    public async Task ForwardMessageAsync(MqttArrivedMessage message)
    {
        var topic = message.TopicName;
        if (this.m_topicToActors.TryGetValue(topic, out var actors))
        {
            foreach (var subscription in actors)
            {
                var actorId = subscription.ClientId;
                var actorTarget = this.FindMqttActor(actorId);
                if (actorTarget == null)
                {
                    continue;
                }
                var qosLevel = MqttExtension.MinQosLevel(subscription.QosLevel, message.QosLevel);
                await actorTarget.DistributeMessagesAsync(new DistributeMessage(message, qosLevel)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }
    }
    /// <summary>
    /// 获取或创建MQTT会话参与者。
    /// </summary>
    /// <param name="clientId">客户端ID。</param>
    /// <returns>MQTT会话参与者。</returns>
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
        if (this.m_topicToActors.TryGetValue(topic, out var actors))
        {
            actors.Remove(subscription);
            actors.Add(subscription);
        }
        else
        {
            this.m_topicToActors.Add(topic, new HashSet<Subscription> { subscription });
        }
    }

    /// <summary>
    /// 移除MQTT会话参与者。
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
    /// 移除MQTT会话参与者。
    /// </summary>
    /// <param name="m_mqttActor">MQTT会话参与者。</param>
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
        if (this.m_topicToActors.TryGetValue(topic, out var subscriptions))
        {
            return subscriptions.Remove(new Subscription(clientId));
        }
        return false;
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

    #endregion Class
}