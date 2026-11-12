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
/// 支持"发送并等待响应"的 Tcp 客户端。
/// </summary>
/// <remarks>
/// 等待的响应在 <see cref="OnTcpReceived(ReceivedDataEventArgs)"/> 中匹配，不匹配的数据照常进入 Received 与插件。
/// </remarks>
public class WaitingTcpClient : TcpClient, IWaitingResponseClient
{
    private readonly WaitingResponseCore m_waitingCore = new WaitingResponseCore();

    /// <inheritdoc/>
    public Task<ResponsedData> SendThenResponseAsync(ReadOnlyMemory<byte> memory, WaitingOptions waitingOptions = default, CancellationToken cancellationToken = default)
    {
        return this.m_waitingCore.SendThenResponseAsync(token => this.SendAsync(memory, token), waitingOptions?.FilterFuncAsync, default, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<ResponsedData> SendThenResponseAsync(IRequestInfo requestInfo, WaitingOptions waitingOptions = default, CancellationToken cancellationToken = default)
    {
        return this.m_waitingCore.SendThenResponseAsync(token => this.SendAsync(requestInfo, token), waitingOptions?.FilterFuncAsync, default, cancellationToken);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosed(ClosedEventArgs e)
    {
        this.m_waitingCore.Fail(new ClientNotConnectedException());
        await base.OnTcpClosed(e).ConfigureDefaultAwait();
    }

    /// <inheritdoc/>
    protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        if (await this.m_waitingCore.TryHandleAsync(default, e.Memory, e.RequestInfo).ConfigureDefaultAwait())
        {
            e.Handled = true;
            return;
        }
        await base.OnTcpReceived(e).ConfigureDefaultAwait();
    }
}
