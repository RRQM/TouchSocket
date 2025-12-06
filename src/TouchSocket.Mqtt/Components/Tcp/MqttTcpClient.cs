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

public class MqttTcpClient : TcpClientBase, IMqttTcpClient
{
    private readonly MqttClientActor m_mqttActor;

    private MqttAdapter m_mqttAdapter;

    public MqttTcpClient()
    {
        var actor = new MqttClientActor
        {
            OutputSendAsync = this.PrivateMqttOnSend,
            Connecting = this.PrivateMqttOnConnecting,
            Connected = this.PrivateMqttOnConnected,
            Closing = this.PrivateMqttOnClosing,
            MessageArrived = this.PrivateMqttOnMessageArrived
        };
        this.m_mqttActor = actor;
    }

    #region MqttActor

    private async Task PrivateMqttOnClosing(MqttActor actor, MqttClosingEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(IMqttClosingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task PrivateMqttOnConnected(MqttActor mqttActor, MqttConnectedEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(IMqttConnectedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task PrivateMqttOnConnecting(MqttActor mqttActor, MqttConnectingEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(IMqttConnectingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task PrivateMqttOnMessageArrived(MqttActor actor, MqttReceivedEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(IMqttReceivedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task PrivateMqttOnSend(MqttActor mqttActor, MqttMessage message, CancellationToken cancellationToken)
    {
        if (message.MessageType == MqttMessageType.Connect)
        {
            this.m_mqttAdapter.Version = message.Version;
        }
        var locker = base.Transport.WriteLocker;
        await locker.WaitAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        try
        {
            var writer = new PipeBytesWriter(base.Transport.Writer);
            message.Build(ref writer);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            locker.Release();
        }
    }

    #endregion MqttActor

    /// <inheritdoc/>
    public override bool Online => base.Online && this.m_mqttActor.Online;

    /// <inheritdoc/>
    public override async Task<Result> CloseAsync(string msg, CancellationToken cancellationToken = default)
    {
        try
        {
            if (this.Online)
            {
                await this.m_mqttActor.DisconnectAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            await base.CloseAsync(msg, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            return Result.Success;
        }
        catch (System.Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <inheritdoc/>
    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (this.Online)
        {
            return;
        }

        var mqttConnectOptions = this.Config.GetValue(MqttConfigExtension.MqttConnectOptionsProperty);
        ThrowHelper.ThrowIfNull(mqttConnectOptions, nameof(mqttConnectOptions));

        var connectMessage = new MqttConnectMessage(mqttConnectOptions);

        await this.PluginManager.RaiseAsync(typeof(IMqttConnectingPlugin), this.Resolver, this, new MqttConnectingEventArgs(connectMessage, default));

        await base.TcpConnectAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        var connAckMessage = await this.m_mqttActor.ConnectAsync(connectMessage, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        if (connAckMessage.ReturnCode!= MqttReasonCode.ConnectionAccepted)
        {
            ThrowHelper.ThrowException($"Connection failed with reason: {connAckMessage.ReturnCode}，reasonString: {connAckMessage.ReasonString}");
        }
        await this.PluginManager.RaiseAsync(typeof(IMqttConnectedPlugin), this.Resolver, this, new MqttConnectedEventArgs(connectMessage, connAckMessage)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    public ValueTask<Result> PingAsync(CancellationToken cancellationToken = default)
    {
        return this.m_mqttActor.PingAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task PublishAsync(MqttPublishMessage mqttMessage, CancellationToken cancellationToken = default)
    {
        return this.m_mqttActor.PublishAsync(mqttMessage, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<MqttSubAckMessage> SubscribeAsync(MqttSubscribeMessage message, CancellationToken cancellationToken = default)
    {
        //订阅
        return this.m_mqttActor.SubscribeAsync(message, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<MqttUnsubAckMessage> UnsubscribeAsync(MqttUnsubscribeMessage message, CancellationToken cancellationToken = default)
    {
        return this.m_mqttActor.UnsubscribeAsync(message, cancellationToken);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosed(ClosedEventArgs e)
    {
        this.m_mqttActor.WaitHandlePool.CancelAll();

        await this.PluginManager.RaiseAsync(typeof(IMqttClosedPlugin), this.Resolver, this, new MqttClosedEventArgs(e.Manual, e.Message)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        await base.OnTcpClosed(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpConnecting(ConnectingEventArgs e)
    {
        await base.OnTcpConnecting(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        this.m_mqttAdapter = new MqttAdapter();
        this.SetAdapter(this.m_mqttAdapter);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        if (e.RequestInfo is MqttMessage mqttMessage)
        {
            await this.PluginManager.RaiseAsync(typeof(IMqttReceivingPlugin), this.Resolver, this, new MqttReceivingEventArgs(mqttMessage)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            await this.m_mqttActor.InputMqttMessageAsync(mqttMessage, CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        await base.OnTcpReceived(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }
}