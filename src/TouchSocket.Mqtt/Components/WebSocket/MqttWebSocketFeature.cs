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

using System.Net.WebSockets;
using TouchSocket.Http;

namespace TouchSocket.Mqtt;

/// <summary>
/// Mqtt WebSocket功能插件
/// </summary>
internal sealed class MqttWebSocketFeature : PluginBase, IHttpPlugin
{
    private readonly MqttBroker m_mqttBroker;
    private readonly MqttWebSocketFeatureOption m_options;

    /// <summary>
    /// 使用指定服务和配置选项初始化MqttWebSocketFeature
    /// </summary>
    /// <param name="service">Mqtt WebSocket服务</param>
    /// <param name="option">Mqtt WebSocket功能配置选项</param>
    public MqttWebSocketFeature(IMqttWebSocketService service, MqttWebSocketFeatureOption option)
    {
        this.m_mqttBroker = service.MqttBroker;
        this.m_options = option ?? throw new ArgumentNullException(nameof(option));
    }

    /// <inheritdoc/>
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        var verifyConnection = this.m_options.VerifyConnection;

        if (verifyConnection != null && !await verifyConnection.Invoke(client, e.Context).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
        {
            await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }
        e.Context.Response.Headers.Add("Sec-WebSocket-Protocol", "mqtt");
        var result = await client.SwitchProtocolToWebSocketAsync(e.Context).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        if (!result.IsSuccess)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            var session = new MqttWebSocketSessionClient(client, this.m_mqttBroker);
            try
            {
                await session.Start(client.ClosedToken);
            }
            finally
            {
                
            }
            
        });
    }
}