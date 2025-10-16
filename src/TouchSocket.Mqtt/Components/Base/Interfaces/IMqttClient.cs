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

using TouchSocket.Sockets;

namespace TouchSocket.Mqtt;

/// <summary>
/// 表示一个Mqtt客户端接口。
/// </summary>
public interface IMqttClient : IMqttSession, IConnectableClient
{
    /// <summary>
    /// 发送Ping请求。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>操作结果。</returns>
    ValueTask<Result> PingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 发布Mqtt消息。
    /// </summary>
    /// <param name="mqttMessage">要发布的消息。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>异步任务。</returns>
    Task PublishAsync(MqttPublishMessage mqttMessage, CancellationToken cancellationToken = default);

    /// <summary>
    /// 订阅主题。
    /// </summary>
    /// <param name="message">订阅消息。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>订阅确认消息。</returns>
    Task<MqttSubAckMessage> SubscribeAsync(MqttSubscribeMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// 取消订阅主题。
    /// </summary>
    /// <param name="message">取消订阅消息。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>取消订阅确认消息。</returns>
    Task<MqttUnsubAckMessage> UnsubscribeAsync(MqttUnsubscribeMessage message, CancellationToken cancellationToken = default);
}
