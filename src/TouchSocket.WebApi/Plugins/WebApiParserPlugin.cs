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

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace TouchSocket.WebApi;

/// <summary>
/// WebApi解析器
/// </summary>
[PluginOption(Singleton = true)]
public sealed class WebApiParserPlugin : PluginBase, IHttpPlugin
{
    private readonly InternalWebApiMapping m_mapping = new InternalWebApiMapping();
    private readonly Dictionary<RpcParameter, WebApiParameterInfo> m_pairsForParameterInfo = new Dictionary<RpcParameter, WebApiParameterInfo>();
    private readonly IRpcServerProvider m_rpcServerProvider;

    /// <summary>
    /// 构造函数
    /// </summary>
    public WebApiParserPlugin(IRpcServerProvider rpcServerProvider)
    {
        ThrowHelper.ThrowArgumentNullExceptionIf(rpcServerProvider, nameof(IRpcServerProvider));
        this.RegisterServer(rpcServerProvider.GetMethods());
        this.m_rpcServerProvider = rpcServerProvider;
        this.Converter = new WebApiSerializerConverter();
        this.Converter.AddJsonSerializerFormatter(new Newtonsoft.Json.JsonSerializerSettings());
    }

    /// <summary>
    /// 转化器
    /// </summary>
    public WebApiSerializerConverter Converter { get; private set; }

    /// <summary>
    /// 获取Get函数路由映射图
    /// </summary>
    [Obsolete("此配置已被弃用，请使用Mapping属性代替", true)]
    public ActionMap GetRouteMap { get; private set; }

    /// <summary>
    /// 获取WebApi映射
    /// </summary>
    public IWebApiMapping Mapping => this.m_mapping;

    /// <summary>
    /// 获取Post函数路由映射图
    /// </summary>
    [Obsolete("此配置已被弃用，请使用Mapping属性代替", true)]
    public ActionMap PostRouteMap { get; private set; }

    /// <summary>
    /// 配置转换器。可以实现json序列化或者xml序列化。
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public WebApiParserPlugin ConfigureConverter(Action<WebApiSerializerConverter> action)
    {
        action.Invoke(this.Converter);
        return this;
    }

    /// <inheritdoc/>
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        var httpMethod = e.Context.Request.Method;
        var url = e.Context.Request.RelativeURL;
        var rpcMethod = this.Mapping.Match(url, httpMethod);
        if (rpcMethod != null)
        {
            using (var callContext = new WebApiCallContext(client, rpcMethod, e.Context, client.Resolver))
            {
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

                await this.ResponseAsync(client, e.Context, invokeResult).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }
        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private static object PrimitiveParse(string source, Type targetType)
    {
        if (targetType.IsPrimitive || targetType == TouchSocketCoreUtility.stringType)
        {
            StringExtension.TryParseToType(source, targetType, out var target);

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
        else if (parameter.Type.IsPrimitive || parameter.Type == typeof(string))
        {
            var parameterInfo = this.GetParameterInfo(parameter);
            if (parameterInfo.IsFromBody)
            {
                var body = await request.GetBodyAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                if (body.HasValue())
                {
                    return WebApiParserPlugin.PrimitiveParse(body, parameter.Type);
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
                if (value.HasValue())
                {
                    return WebApiParserPlugin.PrimitiveParse(value, parameter.Type);
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
                    return WebApiParserPlugin.PrimitiveParse(value, parameter.Type);
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
                if (value.HasValue())
                {
                    return WebApiParserPlugin.PrimitiveParse(value, parameter.Type);
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
                    httpResponse.SetContent(this.Converter.Serialize(httpContext, invokeResult.Result)).SetStatus();
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
                    httpResponse.SetContent(jsonString).SetStatus(405, "UnEnable");
                    break;
                }
            case InvokeStatus.InvocationException:
            case InvokeStatus.Exception:
                {
                    var jsonString = this.Converter.Serialize(httpContext, new ActionResult() { Status = invokeResult.Status, Message = invokeResult.Message });
                    httpResponse.SetContent(jsonString).SetStatus(422, "Exception");
                    break;
                }
        }

        await httpResponse.AnswerAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        if (!httpContext.Request.KeepAlive)
        {
            await client.ShutdownAsync(SocketShutdown.Both);
        }
    }
}