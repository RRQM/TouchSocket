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

public class MqttTcpSessionClient : TcpSessionClientBase, IMqttTcpSessionClient
{
    private readonly MqttBroker m_mqttBroker;
    private MqttSessionActor m_mqttActor;
    private bool m_cleanSession;

    public MqttTcpSessionClient(MqttBroker mqttBroker)
    {
        this.m_mqttBroker = mqttBroker;
    }

    #region MqttActor

    private async Task PrivateMqttOnClosing(MqttActor actor, MqttClosingEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(IMqttClosingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task PrivateMqttOnConnected(MqttActor mqttActor, MqttConnectedEventArgs args)
    {
        await this.PluginManager.RaiseAsync(typeof(IMqttConnectedPlugin), this.Resolver, this, args).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task PrivateMqttOnConnecting(MqttActor mqttActor, MqttConnectingEventArgs e)
    {
        if (e.ConnAckMessage.ReturnCode == MqttReasonCode.ConnectionAccepted)
        {
            await this.ResetIdAsync(e.ConnectMessage.ClientId, CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }

        await this.PluginManager.RaiseAsync(typeof(IMqttConnectingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task PrivateMqttOnMessageArrived(MqttActor actor, MqttReceivedEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(IMqttReceivedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task PrivateMqttOnSend(MqttActor mqttActor, MqttMessage message, CancellationToken cancellationToken)
    {
        var locker = base.Transport.WriteLocker;
        await locker.WaitAsync(cancellationToken);
        try
        {
            var writer = new PipeBytesWriter(base.Transport.Writer);
            message.Build(ref writer);
            await writer.FlushAsync(cancellationToken);
        }
        finally
        {
            locker.Release();
        }
    }

    #endregion MqttActor

    public bool CleanSession => this.m_cleanSession;
    public MqttSessionActor MqttActor => this.m_mqttActor;

    /// <inheritdoc/>
    protected override async Task OnTcpClosed(ClosedEventArgs e)
    {
        var mqttActor = this.m_mqttActor;
        if (mqttActor is not null)
        {
            mqttActor.OutputSendAsync = null;
            mqttActor.Connecting = null;
            mqttActor.Connected = null;
            mqttActor.MessageArrived = null;
            mqttActor.Closing = null;
            await mqttActor.Deactivate().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            if (this.CleanSession)
            {
                this.m_mqttBroker.RemoveMqttSessionActor(mqttActor);
            }
            await this.PluginManager.RaiseAsync(typeof(IMqttClosedPlugin), this.Resolver, this, new MqttClosedEventArgs(e.Manual, e.Message)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        await base.OnTcpClosed(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpConnecting(ConnectingEventArgs e)
    {
        await base.OnTcpConnecting(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        base.SetAdapter(new MqttAdapter());
    }

    /// <inheritdoc/>
    protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        if (e.RequestInfo is MqttMessage mqttMessage)
        {
            if (this.m_mqttActor is null)
            {
                if (mqttMessage is not MqttConnectMessage mqttConnectMessage)
                {
                    ThrowHelper.ThrowInvalidOperationException("在未初始化时，必须先进行初始化");
                    return;
                }

                this.m_cleanSession = mqttConnectMessage.CleanSession;
                var actor = this.m_mqttBroker.GetOrCreateMqttSessionActor(mqttConnectMessage.ClientId);

                actor.OutputSendAsync = this.PrivateMqttOnSend;
                actor.Connecting = this.PrivateMqttOnConnecting;
                actor.Connected = this.PrivateMqttOnConnected;
                actor.MessageArrived = this.PrivateMqttOnMessageArrived;
                actor.Closing = this.PrivateMqttOnClosing;
                actor.Activate(mqttConnectMessage);
                this.m_mqttActor = actor;
            }

            await this.PluginManager.RaiseIMqttReceivingPluginAsync(this.Resolver, this, new MqttReceivingEventArgs(mqttMessage)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            await this.m_mqttActor.InputMqttMessageAsync(mqttMessage, CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        await base.OnTcpReceived(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }
}