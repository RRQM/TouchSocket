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
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Mqtt;

/// <summary>
/// 表示一个 MQTT 会话的 Actor。
/// </summary>
public class MqttSessionActor : MqttActor
{
    private readonly AsyncResetEvent m_asyncResetEvent = new(false, false);

    private readonly Channel<DistributeMessage> m_mqttArrivedMessageQueue = Channel.CreateBounded<DistributeMessage>(new BoundedChannelOptions(1000)
    {
        FullMode = BoundedChannelFullMode.DropNewest,
        SingleReader = true,
        SingleWriter = false,
        AllowSynchronousContinuations = false,
        Capacity = 1000
    });

    private readonly MqttBroker m_mqttBroker;
    private bool m_sessionPresent;

    /// <summary>
    /// 初始化 <see cref="MqttSessionActor"/> 类的新实例。
    /// </summary>
    /// <param name="messageCenter">MQTT 消息中心。</param>
    public MqttSessionActor(MqttBroker messageCenter)
    {
        this.m_mqttBroker = messageCenter;
        Task.Run(this.WaitForReadAsync);
    }

    /// <summary>
    /// 获取会话是否存在。
    /// </summary>
    public bool SessionPresent => this.m_sessionPresent;

    /// <summary>
    /// 激活会话。
    /// </summary>
    public void Activate()
    {
        this.m_asyncResetEvent.Set();
    }

    /// <summary>
    /// 取消激活会话。
    /// </summary>
    public void Deactivate()
    {
        this.m_asyncResetEvent.Reset();
    }

    /// <summary>
    /// 异步分发消息。
    /// </summary>
    /// <param name="message">要分发的消息。</param>
    public async Task DistributeMessagesAsync(DistributeMessage message)
    {
        var tokenClosed = this.TokenSource.Token;
        if (tokenClosed.IsCancellationRequested)
        {
            return;
        }

        try
        {
            await this.m_mqttArrivedMessageQueue.Writer.WriteAsync(message, tokenClosed).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        catch(Exception ex)
        {
            Console.WriteLine("DistributeMessagesAsync:" + ex.Message);
        }
    }

    /// <summary>
    /// 设置会话存在标志为 true。
    /// </summary>
    public void MakeSessionPresent()
    {
        this.m_sessionPresent = true;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.m_asyncResetEvent.Dispose();
            //this.m_mqttArrivedMessageQueue.Reader.();
        }
        base.Dispose(disposing);
    }

    #region 属性

    /// <summary>
    /// 获取 MQTT 消息中心。
    /// </summary>
    public MqttBroker MqttBroker => this.m_mqttBroker;

    #endregion 属性

    protected override Task InputMqttConnAckMessageAsync(MqttConnAckMessage message)
    {
        throw new NotImplementedException();
    }

    protected override async Task InputMqttConnectMessageAsync(MqttConnectMessage message)
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
            await this.ProtectedOutputSendAsync(mqttConnAckMessage).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }

        this.Id = message.ClientId;

        await this.ProtectedOutputSendAsync(mqttConnAckMessage).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        this.Online = true;

        _ = EasyTask.Run(this.ProtectedMqttOnConnected, new MqttConnectedEventArgs(message, mqttConnAckMessage));
    }

    protected override Task InputMqttPingRespMessageAsync(MqttPingRespMessage message)
    {
        throw new NotImplementedException();
    }

    protected override async Task InputMqttSubscribeMessageAsync(MqttSubscribeMessage message)
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
        await this.ProtectedOutputSendAsync(contentForAck).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    protected override async Task InputMqttUnsubscribeMessageAsync(MqttUnsubscribeMessage message)
    {
        foreach (var topic in message.TopicFilters)
        {
            this.m_mqttBroker.UnregisterActor(this.Id, topic);
        }
        var contentForAck = new MqttUnsubAckMessage()
        {
            MessageId = message.MessageId
        };
        await this.ProtectedOutputSendAsync(contentForAck).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    protected override async Task PublishMessageArrivedAsync(MqttArrivedMessage message)
    {
        await this.m_mqttBroker.ForwardMessageAsync(message).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        await base.PublishMessageArrivedAsync(message).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task PublishDistributeMessageAsync(DistributeMessage distributeMessage)
    {
        var token = this.TokenSource.Token;
        if (token.IsCancellationRequested)
        {
            return;
        }

        var message = distributeMessage.Message;
        var qosLevel = distributeMessage.QosLevel;

        var publishMessage = new MqttPublishMessage(message.TopicName, message.Retain, qosLevel, message.Payload);

        await this.m_asyncResetEvent.WaitOneAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        try
        {
            await this.PublishAsync(publishMessage).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        catch (Exception ex)
        {
            Console.WriteLine("PublishDistributeMessageAsync:"+ex);
        }
    }

    private async Task WaitForReadAsync()
    {
        var token = this.TokenSource.Token;
        while (true)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("WaitForReadAsync IsCancellationRequested");
                    return;
                }

                var distributeMessage = await this.m_mqttArrivedMessageQueue.Reader.ReadAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                if (distributeMessage is null)
                {
                    Console.WriteLine("WaitForReadAsync distributeMessage is null");
                    continue;
                }


                await this.PublishDistributeMessageAsync(distributeMessage).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}