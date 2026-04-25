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

/// <summary>
/// 基于 TCP 的 Mqtt 会话客户端。
/// </summary>
public class MqttTcpSessionClient : TcpSessionClientBase, IMqttTcpSessionClient
{
    private readonly MqttBroker m_mqttBroker;
    private bool m_cleanSession;
    private MqttSessionActor m_mqttActor;

    /// <summary>
    /// 初始化 <see cref="MqttTcpSessionClient"/> 类的新实例。
    /// </summary>
    /// <param name="mqttBroker">Mqtt 中介实例。</param>
    public MqttTcpSessionClient(MqttBroker mqttBroker)
    {
        this.m_mqttBroker = mqttBroker;
    }

    #region MqttActor

    private async Task PrivateMqttOnClosing(MqttActor actor, MqttClosingEventArgs e)
    {
        await this.PluginManager.RaiseIMqttClosingPluginAsync(this.Resolver, this, e).ConfigureDefaultAwait();
    }

    private async Task PrivateMqttOnConnected(MqttActor mqttActor, MqttConnectedEventArgs args)
    {
        await this.PluginManager.RaiseIMqttConnectedPluginAsync(this.Resolver, this, args).ConfigureDefaultAwait();
    }

    private async Task PrivateMqttOnConnecting(MqttActor mqttActor, MqttConnectingEventArgs e)
    {
        if (e.ConnAckMessage.ReturnCode == MqttReasonCode.ConnectionAccepted)
        {
            await this.ResetIdAsync(e.ConnectMessage.ClientId, CancellationToken.None).ConfigureDefaultAwait();
        }

        await this.PluginManager.RaiseIMqttConnectingPluginAsync(this.Resolver, this, e).ConfigureDefaultAwait();
    }

    private async Task PrivateMqttOnMessageArrived(MqttActor actor, MqttReceivedEventArgs e)
    {
        await this.PluginManager.RaiseIMqttReceivedPluginAsync(this.Resolver, this, e).ConfigureDefaultAwait();
    }

    private async Task PrivateMqttOnMessageDiscarded(MqttSessionActor actor, MqttMessageDiscardedEventArgs e)
    {
        await this.PluginManager.RaiseIMqttMessageDiscardedPluginAsync(this.Resolver, this, e).ConfigureDefaultAwait();
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

    /// <summary>
    /// 获取是否清除会话。
    /// </summary>
    public bool CleanSession => this.m_cleanSession;

    /// <summary>
    /// 获取 Mqtt 会话 Actor 实例。
    /// </summary>
    public MqttActor MqttActor => this.m_mqttActor;

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
            mqttActor.MessageDiscarded = null;
            mqttActor.Closing = null;
            await mqttActor.Deactivate().ConfigureDefaultAwait();
            if (this.CleanSession)
            {
                this.m_mqttBroker.RemoveMqttSessionActor(mqttActor.Id);
            }
            await this.PluginManager.RaiseIMqttClosedPluginAsync(this.Resolver, this, new MqttClosedEventArgs(e.Message)).ConfigureDefaultAwait();
        }
        await base.OnTcpClosed(e).ConfigureDefaultAwait();
    }

    /// <inheritdoc/>
    protected override async Task OnTcpConnecting(ConnectingEventArgs e)
    {
        await base.OnTcpConnecting(e).ConfigureDefaultAwait();
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
                actor.MessageDiscarded = this.PrivateMqttOnMessageDiscarded;
                actor.Closing = this.PrivateMqttOnClosing;
                actor.Activate(mqttConnectMessage);
                this.m_mqttActor = actor;
            }

            await this.PluginManager.RaiseIMqttReceivingPluginAsync(this.Resolver, this, new MqttReceivingEventArgs(mqttMessage)).ConfigureDefaultAwait();

            await this.m_mqttActor.InputMqttMessageAsync(mqttMessage, CancellationToken.None).ConfigureDefaultAwait();
        }
        await base.OnTcpReceived(e).ConfigureDefaultAwait();
    }
}