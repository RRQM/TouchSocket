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
using System.Reflection;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace TouchSocket.WebApi
{
    /// <summary>
    /// WebApi解析器
    /// </summary>
    [PluginOption(Singleton = true)]
    public class WebApiParserPlugin : PluginBase
    {
        private readonly Lock m_locker = LockFactory.Create();
        private readonly Dictionary<RpcParameter, WebApiParameterInfo> m_pairsForParameterInfo = new Dictionary<RpcParameter, WebApiParameterInfo>();
        private readonly IResolver m_resolver;
        private readonly IRpcServerProvider m_rpcServerProvider;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WebApiParserPlugin(IRpcServerProvider rpcServerProvider, IResolver resolver)
        {
            ThrowHelper.ThrowArgumentNullExceptionIf(rpcServerProvider, nameof(IRpcServerProvider));

            this.GetRouteMap = new ActionMap(true);
            this.PostRouteMap = new ActionMap(true);
            this.RegisterServer(rpcServerProvider.GetMethods());
            this.m_rpcServerProvider = rpcServerProvider;
            this.m_resolver = ThrowHelper.ThrowArgumentNullExceptionIf(resolver, nameof(IResolver));

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
        public ActionMap GetRouteMap { get; private set; }

        /// <summary>
        /// 获取Post函数路由映射图
        /// </summary>
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
        protected override void Loaded(IPluginManager pluginManager)
        {
            pluginManager.Add<IHttpSessionClient, HttpContextEventArgs>(typeof(IHttpPlugin), this.OnHttpRequest);
            base.Loaded(pluginManager);
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

            lock (this.m_locker)
            {
                if (!m_pairsForParameterInfo.ContainsKey(parameter))
                {
                    m_pairsForParameterInfo.Add(parameter, new WebApiParameterInfo(parameter));
                }
            }
            return this.GetParameterInfo(parameter);
        }

        private async Task OnHttpGet(IHttpSessionClient client, HttpContextEventArgs e)
        {
            if (this.GetRouteMap.TryGetRpcMethod(e.Context.Request.RelativeURL, out var rpcMethod))
            {
                e.Handled = true;

                var invokeResult = new InvokeResult();
                object[] ps = null;

                var callContext = new WebApiCallContext(client, rpcMethod, this.m_resolver, e.Context);

                if (rpcMethod.IsEnable)
                {
                    try
                    {
                        ps = new object[rpcMethod.Parameters.Length];
                        for (var i = 0; i < rpcMethod.Parameters.Length; i++)
                        {
                            var parameter = rpcMethod.Parameters[i];
                            ps[i] = await this.ParseParameterAsync(parameter, callContext).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        invokeResult.Status = InvokeStatus.Exception;
                        invokeResult.Message = ex.Message;
                    }
                }
                else
                {
                    invokeResult.Status = InvokeStatus.UnEnable;
                }
                if (invokeResult.Status == InvokeStatus.Ready)
                {
                    invokeResult = await this.m_rpcServerProvider.ExecuteAsync(callContext, ps).ConfigureAwait(false);
                }

                if (e.Context.Response.Responsed)
                {
                    return;
                }

                await this.ResponseAsync(client, e.Context, invokeResult).ConfigureAwait(false);
                return;
            }
            await e.InvokeNext().ConfigureAwait(false);
        }

        private async Task OnHttpPost(IHttpSessionClient client, HttpContextEventArgs e)
        {
            if (this.PostRouteMap.TryGetRpcMethod(e.Context.Request.RelativeURL, out var rpcMethod))
            {
                e.Handled = true;

                var invokeResult = new InvokeResult();
                object[] ps = null;
                if (rpcMethod.IsEnable)
                {
                    var callContext = new WebApiCallContext(client, rpcMethod, this.m_resolver, e.Context);
                    try
                    {
                        ps = new object[rpcMethod.Parameters.Length];

                        for (var i = 0; i < rpcMethod.Parameters.Length; i++)
                        {
                            var parameter = rpcMethod.Parameters[i];
                            ps[i] = await this.ParseParameterAsync(parameter, callContext).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        invokeResult.Status = InvokeStatus.Exception;
                        invokeResult.Message = ex.Message;
                    }

                    if (invokeResult.Status == InvokeStatus.Ready)
                    {
                        invokeResult = await this.m_rpcServerProvider.ExecuteAsync(callContext, ps).ConfigureAwait(false);
                    }

                    if (e.Context.Response.Responsed)
                    {
                        return;
                    }
                }
                else
                {
                    invokeResult.Status = InvokeStatus.UnEnable;
                }

                await this.ResponseAsync(client, e.Context, invokeResult).ConfigureAwait(false);
            }
            await e.InvokeNext().ConfigureAwait(false);
        }

        private Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
        {
            if (e.Context.Request.Method == HttpMethod.Get)
            {
                return this.OnHttpGet(client, e);
            }
            else if (e.Context.Request.Method == HttpMethod.Post)
            {
                return this.OnHttpPost(client, e);
            }
            else
            {
                return e.InvokeNext();
            }
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
                return this.m_resolver.Resolve(parameter.Type);
            }
            else if (parameter.Type.IsPrimitive || parameter.Type == typeof(string))
            {
                var parameterInfo = this.GetParameterInfo(parameter);
                if (parameterInfo.IsFromBody)
                {
                    var body = await request.GetBodyAsync().ConfigureAwait(false);
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
                    var value =(await request.GetFormCollectionAsync().ConfigureAwait(false)).Get(parameterInfo.FromFormName);
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
                    var value = request.Query.Get(parameterInfo.FromQueryName?? parameter.Name);
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
                var str = await request.GetBodyAsync().ConfigureAwait(false);
                return this.Converter.Deserialize(callContext.HttpContext, str, parameter.Type);
            }
        }

        private void RegisterServer(RpcMethod[] rpcMethods)
        {
            foreach (var rpcMethod in rpcMethods)
            {
                if (rpcMethod.GetAttribute<WebApiAttribute>() is WebApiAttribute attribute)
                {
                    var actionUrls = attribute.GetRouteUrls(rpcMethod);
                    if (actionUrls != null)
                    {
                        foreach (var item in actionUrls)
                        {
                            if (attribute.Method == HttpMethodType.Get)
                            {
                                this.GetRouteMap.Add(item, rpcMethod);
                            }
                            else if (attribute.Method == HttpMethodType.Post)
                            {
                                this.PostRouteMap.Add(item, rpcMethod);
                            }
                        }
                    }
                }
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

            await httpResponse.AnswerAsync().ConfigureAwait(false);

            if (!httpContext.Request.KeepAlive)
            {
                client.TryShutdown(SocketShutdown.Both);
            }
        }
    }
}