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
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace TouchSocket.Mcp;

/// <summary>
/// 基于 HTTP（Streamable HTTP 传输）的 MCP 客户端。
/// </summary>
public sealed class McpHttpClientSlim : HttpClientSlim, IMcpClient
{
    private readonly McpClientBaseImpl m_clientBase = new McpClientBaseImpl();
    private Uri m_endpoint;
    private string m_sessionId;
    private bool m_connected;

    /// <summary>
    /// 初始化 <see cref="McpHttpClientSlim"/> 的新实例。
    /// </summary>
    public McpHttpClientSlim(System.Net.Http.HttpClient httpClient = default) : base(httpClient)
    {
    }

    /// <summary>
    /// 完成连接准备。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (this.m_endpoint == null)
        {
            throw new InvalidOperationException("McpHttpClient has not been setup.");
        }

        this.m_connected = true;
        this.m_clientBase.SetConnected(true);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<McpInitializeResult> InitializeAsync(CancellationToken cancellationToken = default)
    {
        return this.m_clientBase.InitializeAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListToolsResult> ListToolsAsync(CancellationToken cancellationToken = default)
    {
        return this.m_clientBase.ListToolsAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpCallToolResult> CallToolAsync(string name, Dictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
    {
        return this.m_clientBase.CallToolAsync(name, arguments, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListResourcesResult> ListResourcesAsync(CancellationToken cancellationToken = default)
    {
        return this.m_clientBase.ListResourcesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListResourceTemplatesResult> ListResourceTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return this.m_clientBase.ListResourceTemplatesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpReadResourceResult> ReadResourceAsync(string uri, CancellationToken cancellationToken = default)
    {
        return this.m_clientBase.ReadResourceAsync(uri, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListPromptsResult> ListPromptsAsync(CancellationToken cancellationToken = default)
    {
        return this.m_clientBase.ListPromptsAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpGetPromptResult> GetPromptAsync(string name, Dictionary<string, string> arguments = null, CancellationToken cancellationToken = default)
    {
        return this.m_clientBase.GetPromptAsync(name, arguments, cancellationToken);
    }

    /// <summary>
    /// 终止当前会话，向服务端发送 DELETE 请求。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(this.m_sessionId))
        {
            return;
        }

        using var deleteRequest = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Delete, this.m_endpoint);
        deleteRequest.Headers.Add("Mcp-Session-Id", this.m_sessionId);
        await this.HttpClient.SendAsync(deleteRequest, cancellationToken).ConfigureAwait(false);
        this.m_sessionId = null;
    }

    /// <inheritdoc/>
    protected override void LoadConfig(TouchSocketConfig config)
    {
        base.LoadConfig(config);

        this.m_endpoint = this.HttpClient.BaseAddress;
        if (this.m_endpoint == null)
        {
            throw new InvalidOperationException("McpHttpClientSlim endpoint is required.");
        }

        this.m_connected = false;
        this.m_sessionId = null;
        this.m_clientBase.Bind(this.SendDataAsync, config.GetValue(McpConfigExtension.McpClientOptionsProperty) ?? new McpClientOptions());
    }

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            this.m_connected = false;
            this.m_clientBase.SetConnected(false);
        }

        base.SafetyDispose(disposing);
    }

    private async Task SendDataAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        using var content = new ReadOnlyMemoryContent(data);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        using var httpRequest = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, this.m_endpoint);
        httpRequest.Content = content;
        httpRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrEmpty(this.m_sessionId))
        {
            httpRequest.Headers.Add("Mcp-Session-Id", this.m_sessionId);
        }

        using var httpResponse = await this.HttpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

        if (httpResponse.Headers.TryGetValues("Mcp-Session-Id", out var sessionValues))
        {
            foreach (var value in sessionValues)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.m_sessionId = value;
                    break;
                }
            }
        }

        var statusCode = (int)httpResponse.StatusCode;
        if (statusCode == 202)
        {
            return;
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"MCP HTTP request failed with status {statusCode}.");
        }

        var responseBytes = await httpResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        if (responseBytes.Length > 0)
        {
            this.m_clientBase.Receive(responseBytes, cancellationToken);
        }
    }

    private sealed class McpClientBaseImpl : McpClientBase
    {
        private Func<ReadOnlyMemory<byte>, CancellationToken, Task> m_sendAction;
        private bool m_connected;
        private bool m_setup;

        public void Bind(Func<ReadOnlyMemory<byte>, CancellationToken, Task> sendAction, McpClientOptions options)
        {
            this.m_sendAction = sendAction;
            this.SetOptions(options);
            this.m_setup = true;
            this.m_connected = false;
        }

        public void SetConnected(bool connected)
        {
            this.m_connected = connected;
        }

        public void Receive(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            this.OnReceiveData(data, cancellationToken);
        }

        protected override Task SendDataAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            return this.m_sendAction(data, cancellationToken);
        }

        protected override void ThrowIfNotConnected()
        {
            this.ThrowIfNotSetup();
            if (!this.m_connected)
            {
                throw new InvalidOperationException("McpHttpClient has not connected.");
            }
        }

        protected override void ThrowIfNotSetup()
        {
            if (!this.m_setup)
            {
                throw new InvalidOperationException("McpHttpClient has not been setup.");
            }
        }
    }

    private sealed class ReadOnlyMemoryContent : System.Net.Http.HttpContent
    {
        private readonly byte[] m_data;

        public ReadOnlyMemoryContent(ReadOnlyMemory<byte> data)
        {
            this.m_data = data.ToArray();
        }

        protected override Task SerializeToStreamAsync(System.IO.Stream stream, System.Net.TransportContext context)
        {
            return stream.WriteAsync(this.m_data, 0, this.m_data.Length);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = this.m_data.Length;
            return true;
        }
    }
}
