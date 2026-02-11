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
            await actorTarget.PostDistributeMessageAsync(distributeMessage).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            deliveredCount++;
        }
        return deliveredCount;
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

}
