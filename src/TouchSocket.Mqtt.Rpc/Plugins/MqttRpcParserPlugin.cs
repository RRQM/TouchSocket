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

using TouchSocket.Rpc;

namespace TouchSocket.Mqtt.Rpc;

/// <summary>
/// MqttRpc 解析器插件。在 <see cref="IMqttClient"/> 连接后自动订阅响应主题，
/// 并在接收到消息时将请求路由到 RPC 方法或将响应路由到等待的调用。
/// </summary>
[PluginOption(Singleton = true)]
public sealed class MqttRpcParserPlugin : PluginBase, IMqttConnectedPlugin, IMqttReceivedPlugin
{
    private readonly MqttRpcOption m_option;

    /// <summary>
    /// 初始化 <see cref="MqttRpcParserPlugin"/> 的新实例。
    /// </summary>
    /// <param name="rpcServerProvider">RPC 服务器提供者，为 <see langword="null"/> 时表示纯客户端模式。</param>
    /// <param name="option">MqttRpc 配置选项。</param>
    public MqttRpcParserPlugin(IRpcServerProvider rpcServerProvider, MqttRpcOption option)
    {
        this.m_option = option;
        this.RpcServerProvider = rpcServerProvider;
        this.SerializerOptions = option.SerializerOptions;

        if (rpcServerProvider is not null)
        {
            MqttRpcActor.AddRpcToMap(rpcServerProvider, this.ActionMap);
        }
    }

    /// <summary>
    /// 获取动作映射。
    /// </summary>
    public ActionMap ActionMap { get; } = new ActionMap(true);

    /// <summary>
    /// 获取 RPC 服务器提供者。
    /// </summary>
    public IRpcServerProvider RpcServerProvider { get; }

    /// <summary>
    /// 获取序列化选项。
    /// </summary>
    public System.Text.Json.JsonSerializerOptions SerializerOptions { get; }

    /// <inheritdoc/>
    public async Task OnMqttConnected(IMqttSession client, MqttConnectedEventArgs e)
    {
        if (client is IMqttClient mqttClient)
        {
            var uniqueId = Guid.NewGuid().ToString("N");
            var responseTopic = $"{this.m_option.ResponseTopicPrefix}/{uniqueId}";
            var qosLevel = this.m_option.QosLevel;
            var requestTopic = this.m_option.RequestTopic;

            var actor = new MqttRpcActor
            {
                SerializerOptions = this.SerializerOptions,
                Resolver = client.Resolver,
                ResponseTopic = responseTopic,
                Logger = client.Logger,
                SendRequestAction = (payload, ct) => mqttClient.PublishAsync(
                    new MqttPublishMessage(requestTopic, false, qosLevel, payload), ct),
                SendResponseAction = (topic, payload, ct) => mqttClient.PublishAsync(
                    new MqttPublishMessage(topic, false, qosLevel, payload), ct),
            };

            if (this.RpcServerProvider is not null)
            {
                actor.SetRpcServerProvider(this.RpcServerProvider, this.ActionMap);
            }

            client.SetValue(MqttRpcClientExtension.MqttRpcActorProperty, actor);

            var subscribeRequests = new List<SubscribeRequest>
            {
                new SubscribeRequest($"{this.m_option.ResponseTopicPrefix}/+", qosLevel),
            };

            if (this.RpcServerProvider is not null)
            {
                subscribeRequests.Add(new SubscribeRequest(requestTopic, qosLevel));
            }

            await mqttClient.SubscribeAsync(new MqttSubscribeMessage(subscribeRequests.ToArray()), CancellationToken.None).ConfigureDefaultAwait();
        }

        await e.InvokeNext().ConfigureDefaultAwait();
    }

    /// <inheritdoc/>
    public async Task OnMqttReceived(IMqttSession client, MqttReceivedEventArgs e)
    {
        if (client.TryGetValue(MqttRpcClientExtension.MqttRpcActorProperty, out var actor))
        {
            var msg = e.MqttMessage;
            var topic = msg.TopicName;
            ReadOnlyMemory<byte> payload;
            if (msg.Payload.IsSingleSegment)
            {
                payload = msg.Payload.First;
            }
            else
            {
                var arr = new byte[(int)msg.Payload.Length];
                var offset = 0;
                foreach (var segment in msg.Payload)
                {
                    segment.Span.CopyTo(new Span<byte>(arr, offset, segment.Length));
                    offset += segment.Length;
                }
                payload = arr;
            }

            if (topic.StartsWith(this.m_option.ResponseTopicPrefix, StringComparison.Ordinal))
            {
                await actor.InputReceiveAsync(payload, null).ConfigureDefaultAwait();
                return;
            }
            else if (topic == this.m_option.RequestTopic && this.RpcServerProvider is not null)
            {
                await actor.InputReceiveAsync(payload, new MqttRpcCallContext(client, client.ClosedToken)).ConfigureDefaultAwait();
                return;
            }
        }

        await e.InvokeNext().ConfigureDefaultAwait();
    }
}
