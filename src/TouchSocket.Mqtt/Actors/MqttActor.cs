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

using TouchSocket.Sockets;

namespace TouchSocket.Mqtt;

/// <inheritdoc/>
public abstract class MqttActor : DisposableObject, IOnlineClient
{
    #region 字段

    private readonly Dictionary<ushort, MqttArrivedMessage> m_qos2MqttArrivedMessage = new();
    private readonly CancellationTokenSource m_tokenSource = new();
    private readonly WaitHandlePool<MqttIdentifierMessage> m_waitHandlePool = new(1, ushort.MaxValue);

    #endregion 字段

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (this.DisposedValue)
        {
            return;
        }
        base.Dispose(disposing);

        if (disposing)
        {
            this.m_tokenSource.Cancel();
            this.m_tokenSource.Dispose();
        }
    }

    /// <inheritdoc/>
    protected virtual Task PublishMessageArrivedAsync(MqttArrivedMessage message)
    {
        if (this.MessageArrived != null)
        {
            var args = new MqttReceivedEventArgs(message);
            return this.MessageArrived(this, args);
        }
        else
        {
            return EasyTask.CompletedTask;
        }
    }

    #region 属性

    /// <inheritdoc/>
    public string Id { get; protected set; }

    /// <inheritdoc/>
    public bool Online { get; protected set; }

    /// <inheritdoc/>
    public CancellationTokenSource TokenSource => this.m_tokenSource;

    /// <inheritdoc/>
    public WaitHandlePool<MqttIdentifierMessage> WaitHandlePool => this.m_waitHandlePool;

    #endregion 属性

    #region Publish

    /// <inheritdoc/>
    public Task PublishAsync(MqttPublishMessage message, CancellationToken cancellationToken)
    {
        switch (message.QosLevel)
        {
            case QosLevel.AtLeastOnce:
                return this.PublishAtLeastOnceMessageAsync(message, cancellationToken);
            case QosLevel.ExactlyOnce:
                return this.PublishExactlyOnceMessageAsync(message, cancellationToken);
            case QosLevel.AtMostOnce:
            default:
                return this.PublishAtMostOnceMessageAsync(message, cancellationToken);
        }
    }

    private async Task PublishAtLeastOnceMessageAsync(MqttPublishMessage message, CancellationToken cancellationToken)
    {
        using (var waitDataAsync = this.m_waitHandlePool.GetWaitDataAsync(message))
        {
            await this.ProtectedOutputSendAsync(message, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            var waitDataStatus = await waitDataAsync.WaitAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            waitDataStatus.ThrowIfNotRunning();

            if (waitDataAsync.CompletedData is MqttPubAckMessage pubAckMessage)
            {
                return;
            }
        }
    }

    private async Task PublishAtMostOnceMessageAsync(MqttPublishMessage message, CancellationToken cancellationToken)
    {
        await this.ProtectedOutputSendAsync(message, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task PublishExactlyOnceMessageAsync(MqttPublishMessage message, CancellationToken cancellationToken)
    {
        using (var waitData_1_Async = this.m_waitHandlePool.GetWaitDataAsync(message))
        {
            await this.ProtectedOutputSendAsync(message, cancellationToken)
                .ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            var waitDataStatus = await waitData_1_Async.WaitAsync(cancellationToken)
                .ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            waitDataStatus.ThrowIfNotRunning();
        }


        var mqttPubRelMessage = new MqttPubRelMessage()
        {
            MessageId = message.MessageId
        };

        using (var waitData_2_Async = this.m_waitHandlePool.GetWaitDataAsync(mqttPubRelMessage, false))
        {
            await this.ProtectedOutputSendAsync(mqttPubRelMessage, cancellationToken)
                .ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            var waitDataStatus = await waitData_2_Async.WaitAsync(cancellationToken)
                .ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            waitDataStatus.ThrowIfNotRunning();
        }
    }

    #endregion Publish

    #region InputMqttMessage

    /// <inheritdoc/>
    public async Task InputMqttMessageAsync(MqttMessage mqttMessage, CancellationToken cancellationToken)
    {
        switch (mqttMessage)
        {
            case MqttConnectMessage message:
                this.Version = message.Version;
                await this.InputMqttConnectMessageAsync(message, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                break;

            case MqttConnAckMessage message:
                await this.InputMqttConnAckMessageAsync(message, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                break;

            case MqttPingReqMessage message:
                await this.InputMqttPingReqMessageAsync(message, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                break;

            case MqttPingRespMessage message:
                await this.InputMqttPingRespMessageAsync(message, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                break;

            case MqttPublishMessage message:
                await this.InputMqttPublishMessageAsync(message, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                break;

            case MqttPubAckMessage message:
                await this.InputMqttPubAckMessageAsync(message, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                break;

            case MqttPubRecMessage message:
                await this.InputMqttPubRecMessageAsync(message, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                break;

            case MqttPubRelMessage message:
                await this.InputMqttPubRelMessageAsync(message, CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                break;

            case MqttPubCompMessage message:
                await this.InputMqttPubCompMessageAsync(message, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                break;

            case MqttSubscribeMessage message:
                await this.InputMqttSubscribeMessageAsync(message, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                break;

            case MqttSubAckMessage message:
                await this.InputMqttSubAckMessageAsync(message, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                break;

            case MqttUnsubscribeMessage message:
                await this.InputMqttUnsubscribeMessageAsync(message, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                break;

            case MqttUnsubAckMessage message:
                await this.InputMqttUnsubAckMessageAsync(message, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                break;

            case MqttDisconnectMessage message:
                await this.InputMqttDisconnectMessageAsync(message, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 处理连接确认消息
    /// </summary>
    /// <param name="message">连接确认消息</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>任务</returns>
    protected abstract Task InputMqttConnAckMessageAsync(MqttConnAckMessage message, CancellationToken cancellationToken);

    /// <summary>
    /// 处理连接消息
    /// </summary>
    /// <param name="message">连接消息</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>任务</returns>
    protected abstract Task InputMqttConnectMessageAsync(MqttConnectMessage message, CancellationToken cancellationToken);

    /// <summary>
    /// 处理PING响应消息
    /// </summary>
    /// <param name="message">PING响应消息</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>任务</returns>
    protected abstract Task InputMqttPingRespMessageAsync(MqttPingRespMessage message, CancellationToken cancellationToken);

    /// <summary>
    /// 处理订阅消息
    /// </summary>
    /// <param name="message">订阅消息</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>任务</returns>
    protected abstract Task InputMqttSubscribeMessageAsync(MqttSubscribeMessage message, CancellationToken cancellationToken);

    /// <summary>
    /// 处理取消订阅消息
    /// </summary>
    /// <param name="message">取消订阅消息</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>任务</returns>
    protected abstract Task InputMqttUnsubscribeMessageAsync(MqttUnsubscribeMessage message, CancellationToken cancellationToken);

    /// <summary>
    /// 处理断开连接消息
    /// </summary>
    /// <param name="message">断开连接消息</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>任务</returns>
    private Task InputMqttDisconnectMessageAsync(MqttDisconnectMessage message, CancellationToken cancellationToken)
    {
        return this.ProtectedMqttOnClosing(message);
    }

    /// <summary>
    /// 处理PING请求消息
    /// </summary>
    /// <param name="message">PING请求消息</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>任务</returns>
    private async Task InputMqttPingReqMessageAsync(MqttPingReqMessage message, CancellationToken cancellationToken)
    {
        var contentForAck = new MqttPingRespMessage();
        await this.ProtectedOutputSendAsync(contentForAck, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 处理发布确认消息
    /// </summary>
    /// <param name="message">发布确认消息</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>任务</returns>
    private Task InputMqttPubAckMessageAsync(MqttPubAckMessage message, CancellationToken cancellationToken)
    {
        this.m_waitHandlePool.Set(message);
        return EasyTask.CompletedTask;
    }

    /// <summary>
    /// 处理发布完成消息
    /// </summary>
    /// <param name="message">发布完成消息</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>任务</returns>
    private Task InputMqttPubCompMessageAsync(MqttPubCompMessage message, CancellationToken cancellationToken)
    {
        this.m_waitHandlePool.Set(message);
        return EasyTask.CompletedTask;
    }

    /// <summary>
    /// 处理发布消息
    /// </summary>
    /// <param name="message">发布消息</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>任务</returns>
    private async Task InputMqttPublishMessageAsync(MqttPublishMessage message, CancellationToken cancellationToken)
    {
        //Console.WriteLine($"TopicName:{message.TopicName}");
        //Console.WriteLine($"Payload:{Encoding.UTF8.GetString(message.Payload.ToArray())}");

        if (message.QosLevel == QosLevel.AtMostOnce)
        {
            await this.PublishMessageArrivedAsync(new MqttArrivedMessage(message)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else if (message.QosLevel == QosLevel.AtLeastOnce)
        {
            await this.PublishMessageArrivedAsync(new MqttArrivedMessage(message)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            var pubAckMessage = new MqttPubAckMessage()
            {
                MessageId = message.MessageId
            };
            await this.ProtectedOutputSendAsync(pubAckMessage, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else if (message.QosLevel == QosLevel.ExactlyOnce)
        {
            this.m_qos2MqttArrivedMessage.Add(message.MessageId, new MqttArrivedMessage(message));
            var pubRecMessage = new MqttPubRecMessage()
            {
                MessageId = message.MessageId
            };
            await this.ProtectedOutputSendAsync(pubRecMessage, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    /// <summary>
    /// 处理发布接收消息
    /// </summary>
    /// <param name="message">发布接收消息</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>任务</returns>
    private Task InputMqttPubRecMessageAsync(MqttPubRecMessage message, CancellationToken cancellationToken)
    {
        this.m_waitHandlePool.Set(message);
        return EasyTask.CompletedTask;
    }

    /// <summary>
    /// 处理发布释放消息
    /// </summary>
    /// <param name="message">发布释放消息</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>任务</returns>
    private async Task InputMqttPubRelMessageAsync(MqttPubRelMessage message, CancellationToken cancellationToken)
    {
        if (this.m_qos2MqttArrivedMessage.TryGetValue(message.MessageId, out var mqttArrivedMessage))
        {
            this.m_qos2MqttArrivedMessage.Remove(message.MessageId);
            await this.PublishMessageArrivedAsync(mqttArrivedMessage).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            var pubCompMessage = new MqttPubCompMessage()
            {
                MessageId = message.MessageId
            };
            await this.ProtectedOutputSendAsync(pubCompMessage, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    /// <summary>
    /// 处理订阅确认消息
    /// </summary>
    /// <param name="message">订阅确认消息</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>任务</returns>
    private Task InputMqttSubAckMessageAsync(MqttSubAckMessage message, CancellationToken cancellationToken)
    {
        this.m_waitHandlePool.Set(message);
        return EasyTask.CompletedTask;
    }

    /// <summary>
    /// 处理取消订阅确认消息
    /// </summary>
    /// <param name="message">取消订阅确认消息</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>任务</returns>
    private Task InputMqttUnsubAckMessageAsync(MqttUnsubAckMessage message, CancellationToken cancellationToken)
    {
        this.m_waitHandlePool.Set(message);
        return EasyTask.CompletedTask;
    }

    #endregion InputMqttMessage

    #region 委托

    /// <inheritdoc/>
    public Func<MqttActor, MqttClosingEventArgs, Task> Closing { get; set; }
    /// <inheritdoc/>
    public Func<MqttActor, MqttConnectedEventArgs, Task> Connected { get; set; }
    /// <inheritdoc/>
    public Func<MqttActor, MqttConnectingEventArgs, Task> Connecting { get; set; }
    /// <inheritdoc/>
    public Func<MqttActor, MqttReceivedEventArgs, Task> MessageArrived { get; set; }
    /// <inheritdoc/>
    public Func<MqttActor, MqttMessage, CancellationToken, Task> OutputSendAsync { get; set; }
    /// <inheritdoc/>
    public MqttProtocolVersion Version { get; protected set; }

    #endregion 委托

    #region 委托方法

    /// <inheritdoc/>
    protected async Task ProtectedMqttOnClosing(MqttDisconnectMessage message)
    {
        if (this.Closing != null)
        {
            await this.Closing.Invoke(this, new MqttClosingEventArgs(message)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    /// <inheritdoc/>
    protected async Task ProtectedMqttOnConnected(MqttConnectedEventArgs e)
    {
        if (this.Connected != null)
        {
            await this.Connected.Invoke(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    /// <inheritdoc/>
    protected async Task ProtectedMqttOnConnecting(MqttConnectingEventArgs e)
    {
        if (this.Connecting != null)
        {
            await this.Connecting.Invoke(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    /// <inheritdoc/>
    protected Task ProtectedOutputSendAsync(MqttMessage message, CancellationToken cancellationToken)
    {
        if (message.MessageType == MqttMessageType.Connect)
        {
            this.Version = message.Version;
        }
        else
        {
            message.InternalSetVersion(this.Version);
        }
        return this.OutputSendAsync(this, message, cancellationToken);
    }

    #endregion 委托方法
}