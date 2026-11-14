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

using System.Text.Json;
using TouchSocket.Http;
using TouchSocket.Rpc;

namespace TouchSocket.Mcp;

/// <summary>提供 JSON 响应模式的 Streamable HTTP MCP 服务。</summary>
[PluginOption(Singleton = true)]
public sealed class McpHttpPlugin : PluginBase, IHttpPlugin
{
    private readonly IRpcServerProvider m_rpcServerProvider;
    private readonly McpHttpPluginOptions m_options;
    private readonly ActionMap m_toolActionMap = new ActionMap(true);
    private readonly ActionMap m_resourceActionMap = new ActionMap(true);
    private readonly ActionMap m_promptActionMap = new ActionMap(true);
    private readonly List<McpToolDefinition> m_tools = new List<McpToolDefinition>();
    private readonly List<McpResource> m_resources = new List<McpResource>();
    private readonly List<McpPrompt> m_prompts = new List<McpPrompt>();
    private readonly Dictionary<string, McpHttpSession> m_sessions = new Dictionary<string, McpHttpSession>(StringComparer.Ordinal);
    private readonly object m_sessionLock = new object();
    private readonly Timer m_expirationTimer;

    /// <summary>初始化 HTTP MCP 插件。</summary>
    /// <param name="rpcServerProvider">RPC 服务提供者。</param>
    /// <param name="options">HTTP 与协议选项。</param>
    public McpHttpPlugin(IRpcServerProvider rpcServerProvider, McpHttpPluginOptions options)
    {
        this.m_rpcServerProvider = rpcServerProvider;
        this.m_options = options ?? new McpHttpPluginOptions();
        this.m_options.ServerOptions ??= new McpServerOptions();
        if (this.m_options.MaximumSessions < 1 || this.m_options.MaximumRequestBytes < 1 || this.m_options.SessionIdleTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(options));
        }
        if (rpcServerProvider is not null)
        {
            McpActor.AddRpcToMaps(rpcServerProvider,
                this.m_toolActionMap, this.m_tools,
                this.m_resourceActionMap, this.m_resources,
                this.m_promptActionMap, this.m_prompts,
                this.m_options.ServerOptions.JsonSerializerOptions,
                this.m_options.ServerOptions.IncludeToolOutputSchemaInToolList);
        }
        var interval = TimeSpan.FromMilliseconds(Math.Max(100, Math.Min(60000, this.m_options.SessionIdleTimeout.TotalMilliseconds / 2)));
        this.m_expirationTimer = new Timer(_ => this.ExpireSessions(), null, interval, interval);
    }

    /// <inheritdoc/>
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        var request = e.Context.Request;
        if (!request.RelativeURL.Equals(this.m_options.Path, StringComparison.OrdinalIgnoreCase))
        {
            await e.InvokeNext().ConfigureDefaultAwait();
            return;
        }
        e.Handled = true;
        var origin = request.Headers["Origin"];
        if (!origin.IsEmpty && (this.m_options.AllowedOrigins is null || !this.m_options.AllowedOrigins.Contains(origin.First)))
        {
            await AnswerStatusAsync(e.Context.Response, 403, "Forbidden").ConfigureDefaultAwait();
            return;
        }
        if (this.m_options.AuthorizeRequest is not null && !this.m_options.AuthorizeRequest(request))
        {
            await AnswerStatusAsync(e.Context.Response, 401, "Unauthorized").ConfigureDefaultAwait();
            return;
        }
        var protocol = request.Headers["MCP-Protocol-Version"];
        if (!protocol.IsEmpty && !this.m_options.ServerOptions.SupportedProtocolVersions.Contains(protocol.First, StringComparer.Ordinal))
        {
            await AnswerStatusAsync(e.Context.Response, 400, "Unsupported MCP Protocol Version").ConfigureDefaultAwait();
            return;
        }
        if (request.Method == HttpMethod.Delete)
        {
            await this.HandleDeleteAsync(e).ConfigureDefaultAwait();
            return;
        }
        if (request.Method == HttpMethod.Post)
        {
            await this.HandlePostAsync(client, e).ConfigureDefaultAwait();
            return;
        }
        await AnswerStatusAsync(e.Context.Response, 405, "Method Not Allowed").ConfigureDefaultAwait();
    }

    private async Task HandlePostAsync(IHttpSessionClient client, HttpContextEventArgs e)
    {
        var request = e.Context.Request;
        var response = e.Context.Response;
        var contentType = request.Headers["Content-Type"].First?.Split(';')[0].Trim();
        if (!string.Equals(contentType, "application/json", StringComparison.OrdinalIgnoreCase))
        {
            await AnswerStatusAsync(response, 415, "Unsupported Media Type").ConfigureDefaultAwait();
            return;
        }
        var accept = request.Headers["Accept"].First;
        if (string.IsNullOrWhiteSpace(accept) || !accept.Split(',').Any(item =>
            item.Split(';')[0].Trim() is "application/json" or "application/*" or "*/*"))
        {
            await AnswerStatusAsync(response, 406, "Not Acceptable").ConfigureDefaultAwait();
            return;
        }
        var body = await request.GetContentAsync(client.ClosedToken).ConfigureDefaultAwait();
        if (body.IsEmpty || body.Length > this.m_options.MaximumRequestBytes)
        {
            await AnswerStatusAsync(response, body.IsEmpty ? 400 : 413, body.IsEmpty ? "Bad Request" : "Payload Too Large").ConfigureDefaultAwait();
            return;
        }
        var valid = McpMessageSerializer.TryParseMessage(body.Span, out var message, out _, out var notification, this.m_options.ServerOptions.JsonSerializerOptions);
        if (!valid || message is null && notification is null)
        {
            await AnswerStatusAsync(response, 400, "Invalid JSON-RPC Message").ConfigureDefaultAwait();
            return;
        }
        var sessionHeader = request.Headers["Mcp-Session-Id"];
        var sessionId = sessionHeader.IsEmpty ? null : sessionHeader.First;
        McpHttpSession session = null;
        var status = 0;
        var createdSession = false;
        lock (this.m_sessionLock)
        {
            if (this.DisposedValue)
            {
                status = 503;
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                if (!this.m_sessions.TryGetValue(sessionId, out session))
                {
                    status = 404;
                }
            }
            else if (message?.Method != McpMethods.Initialize)
            {
                status = 400;
            }
            else if (this.m_sessions.Count >= this.m_options.MaximumSessions)
            {
                status = 503;
            }
            else
            {
                sessionId = Guid.NewGuid().ToString("N");
                var actor = new McpActor(this.m_options.ServerOptions)
                {
                    Resolver = client.Resolver,
                    Logger = client.Logger
                };
                actor.SetRpcServerProvider(this.m_rpcServerProvider,
                    this.m_toolActionMap, this.m_resourceActionMap, this.m_promptActionMap,
                    this.m_tools, this.m_resources, this.m_prompts);
                session = new McpHttpSession(actor);
                this.m_sessions.Add(sessionId, session);
                createdSession = true;
            }
            if (status == 0)
            {
                session.ActiveRequests++;
                session.LastActivityUtc = DateTime.UtcNow;
            }
        }
        if (status != 0)
        {
            await AnswerStatusAsync(response, status, "MCP Session Unavailable").ConfigureDefaultAwait();
            return;
        }
        using var context = new McpHttpCallContext(client, e.Context, client.ClosedToken);
        context.SetResolver(client.Resolver);
        context.SendAction = async (data, token) =>
        {
            response.SetContent(new ReadonlyMemoryHttpContent(data, "application/json")).SetStatusWithSuccess();
            if (session.Actor.ProtocolVersion is not null)
            {
                response.Headers["Mcp-Session-Id"] = sessionId;
            }
            await response.AnswerAsync(token).ConfigureDefaultAwait();
        };
        try
        {
            await session.Actor.InputReceiveAsync(body, context).ConfigureDefaultAwait();
            if (!response.Responsed)
            {
                // 已取消的请求不再输出 JSON-RPC 结果。普通调用绝不使用通知的 202 响应。
                var responseStatus = notification is not null ? 202 : context.Token.IsCancellationRequested ? 204 : 500;
                response.Headers["Mcp-Session-Id"] = sessionId;
                await AnswerStatusAsync(response, responseStatus, responseStatus == 202 ? "Accepted" : responseStatus == 204 ? "No Content" : "Missing MCP Response").ConfigureDefaultAwait();
            }
        }
        finally
        {
            var removeSession = createdSession && session.Actor.ProtocolVersion is null;
            lock (this.m_sessionLock)
            {
                session.ActiveRequests--;
                session.LastActivityUtc = DateTime.UtcNow;
                if (removeSession)
                {
                    this.m_sessions.Remove(sessionId);
                }
            }
            if (removeSession)
            {
                session.Dispose();
            }
        }
    }

    private async Task HandleDeleteAsync(HttpContextEventArgs e)
    {
        var header = e.Context.Request.Headers["Mcp-Session-Id"];
        McpHttpSession removed = null;
        lock (this.m_sessionLock)
        {
            if (!header.IsEmpty && this.m_sessions.TryGetValue(header.First, out removed))
            {
                this.m_sessions.Remove(header.First);
            }
        }
        removed?.Dispose();
        await AnswerStatusAsync(e.Context.Response, removed is null ? 404 : 204, removed is null ? "Not Found" : "No Content").ConfigureDefaultAwait();
    }

    private void ExpireSessions()
    {
        var expired = new List<McpHttpSession>();
        lock (this.m_sessionLock)
        {
            var now = DateTime.UtcNow;
            var keys = this.m_sessions.Where(item => item.Value.ActiveRequests == 0 && now - item.Value.LastActivityUtc >= this.m_options.SessionIdleTimeout)
                .Select(item => item.Key).ToList();
            foreach (var key in keys)
            {
                expired.Add(this.m_sessions[key]);
                this.m_sessions.Remove(key);
            }
        }
        foreach (var session in expired)
        {
            session.Dispose();
        }
    }

    private static Task AnswerStatusAsync(HttpResponse response, int status, string message)
    {
        response.SetStatus(status, message);
        return response.AnswerAsync();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.m_expirationTimer.Dispose();
            List<McpHttpSession> sessions;
            lock (this.m_sessionLock)
            {
                sessions = this.m_sessions.Values.ToList();
                this.m_sessions.Clear();
                base.Dispose(disposing);
            }
            foreach (var session in sessions)
            {
                session.Dispose();
            }
            return;
        }
        base.Dispose(disposing);
    }
}
