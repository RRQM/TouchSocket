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

using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using TouchSocket.Http.WebSockets;

namespace TouchSocket.Mqtt;

public class MqttWebSocketClient : SetupClientWebSocket, IMqttWebSocketClient
{
    private readonly MqttClientActor m_mqttActor;
    private readonly MqttAdapter m_mqttAdapter = new MqttAdapter();
    private readonly SegmentedPipe m_pipe = new SegmentedPipe();
    private readonly SemaphoreSlim m_semaphoreSlimConnect = new SemaphoreSlim(1, 1);

    public MqttWebSocketClient()
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

    /// <inheritdoc/>
    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        await this.m_semaphoreSlimConnect.WaitAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        try
        {
            if (this.Online)
            {
                return;
            }

            var mqttConnectOptions = this.Config.GetValue(MqttConfigExtension.MqttConnectOptionsProperty);
            ThrowHelper.ThrowIfNull(mqttConnectOptions, nameof(mqttConnectOptions));

            var connectMessage = new MqttConnectMessage(mqttConnectOptions);

            await this.PluginManager.RaiseAsync(typeof(IMqttConnectingPlugin), this.Resolver, this, new MqttConnectingEventArgs(connectMessage, default));

            await base.WebSocketConnectAsync(cancellationToken, option =>
            {
                option.AddSubProtocol("mqtt");
            }).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            var connAckMessage = await this.m_mqttActor.ConnectAsync(connectMessage, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            if (connAckMessage.ReturnCode != MqttReasonCode.ConnectionAccepted)
            {
                ThrowHelper.ThrowException($"Connection failed with reason: {connAckMessage.ReturnCode}，reasonString: {connAckMessage.ReasonString}");
            }
            await this.PluginManager.RaiseAsync(typeof(IMqttConnectedPlugin), this.Resolver, this, new MqttConnectedEventArgs(connectMessage, connAckMessage)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            this.m_semaphoreSlimConnect.Release();
        }
    }

    public ValueTask<Result> PingAsync(CancellationToken cancellationToken = default)
    {
        return this.m_mqttActor.PingAsync(cancellationToken);
    }

    public Task PublishAsync(MqttPublishMessage mqttMessage, CancellationToken cancellationToken = default)
    {
        return this.m_mqttActor.PublishAsync(mqttMessage, cancellationToken);
    }

    public Task<MqttSubAckMessage> SubscribeAsync(MqttSubscribeMessage message, CancellationToken cancellationToken = default)
    {
        return this.m_mqttActor.SubscribeAsync(message, cancellationToken);
    }

    public Task<MqttUnsubAckMessage> UnsubscribeAsync(MqttUnsubscribeMessage message, CancellationToken cancellationToken = default)
    {
        return this.m_mqttActor.UnsubscribeAsync(message, cancellationToken);
    }

    protected override async Task OnWebSocketReceived(WebSocketMessageType messageType, ReadOnlySequence<byte> sequenceSrc)
    {
        if (messageType != WebSocketMessageType.Binary)
        {
            return;
        }

        foreach (var item in sequenceSrc)
        {
            this.m_pipe.Writer.Write(item.Span);
        }

        while (m_pipe.Count > 0)
        {
            var readResult = m_pipe.Reader.Read();

            var sequence = readResult.Buffer;

            if (sequence.Length > 0)
            {
                var reader = new BytesReader(sequence);
                if (!this.m_mqttAdapter.TryParseRequest(ref reader, out var mqttMessage))
                {
                    break;
                }
                await this.ProcessMqttMessage(mqttMessage);

                m_pipe.Reader.AdvanceTo(sequence.GetPosition(reader.BytesRead));
            }

            if (readResult.IsCompleted)
            {
                break;
            }
        }
    }

    private async Task ProcessMqttMessage(MqttMessage mqttMessage)
    {
        await this.PluginManager.RaiseAsync(typeof(IMqttReceivingPlugin), this.Resolver, this, new MqttReceivingEventArgs(mqttMessage)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        await this.m_mqttActor.InputMqttMessageAsync(mqttMessage, CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #region MqttActor

    private readonly SemaphoreSlim m_semaphoreSlimSend = new SemaphoreSlim(1, 1);

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
        var locker = m_semaphoreSlimSend;
        await locker.WaitAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        var writer = new SegmentedBytesWriter();
        try
        {
            message.Build(ref writer);
            await this.SendAsync(writer.Sequence, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            writer.Dispose();
            locker.Release();
        }
    }

    #endregion MqttActor

    private async Task SendAsync(ReadOnlySequence<byte> sequence, CancellationToken cancellationToken)
    {
        if (sequence.IsSingleSegment)
        {
            await this.ProtectedSendAsync(sequence.First, WebSocketMessageType.Binary, true, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else
        {
            var enumerator = sequence.GetEnumerator();
            var hasNext = enumerator.MoveNext();
            while (hasNext)
            {
                var current = enumerator.Current;
                hasNext = enumerator.MoveNext();
                await this.ProtectedSendAsync(current, WebSocketMessageType.Binary, !hasNext, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }
    }
}