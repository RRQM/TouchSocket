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

using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using TouchSocket.Rpc;

namespace TouchSocket.Mcp;

/// <summary>
/// 表示 MCP 服务端协议处理核心，负责分发和响应 MCP 请求。
/// </summary>
public sealed class McpActor : DisposableObject
{
    private sealed class McpResourceTemplateRoute
    {
        public string UriTemplate { get; set; }

        public string LegacyKey { get; set; }

        public RpcMethod RpcMethod { get; set; }

        public Regex Regex { get; set; }

        public string[] ParameterNames { get; set; }
    }

    private readonly McpServerOptions m_options;
    private readonly List<McpToolDefinition> m_tools = new List<McpToolDefinition>();
    private readonly List<McpResource> m_resources = new List<McpResource>();
    private readonly List<McpResourceTemplate> m_resourceTemplates = new List<McpResourceTemplate>();
    private readonly List<McpPrompt> m_prompts = new List<McpPrompt>();
    private readonly Dictionary<string, RpcMethod> m_legacyResourceActionMap = new Dictionary<string, RpcMethod>(StringComparer.OrdinalIgnoreCase);
    private readonly List<McpResourceTemplateRoute> m_resourceTemplateRoutes = new List<McpResourceTemplateRoute>();
    private IRpcServerProvider m_rpcServerProvider;
    private bool m_initialized;

    /// <summary>
    /// 初始化 <see cref="McpActor"/> 实例。
    /// </summary>
    /// <param name="options">服务端选项。</param>
    public McpActor(McpServerOptions options = null)
    {
        this.m_options = options ?? new McpServerOptions();
    }

    /// <summary>
    /// 获取或设置工具方法的动作映射。
    /// </summary>
    public ActionMap ToolActionMap { get; private set; } = new ActionMap(true);

    /// <summary>
    /// 获取或设置资源方法的动作映射。
    /// </summary>
    public ActionMap ResourceActionMap { get; private set; } = new ActionMap(true);

    /// <summary>
    /// 获取或设置提示模板方法的动作映射。
    /// </summary>
    public ActionMap PromptActionMap { get; private set; } = new ActionMap(true);

    /// <summary>
    /// 获取或设置 RPC 调度器。
    /// </summary>
    public IRpcDispatcher<McpActor, IMcpCallContext> RpcDispatcher { get; set; } = new ImmediateRpcDispatcher<McpActor, IMcpCallContext>();

    /// <summary>
    /// 获取或设置数据发送委托，由传输层实现负责注入。
    /// </summary>
    public Func<ReadOnlyMemory<byte>, CancellationToken, Task> SendAction { get; set; }

    /// <summary>
    /// 获取或设置依赖注入解析器。
    /// </summary>
    public IResolver Resolver { get; set; }

    /// <summary>
    /// 获取或设置日志记录器。
    /// </summary>
    public ILog Logger { get; set; }

    /// <summary>
    /// 将 <see cref="IRpcServerProvider"/> 中标注了 MCP 特性的方法注册到各动作映射和元数据列表中。
    /// </summary>
    /// <param name="rpcServerProvider">RPC 服务提供者。</param>
    /// <param name="toolMap">工具动作映射。</param>
    /// <param name="toolList">工具元数据列表。</param>
    /// <param name="resourceMap">资源动作映射。</param>
    /// <param name="resourceList">资源元数据列表。</param>
    /// <param name="promptMap">提示模板动作映射。</param>
    /// <param name="promptList">提示模板元数据列表。</param>
    public static void AddRpcToMaps(
        IRpcServerProvider rpcServerProvider,
        ActionMap toolMap, List<McpToolDefinition> toolList,
        ActionMap resourceMap, List<McpResource> resourceList,
        ActionMap promptMap, List<McpPrompt> promptList)
    {
        AddRpcToMaps(
            rpcServerProvider,
            toolMap, toolList,
            resourceMap, resourceList,
            promptMap, promptList,
            McpOptionsBase.CreateDefaultJsonSerializerOptions());
    }

    internal static void AddRpcToMaps(
        IRpcServerProvider rpcServerProvider,
        ActionMap toolMap, List<McpToolDefinition> toolList,
        ActionMap resourceMap, List<McpResource> resourceList,
        ActionMap promptMap, List<McpPrompt> promptList,
        JsonSerializerOptions jsonSerializerOptions)
    {
        AddRpcToMaps(
            rpcServerProvider,
            toolMap, toolList,
            resourceMap, resourceList, new List<McpResourceTemplate>(), new Dictionary<string, RpcMethod>(StringComparer.OrdinalIgnoreCase), new List<McpResourceTemplateRoute>(),
            promptMap, promptList,
            jsonSerializerOptions);
    }

    private static void AddRpcToMaps(
        IRpcServerProvider rpcServerProvider,
        ActionMap toolMap, List<McpToolDefinition> toolList,
        ActionMap resourceMap, List<McpResource> resourceList, List<McpResourceTemplate> resourceTemplateList, Dictionary<string, RpcMethod> legacyResourceMap, List<McpResourceTemplateRoute> resourceTemplateRoutes,
        ActionMap promptMap, List<McpPrompt> promptList,
        JsonSerializerOptions jsonSerializerOptions)
    {
        foreach (var rpcMethod in rpcServerProvider.GetMethods())
        {
            if (rpcMethod.GetAttribute<McpToolAttribute>() is McpToolAttribute toolAttr)
            {
                toolMap.Add(toolAttr.GetInvokeKey(rpcMethod), rpcMethod);
                toolList.Add(BuildToolMeta(rpcMethod, toolAttr, jsonSerializerOptions));
            }
            else if (rpcMethod.GetAttribute<McpResourceAttribute>() is McpResourceAttribute resourceAttr)
            {
                RegisterResource(rpcMethod, resourceAttr, resourceMap, resourceList, resourceTemplateList, legacyResourceMap, resourceTemplateRoutes);
            }
            else if (rpcMethod.GetAttribute<McpPromptAttribute>() is McpPromptAttribute promptAttr)
            {
                promptMap.Add(promptAttr.GetInvokeKey(rpcMethod), rpcMethod);
                promptList.Add(BuildPromptMeta(rpcMethod));
            }
        }
    }

    private static void RegisterResource(
        RpcMethod rpcMethod,
        McpResourceAttribute attr,
        ActionMap resourceMap,
        List<McpResource> resourceList,
        List<McpResourceTemplate> resourceTemplateList,
        Dictionary<string, RpcMethod> legacyResourceMap,
        List<McpResourceTemplateRoute> resourceTemplateRoutes)
    {
        var uriPattern = attr.UriPattern;
        var legacyKey = attr.GetInvokeKey(rpcMethod);

        if (IsResourceTemplate(uriPattern))
        {
            resourceTemplateList.Add(BuildResourceTemplateMeta(rpcMethod, attr));
            resourceTemplateRoutes.Add(BuildResourceTemplateRoute(rpcMethod, uriPattern, legacyKey));
        }
        else
        {
            resourceMap.Add(uriPattern, rpcMethod);
            resourceList.Add(BuildResourceMeta(rpcMethod, attr));
        }

        legacyResourceMap[legacyKey] = rpcMethod;
    }

    private static bool IsResourceTemplate(string uriPattern)
    {
        return !string.IsNullOrEmpty(uriPattern)
            && uriPattern.IndexOf('{') >= 0
            && uriPattern.IndexOf('}') > uriPattern.IndexOf('{');
    }

    private static McpResourceTemplate BuildResourceTemplateMeta(RpcMethod rpcMethod, McpResourceAttribute attr)
    {
        return new McpResourceTemplate
        {
            UriTemplate = attr.UriPattern,
            Name = attr.GetInvokeKey(rpcMethod),
            Description = rpcMethod.GetDescription(),
            MimeType = attr.MimeType
        };
    }

    private static McpResourceTemplateRoute BuildResourceTemplateRoute(RpcMethod rpcMethod, string uriTemplate, string legacyKey)
    {
        var matches = Regex.Matches(uriTemplate, "\\{([^{}]+)\\}");
        var parameterNames = matches.Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
        var builder = new StringBuilder("^");
        var lastIndex = 0;

        foreach (Match match in matches)
        {
            builder.Append(Regex.Escape(uriTemplate.Substring(lastIndex, match.Index - lastIndex)));
            builder.Append($"(?<{match.Groups[1].Value}>[^/]+)");
            lastIndex = match.Index + match.Length;
        }

        builder.Append(Regex.Escape(uriTemplate.Substring(lastIndex)));
        builder.Append('$');

        return new McpResourceTemplateRoute
        {
            UriTemplate = uriTemplate,
            LegacyKey = legacyKey,
            RpcMethod = rpcMethod,
            ParameterNames = parameterNames,
            Regex = new Regex(builder.ToString(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        };
    }

    private static bool TryMatchResourceTemplate(string uri, McpResourceTemplateRoute route, out Dictionary<string, string> templateArguments)
    {
        templateArguments = null;
        var match = route.Regex.Match(uri);
        if (!match.Success)
        {
            return false;
        }

        templateArguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var parameterName in route.ParameterNames)
        {
            templateArguments[parameterName] = Uri.UnescapeDataString(match.Groups[parameterName].Value);
        }

        return true;
    }

    private bool TryResolveResource(string resourceKey, out RpcMethod rpcMethod, out Dictionary<string, string> templateArguments)
    {
        templateArguments = null;
        if (this.ResourceActionMap.TryGetRpcMethod(resourceKey, out rpcMethod))
        {
            return true;
        }

        foreach (var route in this.m_resourceTemplateRoutes)
        {
            if (TryMatchResourceTemplate(resourceKey, route, out templateArguments))
            {
                rpcMethod = route.RpcMethod;
                return true;
            }
        }

        return this.m_legacyResourceActionMap.TryGetValue(resourceKey, out rpcMethod);
    }

    /// <summary>
    /// 设置 RPC 服务提供者，扫描其方法并注册到当前 Actor 的动作映射中。
    /// </summary>
    /// <param name="rpcServerProvider">RPC 服务提供者。</param>
    public void SetRpcServerProvider(IRpcServerProvider rpcServerProvider)
    {
        AddRpcToMaps(
            rpcServerProvider,
            this.ToolActionMap, this.m_tools,
            this.ResourceActionMap, this.m_resources, this.m_resourceTemplates, this.m_legacyResourceActionMap, this.m_resourceTemplateRoutes,
            this.PromptActionMap, this.m_prompts,
            this.m_options.JsonSerializerOptions);
        this.m_rpcServerProvider = rpcServerProvider;
    }

    /// <summary>
    /// 使用外部已构建的动作映射设置 RPC 服务提供者。
    /// </summary>
    /// <param name="rpcServerProvider">RPC 服务提供者。</param>
    /// <param name="toolMap">工具动作映射。</param>
    /// <param name="resourceMap">资源动作映射。</param>
    /// <param name="promptMap">提示模板动作映射。</param>
    /// <param name="tools">工具元数据列表。</param>
    /// <param name="resources">资源元数据列表。</param>
    /// <param name="prompts">提示模板元数据列表。</param>
    public void SetRpcServerProvider(
        IRpcServerProvider rpcServerProvider,
        ActionMap toolMap, ActionMap resourceMap, ActionMap promptMap,
        List<McpToolDefinition> tools, List<McpResource> resources, List<McpPrompt> prompts)
    {
        this.m_rpcServerProvider = rpcServerProvider;
        this.ToolActionMap = toolMap;
        this.ResourceActionMap = resourceMap;
        this.PromptActionMap = promptMap;
        this.m_tools.AddRange(tools);
        this.m_resources.AddRange(resources);
        this.m_prompts.AddRange(prompts);

        this.m_resourceTemplates.Clear();
        this.m_legacyResourceActionMap.Clear();
        this.m_resourceTemplateRoutes.Clear();

        foreach (var rpcMethod in rpcServerProvider.GetMethods())
        {
            if (rpcMethod.GetAttribute<McpResourceAttribute>() is McpResourceAttribute resourceAttr)
            {
                RegisterResource(
                    rpcMethod,
                    resourceAttr,
                    new ActionMap(true), new List<McpResource>(), this.m_resourceTemplates, this.m_legacyResourceActionMap, this.m_resourceTemplateRoutes);
            }
        }
    }

    /// <summary>
    /// 接收来自传输层的消息数据并进行处理。
    /// </summary>
    /// <param name="data">原始 JSON 消息数据。</param>
    /// <param name="callContext">MCP 调用上下文。</param>
    public async Task InputReceiveAsync(ReadOnlyMemory<byte> data, McpCallContextBase callContext)
    {
        if (!McpMessageSerializer.TryParseMessage(data.Span, out var request, out _, out var notification, this.m_options.JsonSerializerOptions))
        {
            await this.SendErrorResponseAsync(null, McpErrorCodes.ParseError, "Parse error", callContext.Token).ConfigureAwait(false);
            return;
        }

        if (notification != null)
        {
            await this.HandleNotificationAsync(notification, callContext.Token).ConfigureAwait(false);
            return;
        }

        if (request != null)
        {
            await this.HandleRequestAsync(request, callContext).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.RpcDispatcher.SafeDispose();
        }
        base.Dispose(disposing);
    }

    private static McpToolDefinition BuildToolMeta(RpcMethod rpcMethod, McpToolAttribute attr, JsonSerializerOptions jsonSerializerOptions)
    {
        var schema = new McpToolInputSchema
        {
            Properties = new Dictionary<string, McpToolProperty>(),
            Required = new List<string>()
        };

        foreach (var param in rpcMethod.Parameters)
        {
            if (param.IsCallContext || param.IsFromServices || param.Type == typeof(CancellationToken))
            {
                continue;
            }

            schema.Properties[param.Name] = McpJsonSchemaGenerator.GenerateForTool(param.Type, param.ParameterInfo.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description, jsonSerializerOptions);

            if (!param.ParameterInfo.HasDefaultValue)
            {
                schema.Required.Add(param.Name);
            }
        }

        if (schema.Properties.Count == 0)
        {
            schema.Properties = null;
        }

        if (schema.Required.Count == 0)
        {
            schema.Required = null;
        }

        return new McpToolDefinition
        {
            Name = attr.GetInvokeKey(rpcMethod),
            Description = rpcMethod.GetDescription(),
            InputSchema = schema,
            OutputSchema = GetOutputSchema(rpcMethod, jsonSerializerOptions)
        };
    }

    private static McpToolProperty GetOutputSchema(RpcMethod rpcMethod, JsonSerializerOptions jsonSerializerOptions)
    {
        if (rpcMethod.RealReturnType == null
            || typeof(McpCallToolResult).IsAssignableFrom(rpcMethod.RealReturnType)
            || typeof(McpContent).IsAssignableFrom(rpcMethod.RealReturnType))
        {
            return null;
        }

        var outputSchema = McpJsonSchemaGenerator.Generate(rpcMethod.RealReturnType, null, jsonSerializerOptions, false);
        if (outputSchema.Type == "object")
        {
            return outputSchema;
        }

        return new McpToolProperty
        {
            Type = "object",
            Properties = new Dictionary<string, McpToolProperty>
            {
                ["result"] = outputSchema
            },
            Required = new List<string> { "result" }
        };
    }

    private static McpResource BuildResourceMeta(RpcMethod rpcMethod, McpResourceAttribute attr)
    {
        return new McpResource
        {
            Uri = attr.UriPattern,
            Name = attr.GetInvokeKey(rpcMethod),
            Description = rpcMethod.GetDescription(),
            MimeType = attr.MimeType
        };
    }

    private static McpPrompt BuildPromptMeta(RpcMethod rpcMethod)
    {
        var arguments = rpcMethod.Parameters
            .Where(p => !p.IsCallContext && !p.IsFromServices && p.Type != typeof(CancellationToken))
            .Select(p =>
            {
                return new McpPromptArgument
                {
                    Name = p.Name,
                    Description = p.ParameterInfo.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description,
                    Required = !p.ParameterInfo.HasDefaultValue ? (bool?)true : null
                };
            }).ToList();

        return new McpPrompt
        {
            Name = rpcMethod.Name,
            Description = rpcMethod.GetDescription(),
            Arguments = arguments.Count > 0 ? arguments : null
        };
    }

    private async Task HandleNotificationAsync(McpNotification notification, CancellationToken cancellationToken)
    {
        switch (notification.Method)
        {
            case McpMethods.NotificationsInitialized:
                this.m_initialized = true;
                break;
            default:
                break;
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    private async Task HandleRequestAsync(McpRequest request, McpCallContextBase callContext)
    {
        switch (request.Method)
        {
            case McpMethods.Initialize:
                await this.HandleInitializeAsync(request, callContext.Token).ConfigureAwait(false);
                break;
            case McpMethods.Ping:
                await this.SendSuccessResponseAsync(request, new McpEmptyResult(), callContext.Token).ConfigureAwait(false);
                break;
            case McpMethods.ToolsList:
                await this.HandleToolsListAsync(request, callContext.Token).ConfigureAwait(false);
                break;
            case McpMethods.ToolsCall:
                await this.HandleToolsCallAsync(request, callContext).ConfigureAwait(false);
                break;
            case McpMethods.ResourcesList:
                await this.HandleResourcesListAsync(request, callContext.Token).ConfigureAwait(false);
                break;
            case McpMethods.ResourcesTemplatesList:
                await this.HandleResourcesTemplatesListAsync(request, callContext.Token).ConfigureAwait(false);
                break;
            case McpMethods.ResourcesRead:
                await this.HandleResourcesReadAsync(request, callContext).ConfigureAwait(false);
                break;
            case McpMethods.PromptsList:
                await this.HandlePromptsListAsync(request, callContext.Token).ConfigureAwait(false);
                break;
            case McpMethods.PromptsGet:
                await this.HandlePromptsGetAsync(request, callContext).ConfigureAwait(false);
                break;
            default:
                await this.SendErrorResponseAsync(request.Id, McpErrorCodes.MethodNotFound, $"Method '{request.Method}' not found.", callContext.Token).ConfigureAwait(false);
                break;
        }
    }

    private async Task HandleInitializeAsync(McpRequest request, CancellationToken cancellationToken)
    {
        McpInitializeParams initializeParams = null;
        if (request.Params.HasValue)
        {
            initializeParams = JsonSerializer.Deserialize<McpInitializeParams>(request.Params.Value.GetRawText(), this.m_options.JsonSerializerOptions);
        }

        var result = new McpInitializeResult
        {
            ProtocolVersion = this.m_options.NegotiateProtocolVersion(initializeParams?.ProtocolVersion),
            ServerInfo = this.m_options.ServerInfo,
            Instructions = this.m_options.Instructions,
            Capabilities = new McpServerCapabilities
            {
                Tools = this.m_tools.Count > 0 ? new McpToolsCapability() : null,
                Resources = this.m_resources.Count > 0 || this.m_resourceTemplates.Count > 0 ? new McpResourcesCapability() : null,
                Prompts = this.m_prompts.Count > 0 ? new McpPromptsCapability() : null
            }
        };

        await this.SendSuccessResponseAsync(request, result, cancellationToken).ConfigureAwait(false);
    }

    private async Task HandleToolsListAsync(McpRequest request, CancellationToken cancellationToken)
    {
        var result = new McpListToolsResult { Tools = this.m_tools };
        await this.SendSuccessResponseAsync(request, result, cancellationToken).ConfigureAwait(false);
    }

    private async Task HandleToolsCallAsync(McpRequest request, McpCallContextBase callContext)
    {
        McpCallToolParams callParams = null;
        if (request.Params.HasValue)
        {
            callParams = JsonSerializer.Deserialize<McpCallToolParams>(request.Params.Value.GetRawText(), this.m_options.JsonSerializerOptions);
        }

        if (callParams == null || string.IsNullOrEmpty(callParams.Name))
        {
            await this.SendErrorResponseAsync(request.Id, McpErrorCodes.InvalidParams, "Missing tool name.", callContext.Token).ConfigureAwait(false);
            return;
        }

        if (!this.ToolActionMap.TryGetRpcMethod(callParams.Name, out var rpcMethod))
        {
            await this.SendErrorResponseAsync(request.Id, McpErrorCodes.MethodNotFound, $"Tool '{callParams.Name}' not found.", callContext.Token).ConfigureAwait(false);
            return;
        }

        callContext.SetMcpRequest(request);
        callContext.SetRpcMethod(rpcMethod);

        if (rpcMethod.Reenterable == false || this.RpcDispatcher.Reenterable == false)
        {
            callContext.SetResolver(this.Resolver);
        }
        else
        {
            callContext.SetResolver(this.Resolver.CreateScopedResolver());
        }

        this.BuildToolParameters(callContext, rpcMethod, callParams.Arguments);

        await this.RpcDispatcher.Dispatcher(this, callContext, this.ThisInvokeToolAsync).ConfigureAwait(false);
    }

    private async Task HandleResourcesListAsync(McpRequest request, CancellationToken cancellationToken)
    {
        var result = new McpListResourcesResult { Resources = this.m_resources };
        await this.SendSuccessResponseAsync(request, result, cancellationToken).ConfigureAwait(false);
    }

    private async Task HandleResourcesTemplatesListAsync(McpRequest request, CancellationToken cancellationToken)
    {
        var result = new McpListResourceTemplatesResult { ResourceTemplates = this.m_resourceTemplates };
        await this.SendSuccessResponseAsync(request, result, cancellationToken).ConfigureAwait(false);
    }

    private async Task HandleResourcesReadAsync(McpRequest request, McpCallContextBase callContext)
    {
        string uri = null;
        if (request.Params.HasValue && request.Params.Value.TryGetProperty("uri", out var uriProp))
        {
            uri = uriProp.GetString();
        }

        if (string.IsNullOrEmpty(uri))
        {
            await this.SendErrorResponseAsync(request.Id, McpErrorCodes.InvalidParams, "Missing resource uri.", callContext.Token).ConfigureAwait(false);
            return;
        }

        if (!this.TryResolveResource(uri, out var rpcMethod, out var templateArguments))
        {
            await this.SendErrorResponseAsync(request.Id, McpErrorCodes.MethodNotFound, $"Resource '{uri}' not found.", callContext.Token).ConfigureAwait(false);
            return;
        }

        callContext.SetMcpRequest(request);
        callContext.SetRpcMethod(rpcMethod);

        if (rpcMethod.Reenterable == false || this.RpcDispatcher.Reenterable == false)
        {
            callContext.SetResolver(this.Resolver);
        }
        else
        {
            callContext.SetResolver(this.Resolver.CreateScopedResolver());
        }

        this.BuildResourceParameters(callContext, rpcMethod, uri, templateArguments);

        await this.RpcDispatcher.Dispatcher(this, callContext, this.ThisInvokeResourceAsync).ConfigureAwait(false);
    }

    private async Task HandlePromptsListAsync(McpRequest request, CancellationToken cancellationToken)
    {
        var result = new McpListPromptsResult { Prompts = this.m_prompts };
        await this.SendSuccessResponseAsync(request, result, cancellationToken).ConfigureAwait(false);
    }

    private async Task HandlePromptsGetAsync(McpRequest request, McpCallContextBase callContext)
    {
        McpGetPromptParams promptParams = null;
        if (request.Params.HasValue)
        {
            promptParams = JsonSerializer.Deserialize<McpGetPromptParams>(request.Params.Value.GetRawText(), this.m_options.JsonSerializerOptions);
        }

        if (promptParams == null || string.IsNullOrEmpty(promptParams.Name))
        {
            await this.SendErrorResponseAsync(request.Id, McpErrorCodes.InvalidParams, "Missing prompt name.", callContext.Token).ConfigureAwait(false);
            return;
        }

        if (!this.PromptActionMap.TryGetRpcMethod(promptParams.Name, out var rpcMethod))
        {
            await this.SendErrorResponseAsync(request.Id, McpErrorCodes.MethodNotFound, $"Prompt '{promptParams.Name}' not found.", callContext.Token).ConfigureAwait(false);
            return;
        }

        callContext.SetMcpRequest(request);
        callContext.SetRpcMethod(rpcMethod);

        if (rpcMethod.Reenterable == false || this.RpcDispatcher.Reenterable == false)
        {
            callContext.SetResolver(this.Resolver);
        }
        else
        {
            callContext.SetResolver(this.Resolver.CreateScopedResolver());
        }

        this.BuildPromptParameters(callContext, rpcMethod, promptParams.Arguments);

        await this.RpcDispatcher.Dispatcher(this, callContext, this.ThisInvokePromptAsync).ConfigureAwait(false);
    }

    private void BuildToolParameters(McpCallContextBase callContext, RpcMethod rpcMethod, JsonElement? arguments)
    {
        var ps = new object[rpcMethod.Parameters.Length];
        for (var i = 0; i < rpcMethod.Parameters.Length; i++)
        {
            var param = rpcMethod.Parameters[i];
            if (param.IsCallContext)
            {
                ps[i] = callContext;
            }
            else if (param.IsFromServices)
            {
                ps[i] = callContext.Resolver?.Resolve(param.Type);
            }
            else if (param.Type == typeof(CancellationToken))
            {
                ps[i] = callContext.Token;
            }
            else if (arguments.HasValue && arguments.Value.ValueKind == JsonValueKind.Object
                     && arguments.Value.TryGetProperty(param.Name, out var pv))
            {
                ps[i] = pv.Deserialize(param.Type, this.m_options.JsonSerializerOptions);
            }
            else if (param.ParameterInfo.HasDefaultValue)
            {
                ps[i] = param.ParameterInfo.DefaultValue;
            }
            else
            {
                ps[i] = param.Type.GetDefault();
            }
        }
        callContext.SetParameters(ps);
    }

    private void BuildResourceParameters(McpCallContextBase callContext, RpcMethod rpcMethod, string uri, Dictionary<string, string> templateArguments)
    {
        var ps = new object[rpcMethod.Parameters.Length];
        for (var i = 0; i < rpcMethod.Parameters.Length; i++)
        {
            var param = rpcMethod.Parameters[i];
            if (param.IsCallContext)
            {
                ps[i] = callContext;
            }
            else if (param.IsFromServices)
            {
                ps[i] = callContext.Resolver?.Resolve(param.Type);
            }
            else if (param.Type == typeof(CancellationToken))
            {
                ps[i] = callContext.Token;
            }
            else if (param.Type == typeof(string) && string.Equals(param.Name, "uri", StringComparison.OrdinalIgnoreCase))
            {
                ps[i] = uri;
            }
            else if (templateArguments != null && templateArguments.TryGetValue(param.Name, out var templateValue))
            {
                ps[i] = param.Type == typeof(string)
                    ? templateValue
                    : JsonSerializer.Deserialize(JsonSerializer.Serialize(templateValue, this.m_options.JsonSerializerOptions), param.Type, this.m_options.JsonSerializerOptions);
            }
            else if (param.ParameterInfo.HasDefaultValue)
            {
                ps[i] = param.ParameterInfo.DefaultValue;
            }
            else
            {
                ps[i] = param.Type.GetDefault();
            }
        }
        callContext.SetParameters(ps);
    }

    private void BuildPromptParameters(McpCallContextBase callContext, RpcMethod rpcMethod, Dictionary<string, string> arguments)
    {
        var ps = new object[rpcMethod.Parameters.Length];
        for (var i = 0; i < rpcMethod.Parameters.Length; i++)
        {
            var param = rpcMethod.Parameters[i];
            if (param.IsCallContext)
            {
                ps[i] = callContext;
            }
            else if (param.IsFromServices)
            {
                ps[i] = callContext.Resolver?.Resolve(param.Type);
            }
            else if (param.Type == typeof(CancellationToken))
            {
                ps[i] = callContext.Token;
            }
            else if (arguments != null && arguments.TryGetValue(param.Name, out var strVal))
            {
                ps[i] = param.Type == typeof(string)
                    ? (object)strVal
                    : JsonSerializer.Deserialize(JsonSerializer.Serialize(strVal, this.m_options.JsonSerializerOptions), param.Type, this.m_options.JsonSerializerOptions);
            }
            else if (param.ParameterInfo.HasDefaultValue)
            {
                ps[i] = param.ParameterInfo.DefaultValue;
            }
            else
            {
                ps[i] = param.Type.GetDefault();
            }
        }
        callContext.SetParameters(ps);
    }

    private string SerializeMcpValue(object value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (value is string s)
        {
            return s;
        }

        return JsonSerializer.Serialize(value, this.m_options.JsonSerializerOptions.GetTypeInfo(value.GetType()));
    }

    private McpCallToolResult BuildToolCallResult(object value)
    {
        if (value is McpCallToolResult toolResult)
        {
            return toolResult;
        }

        if (value is McpContent content)
        {
            return new McpCallToolResult
            {
                IsError = false,
                Content = new List<McpContent> { content }
            };
        }

        if (value is IEnumerable<McpContent> contents)
        {
            return new McpCallToolResult
            {
                IsError = false,
                Content = contents.ToList()
            };
        }

        if (value == null)
        {
            return new McpCallToolResult
            {
                IsError = false,
                Content = new List<McpContent> { new McpTextContent { Text = string.Empty } }
            };
        }

        return new McpCallToolResult
        {
            IsError = false,
            Content = new List<McpContent> { new McpTextContent { Text = SerializeMcpValue(value) } },
            StructuredContent = CreateStructuredContent(value, this.m_options.JsonSerializerOptions)
        };
    }

    private static JsonElement? CreateStructuredContent(object value, JsonSerializerOptions jsonSerializerOptions)
    {
        if (value == null)
        {
            return null;
        }

        var type = value.GetType();
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        if (underlying == typeof(string)
            || underlying == typeof(char)
            || underlying == typeof(DateTime)
            || underlying == typeof(DateTimeOffset)
            || underlying == typeof(Guid)
            || underlying == typeof(TimeSpan)
            || underlying == typeof(Uri)
            || underlying == typeof(bool)
            || underlying.IsPrimitive
            || underlying.IsEnum
            || underlying == typeof(decimal)
            || (underlying != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(underlying)))
        {
            return CreateJsonElement(new Dictionary<string, object> { ["result"] = value }, jsonSerializerOptions);
        }

        return CreateJsonElement(value, jsonSerializerOptions);
    }

    private static JsonElement CreateJsonElement(object value, JsonSerializerOptions jsonSerializerOptions)
    {
        using var document = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(value, jsonSerializerOptions.GetTypeInfo(value.GetType())));
        return document.RootElement.Clone();
    }

    private static void ApplyResourceDefaults(McpResourceContent content, string uri, string mimeType)
    {
        content.Uri ??= uri;
        content.MimeType ??= mimeType;
    }

    private static string GetRequestedResourceUri(McpCallContextBase callContext)
    {
        if (callContext.McpRequest?.Params.HasValue == true
            && callContext.McpRequest.Params.Value.TryGetProperty("uri", out var uriProp))
        {
            return uriProp.GetString();
        }

        return null;
    }

    private McpReadResourceResult BuildReadResourceResult(object value, McpCallContextBase callContext)
    {
        if (value is McpReadResourceResult resourceResult)
        {
            return resourceResult;
        }

        var resourceAttr = callContext.RpcMethod?.GetAttribute<McpResourceAttribute>();
        var uri = GetRequestedResourceUri(callContext);
        var mimeType = resourceAttr?.MimeType;

        if (value is McpResourceContent resourceContent)
        {
            ApplyResourceDefaults(resourceContent, uri, mimeType);
            return new McpReadResourceResult
            {
                Contents = new List<McpResourceContent> { resourceContent }
            };
        }

        if (value is IEnumerable<McpResourceContent> resourceContents)
        {
            var contents = resourceContents.ToList();
            foreach (var content in contents)
            {
                ApplyResourceDefaults(content, uri, mimeType);
            }

            return new McpReadResourceResult
            {
                Contents = contents
            };
        }

        if (value is byte[] bytes)
        {
            return new McpReadResourceResult
            {
                Contents = new List<McpResourceContent>
                {
                    new McpResourceContent
                    {
                        Uri = uri,
                        MimeType = mimeType ?? "application/octet-stream",
                        Blob = Convert.ToBase64String(bytes)
                    }
                }
            };
        }

        return new McpReadResourceResult
        {
            Contents = new List<McpResourceContent>
            {
                new McpResourceContent
                {
                    Uri = uri,
                    MimeType = mimeType,
                    Text = SerializeMcpValue(value)
                }
            }
        };
    }

    private McpGetPromptResult BuildPromptResult(object value)
    {
        if (value is McpGetPromptResult promptResult)
        {
            return promptResult;
        }

        if (value is McpPromptMessage promptMessage)
        {
            return new McpGetPromptResult
            {
                Messages = new List<McpPromptMessage> { promptMessage }
            };
        }

        if (value is IEnumerable<McpPromptMessage> promptMessages)
        {
            return new McpGetPromptResult
            {
                Messages = promptMessages.ToList()
            };
        }

        if (value is McpContent content)
        {
            return new McpGetPromptResult
            {
                Messages = new List<McpPromptMessage>
                {
                    new McpPromptMessage { Role = "user", Content = content }
                }
            };
        }

        if (value is IEnumerable<McpContent> contents)
        {
            return new McpGetPromptResult
            {
                Messages = contents.Select(contentItem => new McpPromptMessage { Role = "user", Content = contentItem }).ToList()
            };
        }

        return new McpGetPromptResult
        {
            Messages = new List<McpPromptMessage>
            {
                new McpPromptMessage { Role = "user", Content = new McpTextContent { Text = SerializeMcpValue(value) } }
            }
        };
    }

    private async Task ThisInvokeToolAsync(IMcpCallContext callContext)
    {
        var ctx = (McpCallContextBase)callContext;
        try
        {
            var invokeResult = await this.m_rpcServerProvider.ExecuteAsync(callContext, new InvokeResult(InvokeStatus.Ready)).ConfigureAwait(false);

            if (!callContext.McpRequestId.HasValue)
            {
                return;
            }

            if (invokeResult.Status != InvokeStatus.Success)
            {
                var errorResult = new McpCallToolResult
                {
                    IsError = true,
                    Content = new List<McpContent> { new McpTextContent { Text = invokeResult.Message ?? "Internal error." } }
                };
                var errorBytes = McpMessageSerializer.BuildSuccessResponse(ctx.McpRequest.Id, errorResult, this.m_options.JsonSerializerOptions);
                await this.SendAction(errorBytes, CancellationToken.None).ConfigureAwait(false);
                return;
            }

            var result = BuildToolCallResult(invokeResult.Result);
            var bytes = McpMessageSerializer.BuildSuccessResponse(ctx.McpRequest.Id, result, this.m_options.JsonSerializerOptions);
            await this.SendAction(bytes, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.Logger?.Debug(this, ex.Message);
        }
        finally
        {
            ctx.Dispose();
        }
    }

    private async Task ThisInvokeResourceAsync(IMcpCallContext callContext)
    {
        var ctx = (McpCallContextBase)callContext;
        try
        {
            var invokeResult = await this.m_rpcServerProvider.ExecuteAsync(callContext, new InvokeResult(InvokeStatus.Ready)).ConfigureAwait(false);

            if (!callContext.McpRequestId.HasValue)
            {
                return;
            }

            if (invokeResult.Status != InvokeStatus.Success)
            {
                await this.SendErrorResponseAsync(ctx.McpRequest.Id, McpErrorCodes.InternalError, invokeResult.Message ?? "Internal error.", CancellationToken.None).ConfigureAwait(false);
                return;
            }

            var result = BuildReadResourceResult(invokeResult.Result, ctx);
            var bytes = McpMessageSerializer.BuildSuccessResponse(ctx.McpRequest.Id, result, this.m_options.JsonSerializerOptions);
            await this.SendAction(bytes, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.Logger?.Debug(this, ex.Message);
        }
        finally
        {
            ctx.Dispose();
        }
    }

    private async Task ThisInvokePromptAsync(IMcpCallContext callContext)
    {
        var ctx = (McpCallContextBase)callContext;
        try
        {
            var invokeResult = await this.m_rpcServerProvider.ExecuteAsync(callContext, new InvokeResult(InvokeStatus.Ready)).ConfigureAwait(false);

            if (!callContext.McpRequestId.HasValue)
            {
                return;
            }

            if (invokeResult.Status != InvokeStatus.Success)
            {
                await this.SendErrorResponseAsync(ctx.McpRequest.Id, McpErrorCodes.InternalError, invokeResult.Message ?? "Internal error.", CancellationToken.None).ConfigureAwait(false);
                return;
            }

            var result = BuildPromptResult(invokeResult.Result);
            var bytes = McpMessageSerializer.BuildSuccessResponse(ctx.McpRequest.Id, result, this.m_options.JsonSerializerOptions);
            await this.SendAction(bytes, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.Logger?.Debug(this, ex.Message);
        }
        finally
        {
            ctx.Dispose();
        }
    }

    private async Task SendSuccessResponseAsync(McpRequest request, object result, CancellationToken cancellationToken)
    {
        var bytes = McpMessageSerializer.BuildSuccessResponse(request.Id, result, this.m_options.JsonSerializerOptions);
        await this.SendAction(bytes, cancellationToken).ConfigureAwait(false);
    }

    private async Task SendErrorResponseAsync(JsonElement? id, int code, string message, CancellationToken cancellationToken)
    {
        var bytes = McpMessageSerializer.BuildErrorResponse(id, code, message, this.m_options.JsonSerializerOptions);
        await this.SendAction(bytes, cancellationToken).ConfigureAwait(false);
    }
}
