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

using TouchSocket.NamedPipe;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace TouchSocket.Mcp;

/// <summary>
/// MCP 命名管道服务端的会话客户端，每个连接独立处理 MCP 请求。
/// </summary>
public class McpNamedPipeSessionClient : NamedPipeSessionClient
{
    private McpActor m_actor;

    /// <inheritdoc/>
    protected override async Task OnInitialized()
    {
        await base.OnInitialized().ConfigureDefaultAwait();
        this.m_actor = new McpActor(this.Config.GetValue(McpConfigExtension.McpServerOptionsProperty))
        {
            Resolver = this.Resolver,
            Logger = this.Logger,
            SendAction = this.SendAsync
        };
        this.m_actor.SetRpcServerProvider(this.Resolver.Resolve<IRpcServerProvider>());
    }

    /// <inheritdoc/>
    protected override Task OnNamedPipeConnecting(ConnectingEventArgs e)
    {
        this.SetAdapter(new TerminatorPackageAdapter("\n"));
        return base.OnNamedPipeConnecting(e);
    }

    /// <inheritdoc/>
    protected override async Task OnNamedPipeReceived(ReceivedDataEventArgs e)
    {
        if (!e.Memory.IsEmpty)
        {
            var callContext = new McpNamedPipeCallContext(this, this.ClosedToken);
            await this.m_actor.InputReceiveAsync(e.Memory, callContext).ConfigureDefaultAwait();
        }
        await base.OnNamedPipeReceived(e).ConfigureDefaultAwait();
    }

    /// <inheritdoc/>
    protected override async Task OnNamedPipeClosed(ClosedEventArgs e)
    {
        try
        {
            await base.OnNamedPipeClosed(e).ConfigureDefaultAwait();
        }
        finally
        {
            this.m_actor.SafeDispose();
        }
    }

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            this.m_actor.SafeDispose();
        }
        base.SafetyDispose(disposing);
    }
}
