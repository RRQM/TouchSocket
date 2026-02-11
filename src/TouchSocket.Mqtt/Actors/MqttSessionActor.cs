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

using System.Threading.Channels;

namespace TouchSocket.Mqtt;

/// <summary>
/// 表示一个 Mqtt 会话的 Actor。
/// </summary>
public class MqttSessionActor : MqttActor
{
    private readonly AsyncManualResetEvent m_asyncResetEvent = new(false);

    private readonly Channel<DistributeMessage> m_mqttArrivedMessageQueue = Channel.CreateBounded<DistributeMessage>(new BoundedChannelOptions(1000)
    {
        FullMode = BoundedChannelFullMode.DropNewest,
        SingleReader = true,
        SingleWriter = false,
        AllowSynchronousContinuations = false,
        Capacity = 1000
    });

    private readonly MqttBroker m_mqttBroker;
    private MqttPublishMessage m_mqttWillMessage;
    private bool m_sessionPresent;

    /// <summary>
    /// 初始化 <see cref="MqttSessionActor"/> 类的新实例。
    /// </summary>
    /// <param name="messageCenter">Mqtt 消息中心。</param>
    public MqttSessionActor(MqttBroker messageCenter)
    {
        this.m_mqttBroker = messageCenter;
        _ = EasyTask.SafeRun(this.WaitForReadAsync);
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

        try
        {
            await this.m_mqttArrivedMessageQueue.Writer.WriteAsync(message, tokenClosed).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        catch
        {
            message.Dispose();
        }
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.m_asyncResetEvent.Set();
            
            this.m_mqttArrivedMessageQueue.Writer.Complete();
            
            while (this.m_mqttArrivedMessageQueue.Reader.TryRead(out var message))
            {
                message.Dispose();
            }
        }
        base.Dispose(disposing);
    }

    #region 属性

    /// <summary>
    /// 获取 Mqtt 消息中心。
    /// </summary>
    public MqttBroker MqttBroker => this.m_mqttBroker;

    #endregion 属性

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
        await this.ProtectedMqttOnConnecting(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        if (mqttConnAckMessage.ReturnCode != MqttReasonCode.ConnectionAccepted)
        {
            await this.ProtectedOutputSendAsync(mqttConnAckMessage, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }

        this.Id = message.ClientId;

        await this.ProtectedOutputSendAsync(mqttConnAckMessage, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

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
        await this.ProtectedOutputSendAsync(contentForAck, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
        await this.ProtectedOutputSendAsync(contentForAck, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected override async Task PublishMessageArrivedAsync(MqttArrivedMessage message)
    {
        await this.m_mqttBroker.ForwardMessageAsync(message).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        await base.PublishMessageArrivedAsync(message).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async ValueTask<bool> PublishDistributeMessageAsync(DistributeMessage distributeMessage, CancellationToken cancellationToken)
    {
        
        try
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            var publishMessage = new MqttPublishMessage(distributeMessage.TopicName, distributeMessage.Retain, distributeMessage.QosLevel, distributeMessage.SharedPayload.Payload);

            await this.m_asyncResetEvent.WaitOneAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            await this.PublishAsync(publishMessage, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
        var reader = this.m_mqttArrivedMessageQueue.Reader;
        while (true)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("WaitForReadAsync IsCancellationRequested");
                    return;
                }

                var b = await reader.WaitToReadAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                if (!b)
                {
                    return;
                }

                if (!reader.TryPeek(out var distributeMessage))
                {
                    continue;
                }

                var published = await this.PublishDistributeMessageAsync(distributeMessage, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                if (published)
                {
                    if (reader.TryRead(out var readMessage))
                    {
                        readMessage.Dispose();
                    }
                }
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