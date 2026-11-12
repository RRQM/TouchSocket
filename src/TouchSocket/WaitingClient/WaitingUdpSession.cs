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

namespace TouchSocket.Sockets;

/// <summary>
/// 支持"发送并等待响应"的 Udp 会话。
/// </summary>
/// <remarks>
/// <para>
/// <see cref="WaitingOptions.RemoteIPHost"/> 不为 <see langword="null"/> 时，数据发送到该地址，且只接受来自该地址的响应；
/// 否则发送到默认远程地址，并接受来自任意地址的响应。
/// </para>
/// <para>等待的响应在 <see cref="OnUdpReceived(UdpReceivedDataEventArgs)"/> 中匹配，不匹配的数据照常进入 Received 与插件。</para>
/// </remarks>
public class WaitingUdpSession : UdpSession, IWaitingResponseClient
{
    private readonly WaitingResponseCore m_waitingCore = new WaitingResponseCore();

    /// <inheritdoc/>
    public Task<ResponsedData> SendThenResponseAsync(ReadOnlyMemory<byte> memory, WaitingOptions waitingOptions = default, CancellationToken cancellationToken = default)
    {
        var endPoint = waitingOptions?.RemoteIPHost?.EndPoint;
        return this.m_waitingCore.SendThenResponseAsync(
            token => endPoint == null ? this.SendAsync(memory, token) : this.SendAsync(endPoint, memory, token),
            waitingOptions?.FilterFuncAsync, endPoint, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<ResponsedData> SendThenResponseAsync(IRequestInfo requestInfo, WaitingOptions waitingOptions = default, CancellationToken cancellationToken = default)
    {
        var endPoint = waitingOptions?.RemoteIPHost?.EndPoint;
        return this.m_waitingCore.SendThenResponseAsync(
            token => endPoint == null ? this.SendAsync(requestInfo, token) : this.SendAsync(endPoint, requestInfo, token),
            waitingOptions?.FilterFuncAsync, endPoint, cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task<Result> StopAsync(CancellationToken cancellationToken = default)
    {
        this.m_waitingCore.Fail(new ClientNotConnectedException());
        return await base.StopAsync(cancellationToken).ConfigureDefaultAwait();
    }

    /// <inheritdoc/>
    protected override async Task OnUdpReceived(UdpReceivedDataEventArgs e)
    {
        if (await this.m_waitingCore.TryHandleAsync(e.EndPoint, e.Memory, e.RequestInfo).ConfigureDefaultAwait())
        {
            e.Handled = true;
            return;
        }
        await base.OnUdpReceived(e).ConfigureDefaultAwait();
    }
}
