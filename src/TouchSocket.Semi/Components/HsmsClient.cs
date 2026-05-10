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

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Semi;

/// <summary>
/// 基于 TCP 的 HSMS 客户端，实现 SEMI E37 的主动连接端（Active）。
/// </summary>
public class HsmsClient : TcpClientBase, IHsmsClient
{
    private readonly WaitHandlePool<HsmsMessage> m_waitHandlePool = new WaitHandlePool<HsmsMessage>();

    /// <inheritdoc/>
    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        await this.TcpConnectAsync(cancellationToken).ConfigureDefaultAwait();

        var selectReq = HsmsMessage.CreateSelectRequest();
        var selectRsp = await this.SendHsmsMessageAsync(selectReq, cancellationToken).ConfigureDefaultAwait();
        var status = (SelectStatus)(selectRsp?.F ?? (byte)SelectStatus.NotReady);
        TouchSocketSemiThrowHelper.ThrowIfNotSuccess(status);
    }

    /// <inheritdoc/>
    public async Task<HsmsMessage?> SendHsmsMessageAsync(HsmsMessage message, CancellationToken cancellationToken = default)
    {
        if (!message.ReplyExpected)
        {
            await this.PrivateSendAsync(message, cancellationToken).ConfigureDefaultAwait();
            return null;
        }

        var waitData = this.m_waitHandlePool.GetWaitDataAsync(message);
        try
        {
            await this.PrivateSendAsync(message, cancellationToken).ConfigureDefaultAwait();

            var status = await waitData.WaitAsync(cancellationToken).ConfigureDefaultAwait();
            status.ThrowIfNotRunning();
            return waitData.CompletedData;
        }
        finally
        {
            waitData.Dispose();
        }
    }

    /// <inheritdoc/>
    protected override async Task OnTcpConnecting(ConnectingEventArgs e)
    {
        await base.OnTcpConnecting(e).ConfigureDefaultAwait();
        this.SetAdapter(new HsmsAdapter());
        await this.OnHsmsConnecting(e).ConfigureDefaultAwait();
    }

    /// <inheritdoc/>
    protected override async Task OnTcpConnected(ConnectedEventArgs e)
    {
        await base.OnTcpConnected(e).ConfigureDefaultAwait();
        await this.OnHsmsConnected(e).ConfigureDefaultAwait();
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosing(ClosingEventArgs e)
    {
        await base.OnTcpClosing(e).ConfigureDefaultAwait();
        await this.OnHsmsClosing(e).ConfigureDefaultAwait();
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosed(ClosedEventArgs e)
    {
        this.m_waitHandlePool.CancelAll();
        await base.OnTcpClosed(e).ConfigureDefaultAwait();
        await this.OnHsmsClosed(e).ConfigureDefaultAwait();
    }

    /// <inheritdoc/>
    protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        if (e.RequestInfo is HsmsMessage message)
        {
            switch (message.MessageType)
            {
                case HsmsMessageType.DataMessage:
                    await this.OnHsmsReceived(new HsmsReceivedEventArgs(message)).ConfigureDefaultAwait();
                    break;
                case HsmsMessageType.SelectRequest:
                    await this.SendHsmsSelectResponseAsync(message).ConfigureDefaultAwait();
                    break;
                case HsmsMessageType.SelectResponse:
                case HsmsMessageType.DeselectResponse:
                case HsmsMessageType.LinkTestResponse:
                    this.m_waitHandlePool.Set(message);
                    break;
                case HsmsMessageType.LinkTestRequest:
                    await this.SendHsmsLinkTestResponseAsync(message).ConfigureDefaultAwait();
                    break;
                case HsmsMessageType.SeparateRequest:
                    _ = this.CloseAsync("Received SeparateRequest");
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 在即将完成连接时触发。
    /// <para>覆盖此方法将不会触发 <see cref="IHsmsConnectingPlugin"/> 插件。</para>
    /// </summary>
    /// <param name="e">连接事件参数。</param>
    protected virtual async Task OnHsmsConnecting(ConnectingEventArgs e)
    {
        await this.PluginManager.RaiseIHsmsConnectingPluginAsync(this.Resolver, this, e).ConfigureDefaultAwait();
    }

    /// <summary>
    /// 在连接完成后触发。
    /// <para>覆盖此方法将不会触发 <see cref="IHsmsConnectedPlugin"/> 插件。</para>
    /// </summary>
    /// <param name="e">连接事件参数。</param>
    protected virtual async Task OnHsmsConnected(ConnectedEventArgs e)
    {
        await this.PluginManager.RaiseIHsmsConnectedPluginAsync(this.Resolver, this, e).ConfigureDefaultAwait();
    }

    /// <summary>
    /// 在即将断开连接时触发（仅主动断开时有效）。
    /// <para>覆盖此方法将不会触发 <see cref="IHsmsClosingPlugin"/> 插件。</para>
    /// </summary>
    /// <param name="e">断开事件参数。</param>
    protected virtual async Task OnHsmsClosing(ClosingEventArgs e)
    {
        await this.PluginManager.RaiseIHsmsClosingPluginAsync(this.Resolver, this, e).ConfigureDefaultAwait();
    }

    /// <summary>
    /// 在断开连接后触发。
    /// <para>覆盖此方法将不会触发 <see cref="IHsmsClosedPlugin"/> 插件。</para>
    /// </summary>
    /// <param name="e">断开事件参数。</param>
    protected virtual async Task OnHsmsClosed(ClosedEventArgs e)
    {
        await this.PluginManager.RaiseIHsmsClosedPluginAsync(this.Resolver, this, e).ConfigureDefaultAwait();
    }

    /// <summary>
    /// 在收到 HSMS 数据消息时触发。
    /// <para>覆盖此方法将不会触发 <see cref="IHsmsReceivedPlugin"/> 插件。</para>
    /// </summary>
    /// <param name="e">包含接收到的 <see cref="HsmsMessage"/> 的事件参数。</param>
    protected virtual async Task OnHsmsReceived(HsmsReceivedEventArgs e)
    {
        await this.PluginManager.RaiseIHsmsReceivedPluginAsync(this.Resolver, this, e).ConfigureDefaultAwait();
    }

    private async Task SendHsmsSelectResponseAsync(HsmsMessage request)
    {
        var response = HsmsMessage.CreateSelectResponse(request.SystemBytes);
        await this.PrivateSendAsync(response, CancellationToken.None).ConfigureDefaultAwait();
    }

    private async Task SendHsmsLinkTestResponseAsync(HsmsMessage request)
    {
        var response = HsmsMessage.CreateLinkTestResponse(request.SystemBytes);
        await this.PrivateSendAsync(response, CancellationToken.None).ConfigureDefaultAwait();
    }

    private Task PrivateSendAsync(HsmsMessage message, CancellationToken cancellationToken)
    {
        return base.ProtectedSendAsync(message, cancellationToken);
    }
}
