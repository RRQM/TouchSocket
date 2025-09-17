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

using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets;

public class PipeTcpClient : TcpClientBase, IPipeTcpClient
{
    private readonly SemaphoreSlim m_semaphoreSlim = new SemaphoreSlim(1, 1);

    /// <inheritdoc/>
    public PipeReader Input => base.Transport.Reader;

    /// <inheritdoc/>
    public PipeWriter Output => base.Transport.Writer;

    /// <inheritdoc/>
    public async Task ConnectAsync(CancellationToken token)
    {
        await this.m_semaphoreSlim.WaitAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        try
        {
            await this.TcpConnectAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            this.m_semaphoreSlim.Release();
        }
    }

    /// <inheritdoc/>
    protected sealed override async Task ReceiveLoopAsync(ITransport transport)
    {
        var token = transport.ClosedToken;
        await Task.Delay(-1, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }
}