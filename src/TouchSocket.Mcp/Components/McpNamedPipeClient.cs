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

using Microsoft.Extensions.AI;
using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace TouchSocket.Mcp;

/// <summary>
/// 基于命名管道的 MCP 客户端，使用换行符分隔 UTF-8 JSON 消息。
/// </summary>
public class McpNamedPipeClient : NamedPipeClientBase, IMcpNamedPipeClient
{
    private readonly McpClientBaseImpl m_clientBase;

    /// <summary>
    /// 初始化 <see cref="McpNamedPipeClient"/> 的新实例。
    /// </summary>
    public McpNamedPipeClient()
    {
        this.m_clientBase = new McpClientBaseImpl(this);
    }

    /// <inheritdoc/>
    public Task ConnectAsync(CancellationToken cancellationToken)
    {
        return this.NamedPipeConnectAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpInitializeResult> InitializeAsync(CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync(token => this.m_clientBase.InitializeAsync(token), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListToolsResult> ListToolsAsync(CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync(token => this.m_clientBase.ListToolsAsync(token), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListToolsResult> ListToolsAsync(string cursor, CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync(token => this.m_clientBase.ListToolsAsync(cursor, token), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListToolsResult> ListAllToolsAsync(CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync(token => this.m_clientBase.ListAllToolsAsync(token), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<List<AIFunction>> ListAllAIFunctionsAsync(CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync(token => this.m_clientBase.ListAllAIFunctionsAsync(token), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<List<AITool>> ListAllAIToolsAsync(CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync(token => this.m_clientBase.ListAllAIToolsAsync(token), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpCallToolResult> CallToolAsync(string name, Dictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync(token => this.m_clientBase.CallToolAsync(name, arguments, token), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListResourcesResult> ListResourcesAsync(CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync(token => this.m_clientBase.ListResourcesAsync(token), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListResourcesResult> ListResourcesAsync(string cursor, CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync(token => this.m_clientBase.ListResourcesAsync(cursor, token), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListResourcesResult> ListAllResourcesAsync(CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync(token => this.m_clientBase.ListAllResourcesAsync(token), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListResourceTemplatesResult> ListResourceTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync(token => this.m_clientBase.ListResourceTemplatesAsync(token), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListResourceTemplatesResult> ListResourceTemplatesAsync(string cursor, CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync(token => this.m_clientBase.ListResourceTemplatesAsync(cursor, token), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListResourceTemplatesResult> ListAllResourceTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync(token => this.m_clientBase.ListAllResourceTemplatesAsync(token), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpReadResourceResult> ReadResourceAsync(string uri, CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync(token => this.m_clientBase.ReadResourceAsync(uri, token), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListPromptsResult> ListPromptsAsync(CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync(token => this.m_clientBase.ListPromptsAsync(token), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListPromptsResult> ListPromptsAsync(string cursor, CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync(token => this.m_clientBase.ListPromptsAsync(cursor, token), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListPromptsResult> ListAllPromptsAsync(CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync(token => this.m_clientBase.ListAllPromptsAsync(token), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpGetPromptResult> GetPromptAsync(string name, Dictionary<string, string> arguments = null, CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync(token => this.m_clientBase.GetPromptAsync(name, arguments, token), cancellationToken);
    }

    /// <inheritdoc/>
    protected override void LoadConfig(TouchSocketConfig config)
    {
        base.LoadConfig(config);
        var options = config.GetValue(McpConfigExtension.McpClientOptionsProperty);
        if (options?.JsonSerializerOptions.WriteIndented == true)
        {
            throw new InvalidOperationException("MCP named pipe messages require compact JSON. WriteIndented must be false.");
        }
        this.m_clientBase.Configure(options);
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
        this.m_clientBase.Receive(e.Memory);
        await base.OnNamedPipeReceived(e).ConfigureDefaultAwait();
    }

    private async Task<T> InvokeAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken)
    {
        this.ThrowIfDisposed();
        this.ThrowIfConfigIsNull();
        this.ThrowIfClientNotConnected();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.ClosedToken);
        return await action(cts.Token).ConfigureDefaultAwait();
    }

    private sealed class McpClientBaseImpl : McpClientBase
    {
        private readonly McpNamedPipeClient m_owner;

        public McpClientBaseImpl(McpNamedPipeClient owner)
        {
            this.m_owner = owner;
        }

        public void Configure(McpClientOptions options)
        {
            this.SetOptions(options);
        }

        public void Receive(ReadOnlyMemory<byte> data)
        {
            this.OnReceiveData(data);
        }

        protected override Task SendDataAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            return this.m_owner.ProtectedSendAsync(data, cancellationToken);
        }

        protected override void ThrowIfNotConnected()
        {
            this.ThrowIfNotSetup();
            this.m_owner.ThrowIfClientNotConnected();
        }

        protected override void ThrowIfNotSetup()
        {
            this.m_owner.ThrowIfDisposed();
            this.m_owner.ThrowIfConfigIsNull();
        }
    }
}
