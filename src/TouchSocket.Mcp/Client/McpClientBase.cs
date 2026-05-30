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

using System.Text.Json;

namespace TouchSocket.Mcp;

/// <summary>
/// 提供 MCP 客户端的基础实现，管理请求/响应的等待匹配。
/// </summary>
public abstract class McpClientBase : IMcpClient
{
    private readonly WaitHandlePool<McpWaitResult> m_waitHandlePool = new WaitHandlePool<McpWaitResult>();
    private McpClientOptions m_options = new McpClientOptions();

    /// <summary>
    /// 获取客户端选项。
    /// </summary>
    protected McpClientOptions Options => this.m_options;

    /// <summary>
    /// 设置客户端选项。
    /// </summary>
    /// <param name="options">客户端选项。</param>
    protected void SetOptions(McpClientOptions options)
    {
        this.m_options = options ?? new McpClientOptions();
    }

    /// <summary>
    /// 验证当前客户端是否已完成连接准备。
    /// </summary>
    protected abstract void ThrowIfNotConnected();

    /// <summary>
    /// 验证当前客户端是否已完成配置。
    /// </summary>
    protected abstract void ThrowIfNotSetup();

    /// <summary>
    /// 使用当前客户端选项初始化 <see cref="McpClientBase"/>。
    /// </summary>
    protected McpClientBase()
    {
    }

    private McpClientOptions ClientOptions => this.m_options;

    /// <inheritdoc/>
    public async Task<McpInitializeResult> InitializeAsync(CancellationToken cancellationToken = default)
    {
        this.ThrowIfNotConnected();

        var result = await this.InvokeAsync<McpInitializeResult>(McpMethods.Initialize, new McpInitializeParams
        {
            ProtocolVersion = McpProtocolVersion.Latest,
            ClientInfo = this.ClientOptions.ClientInfo,
            Capabilities = this.ClientOptions.Capabilities
        }, cancellationToken).ConfigureAwait(false);

        var notificationBytes = McpMessageSerializer.BuildNotification(McpMethods.NotificationsInitialized, null, this.ClientOptions.JsonSerializerOptions);
        await this.SendDataAsync(notificationBytes, cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <inheritdoc/>
    public Task<McpListToolsResult> ListToolsAsync(CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync<McpListToolsResult>(McpMethods.ToolsList, null, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpCallToolResult> CallToolAsync(string name, Dictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
    {
        object paramsObj;
        if (arguments != null && arguments.Count > 0)
        {
            var argsJson = JsonSerializer.Serialize(arguments, this.ClientOptions.JsonSerializerOptions);
            var argsElement = JsonDocument.Parse(argsJson).RootElement.Clone();
            paramsObj = new McpCallToolParams { Name = name, Arguments = argsElement };
        }
        else
        {
            paramsObj = new McpCallToolParams { Name = name };
        }

        return this.InvokeAsync<McpCallToolResult>(McpMethods.ToolsCall, paramsObj, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListResourcesResult> ListResourcesAsync(CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync<McpListResourcesResult>(McpMethods.ResourcesList, null, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListResourceTemplatesResult> ListResourceTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync<McpListResourceTemplatesResult>(McpMethods.ResourcesTemplatesList, null, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpReadResourceResult> ReadResourceAsync(string uri, CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync<McpReadResourceResult>(McpMethods.ResourcesRead, new { uri }, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpListPromptsResult> ListPromptsAsync(CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync<McpListPromptsResult>(McpMethods.PromptsList, null, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<McpGetPromptResult> GetPromptAsync(string name, Dictionary<string, string> arguments = null, CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync<McpGetPromptResult>(McpMethods.PromptsGet, new McpGetPromptParams { Name = name, Arguments = arguments }, cancellationToken);
    }

    /// <summary>
    /// 子类实现，用于发送原始字节到传输层。
    /// </summary>
    /// <param name="data">要发送的字节数据。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    protected abstract Task SendDataAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken);

    /// <summary>
    /// 当传输层接收到数据时调用，处理响应消息并完成对应的等待。
    /// </summary>
    /// <param name="data">接收到的原始字节数据。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    protected void OnReceiveData(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        if (!McpMessageSerializer.TryParseMessage(data.Span, out _, out var response, out _, this.ClientOptions.JsonSerializerOptions))
        {
            return;
        }

        if (response == null)
        {
            return;
        }

        if (!response.Id.HasValue)
        {
            return;
        }

        int sign;
        var idElement = response.Id.Value;
        if (idElement.ValueKind == JsonValueKind.Number)
        {
            if (!idElement.TryGetInt32(out sign))
            {
                return;
            }
        }
        else if (idElement.ValueKind == JsonValueKind.String)
        {
            if (!int.TryParse(idElement.GetString(), out sign))
            {
                return;
            }
        }
        else
        {
            return;
        }

        this.m_waitHandlePool.Set(new McpWaitResult { Sign = sign, Response = response });
    }

    private async Task<T> InvokeAsync<T>(string method, object paramObj, CancellationToken cancellationToken)
    {
        var waitResult = new McpWaitResult();
        using var waitData = this.m_waitHandlePool.GetWaitDataAsync(waitResult);

        var sign = waitResult.Sign;
        using var signDoc = JsonDocument.Parse(sign.ToString());
        var idElement = signDoc.RootElement.Clone();

        JsonElement? paramsElement = null;
        if (paramObj != null)
        {
            var json = JsonSerializer.Serialize(paramObj, paramObj.GetType(), this.ClientOptions.JsonSerializerOptions);
            using var doc = JsonDocument.Parse(json);
            paramsElement = doc.RootElement.Clone();
        }

        var request = new McpRequest
        {
            Id = idElement,
            Method = method,
            Params = paramsElement
        };

        var requestBytes = McpMessageSerializer.SerializeToBytes(request, this.ClientOptions.JsonSerializerOptions);
        await this.SendDataAsync(requestBytes, cancellationToken).ConfigureAwait(false);

        this.ThrowIfNotConnected();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(this.ClientOptions.Timeout);

        var status = await waitData.WaitAsync(cts.Token).ConfigureAwait(false);

        if (status == WaitDataStatus.Canceled)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        if (status != WaitDataStatus.Success)
        {
            throw new TimeoutException($"MCP request '{method}' timed out.");
        }

        var completedResult = waitData.CompletedData;
        if (completedResult?.Response == null)
        {
            throw new InvalidOperationException($"MCP request '{method}' received null response.");
        }

        var response = completedResult.Response;
        if (!response.IsSuccess)
        {
            throw new InvalidOperationException($"MCP error [{response.Error.Code}]: {response.Error.Message}");
        }

        if (!response.Result.HasValue || response.Result.Value.ValueKind == JsonValueKind.Null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(response.Result.Value.GetRawText(), this.ClientOptions.JsonSerializerOptions);
    }
}
