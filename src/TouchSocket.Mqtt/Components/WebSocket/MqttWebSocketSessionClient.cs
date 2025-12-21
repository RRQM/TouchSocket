// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Sockets;

namespace TouchSocket.Mqtt;

internal class MqttWebSocketSessionClient : RoomDependencyObject, IMqttWebSocketSessionClient
{
    private readonly MqttAdapter m_adapter = new MqttAdapter();
    private readonly IHttpSessionClient m_client;
    private readonly MqttBroker m_mqttBroker;
    private readonly IWebSocket m_webSocket;
    private bool m_cleanSession;

    private MqttSessionActor m_mqttActor = null;

    public MqttWebSocketSessionClient(IHttpSessionClient httpSessionClient, MqttBroker mqttBroker) : base(httpSessionClient)
    {
        this.m_client = httpSessionClient;
        this.m_mqttBroker = mqttBroker;
        this.m_adapter.Config(this.m_client.Config);
        this.m_webSocket = this.m_client.WebSocket;
        this.m_webSocket.AllowAsyncRead = true;
    }

    public IHttpSessionClient Client => this.m_client;
    public CancellationToken ClosedToken => this.m_client.ClosedToken;
    public TouchSocketConfig Config => this.m_client.Config;
    public bool IsClient => false;
    public DateTimeOffset LastReceivedTime => this.m_client.LastReceivedTime;
    public DateTimeOffset LastSentTime => this.m_client.LastSentTime;
    public ILog Logger => this.m_client.Logger;
    public bool Online => this.m_webSocket.Online;
    public IPluginManager PluginManager => this.m_client.PluginManager;
    public Protocol Protocol => new Protocol("mqtt");
    public IResolver Resolver => this.m_client.Resolver;

    public async Task Start(CancellationToken cancellationToken)
    {
        using var webSocket = this.m_webSocket;
        var messageCombinator = webSocket.GetMessageCombinator();
        using var pipe = new SegmentedPipe();
        while (true)
        {
            using (var receiveResult = await webSocket.ReadAsync(cancellationToken))
            {
                var dataFrame = receiveResult.DataFrame;
                if (!messageCombinator.TryCombine(dataFrame, out var webSocketMessage))
                {
                    continue;
                }

                using (webSocketMessage)
                {
                    if (webSocketMessage.Opcode == WSDataType.Binary)
                    {
                        foreach (var item in webSocketMessage.PayloadSequence)
                        {
                            pipe.Writer.Write(item.Span);
                        }

                        while (pipe.Count > 0)
                        {
                            var readResult = pipe.Reader.Read();

                            var sequence = readResult.Buffer;

                            if (sequence.Length > 0)
                            {
                                var reader = new BytesReader(sequence);
                                if (!this.m_adapter.TryParseRequest(ref reader, out var mqttMessage))
                                {
                                    break;
                                }
                                await this.ProcessMqttMessage(mqttMessage);

                                pipe.Reader.AdvanceTo(sequence.GetPosition(reader.BytesRead));
                            }

                            if (readResult.IsCompleted)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private async Task ProcessMqttMessage(MqttMessage mqttMessage)
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

    #region MqttActor

    private readonly SemaphoreSlim m_semaphoreSlimSend = new SemaphoreSlim(1, 1);

    public Task<Result> CloseAsync(string msg, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

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
            await this.m_client.ResetIdAsync(e.ConnectMessage.ClientId, CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }

        await this.PluginManager.RaiseAsync(typeof(IMqttConnectingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task PrivateMqttOnMessageArrived(MqttActor actor, MqttReceivedEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(IMqttReceivedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task PrivateMqttOnSend(MqttActor mqttActor, MqttMessage message, CancellationToken cancellationToken)
    {
        var locker = this.m_semaphoreSlimSend;
        await locker.WaitAsync(cancellationToken);
        var writer = new SegmentedBytesWriter();
        try
        {
            message.Build(ref writer);
            await this.m_webSocket.SendAsync(writer.Sequence, WSDataType.Binary, cancellationToken);
        }
        finally
        {
            writer.Dispose();
            locker.Release();
        }
    }

    #endregion MqttActor
}