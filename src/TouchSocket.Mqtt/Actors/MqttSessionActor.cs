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
/// 表示一个 Mqtt 会话的 Actor。
/// </summary>
public class MqttSessionActor : MqttActor
{
    private readonly AsyncManualResetEvent m_asyncResetEvent = new(false);
    private readonly Queue<DistributeMessage> m_messageQueue = new Queue<DistributeMessage>();
    private readonly MqttBroker m_mqttBroker;
    private readonly Lock m_queueLock = new Lock();
    private readonly SemaphoreSlim m_queueSemaphore = new SemaphoreSlim(0);
    private int m_messageCapacity = 1000;
    private TimeSpan m_messageExpiry = TimeSpan.Zero;
    private MqttPublishMessage m_mqttWillMessage;
    private long m_offlineTicksUtc = -1L;
    private bool m_sessionPresent;

    /// <summary>
    /// 初始化 <see cref="MqttSessionActor"/> 类的新实例。
    /// </summary>
    /// <param name="broker">Mqtt 消息中心。</param>
    public MqttSessionActor(MqttBroker broker)
    {
        this.m_mqttBroker = broker;
        _ = EasyTask.SafeNewRun(this.WaitForReadAsync);
    }

    /// <summary>
    /// 获取或设置消息队列的最大容量，达到上限时新消息将被丢弃。默认值为 1000。
    /// </summary>
    public int MessageCapacity
    {
        get => this.m_messageCapacity;
        set => this.m_messageCapacity = value > 0 ? value : 1000;
    }

    /// <summary>
    /// 当 Mqtt 消息被丢弃时调用。
    /// </summary>
    public Func<MqttSessionActor, MqttMessageDiscardedEventArgs, Task> MessageDiscarded { get; set; }

    /// <summary>
    /// 获取或设置消息的过期时间。从服务端接收到消息起计算，超过此时间的消息将被丢弃。
    /// 设置为 <see cref="TimeSpan.Zero"/> 时表示永不过期。
    /// </summary>
    public TimeSpan MessageExpiry
    {
        get => this.m_messageExpiry;
        set => this.m_messageExpiry = value;
    }

    /// <summary>
    /// 获取会话离线的时间。若会话在线，则返回 <see langword="null"/>。
    /// </summary>
    public DateTimeOffset? OfflineSince
    {
        get
        {
            var ticks = Interlocked.Read(ref this.m_offlineTicksUtc);
            return ticks < 0L ? null : new DateTimeOffset(ticks, TimeSpan.Zero);
        }
    }

    /// <summary>
    /// 获取会话是否存在。
    /// </summary>
    public bool SessionPresent => this.m_sessionPresent;

    /// <summary>
    /// 激活会话。
    /// </summary>
    public void Activate(MqttConnectMessage mqttConnectMessage)
    {
        Interlocked.Exchange(ref this.m_offlineTicksUtc, -1L);
        if (mqttConnectMessage.WillFlag)
        {
            var mqttPublishMessage = new MqttPublishMessage(mqttConnectMessage.WillTopic, mqttConnectMessage.WillPayload);
            this.m_mqttWillMessage = mqttPublishMessage;
        }
        else
        {
            this.m_mqttWillMessage = null;
        }
        this.m_asyncResetEvent.Set();
    }

    /// <summary>
    /// 取消激活会话。
    /// </summary>
    public async Task Deactivate()
    {
        Interlocked.Exchange(ref this.m_offlineTicksUtc, DateTimeOffset.UtcNow.UtcTicks);
        this.m_asyncResetEvent.Reset();
        var willMessage = this.m_mqttWillMessage;
        this.m_mqttWillMessage = null;
        if (willMessage != null)
        {
            await this.m_mqttBroker.ForwardMessageAsync(new MqttArrivedMessage(willMessage.TopicName, willMessage.QosLevel, willMessage.Retain, willMessage.Payload));
        }
    }

    /// <summary>
    /// 设置会话存在标志为 true。
    /// </summary>
    public void MakeSessionPresent()
    {
        this.m_sessionPresent = true;
    }

    /// <summary>
    /// 异步分发消息。
    /// </summary>
    /// <param name="message">要分发的消息。</param>
    internal async Task PostDistributeMessageAsync(DistributeMessage message)
    {
        var tokenClosed = this.TokenSource.Token;
        if (tokenClosed.IsCancellationRequested)
        {
            message.Dispose();
            return;
        }

        bool enqueued;
        lock (this.m_queueLock)
        {
            if (this.m_messageQueue.Count >= this.m_messageCapacity)
            {
                enqueued = false;
            }
            else
            {
                this.m_messageQueue.Enqueue(message);
                enqueued = true;
            }
        }

        if (!enqueued)
        {
            await this.OnMessageDiscardedAsync(message, DiscardReason.QueueFull).ConfigureDefaultAwait();
            message.Dispose();
            return;
        }

        this.m_queueSemaphore.Release();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.m_asyncResetEvent.Set();
        }
        base.Dispose(disposing);
        if (disposing)
        {
            lock (this.m_queueLock)
            {
                while (this.m_messageQueue.TryDequeue(out var message))
                {
                    message.Dispose();
                }
            }
        }
    }

    /// <inheritdoc/>
    protected override Task InputMqttConnAckMessageAsync(MqttConnAckMessage message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    protected override async Task InputMqttConnectMessageAsync(MqttConnectMessage message, CancellationToken cancellationToken)
    {
        var mqttConnAckMessage = new MqttConnAckMessage(this.m_sessionPresent)
        {
            ReturnCode = MqttReasonCode.ConnectionAccepted,
            AssignedClientIdentifier = message.ClientId,
            AuthenticationData = message.AuthenticationData,
            AuthenticationMethod = message.AuthenticationMethod,
            MaximumPacketSize = message.MaximumPacketSize,
            ReceiveMaximum = message.ReceiveMaximum,
            SessionExpiryInterval = message.SessionExpiryInterval,
            ReasonCode = MqttReasonCode.Success,
            ReasonString = "Success",
            ResponseInformation = "Success"
        };

        var e = new MqttConnectingEventArgs(message, mqttConnAckMessage);
        await this.ProtectedMqttOnConnecting(e).ConfigureDefaultAwait();

        if (mqttConnAckMessage.ReturnCode != MqttReasonCode.ConnectionAccepted)
        {
            await this.ProtectedOutputSendAsync(mqttConnAckMessage, cancellationToken).ConfigureDefaultAwait();
            return;
        }

        this.Id = message.ClientId;

        await this.ProtectedOutputSendAsync(mqttConnAckMessage, cancellationToken).ConfigureDefaultAwait();

        this.Online = true;

        _ = EasyTask.SafeRun(this.ProtectedMqttOnConnected, new MqttConnectedEventArgs(message, mqttConnAckMessage));
    }

    /// <inheritdoc/>
    protected override Task InputMqttPingRespMessageAsync(MqttPingRespMessage message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    protected override async Task InputMqttSubscribeMessageAsync(MqttSubscribeMessage message, CancellationToken cancellationToken)
    {
        var contentForAck = new MqttSubAckMessage()
        {
            MessageId = message.MessageId
        };
        foreach (var item in message.SubscribeRequests)
        {
            this.m_mqttBroker.RegisterActor(this.Id, item.Topic, item.QosLevel);
            contentForAck.AddReturnCode(item.QosLevel);
        }
        await this.ProtectedOutputSendAsync(contentForAck, cancellationToken).ConfigureDefaultAwait();
    }

    /// <inheritdoc/>
    protected override async Task InputMqttUnsubscribeMessageAsync(MqttUnsubscribeMessage message, CancellationToken cancellationToken)
    {
        foreach (var topic in message.TopicFilters)
        {
            this.m_mqttBroker.UnregisterActor(this.Id, topic);
        }
        var contentForAck = new MqttUnsubAckMessage()
        {
            MessageId = message.MessageId
        };
        await this.ProtectedOutputSendAsync(contentForAck, cancellationToken).ConfigureDefaultAwait();
    }

    /// <inheritdoc/>
    protected override async Task PublishMessageArrivedAsync(MqttArrivedMessage message)
    {
        await this.m_mqttBroker.ForwardMessageAsync(message).ConfigureDefaultAwait();
        await base.PublishMessageArrivedAsync(message).ConfigureDefaultAwait();
    }

    private async ValueTask OnMessageDiscardedAsync(DistributeMessage message, DiscardReason reason)
    {
        if (this.MessageDiscarded != null)
        {
            var e = new MqttMessageDiscardedEventArgs(this.Id, message, reason);
            await this.MessageDiscarded.Invoke(this, e).ConfigureAwait(false);
        }
    }

    #region 属性

    /// <summary>
    /// 获取 Mqtt 消息中心。
    /// </summary>
    public MqttBroker MqttBroker => this.m_mqttBroker;

    #endregion 属性

    private async ValueTask<bool> PublishDistributeMessageAsync(DistributeMessage distributeMessage, CancellationToken cancellationToken)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            var publishMessage = new MqttPublishMessage(distributeMessage.TopicName, distributeMessage.Retain, distributeMessage.QosLevel, distributeMessage.SharedPayload.Payload);

            await this.m_asyncResetEvent.WaitOneAsync(cancellationToken).ConfigureDefaultAwait();

            await this.PublishAsync(publishMessage, cancellationToken).ConfigureDefaultAwait();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task WaitForReadAsync()
    {
        var cancellationToken = this.TokenSource.Token;
        while (true)
        {
            try
            {
                await this.m_queueSemaphore.WaitAsync(cancellationToken).ConfigureDefaultAwait();

                DistributeMessage distributeMessage;
                lock (this.m_queueLock)
                {
                    if (!this.m_messageQueue.TryDequeue(out distributeMessage))
                    {
                        continue;
                    }
                }

                var expiry = this.m_messageExpiry;
                if (expiry > TimeSpan.Zero && DateTimeOffset.UtcNow - distributeMessage.ReceivedTime > expiry)
                {
                    await this.OnMessageDiscardedAsync(distributeMessage, DiscardReason.Expired).ConfigureDefaultAwait();
                    distributeMessage.Dispose();
                    continue;
                }

                var published = await this.PublishDistributeMessageAsync(distributeMessage, cancellationToken).ConfigureDefaultAwait();
                if (!published)
                {
                    await this.OnMessageDiscardedAsync(distributeMessage, DiscardReason.PublishFailed).ConfigureDefaultAwait();
                }
                distributeMessage.Dispose();
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch
            {
                //
            }
        }
    }
}