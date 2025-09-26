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

namespace TouchSocket.Http.WebSockets;

/// <summary>
/// 基于Http的WebSocket的扩展。
/// <para>此组件只能挂载在<see cref="HttpService"/>中</para>
/// </summary>
[PluginOption(Singleton = true)]
public sealed class WebSocketFeature : PluginBase, IHttpPlugin
{
    /// <summary>
    /// 自动响应Close报文
    /// </summary>
    public static readonly DependencyProperty<bool> AutoCloseProperty =
       new("AutoClose", true);

    /// <summary>
    /// 自动响应Ping报文
    /// </summary>
    public static readonly DependencyProperty<bool> AutoPongProperty =
       new("AutoPong", false);

    private readonly WebSocketFeatureOptions m_options;

    /// <summary>
    /// 使用指定配置选项初始化WebSocketFeature
    /// </summary>
    /// <param name="options">WebSocket功能配置选项</param>
    public WebSocketFeature(WebSocketFeatureOptions options)
    {
        this.m_options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// 获取配置选项的只读副本
    /// </summary>
    public WebSocketFeatureOptions Options => this.m_options;

    /// <inheritdoc/>
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        if (client.Protocol == Protocol.Http)
        {
            var verifyConnection = this.m_options.VerifyConnection;

            if (await verifyConnection.Invoke(client, e.Context).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
            {
                e.Handled = true;
                var result = await client.SwitchProtocolToWebSocketAsync(e.Context).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                if (!result.IsSuccess)
                {
                    client.Logger?.Debug(this, result.Message);
                    return;
                }

                if (!this.m_options.AutoClose)
                {
                    client.SetValue(AutoCloseProperty, false);
                }
                if (this.m_options.AutoPong)
                {
                    client.SetValue(AutoPongProperty, true);
                }
                return;
            }
        }
        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }
}