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

using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Xml.Linq;
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
        private readonly IResolver m_resolver;
        private readonly IRpcServerProvider m_rpcServerProvider;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WebApiParserPlugin(IRpcServerProvider rpcServerProvider, IResolver resolver)
        {
            if (rpcServerProvider is null)
            {
                throw new ArgumentNullException(nameof(rpcServerProvider));
            }

            this.GetRouteMap = new ActionMap(true);
            this.PostRouteMap = new ActionMap(true);
            this.RegisterServer(rpcServerProvider.GetMethods());
            this.m_rpcServerProvider = rpcServerProvider;
            this.m_resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));

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
            pluginManager.Add(nameof(IHttpPlugin.OnHttpRequest), this.OnHttpRequest);
            base.Loaded(pluginManager);
        }

        private static object PrimitiveParse(string source, Type targetType)
        {
            if (targetType.IsPrimitive || targetType == TouchSocketCoreUtility.stringType)
            {
                StringExtension.TryParseToType(source, targetType, out var target);

                return target;
            }
            //return this.Converter.Deserialize(source, targetType);

            return default;
        }

        private async Task OnHttpGet(IHttpSocketClient client, HttpContextEventArgs e)
        {
            if (this.GetRouteMap.TryGetMethodInstance(e.Context.Request.RelativeURL, out var methodInstance))
            {
                e.Handled = true;

                var invokeResult = new InvokeResult();
                object[] ps = null;

                var callContext = new WebApiCallContext(client, methodInstance, m_resolver, e.Context);

                if (methodInstance.IsEnable)
                {
                    try
                    {
                        ps = new object[methodInstance.Parameters.Length];
                        for (var i = 0; i < methodInstance.Parameters.Length; i++)
                        {
                            var parameter = methodInstance.Parameters[i];
                            if (parameter.IsCallContext)
                            {
                                ps[i] = callContext;
                            }
                            else if (parameter.IsFromServices)
                            {
                                ps[i] = this.m_resolver.Resolve(parameter.Type);
                            }
                            else
                            {
                                var value = e.Context.Request.Query.Get(parameter.Name);
                                if (value.HasValue())
                                {
                                    ps[i] = WebApiParserPlugin.PrimitiveParse(value, parameter.Type);
                                }
                                else if (parameter.ParameterInfo.HasDefaultValue)
                                {
                                    ps[i] = parameter.ParameterInfo.DefaultValue;
                                }
                                else
                                {
                                    ps[i] = parameter.Type.GetDefault();
                                }
                            }
                        }

                        //var i = 0;
                        //if (methodInstance.IncludeCallContext)
                        //{
                        //    ps[i] = callContext;
                        //    i++;
                        //}
                        //if (e.Context.Request.Query == null)
                        //{
                        //    for (; i < methodInstance.Parameters.Length; i++)
                        //    {
                        //        ps[i] = methodInstance.ParameterTypes[i].GetDefault();
                        //    }
                        //}
                        //else
                        //{
                        //    for (; i < methodInstance.Parameters.Length; i++)
                        //    {
                        //        var value = e.Context.Request.Query.Get(methodInstance.ParameterNames[i]);
                        //        if (!value.IsNullOrEmpty())
                        //        {
                        //            ps[i] = WebApiParserPlugin.PrimitiveParse(value, methodInstance.ParameterTypes[i]);
                        //        }
                        //        else
                        //        {
                        //            ps[i] = methodInstance.ParameterTypes[i].GetDefault();
                        //        }
                        //    }
                        //}
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
                    invokeResult = await this.m_rpcServerProvider.ExecuteAsync(callContext, ps);
                }

                if (e.Context.Response.Responsed)
                {
                    return;
                }

                this.Response(client, e.Context, invokeResult);
            }
            await e.InvokeNext();
        }

        private async Task OnHttpPost(IHttpSocketClient client, HttpContextEventArgs e)
        {
            if (this.PostRouteMap.TryGetMethodInstance(e.Context.Request.RelativeURL, out var methodInstance))
            {
                e.Handled = true;

                var invokeResult = new InvokeResult();
                object[] ps = null;
                if (methodInstance.IsEnable)
                {
                    var callContext = new WebApiCallContext(client, methodInstance, m_resolver, e.Context);
                    try
                    {
                        ps = new object[methodInstance.Parameters.Length];

                        for (var i = 0; i < methodInstance.Parameters.Length; i++)
                        {
                            var parameter = methodInstance.Parameters[i];
                            if (parameter.IsCallContext)
                            {
                                ps[i] = callContext;
                            }
                            else if (parameter.IsFromServices)
                            {
                                ps[i] = this.m_resolver.Resolve(parameter.Type);
                            }
                            else if (parameter.Type.IsPrimitive || parameter.Type == typeof(string))
                            {
                                var value = e.Context.Request.Query.Get(parameter.Name);
                                if (value.HasValue())
                                {
                                    ps[i] = WebApiParserPlugin.PrimitiveParse(value, parameter.Type);
                                }
                                else if (parameter.ParameterInfo.HasDefaultValue)
                                {
                                    ps[i] = parameter.ParameterInfo.DefaultValue;
                                }
                                else
                                {
                                    ps[i] = parameter.Type.GetDefault();
                                }
                            }
                            else
                            {
                                var str = e.Context.Request.GetBody();
                                ps[i] = this.Converter.Deserialize(e.Context, str, parameter.Type);
                            }
                        }

                        //var i = 0;
                        
                        //if (methodInstance.IncludeCallContext)
                        //{
                        //    //包含上下文
                        //    ps[i] = callContext;
                        //    i++;
                        //    index = methodInstance.Parameters.Length - 2;

                           
                        //}
                        //else
                        //{
                        //    index = methodInstance.Parameters.Length - 1;
                        //}

                        //var lastType = methodInstance.ParameterTypes.LastOrDefault();
                        //if (true)
                        //{

                        //}
                        //var psLength = methodInstance.ParameterTypes.Length - 1;
                        //if (e.Context.Request.Query.Count==0)
                        //{
                        //    for (; i < psLength; i++)
                        //    {
                        //        ps[i] = methodInstance.ParameterTypes[i].GetDefault();
                        //    }
                        //}
                        //else
                        //{
                        //    for (; i < psLength + 1; i++)
                        //    {
                        //        var value = e.Context.Request.Query.Get(methodInstance.ParameterNames[i]);
                        //        if (!value.IsNullOrEmpty())
                        //        {
                        //            ps[i] = WebApiParserPlugin.PrimitiveParse(value, methodInstance.ParameterTypes[i]);
                        //            index--;
                        //        }
                        //        else
                        //        {
                        //            ps[i] = methodInstance.ParameterTypes[i].GetDefault();
                        //        }
                        //    }
                        //}

                        //if (index >= 0)
                        //{
                        //    var str = e.Context.Request.GetBody();
                        //    if (methodInstance.IncludeCallContext)
                        //    {
                        //        index++;
                        //    }
                        //    ps[index] = this.Converter.Deserialize(e.Context, str, methodInstance.ParameterTypes[index]);
                        //}
                    }
                    catch (Exception ex)
                    {
                        invokeResult.Status = InvokeStatus.Exception;
                        invokeResult.Message = ex.Message;
                    }

                    if (invokeResult.Status == InvokeStatus.Ready)
                    {
                        invokeResult = await this.m_rpcServerProvider.ExecuteAsync(callContext, ps);
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

                this.Response(client, e.Context, invokeResult);
            }
            await e.InvokeNext();
        }

        private Task OnHttpRequest(object sender, PluginEventArgs args)
        {
            var client = (IHttpSocketClient)sender;
            var e = (HttpContextEventArgs)args;

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

        private void RegisterServer(MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                if (methodInstance.GetAttribute<WebApiAttribute>() is WebApiAttribute attribute)
                {
                    var actionUrls = attribute.GetRouteUrls(methodInstance);
                    if (actionUrls != null)
                    {
                        foreach (var item in actionUrls)
                        {
                            if (attribute.Method == HttpMethodType.GET)
                            {
                                this.GetRouteMap.Add(item, methodInstance);
                            }
                            else if (attribute.Method == HttpMethodType.POST)
                            {
                                this.PostRouteMap.Add(item, methodInstance);
                            }
                        }
                    }
                }
            }
        }

        private void Response(IHttpSocketClient client, HttpContext httpContext, in InvokeResult invokeResult)
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

            httpResponse.ContentType= contentType;

            switch (invokeResult.Status)
            {
                case InvokeStatus.Success:
                    {
                        httpResponse.SetContent(this.Converter.Serialize(httpContext,invokeResult.Result)).SetStatus();
                        break;
                    }
                case InvokeStatus.UnFound:
                    {
                        var jsonString = this.Converter.Serialize(httpContext,new ActionResult() { Status = invokeResult.Status, Message = invokeResult.Message });
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

            using (var byteBlock = new ByteBlock())
            {
                httpResponse.Build(byteBlock);
                client.DefaultSend(byteBlock);
            }

            if (!httpContext.Request.KeepAlive)
            {
                client.TryShutdown(SocketShutdown.Both);
            }
        }
    }
}