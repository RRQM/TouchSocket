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

using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace TouchSocket.WebApi;

/// <summary>
/// WebApi解析器
/// </summary>
[PluginOption(Singleton = true)]
public sealed class WebApiParserPlugin : PluginBase, IHttpPlugin, ITcpClosedPlugin
{
    private static readonly DependencyProperty<WebApiCallContext> s_webApiCallContextProperty = new DependencyProperty<WebApiCallContext>("WebApiCallContext", default);
    private readonly InternalWebApiMapping m_mapping = new InternalWebApiMapping();
    private readonly Dictionary<RpcParameter, WebApiParameterInfo> m_pairsForParameterInfo = new Dictionary<RpcParameter, WebApiParameterInfo>();
    private readonly IRpcServerProvider m_rpcServerProvider;

    /// <summary>
    /// 构造函数
    /// </summary>
    public WebApiParserPlugin(IRpcServerProvider rpcServerProvider, WebApiOption option)
    {
        ThrowHelper.ThrowIfNull(rpcServerProvider, nameof(IRpcServerProvider));
        this.RegisterServer(rpcServerProvider.GetMethods());
        this.m_rpcServerProvider = rpcServerProvider;
        this.Converter = option.Converter;
    }

    /// <summary>
    /// 转化器
    /// </summary>
    public WebApiSerializerConverter Converter { get; }

    /// <summary>
    /// 获取WebApi映射
    /// </summary>
    public IWebApiMapping Mapping => this.m_mapping;

    /// <inheritdoc/>
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        var httpMethod = e.Context.Request.Method;
        var url = e.Context.Request.RelativeURL;

        // 尝试匹配路由
        var matchResult = this.m_mapping.TryMatch(url, httpMethod);

        switch (matchResult.Status)
        {
            case RouteMatchStatus.Success:
                // 路由和方法都匹配,执行RPC调用
                await this.ExecuteRpcMethodAsync(client, e, matchResult.RpcMethod).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                return;

            case RouteMatchStatus.MethodNotAllowed:
                // 路由匹配但方法不允许,返回405
                await this.ResponseMethodNotAllowedAsync(client, e.Context, matchResult.AllowedMethods).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                return;

            case RouteMatchStatus.NotFound:
            default:
                // 路由未找到,继续执行后续插件
                await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                return;
        }
    }

    private async Task ExecuteRpcMethodAsync(IHttpSessionClient client, HttpContextEventArgs e, RpcMethod rpcMethod)
    {
        var callContext = new WebApiCallContext(client, rpcMethod, e.Context, client.Resolver);
        try
        {
            client.SetValue(s_webApiCallContextProperty, callContext);
            var invokeResult = new InvokeResult();
            var ps = new object[rpcMethod.Parameters.Length];

            try
            {
                for (var i = 0; i < rpcMethod.Parameters.Length; i++)
                {
                    var parameter = rpcMethod.Parameters[i];
                    ps[i] = await this.ParseParameterAsync(parameter, callContext).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
            }
            catch (Exception ex)
            {
                invokeResult.Status = InvokeStatus.Exception;
                invokeResult.Message = ex.Message;
                invokeResult.Exception = ex;
            }

            callContext.SetParameters(ps);
            invokeResult = await this.m_rpcServerProvider.ExecuteAsync(callContext, invokeResult).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            if (e.Context.Response.Responsed)
            {
                return;
            }

            if (!client.Online)
            {
                return;
            }

            await this.ResponseAsync(client, e.Context, invokeResult).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            callContext.Dispose();
            client.RemoveValue(s_webApiCallContextProperty);
        }
    }

    private async Task ResponseMethodNotAllowedAsync(IHttpSessionClient client, HttpContext httpContext, IEnumerable<HttpMethod> allowedMethods)
    {
        var httpResponse = httpContext.Response;

        // 设置Allow头,列出允许的方法
        if (allowedMethods != null)
        {
            var allowHeader = string.Join(", ", allowedMethods.Select(m => m.ToString()));
            httpResponse.Headers.TryAdd("Allow", allowHeader);
        }

        var jsonString = this.Converter.Serialize(httpContext, new ActionResult()
        {
            Status = InvokeStatus.UnEnable,
            Message = "HTTP方法不被允许"
        });

        await httpResponse
            .SetContent(jsonString)
            .SetStatus(405, "Method Not Allowed")
            .AnswerAsync()
            .ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        if (!httpContext.Request.KeepAlive)
        {
            await client.CloseAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    /// <inheritdoc/>
    public async Task OnTcpClosed(ITcpSession client, ClosedEventArgs e)
    {
        if (client.TryRemoveValue(s_webApiCallContextProperty, out var webApiCallContext))
        {
            webApiCallContext.Cancel();
        }
        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private static bool IsSimpleType(Type type)
    {
        if (type.IsPrimitive || type == TouchSocketCoreUtility.StringType)
        {
            return true;
        }

        if (type.IsNullableType(out var actualType))
        {
            return IsSimpleType(actualType);
        }
        return false;
    }

    private static object ParseSimpleType(string source, Type targetType)
    {
        if (targetType.IsNullableType(out var actualType))
        {
            targetType = actualType;
        }

        if (StringExtension.TryParseToType(source, targetType, out var target))
        {
            return target;
        }

        return default;
    }

    private WebApiParameterInfo GetParameterInfo(RpcParameter parameter)
    {
        if (this.m_pairsForParameterInfo.TryGetValue(parameter, out var webApiParameterInfo))
        {
            return webApiParameterInfo;
        }

        lock (this.m_pairsForParameterInfo)
        {
            if (!this.m_pairsForParameterInfo.ContainsKey(parameter))
            {
                this.m_pairsForParameterInfo.Add(parameter, new WebApiParameterInfo(parameter));
            }
        }
        return this.GetParameterInfo(parameter);
    }

    private async Task<object> ParseParameterAsync(RpcParameter parameter, WebApiCallContext callContext)
    {
        var request = callContext.HttpContext.Request;

        if (parameter.IsCallContext)
        {
            return callContext;
        }
        else if (parameter.IsFromServices)
        {
            return callContext.Resolver.Resolve(parameter.Type);
        }
        else if (IsSimpleType(parameter.Type))
        {
            var parameterInfo = this.GetParameterInfo(parameter);
            if (parameterInfo.IsFromBody)
            {
                var body = await request.GetBodyAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                if (body.HasValue())
                {
                    return WebApiParserPlugin.ParseSimpleType(body, parameter.Type);
                }
                else if (parameter.ParameterInfo.HasDefaultValue)
                {
                    return parameter.ParameterInfo.DefaultValue;
                }
                else
                {
                    return parameter.Type.GetDefault();
                }
            }
            else if (parameterInfo.IsFromHeader)
            {
                var value = request.Headers.Get(parameterInfo.FromHeaderName);
                if (!value.IsEmpty)
                {
                    return WebApiParserPlugin.ParseSimpleType(value.First, parameter.Type);
                }
                else if (parameter.ParameterInfo.HasDefaultValue)
                {
                    return parameter.ParameterInfo.DefaultValue;
                }
                else
                {
                    return parameter.Type.GetDefault();
                }
            }
            else if (parameterInfo.IsFromForm)
            {
                var value = (await request.GetFormCollectionAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext)).Get(parameterInfo.FromFormName);
                if (value.HasValue())
                {
                    return WebApiParserPlugin.ParseSimpleType(value, parameter.Type);
                }
                else if (parameter.ParameterInfo.HasDefaultValue)
                {
                    return parameter.ParameterInfo.DefaultValue;
                }
                else
                {
                    return parameter.Type.GetDefault();
                }
            }
            else
            {
                var value = request.Query.Get(parameterInfo.FromQueryName ?? parameter.Name);
                if (!value.IsEmpty)
                {
                    return WebApiParserPlugin.ParseSimpleType(value, parameter.Type);
                }
                else if (parameter.ParameterInfo.HasDefaultValue)
                {
                    return parameter.ParameterInfo.DefaultValue;
                }
                else
                {
                    return parameter.Type.GetDefault();
                }
            }
        }
        else
        {
            var str = await request.GetBodyAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return this.Converter.Deserialize(callContext.HttpContext, str, parameter.Type);
        }
    }

    private void RegisterServer(RpcMethod[] rpcMethods)
    {
        foreach (var rpcMethod in rpcMethods)
        {
            this.m_mapping.AddRpcMethod(rpcMethod);
        }

        this.m_mapping.MakeReadonly();
    }

    private async Task ResponseAsync(IHttpSessionClient client, HttpContext httpContext, InvokeResult invokeResult)
    {
        var httpResponse = httpContext.Response;
        var httpRequest = httpContext.Request;

        string contentType;

        switch (httpRequest.Accept)
        {
            case "application/json":
            case "application/xml":
            case "text/json":
            case "text/xml":
            case "text/plain":
                {
                    contentType = httpRequest.Accept;
                    break;
                }
            default:
                contentType = "application/json";
                break;
        }

        httpResponse.ContentType = contentType;

        switch (invokeResult.Status)
        {
            case InvokeStatus.Success:
                {
                    httpResponse.SetContent(this.Converter.Serialize(httpContext, invokeResult.Result)).SetStatusWithSuccess();
                    break;
                }
            case InvokeStatus.UnFound:
                {
                    var jsonString = this.Converter.Serialize(httpContext, new ActionResult() { Status = invokeResult.Status, Message = invokeResult.Message });
                    httpResponse.SetContent(jsonString).SetStatus(404, "Not Found");
                    break;
                }
            case InvokeStatus.UnEnable:
                {
                    var jsonString = this.Converter.Serialize(httpContext, new ActionResult() { Status = invokeResult.Status, Message = invokeResult.Message });
                    httpResponse.SetContent(jsonString).SetStatus(405, "Method Not Allowed");
                    break;
                }
            case InvokeStatus.InvocationException:
            case InvokeStatus.Exception:
                {
                    var jsonString = this.Converter.Serialize(httpContext, new ActionResult() { Status = invokeResult.Status, Message = invokeResult.Message });
                    httpResponse.SetContent(jsonString).SetStatus(422, "Unprocessable Entity");
                    break;
                }
        }

        await httpResponse.AnswerAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        if (!httpContext.Request.KeepAlive)
        {
            await client.CloseAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }
}