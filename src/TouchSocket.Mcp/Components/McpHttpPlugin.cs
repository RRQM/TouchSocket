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

using System.Collections.Concurrent;
using TouchSocket.Http;
using TouchSocket.Rpc;

namespace TouchSocket.Mcp;

/// <summary>
/// 基于 HTTP（Streamable HTTP 传输）的 MCP 服务端插件�?
/// 支持 JSON 响应（同步请�?响应模式）�?
/// </summary>
[PluginOption(Singleton = true)]
public sealed class McpHttpPlugin : PluginBase, IHttpPlugin
{
    private readonly IRpcServerProvider m_rpcServerProvider;
    private readonly McpHttpPluginOptions m_options;
    private readonly ActionMap m_toolActionMap = new ActionMap(true);
    private readonly ActionMap m_resourceActionMap = new ActionMap(true);
    private readonly ActionMap m_promptActionMap = new ActionMap(true);
    private readonly List<McpTool> m_tools = new List<McpTool>();
    private readonly List<McpResource> m_resources = new List<McpResource>();
    private readonly List<McpPrompt> m_prompts = new List<McpPrompt>();
    private readonly ConcurrentDictionary<string, McpActor> m_sessions = new ConcurrentDictionary<string, McpActor>(StringComparer.Ordinal);

    /// <summary>
    /// 初始�?<see cref="McpHttpPlugin"/>�?
    /// </summary>
    /// <param name="rpcServerProvider">RPC 服务提供者。</param>
    /// <param name="options">MCP HTTP 插件选项。</param>
    public McpHttpPlugin(IRpcServerProvider rpcServerProvider, McpHttpPluginOptions options)
    {
        this.m_rpcServerProvider = rpcServerProvider;
        this.m_options = options ?? new McpHttpPluginOptions();

        if (rpcServerProvider != null)
        {
            McpActor.AddRpcToMaps(rpcServerProvider,
                this.m_toolActionMap, this.m_tools,
                this.m_resourceActionMap, this.m_resources,
                this.m_promptActionMap, this.m_prompts);
        }
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

        if (request.Method == HttpMethod.Delete)
        {
            await this.HandleDeleteAsync(e, CancellationToken.None).ConfigureDefaultAwait();
            return;
        }

        if (request.Method == HttpMethod.Post)
        {
            await this.HandlePostAsync(client, e, CancellationToken.None).ConfigureDefaultAwait();
            return;
        }

        e.Context.Response.SetStatus(405, "Method Not Allowed");
        await e.Context.Response.AnswerAsync().ConfigureDefaultAwait();
    }

    private async Task HandlePostAsync(IHttpSessionClient client, HttpContextEventArgs e, CancellationToken cancellationToken)
    {
        var request = e.Context.Request;
        var response = e.Context.Response;

        var bodyBytes = await request.GetContentAsync(cancellationToken).ConfigureDefaultAwait();
        if (bodyBytes.IsEmpty)
        {
            response.SetStatus(400, "Bad Request");
            await response.AnswerAsync(cancellationToken).ConfigureDefaultAwait();
            return;
        }

        var sessionIdHeader = request.Headers["Mcp-Session-Id"];
        var sessionId = sessionIdHeader.IsEmpty ? null : sessionIdHeader.First;

        McpActor actor;

        if (string.IsNullOrEmpty(sessionId) || !this.m_sessions.TryGetValue(sessionId, out actor))
        {
            sessionId = Guid.NewGuid().ToString("N");
            actor = this.CreateActor(sessionId, client, response);
            this.m_sessions[sessionId] = actor;
        }
        else
        {
            actor.Resolver = client.Resolver;
            actor.SendAction = async (data, ct) =>
            {
                response.SetContent(data.ToArray())
                    .SetStatusWithSuccess();
                response.Headers["Mcp-Session-Id"] = sessionId;
                response.ContentType = "application/json";
                await response.AnswerAsync(ct).ConfigureDefaultAwait();
            };
        }

        var callContext = new McpHttpCallContext(client, client.ClosedToken);
        await actor.InputReceiveAsync(bodyBytes, callContext).ConfigureDefaultAwait();

        if (!response.Responsed)
        {
            response.SetStatus(202, "Accepted");
            response.Headers["Mcp-Session-Id"] = sessionId;
            await response.AnswerAsync(cancellationToken).ConfigureDefaultAwait();
        }
    }

    private async Task HandleDeleteAsync(HttpContextEventArgs e, CancellationToken cancellationToken)
    {
        var request = e.Context.Request;
        var sessionIdHeader = request.Headers["Mcp-Session-Id"];
        if (!sessionIdHeader.IsEmpty)
        {
            if (this.m_sessions.TryRemove(sessionIdHeader.First, out var actor))
            {
                actor.SafeDispose();
            }
        }

        e.Context.Response.SetStatus(204, "No Content");
        await e.Context.Response.AnswerAsync(cancellationToken).ConfigureDefaultAwait();
    }

    private McpActor CreateActor(string sessionId, IHttpSessionClient client, HttpResponse response)
    {
        var actor = new McpActor(this.m_options.ServerOptions ?? new McpServerOptions());
        actor.Resolver = client.Resolver;
        actor.Logger = client.Logger;
        actor.SendAction = async (data, ct) =>
        {
            response.SetContent(data.ToArray())
                .SetStatusWithSuccess();
            response.Headers["Mcp-Session-Id"] = sessionId;
            response.ContentType = "application/json";
            await response.AnswerAsync(ct).ConfigureDefaultAwait();
        };

        actor.SetRpcServerProvider(this.m_rpcServerProvider,
            this.m_toolActionMap, this.m_resourceActionMap, this.m_promptActionMap,
            this.m_tools, this.m_resources, this.m_prompts);

        return actor;
    }
}
